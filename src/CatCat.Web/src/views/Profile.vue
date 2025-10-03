<template>
  <div class="profile-page">
    <va-card class="profile-header">
      <va-card-content>
        <div class="user-section">
          <va-avatar size="80px" :src="userStore.userInfo?.avatar || getDefaultAvatar()" />
          <div class="user-info">
            <h2 class="va-h2">{{ userStore.userInfo?.nickName || 'Guest' }}</h2>
            <p class="va-text-secondary">{{ userStore.userInfo?.phone || 'Not logged in' }}</p>
          </div>
          <va-button preset="plain" icon="qr_code" @click="showQRCode" />
        </div>

        <!-- Stats -->
        <div class="stats-grid">
          <va-card class="stat-card" @click="$router.push('/orders?status=0')">
            <va-card-content>
              <div class="stat-value">{{ stats.pending }}</div>
              <div class="stat-label">Pending</div>
            </va-card-content>
          </va-card>
          <va-card class="stat-card" @click="$router.push('/orders?status=3')">
            <va-card-content>
              <div class="stat-value">{{ stats.inService }}</div>
              <div class="stat-label">In Service</div>
            </va-card-content>
          </va-card>
          <va-card class="stat-card" @click="$router.push('/orders?status=4')">
            <va-card-content>
              <div class="stat-value">{{ stats.completed }}</div>
              <div class="stat-label">Completed</div>
            </va-card-content>
          </va-card>
        </div>
      </va-card-content>
    </va-card>

    <!-- Menu Sections -->
    <va-card class="menu-section">
      <va-card-title>My Services</va-card-title>
      <va-card-content>
        <va-list>
          <va-list-item @click="$router.push('/pets')">
            <va-list-item-section icon>
              <va-icon name="pets" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>My Pets</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="$router.push('/orders')">
            <va-list-item-section icon>
              <va-icon name="receipt_long" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>My Orders</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="location_on" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Addresses</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="comment" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Reviews</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>
        </va-list>
      </va-card-content>
    </va-card>

    <va-card class="menu-section">
      <va-card-title>Settings</va-card-title>
      <va-card-content>
        <va-list>
          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="settings" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Account Settings</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="shield" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Privacy & Security</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="notifications" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Notifications</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="language" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Language</va-list-item-label>
              <va-list-item-label caption>English</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>
        </va-list>
      </va-card-content>
    </va-card>

    <va-card class="menu-section">
      <va-card-title>About</va-card-title>
      <va-card-content>
        <va-list>
          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="help" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Help Center</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="description" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>Terms of Service</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>

          <va-list-separator />

          <va-list-item @click="showComingSoon">
            <va-list-item-section icon>
              <va-icon name="info" />
            </va-list-item-section>
            <va-list-item-section>
              <va-list-item-label>About Us</va-list-item-label>
            </va-list-item-section>
            <va-list-item-section icon>
              <va-icon name="chevron_right" />
            </va-list-item-section>
          </va-list-item>
        </va-list>
      </va-card-content>
    </va-card>

    <!-- Logout -->
    <div class="logout-section">
      <va-button block color="danger" @click="handleLogout">
        <va-icon name="logout" /> Sign Out
      </va-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()
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
  notify({ message: 'QR Code feature coming soon!', color: 'info' })
}

const showComingSoon = () => {
  notify({ message: 'Coming soon!', color: 'info' })
}

const loadStats = async () => {
  // Simulate loading stats
  stats.value = {
    pending: 2,
    inService: 1,
    completed: 15
  }
}

const handleLogout = async () => {
  const agreed = await confirm({
    title: 'Confirm',
    message: 'Are you sure you want to sign out?',
    okText: 'Sign Out',
    cancelText: 'Cancel'
  })

  if (agreed) {
    userStore.logout()
    notify({ message: 'Signed out successfully', color: 'success' })
    router.push('/login')
  }
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
.profile-page {
  padding: var(--va-content-padding);
  min-height: 100vh;
}

.profile-header {
  margin-bottom: var(--va-content-padding);
}

.user-section {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}

.user-info {
  flex: 1;
}

.user-info h2 {
  margin: 0 0 4px 0;
}

.user-info p {
  margin: 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.stat-card {
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--va-shadow-lg);
}

.stat-card:active {
  transform: scale(0.98);
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--va-primary);
  text-align: center;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 13px;
  color: var(--va-text-secondary);
  text-align: center;
}

.menu-section {
  margin-bottom: var(--va-content-padding);
}

.va-list-item {
  cursor: pointer;
  transition: background 0.2s;
}

.va-list-item:hover {
  background: var(--va-background-element);
}

.logout-section {
  margin-top: 24px;
  padding-bottom: 24px;
}

@media (max-width: 768px) {
  .profile-page {
    padding: 12px;
  }

  .stats-grid {
    gap: 8px;
  }

  .stat-value {
    font-size: 20px;
  }

  .stat-label {
    font-size: 12px;
  }
}
</style>
