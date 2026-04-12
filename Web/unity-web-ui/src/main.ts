import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { RouterContext } from '@runtime/RouterContext'
import '@runtime/InputSystem'
import '@runtime/Message/UnitySession'
import { useI18nStore } from '@runtime/store/useI18nStore'

window.addEventListener('DOMContentLoaded', OnDOMContentLoaded);
window.addEventListener('load', OnWindowLoad);

function OnDOMContentLoaded(): void {
    const { i18n } = useI18nStore();

    const app = createApp(App);

    app.use(i18n);
    app.use(RouterContext.Router);

    app.mount('#app');
}

function OnWindowLoad(): void {
};