import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { RouterContext } from '@runtime/RouterContext'
import '@runtime/InputSystem'
import '@runtime/Message/UnitySession'

window.addEventListener('DOMContentLoaded', OnDOMContentLoaded);
window.addEventListener('load', OnWindowLoad);

function OnDOMContentLoaded(): void {
    const app = createApp(App);

    app.use(RouterContext.Router);

    app.mount('#app');
}

function OnWindowLoad(): void {
};