<template>
  <div class="login-page">
    <!-- Left Side - Illustration -->
    <div class="login-left">
      <div class="brand-section">
        <div class="brand-logo">
          <div class="logo-circle">
            <va-icon name="pets" size="64px" color="white" />
          </div>
        </div>
        <h1 class="brand-title">{{ t('home.title') }}</h1>
        <p class="brand-subtitle">{{ t('home.subtitle') }}</p>
      </div>

      <div class="illustration-section">
        <div class="floating-card" v-for="i in 3" :key="i" :style="{ animationDelay: `${i * 0.2}s` }">
          <div class="card-icon" :style="{ background: getGradient(i) }">
            <va-icon :name="getIcon(i)" size="large" color="white" />
          </div>
        </div>
      </div>

      <div class="features-preview">
        <div class="feature-item" v-for="feature in features" :key="feature.title">
          <va-icon :name="feature.icon" size="small" :color="feature.color" />
          <span>{{ feature.title }}</span>
        </div>
      </div>
    </div>

    <!-- Right Side - Login Form -->
    <div class="login-right">
      <div class="form-container">
        <!-- Language Switcher -->
        <div class="language-switcher">
          <va-button
            preset="plain"
            size="small"
            :icon="currentLocale === 'zh-CN' ? 'language' : 'translate'"
            @click="toggleLanguage"
          >
            {{ currentLocale === 'zh-CN' ? '中' : 'EN' }}
          </va-button>
        </div>

        <!-- Form Header -->
        <div class="form-header">
          <h2 class="form-title">{{ t('auth.welcomeBack') }}</h2>
          <p class="form-subtitle">{{ t('auth.signInToContinue') }}</p>
        </div>

        <!-- Login Form -->
        <va-form ref="formRef" class="login-form">
          <div class="form-group">
            <label class="form-label">{{ t('auth.phoneNumber') }}</label>
            <va-input
              v-model="form.phone"
              :placeholder="t('auth.enterPhoneNumber')"
              size="large"
              :rules="[(v: string) => !!v || t('auth.phoneRequired')]"
            >
              <template #prepend>
                <va-icon name="phone" />
              </template>
            </va-input>
          </div>

          <div class="form-group">
            <label class="form-label">{{ t('auth.password') }}</label>
            <va-input
              v-model="form.password"
              type="password"
              :placeholder="t('auth.enterPassword')"
              size="large"
              :rules="[(v: string) => !!v || t('auth.passwordRequired')]"
              @keyup.enter="handleLogin"
            >
              <template #prepend>
                <va-icon name="lock" />
              </template>
            </va-input>
          </div>

          <div class="form-options">
            <va-checkbox v-model="rememberMe">
              {{ t('auth.rememberMe') }}
            </va-checkbox>
            <va-button preset="plain" size="small" @click="showForgotPassword">
              {{ t('auth.forgotPassword') }}
            </va-button>
          </div>

          <va-button
            class="submit-button"
            size="large"
            @click="handleLogin"
            :loading="loading"
            :disabled="!isFormValid"
            block
          >
            {{ t('auth.signIn') }}
          </va-button>

          <!-- Debug Mode -->
          <div v-if="isDebugMode" class="debug-section">
            <div class="divider">
              <span>{{ t('auth.or') }}</span>
            </div>
            <va-button
              class="debug-button"
              size="large"
              @click="handleDebugLogin"
              block
            >
              {{ t('auth.debugSkipLogin') }}
            </va-button>
          </div>

          <div class="form-footer">
            {{ t('auth.noAccount') }}
            <router-link to="/register" class="link">{{ t('auth.signUp') }}</router-link>
          </div>
        </va-form>

        <!-- Copyright -->
        <div class="copyright">
          © 2025 CatCat. All rights reserved.
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useUserStore } from '@/stores/user'
import { useToast } from 'vuestic-ui'

const { t, locale } = useI18n()
const { init: notify } = useToast()
const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const rememberMe = ref(false)
const isDebugMode = import.meta.env.VITE_DEBUG_MODE === 'true'
const formRef = ref()

const currentLocale = computed(() => locale.value)

const form = ref({
  phone: '',
  password: ''
})

const features = [
  { icon: 'verified_user', title: t('home.verifiedSitters'), color: 'primary' },
  { icon: 'shield', title: t('home.serviceGuarantee'), color: 'success' },
  { icon: 'photo_camera', title: t('home.realtimeUpdates'), color: 'info' }
]

const isFormValid = computed(() => {
  return form.value.phone && form.value.password
})

const getGradient = (index: number) => {
  const gradients = [
    'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)'
  ]
  return gradients[(index - 1) % gradients.length]
}

