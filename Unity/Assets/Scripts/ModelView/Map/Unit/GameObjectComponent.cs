using UnityEngine;

namespace Chaos
{
    [ComponentOf(typeof(Unit))]
    public sealed class GameObjectComponent : Entity, IAwake, IDestroy
    {
        private GameObject gameObject;

        public GameObject GameObject
        {
            get => gameObject;
            set
            {
                gameObject = value;
                Transform = value.transform;
            }
        }

        public Transform Transform { get; private set; }
    }
}