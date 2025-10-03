<template>
  <div class="orders-page">
    <van-nav-bar title="My Orders" fixed placeholder />

    <van-tabs v-model:active="activeTab" @change="onTabChange" sticky>
      <van-tab title="All" />
      <van-tab title="Pending" />
      <van-tab title="In Service" />
      <van-tab title="Completed" />
    </van-tabs>

    <van-pull-refresh v-model="refreshing" @refresh="onRefresh">
      <div class="order-list">
        <!-- Skeleton Loading -->
        <template v-if="loading && !refreshing">
          <van-skeleton title :row="3" class="skeleton-item" v-for="i in 3" :key="i" />
        </template>

        <!-- Empty State -->
        <van-empty 
          v-else-if="orders.length === 0" 
          image="search"
          description="No orders yet"
        >
          <van-button type="primary" round @click="$router.push('/')">
            <van-icon name="plus" />
            Create Order
          </van-button>
        </van-empty>

        <!-- Order List -->
        <div v-else class="order-items">
          <div
            v-for="order in orders"
            :key="order.id"
            class="order-card"
            @click="viewDetail(order.id)"
          >
            <div class="order-header">
              <span class="order-no">{{ order.orderNo }}</span>
              <van-tag 
                :type="getStatusType(order.status)" 
                size="medium"
                round
              >
                {{ getStatusText(order.status) }}
              </van-tag>
            </div>

            <van-divider />

            <div class="order-info">
              <div class="info-row">
                <van-icon name="clock-o" />
                <span>{{ formatDateTime(order.scheduledTime) }}</span>
              </div>
              <div class="info-row">
                <van-icon name="location-o" />
                <span>{{ order.address }}</span>
              </div>
              <div class="info-row">
                <van-icon name="paid" />
                <span class="price">¥{{ order.totalPrice }}</span>
              </div>
            </div>

            <div class="order-footer" @click.stop>
              <van-button
                v-if="canCancel(order.status)"
                size="small"
                plain
                @click="handleCancelOrder(order.id)"
              >
                Cancel
              </van-button>
              <van-button
                size="small"
                type="primary"
                @click="viewDetail(order.id)"
              >
                View Details
              </van-button>
            </div>
          </div>
        </div>
      </div>
    </van-pull-refresh>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyOrders, cancelOrder } from '@/api/orders'
import { showToast, showConfirmDialog, showLoadingToast, closeToast } from 'vant'

const router = useRouter()
const activeTab = ref(0)
const loading = ref(false)
const refreshing = ref(false)
const orders = ref<any[]>([])

const statusMap = [
  { value: undefined, text: 'All' },
  { value: 0, text: 'Pending' },
  { value: 2, text: 'In Service' },
  { value: 3, text: 'Completed' }
]

const getStatusType = (status: number) => {
  const types: Record<number, any> = {
    '-1': 'default',
    0: 'warning',
    1: 'primary',
    2: 'primary',
    3: 'success',
    4: 'danger'
  }
  return types[status] || 'default'
}

const getStatusText = (status: number) => {
  const texts: Record<number, string> = {
    '-1': 'Processing',
    0: 'Pending',
    1: 'Accepted',
    2: 'In Service',
    3: 'Completed',
    4: 'Cancelled'
  }
  return texts[status] || 'Unknown'
}

const canCancel = (status: number) => {
  return status === -1 || status === 0
}

const formatDateTime = (dateTime: string) => {
  if (!dateTime) return '-'
  const date = new Date(dateTime)
  return date.toLocaleString('en-US', { 
    month: 'short', 
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fetchOrders = async () => {
  loading.value = true
  try {
    const status = statusMap[activeTab.value].value
    const res = await getMyOrders({
      page: 1,
      pageSize: 20,
      status
    })
    orders.value = res.data.items || []
  } catch (error: any) {
    showToast({
      message: error.message || 'Failed to load orders',
      icon: 'fail'
    })
  } finally {
    loading.value = false
    refreshing.value = false
  }
}

const onTabChange = () => {
  fetchOrders()
}

const onRefresh = () => {
  refreshing.value = true
  fetchOrders()
}

const viewDetail = (orderId: number) => {
  router.push(`/orders/${orderId}`)
}

const handleCancelOrder = async (orderId: number) => {
  try {
    await showConfirmDialog({
      title: 'Cancel Order',
      message: 'Are you sure you want to cancel this order?',
      confirmButtonText: 'Yes, Cancel',
      cancelButtonText: 'No'
    })

    showLoadingToast({
      message: 'Cancelling...',
      forbidClick: true,
      duration: 0
    })

    await cancelOrder(orderId, 'User cancelled')
    
    closeToast()
    showToast({
      message: 'Order cancelled successfully',
      icon: 'success'
    })
    
    fetchOrders()
  } catch (error: any) {
    closeToast()
    if (error !== 'cancel') {
      showToast({
        message: error.message || 'Failed to cancel order',
        icon: 'fail'
      })
    }
  }
}

onMounted(() => {
  fetchOrders()
})
</script>

<style scoped>
.orders-page {
  min-height: 100vh;
  background: var(--gray-50);
  padding-bottom: 70px;
}

.order-list {
  padding: 12px;
  min-height: 400px;
}

.skeleton-item {
  margin-bottom: 12px;
  padding: 16px;
  background: white;
  border-radius: var(--radius);
}

.order-items {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.order-card {
  background: white;
  border-radius: var(--radius);
  padding: 16px;
  cursor: pointer;
  transition: all var(--transition);
  border: 1px solid var(--gray-200);
}

.order-card:active {
  transform: scale(0.98);
  border-color: var(--primary);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.order-no {
  font-size: 14px;
  font-weight: 600;
  color: var(--gray-700);
}

.order-info {
  margin: 12px 0;
}

.info-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
  font-size: 14px;
  color: var(--gray-600);
}

.info-row .va-icon {
  color: var(--gray-400);
}

.price {
  color: var(--primary);
  font-weight: 600;
  font-size: 16px;
}

.order-footer {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 12px;
}
</style>
