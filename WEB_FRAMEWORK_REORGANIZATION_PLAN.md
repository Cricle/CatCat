# 🔄 CatCat Web 框架重组计划

**日期**: 2025-10-03  
**目标**: 重新整理web框架，删除不需要的模块，补充遗漏的功能  
**基于**: 上门喂猫服务的核心痛点和业务需求

---

## 🎯 核心业务痛点回顾

### 1. 服务人员上门痛点（已解决）
- ✅ 不知道猫粮位置 → `food_location`
- ✅ 不知道水盆位置 → `water_location`
- ✅ 不知道猫砂盆位置 → `litter_box_location`
- ✅ 不知道清洁用品位置 → `cleaning_supplies_location`
- ✅ 特殊说明 → `special_instructions`

### 2. 服务追踪痛点（已解决）
- ✅ 9个服务进度状态
- ✅ 服务进度时间线
- ⏳ 照片上传（UI完成，后端待实现）
- ⏳ 实时位置追踪（待实现）

### 3. 用户信任痛点（部分解决）
- ✅ 服务进度透明化
- ✅ 服务评价系统
- ⏳ 服务人员资质展示
- ❌ 服务保障说明

---

## 📊 当前页面结构分析

### ✅ 已有且需要的核心页面

#### 用户端（5个）
1. ✅ **Dashboard** - 仪表板
2. ✅ **PetsPage** - 宠物管理
3. ✅ **OrdersPage** - 订单列表
4. ✅ **CreateOrderPage** - 创建订单
5. ✅ **OrderDetailPage** - 订单详情

#### 服务人员端（4个）
1. ✅ **AvailableOrdersPage** - 可接订单
2. ✅ **MyTasksPage** - 我的任务
3. ✅ **ProgressUpdatePage** - 服务进度更新
4. ✅ **EarningsPage** - 收入统计

#### 认证（4个）
1. ✅ **Login** - 登录
2. ✅ **Signup** - 注册
3. ✅ **RecoverPassword** - 密码恢复
4. ✅ **CheckTheEmail** - 邮箱验证

#### 设置（2个）
1. ✅ **Preferences** - 个人偏好
2. ✅ **Settings** - 系统设置

---

## ❌ 不需要的模块（需要删除）

### 1. Dashboard 示例卡片
```
src/pages/admin/dashboard/cards/
├── ProjectTable.vue          ❌ 删除（示例项目表格）
├── RegionRevenue.vue          ❌ 删除（示例地区收入）
├── RevenueByLocationMap.vue   ❌ 删除（示例地图）
├── RevenueReport.vue          ❌ 删除（示例收入报告）
├── RevenueReportChart.vue     ❌ 删除（示例图表）
├── Timeline.vue               ✅ 保留（可用于订单时间线）
├── YearlyBreakup.vue          ❌ 删除（示例年度分析）
├── MonthlyEarnings.vue        ⏳ 改造（收入卡片可用）
```

### 2. 重复的页面
```
src/pages/admin/pages/
├── 404PagesPage.vue          ❌ 删除（已有 404.vue）
```

### 3. 重复的设置页面
```
src/pages/preferences/settings/
├── Settings.vue              ❌ 删除（已有 src/pages/settings/Settings.vue）
```

---

## 🆕 遗漏的重要模块（需要添加）

### 1. 套餐相关页面（高优先级）
```
src/pages/packages/
├── PackagesPage.vue          🆕 套餐列表页面
│   - 展示所有服务套餐
│   - 价格、服务内容、时长
│   - 筛选和排序
│   - 购买按钮跳转到创建订单
└── PackageDetailPage.vue     🆕 套餐详情页面
    - 套餐完整信息
    - 服务项目说明
    - 用户评价
    - 立即下单按钮
```

**路由**:
- `/packages` - 套餐列表
- `/packages/:id` - 套餐详情

**导航**: 主菜单添加"服务套餐"

