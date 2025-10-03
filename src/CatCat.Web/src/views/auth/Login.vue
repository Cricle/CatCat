<template>
  <div class="login-page">
    <va-card class="login-card">
      <va-card-content>
        <div class="logo-section">
          <div class="logo-icon">🐱</div>
          <h1 class="logo-title">CatCat</h1>
          <p class="logo-subtitle">Professional Cat Sitting Service</p>
        </div>

        <va-form ref="formRef" class="login-form">
          <va-input
            v-model="form.phone"
            label="Phone"
            placeholder="Enter phone number"
            :rules="phoneRules">
            <template #prepend><va-icon name="phone" /></template>
          </va-input>

          <va-input
            v-model="form.password"
            type="password"
            label="Password"
            placeholder="Enter password"
            :rules="passwordRules">
            <template #prepend><va-icon name="lock" /></template>
          </va-input>

          <va-button
            class="login-button"
            :loading="loading"
            @click="handleLogin"
            size="large"
            block>
            Login
          </va-button>

          <va-button
            v-if="isDebugMode"
            class="debug-button"
            color="warning"
            @click="handleDebugLogin"
            size="large"
            block>
            🚀 Debug: Skip Login
          </va-button>

          <div class="links">
            <router-link to="/register" class="link">Create account</router-link>
          </div>
        </va-form>
      </va-card-content>
    </va-card>

    <div class="background-decoration">
      <div class="decoration-circle circle-1"></div>
      <div class="decoration-circle circle-2"></div>
      <div class="decoration-circle circle-3"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import type { LoginRequest } from '@/api/auth'
import { VaForm } from 'vuestic-ui'

const router = useRouter()
const userStore = useUserStore()
const formRef = ref<InstanceType<typeof VaForm>>()
const loading = ref(false)

const isDebugMode = computed(() => import.meta.env.VITE_DEBUG_MODE === 'true')

const form = reactive<LoginRequest>({
  phone: '',
  password: ''
})

const phoneRules = [
  (v: string) => !!v || 'Phone number required',
  (v: string) => /^1[3-9]\d{9}$/.test(v) || 'Invalid phone format'
]

const passwordRules = [
  (v: string) => !!v || 'Password required',
  (v: string) => v.length >= 6 || 'Password must be at least 6 characters'
]

const handleLogin = async () => {
  const valid = await formRef.value?.validate()
  if (!valid) return

  loading.value = true
  try {
    await userStore.loginUser(form)
    router.push('/')
  } catch (error: any) {
    console.error('Login failed:', error)
  } finally {
    loading.value = false
  }
}

const handleDebugLogin = () => {
  userStore.debugLogin()
  console.log('🚀 Debug mode: Logged in as Debug User')
  router.push('/')
}
</script>

<style scoped>
.login-page {
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, var(--va-primary) 0%, var(--va-secondary) 100%);
  overflow: hidden;
}

.login-card {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 420px;
  margin: 20px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.logo-section {
  text-align: center;
  margin-bottom: 32px;
}

.logo-icon {
  font-size: 64px;
  animation: bounce 2s infinite;
}

.logo-title {
  margin: 12px 0 8px;
  font-size: 32px;
  font-weight: 700;
  background: linear-gradient(135deg, var(--va-primary), var(--va-secondary));
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.logo-subtitle {
  margin: 0;
  color: var(--va-text-secondary);
  font-size: 14px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.login-button {
  margin-top: 8px;
}

.debug-button {
  margin-top: 12px;
  border: 2px dashed var(--va-warning);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

.links {
  text-align: center;
  font-size: 14px;
}

.link {
  color: var(--va-primary);
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}

.background-decoration {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
}

.decoration-circle {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.circle-1 {
  width: 300px;
  height: 300px;
  top: -100px;
  left: -100px;
  animation: float 6s ease-in-out infinite;
}

.circle-2 {
  width: 200px;
  height: 200px;
  top: 50%;
  right: -50px;
  animation: float 8s ease-in-out infinite;
}

.circle-3 {
  width: 150px;
  height: 150px;
  bottom: -50px;
  left: 30%;
  animation: float 7s ease-in-out infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

@keyframes float {
  0%, 100% { transform: translateY(0) rotate(0deg); }
  50% { transform: translateY(-20px) rotate(10deg); }
}

@media (max-width: 768px) {
  .login-card {
    margin: 0;
    border-radius: 0;
    box-shadow: none;
  }

  .logo-icon {
    font-size: 48px;
  }

  .logo-title {
    font-size: 24px;
  }
}
</style>
