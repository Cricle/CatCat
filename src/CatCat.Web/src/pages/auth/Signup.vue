<template>
  <VaForm ref="form" @submit.prevent="submit">
    <h1 class="font-semibold text-4xl mb-4">注册账号</h1>
    <p class="text-base mb-4 leading-5">
      已有账号？
      <RouterLink :to="{ name: 'login' }" class="font-semibold text-primary">立即登录</RouterLink>
    </p>
    <VaInput
      v-model="formData.userName"
      :rules="[validators.required]"
      :disabled="isLoading"
      class="mb-4"
      label="用户名"
      placeholder="请输入用户名"
    />
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
        placeholder="至少6位"
        messages="密码至少6位，建议包含字母、数字和特殊字符"
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
      <VaInput
        v-model="formData.repeatPassword"
        :rules="[validators.required, (v: string) => v === formData.password || '两次密码不一致']"
        :type="isPasswordVisible.value ? 'text' : 'password'"
        :disabled="isLoading"
        class="mb-4"
        label="确认密码"
        placeholder="请再次输入密码"
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

    <div class="flex justify-center mt-4">
      <VaButton class="w-full" :loading="isLoading" @click="submit">
        {{ isLoading ? '注册中...' : '创建账号' }}
      </VaButton>
    </div>
  </VaForm>
</template>

<script lang="ts" setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useForm, useToast } from 'vuestic-ui'
import { validators } from '../../services/utils'
import { useUserStore } from '../../stores/user-store'

const { validate } = useForm('form')
const { push } = useRouter()
const { init } = useToast()
const userStore = useUserStore()

const isLoading = ref(false)

const formData = reactive({
  userName: '',
  phone: '',
  password: '',
  repeatPassword: '',
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

// Submit
const submit = async () => {
  if (!validate()) return

  isLoading.value = true
  try {
    const result = await userStore.register({
      userName: formData.userName,
      phone: formData.phone,
      password: formData.password,
    })

    if (result.success) {
      init({ message: '注册成功！', color: 'success' })
      push({ name: 'dashboard' })
    } else {
      init({ message: result.error || '注册失败，请稍后重试', color: 'danger' })
    }
  } catch (error: any) {
    init({ message: error.message || '注册失败，请稍后重试', color: 'danger' })
  } finally {
    isLoading.value = false
  }
}
</script>
