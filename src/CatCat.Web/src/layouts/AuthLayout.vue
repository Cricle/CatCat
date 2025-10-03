<template>
  <VaLayout v-if="breakpoint.lgUp" class="h-screen bg-[var(--va-background-secondary)]">
    <template #left>
      <RouterLink
        class="bg-primary h-full flex items-center justify-center"
        style="width: 35vw"
        to="/"
        aria-label="Visit homepage"
      >
        <div class="logo-container">
          <VaIcon name="pets" size="4rem" color="#FFF" />
          <h1 class="text-white text-4xl font-bold mt-4">CatCat</h1>
        </div>
      </RouterLink>
    </template>
    <template #content>
      <main class="h-full flex items-center justify-center mx-auto max-w-[420px]">
        <RouterView />
      </main>
    </template>
  </VaLayout>

  <VaLayout v-else class="h-screen bg-[var(--va-background-secondary)]">
    <template #content>
      <div class="p-4">
        <main class="h-full flex flex-row items-center justify-start mx-auto max-w-[420px]">
          <div class="flex flex-col items-start">
            <RouterLink class="py-4 flex items-center gap-2" to="/" aria-label="Visit homepage">
              <VaIcon name="pets" size="2rem" color="primary" />
              <h1 class="text-primary text-2xl font-bold">CatCat</h1>
            </RouterLink>
            <RouterView />
          </div>
        </main>
      </div>
    </template>
  </VaLayout>
</template>

<script lang="ts" setup>
import { useBreakpoint } from 'vuestic-ui'

const breakpoint = useBreakpoint()
</script>

<style scoped>
.logo-container {
  display: flex;
  flex-direction: column;
  align-items: center;
}
</style>
