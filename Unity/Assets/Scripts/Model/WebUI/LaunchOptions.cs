namespace Chaos
{
    public sealed record LaunchOptions
    {
        public const string DefaultFilePath = "LaunchOptions.json";

        public int ProcessId { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string SessionName { get; set; }
        public MemoryMappedFileRole SessionRole { get; set; }
        public string Address { get; set; }
        public bool ShowDevTools { get; set; }
    }
}