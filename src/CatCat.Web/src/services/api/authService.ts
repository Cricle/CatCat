import { BaseApiService } from './base'

/**
 * 登录请求
 */
export interface LoginRequest {
  phoneNumber: string
  password: string
}

/**
 * 登录响应
 */
export interface LoginResponse {
  token: string
  refreshToken: string
  expiresIn: number
  user: {
    id: number
    phoneNumber: string
    nickName: string
    role: string
    avatar?: string
  }
}

/**
 * 注册请求
 */
export interface RegisterRequest {
  phoneNumber: string
  password: string
  nickName: string
  verificationCode: string
}

/**
 * 验证码请求
 */
export interface SendCodeRequest {
  phoneNumber: string
  purpose: 'register' | 'login' | 'reset-password'
}

/**
 * 认证服务
 */
class AuthService extends BaseApiService {
  /**
   * 用户登录
   */
  async login(data: LoginRequest): Promise<LoginResponse> {
    return this.post<LoginResponse>('/api/auth/login', data)
  }

  /**
   * 用户注册
   */
  async register(data: RegisterRequest): Promise<LoginResponse> {
    return this.post<LoginResponse>('/api/auth/register', data)
  }

  /**
   * 发送验证码
   */
  async sendVerificationCode(data: SendCodeRequest): Promise<void> {
    return this.post<void>('/api/auth/send-code', data)
  }

  /**
   * 刷新 Token
   */
  async refreshToken(refreshToken: string): Promise<LoginResponse> {
    return this.post<LoginResponse>('/api/auth/refresh', { refreshToken })
  }

  /**
   * 退出登录
   */
  async logout(): Promise<void> {
    return this.post<void>('/api/auth/logout')
  }

  /**
   * 修改密码
   */
  async changePassword(oldPassword: string, newPassword: string): Promise<void> {
    return this.post<void>('/api/auth/change-password', { oldPassword, newPassword })
  }

  /**
   * 重置密码
   */
  async resetPassword(phoneNumber: string, code: string, newPassword: string): Promise<void> {
    return this.post<void>('/api/auth/reset-password', { phoneNumber, code, newPassword })
  }
}

export const authService = new AuthService()

