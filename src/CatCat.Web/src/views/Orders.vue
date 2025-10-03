<template>
  <div class="orders-page">
    <va-card>
      <va-card-title>
        <h1 class="va-h1">My Orders</h1>
      </va-card-title>

      <va-card-content>
        <!-- Tabs -->
        <va-tabs v-model="activeTab" grow>
          <va-tab v-for="tab in tabs" :key="tab.value" :label="tab.label" />
        </va-tabs>

        <!-- Loading State -->
        <div v-if="loading" class="loading-container">
          <va-skeleton height="120px" v-for="i in 3" :key="i" class="skeleton-item" />
        </div>

        <!-- Empty State -->
        <va-card v-else-if="orders.length === 0" class="empty-state">
          <va-card-content>
            <div class="text-center">
              <va-icon name="receipt_long" size="64px" color="secondary" />
              <h3 class="va-h3">No Orders Yet</h3>
              <p class="va-text-secondary">Start by creating your first order</p>
              <va-button color="primary" @click="$router.push('/order/create')">
                <va-icon name="add" /> Create Order
              </va-button>
            </div>
          </va-card-content>
        </va-card>

        <!-- Order List -->
        <div v-else class="va-row">
          <div v-for="order in orders" :key="order.id" class="flex xs12 md6">
            <va-card hover class="order-card" @click="viewDetail(order.id)">
              <va-card-content>
                <!-- Header -->
                <div class="order-header">
                  <span class="order-no">{{ order.orderNo }}</span>
                  <va-badge
                    :text="getStatusText(order.status)"
                    :color="getStatusColor(order.status)"
                  />
                </div>

                <va-divider />

                <!-- Info -->
                <div class="order-info">
                  <div class="info-row">
                    <va-icon name="schedule" color="secondary" />
                    <span>{{ formatDateTime(order.scheduledTime) }}</span>
                  </div>
                  <div class="info-row">
                    <va-icon name="location_on" color="secondary" />
                    <span>{{ order.address }}</span>
                  </div>
                  <div class="info-row">
                    <va-icon name="payments" color="success" />
                    <span class="price">¥{{ order.totalPrice }}</span>
                  </div>
                </div>

                <va-divider />

                <!-- Footer -->
                <div class="order-footer">
                  <va-button
                    v-if="canCancel(order.status)"
                    size="small"
                    preset="plain"
                    @click.stop="handleCancelOrder(order.id)"
                  >
                    Cancel
                  </va-button>
                  <va-button size="small" color="primary" @click.stop="viewDetail(order.id)">
                    View Details
                  </va-button>
                </div>
              </va-card-content>
            </va-card>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="orders.length > 0" class="pagination-wrapper">
          <va-pagination
            v-model="currentPage"
            :pages="totalPages"
            @update:modelValue="fetchOrders"
          />
        </div>
      </va-card-content>
    </va-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyOrders, cancelOrder } from '@/api/orders'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()
const router = useRouter()

const activeTab = ref(0)
const loading = ref(false)
const orders = ref<any[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

const tabs = [
  { label: 'All', value: undefined },
  { label: 'Queued', value: 0 },
  { label: 'Pending', value: 1 },
  { label: 'In Service', value: 3 },
  { label: 'Completed', value: 4 }
]

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))

const getStatusText = (status: number) => {
  const map: Record<number, string> = {
    0: 'Queued',
    1: 'Pending',
    2: 'Accepted',
    3: 'In Service',
    4: 'Completed',
    5: 'Cancelled'
  }
  return map[status] || 'Unknown'
}

const getStatusColor = (status: number) => {
  const map: Record<number, string> = {
    0: 'info',
    1: 'warning',
    2: 'primary',
    3: 'info',
    4: 'success',
    5: 'danger'
  }
  return map[status] || 'secondary'
}

const canCancel = (status: number) => {
  return status === 0 || status === 1
}

const formatDateTime = (dateTime: string) => {
  return new Date(dateTime).toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fetchOrders = async () => {
  loading.value = true
  try {
    const statusFilter = tabs[activeTab.value]?.value
    const res = await getMyOrders({
      page: currentPage.value,
      pageSize: pageSize.value,
      status: statusFilter as number | undefined
    })
    orders.value = res.data.items
    total.value = res.data.total
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load orders', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const viewDetail = (orderId: number) => {
  router.push(`/orders/${orderId}`)
}

const handleCancelOrder = async (orderId: number) => {
  const agreed = await confirm({
    title: 'Cancel Order',
    message: 'Are you sure you want to cancel this order?',
    okText: 'Yes, Cancel',
    cancelText: 'No'
  })

  if (agreed) {
    try {
      await cancelOrder(orderId)
      notify({ message: 'Order cancelled successfully', color: 'success' })
      fetchOrders()
    } catch (error: any) {
      notify({ message: error.message || 'Failed to cancel order', color: 'danger' })
    }
  }
}

onMounted(() => {
  fetchOrders()
})
</script>

<style scoped>
.orders-page {
  padding: var(--va-content-padding);
}

.loading-container {
  padding: var(--va-content-padding) 0;
}

.skeleton-item {
  margin-bottom: 16px;
}

.empty-state {
  margin-top: var(--va-content-padding);
}

.text-center {
  text-align: center;
  padding: var(--va-content-padding);
}

.order-card {
  width: 100%;
  margin-bottom: var(--va-content-padding);
  cursor: pointer;
  transition: transform 0.2s;
}

.order-card:hover {
  transform: translateY(-4px);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.order-no {
  font-weight: 600;
  font-size: 14px;
  color: var(--va-text-primary);
}

.order-info {
  margin: 16px 0;
}

.info-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
  font-size: 14px;
  color: var(--va-text-secondary);
}

.price {
  font-weight: 700;
  font-size: 16px;
  color: var(--va-success);
}

.order-footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 12px;
}

.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: var(--va-content-padding);
}

@media (max-width: 768px) {
  .orders-page {
    padding: 12px;
  }

  .order-card {
    margin-bottom: 12px;
  }
}
</style>
