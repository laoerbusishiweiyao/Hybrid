import { MessageObject, RequestObject, ResponseObject, type IWebMessage, type IWebRequest, type IWebResponse } from '../../Message/IMessage';

export class Wpf2UnityLoadedMessage extends MessageObject implements IWebMessage {
	processId!: number;
	constructor(processId: number) {
		super();
		this.processId = processId;
	}
}
export class Web2UnityLoadedMessage extends MessageObject implements IWebMessage {
}
export class Unity2WebLoadedMessage extends MessageObject implements IWebMessage {
}
export class Unity2WpfFocusChangedMessage extends MessageObject implements IWebMessage {
	hasFocus!: boolean;
	constructor(hasFocus: boolean) {
		super();
		this.hasFocus = hasFocus;
	}
}
export class Unity2WpfShutdownMessage extends MessageObject implements IWebMessage {
}
export class Web2UnityVersionRequest extends RequestObject implements IWebRequest {
}
export class Unity2WebVersionResponse extends ResponseObject implements IWebResponse {
	version!: string;
	unityVersion!: string;
	constructor(version: string, unityVersion: string) {
		super();
		this.version = version;
		this.unityVersion = unityVersion;
	}
}
export class Unity2WebUserAgentRequest extends RequestObject implements IWebRequest {
}
export class Web2UnityUserAgentResponse extends ResponseObject implements IWebResponse {
	userAgent!: string;
	constructor(userAgent: string) {
		super();
		this.userAgent = userAgent;
	}
}
export class Web2UnityTouchDataMessage extends MessageObject implements IWebMessage {
	id!: number;
	phase!: number;
	x!: number;
	y!: number;
	constructor(id: number, phase: number, x: number, y: number) {
		super();
		this.id = id;
		this.phase = phase;
		this.x = x;
		this.y = y;
	}
}
export class Web2UnityMouseDataMessage extends MessageObject implements IWebMessage {
	buttons!: number;
	x!: number;
	y!: number;
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
