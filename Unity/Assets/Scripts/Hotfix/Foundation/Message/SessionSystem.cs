using System;
using System.Linq;
using Serilog;

namespace Chaos
{
    [EntitySystemOf(typeof(Session))]
    public static partial class SessionSystem
    {
        [EntitySystem]
        private static void Awake(this Session self, Service service)
        {
            self.Service = service;
            var now = TimeInfo.Default.ClientNow();
            self.LastRecvTime = now;
            self.LastSendTime = now;

            self.RequestCallbacks.Clear();

            Log.Information("session create: zone: {zone} id: {session} fiber: {fiber} {now} ", self.Zone(), self.Id, self.Fiber().Id, now);
        }

        [EntitySystem]
        private static void Destroy(this Session self)
        {
            var root = self.Root();
            if (root == null)
            {
                return;
            }

            self.Service.Remove(self.Id, self.StatusCode);

            foreach (var info in self.RequestCallbacks.Values.ToArray())
            {
                info.SetException(new RpcException(self.StatusCode, $"session dispose: {self.Id} {self.RemoteAddress}"));
            }

            Log.Information("session dispose: {remote} id: {session} ErrorCode: {statusCode}, please see StatusCodes. {now}", self.RemoteAddress, self.Id, self.StatusCode, TimeInfo.Default.ClientNow());

            self.RequestCallbacks.Clear();
        }

        public static void OnResponse(this Session self, IResponse response)
        {
            NetworkLogger.Default.Recv(self.Fiber(), response);
            if (!self.RequestCallbacks.Remove(response.RequestId, out var info))
            {
                return;
            }

            info.SetResult(response);
        }

        public static async ThreadTask<IResponse> CallAsync(this Session self, IRequest request)
        {
            var requestId = ++self.RequestId;
            var requestInfo = new RequestInfo(request.GetType());
            self.RequestCallbacks[requestId] = requestInfo;

            request.RequestId = requestId;

            self.Send(request);

            void CancelAction()
            {
                if (!self.RequestCallbacks.Remove(requestId, out var info))
                {
                    return;
                }

                var responseType = OpcodeTypeRegistry.Default.GetResponseType(info.RequestType);
                var response = (IResponse)Activator.CreateInstance(responseType);
                response.StatusCode = StatusCodes.Cancel;
                info.SetResult(response);
            }

            var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
            IResponse result;
            try
            {
                cancelSignal?.Add(CancelAction);
                result = await requestInfo.WaitAsync();
            }
            finally
            {
                cancelSignal?.Remove(CancelAction);
            }

            return result;
        }

        public static async ThreadTask<IResponse> CallAsync(this Session self, IRequest request, int timeout)
        {
            return await self.CallAsync(request).TimeoutAsync(timeout);
        }

        public static void Send(this Session self, IMessage message)
        {
            self.Send(default, message);
        }

        public static void Send(this Session self, FiberInstanceId fiberInstanceId, IMessage message)
        {
            NetworkLogger.Default.Send(self.Fiber(), message);

            self.LastSendTime = TimeInfo.Default.ClientNow();

            var (opcode, memoryBuffer) = MessageSerializer.ToMemoryBuffer(self.Service, fiberInstanceId, message);

            self.Service.Send(self.Id, memoryBuffer);
        }
    }
}