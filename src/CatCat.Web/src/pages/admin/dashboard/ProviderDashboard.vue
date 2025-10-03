<template>
  <h1 class="page-title">{{ t('providerDashboard.title') }}</h1>

  <!-- Statistics Cards -->
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
    <VaCard color="primary" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">{{ stats.totalTasks }}</div>
            <div class="text-sm opacity-80 mt-1">{{ t('providerDashboard.totalTasks') }}</div>
          </div>
          <VaIcon name="task_alt" size="large" />
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="success" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">{{ stats.completed }}</div>
            <div class="text-sm opacity-80 mt-1">{{ t('providerDashboard.completed') }}</div>
          </div>
          <VaIcon name="check_circle" size="large" />
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="warning" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">{{ stats.inProgress }}</div>
            <div class="text-sm opacity-80 mt-1">{{ t('providerDashboard.inProgress') }}</div>
          </div>
          <VaIcon name="pending" size="large" />
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="info" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">¥{{ formatCurrency(stats.totalEarnings) }}</div>
            <div class="text-sm opacity-80 mt-1">{{ t('providerDashboard.totalEarnings') }}</div>
          </div>
          <VaIcon name="payments" size="large" />
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Quick Actions -->
  <QuickActions class="mb-6" />

  <!-- My Tasks & Available Orders -->
  <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
    <!-- My Current Tasks -->
    <VaCard>
      <VaCardTitle>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <VaIcon name="assignment" />
            <span>{{ t('providerDashboard.myCurrentTasks') }}</span>
          </div>
          <VaButton preset="secondary" size="small" @click="$router.push('/provider/tasks')">
            {{ t('dashboard.cards.viewAll') }}
          </VaButton>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div v-if="loadingTasks" class="flex justify-center py-8">
          <VaProgressCircle indeterminate />
        </div>
        <div v-else-if="myTasks.length === 0" class="text-center py-8 text-secondary">
          <VaIcon name="assignment" size="3rem" color="secondary" />
          <p class="mt-2">{{ t('providerDashboard.noTasks') }}</p>
        </div>
        <div v-else class="space-y-3">
          <VaCard
            v-for="task in myTasks"
            :key="task.id"
            color="backgroundBorder"
            class="cursor-pointer hover:shadow-sm transition"
            @click="$router.push(`/provider/progress/${task.id}`)"
          >
            <VaCardContent>
              <div class="flex items-start justify-between">
                <div>
                  <div class="font-semibold">{{ task.pet?.name }}</div>
                  <div class="text-sm text-secondary">{{ task.package?.name }}</div>
                  <div class="text-xs text-secondary mt-1">
                    {{ formatDate(task.serviceDate) }}
                  </div>
                </div>
                <VaChip :color="getStatusColor(task.status)" size="small">
                  {{ getStatusText(task.status) }}
                </VaChip>
              </div>
            </VaCardContent>
          </VaCard>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Available Orders -->
    <VaCard>
      <VaCardTitle>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <VaIcon name="work_outline" />
            <span>{{ t('providerDashboard.availableOrders') }}</span>
          </div>
          <VaButton preset="secondary" size="small" @click="$router.push('/provider/available')">
            {{ t('dashboard.cards.viewAll') }}
          </VaButton>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div v-if="loadingAvailable" class="flex justify-center py-8">
          <VaProgressCircle indeterminate />
        </div>
        <div v-else-if="availableOrders.length === 0" class="text-center py-8 text-secondary">
          <VaIcon name="work_off" size="3rem" color="secondary" />
          <p class="mt-2">{{ t('providerDashboard.noAvailable') }}</p>
        </div>
        <div v-else class="space-y-3">
          <VaCard
            v-for="order in availableOrders"
            :key="order.id"
            color="backgroundBorder"
            class="cursor-pointer hover:shadow-sm transition"
            @click="acceptOrder(order)"
          >
            <VaCardContent>
              <div class="flex items-start justify-between">
                <div>
                  <div class="font-semibold">{{ order.pet?.name }}</div>
                  <div class="text-sm text-secondary">{{ order.package?.name }}</div>
                  <div class="text-xs text-secondary mt-1">
                    {{ formatDate(order.serviceDate) }}
                  </div>
                </div>
                <div class="text-right">
                  <div class="font-bold text-primary">¥{{ order.totalAmount }}</div>
                  <VaButton size="small" class="mt-2">
                    {{ t('providerDashboard.accept') }}
                  </VaButton>
                </div>
              </div>
            </VaCardContent>
          </VaCard>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Earnings Chart -->
  <VaCard>
    <VaCardTitle>
      <div class="flex items-center gap-2">
        <VaIcon name="trending_up" />
        <span>{{ t('providerDashboard.earningsTrend') }}</span>
      </div>
    </VaCardTitle>
    <VaCardContent>
      <div class="text-center py-12 text-secondary">
        <VaIcon name="insert_chart" size="4rem" color="secondary" />
        <p class="mt-2">{{ t('providerDashboard.chartPlaceholder') }}</p>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { orderApi } from '../../../services/catcat-api'
import QuickActions from '../../../components/QuickActions.vue'

const { t } = useI18n()
const router = useRouter()
const { init: notify } = useToast()

const stats = ref({
  totalTasks: 0,
  completed: 0,
  inProgress: 0,
  totalEarnings: 0,
})

const myTasks = ref<any[]>([])
const availableOrders = ref<any[]>([])
const loadingTasks = ref(false)
const loadingAvailable = ref(false)

const loadStats = async () => {
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 100 })
    const orders = response.data.items || []

    stats.value.totalTasks = orders.length
    stats.value.completed = orders.filter((o) => o.status === 4).length
    stats.value.inProgress = orders.filter((o) => o.status === 2 || o.status === 3).length
    stats.value.totalEarnings = orders.reduce((sum, o) => sum + (o.totalAmount || 0), 0)
  } catch (error) {
    console.error('Failed to load stats:', error)
  }
}

const loadMyTasks = async () => {
  loadingTasks.value = true
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 5 })
    myTasks.value = (response.data.items || []).filter((o) => o.status >= 2 && o.status <= 3).slice(0, 5)
  } catch (error) {
    console.error('Failed to load tasks:', error)
  } finally {
    loadingTasks.value = false
  }
}

const loadAvailableOrders = async () => {
  loadingAvailable.value = true
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 5 })
    availableOrders.value = (response.data.items || []).filter((o) => o.status === 1).slice(0, 5)
  } catch (error) {
    console.error('Failed to load available orders:', error)
  } finally {
    loadingAvailable.value = false
  }
}

const formatCurrency = (amount: number) => {
  return amount.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
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

const acceptOrder = (order: any) => {
  router.push(`/provider/progress/${order.id}`)
}

onMounted(() => {
  loadStats()
  loadMyTasks()
  loadAvailableOrders()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}
</style>

