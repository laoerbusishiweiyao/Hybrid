using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Chaos
{
    [InitializeOnLoad]
    public static class GameViewContext
    {
        private static EditorWindow gameViewWindow;

        /// <summary>
        /// Game 视图在窗口内的区域(去除 Game 窗口自身的工具栏)
        /// </summary>
        private static PropertyInfo viewInWindowPropertyInfo;

        /// <summary>
        /// 实际游戏画面在视图内的区域
        /// </summary>
        private static PropertyInfo targetInViewPropertyInfo;

        public static Rect RenderArea
        {
            get
            {
                var position = gameViewWindow.position;
                if (viewInWindowPropertyInfo.GetValue(gameViewWindow, null) is Rect viewInWindow && targetInViewPropertyInfo.GetValue(gameViewWindow, null) is Rect targetInView)
                {
                    return new Rect(targetInView.position.x, position.y + viewInWindow.y + (position.height - viewInWindow.height), targetInView.width, targetInView.height);
                }

                return position;
            }
        }

        static GameViewContext()
        {
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


            // string[] callbacks =
            // {
            //     "OnResized",
            //     "OnBackgroundViewResized",
            //     "OnPlayModeStateChanged",
            //     "OnEditorModeChanged",
            //     "SizeSelectionCallback"
            // };

            // foreach (var callbackName in callbacks)
            // {
            //     // 检查是否是方法
            //     var method = gameViewType.GetMethod(callbackName,
            //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //
            //     if (method != null)
            //     {
            //         Debug.Log($"[方法] {callbackName}");
            //         Debug.Log($"  └─ 返回类型：{method.ReturnType}");
            //         Debug.Log($"  └─ 参数：{string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"))}");
            //         Debug.Log($"  └─ 访问修饰符：{(method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected")}");
            //     }
            //
            //     // 检查是否是字段（委托）
            //     var field = gameViewType.GetField(callbackName,
            //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //
            //     if (field != null)
            //     {
            //         Debug.Log($"[字段/委托] {callbackName}");
            //         Debug.Log($"  └─ 类型：{field.FieldType}");
            //         Debug.Log($"  └─ 是委托：{typeof(Delegate).IsAssignableFrom(field.FieldType)}");
            //     }
            // }

            Debug.Log(RenderArea);
            Debug.Log(Process.GetCurrentProcess().Id);
        }


        [MenuItem("Tools/GameViewContext")]
        private static void GetGameView()
        {
            if (!gameViewWindow)
            {
                return;
            }

            Debug.Log(Process.GetCurrentProcess().Id);
            Debug.Log(RenderArea);
            Debug.Log(Process.GetCurrentProcess().Id);
        }
    }
}