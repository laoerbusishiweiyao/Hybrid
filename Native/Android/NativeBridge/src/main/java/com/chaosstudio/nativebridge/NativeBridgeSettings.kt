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
                entryUrl = json.optString("EntryUrl").takeIf { it.isNotEmpty() },
                localAssetDomain = json.optString("LocalAssetDomain").takeIf { it.isNotEmpty() },
                assetFolderName = json.optString("AssetFolderName").takeIf { it.isNotEmpty() },
                logEntryOpcode = json.optInt("LogEntryOpcode").takeIf { it in 0..65535 }?.toUShort()
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

