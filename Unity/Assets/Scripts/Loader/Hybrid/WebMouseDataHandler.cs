// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.LowLevel;
//
// namespace Chaos
// {
//     public sealed class WebMouseDataHandler : WebMessageHandler<WebMouseData>
//     {
//         protected override async Task RunAsync(WebMouseData message)
//         {
//             InputSystem.QueueStateEvent(Mouse.current, new MouseState
//             {
//                 buttons = message.Buttons,
//                 position = new Vector2(message.X, message.Y),
//                 delta = new Vector2(message.DeltaX ?? 0, message.DeltaY ?? 0),
//             });
//             await Task.CompletedTask;
//         }
//     }
// }