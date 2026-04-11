export interface IMessage { }

export interface IRequest extends IMessage {
    requestId: number;
}

export interface IResponse extends IRequest {
    statusCode: number;
    message?: string;
}

export interface IWebMessage extends IMessage { }
export interface IWebRequest extends IWebMessage, IRequest { }
export interface IWebResponse extends IWebMessage, IResponse { }

export interface ISessionMessage extends IMessage { }
export interface ISessionRequest extends ISessionMessage, IRequest { }
export interface ISessionResponse extends ISessionMessage, IResponse { }

export class MessageObject implements IMessage {
}

export abstract class RequestObject extends MessageObject implements IRequest {
    requestId: number = -1;
}

export class ResponseObject extends RequestObject implements IResponse {
    statusCode: number = 0;
    message?: string;

    constructor(statusCode: number = 0, message?: string) {
        super();
        this.statusCode = statusCode;
        this.message = message;
    }
}

export type MessageType<T extends MessageObject = MessageObject> = new (...args: any[]) => T;
export type RequestType<T extends RequestObject = RequestObject> = new (...args: any[]) => T;
export type ResponseType<T extends ResponseObject = ResponseObject> = new (...args: any[]) => T;

export interface IWebMessageHandler {
    get messageType(): MessageType;
    get responseType(): ResponseType | null;
    handle(message: Object): void;
}

export abstract class WebMessageHandler<TMessage extends MessageObject = MessageObject> implements IWebMessageHandler {
    abstract get messageType(): MessageType;
    abstract get responseType(): ResponseType | null;

    handle(message: Object): void {
        this.handleAsync(message as TMessage);
    }

    private async handleAsync(message: TMessage): Promise<void> {
        await this.runAsync(message);
    }

    protected abstract runAsync(message: TMessage): Promise<void>;
}

export class WebSessionException extends Error {
    statusCode: number = 0;

    constructor(statusCode: number = 0, message?: string) {
        super(message);
        this.statusCode = statusCode;
    }
}

export abstract class WebRequestHandler<TRequestType extends RequestObject = RequestObject, TResponseType extends ResponseObject = ResponseObject> implements IWebMessageHandler {
    abstract get messageType(): MessageType;
    abstract get responseType(): ResponseType;

    handle(message: Object): void {
        this.handleAsync(message);
    }

    private async handleAsync(message: Object): Promise<void> {
        try {
            if (!(message instanceof RequestObject)) {
                throw new Error("Message is not a request object");
            }

            const request = message as TRequestType;
            const requestId = request.requestId;
            const response = new this.responseType() as TResponseType;
            try {
                await this.runAsync(request, response);
            }
            catch (error) {
                if (error instanceof WebSessionException) {
                    console.error(error);
                    response.statusCode = error.statusCode;
                } else {
                    console.error(error);
                    response.statusCode = -1;
                }
            }

            response.requestId = requestId;
            window.send(response);
        } catch (error) {
            console.error(error);
        }
    }

    protected abstract runAsync(request: TRequestType, response: TResponseType): Promise<void>;
}