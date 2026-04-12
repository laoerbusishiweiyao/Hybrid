import { createI18n } from 'vue-i18n';
import messages from '@intlify/unplugin-vue-i18n/messages';
import { readonly } from "vue";

const i18n = createI18n({
    locale: window.localStorage.getItem('language') || 'zh',
    messages
})

const switchLanguage = (language: string) => {
    i18n.global.locale = language;
    window.localStorage.setItem('language', language);
    console.log('switch language', language);
}

const translate = (key: string) => {
    return i18n.global.t(key);
}

export const useI18nStore = () => ({
    i18n: readonly(i18n),
    availableLocales: readonly(i18n.global.availableLocales),
    switchLanguage,
    translate,
});