namespace Chaos
{
    public sealed class InvokeHandlerAttribute : BaseAttribute
    {
        public readonly long Type;

        public InvokeHandlerAttribute(long type = 0)
        {
            Type = type;
        }
    }
}