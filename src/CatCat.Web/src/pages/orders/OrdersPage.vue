<template>
  <h1 class="page-title">我的订单</h1>

  <VaCard>
    <VaCardContent>
      <!-- Filter Tabs -->
      <VaTabs v-model="activeTab" class="mb-4">
        <template #tabs>
          <VaTab v-for="status in orderStatusTabs" :key="status.value" :name="status.value">
            {{ status.label }}
            <VaBadge v-if="getCountByStatus(status.value) > 0" :text="getCountByStatus(status.value)" class="ml-2" />
          </VaTab>
        </template>
      </VaTabs>

      <!-- Search and Actions -->
      <div class="flex flex-col md:flex-row gap-2 mb-4 justify-between">
        <VaInput v-model="filter" placeholder="搜索订单号、宠物名称..." class="flex-grow md:max-w-md">
          <template #prependInner>
            <VaIcon name="search" color="secondary" />
          </template>
        </VaInput>
        <VaButton to="/orders/create" color="primary">创建订单</VaButton>
      </div>

      <!-- Orders List -->
      <div v-if="loading" class="flex justify-center py-8">
        <VaProgressCircle indeterminate />
      </div>

      <div v-else-if="filteredOrders.length === 0" class="text-center py-8">
        <VaIcon name="inbox" size="large" color="secondary" />
        <p class="text-secondary mt-2">暂无订单</p>
      </div>

      <div v-else class="space-y-4">
        <VaCard v-for="order in paginatedOrders" :key="order.id" class="order-card">
          <VaCardContent>
            <div class="flex flex-col md:flex-row justify-between gap-4">
              <!-- Order Info -->
              <div class="flex-grow">
                <div class="flex items-center gap-2 mb-2">
                  <VaChip :color="getStatusColor(order.status)" size="small">
                    {{ getStatusText(order.status) }}
                  </VaChip>
                  <span class="text-sm text-secondary">订单号: {{ order.orderNo }}</span>
                </div>

                <div class="space-y-1">
                  <div class="flex items-center gap-2">
                    <VaIcon name="pets" size="small" />
                    <span>宠物: {{ order.pet?.name || '未知' }}</span>
                  </div>
                  <div class="flex items-center gap-2">
                    <VaIcon name="business_center" size="small" />
                    <span>套餐: {{ order.package?.name || '未知' }}</span>
                  </div>
                  <div class="flex items-center gap-2">
                    <VaIcon name="event" size="small" />
                    <span>服务时间: {{ formatDateTime(order.serviceDate, order.serviceTime) }}</span>
                  </div>
                  <div class="flex items-center gap-2">
                    <VaIcon name="location_on" size="small" />
                    <span>地址: {{ order.address }}</span>
                  </div>
                </div>
              </div>

              <!-- Order Amount & Actions -->
              <div class="flex flex-col justify-between items-end">
                <div class="text-right">
                  <div class="text-2xl font-bold text-primary">¥{{ order.totalAmount.toFixed(2) }}</div>
                  <div class="text-sm text-secondary">{{ formatDate(order.createdAt) }}</div>
                </div>

                <div class="flex gap-2 mt-2">
                  <VaButton size="small" preset="secondary" @click="viewOrder(order)">查看详情</VaButton>
                  <VaButton
                    v-if="canCancelOrder(order.status)"
                    size="small"
                    color="danger"
                    @click="cancelOrder(order)"
                  >
                    取消订单
                  </VaButton>
                </div>
              </div>
            </div>
          </VaCardContent>
        </VaCard>
      </div>

      <!-- Pagination -->
      <div v-if="!loading && filteredOrders.length > 0" class="flex justify-center mt-4">
        <VaPagination
          v-model="pagination.page"
          :pages="totalPages"
          :visible-pages="5"
          buttons-preset="secondary"
        />
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vuestic-ui'
import { orderApi } from '../../services/catcat-api'
import type { Order, OrderStatus } from '../../types/catcat-types'

const router = useRouter()
const { init: notify } = useToast()

const orders = ref<Order[]>([])
const loading = ref(false)
const filter = ref('')
const activeTab = ref<OrderStatus | 'all'>('all')

const pagination = ref({
  page: 1,
  perPage: 10,
})

const orderStatusTabs = [
  { value: 'all' as const, label: '全部' },
  { value: 0 as const, label: '队列中' },
  { value: 1 as const, label: '待接单' },
  { value: 2 as const, label: '已接单' },
  { value: 3 as const, label: '服务中' },
  { value: 4 as const, label: '已完成' },
  { value: 5 as const, label: '已取消' },
]

// Load orders
const loadOrders = async () => {
  loading.value = true
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 100 })
    orders.value = response.data.items || []
  } catch (error: any) {
    notify({ message: '加载订单失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered orders
const filteredOrders = computed(() => {
  let result = orders.value

  // Filter by status
  if (activeTab.value !== 'all') {
    result = result.filter((order) => order.status === activeTab.value)
  }

  // Filter by search
  if (filter.value) {
    const searchLower = filter.value.toLowerCase()
    result = result.filter(
      (order) =>
        order.orderNo.toLowerCase().includes(searchLower) ||
        order.pet?.name.toLowerCase().includes(searchLower) ||
        order.address.toLowerCase().includes(searchLower),
    )
  }

  return result
})

// Paginated orders
const paginatedOrders = computed(() => {
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredOrders.value.slice(start, end)
})

// Total pages
const totalPages = computed(() => {
  return Math.ceil(filteredOrders.value.length / pagination.value.perPage)
})

// Get count by status
const getCountByStatus = (status: OrderStatus | 'all') => {
  if (status === 'all') return orders.value.length
  return orders.value.filter((order) => order.status === status).length
}

// Get status text
const getStatusText = (status: OrderStatus) => {
  const map: Record<OrderStatus, string> = {
    0: '队列中',
    1: '待接单',
    2: '已接单',
    3: '服务中',
    4: '已完成',
    5: '已取消',
  }
  return map[status] || '未知'
}

// Get status color
const getStatusColor = (status: OrderStatus) => {
  const map: Record<OrderStatus, string> = {
    0: 'info',
    1: 'warning',
    2: 'primary',
    3: 'success',
    4: 'success',
    5: 'danger',
  }
  return map[status] || 'secondary'
}

// Can cancel order
const canCancelOrder = (status: OrderStatus) => {
  return [0, 1, 2].includes(status) // Queued, Pending, Accepted
}

// Format date
const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-CN')
}

// Format date time
const formatDateTime = (dateStr: string, timeStr: string) => {
  const date = new Date(dateStr)
  return `${date.toLocaleDateString('zh-CN')} ${timeStr}`
}

// View order
const viewOrder = (order: Order) => {
  router.push(`/orders/${order.id}`)
}

// Cancel order
const cancelOrder = async (order: Order) => {
  if (!confirm(`确定要取消订单 "${order.orderNo}" 吗？`)) return

  try {
    await orderApi.cancelOrder(order.id, '用户主动取消')
    notify({ message: '订单已取消', color: 'success' })
    await loadOrders()
  } catch (error: any) {
    notify({ message: '取消订单失败', color: 'danger' })
  }
}

onMounted(() => {
  loadOrders()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.order-card {
  transition: all 0.3s ease;
}

.order-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}
</style>

