<template>
  <div class="order-detail-page">
    <van-nav-bar
      title="Order Details"
      left-arrow
      @click-left="$router.back()"
      fixed
      placeholder
    />

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <van-skeleton title :row="3" style="margin-bottom: 16px" />
      <van-skeleton title :row="2" style="margin-bottom: 16px" />
      <van-skeleton title :row="2" />
    </div>

    <template v-else-if="order">
      <!-- Status Banner -->
      <div class="status-banner" :class="`status-${order.status}`">
        <div class="status-icon">
          <van-icon :name="getStatusIcon(order.status)" />
        </div>
        <div class="status-content">
          <van-tag round size="large" :type="getStatusType(order.status)">
            {{ getStatusText(order.status) }}
          </van-tag>
          <div class="order-no">Order No: {{ order.orderNo }}</div>
          <div v-if="order.status === -1" class="processing-tip">
            <van-loading size="16px" vertical>Processing...</van-loading>
          </div>
        </div>
      </div>

      <!-- Service Info -->
      <van-cell-group title="Service Information" inset>
        <van-cell title="Service Date & Time" icon="clock-o">
          <template #value>
            {{ order.serviceDate }} {{ order.serviceTime }}
          </template>
        </van-cell>
        <van-cell title="Service Address" icon="location-o" :value="order.address" />
        <van-cell v-if="order.addressDetail" icon="home-o" :value="order.addressDetail" />
      </van-cell-group>

      <!-- Payment Info -->
      <van-cell-group title="Payment Information" inset>
        <van-cell title="Service Fee" icon="paid">
          <template #value>
            <span class="price">¥{{ order.price }}</span>
          </template>
        </van-cell>
      </van-cell-group>

      <!-- Customer Remarks -->
      <van-cell-group v-if="order.customerRemark" title="Customer Remarks" inset>
        <div class="remarks-content">
          <van-icon name="comment-o" class="remarks-icon" />
          <p>{{ order.customerRemark }}</p>
        </div>
      </van-cell-group>

      <!-- Action Buttons -->
      <div class="action-buttons" v-if="order.status === -1 || order.status === 0">
        <van-button
          block
          round
          type="danger"
          icon="cross"
          @click="handleCancelOrder"
          :loading="cancelling"
        >
          Cancel Order
        </van-button>
      </div>

      <!-- Order Info -->
      <van-cell-group title="Order Information" inset>
        <van-cell title="Created At" icon="clock-o">
          <template #value>
            {{ formatDateTime(order.createdAt) }}
          </template>
        </van-cell>
        <van-cell v-if="order.serviceProviderId" title="Provider ID" icon="user-o">
          <template #value>
            {{ order.serviceProviderId }}
          </template>
        </van-cell>
      </van-cell-group>
    </template>

    <!-- Error State -->
    <van-empty v-else description="Order not found">
      <van-button round type="primary" @click="$router.back()">Go Back</van-button>
    </van-empty>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getOrderDetail, cancelOrder } from '@/api/orders'
import type { Order } from '@/api/orders'
import { showToast, showConfirmDialog, showSuccessToast } from 'vant'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const cancelling = ref(false)
const order = ref<Order>()

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
    1: 'Confirmed',
    2: 'In Service',
    3: 'Completed',
    4: 'Cancelled'
  }
  return texts[status] || 'Unknown'
}

const getStatusIcon = (status: number) => {
  const icons: Record<number, string> = {
    '-1': 'clock-o',
    0: 'todo-list-o',
    1: 'checked',
    2: 'service-o',
    3: 'success',
    4: 'cross'
  }
  return icons[status] || 'info-o'
}

const formatDateTime = (dateTime: string) => {
  if (!dateTime) return '-'
  const date = new Date(dateTime)
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fetchOrder = async () => {
  loading.value = true
  try {
    const id = Number(route.params.id)
    const res = await getOrderDetail(id)
    order.value = res.data
  } catch (error: any) {
    showToast({
      message: error.message || 'Failed to load order',
      icon: 'fail'
    })
    setTimeout(() => router.back(), 1500)
  } finally {
    loading.value = false
  }
}

const handleCancelOrder = async () => {
  try {
    await showConfirmDialog({
      title: 'Cancel Order',
      message: 'Are you sure you want to cancel this order?',
      confirmButtonText: 'Yes, Cancel',
      cancelButtonText: 'No'
    })

    cancelling.value = true
    await cancelOrder(order.value!.id, 'User cancelled')
    showSuccessToast({
      message: 'Order cancelled successfully',
      icon: 'success'
    })
    setTimeout(() => router.back(), 1500)
  } catch (error: any) {
    if (error !== 'cancel') {
      showToast({
        message: error.message || 'Cancellation failed',
        icon: 'fail'
      })
    }
  } finally {
    cancelling.value = false
  }
}

onMounted(() => {
  fetchOrder()
})
</script>

<style scoped>
.order-detail-page {
  min-height: 100vh;
  background-color: #f7f8fa;
  padding-bottom: 100px;
}

.loading-container {
  padding: 20px 16px;
}

.status-banner {
  background: white;
  padding: 24px;
  margin-bottom: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
  border-radius: 0;
}

.status-banner.status--1 {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
}

.status-banner.status-0 {
  background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
}

.status-banner.status-1,
.status-banner.status-2 {
  background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
}

.status-banner.status-3 {
  background: linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%);
}

.status-banner.status-4 {
  background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%);
}

.status-icon {
  width: 56px;
  height: 56px;
  background: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.status-content {
  flex: 1;
  text-align: left;
}

.order-no {
  margin-top: 8px;
  font-size: 13px;
  color: #646566;
  font-family: monospace;
}

.processing-tip {
  margin-top: 8px;
  font-size: 12px;
  color: #969799;
}

:deep(.van-cell-group) {
  margin-bottom: 16px;
}

.remarks-content {
  padding: 12px 16px;
  display: flex;
  gap: 12px;
  align-items: flex-start;
}

.remarks-icon {
  color: var(--primary);
  font-size: 20px;
  flex-shrink: 0;
}

.remarks-content p {
  margin: 0;
  color: #646566;
  line-height: 1.6;
}

.price {
  color: #ee0a24;
  font-weight: 700;
  font-size: 20px;
}

.action-buttons {
  padding: 16px;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: white;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
  z-index: 100;
}

:deep(.van-steps) {
  padding: 16px;
}

:deep(.van-step__title) {
  font-size: 15px;
  font-weight: 600;
}

:deep(.van-step__circle) {
  background: var(--primary);
}
</style>

