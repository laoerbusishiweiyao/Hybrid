using System.Text.Encodings.Web;
using System.Text.Json;

namespace Hybrid.Native.Windows;

public sealed record AppSettings
{
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public required LaunchOptions LaunchOptions { get; init; }
}

public sealed record LaunchOptions
{
    public const string DefaultFilePath = "LaunchOptions.json";

    public required int ProcessId { get; init; }
    public required double Left { get; init; }
    public required double Top { get; init; }
    public required double Width { get; init; }
    public required double Height { get; init; }
    public required string SessionName { get; init; }
    public required MemoryMappedFileRole SessionRole { get; init; }
    public required string Address { get; init; }
    public required bool ShowDevTools { get; init; }
}