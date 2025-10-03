<template>
  <div v-if="loading" class="flex justify-center py-8">
    <VaProgressCircle indeterminate size="large" />
  </div>

  <div v-else-if="!order" class="text-center py-8">
    <VaIcon name="error_outline" size="large" color="danger" />
    <p class="text-secondary mt-2">订单不存在</p>
    <VaButton class="mt-4" to="/orders">返回订单列表</VaButton>
  </div>

  <div v-else>
    <!-- Header -->
    <div class="flex justify-between items-start mb-4">
      <div>
        <h1 class="page-title">订单详情</h1>
        <div class="flex items-center gap-2">
          <VaChip :color="getStatusColor(order.status)">{{ getStatusText(order.status) }}</VaChip>
          <span class="text-secondary">订单号: {{ order.orderNo }}</span>
        </div>
      </div>
      <div class="flex gap-2">
        <VaButton preset="secondary" icon="arrow_back" @click="$router.back()">返回</VaButton>
        <VaButton
          v-if="canCancelOrder(order.status)"
          color="danger"
          icon="cancel"
          :loading="cancelling"
          @click="handleCancel"
        >
          取消订单
        </VaButton>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Left Column: Order Info -->
      <div class="lg:col-span-2 space-y-4">
        <!-- Pet Info -->
        <VaCard>
          <VaCardTitle>
            <div class="flex items-center gap-2">
              <VaIcon name="pets" />
              宠物信息
            </div>
          </VaCardTitle>
          <VaCardContent>
            <div v-if="order.pet" class="flex items-start gap-4">
              <VaAvatar :src="order.pet.avatarUrl || '/default-pet.png'" size="large" />
              <div class="flex-grow">
                <h3 class="font-semibold text-lg">{{ order.pet.name }}</h3>
                <div class="grid grid-cols-2 gap-2 mt-2 text-sm">
                  <div><span class="text-secondary">类型:</span> {{ order.pet.type }}</div>
                  <div><span class="text-secondary">品种:</span> {{ order.pet.breed }}</div>
                  <div><span class="text-secondary">年龄:</span> {{ order.pet.age }}岁</div>
                  <div><span class="text-secondary">性别:</span> {{ order.pet.gender }}</div>
                </div>
                <div v-if="order.pet.specialInstructions" class="mt-3 p-3 bg-warning bg-opacity-10 rounded">
                  <div class="font-semibold text-sm mb-1">特殊说明:</div>
                  <p class="text-sm">{{ order.pet.specialInstructions }}</p>
                </div>
              </div>
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Package Info -->
        <VaCard>
          <VaCardTitle>
            <div class="flex items-center gap-2">
              <VaIcon name="business_center" />
              服务套餐
            </div>
          </VaCardTitle>
          <VaCardContent>
            <div v-if="order.package">
              <div class="flex justify-between items-start mb-3">
                <div>
                  <h3 class="font-semibold text-lg">{{ order.package.name }}</h3>
                  <p class="text-secondary text-sm mt-1">{{ order.package.description }}</p>
                </div>
                <div class="text-right">
                  <div class="text-2xl font-bold text-primary">¥{{ order.package.price }}</div>
                </div>
              </div>
              <div class="grid grid-cols-3 gap-4 mt-4">
                <div class="text-center p-3 bg-primary bg-opacity-10 rounded">
                  <div class="text-2xl font-bold">{{ order.package.duration }}</div>
                  <div class="text-sm text-secondary">天数</div>
                </div>
                <div class="text-center p-3 bg-success bg-opacity-10 rounded">
                  <div class="text-2xl font-bold">{{ order.package.visitsPerDay }}</div>
                  <div class="text-sm text-secondary">次/天</div>
                </div>
                <div class="text-center p-3 bg-info bg-opacity-10 rounded">
                  <div class="text-2xl font-bold">{{ order.package.minutesPerVisit }}</div>
                  <div class="text-sm text-secondary">分钟/次</div>
                </div>
              </div>
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Service Progress -->
        <VaCard v-if="order.status >= 2">
          <VaCardTitle>
            <div class="flex items-center gap-2">
              <VaIcon name="timeline" />
              服务进度
            </div>
          </VaCardTitle>
          <VaCardContent>
            <div v-if="loadingProgress" class="flex justify-center py-4">
              <VaProgressCircle indeterminate />
            </div>
            <div v-else-if="progressList.length === 0" class="text-center py-4 text-secondary">暂无服务记录</div>
            <div v-else class="space-y-4">
              <VaTimeline vertical>
                <VaTimelineItem v-for="progress in progressList" :key="progress.id" :color="getProgressColor(progress.status)">
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
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Review Section -->
        <VaCard v-if="order.status === 4">
          <VaCardTitle>
            <div class="flex items-center gap-2">
              <VaIcon name="star" />
              {{ order.review ? '我的评价' : '服务评价' }}
            </div>
          </VaCardTitle>
          <VaCardContent>
            <div v-if="order.review">
              <VaRating v-model="order.review.rating" readonly />
              <p class="mt-2">{{ order.review.comment }}</p>
              <div class="text-sm text-secondary mt-1">{{ formatDate(order.review.createdAt) }}</div>
            </div>
            <div v-else>
              <VaButton color="primary" @click="showReviewModal = true">写评价</VaButton>
            </div>
          </VaCardContent>
        </VaCard>
      </div>

      <!-- Right Column: Summary & Actions -->
      <div class="space-y-4">
        <!-- Order Summary -->
        <VaCard color="background-border">
          <VaCardTitle>订单摘要</VaCardTitle>
          <VaCardContent>
            <div class="space-y-3">
              <div class="flex items-center gap-2">
                <VaIcon name="event" size="small" />
                <div class="flex-grow">
                  <div class="text-sm text-secondary">服务日期</div>
                  <div class="font-semibold">{{ formatDate(order.serviceDate) }}</div>
                </div>
              </div>
              <div class="flex items-center gap-2">
                <VaIcon name="schedule" size="small" />
                <div class="flex-grow">
                  <div class="text-sm text-secondary">服务时间</div>
                  <div class="font-semibold">{{ order.serviceTime }}</div>
                </div>
              </div>
              <div class="flex items-center gap-2">
                <VaIcon name="location_on" size="small" />
                <div class="flex-grow">
                  <div class="text-sm text-secondary">服务地址</div>
                  <div class="font-semibold">{{ order.address }}</div>
                </div>
              </div>
              <VaDivider />
              <div class="flex justify-between items-center">
                <span class="text-secondary">订单金额</span>
                <span class="text-2xl font-bold text-primary">¥{{ order.totalAmount.toFixed(2) }}</span>
              </div>
            </div>
          </VaCardContent>
        </VaCard>

        <!-- Provider Info (if assigned) -->
        <VaCard v-if="order.provider" color="background-border">
          <VaCardTitle>服务人员</VaCardTitle>
          <VaCardContent>
            <div class="flex items-center gap-3">
              <VaAvatar :src="order.provider.avatarUrl" />
              <div>
                <div class="font-semibold">{{ order.provider.name }}</div>
                <VaRating :model-value="order.provider.rating || 5" readonly size="small" />
              </div>
            </div>
            <VaButton block class="mt-3" preset="secondary" icon="phone">联系TA</VaButton>
          </VaCardContent>
        </VaCard>

        <!-- Order Timeline -->
        <VaCard color="background-border">
          <VaCardTitle>订单记录</VaCardTitle>
          <VaCardContent>
            <div class="space-y-2 text-sm">
              <div>
                <div class="text-secondary">创建时间</div>
                <div>{{ formatDateTime(order.createdAt) }}</div>
              </div>
              <div v-if="order.confirmedAt">
                <div class="text-secondary">确认时间</div>
                <div>{{ formatDateTime(order.confirmedAt) }}</div>
              </div>
              <div v-if="order.completedAt">
                <div class="text-secondary">完成时间</div>
                <div>{{ formatDateTime(order.completedAt) }}</div>
              </div>
            </div>
          </VaCardContent>
        </VaCard>
      </div>
    </div>

    <!-- Review Modal -->
    <VaModal v-model="showReviewModal" title="服务评价" size="medium" @ok="submitReview">
      <div class="space-y-4">
        <div>
          <label class="block mb-2">评分</label>
          <VaRating v-model="reviewForm.rating" />
        </div>
        <VaTextarea
          v-model="reviewForm.comment"
          label="评价内容"
          placeholder="分享您的服务体验..."
          :min-rows="4"
          required
        />
      </div>
    </VaModal>

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
import { orderApi, progressApi, reviewApi } from '../../services/catcat-api'
import type { Order, ServiceProgress, OrderStatus } from '../../types/catcat-types'

