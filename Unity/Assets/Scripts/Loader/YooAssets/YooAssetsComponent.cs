using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Chaos
{
    public sealed class RemoteServices : IRemoteServices
    {
        private readonly string defaultHostServer;
        private readonly string fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            this.defaultHostServer = defaultHostServer;
            this.fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{fallbackHostServer}/{fileName}";
        }
    }

    public sealed class YooAssetsComponent : Singleton<YooAssetsComponent>, ISingletonAwake
    {
        public void Awake()
        {
            YooAssets.Initialize();
        }

        protected override void Destroy()
        {
            YooAssets.Destroy();
        }

        public async ThreadTask CreatePackageAsync(string packageName, bool isDefault = false)
        {
            var settings = Resources.Load<YooAssetBuildSettings>(nameof(YooAssetBuildSettings));
            var package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                YooAssets.SetDefaultPackage(package);
            }

            switch (settings.PlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                {
                    var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                    var packageRoot = buildResult.PackageRootDirectory;
                    EditorSimulateModeParameters parameters = new()
                    {
                        EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                    };
                    await package.InitializeAsync(parameters).Task;
                    break;
                }
                case EPlayMode.OfflinePlayMode:
                {
                    OfflinePlayModeParameters parameters = new()
                    {
                        BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters()
                    };
                    await package.InitializeAsync(parameters).Task;
                    break;
                }
                case EPlayMode.HostPlayMode:
                {
                    var defaultHostServer = GetHostServerURL(settings.Url, package.PackageName);
                    var fallbackHostServer = GetHostServerURL(settings.Url, package.PackageName);
                    var services = new RemoteServices(defaultHostServer, fallbackHostServer);
                    HostPlayModeParameters parameters = new()
                    {
                        BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(),
                        CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(services)
                    };
                    await package.InitializeAsync(parameters).Task;
                    break;
                }
                case EPlayMode.WebPlayMode:
                {
                    var defaultHostServer = GetHostServerURL(settings.Url, package.PackageName);
                    var fallbackHostServer = GetHostServerURL(settings.Url, package.PackageName);
                    var remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    WebPlayModeParameters parameters = new()
                    {
                        WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(),
                        WebRemoteFileSystemParameters = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices)
                    };
                    await package.InitializeAsync(parameters).Task;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var version = package.RequestPackageVersionAsync();
            await version.Task;
            await package.UpdatePackageManifestAsync(version.PackageVersion).Task;
        }

        string GetHostServerURL(string url, string pacakgeName)
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = url;
            string appVersion = "v1.0";


#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                    return $"{hostServerIP}/CDN/Android/{appVersion}";
                case UnityEditor.BuildTarget.iOS:
                    return $"{hostServerIP}/CDN/IPhone/{appVersion}";
                case UnityEditor.BuildTarget.WebGL:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{pacakgeName}";
                }
                default:
                    return $"{hostServerIP}/CDN/PC/{appVersion}";
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return $"{hostServerIP}/CDN/Android/{appVersion}";
                case RuntimePlatform.IPhonePlayer:
                    return $"{hostServerIP}/CDN/IPhone/{appVersion}";
                case RuntimePlatform.WebGLPlayer:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{pacakgeName}";
                }
                default:
                    return $"{hostServerIP}/CDN/PC/{appVersion}";
            }
#endif
        }

        public async ThreadTask DestroyPackageAsync(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            await package.DestroyAsync().Task;
        }

        public async ThreadTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            var assetHandle = YooAssets.LoadAssetAsync<T>(location);
            await assetHandle.Task;
            var assetObject = (T)assetHandle.AssetObject;
            assetHandle.Release();
            return assetObject;
        }

        public async ThreadTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            var allAssetsHandle = YooAssets.LoadAllAssetsAsync<T>(location);
            await allAssetsHandle.Task;
            var allAssets = new Dictionary<string, T>();
            foreach (var assetObject in allAssetsHandle.AllAssetObjects)
            {
                var asset = (T)assetObject;
                allAssets.Add(asset.name, asset);
            }

            allAssetsHandle.Release();
            return allAssets;
        }
    }
}