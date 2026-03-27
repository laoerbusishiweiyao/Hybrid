<script setup lang="ts">
import { ApplicationContext } from '@runtime/ApplicationContext';
import type { ResponseObject } from '@runtime/Session/IMessage';
import { UnityInformationRequest, WebLoaded } from '@runtime/Session/Message';
import { UnitySession } from '@runtime/Session/UnitySession';
import { ref } from 'vue';

const count = ref(0);
const duration = ref(0);
const response = ref<ResponseObject | null>(null);
let isSending = false;

function onSend() {
    UnitySession.send(new WebLoaded());
}

async function onSendAsync() {
    if (isSending) {
        return;
    }
    count.value++;
    isSending = true;
    const start = Date.now();
    response.value = await UnitySession.sendAsync(new UnityInformationRequest());
    duration.value = Date.now() - start;
    isSending = false;
}
</script>

<template>
    <button data-web-ui @click="onSend">
        <span>Web2Unity(Without Response)</span>
    </button>
    <button data-web-ui @click="onSendAsync">
        <span>Web2UnityAsync(With Response)</span>
    </button>
    <div>
        SendAsync Duration(第{{ count }}次): {{ duration }}ms {{ response }}
    </div>
    <div>
        {{ ApplicationContext.messages }}
    </div>
</template>