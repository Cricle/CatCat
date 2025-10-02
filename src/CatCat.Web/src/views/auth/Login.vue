<template>
  <div class="login-page">
    <va-card class="login-card">
      <va-card-content>
        <div class="logo-section">
          <div class="logo-icon">🐱</div>
          <h1 class="logo-title">CatCat</h1>
          <p class="logo-subtitle">专业上门喂猫服务</p>
        </div>

        <va-form ref="formRef" class="login-form">
          <va-input v-model="form.phone" label="手机号" placeholder="请输入手机号"
            :rules="[(v: string) => !!v || '请输入手机号', (v: string) => /^1[3-9]\d{9}$/.test(v) || '手机号格式不正确']">
            <template #prepend><va-icon name="phone" /></template>
          </va-input>

          <div class="sms-input-group">
            <va-input v-model="form.smsCode" label="验证码" placeholder="请输入验证码"
              :rules="[(v: string) => !!v || '请输入验证码']" style="flex: 1">
              <template #prepend><va-icon name="lock" /></template>
            </va-input>
            <va-button :disabled="countdown > 0" :loading="sendingCode" @click="sendCode" class="sms-button">
              {{ countdown > 0 ? `${countdown}s` : '获取验证码' }}
            </va-button>
          </div>

          <va-button class="login-button" :loading="loading" @click="handleLogin" size="large" block>登录</va-button>

          <div class="footer-links">
            <va-checkbox v-model="agreed" size="small">
              我已阅读并同意<a href="#" class="link">《服务协议》</a>和<a href="#" class="link">《隐私政策》</a>
            </va-checkbox>
          </div>
        </va-form>

        <div class="other-login">
          <va-divider>其他登录方式</va-divider>
          <div class="social-buttons">
            <va-button preset="plain" icon="wechat" round @click="wechatLogin">微信</va-button>
            <va-button preset="plain" icon="phone_android" round @click="alipayLogin">支付宝</va-button>
          </div>
        </div>
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
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { sendSmsCode, login } from '@/api/auth'

const router = useRouter()
const userStore = useUserStore()
const formRef = ref()
const loading = ref(false)
const sendingCode = ref(false)
const countdown = ref(0)
const agreed = ref(false)
const form = reactive({ phone: '', smsCode: '' })

const sendCode = async () => {
  if (!/^1[3-9]\d{9}$/.test(form.phone)) return
  sendingCode.value = true
  try {
    await sendSmsCode(form.phone)
    countdown.value = 60
    const timer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) clearInterval(timer)
    }, 1000)
    showToast('验证码已发送', 'success')
  } catch (error: any) {
    showToast(error.message || '发送失败', 'error')
  } finally {
    sendingCode.value = false
  }
}

const handleLogin = async () => {
  if (!agreed.value) {
    showToast('请先阅读并同意服务协议和隐私政策', 'warning')
    return
  }
  const valid = await formRef.value?.validate()
  if (!valid) return
  loading.value = true
  try {
    const res = await login(form.phone, form.smsCode)
    userStore.setToken(res.token)
    await userStore.fetchUser()
    showToast('登录成功', 'success')
    router.push('/')
  } catch (error: any) {
    showToast(error.message || '登录失败', 'error')
  } finally {
    loading.value = false
  }
}

const wechatLogin = () => showToast('微信登录功能开发中', 'info')
const alipayLogin = () => showToast('支付宝登录功能开发中', 'info')
const showToast = (message: string, color: string) => console.log(message, color)
</script>

<style scoped>
.login-page { position: relative; display: flex; justify-content: center; align-items: center; min-height: 100vh; background: linear-gradient(135deg, var(--va-primary) 0%, var(--va-secondary) 100%); overflow: hidden; }
.login-card { position: relative; z-index: 1; width: 100%; max-width: 420px; margin: 20px; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3); }
.logo-section { text-align: center; margin-bottom: 32px; }
.logo-icon { font-size: 64px; animation: bounce 2s infinite; }
.logo-title { margin: 12px 0 8px 0; font-size: 32px; font-weight: 700; background: linear-gradient(135deg, var(--va-primary), var(--va-secondary)); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
.logo-subtitle { margin: 0; color: var(--va-text-secondary); font-size: 14px; }
.login-form { display: flex; flex-direction: column; gap: 20px; }
.sms-input-group { display: flex; gap: 12px; align-items: flex-start; }
.sms-button { margin-top: 24px; flex-shrink: 0; white-space: nowrap; }
.login-button { margin-top: 8px; }
.footer-links { text-align: center; font-size: 13px; }
.link { color: var(--va-primary); text-decoration: none; }
.link:hover { text-decoration: underline; }
.other-login { margin-top: 32px; }
.social-buttons { display: flex; justify-content: center; gap: 16px; margin-top: 16px; }
.background-decoration { position: absolute; top: 0; left: 0; right: 0; bottom: 0; overflow: hidden; pointer-events: none; }
.decoration-circle { position: absolute; border-radius: 50%; background: rgba(255, 255, 255, 0.1); backdrop-filter: blur(10px); }
.circle-1 { width: 300px; height: 300px; top: -100px; left: -100px; animation: float 6s ease-in-out infinite; }
.circle-2 { width: 200px; height: 200px; top: 50%; right: -50px; animation: float 8s ease-in-out infinite; }
.circle-3 { width: 150px; height: 150px; bottom: -50px; left: 30%; animation: float 7s ease-in-out infinite; }
@keyframes bounce { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-10px); } }
@keyframes float { 0%, 100% { transform: translateY(0) rotate(0deg); } 50% { transform: translateY(-20px) rotate(10deg); } }
@media (max-width: 768px) {
  .login-card { margin: 0; border-radius: 0; box-shadow: none; }
  .logo-icon { font-size: 48px; }
  .logo-title { font-size: 24px; }
}
</style>
