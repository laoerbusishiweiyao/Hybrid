namespace Chaos
{
    [ComponentOf(typeof(Scene))]
    public sealed class FiberParentComponent : Entity, IAwake
    {
        public int ParentFiberId { get; set; }
    }
}