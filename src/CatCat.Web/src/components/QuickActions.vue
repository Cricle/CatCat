<template>
  <VaCard class="quick-actions-card">
    <VaCardTitle class="quick-actions-title">
      <VaIcon name="flash_on" size="small" />
      {{ t('quickActions.title') }}
    </VaCardTitle>
    <VaCardContent>
      <div class="actions-grid">
        <div
          v-for="action in actions"
          :key="action.to"
          class="action-item"
          @click="$router.push(action.to)"
        >
          <div :class="['action-icon-wrapper', `action-${action.color}`]">
            <VaIcon :name="action.icon" size="1.5rem" />
          </div>
          <div class="action-label">{{ t(action.label) }}</div>
        </div>
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
.quick-actions-card {
  border: none;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.quick-actions-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

@media (min-width: 768px) {
  .actions-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

.action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 1.25rem;
  border-radius: 0.75rem;
  background: var(--va-background-element);
  border: 1px solid var(--va-background-border);
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-item:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.08);
  border-color: var(--va-primary);
}

.action-icon-wrapper {
  width: 3rem;
  height: 3rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.75rem;
  transition: all 0.3s ease;
}

.action-primary {
  background: rgba(var(--va-primary-rgb), 0.1);
  color: var(--va-primary);
}

.action-success {
  background: rgba(var(--va-success-rgb), 0.1);
  color: var(--va-success);
}

.action-info {
  background: rgba(var(--va-info-rgb), 0.1);
  color: var(--va-info);
}

.action-warning {
  background: rgba(var(--va-warning-rgb), 0.1);
  color: var(--va-warning);
}

.action-danger {
  background: rgba(var(--va-danger-rgb), 0.1);
  color: var(--va-danger);
}

.action-label {
  font-size: 0.875rem;
  font-weight: 500;
  text-align: center;
  color: var(--va-text-primary);
}
</style>

