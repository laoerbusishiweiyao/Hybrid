using System.IO;
using System.Threading;
using Serilog;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Chaos
{
    public static class AssemblyBuilder
    {
        private static SynchronizationContext unitySynchronizationContext;

        public static readonly string[] DllNames = { "Unity.Hotfix", "Unity.HotfixView", "Unity.Model", "Unity.ModelView" };

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            unitySynchronizationContext = SynchronizationContext.Current;
        }

        [MenuItem("Tools/Scripts/Compile _F6", false)]
        private static void MenuItemOfCompile()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Compile();
        }

        [MenuItem("Tools/Scripts/Reload _F7", false)]
        private static void MenuItemOfReload()
        {
            if (Application.isPlaying)
            {
                CodeLoader.Default?.Reload();
            }
        }

        public static void Compile()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var isCompileSuccessful = CompileScripts();
            if (!isCompileSuccessful)
            {
                return;
            }

            CopyHotUpdateDlls();
            Log.Information("编译完成");
        }

        private static bool CompileScripts()
        {
            var lastSynchronizationContext = Application.isPlaying ? SynchronizationContext.Current : null;
            SynchronizationContext.SetSynchronizationContext(unitySynchronizationContext);

            bool isCompileSuccessful;

            try
            {
                Directory.CreateDirectory(UnitySettings.BuildOutputDirectory);
                var target = EditorUserBuildSettings.activeBuildTarget;
                var group = BuildPipeline.GetBuildTargetGroup(target);
                ScriptCompilationSettings scriptCompilationSettings = new()
                {
                    group = group,
                    target = target,
                    extraScriptingDefines = new[] { "UNITY_COMPILE" },
                    options = EditorUserBuildSettings.development ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None
                };
                var result = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, UnitySettings.BuildOutputDirectory);
                isCompileSuccessful = result.assemblies.Count > 0;
                EditorUtility.ClearProgressBar();
            }
            finally
            {
                if (lastSynchronizationContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(lastSynchronizationContext);
                }
            }

            return isCompileSuccessful;
        }

        private static void CopyHotUpdateDlls()
        {
            if (Directory.Exists(UnitySettings.CodeDirectory))
            {
                Directory.Delete(UnitySettings.CodeDirectory, true);
            }

            if (!Directory.Exists(UnitySettings.CodeDirectory))
            {
                Directory.CreateDirectory(UnitySettings.CodeDirectory);
            }

            foreach (var dllName in DllNames)
            {
                var sourceDll = $"{UnitySettings.BuildOutputDirectory}/{dllName}.dll";
                var sourcePdb = $"{UnitySettings.BuildOutputDirectory}/{dllName}.pdb";
                File.Copy(sourceDll, $"{UnitySettings.CodeDirectory}/{dllName}.dll.bytes", true);
                File.Copy(sourcePdb, $"{UnitySettings.CodeDirectory}/{dllName}.pdb.bytes", true);
            }

            AssetDatabase.Refresh();
        }
    }
}