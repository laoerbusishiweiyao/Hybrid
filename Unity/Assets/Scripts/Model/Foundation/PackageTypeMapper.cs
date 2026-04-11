namespace Chaos
{
    [CodeProcess]
    public class PackageTypeMapper : CodeMapper<PackageTypeMapper>, ISingletonAwake
    {
        public void Awake()
        {
            Initialize(typeof(PackageType));
        }
    }
}