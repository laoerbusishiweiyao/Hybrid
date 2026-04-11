using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Serilog;
using TMPro;
using UnityEngine;

namespace Chaos
{
    public sealed class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
    {
        private Dictionary<string, TextAsset> dlls;
        private Dictionary<string, TextAsset> aotDlls;
        private readonly List<Assembly> assemblies = new();

        public void Awake()
        {
        }

        private async ThreadTask DownloadAsync()
        {
            try
            {
#if UNITY_EDITOR
                await ThreadTask.CompletedTask;
#else
                dlls = await YooAssetsComponent.Default.LoadAllAssetsAsync<TextAsset>("Assets/Bundles/DefaultPackage/Code/Unity.Model.dll.bytes");
                aotDlls = await YooAssetsComponent.Default.LoadAllAssetsAsync<TextAsset>("Assets/Bundles/DefaultPackage/AotDlls/mscorlib.dll.bytes");
#endif
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }

        public async ThreadTask StartAsync()
        {
            await DownloadAsync();

            HashSet<string> assemblyNames = new()
            {
                "Unity.Foundation",
                "Unity.Loader",
#if UNITY_EDITOR
                "Unity.Model",
                "Unity.ModelView",
                "Unity.BehaviorTree.Editor",
#endif
            };

            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in domainAssemblies)
            {
                var assemblyName = assembly.GetName().Name;
                if (assemblyNames.Contains(assemblyName))
                {
                    assemblies.Add(assembly);
                }
            }

            List<Assembly> list = new(assemblies);

#if !UNITY_EDITOR
            var modelAssBytes = dlls["Unity.Model.dll"].bytes;
            var modelPdbBytes = dlls["Unity.Model.pdb"].bytes;
            var modelViewAssBytes = dlls["Unity.ModelView.dll"].bytes;
            var modelViewPdbBytes = dlls["Unity.ModelView.pdb"].bytes;

            foreach (var pair in aotDlls)
            {
                var textAsset = pair.Value;
                HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HybridCLR.HomologousImageMode.SuperSet);
            }

            var modelAssembly = Assembly.Load(modelAssBytes, modelPdbBytes);
            var modelViewAssembly = Assembly.Load(modelViewAssBytes, modelViewPdbBytes);
            list.Add(modelAssembly);
            list.Add(modelViewAssembly);
#endif

            var (hotfixAssembly, hotfixViewAssembly) = LoadHotfix();

            list.Add(hotfixViewAssembly);
            list.Add(hotfixAssembly);
            World.Default.AddSingleton<CodeTypeRegistry, Assembly[]>(list.ToArray());
            var invoker = new StaticMethodInvoker(hotfixAssembly, "Chaos.Entry", "Start");
            invoker.Run();
        }

        private (Assembly, Assembly) LoadHotfix(bool isReload = false)
        {
#if UNITY_EDITOR
            Assembly hotfixAssembly = null;
            Assembly hotfixViewAssembly = null;

            if (isReload)
            {
                var hotfixAssBytes = File.ReadAllBytes(Path.Combine(UnitySettings.CodeDirectory, "Unity.Hotfix.dll.bytes"));
                var hotfixPdbBytes = File.ReadAllBytes(Path.Combine(UnitySettings.CodeDirectory, "Unity.Hotfix.pdb.bytes"));
                var hotfixViewAssBytes = File.ReadAllBytes(Path.Combine(UnitySettings.CodeDirectory, "Unity.HotfixView.dll.bytes"));
                var hotfixViewPdbBytes = File.ReadAllBytes(Path.Combine(UnitySettings.CodeDirectory, "Unity.HotfixView.pdb.bytes"));
                hotfixAssembly = Assembly.Load(hotfixAssBytes, hotfixPdbBytes);
                hotfixViewAssembly = Assembly.Load(hotfixViewAssBytes, hotfixViewPdbBytes);
            }
            else
            {
                var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in domainAssemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    if (assemblyName == "Unity.Hotfix")
                    {
                        hotfixAssembly = assembly;
                        continue;
                    }

                    if (assemblyName == "Unity.HotfixView")
                    {
                        hotfixViewAssembly = assembly;
                        continue;
                    }

                    if (hotfixAssembly != null && hotfixViewAssembly != null)
                    {
                        break;
                    }
                }
            }
#else
            var hotfixAssemblyBytes = dlls["Unity.Hotfix.dll"].bytes;
            var hotfixPdbBytes = dlls["Unity.Hotfix.pdb"].bytes;
            var hotfixViewAssemblyBytes = dlls["Unity.HotfixView.dll"].bytes;
            var hotfixViewPdbBytes = dlls["Unity.HotfixView.pdb"].bytes;

            var hotfixAssembly = Assembly.Load(hotfixAssemblyBytes, hotfixPdbBytes);
            var hotfixViewAssembly = Assembly.Load(hotfixViewAssemblyBytes, hotfixViewPdbBytes);
#endif
            return (hotfixAssembly, hotfixViewAssembly);
        }

        public void Reload()
        {
            var (hotfixAssembly, hotfixViewAssembly) = LoadHotfix(true);
            List<Assembly> list = new(assemblies)
            {
                hotfixViewAssembly,
                hotfixAssembly
            };
            var codeTypeRegistry = World.Default.AddSingleton<CodeTypeRegistry, Assembly[]>(list.ToArray());
            codeTypeRegistry.Execute();

            Log.Information("Reload dll finish.");
        }
    }
}