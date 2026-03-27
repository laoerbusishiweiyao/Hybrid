package com.chaosstudio.nativebridge

import android.content.Context
import android.util.Log
import androidx.fragment.app.FragmentActivity

/*
* WebView 组件
* */
object WebViewComponent {
    const val WEB_VIEW_TAG = "WebViewFragment"

    @JvmStatic
    fun load(context: Context, address: String, domain: String, contentPath: String) {
        WebViewSettings.address = address;
        WebViewSettings.domain = domain;
        WebViewSettings.contentPath = contentPath;

        unload(context)

        if (context is FragmentActivity) {
            context.runOnUiThread {
                val fragment = TransparentWebViewFragment()
                context.supportFragmentManager.beginTransaction()
                    .add(android.R.id.content, fragment, WEB_VIEW_TAG)
                    .commit()
            }
        }
    }

    @JvmStatic
    fun unload(context: Context) {
        if (context is FragmentActivity) {
            context.runOnUiThread {
                val fragment = context.supportFragmentManager.findFragmentByTag(WEB_VIEW_TAG)
                if (fragment != null) {
                    context.supportFragmentManager.beginTransaction()
                        .remove(fragment)
                        .commit()
                }
            }
        }
    }

    /*
    * Unity 发送消息到 Web
    * */
    @JvmStatic
    fun send(context: Context, opcode: Int, content: String) {
        if (context !is FragmentActivity) {
            return
        }

        val fragment = context.supportFragmentManager.findFragmentByTag(WEB_VIEW_TAG) as? TransparentWebViewFragment ?: return
        fragment.send(opcode.toUShort(), content)
    }
}
