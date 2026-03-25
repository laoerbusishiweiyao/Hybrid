export interface IMessage { }

export interface IRequest extends IMessage {
    requestId: number;
}

export interface IResponse extends IRequest {
    statusCode: number;
    message?: string;
}

export class MessageObject {
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