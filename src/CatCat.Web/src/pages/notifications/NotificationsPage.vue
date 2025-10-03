<template>
  <h1 class="page-title">{{ t('notifications.title') }}</h1>

  <!-- Filters -->
  <VaCard class="mb-6">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4 items-end">
        <VaButtonGroup class="flex-grow">
          <VaButton
            v-for="tab in tabs"
            :key="tab.value"
            :preset="activeTab === tab.value ? 'primary' : 'secondary'"
            @click="activeTab = tab.value"
          >
            {{ tab.label }}
            <VaBadge v-if="tab.count > 0" :text="tab.count" color="danger" class="ml-2" />
          </VaButton>
        </VaButtonGroup>

        <VaButton preset="secondary" icon="done_all" @click="markAllAsRead">
          {{ t('notifications.markAllRead') }}
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Loading -->
  <div v-if="loading" class="flex justify-center py-12">
    <VaProgressCircle indeterminate />
  </div>

  <!-- Empty -->
  <div v-else-if="filteredNotifications.length === 0" class="text-center py-12">
    <VaIcon name="notifications_off" size="4rem" color="secondary" />
    <p class="text-xl mt-4 text-secondary">{{ t('notifications.noNotifications') }}</p>
  </div>

  <!-- Notifications List -->
  <div v-else class="space-y-3">
    <VaCard
      v-for="notification in filteredNotifications"
      :key="notification.id"
      :class="{ 'opacity-60': notification.isRead }"
      class="notification-card cursor-pointer hover:shadow-md transition-all"
      @click="handleNotificationClick(notification)"
    >
      <VaCardContent>
        <div class="flex items-start gap-4">
          <VaIcon :name="getNotificationIcon(notification.type)" :color="getNotificationColor(notification.type)" size="large" />

          <div class="flex-grow">
            <div class="flex items-start justify-between mb-2">
              <h3 class="font-semibold text-lg">{{ notification.title }}</h3>
              <VaBadge v-if="!notification.isRead" text="新" color="danger" />
            </div>

            <p class="text-secondary mb-2">{{ notification.content }}</p>

            <div class="flex items-center gap-4 text-sm text-secondary">
              <div class="flex items-center gap-1">
                <VaIcon name="schedule" size="small" />
                <span>{{ formatTime(notification.createdAt) }}</span>
              </div>
              <VaChip :color="getNotificationColor(notification.type)" size="small">
                {{ getNotificationTypeText(notification.type) }}
              </VaChip>
            </div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Pagination -->
  <div v-if="!loading && filteredNotifications.length > 0" class="flex justify-center mt-6">
    <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'

const { t } = useI18n()
const router = useRouter()
const { init: notify } = useToast()

const loading = ref(false)
const activeTab = ref('all')
const notifications = ref<any[]>([])

const tabs = computed(() => [
  { value: 'all', label: t('notifications.all'), count: notifications.value.length },
  { value: 'unread', label: t('notifications.unread'), count: notifications.value.filter((n) => !n.isRead).length },
  { value: 'order', label: t('notifications.order'), count: notifications.value.filter((n) => n.type === 'order').length },
  { value: 'system', label: t('notifications.system'), count: notifications.value.filter((n) => n.type === 'system').length },
])

const pagination = ref({
  page: 1,
  perPage: 10,
})

const loadNotifications = async () => {
  loading.value = true
  try {
    // Mock data
    notifications.value = [
      {
        id: 1,
        type: 'order',
        title: '订单已接单',
        content: '您的订单 #12345 已被服务人员接单，预计明天上午10:00开始服务',
        isRead: false,
        createdAt: new Date().toISOString(),
        link: '/orders/12345',
      },
      {
        id: 2,
        type: 'progress',
        title: '服务进度更新',
        content: '服务人员已到达服务地点，开始为您的宠物提供服务',
        isRead: false,
        createdAt: new Date(Date.now() - 3600000).toISOString(),
        link: '/orders/12345',
      },
      {
        id: 3,
        type: 'order',
        title: '服务已完成',
        content: '订单 #12344 的服务已完成，请查看服务照片并进行评价',
        isRead: true,
        createdAt: new Date(Date.now() - 86400000).toISOString(),
        link: '/orders/12344',
      },
      {
        id: 4,
        type: 'system',
        title: '新功能上线',
        content: '我们上线了新的套餐服务，快来查看吧！',
        isRead: true,
        createdAt: new Date(Date.now() - 172800000).toISOString(),
        link: '/packages',
      },
    ]
  } catch (error: any) {
    notify({ message: error.message || '加载通知失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const filteredNotifications = computed(() => {
  let result = notifications.value

  if (activeTab.value === 'unread') {
    result = result.filter((n) => !n.isRead)
  } else if (activeTab.value === 'order') {
    result = result.filter((n) => n.type === 'order' || n.type === 'progress')
  } else if (activeTab.value === 'system') {
    result = result.filter((n) => n.type === 'system')
  }

  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return result.slice(start, end)
})

const totalPages = computed(() => {
  let count = notifications.value.length
  if (activeTab.value === 'unread') {
    count = notifications.value.filter((n) => !n.isRead).length
  } else if (activeTab.value === 'order') {
    count = notifications.value.filter((n) => n.type === 'order' || n.type === 'progress').length
  } else if (activeTab.value === 'system') {
    count = notifications.value.filter((n) => n.type === 'system').length
  }
  return Math.ceil(count / pagination.value.perPage)
})

const getNotificationIcon = (type: string) => {
  const map: Record<string, string> = {
    order: 'shopping_cart',
    progress: 'update',
    system: 'campaign',
  }
  return map[type] || 'notifications'
}

const getNotificationColor = (type: string) => {
  const map: Record<string, string> = {
    order: 'primary',
    progress: 'success',
    system: 'warning',
  }
  return map[type] || 'info'
}

const getNotificationTypeText = (type: string) => {
  const map: Record<string, string> = {
    order: '订单通知',
    progress: '进度更新',
    system: '系统通知',
  }
  return map[type] || '通知'
}

const formatTime = (dateStr: string) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diff = now.getTime() - date.getTime()

  if (diff < 3600000) {
    return `${Math.floor(diff / 60000)} 分钟前`
  } else if (diff < 86400000) {
    return `${Math.floor(diff / 3600000)} 小时前`
  } else if (diff < 604800000) {
    return `${Math.floor(diff / 86400000)} 天前`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

const handleNotificationClick = (notification: any) => {
  notification.isRead = true
  if (notification.link) {
    router.push(notification.link)
  }
}

const markAllAsRead = () => {
  notifications.value.forEach((n) => (n.isRead = true))
  notify({ message: '所有通知已标记为已读', color: 'success' })
}

onMounted(() => {
  loadNotifications()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.notification-card {
  transition: all 0.3s ease;
}

.notification-card:hover {
  transform: translateX(4px);
}
</style>

