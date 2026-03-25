using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using PipeOptions = System.IO.Pipes.PipeOptions;

namespace Hybrid.Native.Windows;

public sealed class NamedPipeSession(IOptions<AppSettings> settings) : BackgroundService
{
    public required NamedPipeServerStream NamedPipe { get; set; }

    public required Pipe ReceiverPipe;
    public required Pipe SenderPipe;

    private readonly Channel<string> cache = Channel.CreateUnbounded<string>();

    public void Send(string message)
    {
        cache.Writer.TryWrite(message);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        NamedPipe = new NamedPipeServerStream(settings.Value.LaunchOptions.NamedPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        var options = new System.IO.Pipelines.PipeOptions(pauseWriterThreshold: 1024 * 1024, resumeWriterThreshold: 1024 * 512, minimumSegmentSize: 4096, useSynchronizationContext: false);
        ReceiverPipe = new Pipe(options);
        SenderPipe = new Pipe(options);

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (settings.Value.LaunchOptions.ProcessId is not 0)
        {
            await NamedPipe.WaitForConnectionAsync(stoppingToken);

            _ = SendAsync(stoppingToken);
            _ = ReceiveAsync(stoppingToken);
            _ = ProcessAsync(stoppingToken);
        }

        await foreach (var content in cache.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var info = JsonSerializer.Deserialize<MessageInfo>(content, AppSettings.DefaultJsonSerializerOptions);
                if (info?.Payload is JsonElement payload)
                {
                    await SendAsync(info.Opcode, payload, stoppingToken);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error sending message\n{exception}", exception);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await ReceiverPipe.Writer.CompleteAsync();
        await SenderPipe.Writer.CompleteAsync();
        await NamedPipe.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    public async Task SendAsync(ushort opcode, JsonElement content, CancellationToken cancellationToken)
    {
        var writer = SenderPipe.Writer;

        try
        {
            var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content, AppSettings.DefaultJsonSerializerOptions));

            var header = writer.GetSpan(6);
            MemoryMarshal.Write(header[..2], opcode);
            MemoryMarshal.Write(header[2..6], payload.Length);
            writer.Advance(6);

            if (payload.Length > 0)
            {
                payload.AsSpan().CopyTo(writer.GetSpan(payload.Length));
                writer.Advance(payload.Length);
            }

            await writer.FlushAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            Log.Error("Error sending message.\n{exception}", exception);
        }
    }

    private async Task SendAsync(CancellationToken cancellationToken)
    {
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
        catch (Exception exception)
        {
            Log.Error("Error sending message.\n{exception}", exception);
        }
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
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
        catch (Exception exception)
        {
            Log.Error("Error receiving message.\n{exception}", exception);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var reader = ReceiverPipe.Reader;

        try
        {
            while (NamedPipe.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                var response = await reader.ReadAsync(cancellationToken);

                if (response.Buffer.IsEmpty && response.IsCompleted)
                {
                    Log.Warning("ProcessAsync: No more data, completing");
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
                        Log.Error("Error processing message.\n{exception}", exception);
                    }
                }

                reader.AdvanceTo(buffer.Start, response.Buffer.End);

                if (response.IsCompleted)
                {
                    Log.Warning("ProcessAsync: Reader completed");
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            Log.Error("Error processing message.\n{exception}", exception);
        }
    }

    private void Process(ushort opcode, ReadOnlySequence<byte> payload)
    {
        Log.Information("Processing {opcode} payload {payload.Length} bytes", opcode, payload.Length);
    }

    private bool TryParse(ref ReadOnlySequence<byte> buffer, out ushort opcode, out ReadOnlySequence<byte> payload)
    {
        opcode = 0;
        payload = default;

        if (buffer.Length < Parser.HeaderLength)
        {
            return false;
        }

        Span<byte> header = stackalloc byte[Parser.HeaderLength];
        buffer.Slice(0, Parser.HeaderLength).CopyTo(header);

        opcode = MemoryMarshal.Read<ushort>(header[..2]);
        var length = MemoryMarshal.Read<int>(header[2..6]);

        if (length is < 0 or > 16 * 1024 * 1024)
        {
            throw new InvalidDataException($"Invalid payload length: {length}");
        }

        if (buffer.Length < Parser.HeaderLength + length)
        {
            return false;
        }

        payload = buffer.Slice(Parser.HeaderLength, length);
        buffer = buffer.Slice(Parser.HeaderLength + length);
        return true;
    }
}