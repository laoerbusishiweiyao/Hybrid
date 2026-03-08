package com.chaosstudio.nativebridge

import android.webkit.JavascriptInterface
import com.unity3d.player.UnityPlayer
import org.json.JSONObject

class UnityBridge {
    companion object {
        fun debug(message: String) {
            log(LogLevel.Debug, message)
        }

        fun information(message: String) {
            log(LogLevel.Information, message)
        }

        private fun log(level: Int, message: String) {
            val document = JSONObject()

            val logEntry = JSONObject()
            logEntry.put("timestamp", System.currentTimeMillis())
            logEntry.put("level", level)
            logEntry.put("message", message)

            document.put("opcode", 10011)
            document.put("payload", logEntry)

            UnityPlayer.UnitySendMessage(UnityGameObjectName, UnityMethodName, document.toString())
        }
    }

    @JavascriptInterface
    fun send(message: String) {
        UnityPlayer.UnitySendMessage(UnityGameObjectName, UnityMethodName, message)
    }

    @JavascriptInterface
    fun debug(message: String) {
        log(LogLevel.Debug, message)
    }

    @JavascriptInterface
    fun information(message: String) {
        log(LogLevel.Information, message)
    }
}