package com.chaosstudio.nativebridge

import android.annotation.SuppressLint
import android.graphics.Bitmap
import android.graphics.Color
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebView
import android.widget.FrameLayout
import androidx.fragment.app.Fragment

class TransparentWebViewFragment : Fragment() {
    private var webView: WebView? = null

    @SuppressLint("ClickableViewAccessibility", "SetJavaScriptEnabled")
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View {
        UnityBridge.debug("TransparentWebViewFragment Creating")

        val layout = FrameLayout(requireContext()).apply {
            setBackgroundColor(Color.TRANSPARENT)
        }

        webView = WebView(requireContext()).apply {
            background = null
            setBackgroundColor(Color.TRANSPARENT)
            isFocusable = false
            isFocusableInTouchMode = false
            isClickable = true
            isEnabled = true

            settings.apply {
                javaScriptEnabled = true
                domStorageEnabled = true
                useWideViewPort = true
                loadWithOverviewMode = true
                setSupportZoom(false)
                builtInZoomControls = false
                displayZoomControls = false
            }

            webViewClient = object : StandaloneWebViewClient(requireContext()) {
                override fun onPageStarted(view: WebView?, url: String?, favicon: Bitmap?) {
                    super.onPageStarted(view, url, favicon)
                    UnityBridge.debug("onPageStarted: $url")
                }

                override fun onPageFinished(view: WebView?, url: String?) {
                    super.onPageFinished(view, url)
                    UnityBridge.debug("onPageFinished: $url")
                }

                override fun onLoadResource(view: WebView?, url: String?) {
                    super.onLoadResource(view, url)
                    UnityBridge.debug("onLoadResource: $url")
                }

                override fun onReceivedError(view: WebView?, request: WebResourceRequest?, error: WebResourceError?) {
                    super.onReceivedError(view, request, error)
                    UnityBridge.debug("onReceivedError: ${error?.errorCode}, ${error?.description}")
                }
            }

            addJavascriptInterface(UnityBridge(), WebBridgeName)

            loadUrl("https://chaos.com")
        }

        layout.addView(
            webView, FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.MATCH_PARENT
            )
        )

        return layout
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
    }

    override fun onDestroyView() {
        UnityBridge.information("TransparentWebViewFragment Destroy")

        webView?.apply {
            loadDataWithBaseURL(null, "", "text/html", "utf-8", null)
            clearHistory()
            destroy()
        }
        webView = null

        super.onDestroyView()
    }

    fun send(message: String) {
        webView?.post {
            webView?.evaluateJavascript("window.receive(`$message`)", null)
        }
    }
}

