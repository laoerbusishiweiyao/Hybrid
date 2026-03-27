export const Opcode = {
    Web: 102,
    WebLoaded: 10201,
    WebTouchData: 10202,
    WebPointerData: 10203,
    WebMouseData: 10204,

    UnityInformationRequest: 10301,
    UnityInformationResponse: 10302,

    UnityInitialized: 10401,
    BrowserInformationRequest: 10402,
    BrowserInformationResponse: 10403,
} as const;

export type TouchPhase = typeof Opcode[keyof typeof Opcode];