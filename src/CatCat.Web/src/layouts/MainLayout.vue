<template>
  <div class="main-layout">
    <!-- Top Navbar -->
    <va-navbar class="top-navbar" color="primary">
      <template #left>
        <va-navbar-item class="brand" @click="goHome">
          <va-icon name="pets" size="large" />
          <span class="brand-text">CatCat</span>
        </va-navbar-item>
      </template>

      <template #right>
        <!-- Language Switcher -->
        <va-navbar-item>
          <va-button preset="plain" :icon="currentLocale === 'zh-CN' ? 'language' : 'translate'" @click="toggleLanguage" color="textPrimary">
            {{ currentLocale === 'zh-CN' ? '中' : 'EN' }}
          </va-button>
        </va-navbar-item>
        
        <va-navbar-item v-if="userStore.isAuthenticated">
          <va-button preset="plain" icon="notifications" color="textPrimary" />
        </va-navbar-item>
        
        <va-navbar-item v-if="userStore.isAuthenticated">
          <va-dropdown placement="bottom-end">
            <template #anchor>
              <va-avatar size="small" color="warning">
                <va-icon name="person" />
              </va-avatar>
            </template>
            <va-dropdown-content class="user-menu">
              <div class="user-info">
                <va-avatar color="warning" size="large">
                  <va-icon name="person" />
                </va-avatar>
                <div class="user-details">
                  <div class="va-text-bold">{{ userStore.userInfo?.name || 'User' }}</div>
                  <div class="va-text-secondary" style="font-size: 12px">
                    {{ userStore.userInfo?.phone }}
                  </div>
                </div>
              </div>
              <va-divider style="margin: 12px 0" />
              <va-button preset="plain" icon="person" @click="goToProfile" block class="menu-item">
                {{ t('profile.myProfile') }}
              </va-button>
              <va-button preset="plain" icon="settings" @click="goToSettings" block class="menu-item">
                {{ t('common.settings') }}
              </va-button>
              <va-divider style="margin: 12px 0" />
              <va-button preset="plain" icon="logout" color="danger" @click="handleLogout" block class="menu-item">
                {{ t('common.logout') }}
              </va-button>
            </va-dropdown-content>
          </va-dropdown>
        </va-navbar-item>
      </template>
    </va-navbar>

    <!-- Main Content -->
    <div class="main-content">
      <router-view />
    </div>

    <!-- Bottom Navigation -->
    <div v-if="userStore.isAuthenticated" class="bottom-nav">
      <va-button 
        v-for="item in navItems" 
        :key="item.name"
        preset="plain"
        :icon="item.icon"
        :color="isActive(item.path) ? 'primary' : 'secondary'"
        class="nav-item"
        @click="navigate(item.path)"
      >
        <div class="nav-content">
          <va-icon :name="item.icon" :size="isActive(item.path) ? 'large' : 'medium'" />
          <span class="nav-label" :class="{ active: isActive(item.path) }">
            {{ t(item.labelKey) }}
          </span>
        </div>
      </va-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useToast, useModal } from 'vuestic-ui'
import { useI18n } from 'vue-i18n'

const { t, locale } = useI18n()
const { init: notify } = useToast()
const { confirm } = useModal()
const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const currentLocale = computed(() => locale.value)

const navItems = [
  { name: 'home', labelKey: 'nav.home', icon: 'home', path: '/' },
  { name: 'pets', labelKey: 'nav.pets', icon: 'pets', path: '/pets' },
  { name: 'orders', labelKey: 'nav.orders', icon: 'receipt_long', path: '/orders' },
  { name: 'profile', labelKey: 'nav.profile', icon: 'person', path: '/profile' }
]

const toggleLanguage = () => {
  locale.value = locale.value === 'zh-CN' ? 'en-US' : 'zh-CN'
  localStorage.setItem('locale', locale.value)
  notify({ 
    message: locale.value === 'zh-CN' ? '已切换到中文' : 'Switched to English', 
    color: 'success' 
  })
}

const isActive = (path: string) => {
  if (path === '/') {
    return route.path === '/'
  }
  return route.path.startsWith(path)
}

const navigate = (path: string) => {
  if (route.path !== path) {
    router.push(path)
  }
}

const goHome = () => {
  router.push('/')
}

const goToProfile = () => {
  router.push('/profile')
}

const goToSettings = () => {
  notify({ message: t('common.settings') + ' - Coming soon!', color: 'info' })
}

const handleLogout = async () => {
  const agreed = await confirm({
    title: t('auth.logoutConfirm'),
    message: t('auth.logoutConfirm'),
    okText: t('common.confirm'),
    cancelText: t('common.cancel')
  })

  if (agreed) {
    await userStore.logout()
    notify({ message: t('auth.logoutSuccess'), color: 'success' })
    router.push('/login')
  }
}
</script>

<style scoped>
.main-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background: var(--va-background);
}

.top-navbar {
  position: sticky;
  top: 0;
  z-index: 1000;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
}

.brand {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.brand-text {
  font-size: 20px;
  font-weight: 700;
  color: white;
}

.main-content {
  flex: 1;
  overflow-y: auto;
  padding-bottom: 80px; /* Space for bottom nav */
}

.user-menu {
  min-width: 240px;
  padding: 12px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
}

.user-details {
  flex: 1;
}

.menu-item {
  justify-content: flex-start !important;
  margin-bottom: 4px;
}

.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 64px;
  background: white;
  border-top: 1px solid #e5e5e7;
  display: flex;
  justify-content: space-around;
  align-items: center;
  z-index: 999;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
  backdrop-filter: blur(10px);
}

.nav-item {
  flex: 1;
  height: 100%;
  border-radius: 0 !important;
  padding: 0 !important;
}

.nav-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  height: 100%;
}

.nav-label {
  font-size: 11px;
  font-weight: 500;
  transition: all 0.2s;
  color: var(--va-text-secondary);
}

.nav-label.active {
  font-weight: 700;
  color: var(--va-primary);
}

@media (max-width: 768px) {
  .brand-text {
    font-size: 18px;
  }
  
  .nav-label {
    font-size: 10px;
  }
}
</style>
