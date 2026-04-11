// using System;
// using System.Threading.Tasks;
// using UnityEngine;
//
// namespace Chaos
// {
//     public abstract class WebMessageHandler<TMessage> : IWebMessageHandler where TMessage : MessageObject
//     {
//         public Type MessageType => typeof(TMessage);
//         public Type ResponseType => null;
//
//         protected abstract Task RunAsync(TMessage message);
//
//         public void Handle(object message)
//         {
//             _ = HandleAsync(message);
//         }
//
//         private async Task HandleAsync(object message)
//         {
//             await RunAsync((TMessage)message);
//         }
//     }
//
//     public abstract class WebMessageHandler<TRequest, TResponse> : IWebMessageHandler where TRequest : MessageObject, IRequest where TResponse : MessageObject, IResponse
//     {
//         public Type MessageType => typeof(TRequest);
//         public Type ResponseType => typeof(TResponse);
//
//         protected abstract Task RunAsync(TRequest request, TResponse response);
//
//         public void Handle(object message)
//         {
//             _ = HandleAsync(message);
//         }
//
//         private async Task HandleAsync(object message)
//         {
//             try
//             {
//                 if (message is not TRequest request)
//                 {
//                     throw new InvalidCastException("WebMessage 消息转换失败");
//                 }
//
//                 var requestId = request.RequestId;
//                 using var response = Activator.CreateInstance<TResponse>();
//                 try
//                 {
//                     await RunAsync(request, response);
//                 }
//                 catch (WebSessionException exception)
//                 {
//                     Debug.LogError(exception);
//                     response.StatusCode = exception.StatusCode;
//                 }
//                 catch (Exception exception)
//                 {
//                     Debug.LogError(exception);
//                     response.StatusCode = -1;
//                 }
//
//                 response.RequestId = requestId;
//                 WebContext.Send(response);
//             }
//             catch (Exception exception)
//             {
//                 throw new Exception($"WebMessage 处理失败\n{exception}");
//             }
//         }
//     }
// }