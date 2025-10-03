<template>
  <div class="home-page">
    <!-- Hero Section with Gradient -->
    <div class="hero-section">
      <div class="hero-content">
        <div class="logo-container">
          <div class="logo">🐱</div>
          <div class="logo-shadow"></div>
        </div>
        <h1 class="hero-title fade-in">CatCat 上门喂猫</h1>
        <p class="hero-subtitle fade-in">专业·安全·可靠的宠物照护服务</p>
        <div class="search-container slide-up">
          <va-input v-model="searchText" placeholder="搜索服务、地点..." class="search-input" size="large">
            <template #prepend>
              <va-icon name="search" />
            </template>
          </va-input>
        </div>
      </div>
      <div class="wave-divider">
        <svg viewBox="0 0 1200 120" preserveAspectRatio="none">
          <path d="M321.39,56.44c58-10.79,114.16-30.13,172-41.86,82.39-16.72,168.19-17.73,250.45-.39C823.78,31,906.67,72,985.66,92.83c70.05,18.48,146.53,26.09,214.34,3V0H0V27.35A600.21,600.21,0,0,0,321.39,56.44Z" fill="currentColor"></path>
        </svg>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions scale-in">
      <va-card v-for="(action, index) in quickActions" :key="action.title" class="action-card" @click="action.onClick" :style="{ animationDelay: `${index * 0.1}s` }">
        <va-card-content>
          <div class="action-icon-wrapper" :style="{ background: action.gradient }">
            <va-icon :name="action.icon" size="medium" color="#fff" />
          </div>
          <div class="action-title">{{ action.title }}</div>
          <div class="action-subtitle">{{ action.subtitle }}</div>
        </va-card-content>
      </va-card>
    </div>

    <!-- Service Packages -->
    <div class="packages-section">
      <div class="section-header">
        <h2 class="section-title">
          <span class="gradient-text">精选套餐</span>
        </h2>
        <va-button size="small" preset="plain" @click="viewAllPackages" icon-right="arrow_forward">
          查看全部
        </va-button>
      </div>

      <div v-if="loading" class="loading-container">
        <va-progress-circle indeterminate color="primary" />
        <p class="loading-text">正在加载精彩内容...</p>
      </div>

      <div v-else class="packages-grid">
        <va-card v-for="(pkg, index) in packages" :key="pkg.id" class="package-card slide-up" :style="{ animationDelay: `${index * 0.1}s` }" @click="selectPackage(pkg)">
          <div class="package-ribbon" v-if="index === 0">
            <span>推荐</span>
          </div>
          <va-card-content>
            <div class="package-header">
              <div class="package-icon-wrapper" :style="{ background: getPackageGradient(pkg.name) }">
                <va-icon :name="getPackageIcon(pkg.name)" size="large" color="#fff" />
              </div>
              <div class="package-meta">
                <h3 class="package-name">{{ pkg.name }}</h3>
                <va-badge :text="`${pkg.duration}分钟`" color="info" />
              </div>
            </div>
            <p class="package-desc">{{ pkg.description }}</p>
            <div class="package-items">
              <va-chip v-for="(item, idx) in getServiceItems(pkg.serviceItems)" :key="idx" size="small" color="primary" outline>
                <va-icon name="check_circle" size="small" />
                {{ item }}
              </va-chip>
            </div>
            <div class="package-footer">
              <div class="package-price">
                <span class="price-symbol">¥</span>
                <span class="price-value">{{ pkg.price }}</span>
                <span class="price-unit">/次</span>
              </div>
              <va-button size="small" color="primary">
                立即预约
                <va-icon name="arrow_forward" size="small" />
              </va-button>
            </div>
          </va-card-content>
        </va-card>
      </div>
    </div>

    <!-- Features Section -->
    <div class="features-section">
      <h2 class="section-title">
        <span class="gradient-text">为什么选择我们</span>
      </h2>
      <div class="features-grid">
        <va-card v-for="(feature, index) in features" :key="feature.title" class="feature-card scale-in" :style="{ animationDelay: `${index * 0.1}s` }">
          <va-card-content>
            <div class="feature-icon-wrapper" :style="{ background: feature.gradient }">
              <va-icon :name="feature.icon" size="large" color="#fff" />
            </div>
            <h3 class="feature-title">{{ feature.title }}</h3>
            <p class="feature-desc">{{ feature.description }}</p>
          </va-card-content>
        </va-card>
      </div>
    </div>

    <!-- Floating Action Button -->
    <va-button class="fab" fab color="primary" icon="add" size="large" @click="quickOrder">
      <template #append>
        <div class="fab-tooltip">快速下单</div>
      </template>
    </va-button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getActivePackages } from '@/api/packages'
