using System.Runtime.InteropServices;

namespace Chaos;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IdStruct
{
    /// <summary>
    /// 16 bit
    /// </summary>
    public ushort Process;

    /// <summary>
    /// 28 bit
    /// </summary>
    public uint Time;

    /// <summary>
    /// 20 bit
    /// </summary>
    public uint Value;

    public long ToLong()
    {
        ulong result = 0;
        result |= Process;
        result <<= 28;
        result |= Time;
        result <<= 20;
        result |= Value;
        return (long)result;
    }

    public IdStruct(uint time, ushort process, uint value)
    {
        Process = process;
        Time = time;
        Value = value;
    }

    public IdStruct(long id)
    {
        ulong result = (ulong)id;
        Value = (uint)(result & IdGenerator.Mask20Bit);
        result >>= 20;
        Time = (uint)(result & IdGenerator.Mask28Bit);
        result >>= 28;
        Process = (ushort)(result & IdGenerator.Mask16Bit);
    }

    public override string ToString()
    {
        return $"IdStruct(Process = {Process}, Time = {Time}, Value = {Value})";
    }
}

public sealed class IdGenerator : Singleton<IdGenerator>, ISingletonAwake
{
    public const uint Mask32Bit = 0xffffffff;
    public const uint Mask28Bit = 0xfffffff;
    public const uint Mask20Bit = 0xfffff;
    public const uint Mask16Bit = 0xffff;

    private long epoch2025;

    private uint value;
    private uint second;

    public void Awake()
    {
        var epoch1970Tick = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
        epoch2025 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970Tick;
    }

    private uint TimeSince2025()
    {
        return (uint)((TimeInfo.Default.ClientNow() - epoch2025) / 1000);
    }

    public long GenerateId()
    {
        var time = TimeSince2025();
        uint v;
        uint s;
        // 这里必须加锁
        lock (this)
        {
            if (time > second)
            {
                value = 0;
                second = time;
            }
            else
            {
                ++value;
                if (value == Mask20Bit)
                {
                    ++second; // 借用下一秒的id
                    value = 0;
                }
            }

            s = second;
            v = value;
        }

        IdStruct idStruct = new(s, GetProcessReplicaIndex(), v);
        return idStruct.ToLong();
    }

    public ushort GetProcessReplicaIndex()
    {
        // 因为改成了服务发现，支持一个进程多个副本，比如gate只需要配一个进程，可以支持多个,同一个Replica是同一个进程Id
        // 需要预留出来Replica数量的进程Id
        if (Options.Default.Process > 50000)
        {
            throw new Exception($"Process is too large: {Options.Default.Process}");
        }

        return (ushort)(Options.Default.Process + Options.Default.ReplicaIndex);
    }
}