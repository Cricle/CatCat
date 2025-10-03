<template>
  <VaCard gradient stripe stripe-color="primary">
    <VaCardContent>
      <div class="flex items-center gap-4 text-white">
        <VaIcon name="shopping_cart" size="3rem" />
        <div class="flex-grow">
          <div class="text-3xl font-bold">{{ stats.total }}</div>
          <div class="text-sm opacity-80">{{ t('dashboard.cards.totalOrders') }}</div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-2 mt-4 text-white">
        <div class="bg-white/20 rounded p-2">
          <div class="text-lg font-semibold">{{ stats.pending }}</div>
          <div class="text-xs opacity-80">{{ t('dashboard.cards.pending') }}</div>
        </div>
        <div class="bg-white/20 rounded p-2">
          <div class="text-lg font-semibold">{{ stats.inProgress }}</div>
          <div class="text-xs opacity-80">{{ t('dashboard.cards.inProgress') }}</div>
        </div>
        <div class="bg-white/20 rounded p-2">
          <div class="text-lg font-semibold">{{ stats.completed }}</div>
          <div class="text-xs opacity-80">{{ t('dashboard.cards.completed') }}</div>
        </div>
        <div class="bg-white/20 rounded p-2">
          <div class="text-lg font-semibold">{{ stats.cancelled }}</div>
          <div class="text-xs opacity-80">{{ t('dashboard.cards.cancelled') }}</div>
        </div>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { orderApi } from '../../../../services/catcat-api'

const { t } = useI18n()

const stats = ref({
  total: 0,
  pending: 0,
  inProgress: 0,
  completed: 0,
  cancelled: 0,
})

const loadStats = async () => {
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 100 })
    const orders = response.data.items || []

    stats.value.total = orders.length
    stats.value.pending = orders.filter((o) => o.status === 1).length
    stats.value.inProgress = orders.filter((o) => o.status === 2 || o.status === 3).length
    stats.value.completed = orders.filter((o) => o.status === 4).length
    stats.value.cancelled = orders.filter((o) => o.status === 5).length
  } catch (error) {
    console.error('Failed to load order stats:', error)
  }
}

onMounted(() => {
  loadStats()
})
</script>

