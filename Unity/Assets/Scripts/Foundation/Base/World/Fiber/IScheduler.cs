using System;

namespace Chaos
{
    internal interface IScheduler : IDisposable
    {
        void AddToScheduler(Fiber fiber);
    }
}