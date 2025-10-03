<template>
  <div class="profile-page">
    <!-- User Header -->
    <div class="user-header">
      <div class="user-avatar-section">
        <van-image
          round
          width="80"
          height="80"
          :src="userStore.userInfo?.avatar || getDefaultAvatar()"
          fit="cover"
        />
        <div class="user-info">
          <h2>{{ userStore.userInfo?.nickName || 'Guest' }}</h2>
          <p>{{ userStore.userInfo?.phone || 'Not logged in' }}</p>
        </div>
      </div>
      <van-icon name="qr" class="qr-icon" @click="showQRCode" />
    </div>

    <!-- Stats -->
    <div class="stats-section">
      <div class="stat-item" @click="$router.push('/orders?status=0')">
        <div class="stat-value">{{ stats.pending }}</div>
        <div class="stat-label">Pending</div>
      </div>
      <div class="stat-item" @click="$router.push('/orders?status=2')">
        <div class="stat-value">{{ stats.inService }}</div>
        <div class="stat-label">In Service</div>
      </div>
      <div class="stat-item" @click="$router.push('/orders?status=3')">
        <div class="stat-value">{{ stats.completed }}</div>
        <div class="stat-label">Completed</div>
      </div>
    </div>

    <!-- Menu Sections -->
    <van-cell-group inset title="My Services">
      <van-cell title="My Pets" is-link icon="friends-o" @click="$router.push('/pets')" />
      <van-cell title="My Orders" is-link icon="orders-o" @click="$router.push('/orders')" />
      <van-cell title="Addresses" is-link icon="location-o" @click="showComingSoon" />
      <van-cell title="Reviews" is-link icon="comment-o" @click="showComingSoon" />
    </van-cell-group>

    <van-cell-group inset title="Settings">
      <van-cell title="Account Settings" is-link icon="setting-o" @click="showComingSoon" />
      <van-cell title="Privacy & Security" is-link icon="shield-o" @click="showComingSoon" />
      <van-cell title="Notifications" is-link icon="bell" @click="showComingSoon" />
      <van-cell title="Language" is-link icon="globe-o" value="English" @click="showComingSoon" />
    </van-cell-group>

    <van-cell-group inset title="About">
      <van-cell title="Help Center" is-link icon="question-o" @click="showComingSoon" />
      <van-cell title="Terms of Service" is-link icon="description" @click="showComingSoon" />
      <van-cell title="About Us" is-link icon="info-o" @click="showComingSoon" />
    </van-cell-group>

    <!-- Logout -->
    <div class="logout-section">
      <van-button block round type="danger" @click="handleLogout">
        <van-icon name="sign-out" />
        Sign Out
      </van-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { showConfirmDialog, showToast } from 'vant'

const router = useRouter()
const userStore = useUserStore()

const stats = ref({
  pending: 0,
  inService: 0,
  completed: 0
})

const getDefaultAvatar = () => {
  return 'https://fastly.jsdelivr.net/npm/@vant/assets/cat.jpeg'
}

const showQRCode = () => {
  showToast({
    message: 'QR Code feature coming soon!',
    icon: 'info'
  })
}

const showComingSoon = () => {
  showToast({
    message: 'Coming soon!',
    icon: 'info'
  })
}

const loadStats = async () => {
  // Simulate loading stats
  // In real app, fetch from API
  stats.value = {
    pending: 2,
    inService: 1,
    completed: 15
  }
}

function handleLogout() {
  showConfirmDialog({
    title: 'Confirm',
    message: 'Are you sure you want to sign out?',
    confirmButtonText: 'Sign Out',
    cancelButtonText: 'Cancel'
  })
    .then(() => {
      userStore.logout()
      showToast({
        message: 'Signed out successfully',
        icon: 'success'
      })
      router.push('/login')
    })
    .catch(() => {})
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
.profile-page {
  min-height: 100vh;
  background: var(--gray-50);
  padding-bottom: 20px;
}

.user-header {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
  color: white;
  padding: 40px 20px 30px;
  position: relative;
}

.user-avatar-section {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-info h2 {
  font-size: 22px;
  font-weight: 700;
  margin: 0 0 6px 0;
}

.user-info p {
  font-size: 14px;
  opacity: 0.9;
  margin: 0;
}

.qr-icon {
  position: absolute;
  top: 40px;
  right: 20px;
  font-size: 24px;
  cursor: pointer;
}

.stats-section {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  padding: 20px;
  background: white;
  margin: -20px 16px 16px;
  border-radius: var(--radius);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.stat-item {
  text-align: center;
  cursor: pointer;
  transition: transform var(--transition);
}

.stat-item:active {
  transform: scale(0.95);
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--primary);
  margin-bottom: 4px;
}

.stat-label {
  font-size: 13px;
  color: var(--gray-600);
}

.van-cell-group {
  margin-bottom: 16px;
}

.logout-section {
  padding: 30px 16px;
}
</style>
