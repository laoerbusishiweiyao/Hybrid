using System;
using System.Collections.Generic;
using Serilog;
using TMPro;
using UnityEngine;
using YooAsset;

namespace Chaos
{
    public sealed class YooAssetsFileOffsetDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            var decryptResult = new DecryptResult
            {
                ManagedStream = null,
                Result = AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset())
            };
            return decryptResult;
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            var decryptResult = new DecryptResult
            {
                ManagedStream = null,
                CreateRequest = AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset())
            };
            return decryptResult;
        }

        /// <summary>
        /// 后备方式获取解密的资源包对象
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleFallback(DecryptFileInfo fileInfo)
        {
            return new DecryptResult();
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        private static ulong GetFileOffset()
        {
            return 32;
        }
    }

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
                    var initialization = package.InitializeAsync(parameters);
                    await initialization.Task;
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

            var manifest = package.UpdatePackageManifestAsync(version.PackageVersion);
            await manifest.Task;
        }

        string GetHostServerURL(string url, string packageName)
        {
            var settings = Resources.Load<GlobalSettings>(nameof(GlobalSettings));

            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = url;
            var appVersion = settings.Version;


#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                    return $"{hostServerIP}/cdn/Android/{appVersion}";
                case UnityEditor.BuildTarget.iOS:
                    return $"{hostServerIP}/cdn/IPhone/{appVersion}";
                case UnityEditor.BuildTarget.WebGL:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{packageName}";
                }
                default:
                    return $"{hostServerIP}/cdn/windows/{appVersion}";
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return $"{hostServerIP}/cdn/Android/{appVersion}";
                case RuntimePlatform.IPhonePlayer:
                    return $"{hostServerIP}/cdn/IPhone/{appVersion}";
                case RuntimePlatform.WebGLPlayer:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{packageName}";
                }
                default:
                    return $"{hostServerIP}/cdn/windows/{appVersion}";
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