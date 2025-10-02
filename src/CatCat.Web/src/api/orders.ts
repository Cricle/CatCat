import request from './request'

export interface Order {
  id: number
  orderNo: string
  customerId: number
  serviceProviderId?: number
  petId: number
  servicePackageId: number
  serviceDate: string
  serviceTime: string
  address: string
  addressDetail?: string
  price: number
  status: number
  customerRemark?: string
  serviceRemark?: string
  createdAt: string
}

export interface OrderListParams {
  page: number
  pageSize: number
  status?: number
}

export interface OrderListResponse {
  items: Order[]
  total: number
  page: number
  pageSize: number
}

export interface CreateOrderParams {
  petId: number
  servicePackageId: number
  serviceDate: string
  serviceTime: string
  address: string
  addressDetail?: string
  customerRemark?: string
}

// 获取我的订单列表
export const getMyOrders = (params: OrderListParams) => {
  return request.get<OrderListResponse>('/api/orders', { params })
}

// 获取订单详情
export const getOrderDetail = (id: number) => {
  return request.get<Order>(`/api/orders/${id}`)
}

// 创建订单
export const createOrder = (data: CreateOrderParams) => {
  return request.post<{ orderId: number; orderNo: string }>('/api/orders', data)
}

// 取消订单
export const cancelOrder = (id: number, reason: string) => {
  return request.post(`/api/orders/${id}/cancel`, { reason })
}

// 接单（服务人员）
export const acceptOrder = (id: number) => {
  return request.post(`/api/orders/${id}/accept`)
}

// 开始服务
export const startOrder = (id: number) => {
  return request.post(`/api/orders/${id}/start`)
}

// 完成服务
export const completeOrder = (id: number, serviceRemark?: string) => {
  return request.post(`/api/orders/${id}/complete`, { serviceRemark })
}

