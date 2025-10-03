<template>
  <div class="home-page">
    <!-- Hero Banner -->
    <div class="hero-banner">
      <div class="hero-content">
        <div class="hero-text">
          <h1 class="hero-title">{{ t('home.title') }}</h1>
          <p class="hero-subtitle">{{ t('home.subtitle') }}</p>
        </div>
        <div class="search-bar">
          <va-input
            v-model="searchText"
            :placeholder="t('home.searchPlaceholder')"
            size="large"
          >
            <template #prepend>
              <va-icon name="search" />
            </template>
          </va-input>
        </div>
      </div>
    </div>

    <!-- Quick Actions Grid -->
    <div class="container">
      <section class="quick-actions">
        <va-button
          v-for="action in quickActions"
          :key="action.name"
          class="action-card"
          preset="plain"
          @click="action.onClick"
        >
          <div class="action-icon" :style="{ background: action.color }">
            <va-icon :name="action.icon" size="large" color="white" />
          </div>
          <span class="action-label">{{ action.title }}</span>
        </va-button>
      </section>

      <!-- Service Packages Section -->
      <section class="services-section">
        <div class="section-header">
          <h2 class="section-title">{{ t('home.ourServices') }}</h2>
          <va-button size="small" preset="plain" @click="viewAllPackages">
            {{ t('home.viewAll') }}
            <va-icon name="arrow_forward" size="small" />
          </va-button>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="services-grid">
          <va-card v-for="i in 3" :key="i" class="service-card">
            <va-card-content>
              <va-skeleton height="120px" width="100%" />
              <va-skeleton height="24px" width="70%" style="margin-top: 16px" />
              <va-skeleton height="16px" width="100%" style="margin-top: 8px" />
            </va-card-content>
          </va-card>
        </div>

        <!-- Error State -->
        <va-card v-else-if="error" class="empty-state">
          <va-card-content>
            <va-icon name="error_outline" size="64px" color="danger" />
            <h3>{{ t('home.failedToLoad') }}</h3>
            <p class="text-secondary">{{ error }}</p>
            <va-button @click="fetchPackages" color="primary">
              <va-icon name="refresh" /> {{ t('common.retry') }}
            </va-button>
          </va-card-content>
        </va-card>

        <!-- Empty State -->
        <va-card v-else-if="packages.length === 0" class="empty-state">
          <va-card-content>
            <va-icon name="inbox" size="64px" color="secondary" />
            <h3>{{ t('home.noServices') }}</h3>
            <p class="text-secondary">{{ t('home.checkBackLater') }}</p>
          </va-card-content>
        </va-card>

        <!-- Service Cards -->
        <div v-else class="services-grid">
          <va-card
            v-for="(pkg, index) in packages"
            :key="pkg.id"
            class="service-card"
            @click="selectPackage(pkg)"
          >
            <va-card-content>
              <div class="card-badge" v-if="isRecommended(index)">
                <va-badge text="推荐" color="warning" />
              </div>
              <div class="card-icon" :style="{ background: getPackageColor(index) }">
                <va-icon :name="getPackageIcon(pkg.name)" size="large" color="white" />
              </div>
              <h3 class="card-title">{{ pkg.name }}</h3>
              <p class="card-description">{{ pkg.description }}</p>
              <div class="card-tags">
                <va-chip
                  v-for="(item, idx) in getServiceItems(pkg.serviceItems)"
                  :key="idx"
                  size="small"
                  outline
                >
                  {{ item }}
                </va-chip>
              </div>
              <va-divider style="margin: 16px 0" />
              <div class="card-footer">
                <div class="card-duration">
                  <va-icon name="schedule" size="small" />
                  {{ pkg.duration }} {{ t('home.minutes') }}
                </div>
                <div class="card-price">¥{{ pkg.price }}</div>
              </div>
            </va-card-content>
          </va-card>
        </div>
      </section>

      <!-- Features Section -->
      <section class="features-section">
        <h2 class="section-title">{{ t('home.whyChooseUs') }}</h2>
        <div class="features-grid">
          <div v-for="feature in features" :key="feature.title" class="feature-card">
            <div class="feature-icon" :style="{ background: feature.color }">
              <va-icon :name="feature.icon" size="large" color="white" />
            </div>
            <h3 class="feature-title">{{ feature.title }}</h3>
            <p class="feature-description">{{ feature.description }}</p>
          </div>
        </div>
      </section>
    </div>

    <!-- FAB Button -->
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
import { useI18n } from 'vue-i18n'
import { getActivePackages } from '@/api/packages'
import type { ServicePackage } from '@/api/packages'

const router = useRouter()
const { t } = useI18n()
const loading = ref(false)
const error = ref('')
const searchText = ref('')
const packages = ref<ServicePackage[]>([])

