import request from './request'

export interface SendCodeRequest {
  phone: string
}

export interface RegisterRequest {
  phone: string
  code: string
  password: string
  nickName?: string
}

export interface LoginRequest {
  phone: string
  password: string
}

export function sendCode(data: SendCodeRequest) {
  return request.post('/auth/send-code', data)
}

export function register(data: RegisterRequest) {
  return request.post('/auth/register', data)
}

export function login(data: LoginRequest) {
  return request.post('/auth/login', data)
}

