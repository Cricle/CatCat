<template>
  <va-app>
    <div class="app-container">
      <div v-if="isDebugMode" class="debug-badge">
        🚀 DEBUG MODE
      </div>
      
      <!-- Use layout for authenticated routes -->
      <main-layout v-if="isAuthenticatedRoute && userStore.isAuthenticated">
        <router-view />
      </main-layout>
      
      <!-- No layout for auth pages -->
      <router-view v-else />
    </div>
  </va-app>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import MainLayout from '@/layouts/MainLayout.vue'

const route = useRoute()
const userStore = useUserStore()

const isDebugMode = computed(() => import.meta.env.VITE_DEBUG_MODE === 'true')

const isAuthenticatedRoute = computed(() => {
  const authRoutes = ['/login', '/register']
  return !authRoutes.includes(route.path)
})
</script>

<style>
* {
  box-sizing: border-box;
}

.app-container {
  position: relative;
  width: 100%;
  min-height: 100vh;
  background: var(--va-background);
}

.debug-badge {
  position: fixed;
  top: 70px;
  right: 10px;
  z-index: 9999;
  padding: 6px 12px;
  background: linear-gradient(135deg, #ff6b6b, #ffa500);
  color: white;
  font-size: 11px;
  font-weight: 700;
  border-radius: 6px;
  box-shadow: 0 2px 8px rgba(255, 107, 107, 0.4);
  animation: debugPulse 2s infinite;
  cursor: default;
  user-select: none;
}

@keyframes debugPulse {
  0%, 100% {
    transform: scale(1);
    box-shadow: 0 2px 8px rgba(255, 107, 107, 0.4);
  }
  50% {
    transform: scale(1.05);
    box-shadow: 0 4px 12px rgba(255, 107, 107, 0.6);
  }
}

/* Global scrollbar styling */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: var(--va-background);
}

::-webkit-scrollbar-thumb {
  background: var(--va-background-border);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: var(--va-text-secondary);
}
</style>

