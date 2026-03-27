import type { MessageType, RequestType, ResponseType } from "./IMessage";
import { BrowserInformationRequest, BrowserInformationResponse, UnityInformationRequest, UnityInformationResponse, UnityInitialized, WebLoaded, WebMouseData, WebTouchData } from "./Message";

class OpcodeRegistryType {
    private readonly opcodeTypeMap = new Map<number, MessageType>();
    private readonly typeOpcodeMap = new Map<MessageType, number>();
    private readonly requestResponseMap = new Map<RequestType, ResponseType>();

    constructor() {
        this.opcodeTypeMap.set(10202, WebTouchData);
        this.typeOpcodeMap.set(WebTouchData, 10202);

        this.opcodeTypeMap.set(10204, WebMouseData);
        this.typeOpcodeMap.set(WebMouseData, 10204);

        this.opcodeTypeMap.set(10201, WebLoaded);
        this.typeOpcodeMap.set(WebLoaded, 10201);

        this.opcodeTypeMap.set(10301, UnityInformationRequest);
        this.typeOpcodeMap.set(UnityInformationRequest, 10301);

        this.opcodeTypeMap.set(10302, UnityInformationResponse);
        this.typeOpcodeMap.set(UnityInformationResponse, 10302);

        this.requestResponseMap.set(UnityInformationRequest, UnityInformationResponse);

        this.opcodeTypeMap.set(10401, UnityInitialized);
        this.typeOpcodeMap.set(UnityInitialized, 10401);

        this.opcodeTypeMap.set(10402, BrowserInformationRequest);
        this.typeOpcodeMap.set(BrowserInformationRequest, 10402);

        this.opcodeTypeMap.set(10403, BrowserInformationResponse);
        this.typeOpcodeMap.set(BrowserInformationResponse, 10403);

        this.requestResponseMap.set(BrowserInformationRequest, BrowserInformationResponse);
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

export const OpcodeRegistry = new OpcodeRegistryType();