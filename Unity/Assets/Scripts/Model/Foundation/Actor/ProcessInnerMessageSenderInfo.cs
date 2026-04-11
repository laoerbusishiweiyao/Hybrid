using System;

namespace Chaos
{
    public readonly struct ProcessInnerMessageSenderInfo
    {
        public FiberInstanceId FiberInstanceId { get; }

        public Type RequestType { get; }

        private readonly ThreadTask<IResponse> task;

        public bool NeedException { get; }

        public ProcessInnerMessageSenderInfo(FiberInstanceId fiberInstanceId, Type requestType, bool needException)
        {
            FiberInstanceId = fiberInstanceId;

            RequestType = requestType;

            task = ThreadTask<IResponse>.Create(true);
            NeedException = needException;
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
}