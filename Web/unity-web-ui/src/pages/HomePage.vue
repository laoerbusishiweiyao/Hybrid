<script setup lang="ts">
import { ApplicationContext } from '@runtime/ApplicationContext';

function onTest() {
    ApplicationContext.messages.push('Hello from HomePage!');
}
</script>

<template>
    <button data-web-ui @click="onTest">
        <span>点击测试</span>
    </button>
    <div>
        {{ ApplicationContext.messages }}
    </div>
</template>