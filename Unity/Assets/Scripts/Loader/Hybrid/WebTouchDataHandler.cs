// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.LowLevel;
//
// namespace Chaos
// {
//     public sealed class WebTouchDataHandler : WebMessageHandler<WebTouchData>
//     {
//         protected override async Task RunAsync(WebTouchData message)
//         {
//             InputSystem.QueueStateEvent(Touchscreen.current, new TouchState
//             {
//                 touchId = message.Id,
//                 phase = message.Phase,
//                 position = new Vector2(message.X, message.Y),
//             });
//             await Task.CompletedTask;
//         }
//     }
// }