---

### 2. 服务人员相关页面（高优先级）
```
src/pages/providers/
├── ProvidersPage.vue          🆕 服务人员列表
│   - 服务人员卡片展示
│   - 评分、服务次数
│   - 认证标识
│   - 擅长服务类型
└── ProviderDetailPage.vue     🆕 服务人员详情
    - 个人简介
    - 服务历史
    - 用户评价
    - 认证资质
```

**路由**:
- `/providers` - 服务人员列表
- `/providers/:id` - 服务人员详情

**导航**: 主菜单添加"服务人员"

---

### 3. 通知中心（中优先级）
```
src/pages/notifications/
└── NotificationsPage.vue      🆕 通知中心
    - 订单状态通知
    - 服务进度通知
    - 系统消息
    - 已读/未读标识
    - 分类筛选
```

**路由**:
- `/notifications` - 通知中心

**导航**: 顶部导航栏铃铛图标

---

### 4. 管理员后台完整功能（高优先级）
```
src/pages/admin/
├── users/
│   └── UsersManagement.vue    🆕 用户管理
│       - 用户列表（CRUD）
│       - 搜索和筛选
│       - 角色管理
│       - 禁用/启用用户
├── packages/
│   ├── PackagesManagement.vue 🆕 套餐管理
│   │   - 套餐列表（CRUD）
│   │   - 创建/编辑套餐
│   │   - 启用/停用套餐
│   └── PackageForm.vue        🆕 套餐表单
├── orders/
│   └── OrdersMonitoring.vue   🆕 订单监控
│       - 所有订单列表
│       - 订单状态统计
│       - 异常订单处理
│       - 退款管理
└── statistics/
    └── StatisticsPage.vue     🆕 数据统计
        - 收入趋势图
        - 用户增长图
        - 订单完成率
        - 服务人员绩效
```

**路由**:
- `/admin/users` - 用户管理
- `/admin/packages` - 套餐管理
- `/admin/orders` - 订单监控
- `/admin/statistics` - 数据统计

**导航**: 管理员菜单（role = 99）

---

### 5. 帮助与支持（中优先级）
```
src/pages/help/
├── HelpCenter.vue            🆕 帮助中心
│   - 常见问题（FAQ）
│   - 使用指南
│   - 服务说明
│   - 搜索功能
└── ContactUs.vue             🆕 联系我们
    - 客服联系方式
    - 反馈表单
    - 服务时间
```

**路由**:
- `/help` - 帮助中心
- `/contact` - 联系我们

**导航**: 底部导航或个人菜单

---

### 6. 服务保障说明（中优先级）
```
src/pages/guarantee/
└── ServiceGuarantee.vue      🆕 服务保障
    - 服务承诺
    - 保险说明
    - 退款政策
    - 安全保障
```

**路由**:
- `/guarantee` - 服务保障

**导航**: 底部导航

---

## 🔧 需要优化的现有模块

### 1. Dashboard 优化
**当前**: 使用示例卡片  
**需要**: 替换为实际业务卡片

```vue
// 需要的卡片组件
src/pages/admin/dashboard/cards/
├── OrderStatsCard.vue       🆕 订单统计卡片
├── PetStatsCard.vue         🆕 宠物统计卡片
├── RevenueCard.vue          🔄 收入卡片（改造 MonthlyEarnings）
├── UserGrowthCard.vue       🆕 用户增长卡片
├── RecentOrdersList.vue     🆕 最近订单列表
└── MyPetsList.vue           🆕 我的宠物列表
```

### 2. ProgressUpdatePage 优化
**当前**: 照片上传UI完成  
**需要**: 后端照片存储API

**待实现**:
- 📸 照片上传后端API
- 🖼️ 图片压缩和存储
- 📦 CDN集成（可选）

### 3. OrderDetailPage 优化
**当前**: 基础信息展示  
**需要**: 更丰富的内容

