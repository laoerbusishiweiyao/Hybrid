namespace Chaos
{
    [ChildOf(typeof(CoroutineLockQueue))]
    public sealed class CoroutineLock : Entity, IAwake<long, long, int>, IDestroy
    {
        internal long Type;
        internal long Key;
        internal int Level;
    }
}