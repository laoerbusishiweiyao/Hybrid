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

            EntitySerializeRegister.Initialize();

            MongoRegister.RegisterStruct<float2>();
            MongoRegister.RegisterStruct<float3>();
            MongoRegister.RegisterStruct<float4>();
            MongoRegister.RegisterStruct<quaternion>();

            World.Default.AddSingleton<SceneTypeMapper, Type>(typeof(SceneType));
            World.Default.AddSingleton<ObjectPool>();
            World.Default.AddSingleton<IdGenerator>();
            World.Default.AddSingleton<OpcodeTypeRegistry>();

            World.Default.AddSingleton<MessageQueue>();

            var networkLogger = World.Default.AddSingleton<NetworkLogger>();
            networkLogger.AddIgnore(typeof(Client2GatePingRequest));
            networkLogger.AddIgnore(typeof(Gate2ClientPingResponse));
            networkLogger.AddIgnore(typeof(MessageResponse));

            World.Default.AddSingleton<WebUiLogger>();

            CodeTypeRegistry.Default.Execute();

            // await World.Default.AddSingleton<ConfigLoader>().LoadAsync();
            // World.Default.AddSingleton<NavmeshComponent>();

            var sceneType = SceneTypeMapper.Default.GetSceneType(Options.Default.SceneName);
            await FiberRegistry.Default.CreateMainFiberAsync(sceneType, $"{Options.Default.SceneName}@{Options.Default.Process}@{Options.Default.ReplicaIndex}");

            await ThreadTask.CompletedTask;
        }
    }
}