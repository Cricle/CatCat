<template>
  <h1 class="page-title">我的任务</h1>

  <!-- Status Tabs -->
  <VaCard class="mb-4">
    <VaCardContent>
      <VaTabs v-model="activeTab">
        <template #tabs>
          <VaTab v-for="status in taskStatusTabs" :key="status.value" :name="status.value">
            {{ status.label }}
            <VaBadge v-if="getCountByStatus(status.value) > 0" :text="getCountByStatus(status.value)" class="ml-2" />
          </VaTab>
        </template>
      </VaTabs>
    </VaCardContent>
  </VaCard>

  <!-- Tasks List -->
  <div v-if="loading" class="flex justify-center py-8">
    <VaProgressCircle indeterminate size="large" />
  </div>

  <div v-else-if="filteredTasks.length === 0" class="text-center py-8">
    <VaIcon name="task_alt" size="large" color="secondary" />
    <p class="text-secondary mt-2">暂无任务</p>
  </div>

  <div v-else class="space-y-4">
    <VaCard v-for="task in filteredTasks" :key="task.id" class="task-card">
      <VaCardContent>
        <div class="flex flex-col md:flex-row gap-4">
          <!-- Left: Task Info -->
          <div class="flex-grow">
            <!-- Header -->
            <div class="flex items-center gap-2 mb-3">
              <VaChip :color="getStatusColor(task.status)" size="small">
                {{ getStatusText(task.status) }}
              </VaChip>
              <span class="text-sm text-secondary">订单号: {{ task.orderNo }}</span>
            </div>

            <!-- Pet Info -->
            <div class="flex items-center gap-3 mb-3">
              <VaAvatar :src="task.pet?.avatarUrl || '/default-pet.png'" />
              <div>
                <div class="font-semibold">{{ task.pet?.name }}</div>
                <div class="text-sm text-secondary">{{ task.pet?.type }} · {{ task.pet?.age }}岁</div>
              </div>
            </div>

            <!-- Service Details -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-sm">
              <div class="flex items-center gap-2">
                <VaIcon name="event" size="small" />
                <span>{{ formatDate(task.serviceDate) }} {{ task.serviceTime }}</span>
              </div>
              <div class="flex items-center gap-2">
                <VaIcon name="location_on" size="small" />
                <span>{{ task.address }}</span>
              </div>
              <div class="flex items-center gap-2">
                <VaIcon name="business_center" size="small" />
                <span>{{ task.package?.name }}</span>
              </div>
              <div class="flex items-center gap-2">
                <VaIcon name="schedule" size="small" />
                <span>{{ task.package?.duration }}天 · {{ task.package?.visitsPerDay }}次/天</span>
              </div>
            </div>
          </div>

          <!-- Right: Amount & Actions -->
          <div class="flex flex-col justify-between items-end md:w-48">
            <div class="text-right">
              <div class="text-2xl font-bold text-primary">¥{{ task.totalAmount.toFixed(2) }}</div>
              <div class="text-sm text-secondary">{{ formatDate(task.createdAt) }}</div>
            </div>

            <div class="flex flex-col gap-2 w-full mt-4">
              <VaButton block preset="secondary" @click="viewDetails(task)">
                查看详情
              </VaButton>
              <VaButton
                v-if="task.status === 2"
                block
                color="success"
                @click="startService(task)"
              >
                开始服务
              </VaButton>
              <VaButton
                v-if="task.status === 3"
                block
                color="primary"
                @click="updateProgress(task)"
              >
                更新进度
              </VaButton>
            </div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vuestic-ui'
import { orderApi } from '../../services/catcat-api'
import type { Order, OrderStatus } from '../../types/catcat-types'

const router = useRouter()
const { init: notify } = useToast()

const tasks = ref<Order[]>([])
const loading = ref(false)
const activeTab = ref<OrderStatus | 'all'>('all')

const taskStatusTabs = [
  { value: 'all' as const, label: '全部' },
  { value: 2 as const, label: '已接单' },
  { value: 3 as const, label: '服务中' },
  { value: 4 as const, label: '已完成' },
]

// Load my tasks (status >= 2: Accepted, InProgress, Completed)
const loadTasks = async () => {
  loading.value = true
  try {
    // TODO: Create provider-specific API endpoint
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 100 })
    tasks.value = response.data.items?.filter((o: Order) => o.status >= 2 && o.status <= 4) || []
  } catch (error: any) {
    notify({ message: '加载任务失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered tasks
const filteredTasks = computed(() => {
  if (activeTab.value === 'all') return tasks.value
  return tasks.value.filter((task) => task.status === activeTab.value)
})

// Get count by status
const getCountByStatus = (status: OrderStatus | 'all') => {
  if (status === 'all') return tasks.value.length
  return tasks.value.filter((task) => task.status === status).length
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

// Format date
const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

// View details
const viewDetails = (task: Order) => {
  router.push(`/orders/${task.id}`)
}

// Start service
const startService = (task: Order) => {
  router.push(`/provider/progress/${task.id}`)
}

// Update progress
const updateProgress = (task: Order) => {
  router.push(`/provider/progress/${task.id}`)
}

onMounted(() => {
  loadTasks()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.task-card {
  transition: all 0.3s ease;
}

.task-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}
</style>

