import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login, register, type LoginRequest, type RegisterRequest } from '@/api/auth'

export const useUserStore = defineStore('user', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const userInfo = ref<any>(null)

  const isAuthenticated = computed(() => !!token.value)

  async function loginUser(data: LoginRequest) {
    const response = await login(data)
    token.value = response.data.token
    userInfo.value = response.data.user
    localStorage.setItem('token', response.data.token)
  }

  async function registerUser(data: RegisterRequest) {
    const response = await register(data)
    token.value = response.data.token
    userInfo.value = response.data.user
    localStorage.setItem('token', response.data.token)
  }

  function logout() {
    token.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
  }

  return {
    token,
    userInfo,
    isAuthenticated,
    loginUser,
    registerUser,
    logout
  }
})

