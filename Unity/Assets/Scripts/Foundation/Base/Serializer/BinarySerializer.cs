using System;
using System.ComponentModel;
using MemoryPack;

namespace Chaos
{
    public static class BinarySerializer
    {
        public static byte[] Serialize(object message)
        {
            if (message is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return MemoryPackSerializer.Serialize(message.GetType(), message);
        }

        public static void Serialize(object message, MemoryBuffer stream)
        {
            if (message is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            MemoryPackSerializer.Serialize(message.GetType(), stream, message);
        }

        public static object Deserialize(Type type, byte[] buffer, int index, int count)
        {
            var instance = MemoryPackSerializer.Deserialize(type, buffer.AsSpan(index, count));
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }

            return instance;
        }

        public static object Deserialize(Type type, byte[] buffer, int index, int count, ref object instance)
        {
            MemoryPackSerializer.Deserialize(type, buffer.AsSpan(index, count), ref instance);
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }

            return instance;
        }

        public static object Deserialize(Type type, MemoryBuffer stream)
        {
            var instance = MemoryPackSerializer.Deserialize(type, stream.GetSpan());
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }

            return instance;
        }

        public static object Deserialize(Type type, MemoryBuffer stream, ref object instance)
        {
            MemoryPackSerializer.Deserialize(type, stream.GetSpan(), ref instance);
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }

            return instance;
        }
    }
}