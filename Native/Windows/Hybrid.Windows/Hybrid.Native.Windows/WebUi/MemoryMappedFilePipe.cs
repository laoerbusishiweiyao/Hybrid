using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;
using Serilog;

namespace Hybrid.Native.Windows;

public sealed class MemoryMappedFilePipe(int offset, MemoryMappedViewAccessor accessor) : IDisposable
{
    private readonly int flagIndex = MemoryMappedFileSession.FlagIndex + offset;

    private readonly byte[] sendBuffer = new byte[MemoryMappedFileSession.BufferSize];
    private readonly byte[] receiveBuffer = new byte[MemoryMappedFileSession.BufferSize];

    public void Write(ushort opcode, object message)
    {
        accessor.Read<byte>(flagIndex, out var flag);
        if (flag != 0)
        {
            // 等待缓冲区数据消费
            return;
        }


        var content = JsonSerializer.Serialize(message, AppSettings.DefaultJsonSerializerOptions);
        var payload = Encoding.UTF8.GetBytes(content);

        var sendLength = MemoryMappedFileSession.FlagLength + MemoryMappedFileSession.OpcodeLength + MemoryMappedFileSession.DataSizeLength + payload.Length;

        sendBuffer[MemoryMappedFileSession.FlagIndex] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(new Span<byte>(sendBuffer, MemoryMappedFileSession.OpcodeIndex, MemoryMappedFileSession.OpcodeLength), opcode);
        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(sendBuffer, MemoryMappedFileSession.DataSizeIndex, MemoryMappedFileSession.DataSizeLength), payload.Length);
        Array.Copy(payload, 0, sendBuffer, MemoryMappedFileSession.DataIndex, payload.Length);

        accessor.WriteArray(flagIndex, sendBuffer, 0, sendLength);
    }

    public object? Read()
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

        var opcode = BitConverter.ToUInt16(receiveBuffer, MemoryMappedFileSession.OpcodeIndex);
        var payloadLength = BitConverter.ToInt32(receiveBuffer, MemoryMappedFileSession.DataSizeIndex);
        if (opcode == 0 || payloadLength > MemoryMappedFileSession.DataCapacity)
        {
            accessor.Write(flagIndex, 0);
            return null;
        }

        var content = Encoding.UTF8.GetString(receiveBuffer, MemoryMappedFileSession.DataIndex, payloadLength);

        // 通知对侧读取完成
        accessor.Write(flagIndex, 0);

        return (opcode, content);
    }

    public void Dispose()
    {
        accessor.Dispose();
    }
}