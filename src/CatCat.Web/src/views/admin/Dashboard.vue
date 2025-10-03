<template>
  <div class="admin-dashboard">
    <van-nav-bar title="Admin Dashboard" fixed placeholder />

    <div v-if="loading" class="loading-container">
      <van-loading size="32px" vertical>Loading statistics...</van-loading>
    </div>

    <div v-else class="dashboard-content">
      <!-- Statistics Cards -->
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon users">
            <van-icon name="friends-o" />
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ stats.totalUsers }}</div>
            <div class="stat-label">Total Users</div>
            <div class="stat-sub">{{ stats.activeUsers }} Active</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon pets">
            <van-icon name="like-o" />
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ stats.totalPets }}</div>
            <div class="stat-label">Total Pets</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon orders">
            <van-icon name="orders-o" />
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ stats.totalOrders }}</div>
            <div class="stat-label">Total Orders</div>
            <div class="stat-sub">{{ stats.pendingOrders }} Pending</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon packages">
            <van-icon name="gift-o" />
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ stats.totalPackages }}</div>
            <div class="stat-label">Service Packages</div>
            <div class="stat-sub">{{ stats.activePackages }} Active</div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <van-cell-group title="Quick Actions" inset>
        <van-cell
          title="User Management"
          icon="friends-o"
          is-link
          @click="$router.push('/admin/users')"
        />
        <van-cell
          title="Pet Management"
          icon="like-o"
          is-link
          @click="$router.push('/admin/pets')"
        />
        <van-cell
          title="Package Management"
          icon="gift-o"
          is-link
          @click="$router.push('/admin/packages')"
        />
      </van-cell-group>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getStatistics, type AdminStatistics } from '@/api/admin'
import { showToast } from 'vant'

const loading = ref(false)
const stats = ref<AdminStatistics>({
  totalUsers: 0,
  activeUsers: 0,
  totalPets: 0,
  totalOrders: 0,
  pendingOrders: 0,
  totalPackages: 0,
  activePackages: 0
})

const fetchStatistics = async () => {
  loading.value = true
  try {
    const res = await getStatistics()
    stats.value = res.data
  } catch (error: any) {
    showToast({
      message: error.message || 'Failed to load statistics',
      icon: 'fail'
    })
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchStatistics()
})
</script>

<style scoped>
.admin-dashboard {
  min-height: 100vh;
  background-color: var(--gray-50);
  padding-bottom: 20px;
}

.loading-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

.dashboard-content {
  padding: 16px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}

.stat-card {
  background: white;
  border-radius: var(--radius);
  padding: 16px;
  display: flex;
  gap: 12px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  transition: all var(--transition);
}

.stat-card:active {
  transform: scale(0.98);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: white;
  flex-shrink: 0;
}

.stat-icon.users {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.stat-icon.pets {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.stat-icon.orders {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
}

.stat-icon.packages {
  background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
}

.stat-info {
  flex: 1;
  min-width: 0;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--gray-900);
  margin-bottom: 2px;
}

.stat-label {
  font-size: 12px;
  color: var(--gray-600);
  font-weight: 500;
  margin-bottom: 4px;
}

.stat-sub {
  font-size: 11px;
  color: var(--gray-500);
}

:deep(.van-cell-group) {
  margin-top: 16px;
}

:deep(.van-cell__title) {
  font-weight: 500;
}
</style>

