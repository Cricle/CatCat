<template>
  <div class="image-uploader">
    <div class="upload-area" @click="triggerFileInput">
      <div v-if="!previewUrl" class="upload-placeholder">
        <VaIcon name="add_photo_alternate" size="3rem" color="secondary" />
        <p class="upload-text">{{ t('imageUploader.clickToUpload') }}</p>
        <p class="upload-hint">{{ t('imageUploader.supportedFormats') }}</p>
      </div>
      <div v-else class="image-preview">
        <img :src="previewUrl" :alt="t('imageUploader.preview')" />
        <div class="image-overlay">
          <VaButton preset="plain" icon="edit" color="white" @click.stop="triggerFileInput" />
          <VaButton preset="plain" icon="delete" color="white" @click.stop="removeImage" />
        </div>
      </div>
    </div>
    <input
      ref="fileInput"
      type="file"
      accept="image/*"
      style="display: none"
      @change="handleFileChange"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'

interface Props {
  modelValue?: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const { t } = useI18n()
const { init: notify } = useToast()

const fileInput = ref<HTMLInputElement>()
const previewUrl = ref<string>(props.modelValue || '')

watch(() => props.modelValue, (newValue) => {
  previewUrl.value = newValue || ''
})

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (!file) return

  // Validate file type
  if (!file.type.startsWith('image/')) {
    notify({ message: t('imageUploader.invalidFormat'), color: 'danger' })
    return
  }

  // Validate file size (max 5MB)
  if (file.size > 5 * 1024 * 1024) {
    notify({ message: t('imageUploader.fileTooLarge'), color: 'danger' })
    return
  }

  // Create preview
  const reader = new FileReader()
  reader.onload = (e) => {
    const result = e.target?.result as string
    previewUrl.value = result
    emit('update:modelValue', result)
  }
  reader.readAsDataURL(file)
}

const removeImage = () => {
  previewUrl.value = ''
  emit('update:modelValue', '')
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}
</script>

<style scoped>
.image-uploader {
  width: 100%;
}

.upload-area {
  width: 100%;
  aspect-ratio: 1;
  max-width: 300px;
  margin: 0 auto;
  border: 2px dashed var(--va-background-border);
  border-radius: 0.75rem;
  cursor: pointer;
  transition: all 0.3s ease;
  overflow: hidden;
}

.upload-area:hover {
  border-color: var(--va-primary);
  background: var(--va-background-element);
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 2rem;
  text-align: center;
}

.upload-text {
  margin: 1rem 0 0.5rem;
  font-weight: 600;
  color: var(--va-text-primary);
}

.upload-hint {
  font-size: 0.875rem;
  color: var(--va-text-secondary);
  margin: 0;
}

.image-preview {
  position: relative;
  width: 100%;
  height: 100%;
}

.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.image-preview:hover .image-overlay {
  opacity: 1;
}
</style>

