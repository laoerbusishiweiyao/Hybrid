import { UnitySession } from "./Session/UnitySession";
import { WebTouchData, WebMouseData, TouchPhase } from "./Session/Message";

class InputSystemType {
    private readonly devicePixelRatio: number;

    constructor() {
        this.devicePixelRatio = window.devicePixelRatio || 1;

        this.addTouchEventListener();

        this.addPointerEventListener();
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

        console.log('Touch event listeners added.');
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

    private processTouchEvent(touch: Touch, phase: typeof TouchPhase[keyof typeof TouchPhase], _: TouchEvent): void {
        const { identifier, clientX, clientY } = touch;
        if (this.isWebUIElement(clientX, clientY)) {
            return;
        }

        const { x, y } = this.convertToUnityCoordinates(clientX, clientY);
        UnitySession.send(new WebTouchData(identifier, phase, x, y));
    }

    private addPointerEventListener() {
        document.removeEventListener('mousedown', this.onMouseDown);
        document.addEventListener('mousedown', this.onMouseDown);

        document.removeEventListener('mousemove', this.onMouseMove);
        document.addEventListener('mousemove', this.onMouseMove);

        document.removeEventListener('mouseup', this.onMouseUp);
        document.addEventListener('mouseup', this.onMouseUp);

        document.removeEventListener('wheel', this.onMouseWheel);
        document.addEventListener('wheel', this.onMouseWheel);

        console.log('Mouse event listeners added.');
    }

    private onMouseDown = (event: MouseEvent) => {
        this.processMouseEvent(event);
    }

    private onMouseMove = (event: MouseEvent) => {
        this.processMouseEvent(event);
    }

    private onMouseUp = (event: MouseEvent) => {
        this.processMouseEvent(event);
    }

    private onMouseWheel = (event: WheelEvent) => {
        this.processMouseEvent(event);
    }

    private processMouseEvent(event: MouseEvent): void {
        const { buttons, clientX, clientY } = event;
        if (this.isWebUIElement(clientX, clientY)) {
            return;
        }

        const { x, y } = this.convertToUnityCoordinates(clientX, clientY);

        if (event instanceof WheelEvent) {
            UnitySession.send(new WebMouseData(buttons, x, y, event.deltaX, event.deltaY));
        } else {
            UnitySession.send(new WebMouseData(buttons, x, y));
        }
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