const getIcon = (index: number) => {
  const icons = ['pets', 'favorite', 'star']
  return icons[(index - 1) % icons.length]
}

const toggleLanguage = () => {
  locale.value = locale.value === 'zh-CN' ? 'en-US' : 'zh-CN'
  localStorage.setItem('locale', locale.value)
  notify({
    message: locale.value === 'zh-CN' ? '已切换到中文' : 'Switched to English',
    color: 'success'
  })
}

const handleLogin = async () => {
  if (!formRef.value?.validate()) return

  loading.value = true
  try {
    await userStore.loginUser(form.value)
    notify({ message: t('auth.welcomeMessage'), color: 'success' })
    router.push('/')
  } catch (error: any) {
    notify({
      message: error.message || t('auth.loginFailed'),
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
  notify({ message: t('common.comingSoon'), color: 'info' })
}
</script>

<style scoped>
.login-page {
  display: flex;
  min-height: 100vh;
  background: #f5f5f7;
}

/* Left Side - Illustration */
.login-left {
  flex: 1;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 80px 60px;
  position: relative;
  overflow: hidden;
}

.login-left::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
  animation: rotate 20s linear infinite;
}

@keyframes rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.brand-section {
  text-align: center;
  z-index: 10;
  margin-bottom: 60px;
}

.brand-logo {
  margin-bottom: 32px;
}

.logo-circle {
  width: 120px;
  height: 120px;
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border-radius: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  animation: float 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-20px); }
}

.brand-title {
  font-size: 42px;
  font-weight: 700;
  color: white;
  margin: 0 0 16px 0;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.brand-subtitle {
  font-size: 18px;
  color: rgba(255, 255, 255, 0.9);
  margin: 0;
}

.illustration-section {
  display: flex;
  gap: 24px;
  margin-bottom: 60px;
  z-index: 10;
}

.floating-card {
  width: 80px;
  height: 80px;
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: float 3s ease-in-out infinite;
}

.card-icon {
  width: 60px;
  height: 60px;
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.features-preview {
  display: flex;
  flex-direction: column;
  gap: 20px;
  z-index: 10;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 12px;
  color: white;
  font-size: 16px;
  padding: 12px 20px;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 12px;
}

/* Right Side - Form */
.login-right {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  background: white;
}

.form-container {
  width: 100%;
  max-width: 460px;
  position: relative;
}

.language-switcher {
  position: absolute;
  top: 0;
  right: 0;
}

.form-header {
  margin-bottom: 40px;
}

.form-title {
  font-size: 32px;
  font-weight: 700;
  color: #1d1d1f;
  margin: 0 0 12px 0;
}

.form-subtitle {
  font-size: 16px;
  color: #6e6e73;
  margin: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-label {
  font-size: 14px;
  font-weight: 600;
  color: #1d1d1f;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: -8px;
}

.submit-button {
  margin-top: 8px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  border: none !important;
  font-weight: 600 !important;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4) !important;
  transition: all 0.3s ease !important;
}

.submit-button:hover {
  transform: translateY(-2px) !important;
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.5) !important;
}

.submit-button:active {
  transform: translateY(0) !important;
}

.debug-section {
  margin-top: -8px;
}

.divider {
  position: relative;
  text-align: center;
  margin: 24px 0;
}

.divider::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 1px;
  background: #e5e5e7;
}

.divider span {
  position: relative;
  background: white;
  padding: 0 16px;
  color: #6e6e73;
  font-size: 14px;
}

.debug-button {
  background: rgba(255, 168, 0, 0.1) !important;
  border: 2px dashed #ffa800 !important;
  color: #ffa800 !important;
  font-weight: 600 !important;
}

.form-footer {
  text-align: center;
  font-size: 14px;
  color: #6e6e73;
}

.link {
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
  margin-left: 4px;
  transition: color 0.2s;
}

.link:hover {
  color: #764ba2;
  text-decoration: underline;
}

.copyright {
  text-align: center;
  margin-top: 40px;
  font-size: 13px;
  color: #a1a1a6;
}

/* Responsive */
@media (max-width: 1024px) {
  .login-left {
    display: none;
  }

  .login-right {
    flex: 1;
    background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
  }

  .form-container {
    background: white;
    padding: 40px;
    border-radius: 20px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  }
}

@media (max-width: 768px) {
  .login-right {
    padding: 20px;
  }

  .form-container {
    padding: 32px 24px;
  }

  .form-title {
    font-size: 28px;
  }

  .form-subtitle {
    font-size: 14px;
  }

  .form-options {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
}
</style>
