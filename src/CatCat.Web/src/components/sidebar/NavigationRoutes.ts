export interface INavigationRoute {
  name: string
  displayName: string
  meta: { icon: string }
  children?: INavigationRoute[]
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
    },
    {
      name: 'packages',
      displayName: 'menu.packages',
      meta: {
        icon: 'inventory_2',
      },
    },
    {
      name: 'orders',
      displayName: 'menu.orders',
      meta: {
        icon: 'receipt_long',
      },
    },
    {
      name: 'provider',
      displayName: 'menu.provider',
      meta: {
        icon: 'work',
      },
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
