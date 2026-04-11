// using System;
// using Serilog;
//
// namespace Chaos
// {
//     [EventHandler(SceneType.WebUi)]
//     public sealed class WebUIMessageReceivedEventHandler : EventHandler<Scene, WebUIMessageReceivedEventArgs>
//     {
//         protected override async ThreadTask RunAsync(Scene scene, WebUIMessageReceivedEventArgs eventArgs)
//         {
//             var session = scene.GetComponent<NamedPipeSessionComponent>().Session;
//             var message = (MessageObject)eventArgs.Message;
//
//             switch (message)
//             {
//                 case IWebResponse response:
//                 {
//                     session.OnResponse(response);
//                     break;
//                 }
//                 case IWebRequest:
//                 case IWebMessage:
//                 {
//                     WebMessageDispatcher.Default.Handle(session, message);
//                     break;
//                 }
//                 case IRequest:
//                 {
//                     // 通过 NeiClient 发往 Server
//                     break;
//                 }
//                 default:
//                 {
//                     Log.Error("Unhandled Message {type}.", message.GetType());
//                     break;
//                 }
//             }
//
//             await ThreadTask.CompletedTask;
//         }
//     }
// }