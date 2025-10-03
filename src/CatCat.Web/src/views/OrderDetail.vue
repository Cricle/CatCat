<template>
  <div class="order-detail-page">
    <va-card>
      <va-card-title>
        <div class="page-header">
          <va-button preset="plain" icon="arrow_back" @click="$router.back()" />
          <h1 class="va-h1">Order Details</h1>
          <va-button v-if="order && (order.status === 2 || order.status === 3)" preset="plain" icon="refresh" @click="refreshProgress">
            Refresh
          </va-button>
        </div>
      </va-card-title>

      <va-card-content>
        <!-- Loading State -->
        <div v-if="loading" class="loading-state">
          <va-skeleton height="120px" class="mb-3" />
          <va-skeleton height="200px" class="mb-3" />
          <va-skeleton height="100px" />
        </div>

        <!-- Order Content -->
        <template v-else-if="order">
          <!-- Status Banner -->
          <va-card :color="getStatusColor(order.status)" gradient class="status-banner mb-4">
            <va-card-content>
              <div class="status-content">
                <va-icon :name="getStatusIcon(order.status)" size="48px" color="white" />
                <div class="status-info">
                  <va-chip :color="getStatusColor(order.status)" size="large">
                    {{ getStatusText(order.status) }}
                  </va-chip>
                  <div class="order-no">Order No: {{ order.orderNo }}</div>
                  <div v-if="order.status === 0" class="processing-tip">
                    <va-progress-circle indeterminate size="small" />
                    <span>Processing...</span>
                  </div>
                </div>
              </div>
            </va-card-content>
          </va-card>

          <!-- Service Progress (Real-time Tracking) -->
          <va-card v-if="(order.status === 2 || order.status === 3) && progressList.length > 0" class="progress-section mb-4">
            <va-card-title>
              <va-icon name="timeline" /> Service Progress
            </va-card-title>
            <va-card-content>
              <!-- Latest Location Map (Placeholder) -->
              <div v-if="latestProgress && latestProgress.latitude && latestProgress.longitude" class="map-container mb-4">
                <div class="map-placeholder">
                  <va-icon name="location_on" size="48px" color="danger" />
                  <p class="va-text-secondary">Location: {{ latestProgress.address || 'Service in progress' }}</p>
                  <p class="coordinates">
                    {{ latestProgress.latitude.toFixed(6) }}, {{ latestProgress.longitude.toFixed(6) }}
                  </p>
                  <va-button size="small" @click="openMap(latestProgress.latitude!, latestProgress.longitude!)">
                    <va-icon name="map" /> View on Map
                  </va-button>
                </div>
              </div>

              <!-- Progress Timeline -->
              <va-timeline>
                <va-timeline-item
                  v-for="progress in progressList"
                  :key="progress.id"
                  :color="getProgressStatusColor(progress.status)"
                  :active="progress.id === progressList[0].id"
                >
                  <template #before>
                    <va-icon :name="getProgressStatusIcon(progress.status)" />
                  </template>

                  <div class="timeline-content">
                    <div class="timeline-header">
                      <strong>{{ getProgressStatusText(progress.status) }}</strong>
                      <span class="timeline-time">{{ formatTime(progress.createdAt) }}</span>
                    </div>
                    <p v-if="progress.description" class="timeline-description">{{ progress.description }}</p>
                    
                    <!-- Service Photos -->
                    <div v-if="progress.imageUrls" class="progress-images">
                      <va-image
                        v-for="(url, index) in JSON.parse(progress.imageUrls)"
                        :key="index"
                        :src="url"
                        class="progress-image"
                      />
                    </div>
                  </div>
                </va-timeline-item>
              </va-timeline>
            </va-card-content>
          </va-card>

          <!-- Service Information -->
          <va-card class="info-section mb-4">
            <va-card-title>
              <va-icon name="info" /> Service Information
            </va-card-title>
            <va-card-content>
              <va-list>
                <va-list-item>
                  <va-list-item-section icon>
                    <va-icon name="calendar_today" color="primary" />
                  </va-list-item-section>
                  <va-list-item-section>
                    <va-list-item-label caption>Service Date & Time</va-list-item-label>
                    <va-list-item-label>
                      {{ order.serviceDate }} {{ order.serviceTime }}
                    </va-list-item-label>
                  </va-list-item-section>
                </va-list-item>

                <va-list-separator />

                <va-list-item>
                  <va-list-item-section icon>
                    <va-icon name="location_on" color="danger" />
                  </va-list-item-section>
                  <va-list-item-section>
                    <va-list-item-label caption>Service Address</va-list-item-label>
                    <va-list-item-label>{{ order.address }}</va-list-item-label>
                    <va-list-item-label v-if="order.addressDetail" caption>
                      {{ order.addressDetail }}
                    </va-list-item-label>
                  </va-list-item-section>
                </va-list-item>
              </va-list>
            </va-card-content>
          </va-card>

          <!-- Payment Information -->
          <va-card class="info-section mb-4">
            <va-card-title>
              <va-icon name="payments" /> Payment Information
            </va-card-title>
            <va-card-content>
              <div class="payment-row">
                <span>Service Fee:</span>
                <strong class="price">¥{{ order.price }}</strong>
              </div>
            </va-card-content>
          </va-card>

          <!-- Customer Remarks -->
          <va-card v-if="order.customerRemark" class="info-section mb-4">
            <va-card-title>
              <va-icon name="comment" /> Customer Remarks
            </va-card-title>
            <va-card-content>
              <p class="remarks-text">{{ order.customerRemark }}</p>
            </va-card-content>
          </va-card>

          <!-- Order Information -->
          <va-card class="info-section mb-4">
            <va-card-title>
              <va-icon name="receipt_long" /> Order Information
            </va-card-title>
            <va-card-content>
              <va-list>
                <va-list-item>
                  <va-list-item-section icon>
                    <va-icon name="schedule" />
                  </va-list-item-section>
                  <va-list-item-section>
                    <va-list-item-label caption>Created At</va-list-item-label>
                    <va-list-item-label>{{ formatDateTime(order.createdAt) }}</va-list-item-label>
                  </va-list-item-section>
                </va-list-item>

                <va-list-separator v-if="order.serviceProviderId" />

                <va-list-item v-if="order.serviceProviderId">
                  <va-list-item-section icon>
                    <va-icon name="person" />
                  </va-list-item-section>
                  <va-list-item-section>
                    <va-list-item-label caption>Service Provider ID</va-list-item-label>
                    <va-list-item-label>{{ order.serviceProviderId }}</va-list-item-label>
                  </va-list-item-section>
                </va-list-item>
              </va-list>
            </va-card-content>
          </va-card>

          <!-- Action Buttons -->
          <div v-if="order.status === 0 || order.status === 1" class="action-buttons">
            <va-button
              block
              color="danger"
              :loading="cancelling"
              @click="handleCancelOrder"
            >
              <va-icon name="cancel" /> Cancel Order
            </va-button>
          </div>
        </template>

        <!-- Error State -->
        <va-card v-else class="empty-state">
          <va-card-content>
            <div class="text-center">
              <va-icon name="error" size="64px" color="danger" />
              <h3 class="va-h3">Order Not Found</h3>
              <p class="va-text-secondary">The order you're looking for doesn't exist.</p>
              <va-button color="primary" @click="$router.back()">Go Back</va-button>
            </div>
          </va-card-content>
        </va-card>
      </va-card-content>
    </va-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getOrderDetail, cancelOrder } from '@/api/orders'
