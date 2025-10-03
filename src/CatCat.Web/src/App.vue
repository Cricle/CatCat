<template>
  <div class="app-container">
    <div v-if="isDebugMode" class="debug-badge">
      🚀 DEBUG MODE
    </div>
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const isDebugMode = computed(() => import.meta.env.VITE_DEBUG_MODE === 'true')
</script>

<style>
.app-container {
  position: relative;
  width: 100%;
  min-height: 100vh;
}

.debug-badge {
  position: fixed;
  top: 10px;
  right: 10px;
  z-index: 9999;
  padding: 6px 12px;
  background: linear-gradient(135deg, #ff6b6b, #ffa500);
  color: white;
  font-size: 12px;
  font-weight: 700;
  border-radius: 4px;
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
</style>