const quickActions = [
  {
    name: 'pets',
    title: t('nav.pets'),
    icon: 'pets',
    color: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    onClick: () => router.push('/pets')
  },
  {
    name: 'orders',
    title: t('nav.orders'),
    icon: 'receipt_long',
    color: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    onClick: () => router.push('/orders')
  },
  {
    name: 'support',
    title: t('home.support'),
    icon: 'support_agent',
    color: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    onClick: () => {}
  },
  {
    name: 'profile',
    title: t('nav.profile'),
    icon: 'person',
    color: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
    onClick: () => router.push('/profile')
  }
]

const features = [
  {
    title: t('home.verifiedSitters'),
    icon: 'verified_user',
    color: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    description: t('home.verifiedSittersDesc')
  },
  {
    title: t('home.serviceGuarantee'),
    icon: 'shield',
    color: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    description: t('home.serviceGuaranteeDesc')
  },
  {
    title: t('home.realtimeUpdates'),
    icon: 'photo_camera',
    color: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    description: t('home.realtimeUpdatesDesc')
  },
  {
    title: t('home.expertTeam'),
    icon: 'groups',
    color: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
    description: t('home.expertTeamDesc')
  }
]

const getPackageIcon = (name: string) => {
  if (name.includes('基础') || name.includes('Basic')) return 'food_bank'
  if (name.includes('标准') || name.includes('Standard')) return 'favorite'
  if (name.includes('高级') || name.includes('Premium')) return 'star'
  return 'pets'
}

const getPackageColor = (index: number) => {
  const colors = [
    'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)'
  ]
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
    error.value = err.message || t('home.failedToLoad')
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
  background: #f5f5f7;
  padding-bottom: 80px;
}

/* Hero Banner */
.hero-banner {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 40px 20px;
  color: white;
}

.hero-content {
  max-width: 1200px;
  margin: 0 auto;
}

.hero-text {
  text-align: center;
  margin-bottom: 24px;
}

.hero-title {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 8px 0;
}

.hero-subtitle {
  font-size: 16px;
  opacity: 0.95;
  margin: 0;
}

.search-bar {
  max-width: 600px;
  margin: 0 auto;
}

.search-bar :deep(.va-input-wrapper) {
  background: white;
  border-radius: 24px;
  overflow: hidden;
}

/* Container */
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

/* Quick Actions */
.quick-actions {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin: -40px 0 32px 0;
  position: relative;
  z-index: 10;
}

.action-card {
  background: white;
  border-radius: 12px;
  padding: 24px 16px;
  display: flex !important;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border: none;
}

.action-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.12);
}

.action-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-label {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

/* Section */
.services-section,
.features-section {
  margin-bottom: 48px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.section-title {
  font-size: 24px;
  font-weight: 700;
  color: #333;
  margin: 0;
}

/* Services Grid */
.services-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
}

.service-card {
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid #e5e5e7;
  position: relative;
}

.service-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: transparent;
}

.card-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  z-index: 10;
}

.card-icon {
  width: 80px;
  height: 80px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
}

.card-title {
  font-size: 18px;
  font-weight: 600;
  color: #333;
  margin: 0 0 8px 0;
  text-align: center;
}

.card-description {
  font-size: 14px;
  color: #666;
  margin: 0 0 12px 0;
  text-align: center;
  line-height: 1.5;
}

.card-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: center;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-duration {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 14px;
  color: #666;
}

.card-price {
  font-size: 24px;
  font-weight: 700;
  color: #f5576c;
}

/* Features Grid */
.features-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
}

.feature-card {
  text-align: center;
}

.feature-icon {
  width: 80px;
  height: 80px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
}

.feature-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 8px 0;
}

.feature-description {
  font-size: 14px;
  color: #666;
  line-height: 1.5;
  margin: 0;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 48px 24px;
}

.empty-state h3 {
  margin: 16px 0 8px 0;
  color: #333;
}

.empty-state .text-secondary {
  color: #666;
  margin-bottom: 16px;
}

/* FAB Button */
.fab-button {
  position: fixed;
  bottom: 90px;
  right: 20px;
  z-index: 998;
  box-shadow: 0 4px 16px rgba(102, 126, 234, 0.4);
}

/* Responsive */
@media (max-width: 1024px) {
  .services-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .features-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .hero-title {
    font-size: 24px;
  }
  
  .quick-actions {
    grid-template-columns: repeat(4, 1fr);
    gap: 12px;
    margin: -30px 0 24px 0;
  }
  
  .action-card {
    padding: 16px 8px;
  }
  
  .action-icon {
    width: 44px;
    height: 44px;
  }
  
  .action-label {
    font-size: 12px;
  }
  
  .services-grid {
    grid-template-columns: 1fr;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
    gap: 32px;
  }
  
  .section-title {
    font-size: 20px;
  }
  
  .fab-button {
    bottom: 76px;
    right: 16px;
  }
}
</style>
