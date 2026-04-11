using System;
using Serilog;

namespace Chaos
{
    public abstract class MessageHandler<TEntity, TMessage> : HandlerObject, IMessageHandler where TEntity : Entity where TMessage : class, IMessage
    {
        protected abstract ThreadTask RunAsync(TEntity entity, TMessage message);

        public async ThreadTask Handle(Entity entity, int fromFiber, MessageObject messageObject)
        {
            if (messageObject is not TMessage msg)
            {
                Log.Error("消息类型转换错误: {src} to {dst}", messageObject.GetType().FullName, typeof(TMessage).Name);
                return;
            }

            if (entity is not TEntity self)
            {
                Log.Error("Actor类型转换错误: {src} to {dst} with {message}", entity.GetType().FullName, typeof(TEntity).Name, typeof(TMessage).Name);
                return;
            }

            await RunAsync(self, msg);
        }

        public Type RequestType
        {
            get
            {
                if (typeof(ILocationMessage).IsAssignableFrom(typeof(TMessage)))
                {
                    Log.Error("message is IActorLocationMessage but handler is IMessageHandler: {message}", typeof(TMessage));
                }

                return typeof(TMessage);
            }
        }

        public Type ResponseType => null;
    }

    public abstract class MessageHandler<TEntity, TRequest, TResponse> : HandlerObject, IMessageHandler where TEntity : Entity where TRequest : MessageObject, IRequest where TResponse : MessageObject, IResponse
    {
        protected abstract ThreadTask RunAsync(TEntity entity, TRequest request, TResponse response);

        public async ThreadTask Handle(Entity entity, int fromFiber, MessageObject messageObject)
        {
            try
            {
                var fiber = entity.Fiber();
                if (messageObject is not TRequest request)
                {
                    Log.Error("消息类型转换错误: {src} to {dst}", messageObject.GetType().FullName, typeof(TRequest).Name);
                    return;
                }

                if (entity is not TEntity self)
                {
                    Log.Error("Actor类型转换错误: {src} to {dst} with {message}", entity.GetType().FullName, typeof(TEntity).FullName, typeof(TRequest).FullName);
                    return;
                }

                var requestId = request.RequestId;
                var response = ObjectPool.Rent<TResponse>();
                try
                {
                    await this.RunAsync(self, request, response);
                }
                catch (RpcException exception)
                {
                    response.StatusCode = exception.StatusCode;
                    response.Message = exception.ToString();
                    Log.Error("{exception}", exception);
                }
                catch (Exception exception)
                {
                    response.StatusCode = StatusCodes.RpcFail;
                    response.Message = exception.ToString();
                    Log.Error("{exception}", exception);
                }

                response.RequestId = requestId;

                fiber.Root.GetComponent<ProcessInnerSender>().Reply(fromFiber, response);
            }
            catch (Exception exception)
            {
                throw new Exception($"解释消息失败: {messageObject.GetType().FullName}", exception);
            }
        }

        public Type RequestType
        {
            get
            {
                if (typeof(ILocationMessage).IsAssignableFrom(typeof(TRequest)))
                {
                    Log.Error("message is IActorLocationMessage but handler is IMessageHandler: {message}", typeof(TRequest));
                }

                return typeof(TRequest);
            }
        }

        public Type ResponseType => null;
    }
}