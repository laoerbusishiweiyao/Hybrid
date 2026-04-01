using MongoDB.Bson.Serialization.Attributes;
using Serilog;

namespace Chaos
{
    [ChildOf]
    public sealed partial class Scene : Entity, IScene
    {
        [BsonIgnore]
        public Fiber Fiber { get; set; }

        public string Name { get; set; }

        public int SceneType { get; set; }

        public Scene()
        {
        }

        public Scene(Fiber fiber, long id, int sceneType, string name)
        {
            Id = id;
            Name = name;
            InstanceId = fiber.NewInstanceId();
            SceneType = sceneType;
            DisableDeserializeSystem = true;
            Fiber = fiber;
            Scene = this;
            IsRegister = true;
            Log.Information("Scene Create: {sceneName} {sceneType} {id} {instanceId}", SceneTypeMapper.Default.GetSceneNameOrDefault(SceneType), SceneType, Id, InstanceId);
        }

        public override void Dispose()
        {
            base.Dispose();
            Log.Information("Scene Dispose: {sceneName} {sceneType} {id} {instanceId}", SceneTypeMapper.Default.GetSceneNameOrDefault(SceneType), SceneType, Id, InstanceId);
        }

        protected override string HierarchyName => $"{GetType().Name} ({Name})";
    }
}