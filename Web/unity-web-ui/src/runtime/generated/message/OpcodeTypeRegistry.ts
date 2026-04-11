import { type MessageType, type RequestType, type ResponseType } from '../../Message/IMessage';
import { Wpf2UnityLoadedMessage, Web2UnityLoadedMessage, Unity2WebLoadedMessage, Unity2WpfFocusChangedMessage, Unity2WpfShutdownMessage, Web2UnityVersionRequest, Unity2WebVersionResponse, Unity2WebUserAgentRequest, Web2UnityUserAgentResponse, Web2UnityTouchDataMessage, Web2UnityMouseDataMessage } from './Message';

class OpcodeTypeRegistryClass {
    private readonly opcodeTypeMap = new Map<number, MessageType>();
    private readonly typeOpcodeMap = new Map<MessageType, number>();
    private readonly requestResponseMap = new Map<RequestType, ResponseType>();

    constructor() {
		this.opcodeTypeMap.set(10001, Wpf2UnityLoadedMessage);
		this.typeOpcodeMap.set(Wpf2UnityLoadedMessage, 10001);
		this.opcodeTypeMap.set(10002, Web2UnityLoadedMessage);
		this.typeOpcodeMap.set(Web2UnityLoadedMessage, 10002);
		this.opcodeTypeMap.set(10003, Unity2WebLoadedMessage);
		this.typeOpcodeMap.set(Unity2WebLoadedMessage, 10003);
		this.opcodeTypeMap.set(10004, Unity2WpfFocusChangedMessage);
		this.typeOpcodeMap.set(Unity2WpfFocusChangedMessage, 10004);
		this.opcodeTypeMap.set(10005, Unity2WpfShutdownMessage);
		this.typeOpcodeMap.set(Unity2WpfShutdownMessage, 10005);
		this.opcodeTypeMap.set(10006, Web2UnityVersionRequest);
		this.typeOpcodeMap.set(Web2UnityVersionRequest, 10006);
		this.opcodeTypeMap.set(10007, Unity2WebVersionResponse);
		this.typeOpcodeMap.set(Unity2WebVersionResponse, 10007);
		this.opcodeTypeMap.set(10008, Unity2WebUserAgentRequest);
		this.typeOpcodeMap.set(Unity2WebUserAgentRequest, 10008);
		this.opcodeTypeMap.set(10009, Web2UnityUserAgentResponse);
		this.typeOpcodeMap.set(Web2UnityUserAgentResponse, 10009);
		this.opcodeTypeMap.set(10010, Web2UnityTouchDataMessage);
		this.typeOpcodeMap.set(Web2UnityTouchDataMessage, 10010);
		this.opcodeTypeMap.set(10011, Web2UnityMouseDataMessage);
		this.typeOpcodeMap.set(Web2UnityMouseDataMessage, 10011);
		this.requestResponseMap.set(Web2UnityVersionRequest, Unity2WebVersionResponse);
		this.requestResponseMap.set(Unity2WebUserAgentRequest, Web2UnityUserAgentResponse);
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

export const OpcodeTypeRegistry = new OpcodeTypeRegistryClass();
