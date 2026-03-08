namespace Chaos
{
    public sealed record AndroidLogEntry : MessageObject
    {
        public readonly long Timestamp;
        public readonly int Level;
        public readonly string Message;

        public AndroidLogEntry(long timestamp, int level, string message)
        {
            Timestamp = timestamp;
            Level = level;
            Message = message;
        }
    }
}