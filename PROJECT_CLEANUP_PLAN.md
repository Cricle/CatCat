# 🧹 CatCat 项目清理计划

## 📋 待删除的示例页面

### Vuestic Admin 模板示例（不需要的）
- ❌ **projects/** - 示例项目管理页面
- ❌ **payments/** - 示例支付页面
- ❌ **billing/** - 示例账单页面
- ❌ **pricing-plans/** - 示例定价页面
- ❌ **faq/** - 示例FAQ页面
- ❌ **users/** - 示例用户管理（可能保留部分用于管理员）

### 要保留的
- ✅ **auth/** - 认证页面（Login, Signup）
- ✅ **admin/dashboard/** - 仪表板
- ✅ **pets/** - 宠物管理
- ✅ **orders/** - 订单管理
- ✅ **provider/** - 服务人员端
- ✅ **preferences/** - 个人偏好设置
- ✅ **settings/** - 系统设置
- ✅ **404.vue** - 404页面

## 🗑️ 清理操作

### 1. 删除示例页面目录
```
src/CatCat.Web/src/pages/projects/
src/CatCat.Web/src/pages/payments/
src/CatCat.Web/src/pages/billing/
src/CatCat.Web/src/pages/pricing-plans/
src/CatCat.Web/src/pages/faq/
```

### 2. 清理路由配置
删除以下路由：
- projects
- payments 及子路由
- billing
- pricing-plans
- faq

### 3. 清理导航菜单
删除以下导航项：
- projects
- payments 及子菜单
- auth（认证页面不应该在主导航）
- faq
- 404（不应该在导航）
- users（示例，留待管理员功能）

### 4. 清理旧文档
可选删除的文档：
- tong.md
- UI_REDESIGN_COMPLETE.md（已合并到其他文档）
- WEB_REWRITE_PLAN.md（已完成）
- VUESTIC_MIGRATION.md（已完成）

## ✅ 清理后的项目结构

### 页面结构
```
src/pages/
├── 404.vue                    ✅ 保留
├── auth/                      ✅ 保留
│   ├── Login.vue
│   ├── Signup.vue
│   ├── RecoverPassword.vue
│   └── CheckTheEmail.vue
├── admin/                     ✅ 保留
│   └── dashboard/
│       └── Dashboard.vue
├── pets/                      ✅ 保留
│   ├── PetsPage.vue
│   └── widgets/
├── orders/                    ✅ 保留
│   ├── OrdersPage.vue
│   ├── CreateOrderPage.vue
│   └── OrderDetailPage.vue
├── provider/                  ✅ 保留
│   ├── AvailableOrdersPage.vue
│   ├── MyTasksPage.vue
│   ├── ProgressUpdatePage.vue
│   └── EarningsPage.vue
├── preferences/               ✅ 保留
│   └── Preferences.vue
└── settings/                  ✅ 保留
    └── Settings.vue
```

### 导航菜单
```
- Dashboard (仪表板)
- My Pets (我的宠物)
- My Orders (我的订单)
- Service Provider (服务人员) - role >= 2
  - Available Orders
  - My Tasks
  - My Earnings
- Preferences (偏好设置)
- Settings (系统设置)
```

## 📊 预期效果

- 减少约 **20+ 个无用文件**
- 清理约 **500+ 行无用代码**
- 简化导航菜单，提升用户体验
- 减少打包体积约 **50-100KB**

