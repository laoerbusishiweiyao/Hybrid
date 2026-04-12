import { reactive, readonly } from "vue";

interface ApplicationInformation {
    loaded: boolean;
    version: string;
}

const information = reactive<ApplicationInformation>({ loaded: false, version: '1.0.0' });

const update = (info: ApplicationInformation) => {
    information.version = info.version;
}

const setLoaded = () => {
    information.loaded = true;
}

export const useApplicationStore = () => ({
    information: readonly(information),
    update,
    setLoaded,
});