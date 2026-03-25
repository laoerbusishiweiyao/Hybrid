namespace Hybrid.Native.Windows;

public static class Parser
{
    public const int OpcodeLength = 2;
    public const int PayloadLength = 4;

    public const int HeaderLength = OpcodeLength + PayloadLength;
}