using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace Chaos
{
    [ChildOf(typeof(Scene))]
    public sealed class MemoryMappedFileComponent : Entity, IAwake<string, MemoryMappedFileRole>, IDestroy, IUpdate
    {
        #region Layout

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

        #endregion

        public MemoryMappedFile File { get; set; }
        public MemoryMappedViewAccessor Accessor { get; set; }

        public MemoryMappedFilePipe ReaderPipe { get; set; }
        public MemoryMappedFilePipe WriterPipe { get; set; }

        public readonly ConcurrentQueue<(ushort Opcode, byte[] Payload)> SendQueue = new();

        public int ProcessId { get; set; }

        public int RequestId { get; set; }

        public readonly Dictionary<int, RequestInfo> RequestCallbacks = new();

        public long LastRecvTime { get; set; }

        public long LastSendTime { get; set; }
    }
}