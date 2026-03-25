declare interface CefSharp {
    bindObjectAsync(objectName: string): Promise<{ Count: number, Success: boolean, Message: string }>;
    postMessage(message: string): void;
}

declare interface WebView {
    postMessage(message: string): void;
}

declare interface Window {
    platform: 'windows' | 'android' | 'ios' | 'macos' | 'linux';

    bridge: NativeBridge;
    receive(message: string): void;

    cefSharp: CefSharp;
    webView: WebView;
}