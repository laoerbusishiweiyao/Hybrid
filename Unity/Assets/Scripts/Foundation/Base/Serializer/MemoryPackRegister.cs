using System.Runtime.CompilerServices;
using MemoryPack;

namespace Chaos
{
    public static class SafeUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref byte source)
        {
            Unsafe.SkipInit(out T value);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref value), ref source, (uint)Unsafe.SizeOf<T>());
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
            Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.As<T, byte>(ref value), (uint)Unsafe.SizeOf<T>());
        }
    }

    public static class MemoryPackRegister
    {
        public static void Initialize()
        {
            MemoryPackFormatterProvider.Register(new MemoryPackChildCollectionFormatter());
            MemoryPackFormatterProvider.Register(new MemoryPackComponentCollectionFormatter());
        }
    }
}