// using System;
// using System.Buffers;
// using System.IO;
// using System.IO.Pipelines;
// using System.IO.Pipes;
// using System.Runtime.InteropServices;
// using System.Text;
// using System.Text.Json;
// using Serilog;
// using PipeOptions = System.IO.Pipes.PipeOptions;
//
// namespace Chaos
// {
//     [EntitySystemOf(typeof(NamedPipeClientComponent))]
//     public static partial class NamedPipeClientComponentSystem
//     {
//         [EntitySystem]
//         private static void Awake(this NamedPipeClientComponent self, string id)
//         {
//             self.Client = new NamedPipeClientStream(".", id, PipeDirection.InOut, PipeOptions.Asynchronous);
//             var options = new System.IO.Pipelines.PipeOptions(pauseWriterThreshold: 1024 * 1024, resumeWriterThreshold: 1024 * 512, minimumSegmentSize: 4096, useSynchronizationContext: false);
//             self.ReceivePipe = new Pipe(options);
//             self.SenderPipe = new Pipe(options);
//
//             self.Cache = System.Threading.Channels.Channel.CreateUnbounded<(ushort opcode, object payload)>();
//
//             self.ConnectAsync().Coroutine();
//         }
//
//         [EntitySystem]
//         private static void Destroy(this NamedPipeClientComponent self)
//         {
//             self.ReceiveMessages.Clear();
//
//             self.Cache.Writer.Complete();
//             self.Cache = null;
//
//             self.ReceivePipe.Writer.Complete();
//             self.SenderPipe.Writer.Complete();
//             self.Client.Dispose();
//
//             self.ReceivePipe = null;
//             self.SenderPipe = null;
//             self.Client = null;
//         }
//
//
//         [EntitySystem]
//         private static void Update(this NamedPipeClientComponent self)
//         {
//             while (true)
//             {
//                 if (!self.ReceiveMessages.TryDequeue(out var message))
//                 {
//                     break;
//                 }
//
//                 EventSystem.Default.Publish(self.Scene(), new WebUIMessageReceivedEventArgs(message));
//             }
//         }
//
//         public static void Send(this NamedPipeClientComponent self, object message)
//         {
//             var opcode = OpcodeTypeRegistry.Default.GetOpcode(message.GetType());
//             self.Cache.Writer.TryWrite((opcode, message));
//         }
//         
//         private static async ThreadTask ConnectAsync(this NamedPipeClientComponent self)
//         {
//             await self.Client.ConnectAsync();
//
//             self.StartSendAsync().Coroutine();
//             self.StartReceiveAsync().Coroutine();
//             self.StartParseAsync().Coroutine();
//             self.ExecuteAsync().Coroutine();
//         }
//
//         private static async ThreadTask ExecuteAsync(this NamedPipeClientComponent self)
//         {
//             await foreach (var (opcode, payload) in self.Cache.Reader.ReadAllAsync())
//             {
//                 try
//                 {
//                     await self.WriteAsync(opcode, payload);
//                 }
//                 catch (Exception exception)
//                 {
//                     Log.Error("{exception}", exception);
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 将消息写入发送管道
//         /// </summary>
//         /// <param name="self"></param>
//         /// <param name="opcode"></param>
//         /// <param name="message"></param>
//         private static async ThreadTask WriteAsync(this NamedPipeClientComponent self, ushort opcode, object message)
//         {
//             if (self.IsDisposed)
//             {
//                 return;
//             }
//
//             var writer = self.SenderPipe.Writer;
//
//             try
//             {
//                 var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, NamedPipeClientComponent.JsonSerializerOptions));
//
//                 var payloadLength = payload.Length;
//                 var header = writer.GetMemory(6);
//                 MemoryMarshal.Write(header.Span[..2], ref opcode);
//                 MemoryMarshal.Write(header.Span[2..6], ref payloadLength);
//                 writer.Advance(6);
//
//                 if (payload.Length > 0)
//                 {
//                     payload.AsSpan().CopyTo(writer.GetSpan(payload.Length));
//                     writer.Advance(payload.Length);
//                 }
//
//                 await writer.FlushAsync();
//             }
//             catch (Exception exception)
//             {
//                 Log.Error("{exception}", exception);
//             }
//         }
//
//         private static async ThreadTask StartSendAsync(this NamedPipeClientComponent self)
//         {
//             if (self.IsDisposed)
//             {
//                 return;
//             }
//
//             var reader = self.SenderPipe.Reader;
//
//             try
//             {
//                 while (!self.IsDisposed && self.Client.IsConnected)
//                 {
//                     var message = await reader.ReadAsync();
//                     if (message.IsCompleted)
//                     {
//                         break;
//                     }
//
//                     foreach (var segment in message.Buffer)
//                     {
//                         await self.Client.WriteAsync(segment);
//                     }
//
//                     reader.AdvanceTo(message.Buffer.End);
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Log.Warning("Sender Pipe was cancelled");
//             }
//             catch (Exception exception)
//             {
//                 Log.Warning("Sender Pipe was closed.\n{exception}", exception);
//             }
//         }
//
//         private static async ThreadTask StartReceiveAsync(this NamedPipeClientComponent self)
//         {
//             if (self.IsDisposed)
//             {
//                 return;
//             }
//
//             var writer = self.ReceivePipe.Writer;
//
//             try
//             {
//                 while (!self.IsDisposed && self.Client.IsConnected)
//                 {
//                     var memory = writer.GetMemory(4096);
//                     var count = await self.Client.ReadAsync(memory);
//
//                     if (count is 0)
//                     {
//                         break;
//                     }
//
//                     writer.Advance(count);
//
//                     var message = await writer.FlushAsync();
//                     if (message.IsCompleted)
//                     {
//                         break;
//                     }
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Log.Warning("Receive Pipe was cancelled");
//             }
//             catch (Exception exception)
//             {
//                 Log.Warning("Receive Pipe was closed.\n{exception}", exception);
//             }
//         }
//
//         private static async ThreadTask StartParseAsync(this NamedPipeClientComponent self)
//         {
//             if (self.IsDisposed)
//             {
//                 return;
//             }
//
//             var reader = self.ReceivePipe.Reader;
//
//             try
//             {
//                 while (!self.IsDisposed && self.Client.IsConnected)
//                 {
//                     var response = await reader.ReadAsync();
//                     if (response.Buffer.IsEmpty && response.IsCompleted)
//                     {
//                         break;
//                     }
//
//                     var buffer = response.Buffer;
//                     while (self.TryParse(ref buffer, out var opcode, out var payload))
//                     {
//                         try
//                         {
//                             var type = OpcodeTypeRegistry.Default.GetType(opcode);
//                             var message = JsonSerializer.Deserialize(payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray(), type, NamedPipeClientComponent.JsonSerializerOptions);
//                             self.ReceiveMessages.Enqueue(message);
//                         }
//                         catch (Exception exception)
//                         {
//                             Log.Warning("Parse was closed.\n{exception}", exception);
//                         }
//                     }
//
//                     reader.AdvanceTo(buffer.Start, response.Buffer.End);
//
//                     if (response.IsCompleted)
//                     {
//                         break;
//                     }
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Log.Warning("Parse was cancelled");
//             }
//             catch (Exception exception)
//             {
//                 Log.Warning("Parse was closed.\n{exception}", exception);
//             }
//         }
//
//         private static bool TryParse(this NamedPipeClientComponent self, ref ReadOnlySequence<byte> buffer, out ushort opcode, out ReadOnlySequence<byte> payload)
//         {
//             const int headerLength = 6;
//
//             opcode = 0;
//             payload = default;
//
//             if (buffer.Length < headerLength)
//             {
//                 return false;
//             }
//
//             Span<byte> header = stackalloc byte[headerLength];
//             buffer.Slice(0, headerLength).CopyTo(header);
//
//             opcode = MemoryMarshal.Read<ushort>(header[..2]);
//             var length = MemoryMarshal.Read<int>(header[2..6]);
//
//             if (length is < 0 or > 16 * 1024 * 1024)
//             {
//                 throw new InvalidDataException($"Invalid payload length: {length}");
//             }
//
//             if (buffer.Length < headerLength + length)
//             {
//                 return false;
//             }
//
//             payload = buffer.Slice(headerLength, length);
//             buffer = buffer.Slice(headerLength + length);
//             return true;
//         }
//     }
// }