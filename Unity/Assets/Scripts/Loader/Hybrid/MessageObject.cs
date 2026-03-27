using System;
using System.Text.Json;

namespace Chaos
{
    public abstract record MessageObject : IMessage, IDisposable
    {
        public virtual void Dispose()
        {
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, AppSettings.DefaultJsonSerializerOptions);
        }
    }
}