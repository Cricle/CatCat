<template>
  <div class="orders-page">
    <!-- Page Header -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">{{ t('order.title') }}</h1>
        <va-button size="large" @click="createNewOrder" icon="add">
          {{ t('order.createOrder') }}
        </va-button>
      </div>

      <!-- Filters -->
      <div class="filters-section">
        <va-button-group>
          <va-button
            v-for="status in statusFilters"
            :key="status.value"
            :preset="activeFilter === status.value ? 'primary' : 'secondary'"
            size="small"
            @click="activeFilter = status.value"
          >
            {{ status.label }}
          </va-button>
        </va-button-group>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="orders-timeline">
      <va-card v-for="i in 3" :key="i" class="order-card">
        <va-card-content>
          <va-skeleton height="24px" width="30%" />
          <va-skeleton height="60px" width="100%" style="margin-top: 16px" />
        </va-card-content>
      </va-card>
    </div>

    <!-- Empty State -->
    <va-card v-else-if="!loading && filteredOrders.length === 0" class="empty-state">
      <va-card-content>
        <div class="empty-content">
          <va-icon name="receipt_long" size="80px" color="secondary" />
          <h3>{{ t('order.noOrders') }}</h3>
          <p>{{ t('order.createFirstOrder') }}</p>
          <va-button size="large" @click="createNewOrder" icon="add">
            {{ t('order.createOrder') }}
          </va-button>
        </div>
      </va-card-content>
    </va-card>

    <!-- Orders Timeline -->
    <div v-else class="orders-timeline">
      <div
        v-for="order in filteredOrders"
        :key="order.id"
        class="order-item"
        @click="viewOrderDetail(order)"
      >
        <!-- Timeline Indicator -->
        <div class="timeline-indicator">
          <div class="timeline-dot" :style="{ background: getStatusGradient(order.status) }">
            <va-icon :name="getStatusIcon(order.status)" size="small" color="white" />
          </div>
          <div class="timeline-line"></div>
        </div>

        <!-- Order Card -->
        <va-card class="order-card">
          <va-card-content>
            <!-- Card Header -->
            <div class="card-header">
              <div class="order-meta">
                <h3 class="order-no">{{ order.orderNo }}</h3>
                <va-chip
                  size="small"
                  :color="getStatusColor(order.status)"
                  :style="{ background: getStatusGradient(order.status) }"
                >
                  {{ t(`order.${getStatusKey(order.status)}`) }}
                </va-chip>
              </div>
              <div class="order-date">
                {{ formatDate(order.serviceDate) }}
              </div>
            </div>

            <va-divider style="margin: 16px 0" />

            <!-- Order Details -->
            <div class="order-details">
              <div class="detail-row">
                <div class="detail-item">
                  <va-icon name="pets" size="small" />
                  <span>{{ order.petName || t('common.unknown') }}</span>
                </div>
                <div class="detail-item">
                  <va-icon name="business_center" size="small" />
                  <span>{{ order.packageName || t('common.unknown') }}</span>
                </div>
              </div>
              <div class="detail-row">
                <div class="detail-item">
                  <va-icon name="location_on" size="small" />
                  <span>{{ order.address || t('common.unknown') }}</span>
                </div>
                <div class="detail-item price">
                  <va-icon name="payments" size="small" />
                  <span class="price-text">¥{{ order.totalPrice }}</span>
                </div>
              </div>
            </div>

            <!-- Quick Actions -->
            <div class="order-actions">
              <va-button
                v-if="canCancelOrder(order)"
                preset="plain"
                size="small"
                color="danger"
                @click.stop="confirmCancelOrder(order)"
              >
                {{ t('order.cancelOrder') }}
              </va-button>
              <va-button
                preset="plain"
                size="small"
                @click.stop="viewOrderDetail(order)"
                icon="arrow_forward"
              >
                {{ t('common.view') }}
              </va-button>
            </div>
          </va-card-content>
        </va-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast, useModal } from 'vuestic-ui'
import { getMyOrders, cancelOrder } from '@/api/orders'

const { t } = useI18n()
const { init: notify } = useToast()
const { confirm } = useModal()
const router = useRouter()

const loading = ref(false)
const orders = ref<any[]>([])
const activeFilter = ref('all')

const statusFilters = [
  { label: t('common.all'), value: 'all' },
  { label: t('order.queued'), value: 'queued' },
  { label: t('order.pending'), value: 'pending' },
  { label: t('order.accepted'), value: 'accepted' },
  { label: t('order.inProgress'), value: 'inProgress' },
  { label: t('order.completed'), value: 'completed' }
]

const filteredOrders = computed(() => {
  if (activeFilter.value === 'all') {
    return orders.value
  }
  return orders.value.filter(order =>
    getStatusKey(order.status).toLowerCase() === activeFilter.value.toLowerCase()
  )
})

const getStatusKey = (status: number) => {
  const statusMap: Record<number, string> = {
    0: 'queued',
    1: 'pending',
    2: 'accepted',
    3: 'inProgress',
    4: 'completed',
    5: 'cancelled'
  }
  return statusMap[status] || 'pending'
}

