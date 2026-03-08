namespace Chaos
{
    /// <summary>
    /// Android WebView 配置
    /// 用于初始化 Hybrid.Android.StandaloneWebViewClient
    /// </summary>
    public sealed record AndroidWebViewOptions
    {
        #region 基础配置

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

        #endregion

        #region 核心功能开关

        /// <summary>
        /// 是否启用 JavaScript 执行
        /// <para>⚠️ 禁用后大部分现代网页将无法正常工作</para>
        /// <para>默认值: <c>true</c></para>
        /// <para>建议值: <c>true</c> (Bing/Google 等必须启用)</para>
        /// </summary>
        public readonly bool JavaScriptEnabled;

        /// <summary>
        /// 是否启用 DOM Storage (localStorage / sessionStorage)
        /// <para>⚠️ 禁用后网页无法保存本地数据，可能导致登录态丢失</para>
        /// <para>默认值: <c>false</c> (Android 原生默认)</para>
        /// <para>建议值: <c>true</c> (现代网页必需)</para>
        /// </summary>
        public readonly bool DomStorageEnabled;

        /// <summary>
        /// 是否使用 viewport meta 标签的 width 属性
        /// <para>启用后网页会根据 viewport 的 width=device-width 进行响应式布局</para>
        /// <para>禁用则网页按桌面端宽度渲染，可能需横向滚动</para>
        /// <para>默认值: <c>false</c></para>
        /// <para>建议值: <c>true</c> (Bing 等响应式网站必需)</para>
        /// </summary>
        public readonly bool UseWideViewPort;

        /// <summary>
        /// 是否缩放内容以适应 WebView 宽度 (概览模式)
        /// <para>启用后页面会整体缩小以完整显示在屏幕内，可能导致内容过小</para>
        /// <para>⚠️ 与 UseWideViewPort 同时启用时，可能导致页面被"竖向拉伸"</para>
        /// <para>默认值: <c>false</c></para>
        /// <para>建议值: <c>false</c> (防止 Bing 页面拉伸的关键设置!)</para>
        /// </summary>
        public readonly bool LoadWithOverviewMode;

        #endregion

        #region 缩放控制

        /// <summary>
        /// 是否允许页面缩放 (双指/控件)
        /// <para>禁用后用户无法通过手势或按钮缩放页面</para>
        /// <para>默认值: <c>true</c> (Android 原生默认)</para>
        /// <para>建议值: <c>false</c> (固定视口体验，防止误操作)</para>
        /// </summary>
        public readonly bool SupportZoom;

        /// <summary>
        /// 是否显示内置缩放控件 (+/- 按钮)
        /// <para>仅当 SupportZoom=true 时生效</para>
        /// <para>默认值: <c>true</c></para>
        /// <para>建议值: <c>false</c> (保持界面简洁)</para>
        /// </summary>
        public readonly bool BuiltInZoomControls;

        /// <summary>
        /// 是否显示缩放控件的浮动提示按钮
        /// <para>仅当 SupportZoom=true 且 BuiltInZoomControls=true 时生效</para>
        /// <para>默认值: <c>true</c></para>
        /// <para>建议值: <c>false</c></para>
        /// </summary>
        public readonly bool DisplayZoomControls;

        /// <summary>
        /// 文本缩放比例 (百分比)
        /// <para>100 = 100% 原始大小, 150 = 1.5 倍放大</para>
        /// <para>不影响图片/布局，仅缩放文字</para>
        /// <para>默认值: <c>100</c></para>
        /// <para>建议值: <c>100</c> (保持设计稿比例)</para>
        /// </summary>
        public readonly int TextZoom;

        /// <summary>
        /// 页面初始缩放比例 (百分比)
        /// <para>0 = 自动计算, 100 = 100% 显示</para>
        /// <para>仅当 LoadWithOverviewMode=false 时生效</para>
        /// <para>默认值: <c>100</c></para>
        /// <para>建议值: <c>100</c> (避免自动缩放导致布局错乱)</para>
        /// </summary>
        public readonly int InitialScale;

        #endregion

        #region 安全与权限

        /// <summary>
        /// 媒体播放是否需要用户手势触发
        /// <para>启用后 &lt;video&gt;/&lt;audio&gt; 自动播放会被拦截</para>
        /// <para>默认值: <c>true</c> (Android 7.0+ 默认，符合 Chrome 策略)</para>
        /// <para>建议值: <c>false</c> (如需自动播放视频/音频)</para>
        /// </summary>
        public readonly bool MediaPlaybackRequiresUserGesture;

        /// <summary>
        /// 是否允许访问 file:// URL 资源
        /// <para>⚠️ 启用后可能读取本地文件，存在安全风险</para>
        /// <para>默认值: <c>true</c> (Android 原生默认)</para>
        /// <para>建议值: <c>false</c> (生产环境建议禁用，通过 shouldInterceptRequest 代理本地资源)</para>
        /// </summary>
        public readonly bool AllowFileAccess;

        /// <summary>
        /// 是否允许访问 content:// URL 资源
        /// <para>⚠️ 启用后可能访问 ContentProvider 数据</para>
        /// <para>默认值: <c>true</c></para>
        /// <para>建议值: <c>false</c> (除非明确需要)</para>
        /// </summary>
        public readonly bool AllowContentAccess;

        /// <summary>
        /// 混合内容模式 (HTTP + HTTPS)
        /// <para>取值: <seealso cref="MixedContentMode"/></para>
        /// <para>默认值: <c>MixedContentMode.NeverAllow</c> (0)</para>
        /// <para>建议值: <c>NeverAllow</c> (安全优先) 或 <c>CompatibilityMode</c> (兼容旧接口)</para>
        /// </summary>
        public readonly int MixedContentMode;

        #endregion

        #region 缓存与布局

        /// <summary>
        /// 缓存加载策略
        /// <para>取值: <seealso cref="CacheMode"/></para>
        /// <para>默认值: <c>CacheMode.LoadDefault</c> (0)</para>
        /// <para>建议值: 
        /// <list type="bullet">
        /// <item><c>LoadDefault</c>: 优先缓存，无缓存则网络 (常规)</item>
        /// <item><c>LoadCacheElseNetwork</c>: 仅用缓存，无缓存才联网 (离线模式)</item>
        /// <item><c>LoadNoCache</c>: 每次都从网络加载 (实时数据)</item>
        /// </list>
        /// </para>
        /// </summary>
        public readonly int CacheMode;

        /// <summary>
        /// 页面布局算法
        /// <para>取值: <seealso cref="LayoutAlgorithm"/></para>
        /// <para>默认值: <c>LayoutAlgorithm.Normal</c></para>
        /// <para>建议值: <c>Normal</c> (标准渲染)，避免使用已废弃的 SINGLE_COLUMN</para>
        /// </summary>
        public readonly string LayoutAlgorithm;

        #endregion

        #region 交互控制

        /// <summary>
        /// WebView 是否可获取焦点 (Focus)
        /// <para>焦点影响：键盘事件接收、软键盘触发、焦点指示器显示</para>
        /// <para>⚠️ Unity 混合开发关键：设为 false 可防止 WebView 抢焦点，避免 Unity UI 输入框失焦</para>
        /// <para>默认值: <c>false</c></para>
        /// <para>建议值: 
        /// <list type="bullet">
        /// <item><c>false</c>: WebView 作为背景层，不抢占焦点 (推荐)</item>
        /// <item><c>true</c>: 需要键盘导航网页内容时启用</item>
        /// </list>
        /// </para>
        /// </summary>
        public readonly bool Focusable;

        /// <summary>
        /// 触摸模式下是否可获取焦点
        /// <para>仅当 <see cref="Focusable"/> = true 时生效</para>
        /// <para>触摸模式下获取焦点会显示焦点高亮框，可能影响视觉体验</para>
        /// <para>默认值: <c>false</c></para>
        /// <para>建议值: <c>false</c> (保持界面干净，避免触摸时出现焦点框)</para>
        /// </summary>
        public readonly bool FocusableInTouchMode;

        /// <summary>
        /// WebView 是否响应点击/触摸事件
        /// <para>⚠️ 禁用后网页将完全无法交互，变成"静态图片"</para>
        /// <para>事件穿透技巧：如需部分区域穿透到 Unity，请重写 onTouchEvent 而非禁用 clickable</para>
        /// <para>默认值: <c>true</c></para>
        /// <para>建议值: 
        /// <list type="bullet">
        /// <item><c>true</c>: 正常网页交互 (绝大多数场景)</item>
        /// <item><c>false</c>: 临时冻结网页，仅用于特殊遮罩/暂停场景</item>
        /// </list>
        /// </para>
        /// </summary>
        public readonly bool Clickable;

        /// <summary>
        /// WebView 全局启用状态 (交互总开关)
        /// <para>设为 false 时：点击/滚动/输入/缩放/JS 交互全部禁用，视觉可能变灰</para>
        /// <para>动态控制场景：加载遮罩显示时临时禁用，加载完成后恢复</para>
        /// <para>默认值: <c>true</c></para>
        /// <para>建议值: 
        /// <list type="bullet">
        /// <item><c>true</c>: 正常运行状态</item>
        /// <item><c>false</c>: 临时冻结，配合 Unity 遮罩使用</item>
        /// </list>
        /// </para>
        /// </summary>
        public readonly bool Enabled;

        #endregion

        public AndroidWebViewOptions(
            // 基础配置
            string entryUrl = "https://chaos.com",
            string localAssetDomain = "chaos.com",
            string assetFolderName = "WebUI",
            ushort logEntryOpcode = Opcode.AndroidLogEntry,
            // 核心功能
            bool javaScriptEnabled = true,
            bool domStorageEnabled = true,
            bool useWideViewPort = true,
            bool loadWithOverviewMode = false,
            // 缩放控制
            bool supportZoom = false,
            bool builtInZoomControls = false,
            bool displayZoomControls = false,
            int textZoom = 100,
            int initialScale = 100,
            // 安全权限
            bool mediaPlaybackRequiresUserGesture = false,
            bool allowFileAccess = false,
            bool allowContentAccess = false,
            int mixedContentMode = WebViewMixedContentMode.NeverAllow,
            // 缓存布局
            int cacheMode = WebViewCacheMode.LoadDefault,
            string layoutAlgorithm = WebViewLayoutAlgorithm.Normal,
            // 交互控制
            bool focusable = true,
            bool focusableInTouchMode = true,
            bool clickable = true,
            bool enabled = true)
        {
            EntryUrl = entryUrl;
            LocalAssetDomain = localAssetDomain;
            AssetFolderName = assetFolderName;
            LogEntryOpcode = logEntryOpcode;

            JavaScriptEnabled = javaScriptEnabled;
            DomStorageEnabled = domStorageEnabled;
            UseWideViewPort = useWideViewPort;
            LoadWithOverviewMode = loadWithOverviewMode;

            SupportZoom = supportZoom;
            BuiltInZoomControls = builtInZoomControls;
            DisplayZoomControls = displayZoomControls;
            TextZoom = textZoom;
            InitialScale = initialScale;

            MediaPlaybackRequiresUserGesture = mediaPlaybackRequiresUserGesture;
            AllowFileAccess = allowFileAccess;
            AllowContentAccess = allowContentAccess;
            MixedContentMode = mixedContentMode;

            CacheMode = cacheMode;
            LayoutAlgorithm = layoutAlgorithm;

            Focusable = focusable;
            FocusableInTouchMode = focusableInTouchMode;
            Clickable = clickable;
            Enabled = enabled;
        }


        /// <summary>
        /// Android WebView CacheMode 常量 (对应 android.webkit.WebSettings)
        /// </summary>
        public static class WebViewCacheMode
        {
            /// <summary>
            /// 默认: 优先使用缓存，缓存不存在或过期则使用网络
            /// </summary>
            public const int LoadDefault = 0;

            /// <summary>
            /// 正常加载，不使用缓存
            /// </summary>
            public const int LoadNormal = 1;

            /// <summary>
            /// 仅使用缓存，缓存不存在则报错 (离线模式)
            /// </summary>
            public const int LoadCacheElseNetwork = 2;

            /// <summary>
            /// 不使用缓存，强制从网络加载
            /// </summary>
            public const int LoadNoCache = 3;

            /// <summary>
            /// 仅使用缓存，即使过期也不联网 (纯离线)
            /// </summary>
            public const int LoadCacheOnly = 4;
        }

        /// <summary>
        /// Android WebView MixedContentMode 常量 (API 21+)
        /// </summary>
        public static class WebViewMixedContentMode
        {
            /// <summary>
            /// 从不允许 HTTPS 页面加载 HTTP 资源 (最安全)
            /// </summary>
            public const int NeverAllow = 0;

            /// <summary>
            /// 允许加载任何混合内容 (兼容模式，有安全风险)
            /// </summary>
            public const int AlwaysAllow = 1;

            /// <summary>
            /// 兼容性模式: 允许加载部分安全的混合内容 (如图片/视频)
            /// </summary>
            public const int CompatibilityMode = 2;
        }

        /// <summary>
        /// Android WebView LayoutAlgorithm 枚举值
        /// </summary>
        public static class WebViewLayoutAlgorithm
        {
            /// <summary>
            /// 标准布局算法 (推荐)
            /// </summary>
            public const string Normal = "NORMAL";

            /// <summary>
            /// ⚠️ 已废弃: 单列布局，会重排页面元素
            /// </summary>
            public const string SingleColumn = "SINGLE_COLUMN";

            /// <summary>
            /// ⚠️ 已废弃: 窄列布局，用于小屏幕
            /// </summary>
            public const string NarrowColumns = "NARROW_COLUMNS";

            /// <summary>
            /// ⚠️ 已废弃: 文本自动缩放 (Android 4.4 以下)
            /// </summary>
            public const string TextAutosizing = "TEXT_AUTOSIZING";
        }
    }
}