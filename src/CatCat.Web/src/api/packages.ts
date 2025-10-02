import request from './request'

export interface ServicePackage {
  id: number
  name: string
  description: string
  price: number
  duration: number // 服务时长（分钟）
  iconUrl?: string
  serviceItems: string
  isActive: boolean
  sortOrder: number
  createdAt: string
  updatedAt?: string
}

// 获取所有服务套餐
export const getServicePackages = () => {
  return request.get<ServicePackage[]>('/api/packages')
}

// 获取激活的服务套餐
export const getActivePackages = () => {
  return request.get<ServicePackage[]>('/api/packages/active')
}

// 获取套餐详情
export const getPackageById = (id: number) => {
  return request.get<ServicePackage>(`/api/packages/${id}`)
}

