<template>
  <VaDropdown v-model="isShown" class="notification-dropdown" placement="bottom-end" :offset="[0, 12]" stick-to-edges>
    <template #anchor>
      <VaButton preset="secondary" :rounded="false" class="notification-button" @click="toggleDropdown">
        <VaIcon name="notifications" />
        <VaBadge v-if="unreadCount > 0" :text="unreadCount" color="danger" overlap placement="top-end" />
      </VaButton>
    </template>

    <VaDropdownContent class="notification-content">
      <div class="px-4 py-3 border-b border-gray-200 dark:border-gray-700">
        <div class="flex items-center justify-between">
          <h3 class="font-semibold text-lg">{{ t('notifications.title') }}</h3>
          <VaButton v-if="notifications.length > 0" preset="plain" size="small" @click="goToNotifications">
            {{ t('dashboard.cards.viewAll') }}
          </VaButton>
        </div>
      </div>

      <div v-if="loading" class="flex justify-center py-8">
        <VaProgressCircle indeterminate size="small" />
      </div>

      <div v-else-if="notifications.length === 0" class="text-center py-8 px-4 text-secondary">
        <VaIcon name="notifications_off" size="2rem" color="secondary" />
        <p class="mt-2">{{ t('notifications.noNotifications') }}</p>
      </div>

      <div v-else class="max-h-96 overflow-y-auto">
        <div
          v-for="notification in notifications.slice(0, 5)"
          :key="notification.id"
          :class="{ 'bg-blue-50 dark:bg-blue-900/10': !notification.isRead }"
          class="notification-item px-4 py-3 border-b border-gray-100 dark:border-gray-800 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 transition"
          @click="handleNotificationClick(notification)"
        >
          <div class="flex gap-3">
            <VaIcon :name="getNotificationIcon(notification.type)" :color="getNotificationColor(notification.type)" />
            <div class="flex-grow min-w-0">
              <div class="flex items-start justify-between gap-2 mb-1">
                <h4 class="font-semibold text-sm truncate">{{ notification.title }}</h4>
                <VaBadge v-if="!notification.isRead" text="" color="primary" class="w-2 h-2 rounded-full" />
              </div>
              <p class="text-sm text-secondary line-clamp-2 mb-1">{{ notification.content }}</p>
              <span class="text-xs text-secondary">{{ formatTime(notification.createdAt) }}</span>
            </div>
          </div>
        </div>
      </div>

      <div v-if="notifications.length > 0" class="px-4 py-3 border-t border-gray-200 dark:border-gray-700">
        <VaButton preset="plain" class="w-full" @click="goToNotifications">
          {{ t('dashboard.cards.viewAll') }}
        </VaButton>
      </div>
    </VaDropdownContent>
  </VaDropdown>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()

const isShown = ref(false)
const loading = ref(false)
const notifications = ref<any[]>([])

const unreadCount = computed(() => notifications.value.filter((n) => !n.isRead).length)

const toggleDropdown = () => {
  isShown.value = !isShown.value
  if (isShown.value && notifications.value.length === 0) {
    loadNotifications()
  }
}

const loadNotifications = async () => {
  loading.value = true
  try {
    await new Promise((resolve) => setTimeout(resolve, 300))
    notifications.value = [
      {
        id: 1,
        type: 'order',
        title: '订单已接单',
        content: '您的订单 #12345 已被服务人员接单',
        isRead: false,
        createdAt: new Date().toISOString(),
        link: '/orders/12345',
      },
      {
        id: 2,
        type: 'progress',
        title: '服务进度更新',
        content: '服务人员已到达服务地点',
        isRead: false,
        createdAt: new Date(Date.now() - 3600000).toISOString(),
        link: '/orders/12345',
      },
      {
        id: 3,
        type: 'order',
        title: '服务已完成',
        content: '订单 #12344 的服务已完成',
        isRead: true,
        createdAt: new Date(Date.now() - 86400000).toISOString(),
        link: '/orders/12344',
      },
    ]
  } catch (error) {
    console.error('Failed to load notifications:', error)
  } finally {
    loading.value = false
  }
}

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
  isShown.value = false
  if (notification.link) {
    router.push(notification.link)
  }
}

const goToNotifications = () => {
  isShown.value = false
  router.push('/notifications')
}

onMounted(() => {
  loadNotifications()
})
</script>

<style scoped>
.notification-dropdown {
  position: relative;
}

.notification-button {
  position: relative;
}

.notification-content {
  min-width: 360px;
  max-width: 420px;
}

.notification-item {
  transition: all 0.2s ease;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
