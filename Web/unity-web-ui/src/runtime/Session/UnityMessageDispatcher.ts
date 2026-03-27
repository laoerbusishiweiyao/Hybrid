import { Opcode } from "@runtime/Opcode";
import { BrowserInformationRequestHandler, UnityInitializedHandler } from "./Handler/BrowserInformationRequestHandler";
import type { IWebMessageHandler } from "./IMessage";

class UnityMessageDispatcherType {
    private readonly allHandler = new Map<number, IWebMessageHandler[]>();

    constructor() {
        this.allHandler.set(Opcode.UnityInitialized, [new UnityInitializedHandler()]);
        this.allHandler.set(Opcode.BrowserInformationRequest, [new BrowserInformationRequestHandler()]);
    }

    handle(opcode: number, message: Object) {
        const handlers = this.allHandler.get(opcode);
        if (handlers === undefined) {
            console.warn(`消息 ${opcode} 无处理器`);
            return;
        }
        
        for (const handler of handlers) {
            try {
                handler.handle(message);
            } catch (error) {
                console.error(`消息 ${opcode} 处理失败`, error);
            }
        }
    }
}

export const UnityMessageDispatcher = new UnityMessageDispatcherType();