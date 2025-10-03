<template>
  <div class="settings-section">
    <h2 class="text-xl font-semibold mb-4">{{ t('preferences.settings') }}</h2>
    
    <VaCard class="mb-4">
      <VaCardContent>
        <div class="flex items-center justify-between mb-4">
          <div>
            <div class="font-semibold">{{ t('preferences.editName') }}</div>
            <div class="text-sm text-secondary">{{ t('preferences.editNameDesc') }}</div>
          </div>
          <VaButton @click="$emit('openNameModal')">
            {{ t('common.edit') }}
          </VaButton>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard>
      <VaCardContent>
        <div class="flex items-center justify-between">
          <div>
            <div class="font-semibold">{{ t('preferences.resetPassword') }}</div>
            <div class="text-sm text-secondary">{{ t('preferences.resetPasswordDesc') }}</div>
          </div>
          <VaButton @click="$emit('openResetPasswordModal')">
            {{ t('common.change') }}
          </VaButton>
        </div>
      </VaCardContent>
    </VaCard>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

defineEmits<{
  (e: 'openNameModal'): void
  (e: 'openResetPasswordModal'): void
}>()
</script>

<style scoped>
.settings-section {
  max-width: 800px;
}
</style>

