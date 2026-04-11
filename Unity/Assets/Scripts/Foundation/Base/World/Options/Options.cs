using System.Text.Json;

namespace Chaos
{
    public sealed class Options : Singleton<Options>, ISingletonAwake
    {
        public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Awake()
        {
        }

        public string SceneName { get; set; }
        public string StartConfig { get; set; }
        public int Process { get; set; }
        public int ReplicaIndex { get; set; }
        public int LogLevel { get; set; }
        public int Console { get; set; }
        public int SingleThread { get; set; }
    }
}