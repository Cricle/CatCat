import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/Login.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/auth/Register.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/',
      redirect: '/home',
      component: () => import('@/layouts/MainLayout.vue'),
      children: [
        {
          path: '/home',
          name: 'home',
          component: () => import('@/views/Home.vue'),
          meta: { requiresAuth: true }
        },
        {
          path: '/orders',
          name: 'orders',
          component: () => import('@/views/Orders.vue'),
          meta: { requiresAuth: true }
        },
        {
          path: '/profile',
          name: 'profile',
          component: () => import('@/views/Profile.vue'),
          meta: { requiresAuth: true }
        },
        {
          path: '/pets',
          name: 'pets',
          component: () => import('@/views/Pets.vue'),
          meta: { requiresAuth: true }
        }
      ]
    },
    {
      path: '/order/create',
      name: 'order-create',
      component: () => import('@/views/CreateOrder.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/orders/:id',
      name: 'order-detail',
      component: () => import('@/views/OrderDetail.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/admin',
      redirect: '/admin/dashboard',
      children: [
        {
          path: '/admin/dashboard',
          name: 'admin-dashboard',
          component: () => import('@/views/admin/Dashboard.vue'),
          meta: { requiresAuth: true, requiresAdmin: true }
        },
        {
          path: '/admin/users',
          name: 'admin-users',
          component: () => import('@/views/admin/Users.vue'),
          meta: { requiresAuth: true, requiresAdmin: true }
        },
        {
          path: '/admin/pets',
          name: 'admin-pets',
          component: () => import('@/views/admin/Pets.vue'),
          meta: { requiresAuth: true, requiresAdmin: true }
        },
        {
          path: '/admin/packages',
          name: 'admin-packages',
          component: () => import('@/views/admin/Packages.vue'),
          meta: { requiresAuth: true, requiresAdmin: true }
        }
      ]
    }
  ]
})

// Navigation guard
router.beforeEach((to, _from, next) => {
  const userStore = useUserStore()

  if (to.meta.requiresAuth && !userStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if (to.meta.requiresAdmin && userStore.userInfo?.role !== 99) {
    // Check if user is admin (role 99)
    next({ name: 'home' })
  } else {
    next()
  }
})

export default router

