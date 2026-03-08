<script setup lang="ts">
import { ApplicationContext } from '@runtime/ApplicationContext';

</script>

<template>
    <button data-web-ui>
        <span>点击测试</span>
    </button>
    <div>
        {{ ApplicationContext.messages }}
    </div>
</template>