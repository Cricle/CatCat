<template>
  <div class="loading-skeleton">
    <!-- Card Skeleton -->
    <div v-if="type === 'card'" class="skeleton-card">
      <VaCard>
        <VaCardContent>
          <div class="flex items-center gap-4">
            <VaSkeleton variant="circle" :width="80" :height="80" />
            <div class="flex-1 space-y-2">
              <VaSkeleton variant="text" :width="`${Math.random() * 40 + 40}%`" />
              <VaSkeleton variant="text" :width="`${Math.random() * 30 + 50}%`" />
              <VaSkeleton variant="text" :width="`${Math.random() * 20 + 60}%`" />
            </div>
          </div>
        </VaCardContent>
      </VaCard>
    </div>

    <!-- List Skeleton -->
    <div v-else-if="type === 'list'" class="skeleton-list">
      <div v-for="i in count" :key="i" class="skeleton-list-item py-4 border-b border-backgroundBorder">
        <div class="flex items-center gap-4">
          <VaSkeleton variant="circle" :width="48" :height="48" />
          <div class="flex-1 space-y-2">
            <VaSkeleton variant="text" :width="`${Math.random() * 40 + 30}%`" />
            <VaSkeleton variant="text" :width="`${Math.random() * 30 + 40}%`" />
          </div>
          <VaSkeleton variant="rectangular" :width="80" :height="32" />
        </div>
      </div>
    </div>

    <!-- Table Skeleton -->
    <div v-else-if="type === 'table'" class="skeleton-table">
      <VaSkeleton variant="rectangular" width="100%" height="40" class="mb-2" />
      <div v-for="i in count" :key="i" class="mb-2">
        <VaSkeleton variant="text" width="100%" />
      </div>
    </div>

    <!-- Grid Skeleton -->
    <div v-else-if="type === 'grid'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <VaCard v-for="i in count" :key="i">
        <VaCardContent>
          <VaSkeleton variant="rectangular" width="100%" height="180" class="mb-3" />
          <VaSkeleton variant="text" width="80%" class="mb-2" />
          <VaSkeleton variant="text" width="60%" />
        </VaCardContent>
      </VaCard>
    </div>

    <!-- Default Text Skeleton -->
    <div v-else class="space-y-2">
      <VaSkeleton variant="text" v-for="i in count" :key="i" :width="`${Math.random() * 30 + 60}%`" />
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  type?: 'card' | 'list' | 'table' | 'grid' | 'text'
  count?: number
}

withDefaults(defineProps<Props>(), {
  type: 'text',
  count: 3,
})
</script>

<style scoped>
.loading-skeleton {
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}
</style>

