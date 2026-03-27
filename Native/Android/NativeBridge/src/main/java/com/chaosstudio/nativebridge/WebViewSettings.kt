package com.chaosstudio.nativebridge

@Suppress("ConstPropertyName")
object WebViewSettings {
    /**
     * Unity 消息接收器物体名称
     */
    const val webViewMessageListener = "WebViewMessageListener"

    /**
     * Unity Web消息接收器物体的方法名称
     */
    const val onWebMessageReceiveMethod = "OnWebMessageReceive"

    /**
     * Unity 日志消息接收器物体的方法名称
     */
    const val onNativeLogReceiveMethod = "OnNativeLogReceive"

    /**
     * 挂载到 Web 端 window 上的实例名称
     */
    const val webBridgeName = "webView"

    /**
     * 网页入口地址(默认值为本地加载模式)
     */
    var address: String = "https://chaos.com"

    /**
     * 本地资源虚拟域名 (拦截域名) - 当请求 URL 的 host 匹配此域名时，请求将被拦截并从本地文件系统加载
     */
    var domain: String = "chaos.com"

    /**
     * 本地资源持久化文件夹名称(对应 Android: context.getExternalFilesDir({contentPath}))
     */
    var contentPath: String = "WebUI"
}

