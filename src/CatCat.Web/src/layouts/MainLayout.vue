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
                My Profile
              </va-button>
              <va-button preset="plain" icon="settings" @click="goToSettings" block class="menu-item">
                Settings
              </va-button>
              <va-divider style="margin: 12px 0" />
              <va-button preset="plain" icon="logout" color="danger" @click="handleLogout" block class="menu-item">
                Logout
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
            {{ item.label }}
          </span>
        </div>
      </va-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()
const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const navItems = [
  { name: 'home', label: 'Home', icon: 'home', path: '/' },
  { name: 'pets', label: 'Pets', icon: 'pets', path: '/pets' },
  { name: 'orders', label: 'Orders', icon: 'receipt_long', path: '/orders' },
  { name: 'profile', label: 'Profile', icon: 'person', path: '/profile' }
]

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
  notify({ message: 'Settings page coming soon!', color: 'info' })
}

const handleLogout = async () => {
  const agreed = await confirm({
    title: 'Confirm Logout',
    message: 'Are you sure you want to logout?',
    okText: 'Logout',
    cancelText: 'Cancel'
  })

  if (agreed) {
    await userStore.logout()
    notify({ message: 'Logged out successfully', color: 'success' })
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
  box-shadow: var(--va-shadow-sm);
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
  background: var(--va-background-element);
  border-top: 1px solid var(--va-background-border);
  display: flex;
  justify-content: space-around;
  align-items: center;
  z-index: 999;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.1);
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
