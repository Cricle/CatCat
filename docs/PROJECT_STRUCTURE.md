# 项目结构说明

## 目录结构

```
CatCat/
├── .github/
│   └── workflows/
│       └── ci-cd.yml              # GitHub Actions CI/CD配置
├── database/
│   └── init.sql                   # 数据库初始化脚本
├── deploy/
│   └── kubernetes/
│       ├── deployment.yml         # K8s部署配置
│       ├── postgres.yml          # PostgreSQL配置
│       ├── redis.yml             # Redis配置
│       ├── nats.yml              # NATS配置
│       └── ingress.yml           # Ingress配置
├── docs/
│   ├── ARCHITECTURE.md           # 架构设计文档
│   ├── API.md                    # API文档
│   ├── QUICK_START.md            # 快速启动指南
│   ├── DEPLOYMENT.md             # 部署指南
│   └── PROJECT_STRUCTURE.md      # 本文档
├── src/
│   ├── CatCat.API/               # API服务层（Minimal API）
│   │   ├── Endpoints/            # API端点定义
│   │   │   ├── AuthEndpoints.cs
│   │   │   ├── UserEndpoints.cs
│   │   │   ├── PetEndpoints.cs
│   │   │   ├── OrderEndpoints.cs
│   │   │   └── ServiceProviderEndpoints.cs
│   │   ├── Middleware/           # 中间件
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Models/               # 请求/响应模型
│   │   │   └── AuthModels.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json      # 配置文件
│   │   ├── CatCat.API.csproj     # 项目文件
│   │   ├── Dockerfile            # Docker构建文件
│   │   └── Program.cs            # 程序入口
│   │
│   ├── CatCat.Core/              # 核心业务逻辑层（待扩展）
│   │   ├── Services/             # 业务服务
│   │   └── Validators/           # 数据验证
│   │
│   ├── CatCat.Domain/            # 领域模型层
│   │   ├── Entities/             # 实体类
│   │   │   ├── User.cs           # 用户实体
│   │   │   ├── ServiceProvider.cs
│   │   │   ├── Pet.cs            # 宠物实体
│   │   │   ├── ServiceOrder.cs   # 订单实体
│   │   │   ├── ServicePackage.cs # 服务套餐
│   │   │   ├── ServiceRecord.cs  # 服务记录
│   │   │   └── Review.cs         # 评价
│   │   └── CatCat.Domain.csproj
│   │
│   ├── CatCat.Infrastructure/    # 基础设施层
│   │   ├── Cache/                # 缓存服务
│   │   │   └── RedisCacheService.cs
│   │   ├── Database/             # 数据库连接
│   │   │   └── DbConnectionFactory.cs
│   │   ├── MessageQueue/         # 消息队列
│   │   │   └── NatsService.cs
│   │   ├── Repositories/         # 数据仓库
│   │   │   └── UserRepository.cs
│   │   └── CatCat.Infrastructure.csproj
│   │
│   └── CatCat.Web/               # 前端应用
│       ├── public/               # 静态资源
│       ├── src/
│       │   ├── api/              # API调用
│       │   │   ├── request.ts    # axios封装
│       │   │   └── auth.ts       # 认证API
│       │   ├── assets/           # 资源文件
│       │   ├── components/       # 公共组件
│       │   ├── layouts/          # 布局组件
│       │   │   └── MainLayout.vue
│       │   ├── router/           # 路由配置
│       │   │   └── index.ts
│       │   ├── stores/           # Pinia状态管理
│       │   │   └── user.ts
│       │   ├── views/            # 页面组件
│       │   │   ├── auth/
│       │   │   │   ├── Login.vue
│       │   │   │   └── Register.vue
│       │   │   ├── Home.vue
│       │   │   ├── Orders.vue
│       │   │   └── Profile.vue
│       │   ├── App.vue           # 根组件
│       │   ├── main.ts           # 应用入口
│       │   └── style.css         # 全局样式
│       ├── .gitignore
│       ├── Dockerfile
│       ├── index.html
│       ├── nginx.conf            # Nginx配置
│       ├── package.json
│       ├── tsconfig.json
│       └── vite.config.ts
│
├── .gitignore                     # Git忽略文件
├── CatCat.sln                     # Visual Studio解决方案
├── CONTRIBUTING.md                # 贡献指南
├── docker-compose.yml             # Docker编排配置
├── LICENSE                        # MIT许可证
└── README.md                      # 项目说明
```

## 层次架构

### 1. API层 (CatCat.API) - Minimal API架构

**职责**：
- 接收HTTP请求
- 参数验证
- 调用业务逻辑
- 返回响应
- 中间件处理（认证、异常等）

**关键文件**：
- `Program.cs`: 应用配置和中间件管道
- `Endpoints/`: Minimal API端点定义（按功能模块组织）
- `Middleware/`: 自定义中间件
- `Models/`: DTO模型

**Minimal API优势**：
- 更轻量、性能更好
- 代码更简洁
- 更好的AOT支持
- 适合微服务架构

### 2. 核心业务层 (CatCat.Core)

**职责**：
- 业务逻辑实现
- 业务规则验证
- 事务管理
- 领域服务

**待实现的服务**：
- `OrderService`: 订单处理逻辑
- `PaymentService`: 支付处理
- `NotificationService`: 通知服务
- `LocationService`: 位置服务

