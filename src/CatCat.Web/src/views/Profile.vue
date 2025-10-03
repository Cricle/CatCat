<template>
  <div class="profile-page">
    <!-- Profile Header -->
    <va-card class="profile-header">
      <va-card-content>
        <div class="header-content">
          <!-- Avatar Section -->
          <div class="avatar-section">
            <div class="avatar-wrapper" :style="{ background: avatarGradient }">
              <va-avatar
                v-if="userInfo.avatar"
                :src="userInfo.avatar"
                size="120px"
              />
              <va-icon v-else name="person" size="60px" color="white" />
            </div>
            <va-button
              preset="plain"
              size="small"
              icon="edit"
              @click="showEditModal = true"
              class="edit-btn"
            >
              {{ t('common.edit') }}
            </va-button>
          </div>

          <!-- User Info -->
          <div class="user-info">
            <h1 class="user-name">{{ userInfo.username || t('common.unknown') }}</h1>
            <p class="user-phone">{{ userInfo.phone || '-' }}</p>
            <div class="user-meta">
              <va-chip size="small" :color="getRoleColor(userInfo.role)">
                {{ t(`user.${getRoleKey(userInfo.role)}`) }}
              </va-chip>
              <span class="join-date">
                {{ t('user.joinDate') }}: {{ formatDate(userInfo.createdAt) }}
              </span>
            </div>
          </div>
        </div>
      </va-card-content>
    </va-card>

    <!-- Stats Cards -->
    <div class="stats-grid">
      <va-card
        v-for="stat in stats"
        :key="stat.key"
        class="stat-card"
        :style="{ borderTop: `4px solid ${stat.color}` }"
      >
        <va-card-content>
          <div class="stat-content">
            <div class="stat-icon" :style="{ background: stat.gradient }">
              <va-icon :name="stat.icon" size="32px" color="white" />
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ stat.value }}</div>
              <div class="stat-label">{{ stat.label }}</div>
            </div>
          </div>
        </va-card-content>
      </va-card>
    </div>

    <!-- Quick Actions -->
    <va-card class="actions-card">
      <va-card-title>
        <h3>{{ t('user.quickActions') }}</h3>
      </va-card-title>
      <va-card-content>
        <div class="actions-grid">
          <div
            v-for="action in quickActions"
            :key="action.key"
            class="action-item"
            @click="handleAction(action.key)"
          >
            <div class="action-icon" :style="{ background: action.gradient }">
              <va-icon :name="action.icon" size="24px" color="white" />
            </div>
            <span class="action-label">{{ action.label }}</span>
            <va-icon name="arrow_forward_ios" size="small" color="secondary" />
          </div>
        </div>
      </va-card-content>
    </va-card>

    <!-- Danger Zone -->
    <va-card class="danger-zone">
      <va-card-content>
        <div class="danger-content">
          <div class="danger-info">
            <h4>{{ t('user.logout') }}</h4>
            <p>{{ t('user.logoutDesc') }}</p>
          </div>
          <va-button color="danger" @click="confirmLogout">
            {{ t('user.logout') }}
          </va-button>
        </div>
      </va-card-content>
    </va-card>

    <!-- Edit Profile Modal -->
    <va-modal
      v-model="showEditModal"
      size="medium"
      :title="t('user.editProfile')"
      @ok="saveProfile"
      @cancel="closeEditModal"
    >
      <va-form ref="formRef">
        <va-input
          v-model="editForm.username"
          :label="t('user.username')"
          :placeholder="t('user.username')"
          :rules="[(v) => !!v || t('user.usernameRequired')]"
        />

        <va-input
          v-model="editForm.phone"
          :label="t('user.phone')"
          :placeholder="t('user.phone')"
          disabled
        />

        <va-input
          v-model="editForm.avatar"
          :label="t('user.avatar')"
          :placeholder="t('user.avatarPlaceholder')"
        />

        <va-textarea
          v-model="editForm.bio"
          :label="t('user.bio')"
          :placeholder="t('user.bioPlaceholder')"
          :min-rows="3"
        />
      </va-form>
    </va-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast, useModal } from 'vuestic-ui'
import { useUserStore } from '@/stores/user'
// API functions will be mocked for now

const { t } = useI18n()
const { init: notify } = useToast()
const { confirm } = useModal()
const router = useRouter()
const userStore = useUserStore()

const showEditModal = ref(false)
const formRef = ref()
const userInfo = ref<any>({
  id: 0,
  username: '',
  phone: '',
  avatar: '',
  role: 1,
  createdAt: new Date().toISOString()
})

const editForm = ref({
  username: '',
  phone: '',
  avatar: '',
  bio: ''
})

const stats = ref([
  {
    key: 'totalOrders',
    label: t('user.totalOrders'),
    value: 0,
    icon: 'receipt_long',
    color: '#667eea',
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
  },
  {
    key: 'completedOrders',
    label: t('user.completedOrders'),
    value: 0,
    icon: 'check_circle',
    color: '#43e97b',
    gradient: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)'
  },
  {
    key: 'totalPets',
    label: t('user.totalPets'),
    value: 0,
    icon: 'pets',
    color: '#fa709a',
    gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)'
  },
  {
    key: 'totalSpent',
    label: t('user.totalSpent'),
    value: '¥0',
    icon: 'payments',
    color: '#f093fb',
    gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)'
  }
])

