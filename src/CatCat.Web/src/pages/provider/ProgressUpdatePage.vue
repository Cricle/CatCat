<template>
  <div v-if="loading" class="flex justify-center py-8">
    <VaProgressCircle indeterminate size="large" />
  </div>

  <div v-else-if="!order" class="text-center py-8">
    <VaIcon name="error_outline" size="large" color="danger" />
    <p class="text-secondary mt-2">订单不存在</p>
    <VaButton class="mt-4" to="/provider/tasks">返回任务列表</VaButton>
  </div>

  <div v-else>
    <!-- Header -->
    <div class="flex justify-between items-start mb-4">
      <div>
        <h1 class="page-title">服务进度更新</h1>
        <div class="flex items-center gap-2">
          <VaChip :color="getStatusColor(order.status)">{{ getStatusText(order.status) }}</VaChip>
          <span class="text-secondary">订单号: {{ order.orderNo }}</span>
        </div>
      </div>
      <VaButton preset="secondary" icon="arrow_back" @click="$router.back()">返回</VaButton>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Left: Order Info -->
      <div class="lg:col-span-2 space-y-4">
        <!-- Pet & Service Info Card -->
        <VaCard>
          <VaCardTitle>订单信息</VaCardTitle>
          <VaCardContent>
            <!-- Pet -->
            <div class="flex items-start gap-4 mb-4 pb-4 border-b">
              <VaAvatar :src="order.pet?.avatarUrl || '/default-pet.png'" size="large" />
              <div class="flex-grow">
                <h3 class="font-semibold text-lg">{{ order.pet?.name }}</h3>
                <div class="grid grid-cols-2 gap-2 mt-2 text-sm">
                  <div><span class="text-secondary">类型:</span> {{ order.pet?.type }}</div>
                  <div><span class="text-secondary">品种:</span> {{ order.pet?.breed }}</div>
                  <div><span class="text-secondary">年龄:</span> {{ order.pet?.age }}岁</div>
                  <div><span class="text-secondary">性别:</span> {{ order.pet?.gender }}</div>
                </div>
                <div v-if="order.pet?.specialInstructions" class="mt-3 p-3 bg-warning bg-opacity-10 rounded">
                  <div class="font-semibold text-sm mb-1">⚠️ 特殊说明:</div>
                  <p class="text-sm">{{ order.pet.specialInstructions }}</p>
                </div>
              </div>
            </div>

            <!-- Service Locations -->
            <div v-if="order.pet" class="mb-4 pb-4 border-b">
              <h4 class="font-semibold mb-2">📍 服务位置</h4>
              <div class="grid grid-cols-2 gap-2 text-sm">
                <div v-if="order.pet.foodLocation">
                  <span class="text-secondary">猫粮:</span> {{ order.pet.foodLocation }}
                </div>
                <div v-if="order.pet.waterLocation">
                  <span class="text-secondary">水盆:</span> {{ order.pet.waterLocation }}
                </div>
                <div v-if="order.pet.litterBoxLocation">
                  <span class="text-secondary">猫砂盆:</span> {{ order.pet.litterBoxLocation }}
                </div>
                <div v-if="order.pet.cleaningSuppliesLocation">
                  <span class="text-secondary">清洁用品:</span> {{ order.pet.cleaningSuppliesLocation }}
                </div>
              </div>
            </div>

            <!-- Service Details -->
            <div>
              <h4 class="font-semibold mb-2">📦 服务详情</h4>
              <div class="grid grid-cols-2 gap-2 text-sm">
                <div><span class="text-secondary">服务时间:</span> {{ formatDate(order.serviceDate) }} {{ order.serviceTime }}</div>
                <div><span class="text-secondary">服务地址:</span> {{ order.address }}</div>
                <div><span class="text-secondary">套餐:</span> {{ order.package?.name }}</div>
                <div><span class="text-secondary">服务时长:</span> {{ order.package?.duration }}天</div>
              </div>
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Progress Timeline -->
        <VaCard>
          <VaCardTitle>服务进度</VaCardTitle>
          <VaCardContent>
            <VaTimeline vertical>
              <VaTimelineItem
                v-for="(progress, index) in progressHistory"
                :key="index"
                :color="getProgressColor(progress.status)"
              >
                <template #before>
                  <div class="text-sm text-secondary">{{ formatDateTime(progress.updatedAt) }}</div>
                </template>
                <div>
                  <div class="font-semibold">{{ getProgressStatusText(progress.status) }}</div>
                  <p v-if="progress.notes" class="text-sm text-secondary mt-1">{{ progress.notes }}</p>
                  <div v-if="progress.photoUrls && progress.photoUrls.length > 0" class="flex gap-2 mt-2">
                    <VaImage
                      v-for="(photo, idx) in progress.photoUrls"
                      :key="idx"
                      :src="photo"
                      :alt="`照片 ${idx + 1}`"
                      class="w-20 h-20 object-cover rounded cursor-pointer"
                      @click="viewPhoto(photo)"
                    />
                  </div>
                </div>
              </VaTimelineItem>
            </VaTimeline>
          </VaCardContent>
        </VaCard>
      </div>

      <!-- Right: Update Form -->
      <div class="space-y-4">
        <!-- Quick Status Update -->
        <VaCard color="primary" gradient>
          <VaCardContent>
            <h3 class="text-white font-semibold mb-3">快速更新进度</h3>
            <div class="grid grid-cols-2 gap-2">
              <VaButton
                v-for="status in quickStatusButtons"
                :key="status.value"
                block
                size="small"
                :color="status.color"
                :disabled="!canUpdateToStatus(status.value)"
                @click="quickUpdateStatus(status.value)"
              >
                {{ status.label }}
              </VaButton>
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Update Form -->
        <VaCard>
          <VaCardTitle>更新服务进度</VaCardTitle>
          <VaCardContent>
            <div class="space-y-4">
              <VaSelect
                v-model="updateForm.status"
                label="进度状态"
                :options="progressStatusOptions"
                text-by="label"
                value-by="value"
                required
              />

              <VaTextarea
                v-model="updateForm.notes"
                label="服务备注"
                placeholder="记录服务详情、宠物状态等..."
                :min-rows="4"
              />

              <!-- Photo Upload -->
              <div>
                <label class="block mb-2 font-semibold">上传照片</label>
                <VaFileUpload
                  v-model="updateForm.photos"
                  type="gallery"
                  file-types="image/*"
                  :disabled="uploading"
                />
                <div class="text-sm text-secondary mt-1">
                  最多上传5张照片，每张不超过5MB
                </div>
              </div>

              <VaButton block color="primary" :loading="updating" @click="submitUpdate">
                <VaIcon name="cloud_upload" class="mr-1" />
                提交更新
              </VaButton>
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Complete Service -->
        <VaCard v-if="order.status === 3" color="success">
          <VaCardContent>
            <h3 class="font-semibold mb-2">完成服务</h3>
            <p class="text-sm mb-3">确认完成所有服务内容后，点击下方按钮完成订单。</p>
            <VaButton block color="success" :loading="completing" @click="completeService">
              <VaIcon name="check_circle" class="mr-1" />
              完成服务
            </VaButton>
          </VaCardContent>
        </VaCard>
      </div>
    </div>

    <!-- Photo Viewer Modal -->
    <VaModal v-model="showPhotoModal" size="large" hide-default-actions>
      <VaImage :src="currentPhoto" alt="照片" class="w-full" />
    </VaModal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vuestic-ui'
