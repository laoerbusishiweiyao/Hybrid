import { Opcode } from "./Opcode";
import { UnityBridge } from "./UnityBridge";

export const TouchPhase = {
    None: 0,
    Began: 1,
    Moved: 2,
    Ended: 3,
    Canceled: 4,
    Stationary: 5,
} as const;

export type TouchPhase = typeof TouchPhase[keyof typeof TouchPhase];

export interface WebTouchData {
    readonly id: number;
    readonly phase: TouchPhase;
    readonly x: number;
    readonly y: number;
}

export class WebPointerData {
    id: number;
    phase: TouchPhase;
    positionX: number;
    positionY: number;
    type: string;

    constructor(id: number, positionX: number, positionY: number, phase: TouchPhase, type: string) {
        this.id = id;
        this.positionX = positionX;
        this.positionY = positionY;
        this.phase = phase;
        this.type = type;
    }
}

export class WebMouseData {
    id: number;
    phase: TouchPhase;
    positionX: number;
    positionY: number;

    constructor(id: number, positionX: number, positionY: number, phase: TouchPhase) {
        this.id = id;
        this.positionX = positionX;
        this.positionY = positionY;
        this.phase = phase;
    }
}

class InputSystemType {
    private readonly devicePixelRatio: number;

    constructor() {
        this.devicePixelRatio = window.devicePixelRatio || 1;

        this.addTouchEventListener();
    }

    private addTouchEventListener() {
        document.removeEventListener('touchstart', this.onTouchStart);
        document.addEventListener('touchstart', this.onTouchStart);

        document.removeEventListener('touchmove', this.onTouchMove);
        document.addEventListener('touchmove', this.onTouchMove);

        document.removeEventListener('touchend', this.onTouchEnd);
        document.addEventListener('touchend', this.onTouchEnd);

        document.removeEventListener('touchcancel', this.onTouchCancel);
        document.addEventListener('touchcancel', this.onTouchCancel);
    }

    private onTouchStart = (event: TouchEvent) => {
        for (const touch of event.changedTouches) {
            this.processTouchEvent(touch, TouchPhase.Began, event);
        }
    }

    private onTouchMove = (event: TouchEvent) => {
        for (const touch of event.changedTouches) {
            this.processTouchEvent(touch, TouchPhase.Moved, event);
        }
    }

    private onTouchEnd = (event: TouchEvent) => {
        for (const touch of event.changedTouches) {
            this.processTouchEvent(touch, TouchPhase.Ended, event);
        }
    }

    private onTouchCancel = (event: TouchEvent) => {
        for (const touch of event.changedTouches) {
            this.processTouchEvent(touch, TouchPhase.Canceled, event);
        }
    }

    private processTouchEvent(touch: Touch, phase: TouchPhase, _: TouchEvent): void {
        const { identifier, clientX, clientY } = touch;

        if (this.isWebUIElement(clientX, clientY)) {
            return;
        }

        const { x, y } = this.convertToUnityCoordinates(clientX, clientY);
        UnityBridge.post(Opcode.WebTouchData, { id: identifier, phase, x, y });
    }

    private isWebUIElement(clientX: number, clientY: number): boolean {
        let element = document.elementFromPoint(clientX, clientY);
        if (element?.nodeType == Node.TEXT_NODE) {
            element = element.parentElement;
        }

        if (!element) {
            return false;
        }

        return element.closest('[data-web-ui]') !== null;
    }

    private convertToUnityCoordinates(clientX: number, clientY: number): { x: number, y: number } {
        const height = window.visualViewport?.height ?? window.innerHeight;
        const physicalHeight = height * this.devicePixelRatio;

        return {
            x: clientX * this.devicePixelRatio,
            y: physicalHeight - clientY * this.devicePixelRatio,
        };
    }
}

export const InputSystem = new InputSystemType();