# Vuestic Admin 集成待办清单

## ✅ 已完成 (2025-10-03)
- [x] 克隆 Vuestic Admin 模板到 `src/CatCat.Web`
- [x] 安装依赖 (`npm install --legacy-peer-deps`)
- [x] 验证构建成功
- [x] 创建迁移文档 (`VUESTIC_MIGRATION.md`)
- [x] 更新主 README

## 🔄 进行中

### 1. API 集成 (高优先级) ⏳
- [ ] 配置 Axios 基础地址
  ```typescript
  // src/services/api.ts
  baseURL: 'http://localhost:5000/api'
  ```
- [ ] 实现 JWT Token 拦截器
  - [ ] 请求拦截器（添加 Authorization Header）
  - [ ] 响应拦截器（处理 401，刷新 Token）
- [ ] 更新 `user-store.ts` 对接后端认证 API
  - [ ] `/api/auth/login` - 登录
  - [ ] `/api/auth/register` - 注册
  - [ ] `/api/auth/refresh` - 刷新 Token
  - [ ] `/api/auth/logout` - 登出

### 2. 页面定制 (高优先级) 🎨
#### 🐱 宠物管理页面
- [ ] 创建 `src/pages/pets/PetsPage.vue`
  - [ ] 宠物列表展示（卡片视图）
  - [ ] 添加宠物表单（包含服务信息字段）
    - 猫粮位置 (FoodLocation)
    - 水盆位置 (WaterLocation)
    - 猫砂盆位置 (LitterBoxLocation)
    - 清洁用品位置 (CleaningSuppliesLocation)
    - 是否需要备水 (NeedsWaterRefill)
    - 特殊说明 (SpecialInstructions)
  - [ ] 编辑宠物
  - [ ] 删除宠物
- [ ] 创建 `src/pages/pets/widgets/`
  - [ ] `PetCard.vue` - 宠物卡片组件
  - [ ] `PetForm.vue` - 宠物表单组件
  - [ ] `ServiceInfoDisplay.vue` - 服务信息展示组件

#### 📦 订单管理页面
- [ ] 创建 `src/pages/orders/OrdersPage.vue`
  - [ ] 订单列表（时间线视图）
  - [ ] 订单状态过滤
    - Queued (队列中)
    - Pending (待接单)
    - Accepted (已接单)
    - InProgress (服务中)
    - Completed (已完成)
    - Cancelled (已取消)
  - [ ] 订单搜索
- [ ] 创建 `src/pages/orders/OrderDetail.vue`
  - [ ] 订单详情展示
  - [ ] 服务进度展示（参考美团）
  - [ ] 服务位置（地图）
  - [ ] 服务照片展示
  - [ ] 评价功能
- [ ] 创建 `src/pages/orders/CreateOrder.vue`
  - [ ] 选择宠物
  - [ ] 选择服务套餐
  - [ ] 选择日期时间
  - [ ] 订单确认

#### 📍 服务进度跟踪
- [ ] 创建 `src/pages/progress/ProgressTracking.vue`
  - [ ] 9个服务状态（参考美团外卖）
    - Pending (待开始)
    - OnTheWay (前往中)
    - Arrived (已到达)
    - Feeding (喂食中)
    - WaterRefill (换水中)
    - LitterCleaning (清理猫砂中)
    - Playing (陪玩中)
    - PhotoTaking (拍照记录中)
    - Completed (已完成)
  - [ ] 时间线展示
  - [ ] 地图定位（集成地图组件）
  - [ ] 照片上传/展示
  - [ ] 自动刷新（轮询或 WebSocket）

#### 🎫 服务套餐页面
- [ ] 创建 `src/pages/services/ServicesPage.vue`
  - [ ] 套餐列表展示（卡片视图）
  - [ ] 套餐详情（包含价格、时长、服务内容）
  - [ ] 立即预约按钮

#### ⚙️ 管理员页面
- [ ] 调整 `src/pages/admin/dashboard/`
  - [ ] 订单统计
  - [ ] 收入统计
  - [ ] 用户统计
  - [ ] 宠物统计
- [ ] 创建 `src/pages/admin/users/`
  - [ ] 用户管理（CRUD）
