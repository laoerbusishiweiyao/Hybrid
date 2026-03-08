package com.chaosstudio.nativebridge

import android.content.Context
import android.util.Log
import androidx.fragment.app.FragmentActivity

object WebBridge {
    const val WEB_VIEW_TAG = "WebViewFragment"

    @JvmStatic
    fun load(context: Context, options: String) {
        Log.i("AndroidNativeBridge", options)
        NativeBridgeSettings.update(options)

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

    @JvmStatic
    fun send(context: Context, message: String) {
        if (context !is FragmentActivity) {
            return
        }

        val fragment = context.supportFragmentManager.findFragmentByTag(WEB_VIEW_TAG) as? TransparentWebViewFragment ?: return

        fragment.send(message)
    }
}

