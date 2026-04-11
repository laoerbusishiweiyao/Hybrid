// namespace Chaos
// {
//     [EntitySystemOf(typeof(NamedPipeSessionComponent))]
//     public static partial class NamedPipeSessionComponentSystem
//     {
//         [EntitySystem]
//         private static void Awake(this NamedPipeSessionComponent self)
//         {
//             self.Session = self.AddChild<NamedPipeSession>();
//         }
//
//         [EntitySystem]
//         private static void Destroy(this NamedPipeSessionComponent self)
//         {
//             self.Session?.Dispose();
//             self.Session = null;
//         }
//     }
// }