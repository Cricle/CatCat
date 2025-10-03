<template>
  <div class="auth-page">
    <div class="auth-container">
      <va-card class="auth-card">
        <va-card-content>
          <div class="auth-header">
            <div class="auth-icon">🐱</div>
            <h1 class="va-h1">Welcome Back</h1>
            <p class="va-text-secondary">Sign in to continue to CatCat</p>
          </div>

          <va-form ref="formRef" class="auth-form">
            <va-input
              v-model="form.phone"
              label="Phone Number"
              placeholder="Enter your phone number"
              :rules="[(v: string) => !!v || 'Phone is required']"
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
              :rules="[(v: string) => !!v || 'Password is required']"
              @keyup.enter="handleLogin"
            >
              <template #prepend>
                <va-icon name="lock" />
              </template>
            </va-input>

            <div class="form-footer">
              <va-checkbox v-model="rememberMe" label="Remember me" />
              <va-button preset="plain" size="small" @click="showForgotPassword">
                Forgot password?
              </va-button>
            </div>

            <va-button
              class="submit-button"
              color="primary"
              @click="handleLogin"
              :loading="loading"
              :disabled="!isFormValid"
              block
            >
              <va-icon name="login" /> Sign In
            </va-button>

            <div v-if="isDebugMode" class="auth-divider">
              <span>or</span>
            </div>

            <va-button
              v-if="isDebugMode"
              class="debug-button"
              @click="handleDebugLogin"
              preset="secondary"
              icon="bug_report"
              block
            >
              🚀 Debug: Skip Login
            </va-button>

            <div class="auth-link">
              Don't have an account?
              <router-link to="/register" class="link">Sign up</router-link>
            </div>
          </va-form>
        </va-card-content>
      </va-card>

      <div class="auth-footer">
        <p>© 2025 CatCat. All rights reserved.</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useToast } from 'vuestic-ui'

const { init: notify } = useToast()
const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const rememberMe = ref(false)
const isDebugMode = import.meta.env.VITE_DEBUG_MODE === 'true'
const formRef = ref()

const form = ref({
  phone: '',
  password: ''
})

const isFormValid = computed(() => {
  return form.value.phone && form.value.password
})

const handleLogin = async () => {
  if (!formRef.value?.validate()) return

  loading.value = true
  try {
    await userStore.loginUser(form.value)
    notify({ message: 'Welcome back!', color: 'success' })
    router.push('/')
  } catch (error: any) {
    notify({ 
      message: error.message || 'Login failed. Please check your credentials.',
      color: 'danger',
      duration: 5000
    })
  } finally {
    loading.value = false
  }
}

const handleDebugLogin = () => {
  userStore.debugLogin()
  notify({ message: 'Debug mode activated!', color: 'info' })
  router.push('/')
}

const showForgotPassword = () => {
  notify({ message: 'Password reset coming soon!', color: 'info' })
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background: linear-gradient(135deg, var(--va-primary) 0%, var(--va-info) 100%);
}

.auth-container {
  width: 100%;
  max-width: 440px;
}

.auth-card {
  box-shadow: var(--va-shadow-lg);
  border-radius: 12px;
}

.auth-header {
  text-align: center;
  margin-bottom: 32px;
}

.auth-icon {
  font-size: 64px;
  margin-bottom: 16px;
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
}

.auth-header h1 {
  margin: 0 0 8px 0;
  font-size: 28px;
  font-weight: 700;
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
  margin-top: -8px;
}

.submit-button {
  margin-top: 8px;
  font-weight: 600;
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
  background: var(--va-background-border);
}

.auth-divider span {
  position: relative;
  background: var(--va-background-element);
  padding: 0 12px;
  color: var(--va-text-secondary);
  font-size: 14px;
}

.debug-button {
  border: 2px dashed var(--va-warning) !important;
  background: rgba(var(--va-warning-rgb), 0.1) !important;
}

.auth-link {
  text-align: center;
  font-size: 14px;
  color: var(--va-text-secondary);
  margin-top: -8px;
}

.link {
  color: var(--va-primary);
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s;
}

.link:hover {
  color: var(--va-info);
  text-decoration: underline;
}

.auth-footer {
  text-align: center;
  margin-top: 24px;
  padding: 20px;
  color: rgba(255, 255, 255, 0.8);
  font-size: 14px;
}

@media (max-width: 768px) {
  .auth-page {
    padding: 12px;
  }

  .auth-container {
    max-width: 100%;
  }

  .auth-icon {
    font-size: 48px;
  }

  .auth-header h1 {
    font-size: 24px;
  }

  .form-footer {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }
}
</style>
