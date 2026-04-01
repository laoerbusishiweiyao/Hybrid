using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Chaos
{
    public abstract class Service : IDisposable
    {
        public Action<long, IPEndPoint> AcceptCallback;
        public Action<long, MemoryBuffer> ReadCallback;
        public Action<long, int> ErrorCallback;

        public long Id { get; set; } = IdGenerator.Default.GenerateId();

        public ServiceType ServiceType { get; protected set; }

        public abstract IPEndPoint GetBindPoint();

        private const int MaxMemoryBufferSize = 1024;

        private readonly Queue<MemoryBuffer> pool = new();

        public MemoryBuffer Fetch(int size = 0)
        {
            switch (size)
            {
                case > MaxMemoryBufferSize:
                    return new MemoryBuffer(size);
                case < MaxMemoryBufferSize:
                    size = MaxMemoryBufferSize;
                    break;
            }

            return pool.Count == 0 ? new MemoryBuffer(size) : pool.Dequeue();
        }

        public void Recycle(MemoryBuffer memoryBuffer)
        {
            if (memoryBuffer.Capacity > 1024)
            {
                return;
            }

            if (pool.Count > 10)
            {
                return;
            }

            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);

            pool.Enqueue(memoryBuffer);
        }


        public virtual void Dispose()
        {
            Id = 0;
        }

        public abstract void Update();

        public abstract void Remove(long id, int error = 0);

        public bool IsDisposed => Id == 0;

        public abstract void Create(long id, IPEndPoint point);

        public abstract void Send(long channelId, MemoryBuffer buffer);

        public virtual (uint, uint) GetChannelConnection(long channelId)
        {
            throw new Exception($"default conn throw Exception! {channelId}");
        }

        public virtual void ChangeAddress(long channelId, IPEndPoint point)
        {
        }
    }
}