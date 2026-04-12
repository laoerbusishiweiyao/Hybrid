<template>
    <div class="splash-page">
        <span>{{ translate('splash.title') }}</span>

        <span v-if="information.loaded">按任意键开始</span>

        <span>{{ information.version }}</span>
    </div>
</template>

<script setup lang="ts">
import { useApplicationStore } from '@runtime/store/useApplicationStore';
import { useI18nStore } from '@runtime/store/useI18nStore';

const { translate } = useI18nStore();
const { information } = useApplicationStore();
</script>

<style scoped>
.splash-page {
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    background-color: #333;

    padding-bottom: 5rem;

    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 2rem;
}

.splash-page>span {
    color: azure;
}

.splash-page>span:first-of-type {
    flex: 1;

    padding-bottom: 30rem;

    font-size: 12rem;
    align-content: center;
}

.splash-page>span:nth-of-type(2) {
    font-size: 3.2rem;
}
</style>
