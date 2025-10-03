<template>
  <div class="dashboard">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="page-title">{{ t('menu.dashboard') }}</h1>
      <p class="text-secondary">{{ t('dashboard.welcome') }}</p>
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
import { useI18n } from 'vue-i18n'
import OrderStatsCard from './cards/OrderStatsCard.vue'
import PetStatsCard from './cards/PetStatsCard.vue'
import RecentOrdersList from './cards/RecentOrdersList.vue'
import MyPetsList from './cards/MyPetsList.vue'
import QuickActions from '../../../components/QuickActions.vue'

const { t } = useI18n()
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
