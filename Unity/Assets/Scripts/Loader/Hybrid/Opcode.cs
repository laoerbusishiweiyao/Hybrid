namespace Chaos
{
    public static class Opcode
    {
        #region 内部保留

        public const ushort Android = 101;

        public const ushort AndroidLogEntry = Android * 100 + 1;

        public const ushort Web = 102;

        public const ushort WebLoaded = Web * 100 + 1;
        public const ushort WebTouchData = Web * 100 + 2;
        public const ushort WebPointerData = Web * 100 + 3;
        public const ushort WebMouseData = Web * 100 + 4;

        #endregion
    }
}