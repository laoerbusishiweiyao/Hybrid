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
import android.webkit.WebSettings
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

        val options = NativeBridgeSettings.webViewOptions

        webView = WebView(requireContext()).apply {
            background = null
            setBackgroundColor(Color.TRANSPARENT)
            isFocusable = options.focusable
            isFocusableInTouchMode = options.focusableInTouchMode
            isClickable = options.clickable
            isEnabled = options.enabled

            settings.apply {
                javaScriptEnabled = options.javaScriptEnabled
                domStorageEnabled = options.domStorageEnabled
                useWideViewPort = options.useWideViewPort
                loadWithOverviewMode = options.loadWithOverviewMode
                setSupportZoom(options.supportZoom)
                builtInZoomControls = options.builtInZoomControls
                displayZoomControls = options.displayZoomControls
                textZoom = options.textZoom
                if (options.initialScale > 0) {
                    setInitialScale(options.initialScale)
                }
                mediaPlaybackRequiresUserGesture = options.mediaPlaybackRequiresUserGesture
                allowFileAccess = options.allowFileAccess
                allowContentAccess = options.allowContentAccess
                cacheMode = options.cacheMode
                mixedContentMode = options.mixedContentMode

                options.layoutAlgorithm.let { algo ->
                    layoutAlgorithm = when (algo.uppercase()) {
                        "SINGLE_COLUMN" -> WebSettings.LayoutAlgorithm.SINGLE_COLUMN
                        "NARROW_COLUMNS" -> WebSettings.LayoutAlgorithm.NARROW_COLUMNS
                        "TEXT_AUTOSIZING" -> WebSettings.LayoutAlgorithm.TEXT_AUTOSIZING
                        else -> WebSettings.LayoutAlgorithm.NORMAL
                    }
                }
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

            loadUrl(NativeBridgeSettings.webViewOptions.entryUrl)
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

