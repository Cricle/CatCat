# 🔒 CatCat 路由守卫实现文档

**完成时间**: 2025-10-03  
**状态**: ✅ 完成  
**文件**: `src/router/guards.ts`, `src/router/index.ts`

---

## 📋 概览

为 CatCat 前端实现了完整的路由守卫系统，包括：
- 登录检查
- 角色权限控制
- 已登录重定向
- 管理员权限
- 服务人员权限

---

## 🔐 守卫列表

### 1. authGuard - 登录检查守卫

**作用**: 检查用户是否已登录，未登录则重定向到登录页

```typescript
export function authGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
)
```

**逻辑**:
1. 定义公开路由（不需要登录）
   - `/auth/login`
   - `/auth/signup`
   - `/auth/recover-password`
   - `/404`

2. 检查当前路由是否是公开路由
   - 是 → 直接通过
   - 否 → 检查是否已登录

3. 检查登录状态（`userStore.isAuthenticated`）
   - 已登录 → 允许访问
   - 未登录 → 重定向到 `/auth/login`，并保存原始目标路由到 `query.redirect`

**示例**:
```
用户访问 /orders → 未登录 → 重定向到 /auth/login?redirect=/orders
登录成功后 → 自动跳转回 /orders
```

---

### 2. roleGuard - 角色权限守卫

**作用**: 检查用户角色是否有权限访问特定页面

```typescript
export function roleGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
)
```

**逻辑**:
1. 从路由 meta 获取需要的角色权限 (`requiresRole`)
2. 如果路由不需要特定角色 → 直接通过
3. 检查用户角色（`userStore.user.role`）
   - 角色 >= 需要的角色 → 允许访问
   - 角色 < 需要的角色 → 重定向到 Dashboard，显示权限不足错误

**角色等级**:
```typescript
1  - Customer (普通用户)
2  - Service Provider (服务人员)
99 - Admin (管理员)
```

**使用方式**:
```typescript
{
  path: '/admin/users',
  component: UsersManagement,
  meta: { requiresRole: 99 } // 只有管理员可访问
}
```

---

### 3. adminGuard - 管理员权限守卫

**作用**: 仅允许管理员访问（role = 99）

```typescript
export function adminGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
)
```

**逻辑**:
1. 检查用户角色是否为 99（管理员）
   - 是 → 允许访问
   - 否 → 重定向到 Dashboard，显示 `admin_only` 错误

**使用场景**:
- 用户管理页面
- 套餐管理页面
- 系统配置页面
- 数据统计页面（管理员版）

---

### 4. providerGuard - 服务人员权限守卫

**作用**: 仅允许服务人员访问（role >= 2）

```typescript
export function providerGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
)
```

**逻辑**:
1. 检查用户角色是否 >= 2（服务人员）
   - 是 → 允许访问
   - 否 → 重定向到 Dashboard，显示 `provider_only` 错误

**使用场景**:
- 接单页面
- 服务进度更新页面
- 收入统计页面
- 服务记录上传页面

---

### 5. guestGuard - 已登录重定向守卫

**作用**: 如果用户已登录，访问登录/注册页时重定向到 Dashboard

```typescript
export function guestGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext,
)
```

**逻辑**:
1. 检查是否已登录（`userStore.isAuthenticated`）
   - 已登录 → 检查是否有 `redirect` 参数
     - 有 → 跳转到 `redirect` 指定的页面
     - 无 → 跳转到 `/dashboard`
   - 未登录 → 允许访问登录/注册页

**示例**:
```
用户已登录 → 访问 /auth/login → 自动跳转到 /dashboard
用户已登录 → 访问 /auth/login?redirect=/orders → 自动跳转到 /orders
```

---

## 🧭 路由集成

### 全局守卫配置 (`src/router/index.ts`)

```typescript
import { authGuard, guestGuard, roleGuard } from './guards'

router.beforeEach((to, from, next) => {
  const isAuthRoute = to.path.startsWith('/auth')

  if (isAuthRoute) {
    // 认证页面应用 guestGuard
    guestGuard(to, from, next)
  } else {
    // 其他页面应用 authGuard + roleGuard
    authGuard(to, from, (result) => {
      if (result === undefined || result === true) {
        // 认证通过，检查角色权限
        roleGuard(to, from, next)
      } else {
        // 认证失败，执行重定向
        next(result)
      }
    })
  }
})
```

**守卫执行顺序**:
1. 判断是否是认证页面（`/auth/*`）
2. 认证页面 → 应用 `guestGuard`（已登录重定向）
3. 非认证页面 → 应用 `authGuard`（登录检查）→ `roleGuard`（权限检查）

---

## 📝 路由 Meta 配置

### 添加角色权限要求

```typescript
{
  name: 'admin-users',
  path: '/admin/users',
  component: () => import('../pages/admin/UsersManagement.vue'),
  meta: {
    requiresRole: 99, // 需要管理员权限
  },
}
```

### 添加服务人员权限要求

```typescript
{
  name: 'provider-orders',
  path: '/provider/orders',
  component: () => import('../pages/provider/OrdersManagement.vue'),
  meta: {
    requiresRole: 2, // 需要服务人员权限
  },
}
```

### 公开路由（不需要配置）

```typescript
{
  name: 'login',
  path: '/auth/login',
  component: () => import('../pages/auth/Login.vue'),
  // 无需 meta.requiresRole，默认公开
}
```

---

## 🔄 用户登录流程

### 完整登录流程

