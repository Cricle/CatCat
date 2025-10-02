<template>
  <div class="orders-page">
    <van-nav-bar title="我的订单" fixed placeholder />

    <van-tabs v-model:active="activeTab" @change="onTabChange">
      <van-tab title="全部" />
      <van-tab title="待接单" />
      <van-tab title="服务中" />
      <van-tab title="已完成" />
    </van-tabs>

    <van-pull-refresh v-model="refreshing" @refresh="onRefresh">
      <div class="order-list">
        <van-loading v-if="loading" class="loading" />
        <van-empty v-else-if="orders.length === 0" description="暂无订单">
          <van-button type="primary" @click="$router.push('/home')">
            去下单
          </van-button>
        </van-empty>

        <van-cell-group
          v-for="order in orders"
          :key="order.id"
          inset
          class="order-item"
        >
          <van-cell>
            <template #title>
              <div class="order-header">
                <span class="order-no">订单号: {{ order.orderNo }}</span>
                <van-tag :type="getStatusType(order.status)">
                  {{ getStatusText(order.status) }}
                </van-tag>
              </div>
            </template>
          </van-cell>
          <van-cell title="服务时间">
            <template #value>
              {{ order.serviceDate }} {{ order.serviceTime }}
            </template>
          </van-cell>
          <van-cell title="服务地址" :value="order.address" />
          <van-cell title="订单金额">
            <template #value>
              <span class="price">¥{{ order.price }}</span>
            </template>
          </van-cell>
          <van-cell>
            <template #default>
              <div class="order-actions">
                <van-button
                  size="small"
                  type="primary"
                  @click="viewDetail(order.id)"
                >
                  查看详情
                </van-button>
                <van-button
                  v-if="order.status === -1 || order.status === 0"
                  size="small"
                  @click="handleCancelOrder(order.id)"
                >
                  取消订单
                </van-button>
              </div>
            </template>
          </van-cell>
        </van-cell-group>
      </div>
    </van-pull-refresh>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyOrders, cancelOrder } from '@/api/orders'
import { showToast, showConfirmDialog } from 'vant'

const router = useRouter()
const activeTab = ref(0)
const loading = ref(false)
const refreshing = ref(false)
const orders = ref<any[]>([])

const statusMap = [
  { value: undefined, text: '全部' },
  { value: 0, text: '待接单' },
  { value: 2, text: '服务中' },
  { value: 3, text: '已完成' }
]

const getStatusType = (status: number) => {
  const types: Record<number, any> = {
    '-1': 'default', // 排队中
    0: 'warning',    // 待接单
    1: 'primary',    // 已接单
    2: 'primary',    // 服务中
    3: 'success',    // 已完成
    4: 'danger'      // 已取消
  }
  return types[status] || 'default'
}

const getStatusText = (status: number) => {
  const texts: Record<number, string> = {
    '-1': '处理中',
    0: '待接单',
    1: '已接单',
    2: '服务中',
    3: '已完成',
    4: '已取消'
  }
  return texts[status] || '未知'
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
    orders.value = res.data.items
  } catch (error: any) {
    showToast(error.message || 'Loading failed')
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
      title: '确认取消',
      message: '确定要取消这个订单吗？'
    })

    await cancelOrder(orderId, '用户主动取消')
    showToast('取消成功')
    fetchOrders()
  } catch (error: any) {
    if (error !== 'cancel') {
      showToast(error.message || '取消失败')
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
  background-color: #f7f8fa;
  padding-bottom: 60px;
}

.order-list {
  padding: 16px;
  min-height: 400px;
}

.loading {
  padding: 60px 0;
  text-align: center;
}

.order-item {
  margin-bottom: 16px;
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.order-no {
  font-weight: 500;
  font-size: 13px;
  color: #646566;
}

.price {
  color: #ee0a24;
  font-weight: 600;
  font-size: 16px;
}

.order-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}
</style>
