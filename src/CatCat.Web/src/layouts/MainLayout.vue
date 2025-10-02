<template>
  <va-layout class="main-layout">
    <template #top>
      <va-navbar color="primary" class="navbar">
        <template #left>
          <va-navbar-item class="logo">
            <span class="logo-icon">🐱</span>
            <span class="logo-text">CatCat</span>
          </va-navbar-item>
        </template>
        <template #center>
          <va-input v-if="!isMobile" v-model="searchText" placeholder="搜索服务、订单..." class="nav-search">
            <template #prepend><va-icon name="search" /></template>
          </va-input>
        </template>
        <template #right>
          <va-button preset="plain" color="#ffffff" icon="notifications" @click="showNotifications">
            <va-badge v-if="unreadCount > 0" :text="unreadCount.toString()" overlap />
          </va-button>
          <va-dropdown placement="bottom-end">
            <template #anchor>
              <va-button preset="plain" color="#ffffff">
                <va-avatar :src="userStore.userInfo?.avatar" size="small">{{ userStore.userInfo?.nickName?.charAt(0) || '?' }}</va-avatar>
                <span v-if="!isMobile" class="user-name">{{ userStore.userInfo?.nickName }}</span>
                <va-icon name="expand_more" size="small" />
              </va-button>
            </template>
            <va-dropdown-content>
              <va-list>
                <va-list-item @click="goToProfile">
                  <va-list-item-section avatar><va-icon name="person" /></va-list-item-section>
                  <va-list-item-section><va-list-item-label>个人资料</va-list-item-label></va-list-item-section>
                </va-list-item>
                <va-list-item @click="goToSettings">
                  <va-list-item-section avatar><va-icon name="settings" /></va-list-item-section>
                  <va-list-item-section><va-list-item-label>设置</va-list-item-label></va-list-item-section>
                </va-list-item>
                <va-list-separator />
                <va-list-item @click="logout">
                  <va-list-item-section avatar><va-icon name="logout" /></va-list-item-section>
                  <va-list-item-section><va-list-item-label>退出登录</va-list-item-label></va-list-item-section>
                </va-list-item>
              </va-list>
            </va-dropdown-content>
          </va-dropdown>
        </template>
      </va-navbar>
    </template>

    <template v-if="!isMobile" #left>
      <va-sidebar v-model="sidebarVisible" color="backgroundElement" width="240px">
        <va-sidebar-item v-for="item in menuItems" :key="item.path" :to="item.path" :active="isActiveRoute(item.path)" hover-opacity="0.08">
          <va-sidebar-item-content>
            <va-icon :name="item.icon" />
            <va-sidebar-item-title>{{ item.title }}</va-sidebar-item-title>
          </va-sidebar-item-content>
        </va-sidebar-item>
      </va-sidebar>
    </template>

    <template #content>
      <main class="main-content">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </template>

    <template v-if="isMobile" #bottom>
      <va-app-bar color="backgroundElement" class="bottom-nav" gradient>
        <va-button v-for="item in bottomNavItems" :key="item.path" preset="plain"
          :color="isActiveRoute(item.path) ? 'primary' : 'secondary'" @click="router.push(item.path)" class="nav-item">
          <div class="nav-item-content">
            <va-icon :name="item.icon" />
            <span class="nav-item-label">{{ item.title }}</span>
          </div>
        </va-button>
      </va-app-bar>
    </template>
  </va-layout>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()
const searchText = ref('')
const sidebarVisible = ref(true)
const unreadCount = ref(3)
const isMobile = ref(window.innerWidth < 768)

const menuItems = [
  { path: '/', title: '首页', icon: 'home' },
  { path: '/pets', title: '我的宠物', icon: 'pets' },
  { path: '/orders', title: '我的订单', icon: 'receipt_long' },
  { path: '/reviews', title: '我的评价', icon: 'star' },
  { path: '/messages', title: '消息中心', icon: 'mail' },
  { path: '/help', title: '帮助中心', icon: 'help' }
]

const bottomNavItems = [
  { path: '/', title: '首页', icon: 'home' },
  { path: '/pets', title: '宠物', icon: 'pets' },
  { path: '/orders', title: '订单', icon: 'receipt' },
  { path: '/profile', title: '我的', icon: 'person' }
]

const isActiveRoute = (path: string) => route.path === path
const handleResize = () => { isMobile.value = window.innerWidth < 768 }
const showNotifications = () => {}
const goToProfile = () => router.push('/profile')
const goToSettings = () => router.push('/settings')
const logout = () => { userStore.logout(); router.push('/login') }

onMounted(() => window.addEventListener('resize', handleResize))
onUnmounted(() => window.removeEventListener('resize', handleResize))
</script>

<style scoped>
.main-layout { height: 100vh; }
.navbar { box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08); }
.logo { display: flex; align-items: center; gap: 8px; font-weight: 600; }
.logo-icon { font-size: 24px; }
.logo-text { font-size: 18px; color: white; }
.nav-search { max-width: 400px; }
.user-name { margin: 0 8px; color: white; }
.main-content { padding: 20px; min-height: 100%; background: var(--va-background-secondary); }
.bottom-nav { display: flex; justify-content: space-around; padding: 8px 0; box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08); }
.nav-item { flex: 1; height: 100%; }
.nav-item-content { display: flex; flex-direction: column; align-items: center; gap: 4px; }
.nav-item-label { font-size: 12px; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
@media (max-width: 768px) {
  .main-content { padding: 12px; padding-bottom: 60px; }
}
</style>
