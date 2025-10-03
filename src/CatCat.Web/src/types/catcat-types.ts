// ============================================
// Auth Types
// ============================================
export interface LoginRequest {
  phone: string
  password: string
}

export interface RegisterRequest {
  phone: string
  password: string
  userName: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  user: UserProfile
}

// ============================================
// User Types
// ============================================
export interface UserProfile {
  id: number
  phone: string
  userName: string
  role: UserRole
  avatar?: string
  createdAt: string
}

export enum UserRole {
  Customer = 1, // C端用户
  ServiceProvider = 2, // B端服务商
  Admin = 99, // 管理员
}

// ============================================
// Pet Types
// ============================================
export interface Pet {
  id: number
  userId: number
  name: string
  type: PetType
  breed?: string
  age: number
  gender: Gender
  avatar?: string
  character?: string
  dietaryHabits?: string
  healthStatus?: string
  remarks?: string
  // Service Information (解决上门服务痛点)
  foodLocationImage?: string
  foodLocationDesc?: string
  waterLocationImage?: string
  waterLocationDesc?: string
  litterBoxLocationImage?: string
  litterBoxLocationDesc?: string
  cleaningSuppliesImage?: string
  cleaningSuppliesDesc?: string
  needsWaterRefill: boolean
  specialInstructions?: string
  createdAt: string
  updatedAt?: string
}

export enum PetType {
  Cat = 1,
  Dog = 2,
  Other = 99,
}

export enum Gender {
  Unknown = 0,
  Male = 1,
  Female = 2,
}

// ============================================
// Service Package Types
// ============================================
export interface ServicePackage {
  id: number
  name: string
  description: string
  price: number
  duration: number // minutes
  isActive: boolean
  createdAt: string
}

// ============================================
// Order Types
// ============================================
export interface Order {
  id: number
  orderNo: string
  userId: number
  serviceProviderId?: number
  petId: number
  packageId: number
  serviceDate: string
  serviceTime: string
  address: string
  status: OrderStatus
  totalAmount: number
  remarks?: string
  createdAt: string
  updatedAt?: string
  // Related data
  pet?: Pet
  package?: ServicePackage
}

export enum OrderStatus {
  Queued = 0, // 队列中
  Pending = 1, // 待接单
  Accepted = 2, // 已接单
  InProgress = 3, // 服务中
  Completed = 4, // 已完成
  Cancelled = 5, // 已取消
}

export interface OrderCreateRequest {
  petId: number
  packageId: number
  serviceDate: string
  serviceTime: string
  address: string
  remarks?: string
}

// ============================================
// Service Progress Types
// ============================================
export interface ServiceProgress {
  id: number
  orderId: number
  status: ProgressStatus
  location?: string
  latitude?: number
  longitude?: number
  photos?: string[]
  remarks?: string
  createdAt: string
}

export enum ProgressStatus {
  Pending = 0, // 待开始
  OnTheWay = 1, // 前往中
  Arrived = 2, // 已到达
  Feeding = 3, // 喂食中
  WaterRefill = 4, // 换水中
  LitterCleaning = 5, // 清理猫砂中
  Playing = 6, // 陪玩中
  PhotoTaking = 7, // 拍照记录中
  Completed = 8, // 已完成
}

// ============================================
// Review Types
// ============================================
export interface Review {
  id: number
  orderId: number
  userId: number
  rating: number
  comment: string
  photos?: string[]
  reply?: string
  createdAt: string
}

// ============================================
// API Response Wrapper
// ============================================
export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: string
  message?: string
}

