export const Opcode = {
	Wpf2UnityLoadedMessage: 10001,
	Web2UnityLoadedMessage: 10002,
	Unity2WebLoadedMessage: 10003,
	Unity2WpfFocusChangedMessage: 10004,
	Unity2WpfShutdownMessage: 10005,
	Web2UnityVersionRequest: 10006,
	Unity2WebVersionResponse: 10007,
	Unity2WebUserAgentRequest: 10008,
	Web2UnityUserAgentResponse: 10009,
	Web2UnityTouchDataMessage: 10010,
	Web2UnityMouseDataMessage: 10011,
} as const;
