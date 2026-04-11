// using System.Collections.Concurrent;
// using System.IO.Pipelines;
// using System.IO.Pipes;
// using System.Text.Json;
//
// namespace Chaos
// {
//     public readonly struct WebUIMessageReceivedEventArgs
//     {
//         public readonly object Message;
//
//         public WebUIMessageReceivedEventArgs(object message)
//         {
//             Message = message;
//         }
//     }
//
//     [ComponentOf(typeof(Scene))]
//     public sealed class NamedPipeClientComponent : Entity, IAwake<string>, IDestroy, IUpdate
//     {
//         public static readonly JsonSerializerOptions JsonSerializerOptions = new()
//         {
//             IncludeFields = true,
//             PropertyNameCaseInsensitive = true,
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//         };
//
//         public NamedPipeClientStream Client { get; set; }
//         public Pipe ReceivePipe { get; set; }
//         public Pipe SenderPipe { get; set; }
//         public System.Threading.Channels.Channel<(ushort opcode, object payload)> Cache { get; set; }
//
//         public readonly ConcurrentQueue<object> ReceiveMessages = new();
//     }
// }