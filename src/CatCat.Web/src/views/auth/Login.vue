<template>
  <div class="auth-page">
    <div class="auth-container">
      <div class="auth-header">
        <div class="auth-icon">🐱</div>
        <h1 class="auth-title">Welcome Back</h1>
        <p class="auth-subtitle">Sign in to your account</p>
      </div>

      <va-form class="auth-form">
        <va-input
          v-model="form.phone"
          label="Phone Number"
          placeholder="Enter your phone number"
          :rules="[(v) => !!v || 'Phone is required']"
        >
          <template #prepend>
            <va-icon name="phone" />
          </template>
        </va-input>

        <va-input
          v-model="form.password"
          label="Password"
          type="password"
          placeholder="Enter your password"
          :rules="[(v) => !!v || 'Password is required']"
        >
          <template #prepend>
            <va-icon name="lock" />
          </template>
        </va-input>

        <div class="form-footer">
          <va-checkbox v-model="rememberMe" label="Remember me" />
          <va-button preset="plain" size="small">Forgot password?</va-button>
        </div>

        <va-button
          class="submit-button"
          @click="handleLogin"
          :loading="loading"
          block
        >
          Sign In
        </va-button>

        <div class="auth-divider">
          <span>or</span>
        </div>

        <va-button
          v-if="isDebugMode"
          class="debug-button"
          @click="handleDebugLogin"
          preset="secondary"
          block
        >
          🚀 Debug: Skip Login
        </va-button>

        <div class="auth-link">
          Don't have an account?
          <router-link to="/register" class="link">Sign up</router-link>
        </div>
      </va-form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()
const loading = ref(false)
const rememberMe = ref(false)
const isDebugMode = import.meta.env.VITE_DEBUG_MODE === 'true'

const form = ref({
  phone: '',
  password: ''
})

const handleLogin = async () => {
  loading.value = true
  try {
    await userStore.loginUser(form.value)
    router.push('/')
  } catch (error: any) {
    console.error('Login failed:', error)
  } finally {
    loading.value = false
  }
}

const handleDebugLogin = () => {
  userStore.debugLogin()
  router.push('/')
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background: var(--gray-50);
}

.auth-container {
  width: 100%;
  max-width: 400px;
  background: white;
  border: 1px solid var(--gray-200);
  border-radius: var(--radius-lg);
  padding: 40px;
}

.auth-header {
  text-align: center;
  margin-bottom: 32px;
}

.auth-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.auth-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--gray-900);
  margin-bottom: 8px;
}

.auth-subtitle {
  font-size: 0.875rem;
  color: var(--gray-600);
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.submit-button {
  margin-top: 8px;
}

.auth-divider {
  position: relative;
  text-align: center;
  margin: 8px 0;
}

.auth-divider::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 1px;
  background: var(--gray-200);
}

.auth-divider span {
  position: relative;
  background: white;
  padding: 0 12px;
  color: var(--gray-600);
  font-size: 0.875rem;
}

.debug-button {
  border: 1px dashed var(--warning) !important;
}

.auth-link {
  text-align: center;
  font-size: 0.875rem;
  color: var(--gray-600);
}

.link {
  color: var(--primary);
  text-decoration: none;
  font-weight: 500;
}

.link:hover {
  text-decoration: underline;
}
</style>
