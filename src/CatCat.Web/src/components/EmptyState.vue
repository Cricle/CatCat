<template>
  <div class="empty-state">
    <div class="empty-state-content">
      <!-- Icon -->
      <div class="empty-state-icon">
        <VaIcon
          :name="icon"
          :size="iconSize"
          :color="iconColor"
        />
      </div>

      <!-- Title -->
      <h3 class="empty-state-title">{{ title }}</h3>

      <!-- Description -->
      <p v-if="description" class="empty-state-description">
        {{ description }}
      </p>

      <!-- Action Button -->
      <VaButton
        v-if="actionText"
        class="empty-state-action"
        :icon="actionIcon"
        :color="actionColor"
        @click="$emit('action')"
      >
        {{ actionText }}
      </VaButton>

      <!-- Secondary Action -->
      <VaButton
        v-if="secondaryActionText"
        preset="secondary"
        class="empty-state-secondary-action"
        @click="$emit('secondary-action')"
      >
        {{ secondaryActionText }}
      </VaButton>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  icon?: string
  iconSize?: string | number
  iconColor?: string
  title: string
  description?: string
  actionText?: string
  actionIcon?: string
  actionColor?: string
  secondaryActionText?: string
}

withDefaults(defineProps<Props>(), {
  icon: 'inbox',
  iconSize: '5rem',
  iconColor: 'secondary',
  actionColor: 'primary',
  actionIcon: 'add',
})

defineEmits<{
  (e: 'action'): void
  (e: 'secondary-action'): void
}>()
</script>

<style scoped>
.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  padding: 3rem 1rem;
}

.empty-state-content {
  max-width: 400px;
  text-align: center;
}

.empty-state-icon {
  margin-bottom: 1.5rem;
  opacity: 0.6;
}

.empty-state-title {
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: var(--va-text-primary);
}

.empty-state-description {
  color: var(--va-text-secondary);
  margin-bottom: 1.5rem;
  line-height: 1.6;
}

.empty-state-action {
  margin-top: 1rem;
}

.empty-state-secondary-action {
  margin-left: 0.5rem;
}

@media (max-width: 640px) {
  .empty-state {
    min-height: 250px;
  }

  .empty-state-title {
    font-size: 1.25rem;
  }

  .empty-state-icon {
    font-size: 4rem !important;
  }
}
</style>

