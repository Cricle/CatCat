<template>
  <h1 class="page-title">{{ t('admin.orders.title') }}</h1>

  <!-- Stats Cards -->
  <div class="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
    <VaCard gradient stripe stripe-color="info">
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.total }}</div>
          <div class="text-sm opacity-80 mt-1">{{ t('admin.orders.total') }}</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard gradient stripe stripe-color="warning">
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.pending }}</div>
          <div class="text-sm opacity-80 mt-1">{{ t('admin.orders.pending') }}</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard gradient stripe stripe-color="primary">
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.inProgress }}</div>
          <div class="text-sm opacity-80 mt-1">{{ t('admin.orders.inProgress') }}</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard gradient stripe stripe-color="success">
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.completed }}</div>
          <div class="text-sm opacity-80 mt-1">{{ t('admin.orders.completed') }}</div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard gradient stripe stripe-color="danger">
      <VaCardContent>
        <div class="text-white">
          <div class="text-3xl font-bold">{{ stats.cancelled }}</div>
          <div class="text-sm opacity-80 mt-1">{{ t('admin.orders.cancelled') }}</div>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Filters -->
  <VaCard class="mb-6">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4">
        <VaInput v-model="filter.search" :placeholder="t('admin.orders.searchPlaceholder')" class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" />
          </template>
        </VaInput>

        <VaSelect v-model="filter.status" :options="statusOptions" :label="t('admin.orders.status')" class="md:w-48" />

        <VaDateInput v-model="filter.dateRange" mode="range" :label="t('admin.orders.dateRange')" class="md:w-64" />

        <VaButton preset="secondary" icon="refresh" @click="loadOrders">
          {{ t('common.refresh') }}
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Orders Table -->
  <VaCard>
    <VaCardContent>
      <div v-if="loading" class="flex justify-center py-8">
        <VaProgressCircle indeterminate />
      </div>

      <div v-else-if="filteredOrders.length === 0" class="text-center py-8 text-secondary">
        {{ t('admin.orders.noOrders') }}
      </div>

      <VaDataTable v-else :items="paginatedOrders" :columns="columns" striped>
        <template #cell(orderNo)="{ rowData }">
          <a :href="`/orders/${rowData.id}`" class="text-primary hover:underline font-mono text-sm">
            {{ rowData.orderNo }}
          </a>
        </template>

        <template #cell(customer)="{ rowData }">
          <div>
            <div class="font-semibold">{{ rowData.user?.name || '未知' }}</div>
            <div class="text-sm text-secondary">{{ rowData.user?.phone }}</div>
          </div>
        </template>

        <template #cell(pet)="{ rowData }">
          <div class="flex items-center gap-2">
            <VaIcon name="pets" size="small" />
            <span>{{ rowData.pet?.name || '未知' }}</span>
          </div>
        </template>

        <template #cell(package)="{ rowData }">
          <div>
            <div class="font-semibold">{{ rowData.package?.name || '未知' }}</div>
            <div class="text-sm text-secondary">¥{{ rowData.totalAmount }}</div>
          </div>
        </template>

        <template #cell(status)="{ rowData }">
          <VaChip :color="getStatusColor(rowData.status)" size="small">
            {{ getStatusText(rowData.status) }}
          </VaChip>
        </template>

        <template #cell(serviceDate)="{ rowData }">
          <div>
            <div>{{ formatDate(rowData.serviceDate) }}</div>
            <div class="text-sm text-secondary">{{ rowData.serviceTime }}</div>
          </div>
        </template>

        <template #cell(createdAt)="{ rowData }">
          {{ formatDateTime(rowData.createdAt) }}
        </template>

        <template #cell(actions)="{ rowData }">
          <div class="flex gap-2">
            <VaButton preset="secondary" size="small" icon="visibility" @click="viewOrder(rowData)">
              {{ t('common.view') }}
            </VaButton>
            <VaButton
              v-if="rowData.status === 1 || rowData.status === 2"
              preset="secondary"
              size="small"
              icon="cancel"
              color="danger"
              @click="cancelOrder(rowData)"
            >
              {{ t('common.cancel') }}
            </VaButton>
          </div>
        </template>
      </VaDataTable>

      <!-- Pagination -->
      <div v-if="!loading && filteredOrders.length > 0" class="flex justify-center mt-4">
        <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { adminApi } from '../../../services/catcat-api'
