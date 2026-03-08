using System.Text.Json;

namespace Chaos
{
    public abstract record MessageObject
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, AppSettings.JsonSerializerOptions);
        }
    }
}