<template>
  <h1 class="page-title">我的收入</h1>

  <!-- Summary Cards -->
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
    <VaCard color="primary" gradient>
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">¥{{ stats.totalEarnings.toFixed(2) }}</div>
          <div class="text-sm opacity-80 mt-1">总收入</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="success" gradient>
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">¥{{ stats.monthEarnings.toFixed(2) }}</div>
          <div class="text-sm opacity-80 mt-1">本月收入</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="warning" gradient>
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.completedOrders }}</div>
          <div class="text-sm opacity-80 mt-1">已完成订单</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="info" gradient>
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.averageRating.toFixed(1) }}</div>
          <div class="text-sm opacity-80 mt-1">平均评分</div>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Filters & Chart -->
  <VaCard class="mb-6">
    <VaCardTitle>收入趋势</VaCardTitle>
    <VaCardContent>
      <div class="flex gap-4 mb-4">
        <VaButtonGroup>
          <VaButton
            v-for="period in periods"
            :key="period.value"
            :preset="selectedPeriod === period.value ? 'primary' : 'secondary'"
            @click="selectedPeriod = period.value"
          >
            {{ period.label }}
          </VaButton>
        </VaButtonGroup>
      </div>

      <!-- Chart Placeholder -->
      <div class="chart-container">
        <canvas ref="chartCanvas"></canvas>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Earnings History -->
  <VaCard>
    <VaCardTitle>
      <div class="flex justify-between items-center">
        <span>收入明细</span>
        <VaButton preset="secondary" icon="download">导出</VaButton>
      </div>
    </VaCardTitle>
    <VaCardContent>
      <!-- Filters -->
      <div class="flex flex-col md:flex-row gap-4 mb-4">
        <VaInput v-model="filter.search" placeholder="搜索订单号..." class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" />
          </template>
        </VaInput>

        <VaDateInput v-model="filter.dateRange" mode="range" label="日期范围" class="md:w-64" />

        <VaSelect
          v-model="filter.status"
          :options="statusOptions"
          placeholder="全部状态"
          class="md:w-48"
        />
      </div>

      <!-- Table -->
      <div v-if="loading" class="flex justify-center py-8">
        <VaProgressCircle indeterminate />
      </div>

      <div v-else-if="filteredEarnings.length === 0" class="text-center py-8 text-secondary">
        暂无收入记录
      </div>

      <VaDataTable v-else :items="paginatedEarnings" :columns="columns">
        <template #cell(orderNo)="{ rowData }">
          <a :href="`/orders/${rowData.orderId}`" class="text-primary hover:underline">
            {{ rowData.orderNo }}
          </a>
        </template>

        <template #cell(amount)="{ rowData }">
          <span class="font-semibold text-primary">¥{{ rowData.amount.toFixed(2) }}</span>
        </template>

        <template #cell(status)="{ rowData }">
          <VaChip :color="getStatusColor(rowData.status)" size="small">
            {{ getStatusText(rowData.status) }}
          </VaChip>
        </template>

        <template #cell(date)="{ rowData }">
          {{ formatDateTime(rowData.completedAt) }}
        </template>

        <template #cell(rating)="{ rowData }">
          <div v-if="rowData.rating" class="flex items-center gap-1">
            <VaRating :model-value="rowData.rating" readonly size="small" />
            <span class="text-sm">({{ rowData.rating }})</span>
          </div>
          <span v-else class="text-secondary text-sm">未评价</span>
        </template>
      </VaDataTable>

      <!-- Pagination -->
      <div v-if="!loading && filteredEarnings.length > 0" class="flex justify-center mt-4">
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
import { ref, computed, onMounted, watch } from 'vue'
import { useToast } from 'vuestic-ui'
import { orderApi } from '../../services/catcat-api'
import type { Order } from '../../types/catcat-types'

const { init: notify } = useToast()

const loading = ref(false)
const selectedPeriod = ref('month')
const chartCanvas = ref<HTMLCanvasElement | null>(null)

