<template>
  <div class="home-page">
    <!-- Hero Section using Vuestic -->
    <va-card class="hero-card" gradient>
      <va-card-content>
        <div class="hero-content">
          <va-icon name="pets" size="64px" color="primary" />
          <h1 class="va-h1">CatCat Pet Care</h1>
          <p class="va-text-secondary">Professional & Reliable Pet Sitting Services</p>
          <va-input 
            v-model="searchText" 
            placeholder="Search services, locations..." 
            size="large"
            class="search-input"
          >
            <template #prepend>
              <va-icon name="search" />
            </template>
          </va-input>
        </div>
      </va-card-content>
    </va-card>

    <!-- Quick Actions using Vuestic Grid -->
    <div class="section-wrapper">
      <va-card>
        <va-card-title>Quick Actions</va-card-title>
        <va-card-content>
          <div class="va-row">
            <div 
              v-for="action in quickActions" 
              :key="action.title" 
              class="flex xs12 sm6 md3"
            >
              <va-button 
                preset="plain" 
                class="action-btn" 
                @click="action.onClick"
              >
                <va-icon :name="action.icon" :color="action.color" size="large" />
                <div class="action-text">
                  <div class="va-text-bold">{{ action.title }}</div>
                  <div class="va-text-secondary">{{ action.subtitle }}</div>
                </div>
              </va-button>
            </div>
          </div>
        </va-card-content>
      </va-card>
    </div>

    <!-- Service Packages using Vuestic Grid -->
    <div class="section-wrapper">
      <div class="section-header">
        <h2 class="va-h2">Our Services</h2>
        <va-button preset="plain" size="small" @click="viewAllPackages">
          View All
          <va-icon name="arrow_forward" size="small" />
        </va-button>
      </div>

      <!-- Skeleton Loading -->
      <div v-if="loading" class="va-row">
        <div v-for="i in 3" :key="i" class="flex xs12 md4">
          <va-card class="package-card">
            <va-card-content>
              <va-skeleton height="48px" width="48px" variant="squared" />
              <va-skeleton height="24px" width="80%" style="margin-top: 16px" />
              <va-skeleton height="40px" width="100%" style="margin-top: 8px" />
              <va-skeleton height="20px" width="60%" style="margin-top: 12px" />
            </va-card-content>
          </va-card>
        </div>
      </div>

      <!-- Error State -->
      <va-card v-else-if="error" class="error-state">
        <va-card-content>
          <div class="text-center">
            <va-icon name="error_outline" size="64px" color="danger" />
            <h3 class="va-h3">Failed to Load Services</h3>
            <p class="va-text-secondary">{{ error }}</p>
            <va-button @click="fetchPackages" color="primary">
              <va-icon name="refresh" /> Retry
            </va-button>
          </div>
        </va-card-content>
      </va-card>

      <!-- Empty State -->
      <va-card v-else-if="packages.length === 0" class="empty-state">
        <va-card-content>
          <div class="text-center">
            <va-icon name="inbox" size="64px" color="secondary" />
            <h3 class="va-h3">No Services Available</h3>
            <p class="va-text-secondary">Check back later for new service packages</p>
          </div>
        </va-card-content>
      </va-card>

      <!-- Packages Grid -->
      <div v-else class="va-row">
        <div 
          v-for="(pkg, index) in packages" 
          :key="pkg.id" 
          class="flex xs12 md4"
        >
          <va-card 
            class="package-card" 
            hover
            @click="selectPackage(pkg)"
          >
            <va-card-content>
              <div class="package-header">
                <va-avatar :color="getPackageColor(index)" size="48px">
                  <va-icon :name="getPackageIcon(pkg.name)" color="white" />
                </va-avatar>
                <va-badge v-if="isRecommended(index)" text="Recommended" color="warning" />
              </div>
              <h3 class="va-h3 package-name">{{ pkg.name }}</h3>
              <p class="va-text-secondary package-desc">{{ pkg.description }}</p>
              <div class="package-tags">
                <va-chip 
                  v-for="(item, idx) in getServiceItems(pkg.serviceItems)" 
                  :key="idx" 
                  size="small" 
                  outline
                >
                  {{ item }}
                </va-chip>
              </div>
              <va-divider />
              <div class="package-footer">
                <va-chip size="small" color="info">
                  <va-icon name="schedule" size="small" /> {{ pkg.duration }} min
                </va-chip>
                <div class="va-h3 va-text-primary">¥{{ pkg.price }}</div>
              </div>
            </va-card-content>
          </va-card>
        </div>
      </div>
    </div>

    <!-- Features using Vuestic Grid -->
    <div class="section-wrapper">
      <h2 class="va-h2 text-center">Why Choose Us</h2>
      <div class="va-row">
        <div v-for="feature in features" :key="feature.title" class="flex xs12 sm6 md3">
          <va-card hover class="feature-card">
            <va-card-content>
              <div class="text-center">
                <va-icon :name="feature.icon" :color="feature.color" size="48px" />
                <h3 class="va-h3">{{ feature.title }}</h3>
                <p class="va-text-secondary">{{ feature.description }}</p>
              </div>
            </va-card-content>
          </va-card>
        </div>
      </div>
    </div>

    <!-- FAB using Vuestic -->
    <va-button 
      class="fab-button" 
      fab 
      color="primary" 
      icon="add" 
      size="large"
      @click="quickOrder"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getActivePackages } from '@/api/packages'
