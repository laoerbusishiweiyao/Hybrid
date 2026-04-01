namespace Chaos
{
    public sealed class ConfigProcessAttribute : BaseAttribute
    {
        public readonly int ConfigType;

        public ConfigProcessAttribute(int configType = 0)
        {
            ConfigType = configType;
        }
    }
}