const stats = ref({
  totalEarnings: 0,
  monthEarnings: 0,
  completedOrders: 0,
  averageRating: 0,
})

const periods = [
  { value: 'week', label: '本周' },
  { value: 'month', label: '本月' },
  { value: 'quarter', label: '本季度' },
  { value: 'year', label: '本年' },
]

const filter = ref({
  search: '',
  dateRange: null as any,
  status: 'all',
})

const statusOptions = ['全部状态', '已完成', '已支付', '待支付']

const earnings = ref<any[]>([])

const columns = [
  { key: 'orderNo', label: '订单号', sortable: true },
  { key: 'petName', label: '宠物', sortable: true },
  { key: 'amount', label: '金额', sortable: true },
  { key: 'status', label: '状态', sortable: false },
  { key: 'date', label: '完成时间', sortable: true },
  { key: 'rating', label: '评分', sortable: true },
]

const pagination = ref({
  page: 1,
  perPage: 10,
})

// Load earnings data
const loadEarnings = async () => {
  loading.value = true
  try {
    // Get completed orders (status = 4)
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 100 })
    const completedOrders = response.data.items?.filter((o: Order) => o.status === 4) || []

    // Transform to earnings data
    earnings.value = completedOrders.map((order: Order) => ({
      id: order.id,
      orderId: order.id,
      orderNo: order.orderNo,
      petName: order.pet?.name || '未知',
      amount: order.totalAmount,
      status: 'completed', // Could be 'completed', 'paid', 'pending'
      completedAt: order.completedAt || order.updatedAt,
      rating: order.review?.rating || null,
    }))

    // Calculate stats
    stats.value.totalEarnings = earnings.value.reduce((sum, e) => sum + e.amount, 0)
    stats.value.completedOrders = earnings.value.length

    // Month earnings
    const now = new Date()
    const thisMonth = earnings.value.filter((e) => {
      const date = new Date(e.completedAt)
      return date.getMonth() === now.getMonth() && date.getFullYear() === now.getFullYear()
    })
    stats.value.monthEarnings = thisMonth.reduce((sum, e) => sum + e.amount, 0)

    // Average rating
    const ratingsCount = earnings.value.filter((e) => e.rating).length
    if (ratingsCount > 0) {
      const totalRating = earnings.value.reduce((sum, e) => sum + (e.rating || 0), 0)
      stats.value.averageRating = totalRating / ratingsCount
    }
  } catch (error: any) {
    notify({ message: '加载收入数据失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered earnings
const filteredEarnings = computed(() => {
  let result = earnings.value

  // Search filter
  if (filter.value.search) {
    const searchLower = filter.value.search.toLowerCase()
    result = result.filter((e) =>
      e.orderNo.toLowerCase().includes(searchLower) ||
      e.petName.toLowerCase().includes(searchLower)
    )
  }

  // Date range filter
  if (filter.value.dateRange) {
    // TODO: Implement date range filtering
  }

  // Status filter
  if (filter.value.status !== '全部状态') {
    // TODO: Implement status filtering
  }

  return result
})

// Paginated earnings
const paginatedEarnings = computed(() => {
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredEarnings.value.slice(start, end)
})

// Total pages
const totalPages = computed(() => {
  return Math.ceil(filteredEarnings.value.length / pagination.value.perPage)
})

// Get status text
const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    completed: '已完成',
    paid: '已支付',
    pending: '待支付',
  }
  return map[status] || status
}

// Get status color
const getStatusColor = (status: string) => {
  const map: Record<string, string> = {
    completed: 'success',
    paid: 'primary',
    pending: 'warning',
  }
  return map[status] || 'secondary'
}

// Format date time
const formatDateTime = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('zh-CN')
}

// Initialize chart
const initChart = () => {
  // TODO: Initialize Chart.js chart
  // For now, just a placeholder
}

// Watch period change
watch(selectedPeriod, () => {
  // TODO: Update chart data based on period
})

onMounted(() => {
  loadEarnings()
  initChart()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.chart-container {
  height: 300px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--va-background-element);
  border-radius: 8px;
}
</style>

