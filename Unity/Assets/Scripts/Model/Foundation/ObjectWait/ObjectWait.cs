using System;
using System.Collections.Generic;

namespace Chaos
{
    public static partial class WaitTypeCodes
    {
        public const int Success = 0;
        public const int Destroy = 1;
        public const int Cancel = 2;
        public const int Timeout = 3;
    }

    public interface IWaitType
    {
        int StatusCode { get; set; }
    }


    public interface IObjectWaitCallbackDestroy
    {
        void SetResult();
    }

    public sealed class ObjectWaitCallback<TEventArgs> : Object, IObjectWaitCallbackDestroy where TEventArgs : struct, IWaitType
    {
        private ThreadTask<TEventArgs> task = ThreadTask<TEventArgs>.Create(true);

        public bool IsDisposed => task == null;

        public ThreadTask<TEventArgs> Task => task;

        public void SetResult(TEventArgs eventArgs)
        {
            var threadTask = task;
            task = null;
            threadTask.SetResult(eventArgs);
        }

        public void SetResult()
        {
            var threadTask = task;
            task = null;
            threadTask.SetResult(new TEventArgs { StatusCode = WaitTypeCodes.Destroy });
        }
    }

    [ComponentOf]
    public sealed class ObjectWait : Entity, IAwake, IDestroy
    {
        public Dictionary<Type, object> AllTask = new();
    }
}