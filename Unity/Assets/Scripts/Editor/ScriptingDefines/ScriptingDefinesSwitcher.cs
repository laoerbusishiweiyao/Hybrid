using System.Linq;
using Serilog;
using UnityEditor;
using UnityEditor.Build;

namespace Chaos
{
    public static class ScriptingDefinesSwitcher
    {
#if HIERARCHY
        [MenuItem("Tools/ScriptingDefines/Remove HIERARCHY", false)]
        public static void RemoveEnableView()
        {
            SwitchDefineSymbols("HIERARCHY", false);
        }
#else
        [MenuItem("Tools/ScriptingDefines/Add HIERARCHY", false)]
        public static void AddEnableView()
        {
            SwitchDefineSymbols("HIERARCHY", true);
        }
#endif
        public static void SwitchDefineSymbols(string symbol, bool state)
        {
            Log.Information("Define symbols {symbols} = {state}", symbol, state);
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            var symbols = defineSymbols.Split(';').ToList();
            if (state)
            {
                if (symbols.Contains(symbol))
                {
                    return;
                }

                symbols.Add(symbol);
            }
            else
            {
                if (!symbols.Contains(symbol))
                {
                    return;
                }

                symbols.Remove(symbol);
            }

            defineSymbols = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), defineSymbols);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}