### 3. 领域模型层 (CatCat.Domain)

**职责**：
- 定义实体类
- 定义枚举类型
- 值对象
- 领域事件

**实体关系**：
```
User (用户)
  ├── 1:N → Pet (宠物)
  ├── 1:N → ServiceOrder (订单-作为客户)
  ├── 1:1 → ServiceProvider (服务人员详情)
  └── 1:N → Review (评价-作为客户)

ServiceProvider (服务人员)
  ├── 1:N → ServiceOrder (订单-作为服务人员)
  └── 1:N → Review (评价-被评价)

ServiceOrder (订单)
  ├── N:1 → User (客户)
  ├── N:1 → ServiceProvider (服务人员)
  ├── N:1 → Pet (宠物)
  ├── N:1 → ServicePackage (服务套餐)
  ├── 1:N → ServiceRecord (服务记录)
  └── 1:1 → Review (评价)
```

### 4. 基础设施层 (CatCat.Infrastructure)

**职责**：
- 数据访问实现
- 缓存实现
- 消息队列实现
- 外部服务集成

**组件**：
- **DbConnectionFactory**: 数据库连接工厂
- **Repositories**: 数据仓库实现
- **RedisCacheService**: Redis缓存服务
- **NatsService**: NATS消息队列服务

### 5. 前端层 (CatCat.Web)

**技术栈**：
- Vue 3 (组合式API)
- Vue Router 4 (路由)
- Pinia (状态管理)
- Vant 4 (移动端UI)
- Axios (HTTP客户端)
- TypeScript (类型安全)

**页面结构**：
```
认证流程：
  Login (登录) ← → Register (注册)
     ↓
主要页面：
  MainLayout (主布局)
    ├── Home (首页 - 服务套餐展示)
    ├── Orders (订单列表)
    └── Profile (个人中心)
```

## 数据流向

### 1. 用户登录流程

```
用户输入 → Login.vue
    ↓
stores/user.ts (loginUser)
    ↓
api/auth.ts (login)
    ↓
HTTP POST /api/auth/login
    ↓
AuthController.Login
    ↓
UserRepository.GetByPhoneAsync
    ↓
PostgreSQL
    ↓
返回Token + UserInfo
    ↓
存储到localStorage
    ↓
跳转到首页
```

### 2. 创建订单流程

```
用户选择服务 → Home.vue
    ↓
填写订单信息
    ↓
HTTP POST /api/orders
    ↓
OrderController.Create
    ↓
OrderService.CreateOrderAsync
    ↓
├── OrderRepository.CreateAsync (保存订单)
├── NatsService.PublishAsync (发布订单事件)
└── CacheService.RemoveAsync (清理缓存)
    ↓
订单创建成功
    ↓
推送通知给服务人员
```

### 3. 服务人员接单流程

```
推送通知 → 服务人员App
    ↓
查看订单详情
    ↓
HTTP POST /api/orders/{id}/accept
    ↓
OrderController.Accept
    ↓
OrderService.AcceptOrderAsync
    ↓
├── 验证服务人员状态
├── 更新订单状态
├── 通知客户（NATS）
└── 更新缓存
    ↓
接单成功
```

## 配置管理

### 后端配置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PostgreSQL连接字符串",
    "Redis": "Redis连接字符串",
    "Nats": "NATS连接字符串"
  },
  "JwtSettings": {
    "SecretKey": "JWT密钥",
    "Issuer": "签发者",
    "Audience": "受众",
    "ExpiryDays": 7
  },
  "AllowedOrigins": ["http://localhost:5173"],
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowMinutes": 1
  }
}
```

### 前端配置

```typescript
// vite.config.ts
export default defineConfig({
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
})
```

## 扩展点

### 1. 添加新实体

1. 在 `CatCat.Domain/Entities/` 创建实体类
2. 在 `database/init.sql` 添加表结构
3. 在 `CatCat.Infrastructure/Repositories/` 创建Repository
4. 在 `CatCat.API/Controllers/` 创建Controller
5. 在前端创建对应的API调用和页面

### 2. 添加新功能模块

1. **后端**:
   - Domain: 定义实体和接口
   - Infrastructure: 实现数据访问
   - Core: 实现业务逻辑
   - API: 暴露HTTP接口

2. **前端**:
   - api/: 定义API调用
   - stores/: 状态管理
   - views/: 页面组件
   - router/: 路由配置

### 3. 集成第三方服务

在 `CatCat.Infrastructure` 中添加：
- 支付服务 (Payment/)
- 短信服务 (Sms/)
- 对象存储 (Storage/)
- 地图服务 (Map/)

## 开发规范

### 命名约定

- **C# 类名**: PascalCase (e.g., `UserRepository`)
- **C# 方法**: PascalCase (e.g., `GetByIdAsync`)
- **C# 参数**: camelCase (e.g., `userId`)
- **TypeScript 文件**: kebab-case (e.g., `user-service.ts`)
- **Vue 组件**: PascalCase (e.g., `UserProfile.vue`)
- **数据库表**: snake_case (e.g., `service_orders`)

### 代码组织

- 单一职责原则
- 依赖注入
- 接口分离
- 异步优先

## 相关文档

- [架构设计](ARCHITECTURE.md)
- [API文档](API.md)
- [快速启动](QUICK_START.md)
- [部署指南](DEPLOYMENT.md)

