import apiClient from './apiClient'
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  UserProfile,
  Pet,
  ServicePackage,
  Order,
  OrderCreateRequest,
  ServiceProgress,
  Review,
} from '../types/catcat-types'

// ============================================
// Auth API
// ============================================
export const authApi = {
  login: (data: LoginRequest) => apiClient.post<AuthResponse>('/auth/login', data),
  register: (data: RegisterRequest) => apiClient.post<AuthResponse>('/auth/register', data),
  refresh: (refreshToken: string) =>
    apiClient.post<AuthResponse>('/auth/refresh', { refreshToken }),
  logout: () => apiClient.post('/auth/logout'),
}

// ============================================
// User API
// ============================================
export const userApi = {
  getProfile: () => apiClient.get<UserProfile>('/user/profile'),
  updateProfile: (data: Partial<UserProfile>) => apiClient.put<UserProfile>('/user/profile', data),
}

// ============================================
// Pet API
// ============================================
export const petApi = {
  getMyPets: () => apiClient.get<Pet[]>('/pet'),
  getPet: (id: number) => apiClient.get<Pet>(`/pet/${id}`),
  createPet: (data: Omit<Pet, 'id' | 'userId' | 'createdAt' | 'updatedAt'>) =>
    apiClient.post<Pet>('/pet', data),
  updatePet: (id: number, data: Partial<Pet>) => apiClient.put<Pet>(`/pet/${id}`, data),
  deletePet: (id: number) => apiClient.delete(`/pet/${id}`),
}

// ============================================
// Service Package API
// ============================================
export const servicePackageApi = {
  getAll: () => apiClient.get<ServicePackage[]>('/package'),
  getById: (id: number) => apiClient.get<ServicePackage>(`/package/${id}`),
}

// ============================================
// Order API
// ============================================
export const orderApi = {
  getMyOrders: (params?: { page?: number; pageSize?: number; status?: string }) =>
    apiClient.get<{ items: Order[]; total: number; page: number; pageSize: number }>('/order', {
      params,
    }),
  getOrderById: (id: number) => apiClient.get<Order>(`/order/${id}`),
  createOrder: (data: OrderCreateRequest) => apiClient.post<Order>('/order', data),
  cancelOrder: (id: number, reason: string) =>
    apiClient.post(`/order/${id}/cancel`, { reason }),
  acceptOrder: (id: number) => apiClient.post(`/order/${id}/accept`),
}

// ============================================
// Service Progress API
// ============================================
export const progressApi = {
  getByOrderId: (orderId: number) => apiClient.get<ServiceProgress[]>(`/progress/${orderId}`),
  updateProgress: (orderId: number, data: Partial<ServiceProgress>) =>
    apiClient.post<ServiceProgress>(`/progress/${orderId}`, data),
}

// ============================================
// Review API
// ============================================
export const reviewApi = {
  getByOrderId: (orderId: number) => apiClient.get<Review>(`/review/order/${orderId}`),
  createReview: (data: { orderId: number; rating: number; comment: string; photos?: string[] }) =>
    apiClient.post<Review>('/review', data),
  replyToReview: (reviewId: number, reply: string) =>
    apiClient.post(`/review/${reviewId}/reply`, { reply }),
}

// ============================================
// Admin API (Role: 99)
// ============================================
export const adminApi = {
  getStatistics: () =>
    apiClient.get<{
      totalUsers: number
      totalOrders: number
      totalRevenue: number
      totalPets: number
    }>('/admin/statistics'),
  getAllUsers: (params?: { page?: number; pageSize?: number; search?: string }) =>
    apiClient.get('/admin/users', { params }),
  getUserById: (id: number) => apiClient.get(`/admin/users/${id}`),
  updateUser: (id: number, data: Partial<UserProfile>) =>
    apiClient.put(`/admin/users/${id}`, data),
  deleteUser: (id: number) => apiClient.delete(`/admin/users/${id}`),
  getAllPackages: () => apiClient.get<ServicePackage[]>('/admin/packages'),
  createPackage: (data: Omit<ServicePackage, 'id'>) =>
    apiClient.post<ServicePackage>('/admin/packages', data),
  updatePackage: (id: number, data: Partial<ServicePackage>) =>
    apiClient.put<ServicePackage>(`/admin/packages/${id}`, data),
  deletePackage: (id: number) => apiClient.delete(`/admin/packages/${id}`),
  getAllPets: (params?: { page?: number; pageSize?: number }) =>
    apiClient.get('/admin/pets', { params }),
}

export default {
  auth: authApi,
  user: userApi,
  pet: petApi,
  package: servicePackageApi,
  order: orderApi,
  progress: progressApi,
  review: reviewApi,
  admin: adminApi,
}

