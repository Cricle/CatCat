import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'

import AuthLayout from '../layouts/AuthLayout.vue'
import AppLayout from '../layouts/AppLayout.vue'

import RouteViewComponent from '../layouts/RouterBypass.vue'
import { authGuard, guestGuard, roleGuard } from './guards'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/:pathMatch(.*)*',
    redirect: { name: 'dashboard' },
  },
  {
    name: 'admin',
    path: '/',
    component: AppLayout,
    redirect: { name: 'dashboard' },
    children: [
      {
        name: 'dashboard',
        path: 'dashboard',
        component: () => import('../pages/admin/dashboard/Dashboard.vue'),
      },
      {
        name: 'settings',
        path: 'settings',
        component: () => import('../pages/settings/Settings.vue'),
      },
      {
        name: 'preferences',
        path: 'preferences',
        component: () => import('../pages/preferences/Preferences.vue'),
      },
      {
        name: 'pets',
        path: 'pets',
        component: () => import('../pages/pets/PetsPage.vue'),
      },
      {
        name: 'orders',
        path: 'orders',
        component: () => import('../pages/orders/OrdersPage.vue'),
      },
      {
        name: 'create-order',
        path: 'orders/create',
        component: () => import('../pages/orders/CreateOrderPage.vue'),
      },
      {
        name: 'order-detail',
        path: 'orders/:id',
        component: () => import('../pages/orders/OrderDetailPage.vue'),
      },
      {
        name: 'provider-available',
        path: 'provider/available',
        component: () => import('../pages/provider/AvailableOrdersPage.vue'),
        meta: { requiresRole: 2 }, // Service provider role
      },
      {
        name: 'provider-tasks',
        path: 'provider/tasks',
        component: () => import('../pages/provider/MyTasksPage.vue'),
        meta: { requiresRole: 2 },
      },
      {
        name: 'provider-progress',
        path: 'provider/progress/:id',
        component: () => import('../pages/provider/ProgressUpdatePage.vue'),
        meta: { requiresRole: 2 },
      },
      {
        name: 'users',
        path: 'users',
        component: () => import('../pages/users/UsersPage.vue'),
      },
      {
        name: 'projects',
        path: 'projects',
        component: () => import('../pages/projects/ProjectsPage.vue'),
      },
      {
        name: 'payments',
        path: '/payments',
        component: RouteViewComponent,
        children: [
          {
            name: 'payment-methods',
            path: 'payment-methods',
            component: () => import('../pages/payments/PaymentsPage.vue'),
          },
          {
            name: 'billing',
            path: 'billing',
            component: () => import('../pages/billing/BillingPage.vue'),
          },
          {
            name: 'pricing-plans',
            path: 'pricing-plans',
            component: () => import('../pages/pricing-plans/PricingPlans.vue'),
          },
        ],
      },
      {
        name: 'faq',
        path: '/faq',
        component: () => import('../pages/faq/FaqPage.vue'),
      },
    ],
  },
  {
    path: '/auth',
    component: AuthLayout,
    children: [
      {
        name: 'login',
        path: 'login',
        component: () => import('../pages/auth/Login.vue'),
      },
      {
        name: 'signup',
        path: 'signup',
        component: () => import('../pages/auth/Signup.vue'),
      },
      {
        name: 'recover-password',
        path: 'recover-password',
        component: () => import('../pages/auth/RecoverPassword.vue'),
      },
      {
        name: 'recover-password-email',
        path: 'recover-password-email',
        component: () => import('../pages/auth/CheckTheEmail.vue'),
      },
      {
        path: '',
        redirect: { name: 'login' },
      },
    ],
  },
  {
    name: '404',
    path: '/404',
    component: () => import('../pages/404.vue'),
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }
    // For some reason using documentation example doesn't scroll on page navigation.
    if (to.hash) {
      return { el: to.hash, behavior: 'smooth' }
    } else {
      window.scrollTo(0, 0)
    }
  },
  routes,
})

// Global navigation guards
router.beforeEach((to, from, next) => {
  // Check if route is public (auth pages)
  const isAuthRoute = to.path.startsWith('/auth')

  if (isAuthRoute) {
    // Apply guest guard (redirect if already logged in)
    guestGuard(to, from, next)
  } else {
    // Apply auth guard (require login) then role guard (check permissions)
    authGuard(to, from, (result) => {
      if (result === undefined || result === true) {
        // Auth check passed, now check role permissions
        roleGuard(to, from, next)
      } else {
        // Auth check failed, result is the redirect
        next(result)
      }
    })
  }
})

export default router
