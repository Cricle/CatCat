import { defineStore } from 'pinia'
import { authApi, userApi } from '../services/catcat-api'
import type { UserProfile, LoginRequest, RegisterRequest } from '../types/catcat-types'

interface UserState {
  user: UserProfile | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
}

export const useUserStore = defineStore('user', {
  state: (): UserState => {
    return {
      user: JSON.parse(localStorage.getItem('user') || 'null'),
      accessToken: localStorage.getItem('accessToken'),
      refreshToken: localStorage.getItem('refreshToken'),
      isAuthenticated: !!localStorage.getItem('accessToken'),
      isLoading: false,
    }
  },

  getters: {
    userName: (state) => state.user?.userName || '',
    userRole: (state) => state.user?.role || 1,
    isAdmin: (state) => state.user?.role === 99,
    isServiceProvider: (state) => state.user?.role === 2,
    isCustomer: (state) => state.user?.role === 1,
    userAvatar: (state) => state.user?.avatar || 'https://picsum.photos/id/22/200/300',
    memberSince: (state) => state.user?.createdAt || '',
  },

  actions: {
    // Login
    async login(credentials: LoginRequest) {
      this.isLoading = true
      try {
        const response = await authApi.login(credentials)
        const { accessToken, refreshToken, user } = response.data

        // Save to state
        this.user = user
        this.accessToken = accessToken
        this.refreshToken = refreshToken
        this.isAuthenticated = true

        // Save to localStorage
        localStorage.setItem('user', JSON.stringify(user))
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', refreshToken)

        return { success: true }
      } catch (error: any) {
        console.error('Login failed:', error)
        return { success: false, error: error.response?.data?.error || 'Login failed' }
      } finally {
        this.isLoading = false
      }
    },

    // Register
    async register(data: RegisterRequest) {
      this.isLoading = true
      try {
        const response = await authApi.register(data)
        const { accessToken, refreshToken, user } = response.data

        // Save to state
        this.user = user
        this.accessToken = accessToken
        this.refreshToken = refreshToken
        this.isAuthenticated = true

        // Save to localStorage
        localStorage.setItem('user', JSON.stringify(user))
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', refreshToken)

        return { success: true }
      } catch (error: any) {
        console.error('Register failed:', error)
        return { success: false, error: error.response?.data?.error || 'Registration failed' }
      } finally {
        this.isLoading = false
      }
    },

    // Logout
    async logout() {
      try {
        if (this.refreshToken) {
          await authApi.logout()
        }
      } catch (error) {
        console.error('Logout API call failed:', error)
      } finally {
        // Clear state
        this.user = null
        this.accessToken = null
        this.refreshToken = null
        this.isAuthenticated = false

        // Clear localStorage
        localStorage.removeItem('user')
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
      }
    },

    // Get user profile
    async fetchProfile() {
      this.isLoading = true
      try {
        const response = await userApi.getProfile()
        this.user = response.data
        localStorage.setItem('user', JSON.stringify(response.data))
        return { success: true }
      } catch (error: any) {
        console.error('Fetch profile failed:', error)
        return { success: false, error: error.response?.data?.error || 'Failed to fetch profile' }
      } finally {
        this.isLoading = false
      }
    },

    // Update user profile
    async updateProfile(data: Partial<UserProfile>) {
      this.isLoading = true
      try {
        const response = await userApi.updateProfile(data)
        this.user = response.data
        localStorage.setItem('user', JSON.stringify(response.data))
        return { success: true }
      } catch (error: any) {
        console.error('Update profile failed:', error)
        return { success: false, error: error.response?.data?.error || 'Failed to update profile' }
      } finally {
        this.isLoading = false
      }
    },

    // Debug: Skip login (for development)
    debugLogin() {
      if (import.meta.env.VITE_DEBUG_MODE === 'true') {
        const mockUser: UserProfile = {
          id: 1,
          phone: '13800138000',
          userName: 'Debug User',
          role: 1,
          avatar: 'https://picsum.photos/id/22/200/300',
          createdAt: new Date().toISOString(),
        }
        this.user = mockUser
        this.accessToken = 'debug-token'
        this.refreshToken = 'debug-refresh-token'
        this.isAuthenticated = true
        localStorage.setItem('user', JSON.stringify(mockUser))
        localStorage.setItem('accessToken', 'debug-token')
        localStorage.setItem('refreshToken', 'debug-refresh-token')
      }
    },
  },
})