import type { ServicePackage } from '@/api/packages'

const router = useRouter()
const loading = ref(false)
const searchText = ref('')
const packages = ref<ServicePackage[]>([])

const quickActions = [
  { 
    title: '我的宠物', 
    subtitle: '管理档案',
    icon: 'pets', 
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    onClick: () => router.push('/pets') 
  },
  { 
    title: '我的订单', 
    subtitle: '查看进度',
    icon: 'receipt_long', 
    gradient: 'linear-gradient(135deg, #06d6a0 0%, #07c160 100%)',
    onClick: () => router.push('/orders') 
  },
  { 
    title: '在线客服', 
    subtitle: '7x24服务',
    icon: 'support_agent', 
    gradient: 'linear-gradient(135deg, #42a5f5 0%, #1989fa 100%)',
    onClick: () => {} 
  },
  { 
    title: '优惠活动', 
    subtitle: '限时福利',
    icon: 'local_offer', 
    gradient: 'linear-gradient(135deg, #ffa726 0%, #ff976a 100%)',
    onClick: () => {} 
  }
]

const features = [
  { 
    title: '实名认证', 
    icon: 'verified_user', 
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    description: '所有服务人员实名认证，背景审查，确保安全可靠' 
  },
  { 
    title: '服务保障', 
    icon: 'shield', 
    gradient: 'linear-gradient(135deg, #06d6a0 0%, #07c160 100%)',
    description: '服务质量保障，问题全额赔付，让您无后顾之忧' 
  },
  { 
    title: '实时反馈', 
    icon: 'photo_camera', 
    gradient: 'linear-gradient(135deg, #42a5f5 0%, #1989fa 100%)',
    description: '服务过程图片视频实时反馈，随时了解宠物状态' 
  },
  { 
    title: '专业团队', 
    icon: 'groups', 
    gradient: 'linear-gradient(135deg, #ffa726 0%, #ff976a 100%)',
    description: '专业宠物护理团队，持证上岗，经验丰富可靠' 
  }
]

const getPackageIcon = (name: string) => {
  if (name.includes('基础')) return 'food_bank'
  if (name.includes('标准')) return 'favorite'
  if (name.includes('高级') || name.includes('豪华')) return 'star'
  return 'pets'
}

const getPackageGradient = (name: string) => {
  if (name.includes('基础')) return 'linear-gradient(135deg, #42a5f5 0%, #1989fa 100%)'
  if (name.includes('标准')) return 'linear-gradient(135deg, #06d6a0 0%, #07c160 100%)'
  if (name.includes('高级') || name.includes('豪华')) return 'linear-gradient(135deg, #ffa726 0%, #ff976a 100%)'
  return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
}

const getServiceItems = (items: string) => items.split('、').slice(0, 4)

const fetchPackages = async () => {
  loading.value = true
  try {
    const res = await getActivePackages()
    packages.value = res.data
  } catch (error: any) {
    console.error('Loading failed:', error)
  } finally {
    loading.value = false
  }
}

