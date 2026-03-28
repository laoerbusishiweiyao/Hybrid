namespace Chaos
{
    public enum AwaiterStatus
    {
        Pending = 0,
        Succeeded = 1,
        Faulted = 2,
    }
    
    public interface IThreadTask
    {
        public ThreadTaskType ThreadTaskType { get; set; }
        public object Context { get; set; }
    }
    
    public enum ThreadTaskType : byte
    {
        Default,
        WithContext,
        ContextTask,
    }
}