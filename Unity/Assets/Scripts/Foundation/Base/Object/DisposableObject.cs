using System;
using System.ComponentModel;

namespace Chaos
{
    public abstract class DisposableObject : Object, IDisposable, ISupportInitialize
    {
        public virtual void Dispose()
        {
        }

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }
    }
}