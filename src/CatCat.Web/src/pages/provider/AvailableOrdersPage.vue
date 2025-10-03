<template>
  <h1 class="page-title">可接订单</h1>

  <!-- Filters -->
  <VaCard class="mb-4">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4">
        <VaInput v-model="filter.search" placeholder="搜索地址、宠物名称..." class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" color="secondary" />
          </template>
        </VaInput>

        <VaSelect
          v-model="filter.sortBy"
          label="排序方式"
          :options="sortOptions"
          text-by="label"
          value-by="value"
          class="md:w-48"
        />

        <VaButton @click="loadOrders" :loading="loading">
          <VaIcon name="refresh" class="mr-1" />
          刷新
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Orders List -->
  <div v-if="loading" class="flex justify-center py-8">
    <VaProgressCircle indeterminate size="large" />
  </div>

  <div v-else-if="filteredOrders.length === 0" class="text-center py-8">
    <VaIcon name="inbox" size="large" color="secondary" />
    <p class="text-secondary mt-2">暂无可接订单</p>
  </div>

  <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
    <VaCard
      v-for="order in paginatedOrders"
      :key="order.id"
      class="order-card"
      :class="{ 'border-2 border-primary': selectedOrderId === order.id }"
    >
      <VaCardContent>
        <!-- Header -->
        <div class="flex justify-between items-start mb-3">
          <div>
            <VaChip size="small" color="warning">待接单</VaChip>
            <div class="text-sm text-secondary mt-1">订单号: {{ order.orderNo }}</div>
          </div>
          <div class="text-right">
            <div class="text-2xl font-bold text-primary">¥{{ order.totalAmount.toFixed(2) }}</div>
            <div class="text-xs text-secondary">{{ order.package?.duration }}天</div>
          </div>
        </div>

        <!-- Pet Info -->
        <div class="flex items-center gap-3 mb-3 p-3 bg-backgroundElement rounded">
          <VaAvatar :src="order.pet?.avatarUrl || '/default-pet.png'" />
          <div class="flex-grow">
            <div class="font-semibold">{{ order.pet?.name }}</div>
            <div class="text-sm text-secondary">{{ order.pet?.type }} · {{ order.pet?.age }}岁</div>
          </div>
        </div>

        <!-- Service Info -->
        <div class="space-y-2 mb-3">
          <div class="flex items-center gap-2 text-sm">
            <VaIcon name="event" size="small" />
            <span>{{ formatDate(order.serviceDate) }} {{ order.serviceTime }}</span>
          </div>
          <div class="flex items-center gap-2 text-sm">
            <VaIcon name="location_on" size="small" />
            <span>{{ order.address }}</span>
          </div>
          <div class="flex items-center gap-2 text-sm">
            <VaIcon name="business_center" size="small" />
            <span>{{ order.package?.name }}</span>
          </div>
        </div>

        <!-- Special Instructions -->
        <div v-if="order.pet?.specialInstructions" class="p-2 bg-warning bg-opacity-10 rounded mb-3">
          <div class="text-xs font-semibold mb-1">特殊说明:</div>
          <div class="text-sm">{{ order.pet.specialInstructions }}</div>
        </div>

        <!-- Actions -->
        <div class="flex gap-2">
          <VaButton block color="primary" @click="viewDetails(order)">
            查看详情
          </VaButton>
          <VaButton color="success" :loading="acceptingOrderId === order.id" @click="acceptOrder(order)">
            接单
          </VaButton>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Pagination -->
  <div v-if="!loading && filteredOrders.length > 0" class="flex justify-center mt-6">
    <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
  </div>

  <!-- Order Details Modal -->
  <VaModal v-model="showDetailsModal" size="large" title="订单详情">
    <div v-if="selectedOrder" class="space-y-4">
      <!-- Pet Details -->
      <div>
        <h3 class="font-semibold mb-2">宠物信息</h3>
        <div class="grid grid-cols-2 gap-2 text-sm">
          <div><span class="text-secondary">名称:</span> {{ selectedOrder.pet?.name }}</div>
          <div><span class="text-secondary">类型:</span> {{ selectedOrder.pet?.type }}</div>
          <div><span class="text-secondary">品种:</span> {{ selectedOrder.pet?.breed }}</div>
          <div><span class="text-secondary">年龄:</span> {{ selectedOrder.pet?.age }}岁</div>
          <div><span class="text-secondary">性别:</span> {{ selectedOrder.pet?.gender }}</div>
        </div>
      </div>

      <!-- Service Locations -->
      <div v-if="selectedOrder.pet">
        <h3 class="font-semibold mb-2">服务位置信息</h3>
        <div class="space-y-2 text-sm">
          <div v-if="selectedOrder.pet.foodLocation">
            <span class="text-secondary">猫粮位置:</span> {{ selectedOrder.pet.foodLocation }}
          </div>
          <div v-if="selectedOrder.pet.waterLocation">
            <span class="text-secondary">水盆位置:</span> {{ selectedOrder.pet.waterLocation }}
          </div>
          <div v-if="selectedOrder.pet.litterBoxLocation">
            <span class="text-secondary">猫砂盆位置:</span> {{ selectedOrder.pet.litterBoxLocation }}
          </div>
          <div v-if="selectedOrder.pet.cleaningSuppliesLocation">
            <span class="text-secondary">清洁用品位置:</span> {{ selectedOrder.pet.cleaningSuppliesLocation }}
          </div>
          <div>
            <span class="text-secondary">需要备水:</span> {{ selectedOrder.pet.needsWaterRefill ? '是' : '否' }}
          </div>
        </div>
      </div>

      <!-- Package Details -->
      <div>
        <h3 class="font-semibold mb-2">服务套餐</h3>
        <div class="grid grid-cols-2 gap-2 text-sm">
          <div><span class="text-secondary">套餐名称:</span> {{ selectedOrder.package?.name }}</div>
          <div><span class="text-secondary">服务天数:</span> {{ selectedOrder.package?.duration }}天</div>
          <div><span class="text-secondary">每天次数:</span> {{ selectedOrder.package?.visitsPerDay }}次</div>
          <div><span class="text-secondary">每次时长:</span> {{ selectedOrder.package?.minutesPerVisit }}分钟</div>
        </div>
      </div>

      <!-- Order Notes -->
      <div v-if="selectedOrder.notes">
        <h3 class="font-semibold mb-2">订单备注</h3>
        <p class="text-sm">{{ selectedOrder.notes }}</p>
      </div>
    </div>

    <template #footer>
      <div class="flex gap-2">
        <VaButton preset="secondary" @click="showDetailsModal = false">关闭</VaButton>
        <VaButton
          color="success"
          :loading="acceptingOrderId === selectedOrder?.id"
          @click="acceptOrder(selectedOrder!)"
        >
          接单
        </VaButton>
      </div>
    </template>
  </VaModal>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vuestic-ui'
