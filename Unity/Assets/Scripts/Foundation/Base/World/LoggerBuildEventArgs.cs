namespace Chaos
{
    public readonly struct LoggerBuildEventArgs
    {
        public readonly string SceneName;

        public LoggerBuildEventArgs(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}