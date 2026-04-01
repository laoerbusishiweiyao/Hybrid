using System;
using Unity.Mathematics;

namespace Chaos
{
    public static class Entry
    {
        public static void Start()
        {
            StartAsync().Coroutine();
        }

        private static async ThreadTask StartAsync()
        {
            WindowsNativeMethods.Initialize();

            MongoRegister.Initialize();
            MemoryPackRegister.Initialize();

            // // 注册Entity序列化器
            // EntitySerializeRegister.Init();

            MongoRegister.RegisterStruct<float2>();
            MongoRegister.RegisterStruct<float3>();
            MongoRegister.RegisterStruct<float4>();
            MongoRegister.RegisterStruct<quaternion>();

            World.Default.AddSingleton<SceneTypeMapper, Type>(typeof(SceneType));
            World.Default.AddSingleton<ObjectPool>();
            World.Default.AddSingleton<IdGenerator>();
            World.Default.AddSingleton<OpcodeTypeRegistry>();

            // World.Instance.AddSingleton<MessageQueue>();

            // LogMsg logMsg = World.Instance.AddSingleton<LogMsg>();
            // logMsg.AddIgnore(typeof(C2G_Ping));
            // logMsg.AddIgnore(typeof(G2C_Ping));
            // logMsg.AddIgnore(typeof(MessageResponse));

            CodeTypeRegistry.Default.Execute();
            //
            // await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();
            // World.Instance.AddSingleton<NavmeshComponent>();
            //
            // int sceneType = SceneTypeSingleton.Instance.GetSceneType(Options.Instance.SceneName);
            // await FiberManager.Instance.CreateMainFiber(sceneType, $"{Options.Instance.SceneName}@{Options.Instance.Process}@{Options.Instance.ReplicaIndex}");

            await ThreadTask.CompletedTask;
        }
    }
}