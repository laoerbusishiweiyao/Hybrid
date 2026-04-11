using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;
using System.Threading;
using Serilog;

namespace Chaos
{
    public sealed class MemoryMappedFilePipe : IDisposable
    {
        private MemoryMappedViewAccessor accessor;

        private readonly int flagIndex;
        private readonly int opcodeIndex;
        private readonly int dataSizeIndex;
        private readonly int dataIndex;

        private readonly byte[] sendBuffer;
        private readonly byte[] receiveBuffer;

        public MemoryMappedFilePipe(int offset, MemoryMappedViewAccessor accessor)
        {
            flagIndex = MemoryMappedFileComponent.FlagIndex + offset;
            opcodeIndex = MemoryMappedFileComponent.OpcodeIndex + offset;
            dataSizeIndex = MemoryMappedFileComponent.DataSizeIndex + offset;
            dataIndex = MemoryMappedFileComponent.DataSizeLength + dataSizeIndex;

            this.accessor = accessor;

            sendBuffer = new byte[MemoryMappedFileComponent.BufferSize];
            receiveBuffer = new byte[MemoryMappedFileComponent.BufferSize];
        }

        public void Write(ushort opcode, byte[] payload)
        {
            accessor.Read<byte>(flagIndex, out var flag);
            if (flag != 0)
            {
                // 等待缓冲区数据消费
                return;
            }

            var sendLength = MemoryMappedFileComponent.FlagLength + MemoryMappedFileComponent.OpcodeLength + MemoryMappedFileComponent.DataSizeLength + payload.Length;

            sendBuffer[MemoryMappedFileComponent.FlagIndex] = 1;
            BinaryPrimitives.WriteUInt16LittleEndian(new Span<byte>(sendBuffer, MemoryMappedFileComponent.OpcodeIndex, MemoryMappedFileComponent.OpcodeLength), opcode);
            BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(sendBuffer, MemoryMappedFileComponent.DataSizeIndex, MemoryMappedFileComponent.DataSizeLength), payload.Length);
            Array.Copy(payload, 0, sendBuffer, MemoryMappedFileComponent.DataIndex, payload.Length);

            accessor.WriteArray(flagIndex, sendBuffer, 0, sendLength);
        }

        public object Read()
        {
            accessor.Read<byte>(flagIndex, out var flag);
            if (flag == 0)
            {
                return null;
            }

            accessor.ReadArray(flagIndex, receiveBuffer, 0, receiveBuffer.Length);

            if (receiveBuffer[0] == 0)
            {
                return null;
            }

            var opcode = BitConverter.ToUInt16(receiveBuffer, MemoryMappedFileComponent.OpcodeIndex);
            var payloadLength = BitConverter.ToInt32(receiveBuffer, MemoryMappedFileComponent.DataSizeIndex);
            if (opcode == 0 || payloadLength > MemoryMappedFileComponent.DataCapacity)
            {
                Log.Warning("Read Invalid opcode {opcode} = {payloadLength}", opcode, payloadLength);
                accessor.Write(flagIndex, 0);
                return null;
            }

            var content = Encoding.UTF8.GetString(receiveBuffer, MemoryMappedFileComponent.DataIndex, payloadLength);
            var type = OpcodeTypeRegistry.Default.GetType(opcode);

            // 通知对侧读取完成
            accessor.Write(flagIndex, 0);

            Log.Information("Read {type} = {msg}", type, content);
            return JsonSerializer.Deserialize(content, type, Options.DefaultJsonSerializerOptions);
        }

        public void Dispose()
        {
            accessor?.Dispose();
            accessor = null;
        }
    }
}