import { orderApi, progressApi } from '../../services/catcat-api'
import type { Order, ServiceProgress, OrderStatus } from '../../types/catcat-types'

const route = useRoute()
const router = useRouter()
const { init: notify } = useToast()

const orderId = route.params.id as string

const order = ref<Order | null>(null)
const progressHistory = ref<ServiceProgress[]>([])
const loading = ref(false)
const updating = ref(false)
const completing = ref(false)
const uploading = ref(false)

const showPhotoModal = ref(false)
const currentPhoto = ref('')

const updateForm = ref({
  status: 0,
  notes: '',
  photos: [] as File[],
})

const progressStatusOptions = [
  { value: 0, label: '0 - 已接单' },
  { value: 1, label: '1 - 准备中' },
  { value: 2, label: '2 - 出发中' },
  { value: 3, label: '3 - 已到达' },
  { value: 4, label: '4 - 进门服务' },
  { value: 5, label: '5 - 喂食中' },
  { value: 6, label: '6 - 换水中' },
  { value: 7, label: '7 - 铲屎中' },
  { value: 8, label: '8 - 服务完成' },
]

const quickStatusButtons = [
  { value: 1, label: '准备中', color: 'info' },
  { value: 2, label: '出发中', color: 'info' },
  { value: 3, label: '已到达', color: 'primary' },
  { value: 4, label: '进门服务', color: 'primary' },
  { value: 5, label: '喂食中', color: 'warning' },
  { value: 6, label: '换水中', color: 'warning' },
  { value: 7, label: '铲屎中', color: 'warning' },
  { value: 8, label: '完成', color: 'success' },
]

