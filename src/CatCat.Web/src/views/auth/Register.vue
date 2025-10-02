<template>
  <div class="register-page">
    <van-nav-bar title="注册" left-arrow @click-left="$router.back()" />

    <van-form @submit="onSubmit">
      <van-cell-group inset>
        <van-field
          v-model="formData.phone"
          name="phone"
          label="手机号"
          placeholder="请输入手机号"
          :rules="[{ required: true, message: '请输入手机号' }]"
          type="tel"
          maxlength="11"
        />
        <van-field
          v-model="formData.code"
          name="code"
          label="验证码"
          placeholder="请输入验证码"
          :rules="[{ required: true, message: '请输入验证码' }]"
        >
          <template #button>
            <van-button
              size="small"
              type="primary"
              @click="sendVerifyCode"
              :disabled="countdown > 0"
            >
              {{ countdown > 0 ? `${countdown}s` : '发送验证码' }}
            </van-button>
          </template>
        </van-field>
        <van-field
          v-model="formData.password"
          type="password"
          name="password"
          label="密码"
          placeholder="请输入密码（6-20位）"
          :rules="[{ required: true, message: '请输入密码' }]"
        />
        <van-field
          v-model="formData.nickName"
          name="nickName"
          label="昵称"
          placeholder="请输入昵称（可选）"
        />
      </van-cell-group>

      <div class="button-group">
        <van-button round block type="primary" native-type="submit" :loading="loading">
          注册
        </van-button>
      </div>
    </van-form>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { sendCode } from '@/api/auth'
import { showToast } from 'vant'

const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const countdown = ref(0)
const formData = reactive({
  phone: '',
  code: '',
  password: '',
  nickName: ''
})

async function sendVerifyCode() {
  if (!formData.phone || formData.phone.length !== 11) {
    showToast('请输入正确的手机号')
    return
  }

  try {
    await sendCode({ phone: formData.phone })
    showToast('验证码已发送')
    countdown.value = 60
    const timer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) {
        clearInterval(timer)
      }
    }, 1000)
  } catch (error) {
    console.error(error)
  }
}

async function onSubmit() {
  try {
    loading.value = true
    await userStore.registerUser(formData)
    showToast('注册成功')
    router.push('/home')
  } catch (error) {
    console.error(error)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.register-page {
  min-height: 100vh;
  background-color: #f7f8fa;
}

.button-group {
  margin-top: 20px;
  padding: 0 16px;
}
</style>

