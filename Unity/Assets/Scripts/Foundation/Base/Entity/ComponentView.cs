using UnityEngine;

#if UNITY_EDITOR && HIERARCHY
namespace Chaos
{
    public sealed class ComponentView : MonoBehaviour
    {
        public EntityReference<Entity> Component { get; set; }
    }
}
#endif