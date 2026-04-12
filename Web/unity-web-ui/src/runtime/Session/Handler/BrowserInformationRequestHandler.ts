import { Unity2WebLoadedMessage, Unity2WebUserAgentRequest, Web2UnityUserAgentResponse } from "@runtime/generated/message/Message";
import { WebMessageHandler, type MessageType, type ResponseType, MessageObject, ResponseObject, WebRequestHandler } from "@runtime/Message/IMessage";
import { useApplicationStore } from "@runtime/store/useApplicationStore";

export class UnityInitializedHandler extends WebMessageHandler<Unity2WebLoadedMessage> {
    get messageType(): MessageType<MessageObject> {
        return Unity2WebLoadedMessage;
    }
    get responseType(): ResponseType<ResponseObject> | null {
        return null;
    }

    protected async runAsync(_: Unity2WebLoadedMessage): Promise<void> {
        console.log('unity loaded');
        const { setLoaded } = useApplicationStore();
        setLoaded();
    }
}

export class BrowserInformationRequestHandler extends WebRequestHandler<Unity2WebUserAgentRequest, Web2UnityUserAgentResponse> {
    get messageType(): MessageType<MessageObject> {
        return Unity2WebUserAgentRequest
    }
    get responseType(): ResponseType<ResponseObject> {
        return Web2UnityUserAgentResponse
    }
    protected async runAsync(_: Unity2WebUserAgentRequest, response: Web2UnityUserAgentResponse): Promise<void> {
        response.userAgent = navigator.userAgent;
    }
}
