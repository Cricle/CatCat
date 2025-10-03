<template>
  <div class="home-page">
    <!-- Flat Hero Section -->
    <div class="hero-section">
      <div class="hero-content">
        <div class="hero-icon">🐱</div>
        <h1 class="hero-title">CatCat Pet Care</h1>
        <p class="hero-subtitle">Professional & Reliable Pet Sitting Services</p>
        <div class="search-bar">
          <va-input 
            v-model="searchText" 
            placeholder="Search services, locations..." 
            size="large"
          >
            <template #prepend>
              <va-icon name="search" />
            </template>
          </va-input>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions">
      <div 
        v-for="action in quickActions" 
        :key="action.title" 
        class="action-item"
        @click="action.onClick"
      >
        <div class="action-icon" :style="{ color: action.color }">
          <va-icon :name="action.icon" size="medium" />
        </div>
        <div class="action-text">
          <div class="action-title">{{ action.title }}</div>
          <div class="action-subtitle">{{ action.subtitle }}</div>
        </div>
      </div>
    </div>

    <!-- Service Packages -->
    <div class="packages-section">
      <div class="section-header">
        <h2 class="section-title">Our Services</h2>
        <va-button preset="plain" size="small" @click="viewAllPackages">
          View All
          <va-icon name="arrow_forward" size="small" />
        </va-button>
      </div>

      <!-- Skeleton Loading -->
      <div v-if="loading" class="packages-grid">
        <va-card v-for="i in 3" :key="i" class="package-card package-skeleton">
          <va-card-content>
            <va-skeleton height="48px" width="48px" variant="squared" />
            <va-skeleton height="24px" width="80%" style="margin-top: 16px" />
            <va-skeleton height="40px" width="100%" style="margin-top: 8px" />
            <va-skeleton height="20px" width="60%" style="margin-top: 12px" />
          </va-card-content>
        </va-card>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <va-icon name="error_outline" size="large" color="danger" />
        <h3>Failed to Load Services</h3>
        <p>{{ error }}</p>
        <va-button @click="fetchPackages">
          <va-icon name="refresh" /> Retry
        </va-button>
      </div>

      <!-- Empty State -->
      <div v-else-if="packages.length === 0" class="empty-state">
        <va-icon name="inbox" size="large" color="secondary" />
        <h3>No Services Available</h3>
        <p>Check back later for new service packages</p>
      </div>

      <div v-else class="packages-grid">
        <va-card 
          v-for="(pkg, index) in packages" 
          :key="pkg.id" 
          class="package-card"
          @click="selectPackage(pkg)"
        >
          <va-card-content>
            <div class="package-header">
              <div class="package-icon" :style="{ background: getPackageColor(index) }">
                <va-icon :name="getPackageIcon(pkg.name)" color="white" />
              </div>
              <va-badge v-if="isRecommended(index)" text="Recommended" color="warning" />
            </div>
            <h3 class="package-name">{{ pkg.name }}</h3>
            <p class="package-desc">{{ pkg.description }}</p>
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
            <div class="package-footer">
              <div class="package-info">
                <span class="package-duration">{{ pkg.duration }} min</span>
              </div>
              <div class="package-price">
                <span class="price">¥{{ pkg.price }}</span>
              </div>
            </div>
          </va-card-content>
        </va-card>
      </div>
    </div>

    <!-- Features -->
    <div class="features-section">
      <h2 class="section-title">Why Choose Us</h2>
      <div class="features-grid">
        <div v-for="feature in features" :key="feature.title" class="feature-item">
          <div class="feature-icon" :style="{ color: feature.color }">
            <va-icon :name="feature.icon" size="large" />
          </div>
          <h3 class="feature-title">{{ feature.title }}</h3>
          <p class="feature-desc">{{ feature.description }}</p>
        </div>
      </div>
    </div>

    <!-- FAB -->
    <va-button 
      class="fab-button" 
      fab 
      color="primary" 
      icon="add" 
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
    color: 'var(--primary)', 
    onClick: () => router.push('/pets') 
  },
  { 
    title: 'Orders', 
    subtitle: 'Track status', 
    icon: 'receipt_long', 
    color: 'var(--success)', 
    onClick: () => router.push('/orders') 
  },
  { 
    title: 'Support', 
    subtitle: '24/7 help', 
    icon: 'support_agent', 
    color: 'var(--info)', 
    onClick: () => {} 
  },
  { 
    title: 'Profile', 
    subtitle: 'Account info', 
    icon: 'person', 
    color: 'var(--warning)', 
    onClick: () => router.push('/profile') 
  }
]

