<template>
  <VaCard>
    <VaCardTitle>
      <div class="flex items-center gap-2">
        <VaIcon name="flash_on" />
        <span>{{ t('quickActions.title') }}</span>
      </div>
    </VaCardTitle>
    <VaCardContent>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <VaButton
          v-for="action in actions"
          :key="action.to"
          :to="action.to"
          size="large"
          :color="action.color"
          class="flex-col h-24 action-button"
          :icon="action.icon"
        >
          {{ t(action.label) }}
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '../stores/user-store'

const { t } = useI18n()
const userStore = useUserStore()

const actions = computed(() => {
  const baseActions = [
    {
      to: '/orders/create',
      icon: 'add_circle',
      label: 'quickActions.createOrder',
      color: 'primary',
    },
    {
      to: '/pets',
      icon: 'pets',
      label: 'quickActions.myPets',
      color: 'success',
    },
    {
      to: '/packages',
      icon: 'inventory_2',
      label: 'quickActions.browsePackages',
      color: 'info',
    },
    {
      to: '/orders',
      icon: 'receipt_long',
      label: 'quickActions.myOrders',
      color: 'warning',
    },
  ]

  // Add provider actions if user is a provider
  if (userStore.user?.role === 2) {
    baseActions.push({
      to: '/provider/available',
      icon: 'work',
      label: 'quickActions.availableOrders',
      color: 'primary',
    })
  }

  // Add admin actions if user is admin
  if (userStore.user?.role === 99) {
    baseActions.push({
      to: '/admin/users',
      icon: 'admin_panel_settings',
      label: 'quickActions.manageUsers',
      color: 'danger',
    })
  }

  return baseActions.slice(0, 4)
})
</script>

<style scoped>
.action-button {
  transition: all 0.3s ease;
}

.action-button:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}
</style>

