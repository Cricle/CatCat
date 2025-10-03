import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router'
import { useUserStore } from '../stores/user-store'

/**
 * 登录检查守卫
 * 检查用户是否已登录，未登录则重定向到登录页
 */
export function authGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
) {
  const userStore = useUserStore()

  // 公开路由（不需要登录）
  const publicRoutes = ['/auth/login', '/auth/signup', '/auth/recover-password', '/404']

  // 检查是否是公开路由
  if (publicRoutes.some((route) => to.path.startsWith(route))) {
    next()
    return
  }

  // 检查是否已登录
  if (!userStore.isAuthenticated) {
    // 未登录，重定向到登录页，并保存原始目标路由
    next({
      path: '/auth/login',
      query: { redirect: to.fullPath },
    })
    return
  }

  // 已登录，允许访问
  next()
}

/**
 * 角色权限守卫
 * 检查用户角色是否有权限访问特定页面
 */
export function roleGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
) {
  const userStore = useUserStore()

  // 获取路由需要的角色权限
  const requiredRole = to.meta.requiresRole as number | undefined

  // 如果路由不需要特定角色，直接通过
  if (!requiredRole) {
    next()
    return
  }

  // 检查用户角色
  if (!userStore.user || userStore.user.role < requiredRole) {
    // 权限不足，重定向到 Dashboard 或 403 页面
    next({
      path: '/dashboard',
      query: { error: 'insufficient_permissions' },
    })
    return
  }

  // 权限检查通过
  next()
}

/**
 * 管理员权限守卫
 * 仅允许管理员访问
 */
export function adminGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
) {
  const userStore = useUserStore()

  // 检查是否是管理员（role = 99）
  if (!userStore.user || userStore.user.role !== 99) {
    next({
      path: '/dashboard',
      query: { error: 'admin_only' },
    })
    return
  }

  next()
}

/**
 * 服务人员权限守卫
 * 仅允许服务人员访问
 */
export function providerGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
) {
  const userStore = useUserStore()

  // 检查是否是服务人员（role = 2）
  if (!userStore.user || userStore.user.role < 2) {
    next({
      path: '/dashboard',
      query: { error: 'provider_only' },
    })
    return
  }

  next()
}

/**
 * 已登录重定向守卫
 * 如果用户已登录，访问登录/注册页时重定向到 Dashboard
 */
export function guestGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
) {
  const userStore = useUserStore()

  // 如果已登录，重定向到 Dashboard
  if (userStore.isAuthenticated) {
    // 检查是否有重定向目标
    const redirect = (to.query.redirect as string) || '/dashboard'
    next(redirect)
    return
  }

  // 未登录，允许访问登录/注册页
  next()
}

