namespace Chaos;

public sealed class SceneTypeMapper : CodeMapper<SceneTypeMapper>, ISingletonAwake<Type>
{
    public static bool IsSame(int left, int right)
    {
        if (left == right)
        {
            return true;
        }

        if (left == 0)
        {
            return true;
        }

        if (right == 0)
        {
            return true;
        }

        return false;
    }

    public void Awake(Type type)
    {
        Initialize(type);
    }

    public string GetSceneNameOrDefault(int sceneType)
    {
        return GetStringByValue(sceneType);
    }

    public int GetSceneType(string sceneName)
    {
        var type = GetValueByName(sceneName);
        return type == 0 ? throw new Exception($"not found scene type: {type} {sceneName}") : type;
    }
}