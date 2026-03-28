using System;

namespace Chaos
{
    public interface IPoolable : IDisposable
    {
        bool IsFromPool { get; set; }
    }
}