using System.Text.Json;

namespace Chaos
{
    public sealed class AppSettings
    {
        public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }
}