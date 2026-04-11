using System;
using Serilog;

namespace Chaos
{
    public abstract class WebMessageHandler<TMessage> : HandlerObject, IWebMessageHandler where TMessage : class, IMessage
    {
        protected abstract ThreadTask RunAsync(WebUiComponent webUiComponent, TMessage message);

        public void Handle(WebUiComponent webUiComponent, MessageObject messageObject)
        {
            HandleAsync(webUiComponent, messageObject).Coroutine();
        }

        private async ThreadTask HandleAsync(WebUiComponent webUiComponent, MessageObject messageObject)
        {
            if (messageObject is not TMessage msg)
            {
                Log.Error("消息类型转换错误: {src} to {dst}", messageObject.GetType().FullName, typeof(TMessage).Name);
                return;
            }

            if (webUiComponent.IsDisposed)
            {
                Log.Error("WebUiComponent disconnect {message}", msg);
                return;
            }

            await RunAsync(webUiComponent, msg);
        }

        public Type RequestType => typeof(TMessage);

        public Type ResponseType => null;
    }

    public abstract class WebMessageHandler<TRequest, TResponse> : HandlerObject, IWebMessageHandler where TRequest : MessageObject, IRequest where TResponse : MessageObject, IResponse
    {
        protected abstract ThreadTask RunAsync(WebUiComponent webUiComponent, TRequest request, TResponse response);

        public void Handle(WebUiComponent webUiComponent, MessageObject messageObject)
        {
            HandleAsync(webUiComponent, messageObject).Coroutine();
        }

        private async ThreadTask HandleAsync(WebUiComponent webUiComponent, MessageObject messageObject)
        {
            try
            {
                if (messageObject is not TRequest request)
                {
                    Log.Error("消息类型转换错误: {src} to {dst}", messageObject.GetType().FullName, typeof(TRequest).Name);
                    return;
                }

                var requestId = request.RequestId;

                EntityReference<WebUiComponent> reference = webUiComponent;

                var response = ObjectPool.Rent<TResponse>();
                try
                {
                    await RunAsync(webUiComponent, request, response);
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

                webUiComponent = reference;
                if (webUiComponent is null)
                {
                    return;
                }

                response.RequestId = requestId;
                webUiComponent.Send(response);
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