import type { Order } from '../../../types/catcat-types'

const { t } = useI18n()
const router = useRouter()
const { init: notify } = useToast()

const loading = ref(false)
const orders = ref<Order[]>([])

const filter = ref({
  search: '',
  status: '全部状态',
  dateRange: null as any,
})

const statusOptions = ['全部状态', '待接单', '已接单', '服务中', '已完成', '已取消']

const stats = ref({
  total: 0,
  pending: 0,
  inProgress: 0,
  completed: 0,
  cancelled: 0,
})

const columns = [
  { key: 'orderNo', label: t('admin.orders.orderNo'), sortable: true },
  { key: 'customer', label: t('admin.orders.customer'), sortable: false },
  { key: 'pet', label: t('admin.orders.pet'), sortable: false },
  { key: 'package', label: t('admin.orders.package'), sortable: false },
  { key: 'status', label: t('admin.orders.status'), sortable: true },
  { key: 'serviceDate', label: t('admin.orders.serviceDate'), sortable: true },
  { key: 'createdAt', label: t('admin.orders.createdAt'), sortable: true },
  { key: 'actions', label: t('admin.orders.actions'), sortable: false },
]

const pagination = ref({
  page: 1,
  perPage: 10,
})

// Load orders
const loadOrders = async () => {
  loading.value = true
  try {
    const response = await adminApi.getAllOrders({ page: 1, pageSize: 100 })
    orders.value = response.data.items || []

    // Calculate stats
    stats.value.total = orders.value.length
    stats.value.pending = orders.value.filter((o) => o.status === 1).length
    stats.value.inProgress = orders.value.filter((o) => o.status === 2 || o.status === 3).length
    stats.value.completed = orders.value.filter((o) => o.status === 4).length
    stats.value.cancelled = orders.value.filter((o) => o.status === 5).length
  } catch (error: any) {
    notify({ message: error.message || '加载订单失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered orders
const filteredOrders = computed(() => {
  let result = orders.value

  // Search
  if (filter.value.search) {
    const search = filter.value.search.toLowerCase()
    result = result.filter(
      (o) =>
        o.orderNo?.toLowerCase().includes(search) ||
        o.user?.name?.toLowerCase().includes(search) ||
        o.user?.phone?.includes(search) ||
        o.pet?.name?.toLowerCase().includes(search),
    )
  }

  // Status
  if (filter.value.status !== '全部状态') {
    const statusMap: Record<string, number> = {
      待接单: 1,
      已接单: 2,
      服务中: 3,
      已完成: 4,
      已取消: 5,
    }
    result = result.filter((o) => o.status === statusMap[filter.value.status])
  }

  // Date range (TODO: implement)

  return result
})

// Paginated orders
const paginatedOrders = computed(() => {
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredOrders.value.slice(start, end)
})

const totalPages = computed(() => Math.ceil(filteredOrders.value.length / pagination.value.perPage))

// Get status text
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

// Get status color
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

// Format date
const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

// Format date time
const formatDateTime = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('zh-CN')
}

// View order
const viewOrder = (order: Order) => {
  router.push(`/orders/${order.id}`)
}

// Cancel order
const cancelOrder = async (order: Order) => {
  if (!confirm(`确定要取消订单"${order.orderNo}"吗？`)) return

  try {
    // await adminApi.cancelOrder(order.id, '管理员取消')
    order.status = 5
    notify({ message: '订单已取消', color: 'success' })
    loadOrders()
  } catch (error: any) {
    notify({ message: error.message || '取消失败', color: 'danger' })
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
</style>

