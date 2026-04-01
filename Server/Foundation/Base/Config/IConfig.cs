namespace Chaos;

public interface IConfig
{
    void ResolveRef();
}

public interface IConfigFactory
{
    Type ConfigType { get; }
    Singleton Create();
}