import { reactive } from "vue";

class ApplicationContextType {
    messages: Array<string> = [];
}

export const ApplicationContext = reactive(new ApplicationContextType());