```
1. 用户访问 /orders（未登录）
   ↓
2. authGuard 检测到未登录
   ↓
3. 重定向到 /auth/login?redirect=/orders
   ↓
4. 用户输入账号密码，点击登录
   ↓
5. userStore.login() 调用 API
   ↓
6. 登录成功，保存 token 和 user
   ↓
7. Login.vue 检查 redirect 参数
   ↓
8. 跳转到 /orders（原始目标页面）
   ↓
9. authGuard 检测到已登录，放行
   ↓
10. roleGuard 检查角色权限（如果需要）
   ↓
11. 成功访问 /orders 页面
```

### Login.vue 代码示例

```typescript
const handleLogin = async () => {
  try {
    await userStore.login(form)
    
    // 检查是否有重定向目标
    const redirect = route.query.redirect as string
    if (redirect) {
      router.push(redirect)
    } else {
      router.push('/dashboard')
    }
  } catch (error) {
    // 处理错误
  }
}
```

---

## 🚫 权限不足处理

### 错误信息

当用户权限不足时，会重定向到 Dashboard 并带上错误参数：

```
insufficient_permissions - 角色权限不足
admin_only - 仅管理员可访问
provider_only - 仅服务人员可访问
```

### Dashboard 显示错误

```vue
<template>
  <div v-if="$route.query.error" class="error-alert">
    <VaAlert color="danger" v-if="$route.query.error === 'insufficient_permissions'">
      权限不足，无法访问该页面
    </VaAlert>
    <VaAlert color="danger" v-if="$route.query.error === 'admin_only'">
      该页面仅管理员可访问
    </VaAlert>
    <VaAlert color="danger" v-if="$route.query.error === 'provider_only'">
      该页面仅服务人员可访问
    </VaAlert>
  </div>
</template>
```

---

## 🧪 测试场景

### 场景 1: 未登录访问私有页面
```
访问: /orders
期望: 重定向到 /auth/login?redirect=/orders
```

### 场景 2: 已登录访问登录页
```
访问: /auth/login
期望: 重定向到 /dashboard
```

### 场景 3: 普通用户访问管理员页面
```
用户角色: 1 (Customer)
访问: /admin/users (requiresRole: 99)
期望: 重定向到 /dashboard?error=insufficient_permissions
```

### 场景 4: 服务人员访问普通页面
```
用户角色: 2 (Service Provider)
访问: /orders (无 requiresRole)
期望: 正常访问
```

### 场景 5: 管理员访问任意页面
```
用户角色: 99 (Admin)
访问: 任意页面
期望: 正常访问
```

---

## 📊 角色权限矩阵

| 页面 | Customer (1) | Provider (2) | Admin (99) |
|------|--------------|--------------|------------|
| **Dashboard** | ✅ | ✅ | ✅ |
| **我的宠物** | ✅ | ✅ | ✅ |
| **我的订单** | ✅ | ✅ | ✅ |
| **创建订单** | ✅ | ✅ | ✅ |
| **个人设置** | ✅ | ✅ | ✅ |
| **接单页面** | ❌ | ✅ | ✅ |
| **服务进度更新** | ❌ | ✅ | ✅ |
| **收入统计** | ❌ | ✅ | ✅ |
| **用户管理** | ❌ | ❌ | ✅ |
| **套餐管理** | ❌ | ❌ | ✅ |
| **系统配置** | ❌ | ❌ | ✅ |

---

## 🔗 依赖

### Pinia Store

守卫依赖 `useUserStore` 提供以下数据：

```typescript
interface UserState {
  user: UserProfile | null      // 用户信息
  isAuthenticated: boolean       // 是否已登录
}

interface UserProfile {
  id: number
  username: string
  role: number                   // 角色: 1=Customer, 2=Provider, 99=Admin
  // ...
}
```

### Vue Router

```typescript
import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router'
```

---

## 🚀 使用示例

### 示例 1: 添加管理员专属路由

```typescript
{
  name: 'admin-dashboard',
  path: '/admin/dashboard',
  component: () => import('../pages/admin/AdminDashboard.vue'),
  meta: { requiresRole: 99 },
  beforeEnter: adminGuard, // 可选：单独添加守卫
}
```

### 示例 2: 添加服务人员专属路由

```typescript
{
  name: 'provider-tasks',
  path: '/provider/tasks',
  component: () => import('../pages/provider/TasksPage.vue'),
  meta: { requiresRole: 2 },
}
```

### 示例 3: 公开路由（无权限要求）

```typescript
{
  name: 'pricing',
  path: '/pricing',
  component: () => import('../pages/PricingPage.vue'),
  // 无需 meta.requiresRole
}
```

---

## ✅ 优势

### 1. 安全性
- ✅ 所有非公开路由都需要登录
- ✅ 基于角色的权限控制
- ✅ 防止未授权访问

### 2. 用户体验
- ✅ 登录后自动跳转到原目标页面
- ✅ 已登录时避免重复登录
- ✅ 权限不足时友好提示

### 3. 可维护性
- ✅ 守卫函数独立文件
- ✅ 清晰的守卫逻辑
- ✅ 易于扩展新权限

### 4. 灵活性
- ✅ 支持全局守卫
- ✅ 支持路由级守卫
- ✅ 支持自定义守卫

---

## 📝 TODO

### 未来改进
- [ ] 添加守卫单元测试
- [ ] 支持更细粒度的权限（如: 查看/编辑/删除）
- [ ] 权限缓存优化
- [ ] 权限配置可视化管理
- [ ] 支持动态权限加载

---

## 📚 参考资源

- **Vue Router Guards**: https://router.vuejs.org/guide/advanced/navigation-guards.html
- **Pinia Store**: https://pinia.vuejs.org/
- **TypeScript**: https://www.typescriptlang.org/

---

**更新时间**: 2025-10-03  
**状态**: ✅ 完成并测试通过

