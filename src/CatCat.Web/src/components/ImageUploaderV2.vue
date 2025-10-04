<template>
  <div class="image-uploader-v2">
    <!-- Preview + Upload Button -->
    <div class="upload-container">
      <!-- Image Preview -->
      <div
        v-if="previewUrl"
        class="image-preview"
        @click="handlePreviewClick"
      >
        <img :src="previewUrl" :alt="label || 'Preview'" />

        <!-- Overlay with actions -->
        <div class="image-overlay">
          <VaButton
            preset="plain"
            icon="visibility"
            color="white"
            @click.stop="showFullPreview = true"
          />
          <VaButton
            preset="plain"
            icon="delete"
            color="white"
            @click.stop="handleRemove"
          />
        </div>

        <!-- Upload Progress -->
        <div v-if="uploading" class="upload-progress">
          <VaProgressCircle
            :model-value="uploadProgress"
            :thickness="0.1"
            size="large"
          >
            {{ uploadProgress }}%
          </VaProgressCircle>
        </div>
      </div>

      <!-- Upload Area -->
      <div
        v-else
        class="upload-area"
        :class="{ 'upload-area-dragging': isDragging }"
        @click="triggerFileInput"
        @drop.prevent="handleDrop"
        @dragover.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
      >
        <VaIcon name="cloud_upload" size="3rem" color="secondary" />
        <p class="upload-text mt-2">{{ t('imageUploader.clickOrDrag') }}</p>
        <p class="upload-hint text-sm text-secondary mt-1">
          {{ hint || `${t('imageUploader.supportedFormats')}: JPG, PNG, GIF (${maxSizeMB}MB ${t('imageUploader.max')})` }}
        </p>
      </div>

      <!-- Hidden File Input -->
      <input
        ref="fileInputRef"
        type="file"
        :accept="accept"
        class="hidden"
        @change="handleFileChange"
      />
    </div>

    <!-- Error Message -->
    <div v-if="errorMessage" class="error-message mt-2">
      <VaIcon name="error" size="small" color="danger" />
      <span class="text-sm text-danger ml-1">{{ errorMessage }}</span>
    </div>

    <!-- Full Preview Modal -->
    <VaModal
      v-model="showFullPreview"
      title="图片预览"
      size="large"
      hide-default-actions
    >
      <div class="full-preview">
        <img :src="previewUrl" alt="Full Preview" />
      </div>
    </VaModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'

interface Props {
  modelValue?: string
  label?: string
  hint?: string
  accept?: string
  maxSizeMB?: number
  compress?: boolean
  compressQuality?: number
}

const props = withDefaults(defineProps<Props>(), {
  accept: 'image/jpeg,image/png,image/gif',
  maxSizeMB: 5,
  compress: true,
  compressQuality: 0.8,
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
  (e: 'error', message: string): void
}>()

const { t } = useI18n()

const fileInputRef = ref<HTMLInputElement>()
const previewUrl = ref<string>()
const isDragging = ref(false)
const uploading = ref(false)
const uploadProgress = ref(0)
const errorMessage = ref('')
const showFullPreview = ref(false)

// Watch modelValue changes
watch(() => props.modelValue, (newValue) => {
  if (newValue && newValue !== previewUrl.value) {
    previewUrl.value = newValue
  }
}, { immediate: true })

const triggerFileInput = () => {
  fileInputRef.value?.click()
}

const handleFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    processFile(file)
  }
}

const handleDrop = (event: DragEvent) => {
  isDragging.value = false
  const file = event.dataTransfer?.files[0]
  if (file) {
    processFile(file)
  }
}

const processFile = async (file: File) => {
  errorMessage.value = ''

  // Validate file type
  if (!file.type.startsWith('image/')) {
    errorMessage.value = t('imageUploader.invalidFormat')
    emit('error', errorMessage.value)
    return
  }

  // Validate file size
  const fileSizeMB = file.size / 1024 / 1024
  if (fileSizeMB > props.maxSizeMB) {
    errorMessage.value = t('imageUploader.fileTooLarge', { max: props.maxSizeMB })
    emit('error', errorMessage.value)
    return
  }

  uploading.value = true
  uploadProgress.value = 0

  try {
    // Simulate upload progress
    const progressInterval = setInterval(() => {
      if (uploadProgress.value < 90) {
        uploadProgress.value += 10
      }
    }, 100)

    // Compress image if needed
    let processedFile = file
    if (props.compress && fileSizeMB > 1) {
      processedFile = await compressImage(file, props.compressQuality)
    }

    // Convert to Base64
    const base64 = await fileToBase64(processedFile)

    clearInterval(progressInterval)
    uploadProgress.value = 100

    // Set preview and emit value
    previewUrl.value = base64
    emit('update:modelValue', base64)

    setTimeout(() => {
      uploading.value = false
      uploadProgress.value = 0
    }, 500)
  } catch (error) {
    console.error('Failed to process image:', error)
    errorMessage.value = t('imageUploader.uploadFailed')
    emit('error', errorMessage.value)
    uploading.value = false
  }
}

const compressImage = (file: File, quality: number): Promise<File> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.readAsDataURL(file)
    reader.onload = (e) => {
      const img = new Image()
      img.src = e.target?.result as string
      img.onload = () => {
        const canvas = document.createElement('canvas')
        const ctx = canvas.getContext('2d')!

        // Calculate new dimensions (max 1920px)
        let width = img.width
        let height = img.height
        const maxDimension = 1920

        if (width > maxDimension || height > maxDimension) {
          if (width > height) {
            height = (height / width) * maxDimension
            width = maxDimension
          } else {
            width = (width / height) * maxDimension
            height = maxDimension
          }
        }

        canvas.width = width
        canvas.height = height

        // Draw and compress
        ctx.drawImage(img, 0, 0, width, height)
        canvas.toBlob(
          (blob) => {
            if (blob) {
              const compressedFile = new File([blob], file.name, {
                type: 'image/jpeg',
                lastModified: Date.now(),
              })
              resolve(compressedFile)
            } else {
              reject(new Error('Failed to compress image'))
            }
          },
          'image/jpeg',
          quality
        )
      }
      img.onerror = reject
    }
    reader.onerror = reject
  })
}

const fileToBase64 = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.readAsDataURL(file)
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = reject
  })
}

const handleRemove = () => {
  previewUrl.value = undefined
  errorMessage.value = ''
  emit('update:modelValue', '')
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

const handlePreviewClick = () => {
  // Do nothing - click is handled by overlay buttons
}
</script>

<style scoped>
.image-uploader-v2 {
  width: 100%;
}

.upload-container {
  position: relative;
}

.upload-area {
  border: 2px dashed var(--va-background-border);
  border-radius: 0.75rem;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  background: var(--va-background-element);
}

.upload-area:hover {
  border-color: var(--va-primary);
  background: var(--va-background-primary);
}

.upload-area-dragging {
  border-color: var(--va-primary);
  background: var(--va-primary-alpha-10);
}

.upload-text {
  font-weight: 500;
  color: var(--va-text-primary);
}

.upload-hint {
  font-size: 0.875rem;
}

.image-preview {
  position: relative;
  border-radius: 0.75rem;
  overflow: hidden;
  cursor: pointer;
  max-height: 300px;
}

.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
  background: var(--va-background-element);
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

.upload-progress {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: rgba(0, 0, 0, 0.8);
  padding: 1rem;
  border-radius: 0.5rem;
}

.error-message {
  display: flex;
  align-items: center;
}

.full-preview {
  max-height: 80vh;
  overflow: auto;
}

.full-preview img {
  width: 100%;
  height: auto;
}

.hidden {
  display: none;
}
</style>

