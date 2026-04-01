namespace Chaos
{
    public abstract class IValue<T> : DisposableObject where T : struct
    {
        public T Value { get; set; }
    }

    public sealed class ValueTypeWrapper<T> : IValue<T>, IPoolable where T : struct
    {
        public static ValueTypeWrapper<T> Create(T value, bool isFromPool = true)
        {
            var wrapper = ObjectPool.Rent<ValueTypeWrapper<T>>(isFromPool);
            wrapper.Value = value;
            return wrapper;
        }

        public bool IsFromPool { get; set; }

        public override void Dispose()
        {
            Value = default;
            ObjectPool.Recycle(this);
        }
    }
}