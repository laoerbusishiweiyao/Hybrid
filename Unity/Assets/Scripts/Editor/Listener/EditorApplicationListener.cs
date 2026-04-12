using UnityEditor;

namespace Chaos
{
    [InitializeOnLoad]
    public static class EditorApplicationListener
    {

        static EditorApplicationListener()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
        }
    }
}