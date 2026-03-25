import type { MessageType, RequestType, ResponseType } from "./IMessage";
import { WebMouseData, WebTouchData } from "./Message";



class OpcodeContextType {
    private readonly opcodeTypeMap = new Map<number, MessageType>();
    private readonly typeOpcodeMap = new Map<MessageType, number>();
    private readonly requestResponseMap = new Map<RequestType, ResponseType>();

    constructor() {
        this.opcodeTypeMap.set(10202, WebTouchData);
        this.typeOpcodeMap.set(WebTouchData, 10202);

        this.opcodeTypeMap.set(10204, WebMouseData);
        this.typeOpcodeMap.set(WebMouseData, 10204);
    }

    findOpcode(type: MessageType) {
        return this.typeOpcodeMap.get(type);
    }

    findType(opcode: number) {
        return this.opcodeTypeMap.get(opcode);
    }

    findResponseType(requestType: RequestType) {
        return this.requestResponseMap.get(requestType);
    }
}

export const OpcodeContext = new OpcodeContextType();