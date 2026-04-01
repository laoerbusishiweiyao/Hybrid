using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;
using PipeOptions = System.IO.Pipes.PipeOptions;

namespace Chaos
{
    public sealed class NamedPipeSession : IDisposable
    {
        public const int HeaderLength = 6;

        public static readonly NamedPipeSession Default = new();

        public NamedPipeClientStream NamedPipe { get; set; }

        public Pipe ReceiverPipe;
        public Pipe SenderPipe;

        private readonly Channel<WebMessageInfo> cache = System.Threading.Channels.Channel.CreateUnbounded<WebMessageInfo>();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        public async Task ConnectAsync(string id)
        {
            NamedPipe = new NamedPipeClientStream(".", id, PipeDirection.InOut, PipeOptions.Asynchronous);

            var options = new System.IO.Pipelines.PipeOptions(pauseWriterThreshold: 1024 * 1024, resumeWriterThreshold: 1024 * 512, minimumSegmentSize: 4096, useSynchronizationContext: false);
            ReceiverPipe = new Pipe(options);
            SenderPipe = new Pipe(options);

            await NamedPipe.ConnectAsync(cancellationTokenSource.Token);

            _ = SendAsync(cancellationTokenSource.Token);
            _ = ReceiveAsync(cancellationTokenSource.Token);
            _ = ProcessAsync(cancellationTokenSource.Token);
            _ = ExecuteAsync(cancellationTokenSource.Token);
        }

        public void Send(ushort opcode, object message)
        {
            cache.Writer.TryWrite(new WebMessageInfo { Opcode = opcode, Payload = message });
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await foreach (var info in cache.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await SendAsync(info.Opcode, info.Payload);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Error sending message\n{exception}");
                }
            }
        }

        public async Task SendAsync(ushort opcode, object message)
        {
            if (NamedPipe is null || SenderPipe is null)
            {
                return;
            }

            var writer = SenderPipe.Writer;

            try
            {
                var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, AppSettings.DefaultJsonSerializerOptions));

                var payloadLength = payload.Length;
                var header = writer.GetMemory(6);
                MemoryMarshal.Write(header.Span[..2], ref opcode);
                MemoryMarshal.Write(header.Span[2..6], ref payloadLength);
                writer.Advance(6);

                if (payload.Length > 0)
                {
                    payload.AsSpan().CopyTo(writer.GetSpan(payload.Length));
                    writer.Advance(payload.Length);
                }

                await writer.FlushAsync(cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private async Task SendAsync(CancellationToken cancellationToken)
        {
            if (NamedPipe is null || SenderPipe is null)
            {
                return;
            }

            var reader = SenderPipe.Reader;

            try
            {
                while (NamedPipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var message = await reader.ReadAsync(cancellationToken);
                    if (message.IsCompleted)
                    {
                        break;
                    }

                    foreach (var segment in message.Buffer)
                    {
                        await NamedPipe.WriteAsync(segment, cancellationToken);
                    }

                    reader.AdvanceTo(message.Buffer.End);
                }
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogWarning($"Pipe write cancelled.{exception.Message}");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            if (NamedPipe is null || ReceiverPipe is null)
            {
                return;
            }

            var writer = ReceiverPipe.Writer;

            try
            {
                while (NamedPipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var memory = writer.GetMemory(4096);
                    var count = await NamedPipe.ReadAsync(memory, cancellationToken);

                    if (count is 0)
                    {
                        break;
                    }

                    writer.Advance(count);

                    var message = await writer.FlushAsync(cancellationToken);
                    if (message.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (TaskCanceledException exception)
            {
                Debug.LogWarning($"Pipe receive cancelled.{exception.Message}");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            if (NamedPipe is null || ReceiverPipe is null)
            {
                return;
            }

            var reader = ReceiverPipe.Reader;

            try
            {
                while (NamedPipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var response = await reader.ReadAsync(cancellationToken);
                    if (response.Buffer.IsEmpty && response.IsCompleted)
                    {
                        Debug.Log("ProcessAsync: No more data, completing");
                        break;
                    }

                    var buffer = response.Buffer;

                    while (TryParse(ref buffer, out var opcode, out var payload))
                    {
                        try
                        {
                            Process(opcode, payload);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError(exception);
                        }
                    }

                    reader.AdvanceTo(buffer.Start, response.Buffer.End);

                    if (response.IsCompleted)
                    {
                        Debug.Log("ProcessAsync: Reader completed");
                        break;
                    }
                }
            }
            catch (OperationCanceledException exception)
            {
                Debug.LogWarning($"Pipe process cancelled.{exception.Message}");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void Process(ushort opcode, ReadOnlySequence<byte> payload)
        {
            var type = OpcodeRegistry.Default.FindType(opcode);
            var message = JsonSerializer.Deserialize(payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray(), type, AppSettings.DefaultJsonSerializerOptions);
            switch (message)
            {
                case IWebResponse response:
                {
                    ThreadSynchronizationContext.Default.Post(() => WebSession.Default.OnResponse(response));
                    break;
                }
                case IWebRequest:
                case IWebMessage:
                {
                    ThreadSynchronizationContext.Default.Post(() => WebMessageDispatcher.Default.Handle(opcode, message));
                    break;
                }
                default:
                {
                    Debug.LogError($"Unknown message type {opcode} = {message}");
                    break;
                }
            }
        }

        private bool TryParse(ref ReadOnlySequence<byte> buffer, out ushort opcode, out ReadOnlySequence<byte> payload)
        {
            opcode = 0;
            payload = default;

            if (buffer.Length < HeaderLength)
            {
                return false;
            }

            Span<byte> header = stackalloc byte[HeaderLength];
            buffer.Slice(0, HeaderLength).CopyTo(header);

            opcode = MemoryMarshal.Read<ushort>(header[..2]);
            var length = MemoryMarshal.Read<int>(header[2..6]);

            if (length is < 0 or > 16 * 1024 * 1024)
            {
                throw new InvalidDataException($"Invalid payload length: {length}");
            }

            if (buffer.Length < HeaderLength + length)
            {
                return false;
            }

            payload = buffer.Slice(HeaderLength, length);
            buffer = buffer.Slice(HeaderLength + length);
            return true;
        }

        public void Dispose()
        {
            _ = DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();

            if (ReceiverPipe is not null)
            {
                await ReceiverPipe.Writer.CompleteAsync();
                ReceiverPipe = null;
            }

            if (SenderPipe is not null)
            {
                await SenderPipe.Writer.CompleteAsync();
                SenderPipe = null;
            }

            if (NamedPipe is not null)
            {
                await NamedPipe.DisposeAsync();
                NamedPipe = null;
            }
        }
    }
}