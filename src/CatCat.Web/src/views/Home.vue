<template>
  <div class="home-page">
    <va-card class="header-card" gradient>
      <va-card-content>
        <div class="header-content">
          <div class="logo">🐱</div>
          <h1 class="title">CatCat 上门喂猫</h1>
          <p class="subtitle">专业、安全、可靠的宠物照护服务</p>
          <va-input v-model="searchText" placeholder="搜索服务..." class="search-input">
            <template #prepend><va-icon name="search" /></template>
          </va-input>
        </div>
      </va-card-content>
    </va-card>

    <div class="quick-actions">
      <va-card v-for="action in quickActions" :key="action.title" class="action-card" @click="action.onClick">
        <va-card-content>
          <va-icon :name="action.icon" size="large" :color="action.color" />
          <div class="action-title">{{ action.title }}</div>
        </va-card-content>
      </va-card>
    </div>

    <div class="packages-section">
      <div class="section-header">
        <h2>服务套餐</h2>
        <va-button size="small" preset="plain" @click="viewAllPackages">
          查看全部<va-icon name="arrow_forward" size="small" />
        </va-button>
      </div>

      <div v-if="loading" class="loading-container">
        <va-progress-circle indeterminate />
        <p>加载中...</p>
      </div>

      <div v-else class="packages-grid">
        <va-card v-for="pkg in packages" :key="pkg.id" class="package-card" @click="selectPackage(pkg)">
          <va-card-content>
            <div class="package-icon">
              <va-icon :name="getPackageIcon(pkg.name)" size="large" color="primary" />
            </div>
            <div class="package-info">
              <h3 class="package-name">{{ pkg.name }}</h3>
              <p class="package-desc">{{ pkg.description }}</p>
              <div class="package-items">
                <va-chip v-for="(item, index) in getServiceItems(pkg.serviceItems)" :key="index" size="small" outline>
                  {{ item }}
                </va-chip>
              </div>
              <div class="package-footer">
                <va-badge :text="`${pkg.duration}分钟`" color="info" />
                <div class="package-price">
                  <span class="price-symbol">¥</span>
                  <span class="price-value">{{ pkg.price }}</span>
                </div>
              </div>
            </div>
          </va-card-content>
        </va-card>
      </div>
    </div>

    <div class="features-section">
      <h2>为什么选择我们</h2>
      <div class="features-grid">
        <va-card v-for="feature in features" :key="feature.title" class="feature-card">
          <va-card-content>
            <va-icon :name="feature.icon" size="large" :color="feature.color" />
            <h3>{{ feature.title }}</h3>
            <p>{{ feature.description }}</p>
          </va-card-content>
        </va-card>
      </div>
    </div>

    <va-button class="fab" fab color="primary" icon="add" @click="quickOrder" />
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
  { title: '我的宠物', icon: 'pets', color: 'primary', onClick: () => router.push('/pets') },
  { title: '我的订单', icon: 'receipt_long', color: 'success', onClick: () => router.push('/orders') },
  { title: '在线客服', icon: 'support_agent', color: 'info', onClick: () => {} },
  { title: '优惠活动', icon: 'local_offer', color: 'warning', onClick: () => {} }
]

const features = [
  { title: '实名认证', icon: 'verified_user', color: 'primary', description: '所有服务人员实名认证，背景审查，确保安全' },
  { title: '服务保障', icon: 'shield', color: 'success', description: '服务质量保障，问题全额赔付，无后顾之忧' },
  { title: '实时反馈', icon: 'photo_camera', color: 'info', description: '服务过程图片视频实时反馈，随时了解宠物状态' },
  { title: '专业团队', icon: 'groups', color: 'warning', description: '专业宠物护理团队，持证上岗，经验丰富' }
]

const getPackageIcon = (name: string) => {
  if (name.includes('基础')) return 'food_bank'
  if (name.includes('标准')) return 'favorite'
  if (name.includes('高级')) return 'star'
  return 'pets'
}

const getServiceItems = (items: string) => items.split('、').slice(0, 3)

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
.home-page { min-height: 100vh; background: var(--va-background-secondary); padding-bottom: 80px; }
.header-card { margin: 0; border-radius: 0 0 24px 24px; background: linear-gradient(135deg, var(--va-primary) 0%, var(--va-secondary) 100%); }
.header-content { text-align: center; color: white; padding: 20px 0; }
.logo { font-size: 64px; margin-bottom: 12px; animation: bounce 2s infinite; }
.title { font-size: 28px; font-weight: 600; margin: 0 0 8px 0; }
.subtitle { font-size: 14px; opacity: 0.9; margin: 0 0 20px 0; }
.search-input { max-width: 400px; margin: 0 auto; }
.quick-actions { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; padding: 16px; margin-top: -20px; }
.action-card { cursor: pointer; transition: transform 0.2s; }
.action-card:hover { transform: translateY(-4px); }
.action-card :deep(.va-card__content) { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 16px 8px; }
.action-title { font-size: 13px; text-align: center; }
.packages-section { padding: 16px; }
.section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.section-header h2 { margin: 0; font-size: 20px; }
.loading-container { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px; gap: 16px; }
.packages-grid { display: grid; gap: 16px; }
.package-card { cursor: pointer; transition: all 0.3s; }
.package-card:hover { transform: translateY(-4px); box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12); }
.package-card :deep(.va-card__content) { display: flex; gap: 16px; }
.package-icon { flex-shrink: 0; width: 60px; height: 60px; display: flex; align-items: center; justify-content: center; background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%); border-radius: 12px; }
.package-info { flex: 1; }
.package-name { margin: 0 0 6px 0; font-size: 18px; font-weight: 600; }
.package-desc { margin: 0 0 12px 0; font-size: 14px; color: var(--va-text-secondary); line-height: 1.5; }
.package-items { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 12px; }
.package-footer { display: flex; justify-content: space-between; align-items: center; }
.package-price { font-size: 24px; font-weight: 600; color: var(--va-danger); }
.price-symbol { font-size: 16px; margin-right: 2px; }
.features-section { padding: 16px; }
.features-section h2 { margin: 0 0 16px 0; font-size: 20px; text-align: center; }
.features-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
.feature-card :deep(.va-card__content) { text-align: center; padding: 20px; }
.feature-card h3 { margin: 12px 0 8px 0; font-size: 16px; }
.feature-card p { margin: 0; font-size: 13px; color: var(--va-text-secondary); line-height: 1.5; }
.fab { position: fixed; bottom: 80px; right: 20px; z-index: 100; }
@media (max-width: 768px) {
  .quick-actions { grid-template-columns: repeat(4, 1fr); }
  .features-grid { grid-template-columns: 1fr; }
}
@media (min-width: 769px) {
  .packages-grid { grid-template-columns: repeat(2, 1fr); }
  .features-grid { grid-template-columns: repeat(4, 1fr); }
}
@keyframes bounce { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }
</style>
