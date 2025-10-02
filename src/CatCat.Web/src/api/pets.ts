import request from './request'

export interface Pet {
  id: number
  userId: number
  name: string
  type: number // 0: Cat, 1: Dog
  breed?: string
  age: number
  gender: number // 0: Male, 1: Female
  avatar?: string
  character?: string
  dietaryHabits?: string
  healthStatus?: string
  remarks?: string
  createdAt: string
  updatedAt?: string
}

export interface CreatePetParams {
  name: string
  type: number
  breed?: string
  age: number
  gender: number
  avatar?: string
  character?: string
  dietaryHabits?: string
  healthStatus?: string
  remarks?: string
}

// 获取我的宠物列表
export const getMyPets = () => {
  return request.get<Pet[]>('/api/pets')
}

// 获取宠物详情
export const getPetById = (id: number) => {
  return request.get<Pet>(`/api/pets/${id}`)
}

// 创建宠物
export const createPet = (data: CreatePetParams) => {
  return request.post<{ id: number }>('/api/pets', data)
}

// 更新宠物信息
export const updatePet = (id: number, data: Partial<CreatePetParams>) => {
  return request.put(`/api/pets/${id}`, data)
}

// 删除宠物
export const deletePet = (id: number) => {
  return request.delete(`/api/pets/${id}`)
}

