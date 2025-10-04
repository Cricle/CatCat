import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse, AxiosError } from 'axios'
import { useUserStore } from '@/stores/userStore'
import router from '@/router'

/**
 * API 响应统一格式
 */
export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  code?: string
}

/**
 * 分页响应格式
 */
export interface PagedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

/**
 * API 基础配置
 */
const baseConfig: AxiosRequestConfig = {
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
}

/**
 * 创建 Axios 实例
 */
const apiClient: AxiosInstance = axios.create(baseConfig)

/**
 * 请求拦截器
 */
apiClient.interceptors.request.use(
  (config) => {
    const userStore = useUserStore()
    
    // 添加 JWT Token
    if (userStore.token) {
      config.headers.Authorization = `Bearer ${userStore.token}`
    }
    
    // 添加请求ID用于追踪
    config.headers['X-Request-ID'] = crypto.randomUUID()
    
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

/**
 * 响应拦截器
 */
apiClient.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    return response
  },
  async (error: AxiosError<ApiResponse>) => {
    const userStore = useUserStore()
    
    // 处理 401 未授权
    if (error.response?.status === 401) {
      userStore.logout()
      router.push({ name: 'Login' })
      return Promise.reject(new Error('未授权，请重新登录'))
    }
    
    // 处理 403 禁止访问
    if (error.response?.status === 403) {
      return Promise.reject(new Error('无权限访问'))
    }
    
    // 处理 404 未找到
    if (error.response?.status === 404) {
      return Promise.reject(new Error('请求的资源不存在'))
    }
    
    // 处理 429 请求过多
    if (error.response?.status === 429) {
      return Promise.reject(new Error('请求过于频繁，请稍后再试'))
    }
    
    // 处理 500 服务器错误
    if (error.response?.status === 500) {
      return Promise.reject(new Error('服务器内部错误'))
    }
    
    // 处理网络错误
    if (!error.response) {
      return Promise.reject(new Error('网络连接失败'))
    }
    
    // 返回错误消息
    const message = error.response?.data?.message || error.message || '请求失败'
    return Promise.reject(new Error(message))
  }
)

/**
 * 基础 API 服务类
 */
export class BaseApiService {
  protected client: AxiosInstance

  constructor() {
    this.client = apiClient
  }

  /**
   * GET 请求
   */
  protected async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<ApiResponse<T>>(url, config)
    if (!response.data.success) {
      throw new Error(response.data.message || '请求失败')
    }
    return response.data.data as T
  }

  /**
   * POST 请求
   */
  protected async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.post<ApiResponse<T>>(url, data, config)
    if (!response.data.success) {
      throw new Error(response.data.message || '请求失败')
    }
    return response.data.data as T
  }

  /**
   * PUT 请求
   */
  protected async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.put<ApiResponse<T>>(url, data, config)
    if (!response.data.success) {
      throw new Error(response.data.message || '请求失败')
    }
    return response.data.data as T
  }

  /**
   * DELETE 请求
   */
  protected async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.delete<ApiResponse<T>>(url, config)
    if (!response.data.success) {
      throw new Error(response.data.message || '请求失败')
    }
    return response.data.data as T
  }

  /**
   * PATCH 请求
   */
  protected async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.patch<ApiResponse<T>>(url, data, config)
    if (!response.data.success) {
      throw new Error(response.data.message || '请求失败')
    }
    return response.data.data as T
  }
}

export { apiClient }

