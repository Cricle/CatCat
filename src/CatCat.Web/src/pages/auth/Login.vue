<template>
  <VaForm ref="form" @submit.prevent="submit">
    <h1 class="font-semibold text-4xl mb-4">CatCat 喂猫服务</h1>
    <p class="text-base mb-4 leading-5">
      没有账号？
      <RouterLink :to="{ name: 'signup' }" class="font-semibold text-primary">立即注册</RouterLink>
    </p>

    <!-- Debug Mode Badge -->
    <VaChip v-if="isDebugMode" color="warning" class="mb-4">
      🚀 DEBUG MODE
      <VaButton size="small" color="primary" class="ml-2" @click="handleDebugLogin">
        一键登录
      </VaButton>
    </VaChip>

    <VaInput
      v-model="formData.phone"
      :rules="[validators.required, phoneValidator]"
      :disabled="isLoading"
      class="mb-4"
      label="手机号"
      type="tel"
      placeholder="请输入手机号"
    />
    <VaValue v-slot="isPasswordVisible" :default-value="false">
      <VaInput
        v-model="formData.password"
        :rules="[validators.required, passwordValidator]"
        :type="isPasswordVisible.value ? 'text' : 'password'"
        :disabled="isLoading"
        class="mb-4"
        label="密码"
        placeholder="请输入密码"
        @clickAppendInner.stop="isPasswordVisible.value = !isPasswordVisible.value"
      >
        <template #appendInner>
          <VaIcon
            :name="isPasswordVisible.value ? 'mso-visibility_off' : 'mso-visibility'"
            class="cursor-pointer"
            color="secondary"
          />
        </template>
      </VaInput>
    </VaValue>

    <div class="auth-layout__options flex flex-col sm:flex-row items-start sm:items-center justify-between">
      <VaCheckbox v-model="formData.keepLoggedIn" class="mb-2 sm:mb-0" label="记住登录状态" />
      <RouterLink :to="{ name: 'recover-password' }" class="mt-2 sm:mt-0 sm:ml-1 font-semibold text-primary">
        忘记密码？
      </RouterLink>
    </div>

    <div class="flex justify-center mt-4">
      <VaButton class="w-full" :loading="isLoading" @click="submit">
        {{ isLoading ? '登录中...' : '登录' }}
      </VaButton>
    </div>
  </VaForm>
</template>

<script lang="ts" setup>
import { reactive, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useForm, useToast } from 'vuestic-ui'
import { validators } from '../../services/utils'
import { useUserStore } from '../../stores/user-store'

const { validate } = useForm('form')
const { push } = useRouter()
const { init } = useToast()
const userStore = useUserStore()

const isLoading = ref(false)
const isDebugMode = computed(() => import.meta.env.VITE_DEBUG_MODE === 'true')

const formData = reactive({
  phone: '',
  password: '',
  keepLoggedIn: false,
})

// Phone validator
const phoneValidator = (value: string) => {
  const phoneRegex = /^1[3-9]\d{9}$/
  return phoneRegex.test(value) || '请输入有效的手机号'
}

// Password validator
const passwordValidator = (value: string) => {
  return value.length >= 6 || '密码至少6位'
}

// Debug login
const handleDebugLogin = () => {
  userStore.debugLogin()
  init({ message: '调试登录成功！', color: 'success' })
  push({ name: 'dashboard' })
}

// Submit
const submit = async () => {
  if (!validate()) return

  isLoading.value = true
  try {
    const result = await userStore.login({
      phone: formData.phone,
      password: formData.password,
    })

    if (result.success) {
      init({ message: '登录成功！', color: 'success' })
      push({ name: 'dashboard' })
    } else {
      init({ message: result.error || '登录失败，请检查手机号和密码', color: 'danger' })
    }
  } catch (error: any) {
    init({ message: error.message || '登录失败，请稍后重试', color: 'danger' })
  } finally {
    isLoading.value = false
  }
}
</script>
