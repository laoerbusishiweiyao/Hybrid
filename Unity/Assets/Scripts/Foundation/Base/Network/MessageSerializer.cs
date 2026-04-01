using System;
using System.IO;

namespace Chaos
{
    public static class MessageSerializer
    {
        public static byte[] Serialize(MessageObject message)
        {
            return BinarySerializer.Serialize(message);
        }

        public static void Serialize(MessageObject message, MemoryBuffer stream)
        {
            BinarySerializer.Serialize(message, stream);
        }

        public static MessageObject Deserialize(Type type, byte[] bytes, int index, int count)
        {
            var instance = ObjectPool.Rent(type);
            BinarySerializer.Deserialize(type, bytes, index, count, ref instance);
            return instance as MessageObject;
        }

        public static MessageObject Deserialize(Type type, MemoryBuffer stream)
        {
            var instance = ObjectPool.Rent(type);
            BinarySerializer.Deserialize(type, stream, ref instance);
            return instance as MessageObject;
        }

        public static ushort MessageToStream(MemoryBuffer stream, MessageObject message, int headOffset = 0)
        {
            var opcode = OpcodeTypeRegistry.Default.GetOpcode(message.GetType());
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);

            stream.GetBuffer().WriteTo(headOffset, opcode);

            Serialize(message, stream);

            stream.Seek(0, SeekOrigin.Begin);
            return opcode;
        }

        public static (ushort, MemoryBuffer) ToMemoryBuffer(Service service, FiberInstanceId fiberInstanceId, object message)
        {
            var memoryBuffer = service.Fetch();
            ushort opcode = 0;
            switch (service.ServiceType)
            {
                case ServiceType.Internal:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message, Packet.FiberInstanceIdLength);
                    memoryBuffer.GetBuffer().WriteTo(0, fiberInstanceId);
                    break;
                }
                case ServiceType.External:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message);
                    break;
                }
            }

            return (opcode, memoryBuffer);
        }

        public static (FiberInstanceId, object) ToMessage(Service service, MemoryBuffer memoryStream)
        {
            object message = null;
            FiberInstanceId fiberInstanceId = default;
            switch (service.ServiceType)
            {
                case ServiceType.External:
                {
                    memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
                    var opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), 0);
                    var type = OpcodeTypeRegistry.Default.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
                case ServiceType.Internal:
                {
                    memoryStream.Seek(Packet.FiberInstanceIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
                    var buffer = memoryStream.GetBuffer();
                    fiberInstanceId.Fiber = BitConverter.ToInt32(buffer, Packet.FiberInstanceIdIndex);
                    fiberInstanceId.InstanceId = BitConverter.ToInt32(buffer, Packet.FiberInstanceIdIndex + 4);
                    var opcode = BitConverter.ToUInt16(buffer, Packet.FiberInstanceIdLength);

                    var type = OpcodeTypeRegistry.Default.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
            }

            return (fiberInstanceId, message);
        }
    }
}