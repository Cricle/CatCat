<template>
  <VaCard>
    <VaCardTitle>
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <VaIcon name="receipt_long" />
          <span>{{ t('dashboard.cards.recentOrders') }}</span>
        </div>
        <VaButton preset="secondary" size="small" @click="router.push('/orders')">
          {{ t('dashboard.cards.viewAll') }}
        </VaButton>
      </div>
    </VaCardTitle>
    <VaCardContent>
      <div v-if="loading" class="flex justify-center py-4">
        <VaProgressCircle indeterminate size="small" />
      </div>

      <div v-else-if="orders.length === 0" class="text-center py-8 text-secondary">
        {{ t('dashboard.cards.noOrders') }}
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="order in orders"
          :key="order.id"
          class="flex items-center gap-3 p-3 rounded hover:bg-gray-50 dark:hover:bg-gray-800 cursor-pointer transition"
          @click="router.push(`/orders/${order.id}`)"
        >
          <VaIcon :name="getStatusIcon(order.status)" :color="getStatusColor(order.status)" size="large" />
          <div class="flex-grow">
            <div class="font-semibold">{{ order.package?.name || '未知套餐' }}</div>
            <div class="text-sm text-secondary">{{ order.pet?.name || '未知宠物' }}</div>
          </div>
          <div class="text-right">
            <div class="font-semibold text-primary">¥{{ order.totalAmount }}</div>
            <div class="text-xs text-secondary">{{ formatDate(order.serviceDate) }}</div>
          </div>
          <VaChip :color="getStatusColor(order.status)" size="small">
            {{ getStatusText(order.status) }}
          </VaChip>
        </div>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { orderApi } from '../../../../services/catcat-api'
import type { Order } from '../../../../types/catcat-types'

const { t } = useI18n()
const router = useRouter()

const loading = ref(false)
const orders = ref<Order[]>([])

const loadOrders = async () => {
  loading.value = true
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 5 })
    orders.value = response.data.items?.slice(0, 5) || []
  } catch (error) {
    console.error('Failed to load orders:', error)
  } finally {
    loading.value = false
  }
}

const getStatusIcon = (status: number) => {
  const map: Record<number, string> = {
    1: 'schedule',
    2: 'check_circle',
    3: 'loop',
    4: 'task_alt',
    5: 'cancel',
  }
  return map[status] || 'help'
}

const getStatusColor = (status: number) => {
  const map: Record<number, string> = {
    1: 'warning',
    2: 'info',
    3: 'primary',
    4: 'success',
    5: 'danger',
  }
  return map[status] || 'secondary'
}

const getStatusText = (status: number) => {
  const map: Record<number, string> = {
    1: '待接单',
    2: '已接单',
    3: '服务中',
    4: '已完成',
    5: '已取消',
  }
  return map[status] || '未知'
}

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

onMounted(() => {
  loadOrders()
})
</script>

