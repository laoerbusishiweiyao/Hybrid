using System.Text;

namespace Chaos
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte self)
        {
            return self.ToString("X2");
        }

        public static string ToHexString(this byte[] self)
        {
            var builder = new StringBuilder();
            foreach (var item in self)
            {
                builder.Append(item.ToString("X2"));
            }

            return builder.ToString();
        }

        public static string ToHexString(this byte[] self, string format)
        {
            var builder = new StringBuilder();
            foreach (var item in self)
            {
                builder.Append(item.ToString(format));
            }

            return builder.ToString();
        }

        public static string ToHexString(this byte[] self, int offset, int count)
        {
            var builder = new StringBuilder();
            for (var i = offset; i < offset + count; ++i)
            {
                builder.Append(self[i].ToString("X2"));
            }

            return builder.ToString();
        }

        public static string ToDefaultString(this byte[] self)
        {
            return Encoding.Default.GetString(self);
        }

        public static string ToDefaultString(this byte[] self, int index, int count)
        {
            return Encoding.Default.GetString(self, index, count);
        }

        public static string ToUtf8String(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToUtf8String(this byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        public static void WriteTo(this byte[] self, int offset, uint value)
        {
            self[offset] = (byte)(value & 0xff);
            self[offset + 1] = (byte)((value & 0xff00) >> 8);
            self[offset + 2] = (byte)((value & 0xff0000) >> 16);
            self[offset + 3] = (byte)((value & 0xff000000) >> 24);
        }

        public static void WriteTo(this byte[] self, int offset, FiberInstanceId fiberInstanceId)
        {
            self.WriteTo(offset, fiberInstanceId.Fiber);
            self.WriteTo(offset + 4, fiberInstanceId.InstanceId);
        }

        public static void WriteTo(this byte[] self, int offset, int value)
        {
            self[offset] = (byte)(value & 0xff);
            self[offset + 1] = (byte)((value & 0xff00) >> 8);
            self[offset + 2] = (byte)((value & 0xff0000) >> 16);
            self[offset + 3] = (byte)((value & 0xff000000) >> 24);
        }

        public static void WriteTo(this byte[] self, int offset, byte value)
        {
            self[offset] = value;
        }

        public static void WriteTo(this byte[] self, int offset, short value)
        {
            self[offset] = (byte)(value & 0xff);
            self[offset + 1] = (byte)((value & 0xff00) >> 8);
        }

        public static void WriteTo(this byte[] self, int offset, ushort value)
        {
            self[offset] = (byte)(value & 0xff);
            self[offset + 1] = (byte)((value & 0xff00) >> 8);
        }

        public static unsafe void WriteTo(this byte[] self, int offset, long value)
        {
            var pointer = (byte*)&value;
            for (var i = 0; i < sizeof(long); ++i)
            {
                self[offset + i] = pointer[i];
            }
        }

        public static long Hash(this byte[] data, int index, int length)
        {
            const int p = 16777619;
            var hash = 2166136261L;

            for (var i = index; i < index + length; i++)
            {
                hash = (hash ^ data[i]) * p;
            }

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        }
    }
}