const selectPackage = (pkg: ServicePackage) => router.push({ path: '/order/create', query: { packageId: pkg.id } })
const viewAllPackages = () => {}
const quickOrder = () => router.push('/order/create')

onMounted(() => fetchPackages())
</script>

<style scoped>
.home-page {
  min-height: 100vh;
  background: linear-gradient(180deg, #f8f9fa 0%, #e9ecef 100%);
  padding-bottom: 100px;
}

/* Hero Section */
.hero-section {
  position: relative;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 60px 20px 80px;
  overflow: hidden;
}

.hero-section::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><circle cx="50" cy="50" r="40" fill="rgba(255,255,255,0.05)"/></svg>');
  background-size: 100px 100px;
  opacity: 0.3;
}

.hero-content {
  position: relative;
  z-index: 1;
  text-align: center;
  color: white;
}

.logo-container {
  position: relative;
  display: inline-block;
  margin-bottom: 20px;
}

.logo {
  font-size: 80px;
  animation: float 3s ease-in-out infinite;
  filter: drop-shadow(0 4px 8px rgba(0, 0, 0, 0.2));
}

.logo-shadow {
  position: absolute;
  bottom: -10px;
  left: 50%;
  transform: translateX(-50%);
  width: 60px;
  height: 10px;
  background: rgba(0, 0, 0, 0.2);
  border-radius: 50%;
  filter: blur(5px);
  animation: shadow 3s ease-in-out infinite;
}

.hero-title {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 12px 0;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  letter-spacing: 1px;
}

.hero-subtitle {
  font-size: 16px;
  opacity: 0.95;
  margin: 0 0 30px 0;
  font-weight: 300;
  letter-spacing: 2px;
}

.search-container {
  max-width: 500px;
  margin: 0 auto;
}

