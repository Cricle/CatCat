<template>
  <div class="order-detail-page">
    <van-nav-bar
      title="订单详情"
      left-arrow
      @click-left="$router.back()"
      fixed
      placeholder
    />

    <van-loading v-if="loading" class="loading" />

    <template v-else-if="order">
      <!-- 订单状态 -->
      <div class="status-card">
        <van-tag round size="large" :type="getStatusType(order.status)">
          {{ getStatusText(order.status) }}
        </van-tag>
        <div class="order-no">订单号：{{ order.orderNo }}</div>
      </div>

      <!-- 服务信息 -->
      <van-cell-group title="服务信息" inset>
        <van-cell title="服务时间">
          <template #value>
            {{ order.serviceDate }} {{ order.serviceTime }}
          </template>
        </van-cell>
        <van-cell title="服务地址" :value="order.address" />
        <van-cell v-if="order.addressDetail" :value="order.addressDetail" />
      </van-cell-group>

      <!-- 费用信息 -->
      <van-cell-group title="费用信息" inset>
        <van-cell title="服务费用">
          <template #value>
            <span class="price">¥{{ order.price }}</span>
          </template>
        </van-cell>
      </van-cell-group>

      <!-- 备注 -->
      <van-cell-group v-if="order.customerRemark" title="备注" inset>
        <van-cell :value="order.customerRemark" />
      </van-cell-group>

      <!-- 操作按钮 -->
      <div class="action-buttons" v-if="order.status === 0">
        <van-button
          block
          type="danger"
          @click="handleCancelOrder"
          :loading="cancelling"
        >
          取消订单
        </van-button>
      </div>
    </template>
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
    0: '待接单',
    1: '已接单',
    2: '服务中',
    3: '已完成',
    4: '已取消'
  }
  return texts[status] || '未知'
}

const fetchOrder = async () => {
  loading.value = true
  try {
    const id = Number(route.params.id)
    const res = await getOrderDetail(id)
    order.value = res.data
  } catch (error: any) {
    showToast(error.message || '加载失败')
    setTimeout(() => router.back(), 1500)
  } finally {
    loading.value = false
  }
}

const handleCancelOrder = async () => {
  try {
    await showConfirmDialog({
      title: '确认取消',
      message: '确定要取消这个订单吗？'
    })

    cancelling.value = true
    await cancelOrder(order.value!.id, '用户主动取消')
    showSuccessToast('订单已取消')
    setTimeout(() => router.back(), 1500)
  } catch (error: any) {
    if (error !== 'cancel') {
      showToast(error.message || '取消失败')
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
  padding-bottom: 80px;
}

.loading {
  padding: 100px 0;
  text-align: center;
}

.status-card {
  background: white;
  padding: 24px;
  text-align: center;
  margin-bottom: 16px;
}

.order-no {
  margin-top: 12px;
  font-size: 13px;
  color: #969799;
}

:deep(.van-cell-group) {
  margin-bottom: 16px;
}

.price {
  color: #ee0a24;
  font-weight: 600;
  font-size: 18px;
}

.action-buttons {
  padding: 16px;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: white;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
}
</style>

