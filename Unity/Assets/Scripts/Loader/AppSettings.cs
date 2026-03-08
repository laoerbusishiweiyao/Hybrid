using System.Text.Json;

namespace Chaos
{
    public sealed class AppSettings
    {
        public static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
        };
    }
}