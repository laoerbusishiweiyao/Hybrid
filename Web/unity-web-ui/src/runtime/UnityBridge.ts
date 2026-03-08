import type { Opcode } from "./Opcode";

interface IMessage { }

interface IRequest extends IMessage {
    rpcId: number;
}

interface IResponse extends IRequest {
    rpcId: number;
    statusCode: number;
    message: string;
}

interface WebMessageInfo {
    opcode: typeof Opcode[keyof typeof Opcode];
    payload: Object;
}

class UnityBridgeType {
    rpcIdCounter: number = 0;

    private callbacks: Map<number, (value: IResponse | PromiseLike<IResponse>) => void> = new Map();

    constructor() {
        window.receive = this.receive.bind(this);
    }

    post<TMessage extends Object>(opcode: typeof Opcode[keyof typeof Opcode], payload: TMessage) {
        window.bridge.send(JSON.stringify({ opcode, payload }));
    }

    postAsync<TRequest extends Object & Partial<IRequest>, TResponse extends IResponse>(opcode: typeof Opcode[keyof typeof Opcode], payload: TRequest): Promise<TResponse> {
        payload.rpcId = ++this.rpcIdCounter;
        const promise = new Promise<TResponse>(resolve => {
            this.callbacks.set(payload.rpcId!, resolve as (value: IResponse | PromiseLike<IResponse>) => void);
        });
        window.bridge.send(JSON.stringify({ opcode, payload }));
        return promise;
    }

    private receive(message: string) {
        const messageInfo: WebMessageInfo = JSON.parse(message);
        // TODO: deserialize payload based on opcode
        const response = messageInfo.payload as IResponse;
        const callback = this.callbacks.get(response.rpcId);
        if (callback) {
            callback(response);
            this.callbacks.delete(response.rpcId);
        }
    }
}

export const UnityBridge = new UnityBridgeType();