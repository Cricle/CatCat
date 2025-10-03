import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login, register, refreshToken as refreshTokenApi, logout as logoutApi, type LoginRequest, type RegisterRequest } from '@/api/auth'

export const useUserStore = defineStore('user', () => {
  const accessToken = ref<string>(localStorage.getItem('accessToken') || '')
  const refreshToken = ref<string>(localStorage.getItem('refreshToken') || '')
  const userInfo = ref<any>(JSON.parse(localStorage.getItem('userInfo') || 'null'))

  const isAuthenticated = computed(() => !!accessToken.value && !!refreshToken.value)

  async function loginUser(data: LoginRequest) {
    const response = await login(data)
    setTokens(response.data.accessToken, response.data.refreshToken)
    userInfo.value = response.data.user
    localStorage.setItem('userInfo', JSON.stringify(response.data.user))
  }

  async function registerUser(data: RegisterRequest) {
    const response = await register(data)
    setTokens(response.data.accessToken, response.data.refreshToken)
    userInfo.value = response.data.user
    localStorage.setItem('userInfo', JSON.stringify(response.data.user))
  }

  async function refreshAccessToken() {
    if (!refreshToken.value) {
      throw new Error('No refresh token available')
    }

    try {
      const response = await refreshTokenApi({ refreshToken: refreshToken.value })
      setTokens(response.data.accessToken, response.data.refreshToken)
      userInfo.value = response.data.user
      localStorage.setItem('userInfo', JSON.stringify(response.data.user))
      return response.data.accessToken
    } catch (error) {
      // If refresh token is invalid, logout user
      logout()
      throw error
    }
  }

  async function logout() {
    try {
      if (accessToken.value) {
        await logoutApi()
      }
    } catch (error) {
      console.error('Logout failed:', error)
    } finally {
      clearTokens()
    }
  }

  function setTokens(access: string, refresh: string) {
    accessToken.value = access
    refreshToken.value = refresh
    localStorage.setItem('accessToken', access)
    localStorage.setItem('refreshToken', refresh)
  }

  function clearTokens() {
    accessToken.value = ''
    refreshToken.value = ''
    userInfo.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userInfo')
  }

  function debugLogin() {
    const debugAccessToken = 'debug_access_token_' + Date.now()
    const debugRefreshToken = 'debug_refresh_token_' + Date.now()
    const debugUser = {
      id: 1,
      phone: '13800138000',
      nickName: 'Debug User',
      email: 'debug@catcat.com',
      role: 1,
      avatar: '🐱'
    }

    setTokens(debugAccessToken, debugRefreshToken)
    userInfo.value = debugUser
    localStorage.setItem('userInfo', JSON.stringify(debugUser))
  }

  return {
    accessToken,
    refreshToken,
    userInfo,
    isAuthenticated,
    loginUser,
    registerUser,
    refreshAccessToken,
    logout,
    debugLogin
  }
})
