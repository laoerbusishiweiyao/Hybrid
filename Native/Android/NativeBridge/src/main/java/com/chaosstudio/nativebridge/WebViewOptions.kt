package com.chaosstudio.nativebridge

import android.webkit.WebSettings

/**
 * WebView 运行时配置
 * 与 Unity 侧 AndroidWebViewOptions 字段一一对应
 */
data class WebViewOptions(
    // 基础配置
    val entryUrl: String = "https://chaos.com/index.html",
    val localAssetDomain: String = "chaos.com",
    val assetFolderName: String = "WebUI",
    val logEntryOpcode: UShort = 10101.toUShort(),

    // 核心功能开关
    val javaScriptEnabled: Boolean = true,
    val domStorageEnabled: Boolean = true,
    val useWideViewPort: Boolean = true,
    val loadWithOverviewMode: Boolean = false,

    // 缩放控制
    val supportZoom: Boolean = false,
    val builtInZoomControls: Boolean = false,
    val displayZoomControls: Boolean = false,
    val textZoom: Int = 100,
    val initialScale: Int = 100,

    // 安全与权限
    val mediaPlaybackRequiresUserGesture: Boolean = false,
    val allowFileAccess: Boolean = false,
    val allowContentAccess: Boolean = false,
    val mixedContentMode: Int = WebSettings.MIXED_CONTENT_NEVER_ALLOW,

    // 缓存与布局
    val cacheMode: Int = WebSettings.LOAD_DEFAULT,
    val layoutAlgorithm: String = "NORMAL",

    // 交互控制
    val focusable: Boolean = false,
    val focusableInTouchMode: Boolean = false,
    val clickable: Boolean = true,
    val enabled: Boolean = true
) {
    /**
     * 部分更新配置 (返回新实例，不可变设计)
     */
    fun copyWith(
        // 基础配置
        entryUrl: String? = null,
        localAssetDomain: String? = null,
        assetFolderName: String? = null,
        logEntryOpcode: UShort? = null,

        // 核心功能
        javaScriptEnabled: Boolean? = null,
        domStorageEnabled: Boolean? = null,
        useWideViewPort: Boolean? = null,
        loadWithOverviewMode: Boolean? = null,

        // 缩放控制
        supportZoom: Boolean? = null,
        builtInZoomControls: Boolean? = null,
        displayZoomControls: Boolean? = null,
        textZoom: Int? = null,
        initialScale: Int? = null,

        // 安全权限
        mediaPlaybackRequiresUserGesture: Boolean? = null,
        allowFileAccess: Boolean? = null,
        allowContentAccess: Boolean? = null,
        mixedContentMode: Int? = null,

        // 缓存布局
        cacheMode: Int? = null,
        layoutAlgorithm: String? = null,

        // 交互控制
        focusable: Boolean? = null,
        focusableInTouchMode: Boolean? = null,
        clickable: Boolean? = null,
        enabled: Boolean? = null
    ): WebViewOptions = copy(
        // 基础配置
        entryUrl = entryUrl ?: this.entryUrl,
        localAssetDomain = localAssetDomain ?: this.localAssetDomain,
        assetFolderName = assetFolderName ?: this.assetFolderName,
        logEntryOpcode = logEntryOpcode ?: this.logEntryOpcode,

        // 核心功能
        javaScriptEnabled = javaScriptEnabled ?: this.javaScriptEnabled,
        domStorageEnabled = domStorageEnabled ?: this.domStorageEnabled,
        useWideViewPort = useWideViewPort ?: this.useWideViewPort,
        loadWithOverviewMode = loadWithOverviewMode ?: this.loadWithOverviewMode,

        // 缩放控制
        supportZoom = supportZoom ?: this.supportZoom,
        builtInZoomControls = builtInZoomControls ?: this.builtInZoomControls,
        displayZoomControls = displayZoomControls ?: this.displayZoomControls,
        textZoom = textZoom ?: this.textZoom,
        initialScale = initialScale ?: this.initialScale,

        // 安全权限
        mediaPlaybackRequiresUserGesture = mediaPlaybackRequiresUserGesture ?: this.mediaPlaybackRequiresUserGesture,
        allowFileAccess = allowFileAccess ?: this.allowFileAccess,
        allowContentAccess = allowContentAccess ?: this.allowContentAccess,
        mixedContentMode = mixedContentMode ?: this.mixedContentMode,

        // 缓存布局
        cacheMode = cacheMode ?: this.cacheMode,
        layoutAlgorithm = layoutAlgorithm ?: this.layoutAlgorithm,

        // 交互控制
        focusable = focusable ?: this.focusable,
        focusableInTouchMode = focusableInTouchMode ?: this.focusableInTouchMode,
        clickable = clickable ?: this.clickable,
        enabled = enabled ?: this.enabled
    )
}