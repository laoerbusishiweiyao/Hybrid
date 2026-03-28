namespace Chaos
{
    public abstract class Singleton : DisposableObject
    {
        internal abstract void Register();

        public virtual ushort Priority => ushort.MaxValue;
    }

    public abstract class Singleton<T> : Singleton where T : Singleton<T>
    {
        public static T Default { get; private set; }

        internal override void Register()
        {
            Default = (T)this;
        }

        protected virtual void Destroy()
        {
        }

        public override void Dispose()
        {
            if (ReferenceEquals(Default, this))
            {
                Default = null;
            }

            Destroy();
        }
    }
}