- [ ] 创建 `src/pages/admin/packages/`
  - [ ] 服务套餐管理（CRUD）
- [ ] 创建 `src/pages/admin/pets/`
  - [ ] 宠物管理（查看/编辑）

### 3. 路由配置 (高优先级) 🛣️
- [ ] 更新 `src/router/index.ts`
  ```typescript
  {
    path: '/pets',
    name: 'pets',
    component: () => import('../pages/pets/PetsPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/orders',
    name: 'orders',
    component: () => import('../pages/orders/OrdersPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/orders/create',
    name: 'create-order',
    component: () => import('../pages/orders/CreateOrder.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/orders/:id',
    name: 'order-detail',
    component: () => import('../pages/orders/OrderDetail.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/services',
    name: 'services',
    component: () => import('../pages/services/ServicesPage.vue')
  },
  {
    path: '/progress/:orderId',
    name: 'progress',
    component: () => import('../pages/progress/ProgressTracking.vue'),
    meta: { requiresAuth: true }
  }
  ```
- [ ] 实现路由守卫（检查登录状态）

### 4. 导航菜单 (中优先级) 📍
- [ ] 更新 `src/components/sidebar/NavigationRoutes.ts`
  ```typescript
  {
    name: 'pets',
    displayName: 'menu.pets',  // 我的宠物
    meta: { icon: 'pets' }
  },
  {
    name: 'orders',
    displayName: 'menu.orders',  // 我的订单
    meta: { icon: 'receipt_long' }
  },
  {
    name: 'services',
    displayName: 'menu.services',  // 服务套餐
    meta: { icon: 'business_center' }
  }
  ```
- [ ] 更新 `src/i18n/locales/cn.json` 添加中文翻译
- [ ] 更新 `src/i18n/locales/gb.json` 添加英文翻译

### 5. 国际化 (中优先级) 🌐
- [ ] 更新 `src/i18n/locales/cn.json`
  ```json
  {
    "menu": {
      "pets": "我的宠物",
      "orders": "我的订单",
      "services": "服务套餐"
    },
    "pets": {
      "title": "我的宠物",
      "addPet": "添加宠物",
      "foodLocation": "猫粮位置",
      "waterLocation": "水盆位置",
      ...
    },
    "orders": {
      "title": "我的订单",
      "createOrder": "创建订单",
      "orderStatus": {
        "queued": "队列中",
        "pending": "待接单",
        ...
      }
    }
  }
  ```

### 6. 第三方集成 (低优先级) 🗺️
- [ ] 集成地图组件（高德地图 / Google Maps）
  - [ ] 服务位置展示
  - [ ] 实时位置更新
- [ ] 集成图片上传组件
  - [ ] 服务照片上传
  - [ ] 宠物头像上传
  - [ ] 图片预览

### 7. 清理示例页面 (低优先级) 🧹
- [ ] 删除 `src/pages/billing/` （如不需要）
- [ ] 删除 `src/pages/pricing-plans/`
- [ ] 删除 `src/pages/faq/`
- [ ] 删除 `src/pages/payments/`
- [ ] 保留 `src/pages/users/` 作为参考
- [ ] 保留 `src/pages/projects/` 作为参考

## 📚 参考资源
- **Vuestic Admin GitHub**: https://github.com/epicmaxco/vuestic-admin
- **Vuestic UI 文档**: https://vuestic.dev/
- **Chart.js 文档**: https://www.chartjs.org/
- **Vue Router 文档**: https://router.vuejs.org/
- **Pinia 文档**: https://pinia.vuejs.org/

## 🔧 开发工具
```bash
# 启动开发服务器
npm run dev

# 构建生产版本
npm run build

# 代码检查
npm run lint

# 代码格式化
npm run format

# Storybook（组件文档）
npm run storybook
```

## 📝 下一步行动
1. **立即开始**: API 集成 + 用户认证
2. **随后**: 宠物管理页面
3. **然后**: 订单管理 + 服务进度
4. **最后**: 清理示例页面 + 优化性能

---

**更新时间**: 2025-10-03  
**状态**: 🔄 进行中