import { getOrderProgress, getProgressStatusText, getProgressStatusIcon, getProgressStatusColor } from '@/api/progress'
import type { Order } from '@/api/orders'
import type { ServiceProgress } from '@/api/progress'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()
const router = useRouter()
const route = useRoute()

const loading = ref(false)
const cancelling = ref(false)
const order = ref<Order>()
const progressList = ref<ServiceProgress[]>([])
const latestProgress = computed(() => progressList.value[0])

let refreshInterval: number | null = null

const getStatusColor = (status: number) => {
  const colors: Record<number, string> = {
    0: 'info',
    1: 'warning',
    2: 'primary',
    3: 'info',
    4: 'success',
    5: 'danger'
  }
  return colors[status] || 'secondary'
}

const getStatusText = (status: number) => {
  const texts: Record<number, string> = {
    0: 'Queued',
    1: 'Pending',
    2: 'Accepted',
    3: 'In Service',
    4: 'Completed',
    5: 'Cancelled'
  }
  return texts[status] || 'Unknown'
}

const getStatusIcon = (status: number) => {
  const icons: Record<number, string> = {
    0: 'schedule',
    1: 'pending',
    2: 'check_circle',
    3: 'local_shipping',
    4: 'done_all',
    5: 'cancel'
  }
  return icons[status] || 'info'
}

