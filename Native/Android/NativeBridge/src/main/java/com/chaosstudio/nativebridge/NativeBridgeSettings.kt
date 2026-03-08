package com.chaosstudio.nativebridge

import org.json.JSONObject
import java.util.concurrent.locks.ReentrantLock
import kotlin.concurrent.withLock

const val UnityGameObjectName = "Program"
const val UnityMethodName = "OnWebMessage"
const val WebBridgeName = "bridge"

object NativeBridgeSettings {
    private val defaultWebViewOptions = WebViewOptions()

    @Volatile
    private var webViewOptionsInstance: WebViewOptions = defaultWebViewOptions
    val webViewOptions: WebViewOptions
        get() = webViewOptionsInstance

    private val lock = ReentrantLock()

    /**
     * 通过 JSON 更新配置 (供 Unity 调用)
     * @param content Unity 传来的配置 JSON
     * @return 更新后的配置
     */
    fun update(content: String): WebViewOptions = lock.withLock {
        try {
            val json = JSONObject(content)
            webViewOptionsInstance = webViewOptionsInstance.copyWith(
                // 基础配置 (空字符串不更新)
                entryUrl = json.optString("EntryUrl").takeIf { it.isNotEmpty() },
                localAssetDomain = json.optString("LocalAssetDomain").takeIf { it.isNotEmpty() },
                assetFolderName = json.optString("AssetFolderName").takeIf { it.isNotEmpty() },
                logEntryOpcode = json.optInt("LogEntryOpcode").takeIf { it in 0..65535 }?.toUShort(),

                // 核心功能开关 (仅当字段存在时更新，避免默认值覆盖)
                javaScriptEnabled = json.takeIf { it.has("JavaScriptEnabled") }?.optBoolean("JavaScriptEnabled"),
                domStorageEnabled = json.takeIf { it.has("DomStorageEnabled") }?.optBoolean("DomStorageEnabled"),
                useWideViewPort = json.takeIf { it.has("UseWideViewPort") }?.optBoolean("UseWideViewPort"),
                loadWithOverviewMode = json.takeIf { it.has("LoadWithOverviewMode") }?.optBoolean("LoadWithOverviewMode"),

                // 缩放控制
                supportZoom = json.takeIf { it.has("SupportZoom") }?.optBoolean("SupportZoom"),
                builtInZoomControls = json.takeIf { it.has("BuiltInZoomControls") }?.optBoolean("BuiltInZoomControls"),
                displayZoomControls = json.takeIf { it.has("DisplayZoomControls") }?.optBoolean("DisplayZoomControls"),
                textZoom = json.optInt("TextZoom").takeIf { it in 10..500 },  // 10%~500% 合理范围
                initialScale = json.optInt("InitialScale").takeIf { it in 0..500 },  // 0=自动

                // 安全与权限
                mediaPlaybackRequiresUserGesture = json.takeIf { it.has("MediaPlaybackRequiresUserGesture") }?.optBoolean("MediaPlaybackRequiresUserGesture"),
                allowFileAccess = json.takeIf { it.has("AllowFileAccess") }?.optBoolean("AllowFileAccess"),
                allowContentAccess = json.takeIf { it.has("AllowContentAccess") }?.optBoolean("AllowContentAccess"),
                mixedContentMode = json.optInt("MixedContentMode").takeIf { it in 0..2 },  // 0,1,2 有效值

                // 缓存与布局
                cacheMode = json.optInt("CacheMode").takeIf { it in 0..4 },  // LOAD_DEFAULT ~ LOAD_CACHE_ONLY
                layoutAlgorithm = json.optString("LayoutAlgorithm").takeIf { it.isNotEmpty() && it.uppercase() in arrayOf("NORMAL", "SINGLE_COLUMN", "NARROW_COLUMNS", "TEXT_AUTOSIZING") },

                // 交互控制
                focusable = json.takeIf { it.has("Focusable") }?.optBoolean("Focusable"),
                focusableInTouchMode = json.takeIf { it.has("FocusableInTouchMode") }?.optBoolean("FocusableInTouchMode"),
                clickable = json.takeIf { it.has("Clickable") }?.optBoolean("Clickable"),
                enabled = json.takeIf { it.has("Enabled") }?.optBoolean("Enabled")
            )
            webViewOptionsInstance
        } catch (exception: Exception) {
            UnityBridge.error("WebViewOptions update failed. ${exception.message}")
            webViewOptionsInstance
        }
    }

    /**
     * 重置为默认配置
     */
    fun reset() = lock.withLock {
        webViewOptionsInstance = defaultWebViewOptions
    }

    /**
     * 直接设置配置 (供内部使用)
     */
    internal fun set(options: WebViewOptions) {
        webViewOptionsInstance = options
    }
}