const quickActions = [
  {
    key: 'orders',
    label: t('user.myOrders'),
    icon: 'receipt_long',
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
  },
  {
    key: 'pets',
    label: t('user.myPets'),
    icon: 'pets',
    gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)'
  },
  {
    key: 'settings',
    label: t('user.settings'),
    icon: 'settings',
    gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)'
  },
  {
    key: 'help',
    label: t('user.help'),
    icon: 'help',
    gradient: 'linear-gradient(135deg, #90a4ae 0%, #78909c 100%)'
  }
]

const avatarGradient = computed(() => {
  return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
})

const getRoleKey = (role: number) => {
  const roleMap: Record<number, string> = {
    1: 'customer',
    2: 'provider',
    99: 'admin'
  }
  return roleMap[role] || 'customer'
}

const getRoleColor = (role: number) => {
  const colorMap: Record<number, string> = {
    1: 'primary',
    2: 'success',
    99: 'danger'
  }
  return colorMap[role] || 'primary'
}

const formatDate = (dateString: string) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const fetchUserInfo = async () => {
  try {
    // Get from userStore
    const userData = userStore.userInfo || {}
    userInfo.value = {
      id: userData.id || 0,
      username: userData.username || 'User',
      phone: userData.phone || '',
      avatar: userData.avatar || '',
      role: userData.role || 1,
      createdAt: new Date().toISOString()
    }
    
    editForm.value = {
      username: userData.username || '',
      phone: userData.phone || '',
      avatar: userData.avatar || '',
      bio: userData.bio || ''
    }

    // Mock stats (will be replaced with real API)
    stats.value[0].value = 8
    stats.value[1].value = 5
    stats.value[2].value = 2
    stats.value[3].value = '¥1580'
  } catch (error: any) {
    notify({ message: error.message || t('user.failedToLoad'), color: 'danger' })
  }
}

const saveProfile = async () => {
  if (!formRef.value?.validate()) return

  try {
    // Mock update (will be replaced with real API)
    notify({ message: t('user.updateSuccess'), color: 'success' })
    closeEditModal()
    fetchUserInfo()
  } catch (error: any) {
    notify({ message: error.message, color: 'danger' })
  }
}

const closeEditModal = () => {
  showEditModal.value = false
}

const handleAction = (key: string) => {
  switch (key) {
    case 'orders':
      router.push('/orders')
      break
    case 'pets':
      router.push('/pets')
      break
    case 'settings':
      notify({ message: t('common.comingSoon'), color: 'info' })
      break
    case 'help':
      notify({ message: t('common.comingSoon'), color: 'info' })
      break
  }
}

const confirmLogout = async () => {
  const agreed = await confirm({
    title: t('common.confirm'),
    message: t('user.logoutConfirm'),
    okText: t('common.confirm'),
    cancelText: t('common.cancel')
  })

  if (agreed) {
    userStore.logout()
    router.push('/login')
    notify({ message: t('user.logoutSuccess'), color: 'success' })
  }
}

onMounted(() => {
  fetchUserInfo()
})
</script>

<style scoped>
.profile-page {
  padding: 24px;
  max-width: 1000px;
  margin: 0 auto;
}

/* Profile Header */
.profile-header {
  margin-bottom: 24px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.header-content {
  display: flex;
  gap: 32px;
  align-items: center;
}

.avatar-section {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.avatar-wrapper {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  border: 4px solid rgba(255, 255, 255, 0.3);
}

.edit-btn {
  color: white !important;
}

.user-info {
  flex: 1;
}

.user-name {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 8px 0;
  color: white;
}

.user-phone {
  font-size: 16px;
  margin: 0 0 16px 0;
  color: rgba(255, 255, 255, 0.9);
}

.user-meta {
  display: flex;
  align-items: center;
  gap: 16px;
}

.join-date {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  width: 64px;
  height: 64px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-info {
  flex: 1;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: #1d1d1f;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #6e6e73;
}

/* Quick Actions */
.actions-card {
  margin-bottom: 24px;
}

.actions-grid {
  display: grid;
  gap: 12px;
}

.action-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: 12px;
  background: #f5f5f7;
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-item:hover {
  background: #e8e8ed;
  transform: translateX(4px);
}

.action-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.action-label {
  flex: 1;
  font-size: 16px;
  font-weight: 500;
  color: #1d1d1f;
}

/* Danger Zone */
.danger-zone {
  border: 1px solid #f5576c;
}

.danger-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.danger-info h4 {
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
  margin: 0 0 4px 0;
}

.danger-info p {
  font-size: 14px;
  color: #6e6e73;
  margin: 0;
}

/* Responsive */
@media (max-width: 768px) {
  .profile-page {
    padding: 16px;
  }

  .header-content {
    flex-direction: column;
    text-align: center;
  }

  .user-info {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .user-name {
    font-size: 24px;
  }

  .stat-value {
    font-size: 24px;
  }

  .danger-content {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }
}

@media (max-width: 480px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>
