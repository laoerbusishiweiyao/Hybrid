// using System;
// using System.Linq;
// using Serilog;
//
// namespace Chaos
// {
//     [EntitySystemOf(typeof(NamedPipeSession))]
//     public static partial class NamedPipeSessionSystem
//     {
//         [EntitySystem]
//         private static void Awake(this NamedPipeSession self)
//         {
//             var now = TimeInfo.Default.ClientNow();
//             self.LastRecvTime = now;
//             self.LastSendTime = now;
//
//             self.RequestCallbacks.Clear();
//
//             Log.Information("named pipe session create: zone: {zone} id: {session} fiber: {fiber} {now} ", self.Zone(), self.Id, self.Fiber().Id, now);
//         }
//
//         [EntitySystem]
//         private static void Destroy(this NamedPipeSession self)
//         {
//             var root = self.Root();
//             if (root == null)
//             {
//                 return;
//             }
//
//             foreach (var info in self.RequestCallbacks.Values.ToArray())
//             {
//                 info.SetException(new RpcException(self.StatusCode, $"named pipe session dispose: {self.Id} {self.RemoteAddress}"));
//             }
//
//             Log.Information("named pipe session dispose: {remote} id: {session} ErrorCode: {statusCode}, please see StatusCodes. {now}", self.RemoteAddress, self.Id, self.StatusCode, TimeInfo.Default.ClientNow());
//
//             self.RequestCallbacks.Clear();
//         }
//
//         public static void OnResponse(this NamedPipeSession self, IResponse response)
//         {
//             WebUiLogger.Default.Recv(self.Fiber(), response);
//             self.LastRecvTime = TimeInfo.Default.ClientNow();
//             if (!self.RequestCallbacks.TryGetValue(response.RequestId, out var request))
//             {
//                 return;
//             }
//
//             request.SetResult(response);
//         }
//
//         public static void Send(this NamedPipeSession self, IMessage message)
//         {
//             WebUiLogger.Default.Send(self.Fiber(), message);
//             self.LastSendTime = TimeInfo.Default.ClientNow();
//             EventSystem.Default.Publish(self.Root(), new WebUiMessageSentEventArgs(message));
//         }
//
//         public static async ThreadTask<IResponse> SendAsync(this NamedPipeSession self, IRequest request)
//         {
//             var requestId = ++self.RequestId;
//             var requestInfo = new RequestInfo(request.GetType());
//             self.RequestCallbacks[requestId] = requestInfo;
//
//             request.RequestId = requestId;
//
//             self.Send(request);
//
//             void CancelAction()
//             {
//                 if (!self.RequestCallbacks.Remove(requestId, out var info))
//                 {
//                     return;
//                 }
//
//                 var responseType = OpcodeTypeRegistry.Default.GetResponseType(info.RequestType);
//                 var response = (IResponse)Activator.CreateInstance(responseType);
//                 response.StatusCode = StatusCodes.Cancel;
//                 info.SetResult(response);
//             }
//
//             var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
//             IResponse result;
//             try
//             {
//                 cancelSignal?.Add(CancelAction);
//                 result = await requestInfo.WaitAsync();
//             }
//             finally
//             {
//                 cancelSignal?.Remove(CancelAction);
//             }
//
//             return result;
//         }
//     }
// }