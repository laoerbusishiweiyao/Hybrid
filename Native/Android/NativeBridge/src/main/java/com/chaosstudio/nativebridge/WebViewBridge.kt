package com.chaosstudio.nativebridge

import android.webkit.JavascriptInterface
import com.unity3d.player.UnityPlayer

class WebViewBridge {
    companion object {
        @JvmStatic
        fun log(message: String) {
            UnityPlayer.UnitySendMessage(
                WebViewSettings.webViewMessageListener,
                WebViewSettings.onNativeLogReceiveMethod,
                message
            )
        }
    }

    @JavascriptInterface
    fun postMessage(message: String) {
        UnityPlayer.UnitySendMessage(
            WebViewSettings.webViewMessageListener,
            WebViewSettings.onWebMessageReceiveMethod,
            message
        )
    }
}