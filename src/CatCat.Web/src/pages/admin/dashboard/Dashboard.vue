<template>
  <div class="dashboard">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="page-title">{{ t('menu.dashboard') }}</h1>
      <p class="text-secondary">{{ t('dashboard.welcome') }}</p>
    </div>

    <!-- Gradient Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <VaCard color="primary" gradient>
        <VaCardContent>
          <div class="flex items-center justify-between text-white">
            <div>
              <div class="text-3xl font-bold">{{ stats.totalOrders }}</div>
              <div class="text-sm opacity-80 mt-1">{{ t('dashboard.stats.totalOrders') }}</div>
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
              <div class="text-sm opacity-80 mt-1">{{ t('dashboard.stats.totalPets') }}</div>
            </div>
            <VaIcon name="pets" size="large" />
          </div>
        </VaCardContent>
      </VaCard>

      <VaCard color="warning" gradient>
        <VaCardContent>
          <div class="flex items-center justify-between text-white">
            <div>
              <div class="text-3xl font-bold">{{ stats.inProgress }}</div>
              <div class="text-sm opacity-80 mt-1">{{ t('dashboard.stats.inProgress') }}</div>
            </div>
            <VaIcon name="pending" size="large" />
          </div>
        </VaCardContent>
      </VaCard>

      <VaCard color="info" gradient>
        <VaCardContent>
          <div class="flex items-center justify-between text-white">
            <div>
              <div class="text-3xl font-bold">¥{{ formatCurrency(stats.totalRevenue) }}</div>
              <div class="text-sm opacity-80 mt-1">{{ t('dashboard.stats.totalRevenue') }}</div>
            </div>
            <VaIcon name="payments" size="large" />
          </div>
        </VaCardContent>
      </VaCard>
    </div>

    <!-- Quick Actions -->
    <QuickActions class="mb-6" />

    <!-- Stats Overview -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
      <OrderStatsCard />
      <PetStatsCard />
    </div>

    <!-- Recent Data -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <RecentOrdersList />
      <MyPetsList />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { orderApi, petApi } from '../../../services/catcat-api'
import OrderStatsCard from './cards/OrderStatsCard.vue'
import PetStatsCard from './cards/PetStatsCard.vue'
import RecentOrdersList from './cards/RecentOrdersList.vue'
import MyPetsList from './cards/MyPetsList.vue'
import QuickActions from '../../../components/QuickActions.vue'

const { t } = useI18n()

const stats = ref({
  totalOrders: 0,
  totalPets: 0,
  inProgress: 0,
  totalRevenue: 0,
})

const loadStats = async () => {
  try {
    const [ordersRes, petsRes] = await Promise.all([
      orderApi.getMyOrders({ page: 1, pageSize: 100 }),
      petApi.getMyPets(),
    ])

    const orders = ordersRes.data.items || []
    stats.value.totalOrders = orders.length
    stats.value.totalPets = petsRes.data?.length || 0
    stats.value.inProgress = orders.filter((o) => o.status === 2 || o.status === 3).length
    stats.value.totalRevenue = orders.reduce((sum, o) => sum + (o.totalAmount || 0), 0)
  } catch (error) {
    console.error('Failed to load stats:', error)
  }
}

const formatCurrency = (amount: number) => {
  return amount.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
.dashboard {
  max-width: 1400px;
  margin: 0 auto;
}

.page-title {
  font-size: 1.875rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  color: var(--va-text-primary);
}
</style>