import { orderApi } from '../../services/catcat-api'
import type { Order } from '../../types/catcat-types'

const router = useRouter()
const { init: notify } = useToast()

const orders = ref<Order[]>([])
const loading = ref(false)
const acceptingOrderId = ref<string | null>(null)
const showDetailsModal = ref(false)
const selectedOrder = ref<Order | null>(null)
const selectedOrderId = ref<string | null>(null)

const filter = ref({
  search: '',
  sortBy: 'newest',
})

const sortOptions = [
  { label: '最新订单', value: 'newest' },
  { label: '金额从高到低', value: 'amount-desc' },
  { label: '金额从低到高', value: 'amount-asc' },
  { label: '服务时间最近', value: 'date-asc' },
]

const pagination = ref({
  page: 1,
  perPage: 9,
})

// Load available orders (status = 1: Pending)
const loadOrders = async () => {
  loading.value = true
  try {
    // TODO: Create provider-specific API endpoint
    // For now, we'll mock pending orders
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 100 })
    orders.value = response.data.items?.filter((o: Order) => o.status === 1) || []
  } catch (error: any) {
    notify({ message: '加载订单失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered orders
const filteredOrders = computed(() => {
  let result = orders.value

  // Search filter
  if (filter.value.search) {
    const searchLower = filter.value.search.toLowerCase()
    result = result.filter(
      (order) =>
        order.address.toLowerCase().includes(searchLower) ||
        order.pet?.name.toLowerCase().includes(searchLower) ||
        order.orderNo.toLowerCase().includes(searchLower),
    )
  }

  // Sort
  switch (filter.value.sortBy) {
    case 'newest':
      result = [...result].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      break
    case 'amount-desc':
      result = [...result].sort((a, b) => b.totalAmount - a.totalAmount)
      break
    case 'amount-asc':
      result = [...result].sort((a, b) => a.totalAmount - b.totalAmount)
      break
    case 'date-asc':
      result = [...result].sort((a, b) => new Date(a.serviceDate).getTime() - new Date(b.serviceDate).getTime())
      break
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

// Format date
const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

// View details
const viewDetails = (order: Order) => {
  selectedOrder.value = order
  selectedOrderId.value = order.id
  showDetailsModal.value = true
}

// Accept order
const acceptOrder = async (order: Order) => {
  if (!confirm(`确定接单 "${order.orderNo}" 吗？`)) return

  acceptingOrderId.value = order.id
  try {
    await orderApi.acceptOrder(order.id)
    notify({ message: '接单成功！', color: 'success' })
    showDetailsModal.value = false
    await loadOrders()
    // Redirect to my tasks
    router.push('/provider/tasks')
  } catch (error: any) {
    notify({ message: error.response?.data?.message || '接单失败', color: 'danger' })
  } finally {
    acceptingOrderId.value = null
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