const formatDateTime = (dateTime: string) => {
  if (!dateTime) return '-'
  return new Date(dateTime).toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatTime = (dateTime: string) => {
  if (!dateTime) return '-'
  return new Date(dateTime).toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fetchOrderDetail = async () => {
  const orderId = route.params.id as string
  if (!orderId) {
    notify({ message: 'Invalid order ID', color: 'danger' })
    router.back()
    return
  }

  loading.value = true
  try {
    const res = await getOrderDetail(Number(orderId))
    order.value = res.data
    
    // Fetch progress if order is in service
    if (order.value.status === 2 || order.value.status === 3) {
      await fetchProgress()
    }
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load order', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const fetchProgress = async () => {
  if (!order.value) return
  
  try {
    const res = await getOrderProgress(order.value.id)
    progressList.value = res.data
  } catch (error: any) {
    console.error('Failed to fetch progress:', error)
  }
}

const refreshProgress = async () => {
  await fetchProgress()
  notify({ message: 'Progress updated', color: 'info', duration: 1000 })
}

const openMap = (lat: number, lng: number) => {
  // Open in Google Maps (or other map services)
  const url = `https://www.google.com/maps?q=${lat},${lng}`
  window.open(url, '_blank')
}

const handleCancelOrder = async () => {
  if (!order.value) return

  const agreed = await confirm({
    title: 'Cancel Order',
    message: 'Are you sure you want to cancel this order?',
    okText: 'Yes, Cancel',
    cancelText: 'No'
  })

  if (agreed) {
    cancelling.value = true
    try {
      await cancelOrder(order.value.id, 'Cancelled by user')
      notify({ message: 'Order cancelled successfully', color: 'success' })
      fetchOrderDetail()
    } catch (error: any) {
      notify({ message: error.message || 'Failed to cancel order', color: 'danger' })
    } finally {
      cancelling.value = false
    }
  }
}

// Auto-refresh progress every 30 seconds for in-service orders
const startAutoRefresh = () => {
  if (order.value && (order.value.status === 2 || order.value.status === 3)) {
    refreshInterval = window.setInterval(() => {
      fetchProgress()
    }, 30000) // 30 seconds
  }
}

const stopAutoRefresh = () => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
    refreshInterval = null
  }
}

onMounted(() => {
  fetchOrderDetail().then(() => {
    startAutoRefresh()
  })
})

onUnmounted(() => {
  stopAutoRefresh()
})
</script>

<style scoped>
.order-detail-page {
  padding: var(--va-content-padding);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  gap: 16px;
}

.page-header h1 {
  margin: 0;
  flex: 1;
  text-align: center;
}

.loading-state {
  padding: 20px 0;
}

.status-banner {
  color: white;
}

.status-content {
  display: flex;
  align-items: center;
  gap: 20px;
}

.status-info {
  flex: 1;
}

.order-no {
  margin-top: 8px;
  font-size: 14px;
  opacity: 0.9;
}

.processing-tip {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
  font-size: 13px;
  opacity: 0.9;
}

.progress-section {
  background: linear-gradient(135deg, var(--va-background-element) 0%, var(--va-background-border) 100%);
}

.map-container {
  border-radius: 8px;
  overflow: hidden;
}

.map-placeholder {
  background: var(--va-background-element);
  padding: 40px 20px;
  text-align: center;
  border: 2px dashed var(--va-background-border);
  border-radius: 8px;
}

.coordinates {
  font-family: monospace;
  font-size: 14px;
  color: var(--va-text-secondary);
  margin: 8px 0;
}

.timeline-content {
  padding: 12px 0;
}

.timeline-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.timeline-time {
  font-size: 12px;
  color: var(--va-text-secondary);
}

.timeline-description {
  margin: 8px 0 0 0;
  color: var(--va-text-secondary);
  font-size: 14px;
}

.progress-images {
  display: flex;
  gap: 8px;
  margin-top: 12px;
  flex-wrap: wrap;
}

.progress-image {
  width: 80px;
  height: 80px;
  border-radius: 8px;
  object-fit: cover;
  cursor: pointer;
  transition: transform 0.2s;
}

.progress-image:hover {
  transform: scale(1.1);
}

.info-section {
  margin-bottom: 16px;
}

.payment-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 18px;
}

.price {
  color: var(--va-success);
  font-size: 24px;
  font-weight: 700;
}

.remarks-text {
  margin: 0;
  line-height: 1.6;
  color: var(--va-text-primary);
}

.action-buttons {
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--va-background-border);
}

.empty-state {
  margin-top: 40px;
}

.text-center {
  text-align: center;
  padding: 40px 20px;
}

.text-center h3 {
  margin: 16px 0 8px 0;
}

.text-center p {
  margin-bottom: 20px;
}

@media (max-width: 768px) {
  .order-detail-page {
    padding: 12px;
  }

  .status-content {
    flex-direction: column;
    text-align: center;
  }

  .payment-row {
    font-size: 16px;
  }

  .price {
    font-size: 20px;
  }

  .progress-image {
    width: 60px;
    height: 60px;
  }
}
</style>
