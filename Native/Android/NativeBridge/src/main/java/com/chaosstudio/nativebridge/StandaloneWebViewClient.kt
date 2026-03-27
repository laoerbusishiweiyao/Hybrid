package com.chaosstudio.nativebridge

import android.content.Context
import android.webkit.WebResourceRequest
import android.webkit.WebResourceResponse
import android.webkit.WebView
import android.webkit.WebViewClient
import java.io.ByteArrayInputStream
import java.io.File
import java.io.FileInputStream
import java.net.URLConnection

open class StandaloneWebViewClient(private val context: Context) : WebViewClient() {
    override fun shouldInterceptRequest(view: WebView?, request: WebResourceRequest?): WebResourceResponse? {
        val url = request?.url ?: return super.shouldInterceptRequest(view, request)

        if (url.host == WebViewSettings.domain) {
            val path = url.path?.takeIf { it.isNotEmpty() } ?: "/"
            return loadFromPersistentDataPath(path, WebViewSettings.contentPath)
        }

        return super.shouldInterceptRequest(view, request)
    }

    private fun loadFromPersistentDataPath(url: String, assetFolderName: String): WebResourceResponse {
        val mimeType = if (url == "/") "text/html" else URLConnection.guessContentTypeFromName(url)
        val path = if (url == "/") "index.html" else url
        val file = File(context.getExternalFilesDir(assetFolderName), path)

        return try {
            return WebResourceResponse(mimeType, "UTF-8", FileInputStream(file))
        } catch (exception: Exception) {
            WebResourceResponse("text/plain", "UTF-8", ByteArrayInputStream("Asset not found: $path $exception".toByteArray())).apply {
                setStatusCodeAndReasonPhrase(404, "Not Found")
            }
        }
    }
}