namespace Chaos
{
    public sealed class WebMouseData : MessageObject
    {
        public readonly ushort Buttons;
        public readonly float X;
        public readonly float Y;
        public readonly float? DeltaX;
        public readonly float? DeltaY;

        public WebMouseData(ushort buttons, float x, float y, float? deltaX = null, float? deltaY = null)
        {
            Buttons = buttons;
            X = x;
            Y = y;
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }
}