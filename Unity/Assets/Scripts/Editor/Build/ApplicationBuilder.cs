using System;
using System.IO;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using Serilog;
using UnityEditor;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

namespace Chaos
{
    public static class ApplicationBuilder
    {
        [MenuItem("Tools/Build/Windows")]
        private static void BuildWindows()
        {
            Build("../Release/Windows", "Unity", BuildTarget.StandaloneWindows64, BuildOptions.None);
        }

        [MenuItem("Tools/Build/HybridClr And YooAssets _F8", false)]
        private static void Prebuild()
        {
            HybridClrBuildAndCopy();
            YooAssetsBuildAndCopy();

            EditorUtility.DisplayDialog("信息", "预构建完成", "确定");
        }

        private static void YooAssetsBuildAndCopy(string packageName = "DefaultPackage", BuildTarget buildTarget = BuildTarget.StandaloneWindows64, string buildPipeline = nameof(ScriptableBuildPipeline))
        {
            var now = DateTime.Now;

            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            var builtinShadersBundleName = packRuleResult.GetBundleName(packageName, uniqueBundleName);

            var buildParameters = new ScriptableBuildParameters
            {
                PackageName = packageName,
                BuildTarget = buildTarget,

                BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),
                BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(),
                BuildPipeline = buildPipeline,
                BuildBundleType = (int)EBuildBundleType.AssetBundle,

                EnableSharePackRule = true,
                VerifyBuildingResult = true,
                FileNameStyle = EFileNameStyle.HashName,
                BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll,
                BuildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(packageName, buildPipeline),
                CompressOption = ECompressOption.LZ4,
                ClearBuildCacheFiles = false,
                UseAssetDependencyDB = true,

                PackageVersion = string.Join('-', now.ToString("yyyy-MM-dd"), now.Hour * 60 + now.Minute),

                EncryptionServices = new EncryptionNone(),
                ManifestProcessServices = new ManifestProcessNone(),
                ManifestRestoreServices = new ManifestRestoreNone(),

                BuiltinShadersBundleName = builtinShadersBundleName,
            };

            try
            {
                var pipeline = new ScriptableBuildPipeline();
                var buildResult = pipeline.Run(buildParameters, true);
                if (buildResult.Success)
                {
                    Log.Information("Build Package Complete\n{output}", buildResult.OutputPackageDirectory);

                    AssetDatabase.Refresh();

                    var settings = Resources.Load<GlobalSettings>(nameof(GlobalSettings));
                    if (!Version.TryParse(settings.Version, out var version))
                    {
                        Log.Warning("YooAssets Copy Failed, version = {version}", version);
                        return;
                    }

                    var platform = Application.platform switch
                    {
                        RuntimePlatform.Android => "android",
                        RuntimePlatform.IPhonePlayer => "ios",
                        _ => "windows",
                    };
                    var output = $"../Release/EdgeServer/cdn/{platform}/{settings.Version}";
                    foreach (var file in Directory.EnumerateFiles(buildResult.OutputPackageDirectory))
                    {
                        var dst = Path.Combine(output, Path.GetFileName(file));
                        if (File.Exists(dst))
                        {
                            File.Delete(dst);
                        }

                        FileUtil.CopyFileOrDirectory(file, dst);
                    }

                    Log.Information("Package Copied\n{output}", output);

                    AssetDatabase.Refresh();
                }
                else
                {
                    Log.Warning("Build Package Failed\n{error}", buildResult.ErrorInfo);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Failed to build\n{exception}", exception);
            }
        }

        private static void HybridClrBuildAndCopy(string dst = "Assets/Bundles/DefaultPackage/AotDlls")
        {
            PrebuildCommand.GenerateAll();

            AssetDatabase.Refresh();

            var target = EditorUserBuildSettings.activeBuildTarget;
            var src = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());

            if (Directory.Exists(dst))
            {
                Directory.Delete(dst, true);
            }

            Directory.CreateDirectory(dst);

            foreach (var aotDll in HybridCLRSettings.Instance.patchAOTAssemblies)
            {
                File.Copy(Path.Combine(src, aotDll), Path.Combine(dst, $"{aotDll}.bytes"), true);
            }

            Log.Debug("Copy AotDll Finish");

            AssetDatabase.Refresh();
        }

        private static void Build(string path, string name, BuildTarget buildTarget, BuildOptions buildOptions)
        {
            var extension = buildTarget switch
            {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.Android => ".apk",
                _ => string.Empty,
            };
            var output = Path.Combine(path, name + extension);

            AssetDatabase.Refresh();

            Log.Information("[Build] Start");

            var buildReport = BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/SampleScene.unity" }, output, buildTarget, buildOptions);
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                Log.Error("[Build] Failed···\n{result}", buildReport.summary.result);
                return;
            }

            Log.Information("[Build] Finish({output})", output);
        }
    }
}