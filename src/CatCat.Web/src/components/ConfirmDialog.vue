<template>
  <VaModal
    v-model="isVisible"
    :title="title"
    :size="size"
    :no-outside-dismiss="!dismissible"
    :no-esc-dismiss="!dismissible"
  >
    <div class="confirm-dialog-content">
      <!-- Icon -->
      <div v-if="icon" class="confirm-dialog-icon">
        <VaIcon :name="icon" :size="iconSize" :color="iconColor" />
      </div>

      <!-- Message -->
      <div class="confirm-dialog-message">
        <p class="text-base">{{ message }}</p>
        <p v-if="detail" class="text-sm text-secondary mt-2">
          {{ detail }}
        </p>
      </div>

      <!-- Additional Content (slot) -->
      <div v-if="$slots.default" class="confirm-dialog-slot mt-4">
        <slot />
      </div>
    </div>

    <template #footer>
      <div class="flex gap-2 justify-end">
        <VaButton
          preset="secondary"
          :disabled="loading"
          @click="handleCancel"
        >
          {{ cancelText }}
        </VaButton>
        <VaButton
          :color="confirmColor"
          :loading="loading"
          @click="handleConfirm"
        >
          {{ confirmText }}
        </VaButton>
      </div>
    </template>
  </VaModal>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

interface Props {
  modelValue: boolean
  title?: string
  message: string
  detail?: string
  icon?: string
  iconSize?: string | number
  iconColor?: string
  confirmText?: string
  cancelText?: string
  confirmColor?: string
  size?: 'small' | 'medium' | 'large'
  dismissible?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: '确认操作',
  confirmText: '确认',
  cancelText: '取消',
  confirmColor: 'primary',
  size: 'small',
  dismissible: true,
  icon: 'help',
  iconSize: '3rem',
  iconColor: 'warning',
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'confirm'): void | Promise<void>
  (e: 'cancel'): void
}>()

const loading = ref(false)

const isVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

const handleConfirm = async () => {
  loading.value = true
  try {
    await emit('confirm')
    isVisible.value = false
  } catch (error) {
    console.error('Confirm action failed:', error)
  } finally {
    loading.value = false
  }
}

const handleCancel = () => {
  emit('cancel')
  isVisible.value = false
}
</script>

<style scoped>
.confirm-dialog-content {
  padding: 1rem 0;
}

.confirm-dialog-icon {
  display: flex;
  justify-content: center;
  margin-bottom: 1rem;
}

.confirm-dialog-message {
  text-align: center;
}
</style>

