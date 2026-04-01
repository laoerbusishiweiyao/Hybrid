using Serilog;

namespace Chaos;

public sealed class CircularBuffer : Stream
{
    public readonly int ChunkSize = 8192;

    private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>();

    private readonly Queue<byte[]> bufferCache = new Queue<byte[]>();

    public int LastIndex { get; set; }

    public int FirstIndex { get; set; }

    private byte[] lastBuffer;

    public CircularBuffer()
    {
        AddLast();
    }

    public override long Length
    {
        get
        {
            int count;
            if (bufferQueue.Count == 0)
            {
                count = 0;
            }
            else
            {
                count = (bufferQueue.Count - 1) * ChunkSize + LastIndex - FirstIndex;
            }

            if (count < 0)
            {
                Log.Error("CircularBuffer count < 0: {0}, {1}, {2}", bufferQueue.Count, LastIndex, FirstIndex);
            }

            return count;
        }
    }

    public void AddLast()
    {
        byte[] buffer;
        if (bufferCache.Count > 0)
        {
            buffer = bufferCache.Dequeue();
        }
        else
        {
            buffer = new byte[ChunkSize];
        }

        bufferQueue.Enqueue(buffer);
        lastBuffer = buffer;
    }

    public void RemoveFirst()
    {
        bufferCache.Enqueue(bufferQueue.Dequeue());
    }

    public byte[] First
    {
        get
        {
            if (bufferQueue.Count == 0)
            {
                AddLast();
            }

            return bufferQueue.Peek();
        }
    }

    public byte[] Last
    {
        get
        {
            if (bufferQueue.Count == 0)
            {
                AddLast();
            }

            return lastBuffer;
        }
    }

    public void Read(Stream stream, int count)
    {
        if (count > Length)
        {
            throw new Exception($"bufferList length < count, {Length} {count}");
        }

        var alreadyCopyCount = 0;
        while (alreadyCopyCount < count)
        {
            var n = count - alreadyCopyCount;
            if (ChunkSize - FirstIndex > n)
            {
                stream.Write(First, FirstIndex, n);
                FirstIndex += n;
                alreadyCopyCount += n;
            }
            else
            {
                stream.Write(First, FirstIndex, ChunkSize - FirstIndex);
                alreadyCopyCount += ChunkSize - FirstIndex;
                FirstIndex = 0;
                RemoveFirst();
            }
        }
    }

    public void Write(Stream stream)
    {
        var count = (int)(stream.Length - stream.Position);

        var alreadyCopyCount = 0;
        while (alreadyCopyCount < count)
        {
            if (LastIndex == ChunkSize)
            {
                AddLast();
                LastIndex = 0;
            }

            var n = count - alreadyCopyCount;
            if (ChunkSize - LastIndex > n)
            {
                _ = stream.Read(lastBuffer, LastIndex, n);
                LastIndex += count - alreadyCopyCount;
                alreadyCopyCount += n;
            }
            else
            {
                _ = stream.Read(lastBuffer, LastIndex, ChunkSize - LastIndex);
                alreadyCopyCount += ChunkSize - LastIndex;
                LastIndex = ChunkSize;
            }
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer.Length < offset + count)
        {
            throw new Exception($"bufferList length < coutn, buffer length: {buffer.Length} {offset} {count}");
        }

        var length = Length;
        if (length < count)
        {
            count = (int)length;
        }

        var alreadyCopyCount = 0;
        while (alreadyCopyCount < count)
        {
            var n = count - alreadyCopyCount;
            if (ChunkSize - FirstIndex > n)
            {
                Array.Copy(First, FirstIndex, buffer, alreadyCopyCount + offset, n);
                FirstIndex += n;
                alreadyCopyCount += n;
            }
            else
            {
                Array.Copy(First, FirstIndex, buffer, alreadyCopyCount + offset, ChunkSize - FirstIndex);
                alreadyCopyCount += ChunkSize - FirstIndex;
                FirstIndex = 0;
                RemoveFirst();
            }
        }

        return count;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        var alreadyCopyCount = 0;
        while (alreadyCopyCount < count)
        {
            if (LastIndex == ChunkSize)
            {
                AddLast();
                LastIndex = 0;
            }

            var n = count - alreadyCopyCount;
            if (ChunkSize - LastIndex > n)
            {
                Array.Copy(buffer, alreadyCopyCount + offset, lastBuffer, LastIndex, n);
                LastIndex += count - alreadyCopyCount;
                alreadyCopyCount += n;
            }
            else
            {
                Array.Copy(buffer, alreadyCopyCount + offset, lastBuffer, LastIndex, ChunkSize - LastIndex);
                alreadyCopyCount += ChunkSize - LastIndex;
                LastIndex = ChunkSize;
            }
        }
    }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Position { get; set; }
}