import request from '@/api/request'

export interface ServiceProgress {
  id: number
  orderId: number
  serviceProviderId: number
  status: ServiceProgressStatus
  description?: string
  latitude?: number
  longitude?: number
  address?: string
  imageUrls?: string
  createdAt: string
}

export enum ServiceProgressStatus {
  OnTheWay = 1,
  Arrived = 2,
  StartService = 3,
  Feeding = 4,
  CleaningLitter = 5,
  Playing = 6,
  Grooming = 7,
  TakingPhotos = 8,
  Completed = 9
}

export const getOrderProgress = (orderId: number) => {
  return request.get<ServiceProgress[]>(`/api/service-progress/order/${orderId}`)
}

export const getLatestProgress = (orderId: number) => {
  return request.get<ServiceProgress>(`/api/service-progress/order/${orderId}/latest`)
}

export const createProgress = (data: {
  orderId: number
  status: ServiceProgressStatus
  description?: string
  latitude?: number
  longitude?: number
  address?: string
  imageUrls?: string
}) => {
  return request.post('/api/service-progress', data)
}

export const getProgressStatusText = (status: ServiceProgressStatus): string => {
  const statusMap: Record<ServiceProgressStatus, string> = {
    [ServiceProgressStatus.OnTheWay]: 'On the way',
    [ServiceProgressStatus.Arrived]: 'Arrived',
    [ServiceProgressStatus.StartService]: 'Service started',
    [ServiceProgressStatus.Feeding]: 'Feeding',
    [ServiceProgressStatus.CleaningLitter]: 'Cleaning litter box',
    [ServiceProgressStatus.Playing]: 'Playing with cat',
    [ServiceProgressStatus.Grooming]: 'Grooming',
    [ServiceProgressStatus.TakingPhotos]: 'Taking photos',
    [ServiceProgressStatus.Completed]: 'Service completed'
  }
  return statusMap[status] || 'Unknown'
}

export const getProgressStatusIcon = (status: ServiceProgressStatus): string => {
  const iconMap: Record<ServiceProgressStatus, string> = {
    [ServiceProgressStatus.OnTheWay]: 'directions_car',
    [ServiceProgressStatus.Arrived]: 'location_on',
    [ServiceProgressStatus.StartService]: 'play_circle',
    [ServiceProgressStatus.Feeding]: 'restaurant',
    [ServiceProgressStatus.CleaningLitter]: 'cleaning_services',
    [ServiceProgressStatus.Playing]: 'pets',
    [ServiceProgressStatus.Grooming]: 'content_cut',
    [ServiceProgressStatus.TakingPhotos]: 'photo_camera',
    [ServiceProgressStatus.Completed]: 'check_circle'
  }
  return iconMap[status] || 'info'
}

export const getProgressStatusColor = (status: ServiceProgressStatus): string => {
  const colorMap: Record<ServiceProgressStatus, string> = {
    [ServiceProgressStatus.OnTheWay]: 'info',
    [ServiceProgressStatus.Arrived]: 'warning',
    [ServiceProgressStatus.StartService]: 'primary',
    [ServiceProgressStatus.Feeding]: 'success',
    [ServiceProgressStatus.CleaningLitter]: 'info',
    [ServiceProgressStatus.Playing]: 'success',
    [ServiceProgressStatus.Grooming]: 'primary',
    [ServiceProgressStatus.TakingPhotos]: 'warning',
    [ServiceProgressStatus.Completed]: 'success'
  }
  return colorMap[status] || 'secondary'
}

