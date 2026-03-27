import { RequestObject, type MessageObject, ResponseObject, type MessageType, type RequestType } from "./IMessage";
import { WebLoaded } from "./Message";
import { OpcodeRegistry } from "./OpcodeRegistry";
import { StatusCode } from "./StatusCode";
import { UnityMessageDispatcher } from "./UnityMessageDispatcher";

interface PlatformReadyEventDetail {
    platform: 'windows' | 'android' | 'ios' | 'macos' | 'linux';
}

type RequestCallback = (value: ResponseObject) => void;

class UnitySessionType {
    private requestIdCounter: number = 0;

    private callbacks: Map<number, RequestCallback> = new Map();

    private transmit?: (content: string) => void;

    constructor() {
        window.removeEventListener('platformReady', this.onPlatformReady);
        window.addEventListener('platformReady', this.onPlatformReady);
    }

    private onPlatformReady = (event: Event) => {
        const customEvent = event as CustomEvent<PlatformReadyEventDetail>;
        const { platform } = customEvent.detail;

        window.receive = this.receive;

        switch (platform) {
            case 'windows':
                this.transmit = window.cefSharp.postMessage;
                break;
            case 'android':
                this.transmit = window.webView.postMessage.bind(window.webView);
                break;
            default:
                console.error(`Unknown platform: ${platform}`);
                break;
        }

        console.log(`Platform is ready: ${platform}`);
        this.send(new WebLoaded());
    }

    send(message: MessageObject): void {
        const type = message.constructor as MessageType;
        const opcode = OpcodeRegistry.findOpcode(type);

        if (opcode === undefined) {
            // 处理错误：发送了一个未注册的消息类型
            console.error(`Unknown message type: ${type.name}`);
            return;
        }
        
        this.transmit?.(JSON.stringify({ opcode, payload: message }));
    }

    sendAsync<TResponse extends ResponseObject>(message: RequestObject): Promise<TResponse> {
        const type = message.constructor as RequestType;
        const opcode = OpcodeRegistry.findOpcode(type);

        if (opcode === undefined) {
            console.error(`Unknown message type: ${type.name}`);
            throw new Error(`Unknown message type: ${type.name}`);
        }

        message.requestId = ++this.requestIdCounter;

        return new Promise<TResponse>(resolve => {
            // 注册回调
            this.callbacks.set(message.requestId, resolve as RequestCallback);

            // 设置 5s 超时
            setTimeout(() => {
                if (this.callbacks.has(message.requestId)) {
                    resolve(new ResponseObject(StatusCode.Timeout, 'Request timeout') as TResponse);
                    this.callbacks.delete(message.requestId);
                }
            }, 5000);

            // 发送
            this.transmit?.(JSON.stringify({ opcode, payload: message }));
        });
    }

    private receive = (opcode: number, content: string) => {
        const type = OpcodeRegistry.findType(opcode);
        if (type === undefined) {
            console.error(`Unknown message type: ${opcode}`);
            return;
        }

        const message = Object.assign(new type(), content) as MessageObject;

        // 响应
        if (message instanceof ResponseObject) {
            const callback = this.callbacks.get(message.requestId);
            if (callback) {
                callback(message);
                this.callbacks.delete(message.requestId);
            }
        }
        // 请求
        else if (message instanceof RequestObject) {
            // 分发处理请求
            UnityMessageDispatcher.handle(opcode, message);
        }
        // 消息
        else {
            // 分发处理消息
            UnityMessageDispatcher.handle(opcode, message);
        }
    }
}

export const UnitySession = new UnitySessionType();

