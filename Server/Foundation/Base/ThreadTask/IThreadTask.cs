namespace Chaos;

public interface IThreadTask
{
    public ThreadTaskType ThreadTaskType { get; set; }
    public object Context { get; set; }
}