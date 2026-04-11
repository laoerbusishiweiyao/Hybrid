using System;
using Serilog;

namespace Chaos
{
    [EntitySystemOf(typeof(ProcessInnerSender))]
    public static partial class ProcessInnerSenderSystem
    {
        [EntitySystem]
        private static void Awake(this ProcessInnerSender self)
        {
            var fiber = self.Fiber();
            MessageQueue.Default.AddQueue(fiber.Id);
        }

        [EntitySystem]
        private static void Destroy(this ProcessInnerSender self)
        {
            var fiber = self.Fiber();
            MessageQueue.Default.RemoveQueue(fiber.Id);
        }

        [EntitySystem]
        private static void Update(this ProcessInnerSender self)
        {
            self.MessageInfos.Clear();
            var fiber = self.Fiber();
            MessageQueue.Default.Fetch(fiber, 1000, self.MessageInfos);

            foreach (var messageInfo in self.MessageInfos)
            {
                self.HandleMessage(fiber, messageInfo);
            }
        }

        private static void HandleMessage(this ProcessInnerSender self, Fiber fiber, in MessageInfo messageInfo)
        {
            if (messageInfo.MessageObject is IResponse response)
            {
                self.HandleActorResponse(response);
                return;
            }

            var fiberInstanceId = messageInfo.FiberInstanceId;
            var message = messageInfo.MessageObject;
            var fromFiber = fiberInstanceId.Fiber;

            Entity entity = self.Fiber().Mailboxes.Get(fiberInstanceId.InstanceId);
            if (entity is not MailboxComponent mailBoxComponent)
            {
                Log.Warning("actor not found mailbox, from: {FiberInstanceId} current: {FiberId} {MessageObject}", fiberInstanceId, fiber.Id, message);
                if (message is IRequest request)
                {
                    self.Reply(fromFiber, ResponseBuilder.Build(request.GetType(), request.RequestId, StatusCodes.NotFoundActor));
                }

                return;
            }

            mailBoxComponent.Add(fromFiber, message);
        }

        private static void HandleActorResponse(this ProcessInnerSender self, IResponse response)
        {
            if (!self.RequestCallback.Remove(response.RequestId, out var info))
            {
                return;
            }

            Run(info, response);
        }

        private static void Run(ProcessInnerMessageSenderInfo info, IResponse response)
        {
            if (response.StatusCode == StatusCodes.MessageTimeout)
            {
                info.SetException(new RpcException(response.StatusCode, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {info.FiberInstanceId} {info.RequestType.FullName}, response: {response}"));
                return;
            }

            if (info.NeedException && StatusCodes.IsNeedThrowException(response.StatusCode))
            {
                info.SetException(new RpcException(response.StatusCode, $"Rpc error: actorId: {info.FiberInstanceId} request: {info.RequestType.FullName}, response: {response}"));
                return;
            }

            info.SetResult(response);
        }

        public static void Reply(this ProcessInnerSender self, int fromFiber, IResponse message)
        {
            self.SendInternal(new FiberInstanceId(fromFiber, 0), (MessageObject)message);
        }

        public static void Send(this ProcessInnerSender self, FiberInstanceId fiberInstanceId, IMessage message)
        {
            self.SendInternal(fiberInstanceId, (MessageObject)message);
        }

        private static bool SendInternal(this ProcessInnerSender self, FiberInstanceId fiberInstanceId, MessageObject message)
        {
            var fiber = self.Fiber();
            return MessageQueue.Default.Send(fiber, fiberInstanceId, message);
        }

        private static int GetRpcId(this ProcessInnerSender self)
        {
            return ++self.RequestId;
        }

        public static async ThreadTask<IResponse> CallAsync(this ProcessInnerSender self, FiberInstanceId fiberInstanceId, IRequest request, bool needException = true)
        {
            var rpcId = self.GetRpcId();
            request.RequestId = rpcId;

            if (fiberInstanceId == default)
            {
                throw new Exception($"FiberInstanceId id is 0: {request}");
            }

            var fiber = self.Fiber();
            var requestType = request.GetType();

            IResponse response;
            if (!self.SendInternal(fiberInstanceId, (MessageObject)request)) // 纤程不存在
            {
                response = ResponseBuilder.Build(requestType, rpcId, StatusCodes.NotFoundActor);
                return response;
            }

            var senderInfo = new ProcessInnerMessageSenderInfo(fiberInstanceId, requestType, needException);
            self.RequestCallback.Add(rpcId, senderInfo);

            TimeoutAsync().Coroutine();

            var beginTime = TimeInfo.Default.ServerNow();

            response = await senderInfo.WaitAsync();

            var endTime = TimeInfo.Default.ServerNow();

            var costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning("actor rpc time > 200: {CostTime} {RequestTypeFullName}", costTime, requestType.FullName);
            }

            return response;

            async ThreadTask TimeoutAsync()
            {
                EntityReference<ProcessInnerSender> reference = self;
                await fiber.Root.TimerComponent.WaitAsync(ProcessInnerSender.Timeout);
                self = reference;
                if (!self.RequestCallback.Remove(rpcId, out var info))
                {
                    return;
                }

                if (needException)
                {
                    info.SetException(new Exception($"actor sender timeout: {requestType.FullName}"));
                }
                else
                {
                    info.SetResult(ResponseBuilder.Build(requestType, rpcId, StatusCodes.MessageTimeout));
                }
            }
        }
    }
}