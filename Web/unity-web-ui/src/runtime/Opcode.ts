export const Opcode = {
    Web: 102,
    WebLoaded: 10201,
    WebTouchData: 10202,
    WebPointerData: 10203,
    WebMouseData: 10204,
} as const;

export type TouchPhase = typeof Opcode[keyof typeof Opcode];