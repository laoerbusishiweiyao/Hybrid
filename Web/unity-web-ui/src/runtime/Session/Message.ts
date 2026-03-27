import { MessageObject, RequestObject, ResponseObject, type IWebRequest, type IWebResponse } from "./IMessage";

export const TouchPhase = {
    None: 0,
    Began: 1,
    Moved: 2,
    Ended: 3,
    Canceled: 4,
    Stationary: 5,
} as const;

type TouchPhase = typeof TouchPhase[keyof typeof TouchPhase];

export class WebTouchData extends MessageObject {
    readonly id: number;
    readonly phase: TouchPhase;
    readonly x: number;
    readonly y: number;

    constructor(id: number, phase: TouchPhase, x: number, y: number) {
        super();
        this.id = id;
        this.phase = phase;
        this.x = x;
        this.y = y;
    }
}

export class WebMouseData extends MessageObject {
    buttons: number;
    x: number;
    y: number;
    deltaX?: number;
    deltaY?: number;

    constructor(buttons: number, x: number, y: number, deltaX?: number, deltaY?: number) {
        super();
        this.buttons = buttons;
        this.x = x;
        this.y = y;
        this.deltaX = deltaX;
        this.deltaY = deltaY;
    }
}

export class WebLoaded extends MessageObject {
}

export class UnityInformationRequest extends RequestObject implements IWebRequest {
}

export class UnityInformationResponse extends ResponseObject implements IWebResponse {
    version!: string;
    unityVersion!: string;
}

export class UnityInitialized extends MessageObject {
}

export class BrowserInformationRequest extends RequestObject implements IWebRequest {
}

export class BrowserInformationResponse extends ResponseObject implements IWebResponse {
    userAgent!: string;
}