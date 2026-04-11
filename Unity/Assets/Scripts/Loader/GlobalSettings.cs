using UnityEngine;

namespace Chaos
{
    public enum BuildType
    {
        Debug,
        Release,
    }

    [CreateAssetMenu(menuName = "User/Global Settings", fileName = "GlobalSettings", order = 0)]
    public sealed class GlobalSettings : ScriptableObject
    {
        public string Version;
        public string SceneName;
        public string Address;
    }
}