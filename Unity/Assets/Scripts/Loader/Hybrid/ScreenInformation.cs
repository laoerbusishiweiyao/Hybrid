using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace Chaos
{
    public static class ScreenInformation
    {
        static ScreenInformation()
        {
#if UNITY_EDITOR
            var assembly = Assembly.GetAssembly(typeof(EditorWindow));
            var gameViewType = assembly.GetType("UnityEditor.GameView");
            if (gameViewType == null)
            {
                return;
            }

            var windows = Resources.FindObjectsOfTypeAll(gameViewType);
            if (windows.Length == 0)
            {
                return;
            }

            gameViewWindow = windows[0] as EditorWindow;
            viewInWindowPropertyInfo = gameViewType.GetProperty("viewInWindow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            targetInViewPropertyInfo = gameViewType.GetProperty("targetInView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        }

#if UNITY_EDITOR
        private static readonly EditorWindow gameViewWindow;

        /// <summary>
        /// Game 视图在窗口内的区域(去除 Game 窗口自身的工具栏)
        /// </summary>
        private static readonly PropertyInfo viewInWindowPropertyInfo;

        /// <summary>
        /// 实际游戏画面在视图内的区域
        /// </summary>
        private static readonly PropertyInfo targetInViewPropertyInfo;
#endif

        public static Rect RenderArea
        {
            get
            {
#if UNITY_EDITOR
                var position = gameViewWindow.position;
                if (viewInWindowPropertyInfo.GetValue(gameViewWindow, null) is Rect viewInWindow && targetInViewPropertyInfo.GetValue(gameViewWindow, null) is Rect targetInView)
                {
                    return new Rect(targetInView.position.x, position.y + viewInWindow.y + (position.height - viewInWindow.height), targetInView.width, targetInView.height);
                }

                return position;
#else
                return new Rect(0, 0, Screen.width, Screen.height);
#endif
            }
        }

        public static double Left => RenderArea.x;

        public static double Top => RenderArea.y;

        public static double Width => RenderArea.width;

        public static double Height => RenderArea.height;
    }
}