// Load order and progress
const loadOrder = async () => {
  loading.value = true
  try {
    const response = await orderApi.getById(orderId)
    order.value = response.data
  } catch (error: any) {
    notify({ message: '加载订单失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const loadProgress = async () => {
  try {
    const response = await progressApi.getByOrderId(orderId)
    progressHistory.value = response.data || []
  } catch (error: any) {
    console.error('Failed to load progress:', error)
  }
}

// Can update to status
const canUpdateToStatus = (status: number) => {
  // Can only move forward
  const currentStatus = progressHistory.value.length > 0
    ? progressHistory.value[progressHistory.value.length - 1].status
    : -1
  return status > currentStatus
}

// Quick update status
const quickUpdateStatus = async (status: number) => {
  updateForm.value.status = status
  await submitUpdate()
}

// Submit update
const submitUpdate = async () => {
  if (updateForm.value.status === null) {
    notify({ message: '请选择进度状态', color: 'warning' })
    return
  }

  updating.value = true
  try {
    // TODO: Upload photos first if any
    const photoUrls: string[] = []
    if (updateForm.value.photos.length > 0) {
      // Upload photos logic here
      // photoUrls = await uploadPhotos(updateForm.value.photos)
    }

    await progressApi.updateProgress(orderId, {
      status: updateForm.value.status,
      notes: updateForm.value.notes,
      photoUrls: photoUrls.length > 0 ? photoUrls : undefined,
    } as any)

    notify({ message: '进度更新成功！', color: 'success' })

    // Reset form
    updateForm.value = {
      status: updateForm.value.status + 1,
      notes: '',
      photos: [],
    }

    // Reload data
    await loadProgress()
    await loadOrder()
  } catch (error: any) {
    notify({ message: error.response?.data?.message || '更新失败', color: 'danger' })
  } finally {
    updating.value = false
  }
}

// Complete service
const completeService = async () => {
  if (!confirm('确认完成服务吗？完成后将无法继续更新进度。')) return

  completing.value = true
  try {
    await progressApi.updateProgress(orderId, {
      status: 8, // Service completed
      notes: '服务已完成',
    } as any)

    notify({ message: '服务已完成！', color: 'success' })
    router.push('/provider/tasks')
  } catch (error: any) {
    notify({ message: '操作失败', color: 'danger' })
  } finally {
    completing.value = false
  }
}

// View photo
const viewPhoto = (url: string) => {
  currentPhoto.value = url
  showPhotoModal.value = true
}

// Helper functions
const getStatusText = (status: OrderStatus) => {
  const map: Record<OrderStatus, string> = {
    0: '队列中',
    1: '待接单',
    2: '已接单',
    3: '服务中',
    4: '已完成',
    5: '已取消',
  }
  return map[status] || '未知'
}

const getStatusColor = (status: OrderStatus) => {
  const map: Record<OrderStatus, string> = {
    0: 'info',
    1: 'warning',
    2: 'primary',
    3: 'success',
    4: 'success',
    5: 'danger',
  }
  return map[status] || 'secondary'
}

const getProgressStatusText = (status: number) => {
  const map: Record<number, string> = {
    0: '已接单',
    1: '准备中',
    2: '出发中',
    3: '已到达',
    4: '进门服务',
    5: '喂食中',
    6: '换水中',
    7: '铲屎中',
    8: '服务完成',
  }
  return map[status] || '未知'
}

const getProgressColor = (status: number) => {
  if (status <= 2) return 'info'
  if (status <= 5) return 'primary'
  if (status <= 7) return 'warning'
  return 'success'
}

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

const formatDateTime = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('zh-CN')
}

onMounted(() => {
  loadOrder()
  loadProgress()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
}
</style>

