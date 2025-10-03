import request from './request'
import type { PagedResult } from './types'

// Statistics
export interface AdminStatistics {
  totalUsers: number
  activeUsers: number
  totalPets: number
  totalOrders: number
  pendingOrders: number
  totalPackages: number
  activePackages: number
}

export function getStatistics() {
  return request.get<AdminStatistics>('/api/admin/statistics')
}

// User Management
export interface User {
  id: number
  phone: string
  email?: string
  nickName: string
  avatar?: string
  role: number // 1: Customer, 2: ServiceProvider, 99: Admin
  status: number // 0: Pending, 1: Active, 2: Suspended, 3: Banned
  createdAt: string
  updatedAt?: string
}

export function getUsers(params: {
  page?: number
  pageSize?: number
  role?: number
  status?: number
}) {
  return request.get<PagedResult<User>>('/api/admin/users', { params })
}

export function updateUserStatus(id: number, status: number) {
  return request.put(`/api/admin/users/${id}/status`, { status })
}

export function updateUserRole(id: number, role: number) {
  return request.put(`/api/admin/users/${id}/role`, { role })
}

// Pet Management
export interface Pet {
  id: number
  userId: number
  name: string
  breed?: string
  age: number
  type: number
  gender: number
  weight?: number
  avatar?: string
  character?: string
  dietaryHabits?: string
  healthStatus?: string
  remarks?: string
  createdAt: string
}

export function getAllPets(params: {
  page?: number
  pageSize?: number
  userId?: number
}) {
  return request.get<PagedResult<Pet>>('/api/admin/pets', { params })
}

export function deletePet(id: number) {
  return request.delete(`/api/admin/pets/${id}`)
}

// Package Management
export interface ServicePackage {
  id: number
  name: string
  description: string
  price: number
  duration: number
  iconUrl?: string
  serviceItems: string
  isActive: boolean
  sortOrder: number
  createdAt: string
  updatedAt?: string
}

export function getPackages(params: { page?: number; pageSize?: number }) {
  return request.get<PagedResult<ServicePackage>>('/api/admin/packages', {
    params
  })
}

export function createPackage(data: {
  name: string
  description: string
  price: number
  duration: number
  iconUrl?: string
  serviceItems: string
  sortOrder: number
}) {
  return request.post('/api/admin/packages', data)
}

export function updatePackage(
  id: number,
  data: {
    name: string
    description: string
    price: number
    duration: number
    iconUrl?: string
    serviceItems: string
    isActive: boolean
    sortOrder: number
  }
) {
  return request.put(`/api/admin/packages/${id}`, data)
}

export function deletePackage(id: number) {
  return request.delete(`/api/admin/packages/${id}`)
}

export function togglePackageStatus(id: number, isActive: boolean) {
  return request.patch(`/api/admin/packages/${id}/toggle`, { isActive })
}