const getStatusColor = (status: number) => {
  const colorMap: Record<number, string> = {
    0: 'warning',
    1: 'secondary',
    2: 'info',
    3: 'primary',
    4: 'success',
    5: 'danger'
  }
  return colorMap[status] || 'secondary'
}

const getStatusGradient = (status: number) => {
  const gradientMap: Record<number, string> = {
    0: 'linear-gradient(135deg, #ffa726 0%, #fb8c00 100%)', // Queued - Orange
    1: 'linear-gradient(135deg, #90a4ae 0%, #78909c 100%)', // Pending - Gray
    2: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)', // Accepted - Blue
    3: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', // InProgress - Purple
    4: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)', // Completed - Green
    5: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)'  // Cancelled - Pink
  }
  return gradientMap[status] || gradientMap[1]
}

const getStatusIcon = (status: number) => {
  const iconMap: Record<number, string> = {
    0: 'schedule',
    1: 'pending',
    2: 'check_circle',
    3: 'local_shipping',
    4: 'done_all',
    5: 'cancel'
  }
  return iconMap[status] || 'pending'
}

const canCancelOrder = (order: any) => {
  return order.status === 0 || order.status === 1 || order.status === 2
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diff = date.getTime() - now.getTime()
  const days = Math.ceil(diff / (1000 * 60 * 60 * 24))

  if (days === 0) {
    return t('common.today')
  } else if (days === 1) {
    return t('common.tomorrow')
  } else if (days < 7) {
    return `${days} ${t('common.daysLater')}`
  }

  return date.toLocaleDateString('zh-CN', { month: 'long', day: 'numeric' })
}

const fetchOrders = async () => {
  loading.value = true
  try {
    const res = await getMyOrders({ page: 1, pageSize: 100 })
    orders.value = res.data?.items || []
  } catch (error: any) {
    notify({ message: error.message || t('order.failedToLoad'), color: 'danger' })
  } finally {
    loading.value = false
  }
}

const createNewOrder = () => {
  router.push('/order/create')
}

const viewOrderDetail = (order: any) => {
  router.push(`/order/${order.id}`)
}

const confirmCancelOrder = async (order: any) => {
  const agreed = await confirm({
    title: t('common.confirm'),
    message: t('order.cancelConfirm'),
    okText: t('common.confirm'),
    cancelText: t('common.cancel')
  })

  if (agreed) {
    try {
      await cancelOrder(order.id, '')
      notify({ message: t('order.cancelSuccess'), color: 'success' })
      fetchOrders()
    } catch (error: any) {
      notify({ message: error.message, color: 'danger' })
    }
  }
}

onMounted(() => {
  fetchOrders()
})
</script>

<style scoped>
.orders-page {
  padding: 24px;
  max-width: 1000px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 32px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.page-title {
  font-size: 32px;
  font-weight: 700;
  color: #1d1d1f;
  margin: 0;
}

.filters-section {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

/* Orders Timeline */
.orders-timeline {
  position: relative;
}

.order-item {
  display: flex;
  gap: 24px;
  margin-bottom: 32px;
  position: relative;
}

.order-item:last-child .timeline-line {
  display: none;
}

/* Timeline Indicator */
.timeline-indicator {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  flex-shrink: 0;
}

.timeline-dot {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 10;
}

.timeline-line {
  width: 2px;
  flex: 1;
  background: linear-gradient(to bottom, #e5e5e7 0%, transparent 100%);
  min-height: 40px;
}

/* Order Card */
.order-card {
  flex: 1;
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid #e5e5e7;
}

.order-card:hover {
  transform: translateX(8px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: transparent;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.order-meta {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.order-no {
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
  margin: 0;
}

.order-date {
  font-size: 14px;
  color: #6e6e73;
}

.order-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  gap: 24px;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #6e6e73;
  flex: 1;
}

.detail-item.price {
  justify-content: flex-end;
}

.price-text {
  font-size: 20px;
  font-weight: 700;
  color: #f5576c;
}

.order-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
}

/* Empty State */
.empty-state {
  margin-top: 60px;
}

.empty-content {
  text-align: center;
  padding: 60px 40px;
}

.empty-content h3 {
  font-size: 24px;
  margin: 24px 0 12px 0;
  color: #1d1d1f;
}

.empty-content p {
  font-size: 16px;
  color: #6e6e73;
  margin: 0 0 32px 0;
}

/* Responsive */
@media (max-width: 768px) {
  .orders-page {
    padding: 16px;
  }

  .page-title {
    font-size: 24px;
  }

  .header-content {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }

  .order-item {
    gap: 16px;
  }

  .timeline-dot {
    width: 40px;
    height: 40px;
  }

  .detail-row {
    flex-direction: column;
    gap: 8px;
  }

  .detail-item.price {
    justify-content: flex-start;
  }

  .order-card:hover {
    transform: translateX(4px);
  }
}
</style>
