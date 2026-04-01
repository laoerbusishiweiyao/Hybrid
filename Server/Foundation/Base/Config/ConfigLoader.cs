using System.Collections.Concurrent;
using System.Reflection;

namespace Chaos;

public readonly struct ConfigDeserializeEventArgs(Type configType, object configBuffer)
{
    public readonly Type ConfigType = configType;
    public readonly object ConfigBuffer = configBuffer;
}

public sealed class ConfigLoader : Singleton<ConfigLoader>, ISingletonAwake
{
    public readonly struct ConfigLoadAllConfigBufferEventArgs
    {
    }

    private readonly ConcurrentDictionary<Type, IConfig> allConfig = new();

    public void Awake()
    {
    }

    public async ThreadTask LoadAsync()
    {
        var configBuffers = await EventSystem.Default.Invoke<ConfigLoadAllConfigBufferEventArgs, ThreadTask<Dictionary<Type, object>>>(new ConfigLoadAllConfigBufferEventArgs());
        var configFactories = LoadConfigFactories();
        var configTypes = GetConfigTypes();
        var loadedConfigs = new Dictionary<Type, IConfig>(configTypes.Count);
        var loadedSingletons = new List<Singleton>(configTypes.Count);

        foreach (var configType in configTypes)
        {
            var singleton = LoadOneConfig(configType, configBuffers, configFactories, loadedConfigs);
            loadedSingletons.Add(singleton);
        }

        foreach (var singleton in loadedSingletons)
        {
            World.Default.ReplaceSingleton(singleton);
        }

        foreach (var config in loadedConfigs.Values)
        {
            config.ResolveRef();
        }

        allConfig.Clear();
        foreach (var (type, config) in loadedConfigs)
        {
            allConfig[type] = config;
        }
    }

    private List<Type> GetConfigTypes()
    {
        var configTypes = new List<Type>(CodeTypeRegistry.Default.GetTypes(typeof(ConfigProcessAttribute)));
        configTypes.Sort((left, right) => string.CompareOrdinal(left.FullName, right.FullName));
        return configTypes;
    }

    private Dictionary<Type, IConfigFactory> LoadConfigFactories()
    {
        var factoryTypes = new List<Type>();
        foreach (var type in CodeTypeRegistry.Default.GetTypes().Values)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            if (!typeof(IConfigFactory).IsAssignableFrom(type))
            {
                continue;
            }

            factoryTypes.Add(type);
        }

        factoryTypes.Sort((left, right) => string.CompareOrdinal(left.FullName, right.FullName));
        var factories = new Dictionary<Type, IConfigFactory>();
        foreach (Type factoryType in factoryTypes)
        {
            if (Activator.CreateInstance(factoryType) is not IConfigFactory factory)
            {
                throw new Exception($"create config factory failed: {factoryType.FullName}");
            }

            if (factory.ConfigType == null)
            {
                throw new Exception($"config factory target is null: {factoryType.FullName}");
            }

            if (!factories.TryAdd(factory.ConfigType, factory))
            {
                throw new Exception($"duplicate config factory: {factory.ConfigType.FullName}");
            }
        }

        return factories;
    }

    private Singleton LoadOneConfig(Type configType, Dictionary<Type, object> configBuffers, Dictionary<Type, IConfigFactory> configFactories, Dictionary<Type, IConfig> loadedConfigs)
    {
        object category;
        var configProcessAttribute = configType.GetCustomAttribute<ConfigProcessAttribute>(false);

        if (configProcessAttribute.ConfigType == ConfigType.Code)
        {
            if (!configFactories.TryGetValue(configType, out var factory))
            {
                throw new Exception($"config factory not found: {configType.FullName}");
            }

            category = factory.Create();
        }
        else
        {
            if (!configBuffers.TryGetValue(configType, out var configBuffer))
            {
                throw new Exception($"config bytes not found: {configType.FullName}");
            }

            category = EventSystem.Default.Invoke<ConfigDeserializeEventArgs, object>(configProcessAttribute.ConfigType, new ConfigDeserializeEventArgs(configType, configBuffer));
        }

        if (category == null)
        {
            throw new Exception($"config create failed: {configType.FullName}");
        }

        if (category.GetType() != configType)
        {
            throw new Exception($"config type mismatch: expect={configType.FullName} actual={category.GetType().FullName}");
        }

        if (category is not Singleton singleton)
        {
            throw new Exception($"config singleton invalid: {configType.FullName}");
        }

        if (category is not IConfig config)
        {
            throw new Exception($"config interface invalid: {configType.FullName}");
        }

        loadedConfigs.Add(configType, config);
        return singleton;
    }
}