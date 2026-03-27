package com.chaosstudio.nativebridge

import android.annotation.SuppressLint
import android.graphics.Color
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.webkit.WebSettings
import android.webkit.WebView
import android.widget.FrameLayout
import androidx.fragment.app.Fragment

class TransparentWebViewFragment : Fragment() {
    private var webView: WebView? = null

    private var isPlatformReady = false

    @SuppressLint("ClickableViewAccessibility", "SetJavaScriptEnabled")
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View {
        val layout = FrameLayout(requireContext()).apply {
            setBackgroundColor(Color.TRANSPARENT)
        }

        webView = WebView(requireContext()).apply {
            background = null
            setBackgroundColor(Color.TRANSPARENT)

            isFocusable = true
            isFocusableInTouchMode = true
            isClickable = true
            isEnabled = true

            settings.apply {
                javaScriptEnabled = true
                domStorageEnabled = true
                useWideViewPort = true
                loadWithOverviewMode = false

                setSupportZoom(false)
                builtInZoomControls = false
                displayZoomControls = false
                textZoom = 100
                setInitialScale(100)

                mediaPlaybackRequiresUserGesture = false
                allowFileAccess = false
                allowContentAccess = false
                mixedContentMode = WebSettings.MIXED_CONTENT_ALWAYS_ALLOW

                cacheMode = WebSettings.LOAD_DEFAULT
                layoutAlgorithm = WebSettings.LayoutAlgorithm.NORMAL
            }

            webViewClient = object : StandaloneWebViewClient(requireContext()) {
                override fun onPageFinished(view: WebView?, url: String?) {
                    super.onPageFinished(view, url)

                    if (!isPlatformReady) {
                        view?.post {
                            view.evaluateJavascript("window.dispatchEvent(new CustomEvent('platformReady', { detail: { platform: 'android' } }));", null)
                        }
                        isPlatformReady = true
                    }
                }
            }

            addJavascriptInterface(WebViewBridge(), WebViewSettings.webBridgeName)

            loadUrl(WebViewSettings.address)
        }

        layout.addView(
            webView, FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.MATCH_PARENT
            )
        )

        return layout
    }

    override fun onDestroyView() {
        webView?.apply {
            loadDataWithBaseURL(null, "", "text/html", "utf-8", null)
            clearHistory()
            destroy()
        }
        webView = null

        super.onDestroyView()
    }

    /*
    * 发送消息到 Web
    * */
    fun send(opcode: UShort, message: String) {
        webView?.post {
            webView?.evaluateJavascript("window.receive($opcode, $message)", null)
        }
    }
}

