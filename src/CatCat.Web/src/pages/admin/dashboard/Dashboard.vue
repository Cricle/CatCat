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
  <QuickActions class="mb-6" />

  <!-- Business Stats -->
  <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
    <OrderStatsCard />
    <PetStatsCard />
  </div>

  <!-- Recent Data -->
  <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
    <RecentOrdersList />
    <MyPetsList />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { orderApi, petApi, adminApi } from '../../../services/catcat-api'
import type { Order, Pet, OrderStatus } from '../../../types/catcat-types'
import OrderStatsCard from './cards/OrderStatsCard.vue'
import PetStatsCard from './cards/PetStatsCard.vue'
import RecentOrdersList from './cards/RecentOrdersList.vue'
import MyPetsList from './cards/MyPetsList.vue'
import QuickActions from '../../../components/QuickActions.vue'

const router = useRouter()

const stats = ref({
  totalOrders: 0,
  totalPets: 0,
  totalUsers: 0,
  totalRevenue: 0,
})

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

// Helper functions
const formatCurrency = (amount: number) => {
  return amount.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}
</style>
