import { Opcode } from "@runtime/generated/message/Opcode";
import { UnityInitializedHandler, BrowserInformationRequestHandler } from "../Session/Handler/BrowserInformationRequestHandler";
import type { IWebMessageHandler } from "./IMessage";

class UnityMessageDispatcherType {
    private readonly allHandler = new Map<number, IWebMessageHandler[]>();

    constructor() {
		this.allHandler.set(Opcode.Unity2WebLoadedMessage, [new UnityInitializedHandler()]);
		this.allHandler.set(Opcode.Unity2WebUserAgentRequest, [new BrowserInformationRequestHandler()]);
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
