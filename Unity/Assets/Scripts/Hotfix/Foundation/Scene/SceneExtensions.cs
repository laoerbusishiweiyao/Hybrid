namespace Chaos
{
    public static class SceneExtensions
    {
        public static string GetSceneConfigName(this string sceneName)
        {
            return sceneName.Split("@")[0];
        }
    }
}