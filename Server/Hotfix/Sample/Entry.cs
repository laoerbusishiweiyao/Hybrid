using Unity.Mathematics;

namespace Chaos;

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

        MongoRegister.RegisterStruct<float2>();
        MongoRegister.RegisterStruct<float3>();
        MongoRegister.RegisterStruct<float4>();
        MongoRegister.RegisterStruct<quaternion>();

        World.Default.AddSingleton<SceneTypeMapper, Type>(typeof(SceneType));
        World.Default.AddSingleton<ObjectPool>();
        World.Default.AddSingleton<IdGenerator>();
        World.Default.AddSingleton<OpcodeTypeRegistry>();

        CodeTypeRegistry.Default.Execute();

        await ThreadTask.CompletedTask;
    }
}