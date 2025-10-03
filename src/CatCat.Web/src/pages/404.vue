<script lang="ts" setup>
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
</script>

<template>
  <div class="flex flex-col justify-center items-center h-screen bg-[var(--va-background-secondary)]">
    <div class="flex flex-col items-center gap-6 px-4">
      <VaIcon name="error_outline" size="10rem" color="secondary" />
      <h1 class="va-h1 text-center sm:text-5xl text-4xl">404</h1>
      <h2 class="text-2xl font-semibold">{{ t('404.pageNotFound') }}</h2>

      <p class="text-center text-secondary">
        {{ t('404.description') }}
      </p>

      <VaButton to="/" icon="home" class="mt-4">
        {{ t('404.backToHome') }}
      </VaButton>
    </div>
  </div>
</template>
