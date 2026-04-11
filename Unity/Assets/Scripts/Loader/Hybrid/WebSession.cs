// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace Chaos
// {
//     public sealed class WebSession : IDisposable
//     {
//         public static readonly WebSession Default = new();
//
//         private static int requestIdCounter;
//
//         public long LastRecvTime;
//         public long LastSendTime;
//
//         public readonly Dictionary<int, WebRequestInfo> Requests = new();
//
//         public void OnResponse(IResponse response)
//         {
//             if (!Requests.TryGetValue(response.RequestId, out var request))
//             {
//                 return;
//             }
//
//             request.SetResult(response);
//         }
//
//         public void Send(IMessage message)
//         {
//             LastSendTime = DateTime.UtcNow.Ticks;
//             WebContext.Send(message);
//         }
//
//         public async Task<IResponse> SendAsync(IRequest request, CancellationToken cancellationToken = default)
//         {
//             var requestId = Interlocked.Increment(ref requestIdCounter);
//             var info = new WebRequestInfo(request.GetType());
//             request.RequestId = requestId;
//             Requests[request.RequestId] = info;
//
//             Send(request);
//
//             void Cancel()
//             {
//                 if (!Requests.Remove(requestId, out var value))
//                 {
//                     return;
//                 }
//
//                 var type = OpcodeRegistry.Default.FindResponseType(value.RequestType);
//                 var response = (IResponse)Activator.CreateInstance(type);
//                 response.StatusCode = 501;
//                 value.SetResult(response);
//             }
//
//             cancellationToken.Register(Cancel);
//             return await info.WaitAsync();
//         }
//
//         public void Dispose()
//         {
//             foreach (var info in Requests.Values)
//             {
//                 info.SetException(new WebSessionException(-1, "WebSession disposed"));
//             }
//
//             Requests.Clear();
//         }
//     }
// }