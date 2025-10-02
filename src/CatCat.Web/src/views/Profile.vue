<template>
  <div class="profile-page">
    <div class="user-info">
      <van-image
        round
        width="80"
        height="80"
        :src="userStore.userInfo?.avatar || 'https://fastly.jsdelivr.net/npm/@vant/assets/cat.jpeg'"
      />
      <div class="info">
        <h2>{{ userStore.userInfo?.nickName || '未登录' }}</h2>
        <p>{{ userStore.userInfo?.phone || '' }}</p>
      </div>
    </div>

    <van-cell-group inset>
      <van-cell title="我的宠物" is-link />
      <van-cell title="收货地址" is-link />
      <van-cell title="服务评价" is-link />
    </van-cell-group>

    <van-cell-group inset>
      <van-cell title="设置" is-link />
      <van-cell title="关于我们" is-link />
    </van-cell-group>

    <div class="logout-section">
      <van-button block type="danger" @click="handleLogout">退出登录</van-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { showConfirmDialog } from 'vant'

const router = useRouter()
const userStore = useUserStore()

function handleLogout() {
  showConfirmDialog({
    title: '提示',
    message: '确定要退出登录吗？',
  })
    .then(() => {
      userStore.logout()
      router.push('/login')
    })
    .catch(() => {
      // on cancel
    })
}
</script>

<style scoped>
.profile-page {
  min-height: 100vh;
  background-color: #f7f8fa;
  padding-bottom: 20px;
}

.user-info {
  display: flex;
  align-items: center;
  padding: 40px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.info {
  margin-left: 20px;
}

.info h2 {
  font-size: 20px;
  margin-bottom: 8px;
}

.info p {
  font-size: 14px;
  opacity: 0.9;
}

.van-cell-group {
  margin-top: 16px;
}

.logout-section {
  padding: 30px 16px;
}
</style>

