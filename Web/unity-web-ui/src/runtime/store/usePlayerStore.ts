import { reactive, readonly } from "vue";

interface Player {
    id: number;
    name: string;
}

const player = reactive<Player>({ id: 0, name: '' });

export const usePlayerStore = () => ({
    player: readonly(player),
});