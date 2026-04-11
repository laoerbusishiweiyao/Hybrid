using UnityEngine;

namespace Chaos
{
    [EntitySystemOf(typeof(GlobalSettingsComponent))]
    public static partial class GlobalSettingsComponentSystem
    {
        [EntitySystem]
        private static void Awake(this GlobalSettingsComponent self)
        {
            self.Settings = Resources.Load<GlobalSettings>(nameof(GlobalSettings));
        }

        [EntitySystem]
        private static void Destroy(this GlobalSettingsComponent self)
        {
            self.Settings = null;
        }
    }
}