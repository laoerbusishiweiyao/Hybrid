import { ApplicationContext } from "@runtime/ApplicationContext";
import { MessageObject, ResponseObject, WebMessageHandler, WebRequestHandler, type MessageType, type ResponseType } from "../IMessage";
import { BrowserInformationRequest, BrowserInformationResponse, UnityInitialized } from "../Message";

export class UnityInitializedHandler extends WebMessageHandler<UnityInitialized> {
    get messageType(): MessageType<MessageObject> {
        return UnityInitialized;
    }
    get responseType(): ResponseType<ResponseObject> | null {
        return null;
    }

    protected async runAsync(message: UnityInitialized): Promise<void> {
        console.log('receive unity initialized', message);
        ApplicationContext.messages.push('receive unity initialized')
    }
}

export class BrowserInformationRequestHandler extends WebRequestHandler<BrowserInformationRequest, BrowserInformationResponse> {
    get messageType(): MessageType<MessageObject> {
        return BrowserInformationRequest
    }
    get responseType(): ResponseType<ResponseObject> {
        return BrowserInformationResponse
    }
    protected async runAsync(_: BrowserInformationRequest, response: BrowserInformationResponse): Promise<void> {
        response.userAgent = navigator.userAgent;
    }
}