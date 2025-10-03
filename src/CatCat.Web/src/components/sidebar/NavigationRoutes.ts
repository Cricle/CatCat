export interface INavigationRoute {
  name: string
  displayName: string
  meta: { icon: string }
  children?: INavigationRoute[]
  roles?: number[] // 允许访问的角色列表，不设置则所有角色都可见
}

export default {
  root: {
    name: '/',
    displayName: 'navigationRoutes.home',
  },
  routes: [
    {
      name: 'dashboard',
      displayName: 'menu.dashboard',
      meta: {
        icon: 'vuestic-iconset-dashboard',
      },
    },
    {
      name: 'pets',
      displayName: 'menu.pets',
      meta: {
        icon: 'pets',
      },
      roles: [1, 99], // 客户和管理员
    },
    {
      name: 'packages',
      displayName: 'menu.packages',
      meta: {
        icon: 'inventory_2',
      },
      roles: [1, 99], // 客户和管理员
    },
    {
      name: 'orders',
      displayName: 'menu.orders',
      meta: {
        icon: 'receipt_long',
      },
      roles: [1, 99], // 客户和管理员
    },
    {
      name: 'providers',
      displayName: 'menu.providers',
      meta: {
        icon: 'people',
      },
      roles: [1, 99], // 客户和管理员
    },
    {
      name: 'provider',
      displayName: 'menu.provider',
      meta: {
        icon: 'work',
      },
      roles: [2], // 仅服务人员
      children: [
        {
          name: 'provider-available',
          displayName: 'menu.availableOrders',
        },
        {
          name: 'provider-tasks',
          displayName: 'menu.myTasks',
        },
        {
          name: 'provider-earnings',
          displayName: 'menu.myEarnings',
        },
      ],
    },
    {
      name: 'admin',
      displayName: 'menu.admin',
      meta: {
        icon: 'admin_panel_settings',
      },
      roles: [99], // 仅管理员
      children: [
        {
          name: 'admin-users',
          displayName: 'menu.adminUsers',
        },
        {
          name: 'admin-packages',
          displayName: 'menu.adminPackages',
        },
        {
          name: 'admin-orders',
          displayName: 'menu.adminOrders',
        },
      ],
    },
    {
      name: 'preferences',
      displayName: 'menu.preferences',
      meta: {
        icon: 'manage_accounts',
      },
    },
    {
      name: 'settings',
      displayName: 'menu.settings',
      meta: {
        icon: 'settings',
      },
    },
  ] as INavigationRoute[],
}
