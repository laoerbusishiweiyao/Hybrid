declare interface UnityBridge {
    send(message: string): void;
    debug(message: string): void;
    information(message: string): void;
}

declare interface Window {
    bridge: UnityBridge;
    receive(message: string): void;
}