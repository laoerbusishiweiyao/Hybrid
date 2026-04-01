namespace Chaos;

public static class EntityExtensions
{
    public static int Zone(this Entity entity)
    {
        return entity.Scene.Fiber.Zone;
    }

    public static Scene Scene(this Entity entity)
    {
        return entity.Scene as Scene;
    }

    public static T Scene<T>(this Entity entity) where T : class, IScene
    {
        return entity.Scene as T;
    }

    public static Scene Root(this Entity entity)
    {
        return entity.Scene.Fiber.Root;
    }

    public static Fiber Fiber(this Entity entity)
    {
        return entity.Scene.Fiber;
    }
}