import type { ServicePackage } from '@/api/packages'

const router = useRouter()
const loading = ref(false)
const error = ref('')
const searchText = ref('')
const packages = ref<ServicePackage[]>([])

const quickActions = [
  { 
    title: 'My Pets', 
    subtitle: 'Manage profiles', 
    icon: 'pets', 
    color: 'primary', 
    onClick: () => router.push('/pets') 
  },
  { 
    title: 'Orders', 
    subtitle: 'Track status', 
    icon: 'receipt_long', 
    color: 'success', 
    onClick: () => router.push('/orders') 
  },
  { 
    title: 'Support', 
    subtitle: '24/7 help', 
    icon: 'support_agent', 
    color: 'info', 
    onClick: () => {} 
  },
  { 
    title: 'Profile', 
    subtitle: 'Account info', 
    icon: 'person', 
    color: 'warning', 
    onClick: () => router.push('/profile') 
  }
]

const features = [
  { 
    title: 'Verified Sitters', 
    icon: 'verified_user', 
    color: 'primary', 
    description: 'All sitters are background-checked and certified' 
  },
  { 
    title: 'Service Guarantee', 
    icon: 'shield', 
    color: 'success', 
    description: 'Quality service guaranteed, full refund for any issues' 
  },
  { 
    title: 'Real-time Updates', 
    icon: 'photo_camera', 
    color: 'info', 
    description: 'Receive photo/video updates during service' 
  },
  { 
    title: 'Expert Team', 
    icon: 'groups', 
    color: 'warning', 
    description: 'Experienced and certified pet care professionals' 
  }
]

const getPackageIcon = (name: string) => {
  if (name.includes('Basic')) return 'food_bank'
  if (name.includes('Standard')) return 'favorite'
  if (name.includes('Premium')) return 'star'
  return 'pets'
}

const getPackageColor = (index: number) => {
  const colors = ['primary', 'success', 'warning', 'info', 'danger']
  return colors[index % colors.length]
}

const isRecommended = (index: number) => index === 1

const getServiceItems = (items: string) => items.split('、').slice(0, 3)

const fetchPackages = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await getActivePackages()
    packages.value = res.data
  } catch (err: any) {
    error.value = err.message || 'Failed to load service packages'
  } finally {
    loading.value = false
  }
}

const selectPackage = (pkg: ServicePackage) => {
  router.push({ name: 'order-create', query: { packageId: pkg.id } })
}

const viewAllPackages = () => {
  // TODO: Navigate to packages list page
}

const quickOrder = () => {
  router.push('/order/create')
}

onMounted(() => {
  fetchPackages()
})
</script>

<style scoped>
.home-page {
  min-height: 100vh;
  background-color: var(--va-background-element);
  padding-bottom: 80px;
}

.hero-card {
  margin-bottom: var(--va-content-padding);
  text-align: center;
}

.hero-content {
  padding: var(--va-content-padding);
}

.hero-content .va-h1 {
  margin: var(--va-content-padding) 0 8px;
}

.search-input {
  max-width: 600px;
  margin: 24px auto 0;
}

.section-wrapper {
  padding: var(--va-content-padding);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--va-content-padding);
}

.action-btn {
  width: 100%;
  height: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.action-text {
  text-align: center;
}

.package-card,
.feature-card {
  width: 100%;
  height: 100%;
  margin-bottom: var(--va-content-padding);
  cursor: pointer;
  transition: transform 0.2s;
}

.package-card:hover,
.feature-card:hover {
  transform: translateY(-4px);
}

.package-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.package-name {
  margin: 16px 0 8px;
}

.package-desc {
  min-height: 40px;
  margin-bottom: 16px;
}

.package-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}

.package-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
}

.error-state,
.empty-state {
  margin-top: var(--va-content-padding);
}

.text-center {
  text-align: center;
  padding: var(--va-content-padding);
}

.fab-button {
  position: fixed;
  bottom: 80px;
  right: 20px;
  z-index: 100;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .section-wrapper {
    padding: 12px;
  }
  
  .hero-content {
    padding: 24px 16px;
  }
  
  .package-desc {
    min-height: auto;
  }
  
  .fab-button {
    bottom: 70px;
    right: 16px;
  }
}
</style>