**增强**:
- 🗺️ 服务地址地图展示
- 📸 服务照片画廊
- 💬 在线沟通功能（可选）
- 📍 实时位置追踪（可选）

---

## 📋 完整的页面结构（重组后）

```
src/pages/
├── 404.vue                              ✅ 保留
│
├── auth/                                ✅ 保留（4个页面）
│   ├── Login.vue
│   ├── Signup.vue
│   ├── RecoverPassword.vue
│   └── CheckTheEmail.vue
│
├── admin/                               🔄 重组优化
│   ├── dashboard/
│   │   ├── Dashboard.vue               ✅ 保留
│   │   ├── DataSection.vue             ✅ 保留
│   │   ├── DataSectionItem.vue         ✅ 保留
│   │   └── cards/
│   │       ├── OrderStatsCard.vue      🆕 新建
│   │       ├── PetStatsCard.vue        🆕 新建
│   │       ├── RevenueCard.vue         🔄 改造
│   │       ├── UserGrowthCard.vue      🆕 新建
│   │       ├── RecentOrdersList.vue    🆕 新建
│   │       └── MyPetsList.vue          🆕 新建
│   ├── users/
│   │   └── UsersManagement.vue         🆕 新建
│   ├── packages/
│   │   ├── PackagesManagement.vue      🆕 新建
│   │   └── PackageForm.vue             🆕 新建
│   ├── orders/
│   │   └── OrdersMonitoring.vue        🆕 新建
│   └── statistics/
│       └── StatisticsPage.vue          🆕 新建
│
├── pets/                                ✅ 保留（宠物管理）
│   ├── PetsPage.vue
│   └── widgets/
│       ├── PetForm.vue
│       └── PetsTable.vue
│
├── orders/                              ✅ 保留（订单管理）
│   ├── OrdersPage.vue
│   ├── CreateOrderPage.vue
│   └── OrderDetailPage.vue
│
├── packages/                            🆕 新建（套餐模块）
│   ├── PackagesPage.vue                🆕 套餐列表
│   └── PackageDetailPage.vue           🆕 套餐详情
│
├── providers/                           🆕 新建（服务人员模块）
│   ├── ProvidersPage.vue               🆕 服务人员列表
│   └── ProviderDetailPage.vue          🆕 服务人员详情
│
├── provider/                            ✅ 保留（服务人员端）
│   ├── AvailableOrdersPage.vue
│   ├── MyTasksPage.vue
│   ├── ProgressUpdatePage.vue
│   └── EarningsPage.vue
│
├── notifications/                       🆕 新建（通知中心）
│   └── NotificationsPage.vue           🆕 通知中心
│
├── help/                                🆕 新建（帮助支持）
│   ├── HelpCenter.vue                  🆕 帮助中心
│   └── ContactUs.vue                   🆕 联系我们
│
├── guarantee/                           🆕 新建（服务保障）
│   └── ServiceGuarantee.vue            🆕 服务保障
│
├── preferences/                         ✅ 保留（个人偏好）
│   ├── Preferences.vue
│   ├── preferences-header/
│   │   └── PreferencesHeader.vue
│   └── modals/
│       ├── EditNameModal.vue
│       └── ResetPasswordModal.vue
│
└── settings/                            ✅ 保留（系统设置）
    ├── Settings.vue
    ├── language-switcher/
    │   └── LanguageSwitcher.vue
    ├── theme-switcher/
    │   └── ThemeSwitcher.vue
    └── notifications/
        └── Notifications.vue
```

---

## 🗺️ 导航菜单结构（重组后）

### 普通用户菜单
```
- 🏠 仪表板 (Dashboard)
- 🐱 我的宠物 (My Pets)
- 📦 服务套餐 (Packages) 🆕
- 📋 我的订单 (My Orders)
- 👷 服务人员 (Providers) 🆕
- 🔔 通知中心 (Notifications) 🆕
- ❓ 帮助中心 (Help) 🆕
- ⚙️ 设置 (Settings)
- 👤 个人中心 (Preferences)
```

