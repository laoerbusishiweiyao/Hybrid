using System;
using System.Collections.Generic;

namespace Chaos
{
    public static class SharedRandom
    {
        [ThreadStatic]
        private static Random random;

        public static Random Default => random ??= new Random(Guid.NewGuid().GetHashCode() ^ Environment.TickCount);

        public static float NextSingle()
        {
            var value = Default.Next();
            float result;
            unsafe
            {
                result = *(float*)&value;
            }

            return result;
        }

        public static ulong NextUInt64()
        {
            var left = NextInt32();
            var right = NextInt32();
            return ((ulong)left << 32) | (uint)right;
        }

        public static int NextInt32()
        {
            return Default.Next();
        }

        public static uint NextUInt32()
        {
            return (uint)Default.Next();
        }

        public static long NextInt64()
        {
            var r1 = NextUInt32();
            var r2 = NextUInt32();
            return (long)(((ulong)r1 << 32) | r2);
        }

        public static int NextInt32(int lower, int upper)
        {
            var value = Default.Next(lower, upper);
            return value;
        }

        public static bool NextBoolean()
        {
            return Default.Next(2) == 0;
        }

        public static T Next<T>(T[] array)
        {
            return array[NextInt32(0, array.Length)];
        }

        public static T Next<T>(List<T> array)
        {
            return array[NextInt32(0, array.Count)];
        }

        public static void Shuffle<T>(T[] array)
        {
            if (array == null || array.Length < 2)
            {
                return;
            }

            for (var i = 0; i < array.Length; i++)
            {
                var index = Default.Next(0, array.Length);
                (array[index], array[i]) = (array[i], array[index]);
            }
        }

        public static void Shuffle<T>(List<T> list)
        {
            if (list == null || list.Count < 2)
            {
                return;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var index = Default.Next(0, list.Count);
                (list[index], list[i]) = (list[i], list[index]);
            }
        }

        public static float NextSingle01()
        {
            var a = NextInt32(0, 1000000);
            return a / 1000000f;
        }
    }
}