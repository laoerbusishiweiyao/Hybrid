using System.Collections.Concurrent;
using System.IO;
using System.IO.MemoryMappedFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hybrid.Native.Windows;

public sealed class MemoryMappedFileSession(IOptions<AppSettings> settings) : BackgroundService
{
    /// <summary>
    /// 数据容量
    /// </summary>
    public const int DataCapacity = 64 * 1024;

    public const int FlagIndex = 0;
    public const int FlagLength = 1;

    public const int OpcodeIndex = FlagIndex + FlagLength;
    public const int OpcodeLength = 2;

    public const int DataSizeIndex = OpcodeIndex + OpcodeLength;
    public const int DataSizeLength = 4;

    public const int DataIndex = DataSizeIndex + DataSizeLength;

    public const int BufferSize = DataSizeIndex + DataSizeLength + DataCapacity;

    public const int Size = BufferSize * 2;

    public event EventHandler<MemoryMappedFileReceivedEventArgs>? MessageReceived;

    private MemoryMappedFile file = null!;
    private MemoryMappedViewAccessor accessor = null!;

    private MemoryMappedFilePipe readerPipe = null!;
    private MemoryMappedFilePipe writerPipe = null!;

    private readonly ConcurrentQueue<(ushort Opcode, object Message)> sendQueue = new();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        switch (settings.Value.LaunchOptions.SessionRole)
        {
            case MemoryMappedFileRole.Server:
            {
                file = MemoryMappedFile.CreateNew(settings.Value.LaunchOptions.SessionName, Size, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, HandleInheritability.None);
                accessor = file.CreateViewAccessor(0, Size);
                readerPipe = new MemoryMappedFilePipe(BufferSize, accessor);
                writerPipe = new MemoryMappedFilePipe(0, accessor);
                break;
            }
            case MemoryMappedFileRole.Client:
            {
                file = MemoryMappedFile.OpenExisting(settings.Value.LaunchOptions.SessionName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
                accessor = file.CreateViewAccessor(0, Size);
                readerPipe = new MemoryMappedFilePipe(0, accessor);
                writerPipe = new MemoryMappedFilePipe(BufferSize, accessor);
                break;
            }
            default:
            {
                throw new NotSupportedException($"[MemoryMappedFileSession] Session role {settings.Value.LaunchOptions.SessionRole} not supported");
            }
        }

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (readerPipe.Read() is (ushort opcode, string content))
            {
                MessageReceived?.Invoke(this, new MemoryMappedFileReceivedEventArgs(opcode, content));
            }

            var count = sendQueue.Count;
            while (count-- > 0)
            {
                if (!sendQueue.TryDequeue(out var info))
                {
                    break;
                }
                
                writerPipe.Write(info.Opcode, info.Message);
            }

            await Task.Delay(1, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        file.Dispose();
        accessor.Dispose();
        readerPipe.Dispose();
        writerPipe.Dispose();

        return base.StopAsync(cancellationToken);
    }

    public void Send(ushort opcode, object message)
    {
        sendQueue.Enqueue((opcode, message));
    }
}