### 服务人员菜单（role >= 2）
```
- 🏠 仪表板 (Dashboard)
- 📋 可接订单 (Available Orders)
- 📝 我的任务 (My Tasks)
- 💰 我的收入 (My Earnings)
- 🔔 通知中心 (Notifications) 🆕
- ⚙️ 设置 (Settings)
- 👤 个人中心 (Preferences)
```

### 管理员菜单（role = 99）
```
- 🏠 仪表板 (Dashboard)
- 👥 用户管理 (Users) 🆕
- 📦 套餐管理 (Packages) 🆕
- 📋 订单监控 (Orders) 🆕
- 📊 数据统计 (Statistics) 🆕
- 🐱 宠物管理 (Pets)
- 👷 服务人员管理 (Providers) 🆕
- ⚙️ 设置 (Settings)
```

---

## 📊 优先级排序

### P0 - 立即实现（核心业务）
1. ✅ **套餐列表页面** - 用户查看服务
2. ✅ **套餐详情页面** - 了解服务内容
3. ✅ **管理员用户管理** - 后台必需
4. ✅ **管理员套餐管理** - 内容管理

### P1 - 高优先级（1周内）
5. ⏳ **服务人员列表页面** - 选择服务人员
6. ⏳ **照片上传后端** - 服务凭证
7. ⏳ **订单监控页面** - 管理员监控
8. ⏳ **删除示例卡片** - 清理代码

### P2 - 中优先级（2周内）
9. ⏳ **通知中心** - 消息提醒
10. ⏳ **服务人员详情** - 了解服务人员
11. ⏳ **帮助中心** - 用户支持
12. ⏳ **数据统计页面** - 管理员分析

### P3 - 低优先级（后续）
13. ⏳ **服务保障页面** - 信任建设
14. ⏳ **联系我们** - 客服支持
15. ⏳ **实时位置追踪** - 增强功能
16. ⏳ **在线沟通** - 增强功能

---

## 🎯 实施步骤

### Phase 1: 清理不需要的模块（1天）
1. 删除 `admin/dashboard/cards/` 中的示例卡片
2. 删除 `admin/pages/404PagesPage.vue`
3. 删除 `preferences/settings/Settings.vue`
4. 更新路由和导航

### Phase 2: 核心业务页面（3天）
1. 创建套餐列表和详情页面
2. 创建管理员用户管理页面
3. 创建管理员套餐管理页面
4. 更新路由和导航

### Phase 3: Dashboard 优化（2天）
1. 创建业务统计卡片
2. 替换示例卡片
3. 优化数据展示

### Phase 4: 服务人员和通知（3天）
1. 创建服务人员列表和详情
2. 创建通知中心
3. 实现照片上传后端

### Phase 5: 管理后台完善（3天）
1. 创建订单监控页面
2. 创建数据统计页面
3. 完善权限控制

### Phase 6: 帮助和支持（2天）
1. 创建帮助中心
2. 创建服务保障页面
3. 创建联系我们页面

---

## 📝 总结

### 需要删除
- ❌ 7个示例卡片组件
- ❌ 2个重复页面
- **总计**: 9个文件

### 需要新建
- 🆕 2个套餐页面
- 🆕 2个服务人员页面
- 🆕 1个通知中心
- 🆕 4个管理员页面
- 🆕 6个Dashboard卡片
- 🆕 3个帮助支持页面
- **总计**: 18个文件

### 需要优化
- 🔄 Dashboard（替换示例卡片）
- 🔄 ProgressUpdatePage（照片上传后端）
- 🔄 OrderDetailPage（增强功能）
- **总计**: 3个文件

---

**预计工时**: 14天（2周）  
**当前进度**: 90%  
**完成后进度**: 95%+  
**状态**: 📋 计划就绪，等待执行

