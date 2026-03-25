namespace Hybrid.Native.Windows;

public sealed record WebMouseData(ushort Buttons, float X, float Y, float? DeltaX, float? DeltaY) : IMessage
{
    public readonly ushort Buttons = Buttons;
    public readonly float X = X;
    public readonly float Y = Y;
    public readonly float? DeltaX = DeltaX;
    public readonly float? DeltaY = DeltaY;
}