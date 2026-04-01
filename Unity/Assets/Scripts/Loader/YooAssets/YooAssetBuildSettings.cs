using UnityEngine;
using YooAsset;

namespace Chaos
{
    [CreateAssetMenu(menuName = "User/YooAsset Build Settings", fileName = "YooAssetBuildSettings", order = 0)]
    public sealed class YooAssetBuildSettings : ScriptableObject
    {
        public EPlayMode PlayMode;
        public string Url;
    }
}