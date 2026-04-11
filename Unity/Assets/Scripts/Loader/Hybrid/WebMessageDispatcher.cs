// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace Chaos
// {
//     public sealed class WebMessageDispatcher : IDisposable
//     {
//         public static readonly WebMessageDispatcher Default = new();
//
//         private readonly Dictionary<ushort, List<IWebMessageHandler>> allHandler = new();
//
//         public WebMessageDispatcher()
//         {
//             allHandler[Opcode.WebLoaded] = new List<IWebMessageHandler> { new WebLoadedHandler() };
//
//             allHandler[Opcode.WebTouchData] = new List<IWebMessageHandler> { new WebTouchDataHandler() };
//             allHandler[Opcode.WebMouseData] = new List<IWebMessageHandler> { new WebMouseDataHandler() };
//
//             allHandler[Opcode.UnityInformationRequest] = new List<IWebMessageHandler> { new UnityInformationRequestHandler() };
//         }
//
//         public void Handle(ushort opcode, object message)
//         {
//             if (!allHandler.TryGetValue(opcode, out var handlers))
//             {
//                 Debug.Log($"消息 {opcode} 无处理器");
//                 return;
//             }
//             
//             foreach (var handler in handlers)
//             {
//                 try
//                 {
//                     handler.Handle(message);
//                 }
//                 catch (Exception exception)
//                 {
//                     Debug.LogError(exception);
//                 }
//             }
//         }
//
//         public void Dispose()
//         {
//         }
//     }
// }