<template>
  <h1 class="page-title">仪表板</h1>

  <!-- Statistics Cards -->
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
    <VaCard color="primary" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">{{ stats.totalOrders }}</div>
            <div class="text-sm opacity-80 mt-1">总订单数</div>
          </div>
          <VaIcon name="receipt_long" size="large" />
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="success" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">{{ stats.totalPets }}</div>
            <div class="text-sm opacity-80 mt-1">托管宠物</div>
          </div>
          <VaIcon name="pets" size="large" />
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="warning" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">{{ stats.totalUsers }}</div>
            <div class="text-sm opacity-80 mt-1">用户总数</div>
          </div>
          <VaIcon name="group" size="large" />
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard color="info" gradient>
      <VaCardContent>
        <div class="flex items-center justify-between text-white">
          <div>
            <div class="text-3xl font-bold">¥{{ formatCurrency(stats.totalRevenue) }}</div>
            <div class="text-sm opacity-80 mt-1">总收入</div>
          </div>
          <VaIcon name="payments" size="large" />
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Quick Actions -->
  <VaCard class="mb-6">
    <VaCardTitle>快速操作</VaCardTitle>
    <VaCardContent>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <VaButton
          to="/orders/create"
          size="large"
          color="primary"
          class="flex-col h-24"
          icon="add_circle"
        >
          创建订单
        </VaButton>
        <VaButton
          to="/pets"
          size="large"
          color="success"
          class="flex-col h-24"
          icon="pets"
        >
          我的宠物
        </VaButton>
        <VaButton
          to="/orders"
          size="large"
          color="warning"
          class="flex-col h-24"
          icon="receipt_long"
        >
          我的订单
        </VaButton>
        <VaButton
          to="/preferences"
          size="large"
          color="info"
          class="flex-col h-24"
          icon="settings"
        >
          个人设置
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Recent Orders & Pets -->
  <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
    <!-- Recent Orders -->
    <VaCard>
      <VaCardTitle>
        <div class="flex items-center justify-between">
          <span>最近订单</span>
          <VaButton preset="plain" to="/orders">查看全部</VaButton>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div v-if="loadingOrders" class="flex justify-center py-4">
          <VaProgressCircle indeterminate />
        </div>
        <div v-else-if="recentOrders.length === 0" class="text-center py-8 text-secondary">
          暂无订单
        </div>
        <div v-else class="space-y-3">
          <div
            v-for="order in recentOrders"
            :key="order.id"
            class="flex items-center justify-between p-3 rounded hover:bg-backgroundElement cursor-pointer"
            @click="$router.push(`/orders/${order.id}`)"
          >
            <div class="flex items-center gap-3">
              <VaChip :color="getOrderStatusColor(order.status)" size="small">
                {{ getOrderStatusText(order.status) }}
              </VaChip>
              <div>
                <div class="font-semibold">{{ order.pet?.name || '宠物' }}</div>
                <div class="text-sm text-secondary">{{ formatDate(order.serviceDate) }}</div>
              </div>
            </div>
            <div class="text-right">
              <div class="font-bold text-primary">¥{{ order.totalAmount.toFixed(2) }}</div>
            </div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- My Pets -->
    <VaCard>
      <VaCardTitle>
        <div class="flex items-center justify-between">
          <span>我的宠物</span>
          <VaButton preset="plain" to="/pets">查看全部</VaButton>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div v-if="loadingPets" class="flex justify-center py-4">
          <VaProgressCircle indeterminate />
        </div>
        <div v-else-if="myPets.length === 0" class="text-center py-8 text-secondary">
          暂无宠物
          <VaButton class="mt-2" to="/pets">添加宠物</VaButton>
        </div>
        <div v-else class="space-y-3">
          <div
            v-for="pet in myPets"
            :key="pet.id"
            class="flex items-center gap-3 p-3 rounded hover:bg-backgroundElement cursor-pointer"
          >
            <VaAvatar :src="pet.avatarUrl || '/default-pet.png'" />
            <div class="flex-grow">
              <div class="font-semibold">{{ pet.name }}</div>
              <div class="text-sm text-secondary">{{ pet.type }} · {{ pet.age }}岁</div>
            </div>
            <VaChip size="small" :color="getPetTypeColor(pet.type)">
              {{ pet.type }}
            </VaChip>
          </div>
        </div>
      </VaCardContent>
    </VaCard>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { orderApi, petApi, adminApi } from '../../../services/catcat-api'
import type { Order, Pet, OrderStatus } from '../../../types/catcat-types'

const router = useRouter()

const stats = ref({
  totalOrders: 0,
  totalPets: 0,
  totalUsers: 0,
  totalRevenue: 0,
})

const recentOrders = ref<Order[]>([])
const myPets = ref<Pet[]>([])
const loadingOrders = ref(false)
const loadingPets = ref(false)

// Load statistics
const loadStats = async () => {
  try {
    // Try admin API first (if user is admin)
    const response = await adminApi.getStatistics()
    stats.value = response.data
  } catch (error) {
    // Fallback: load user-specific data
    try {
      const [ordersRes, petsRes] = await Promise.all([
        orderApi.getMyOrders({ page: 1, pageSize: 100 }),
        petApi.getMyPets(),
      ])

      stats.value.totalOrders = ordersRes.data.total || ordersRes.data.items?.length || 0
      stats.value.totalPets = petsRes.data?.length || 0
      stats.value.totalRevenue =
        ordersRes.data.items?.reduce((sum: number, o: Order) => sum + o.totalAmount, 0) || 0
    } catch (e) {
      console.error('Failed to load stats:', e)
    }
  }
}

// Load recent orders
const loadRecentOrders = async () => {
  loadingOrders.value = true
  try {
    const response = await orderApi.getMyOrders({ page: 1, pageSize: 5 })
    recentOrders.value = response.data.items || []
  } catch (error) {
    console.error('Failed to load orders:', error)
  } finally {
    loadingOrders.value = false
  }
}

// Load my pets
const loadMyPets = async () => {
  loadingPets.value = true
  try {
    const response = await petApi.getMyPets()
    myPets.value = (response.data || []).slice(0, 5) // Show only first 5
  } catch (error) {
    console.error('Failed to load pets:', error)
  } finally {
    loadingPets.value = false
  }
}

// Helper functions
const formatCurrency = (amount: number) => {
  return amount.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

const getOrderStatusText = (status: OrderStatus) => {
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

const getOrderStatusColor = (status: OrderStatus) => {
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

const getPetTypeColor = (type: string) => {
  const map: Record<string, string> = {
    猫: 'primary',
    狗: 'success',
    其他: 'warning',
  }
  return map[type] || 'secondary'
}

onMounted(() => {
  loadStats()
  loadRecentOrders()
  loadMyPets()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}
</style>
