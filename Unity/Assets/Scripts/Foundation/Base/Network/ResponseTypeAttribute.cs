namespace Chaos
{
    public sealed class ResponseTypeAttribute : BaseAttribute
    {
        public readonly string Type;

        public ResponseTypeAttribute(string type)
        {
            Type = type;
        }
    }
}