const features = [
  { 
    title: 'Verified Sitters', 
    icon: 'verified_user', 
    color: 'var(--primary)', 
    description: 'All sitters are background-checked and certified' 
  },
  { 
    title: 'Service Guarantee', 
    icon: 'shield', 
    color: 'var(--success)', 
    description: 'Quality service guaranteed, full refund for any issues' 
  },
  { 
    title: 'Real-time Updates', 
    icon: 'photo_camera', 
    color: 'var(--info)', 
    description: 'Receive photo/video updates during service' 
  },
  { 
    title: 'Expert Team', 
    icon: 'groups', 
    color: 'var(--warning)', 
    description: 'Experienced and certified pet care professionals' 
  }
]

const getPackageIcon = (name: string) => {
  if (name.includes('Basic')) return 'food_bank'
  if (name.includes('Standard')) return 'favorite'
  if (name.includes('Premium')) return 'star'
  return 'pets'
}

const getServiceItems = (items: string) => items.split('、').slice(0, 3)

const fetchPackages = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await getActivePackages()
    packages.value = res.data
  } catch (err: any) {
    console.error('Failed to load packages:', err)
    error.value = err.message || 'Unable to load services'
  } finally {
    loading.value = false
  }
}

const getPackageColor = (index: number) => {
  return ['var(--primary)', 'var(--success)', 'var(--info)'][index % 3]
}

const isRecommended = (index: number) => index === 0

const selectPackage = (pkg: ServicePackage) => router.push({ path: '/order/create', query: { packageId: pkg.id } })
const viewAllPackages = () => console.log('View all packages')
const quickOrder = () => router.push('/order/create')

onMounted(() => fetchPackages())
</script>

<style scoped>
.home-page {
  min-height: 100vh;
  padding-bottom: 80px;
}

/* Flat Hero */
.hero-section {
  background: white;
  border-bottom: 1px solid var(--gray-200);
  padding: 48px 20px;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
  text-align: center;
}

.hero-icon {
  font-size: 64px;
  margin-bottom: 16px;
}

.hero-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: var(--gray-900);
  margin-bottom: 8px;
}

.hero-subtitle {
  font-size: 1.125rem;
  color: var(--gray-600);
  margin-bottom: 32px;
}

.search-bar {
  max-width: 500px;
  margin: 0 auto;
}

/* Quick Actions */
.quick-actions {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 16px;
  padding: 24px 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.action-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: white;
  border: 1px solid var(--gray-200);
  border-radius: var(--radius);
  cursor: pointer;
  transition: border-color var(--transition);
}

.action-item:hover {
  border-color: var(--primary);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.action-icon {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--gray-100);
  border-radius: var(--radius);
}

.action-text {
  flex: 1;
}

.action-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--gray-900);
  margin-bottom: 2px;
}

.action-subtitle {
  font-size: 0.875rem;
  color: var(--gray-600);
}

/* Packages Section */
.packages-section {
  padding: 40px 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.section-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--gray-900);
}

.package-skeleton {
  pointer-events: none;
}

.error-state,
.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.error-state h3,
.empty-state h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--gray-900);
  margin: 16px 0 8px;
}

.error-state p,
.empty-state p {
  color: var(--gray-600);
  margin-bottom: 24px;
}

.packages-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.package-card {
  cursor: pointer;
  transition: all var(--transition);
}

.package-card:hover {
  border-color: var(--primary) !important;
  transform: translateY(-4px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.package-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.package-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius);
  display: flex;
  align-items: center;
  justify-content: center;
}

.package-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--gray-900);
  margin-bottom: 8px;
}

.package-desc {
  font-size: 0.875rem;
  color: var(--gray-600);
  margin-bottom: 16px;
  line-height: 1.5;
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
  border-top: 1px solid var(--gray-200);
}

.package-duration {
  font-size: 0.875rem;
  color: var(--gray-600);
}

.package-price .price {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--primary);
}

/* Features */
.features-section {
  padding: 40px 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 24px;
  margin-top: 24px;
}

.feature-item {
  text-align: center;
  padding: 32px 20px;
  background: white;
  border: 1px solid var(--gray-200);
  border-radius: var(--radius);
}

.feature-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 16px;
  background: var(--gray-100);
  border-radius: var(--radius);
  display: flex;
  align-items: center;
  justify-content: center;
}

.feature-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--gray-900);
  margin-bottom: 8px;
}

.feature-desc {
  font-size: 0.875rem;
  color: var(--gray-600);
  line-height: 1.5;
}

/* FAB */
.fab-button {
  position: fixed;
  bottom: 24px;
  right: 24px;
  box-shadow: var(--shadow-sm) !important;
}

/* Responsive */
@media (max-width: 768px) {
  .hero-title {
    font-size: 2rem;
  }
  
  .quick-actions {
    grid-template-columns: 1fr;
  }
  
  .packages-grid {
    grid-template-columns: 1fr;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
  }
}
</style>
