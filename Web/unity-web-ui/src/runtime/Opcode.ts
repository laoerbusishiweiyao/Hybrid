export const Opcode = {
    Web: 1002,
    WebLoaded: 100201,
    WebTouchData: 100202,
    WebPointerData: 100203,
    WebMouseData: 100204,
} as const;

export type TouchPhase = typeof Opcode[keyof typeof Opcode];