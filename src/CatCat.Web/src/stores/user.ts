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

  function debugLogin() {
    const debugToken = 'debug_token_' + Date.now()
    const debugUser = {
      id: 1,
      phone: '13800138000',
      nickName: 'Debug User',
      email: 'debug@catcat.com',
      role: 1,
      avatar: '🐱'
    }

    token.value = debugToken
    userInfo.value = debugUser
    localStorage.setItem('token', debugToken)
    localStorage.setItem('userInfo', JSON.stringify(debugUser))
  }

  return {
    token,
    userInfo,
    isAuthenticated,
    loginUser,
    registerUser,
    logout,
    debugLogin
  }
})

