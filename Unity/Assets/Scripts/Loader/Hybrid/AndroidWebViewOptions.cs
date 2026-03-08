namespace Chaos
{
    /// <summary>
    /// Android WebView 配置
    /// 用于初始化 Hybrid.Android.StandaloneWebViewClient
    /// </summary>
    public sealed record AndroidWebViewOptions
    {
        /// <summary>
        /// 入口 URL
        /// <para>本地资源格式: "https://{LocalAssetDomain}/index.html"</para>
        /// <para>远程资源格式: "https://example.com/app"</para>
        /// <para>默认值: "https://chaos.com/index.html"</para>
        /// </summary>
        public readonly string EntryUrl;

        /// <summary>
        /// 本地资源虚拟域名 (拦截域名)
        /// <para>当请求 URL 的 host 匹配此域名时，请求将被拦截并从本地文件系统加载</para>
        /// <para>必须与 Android 侧 <c>com.chaosstudio.nativebridge.StandaloneWebViewClient.domain</c> 保持一致</para>
        /// <para>默认值: "chaos.com"</para>
        /// </summary>
        public readonly string LocalAssetDomain;

        /// <summary>
        /// 本地资源持久化文件夹名称
        /// <para>对应 Android: <c>context.getExternalFilesDir({AssetFolderName})</c></para>
        /// <para>默认值: "WebUI"</para>
        /// </summary>
        public readonly string AssetFolderName;

        /// <summary>
        /// Android 端日志消息码
        /// <para>默认值: <seealso cref="Opcode.AndroidLogEntry"/></para>
        /// </summary>
        public readonly ushort LogEntryOpcode;

        public AndroidWebViewOptions(string entryUrl = "https://chaos.com", string localAssetDomain = "chaos.com", string assetFolderName = "WebUI", ushort logEntryOpcode = Opcode.AndroidLogEntry)
        {
            EntryUrl = entryUrl;
            LocalAssetDomain = localAssetDomain;
            AssetFolderName = assetFolderName;
            LogEntryOpcode = logEntryOpcode;
        }
    }
}