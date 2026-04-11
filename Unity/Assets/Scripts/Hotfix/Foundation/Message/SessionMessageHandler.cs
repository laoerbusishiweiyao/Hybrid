using System;
using Serilog;

namespace Chaos
{
    public abstract class SessionMessageHandler<TMessage> : HandlerObject, ISessionMessageHandler where TMessage : MessageObject
    {
        protected abstract ThreadTask RunAsync(Session session, TMessage message);

        public void Handle(Session session, object message)
        {
            HandleAsync(session, message).Coroutine();
        }

        private async ThreadTask HandleAsync(Session session, object msg)
        {
            if (msg is not TMessage message)
            {
                Log.Error("消息类型转换错误: {src} to {dst}", msg.GetType().FullName, typeof(TMessage).Name);
                return;
            }

            if (session.IsDisposed)
            {
                Log.Error("session disconnect {message}", message);
                return;
            }

            await RunAsync(session, message);
        }

        public Type MessageType => typeof(TMessage);
        public Type ResponseType => null;
    }

    public abstract class SessionMessageHandler<TRequest, TResponse> : HandlerObject, ISessionMessageHandler where TRequest : MessageObject, IRequest where TResponse : MessageObject, IResponse
    {
        protected abstract ThreadTask RunAsync(Session session, TRequest request, TResponse response);

        public void Handle(Session session, object message)
        {
            HandleAsync(session, message).Coroutine();
        }

        private async ThreadTask HandleAsync(Session session, object message)
        {
            try
            {
                if (message is not TRequest request)
                {
                    throw new Exception($"消息类型转换错误: {message.GetType().FullName} to {typeof(TRequest).FullName}");
                }

                var requestId = request.RequestId;

                EntityReference<Session> reference = session;

                using var response = ObjectPool.Rent<TResponse>();
                try
                {
                    await RunAsync(session, request, response);
                }
                catch (RpcException exception)
                {
                    Log.Error("{exception}", exception);
                    response.StatusCode = exception.StatusCode;
                }
                catch (Exception exception)
                {
                    Log.Error("{exception}", exception);
                    response.StatusCode = StatusCodes.RpcFail;
                }

                session = reference;
                if (session == null)
                {
                    return;
                }

                response.RequestId = requestId;
                session.Send(response);
            }
            catch (Exception exception)
            {
                throw new Exception($"解释消息失败: {message.GetType().FullName}", exception);
            }
        }

        public Type MessageType => typeof(TRequest);
        public Type ResponseType => typeof(TResponse);
    }
}