namespace Chaos;

public sealed class ConfigProcessAttribute(int configType = 0) : BaseAttribute
{
    public readonly int ConfigType = configType;
}