const route = useRoute()
const router = useRouter()
const { init: notify } = useToast()

const orderId = route.params.id as string

const order = ref<Order | null>(null)
const progressList = ref<ServiceProgress[]>([])
const loading = ref(false)
const loadingProgress = ref(false)
const cancelling = ref(false)

const showReviewModal = ref(false)
const showPhotoModal = ref(false)
const currentPhoto = ref('')

const reviewForm = ref({
  rating: 5,
  comment: '',
})

// Load order
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

// Load progress
const loadProgress = async () => {
  loadingProgress.value = true
  try {
    const response = await progressApi.getByOrderId(orderId)
    progressList.value = response.data || []
  } catch (error: any) {
    console.error('Failed to load progress:', error)
  } finally {
    loadingProgress.value = false
  }
}

// Cancel order
const handleCancel = async () => {
  if (!confirm('确定要取消订单吗？')) return

  cancelling.value = true
  try {
    await orderApi.cancelOrder(orderId, '用户主动取消')
    notify({ message: '订单已取消', color: 'success' })
    await loadOrder()
  } catch (error: any) {
    notify({ message: '取消订单失败', color: 'danger' })
  } finally {
    cancelling.value = false
  }
}

// Submit review
const submitReview = async () => {
  try {
    await reviewApi.create({
      orderId: orderId,
      rating: reviewForm.value.rating,
      comment: reviewForm.value.comment,
    })
    notify({ message: '评价成功！', color: 'success' })
    showReviewModal.value = false
    await loadOrder()
  } catch (error: any) {
    notify({ message: '提交评价失败', color: 'danger' })
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

const canCancelOrder = (status: OrderStatus) => {
  return [0, 1, 2].includes(status)
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

