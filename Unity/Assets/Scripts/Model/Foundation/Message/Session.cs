using System;
using System.Collections.Generic;
using Serilog;

namespace Chaos
{
    public readonly struct RequestInfo
    {
        public readonly Type RequestType;

        private readonly ThreadTask<IResponse> task;

        public RequestInfo(Type requestType)
        {
            RequestType = requestType;
            task = ThreadTask<IResponse>.Create(true);
        }

        public void SetResult(IResponse response)
        {
            task.SetResult(response);
        }

        public void SetException(Exception exception)
        {
            task.SetException(exception);
        }

        public async ThreadTask<IResponse> WaitAsync()
        {
            return await task;
        }
    }

    [ChildOf]
    public sealed class Session : Entity, IAwake<Service>, IDestroy
    {
        public Service Service { get; set; }

        public int RequestId { get; set; }

        public readonly Dictionary<int, RequestInfo> RequestCallbacks = new();

        public long LastRecvTime { get; set; }

        public long LastSendTime { get; set; }

        public int StatusCode { get; set; }

        public Address RemoteAddress { get; set; }
    }
}