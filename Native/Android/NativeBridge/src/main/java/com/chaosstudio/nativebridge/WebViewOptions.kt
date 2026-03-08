package com.chaosstudio.nativebridge

/**
 * WebView 运行时配置
 * 与 Unity 侧 AndroidWebViewOptions 字段一一对应
 */
data class WebViewOptions(val entryUrl: String = "https://chaos.com/index.html", val localAssetDomain: String = "chaos.com", val assetFolderName: String = "WebUI", val logEntryOpcode: UShort = 10101.toUShort()) {
    /**
     * 部分更新配置 (返回新实例，不可变设计)
     */
    fun copyWith(
        entryUrl: String? = null,
        localAssetDomain: String? = null,
        assetFolderName: String? = null,
        logEntryOpcode: UShort? = null
    ): WebViewOptions = copy(
        entryUrl = entryUrl ?: this.entryUrl,
        localAssetDomain = localAssetDomain ?: this.localAssetDomain,
        assetFolderName = assetFolderName ?: this.assetFolderName,
        logEntryOpcode = logEntryOpcode ?: this.logEntryOpcode
    )
}