.search-input {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.search-input :deep(.va-input__container) {
  border-radius: 50px !important;
  padding: 8px 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.wave-divider {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  overflow: hidden;
  line-height: 0;
  color: #f8f9fa;
}

.wave-divider svg {
  position: relative;
  display: block;
  width: calc(100% + 1.3px);
  height: 60px;
}

/* Quick Actions */
.quick-actions {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  padding: 20px;
  margin-top: -40px;
  position: relative;
  z-index: 2;
}

.action-card {
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  animation: scaleIn 0.5s ease-out backwards;
}

.action-card:hover {
  transform: translateY(-8px) scale(1.02);
  box-shadow: 0 12px 32px rgba(102, 126, 234, 0.2) !important;
}

.action-card :deep(.va-card__content) {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 20px 12px;
}

.action-icon-wrapper {
  width: 50px;
  height: 50px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transition: transform 0.3s;
}

.action-card:hover .action-icon-wrapper {
  transform: scale(1.1) rotate(5deg);
}

.action-title {
  font-size: 14px;
  font-weight: 600;
  text-align: center;
  color: #2c3e50;
}

.action-subtitle {
  font-size: 11px;
  color: #7f8c8d;
  text-align: center;
}

/* Packages Section */
.packages-section {
  padding: 40px 20px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.section-title {
  font-size: 24px;
  margin: 0;
  font-weight: 700;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  gap: 20px;
}

.loading-text {
  color: #7f8c8d;
  font-size: 14px;
}

.packages-grid {
  display: grid;
  gap: 20px;
}

.package-card {
  position: relative;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  animation: slideUp 0.6s ease-out backwards;
  overflow: visible;
}

.package-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.12) !important;
}

.package-ribbon {
  position: absolute;
  top: -6px;
  right: 20px;
  z-index: 10;
  background: linear-gradient(135deg, #ff6b6b 0%, #ee0a24 100%);
  color: white;
  padding: 4px 16px;
  border-radius: 0 0 8px 8px;
  font-size: 12px;
  font-weight: 600;
  box-shadow: 0 2px 8px rgba(255, 107, 107, 0.4);
}

.package-header {
  display: flex;
  gap: 16px;
  align-items: flex-start;
  margin-bottom: 16px;
}

.package-icon-wrapper {
  flex-shrink: 0;
  width: 64px;
  height: 64px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  transition: transform 0.3s;
}

.package-card:hover .package-icon-wrapper {
  transform: scale(1.05) rotate(-5deg);
}

.package-meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.package-name {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: #2c3e50;
}

.package-desc {
  margin: 0 0 16px 0;
  font-size: 14px;
  color: #7f8c8d;
  line-height: 1.6;
}

.package-items {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 20px;
}

.package-items .va-chip {
  display: flex;
  align-items: center;
  gap: 4px;
}

.package-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #ecf0f1;
}

.package-price {
  display: flex;
  align-items: baseline;
  gap: 2px;
}

.price-symbol {
  font-size: 18px;
  font-weight: 600;
  color: #ff6b6b;
}

.price-value {
  font-size: 32px;
  font-weight: 700;
  color: #ff6b6b;
}

.price-unit {
  font-size: 14px;
  color: #95a5a6;
  margin-left: 4px;
}

/* Features Section */
.features-section {
  padding: 40px 20px;
  background: white;
  border-radius: 32px 32px 0 0;
  margin-top: 20px;
}

.features-section .section-title {
  text-align: center;
  margin-bottom: 32px;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.feature-card {
  animation: scaleIn 0.5s ease-out backwards;
}

.feature-card :deep(.va-card__content) {
  text-align: center;
  padding: 28px 20px;
}

.feature-icon-wrapper {
  width: 64px;
  height: 64px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  transition: transform 0.3s;
}

.feature-card:hover .feature-icon-wrapper {
  transform: scale(1.1) rotate(5deg);
}

.feature-title {
  margin: 0 0 12px 0;
  font-size: 17px;
  font-weight: 600;
  color: #2c3e50;
}

.feature-desc {
  margin: 0;
  font-size: 13px;
  color: #7f8c8d;
  line-height: 1.6;
}

/* FAB */
.fab {
  position: fixed;
  bottom: 100px;
  right: 24px;
  z-index: 1000;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.4) !important;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.fab:hover {
  transform: scale(1.1) rotate(90deg);
  box-shadow: 0 12px 32px rgba(102, 126, 234, 0.6) !important;
}

.fab-tooltip {
  position: absolute;
  right: 60px;
  background: #2c3e50;
  color: white;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 13px;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.3s;
}

.fab:hover .fab-tooltip {
  opacity: 1;
}

/* Animations */
@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-15px);
  }
}

@keyframes shadow {
  0%, 100% {
    transform: translateX(-50%) scale(1);
    opacity: 0.2;
  }
  50% {
    transform: translateX(-50%) scale(0.8);
    opacity: 0.1;
  }
}

/* Responsive */
@media (max-width: 768px) {
  .quick-actions {
    grid-template-columns: repeat(4, 1fr);
    gap: 12px;
    padding: 16px;
    margin-top: -30px;
  }
  
  .action-card :deep(.va-card__content) {
    padding: 16px 8px;
  }
  
  .action-icon-wrapper {
    width: 40px;
    height: 40px;
    border-radius: 12px;
  }
  
  .action-title {
    font-size: 12px;
  }
  
  .action-subtitle {
    display: none;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
  }
  
  .packages-grid {
    gap: 16px;
  }
  
  .hero-title {
    font-size: 26px;
  }
  
  .logo {
    font-size: 64px;
  }
}

@media (min-width: 769px) {
  .packages-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .features-grid {
    grid-template-columns: repeat(4, 1fr);
  }
  
  .quick-actions {
    max-width: 800px;
    margin-left: auto;
    margin-right: auto;
  }
  
  .packages-section,
  .features-section {
    max-width: 1200px;
    margin-left: auto;
    margin-right: auto;
  }
}
</style>
