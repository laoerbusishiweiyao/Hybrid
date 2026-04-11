<script setup lang="ts">
import { ApplicationContext } from '@runtime/ApplicationContext';
import { Web2UnityLoadedMessage, Web2UnityVersionRequest } from '@runtime/generated/message/Message';
import type { ResponseObject } from '@runtime/Message/IMessage';
import { UnitySession } from '@runtime/Message/UnitySession';
import { ref } from 'vue';

const count = ref(0);
const duration = ref(0);
const response = ref<ResponseObject | null>(null);
let isSending = false;

function onSend() {
    UnitySession.send(new Web2UnityLoadedMessage());
}

async function onSendAsync() {
    if (isSending) {
        return;
    }
    count.value++;
    isSending = true;
    const start = Date.now();
    response.value = await UnitySession.sendAsync(new Web2UnityVersionRequest());
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