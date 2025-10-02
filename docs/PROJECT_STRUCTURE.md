# 项目结构说明

## 目录结构

```
CatCat/
├── .github/
│   └── workflows/
│       └── ci-cd.yml              # GitHub Actions CI/CD配置
├── database/
│   └── init.sql                   # PostgreSQL数据库初始化脚本
├── deploy/
│   └── kubernetes/
│       ├── deployment.yml         # K8s部署配置
│       ├── postgres.yml          # PostgreSQL配置
│       ├── redis.yml             # Redis配置
│       └── nats.yml              # NATS配置
├── docs/
│   ├── ARCHITECTURE.md           # 架构设计文档
│   ├── API.md                    # API文档
│   ├── AOT_AND_CLUSTER.md        # AOT和集群指南
│   ├── CENTRAL_PACKAGE_MANAGEMENT.md  # 中央包管理
│   ├── DEPLOYMENT.md             # 部署指南
│   ├── ENVIRONMENT.md            # 环境配置
│   ├── NATS_PEAK_CLIPPING.md     # NATS削峰指南
│   ├── OPENTELEMETRY_GUIDE.md    # OpenTelemetry指南
│   ├── OPTIMIZATION_SUMMARY.md   # 优化总结
│   ├── PROJECT_STRUCTURE.md      # 本文档
│   ├── QUICK_START.md            # 快速启动指南
│   ├── RATE_LIMITING_GUIDE.md    # 限流指南
│   └── optimization-history/     # 优化历史记录
│       ├── AOT_FUSIONCACHE_REVIEW.md
│       ├── CONTINUOUS_OPTIMIZATION_2.md
│       ├── CONTINUOUS_OPTIMIZATION_3.md
│       ├── CONTINUOUS_OPTIMIZATION_4.md
│       └── YARP_MIGRATION.md
├── scripts/
│   ├── build.ps1                 # Windows编译脚本（支持AOT）
│   └── build.sh                  # Linux编译脚本（支持AOT）
├── src/
│   ├── CatCat.API/               # Web API服务（Minimal API）
│   │   ├── Configuration/        # 配置模块
│   │   │   ├── OpenTelemetryConfiguration.cs
│   │   │   └── RateLimitingConfiguration.cs
│   │   ├── Endpoints/            # API端点定义（按模块组织）
│   │   │   ├── AuthEndpoints.cs  # 认证端点
│   │   │   ├── UserEndpoints.cs  # 用户端点
│   │   │   ├── PetEndpoints.cs   # 宠物端点
│   │   │   ├── OrderEndpoints.cs # 订单端点
│   │   │   └── ReviewEndpoints.cs# 评价端点
│   │   ├── Extensions/           # 扩展方法
│   │   │   ├── ClaimsPrincipalExtensions.cs # JWT声明扩展
│   │   │   └── ServiceCollectionExtensions.cs # 服务注册扩展
│   │   ├── Json/                 # JSON配置（AOT源生成）
│   │   │   └── AppJsonContext.cs # System.Text.Json源生成上下文
│   │   ├── Middleware/           # 中间件
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Models/               # 数据模型
│   │   │   ├── ApiResult.cs      # API响应统一格式
│   │   │   ├── Requests.cs       # 请求DTO
│   │   │   └── Responses.cs      # 响应DTO
│   │   ├── Observability/        # 可观测性
│   │   │   └── CustomMetrics.cs  # 自定义指标
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json      # 配置文件
│   │   ├── CatCat.API.csproj     # 项目文件
│   │   ├── Dockerfile            # Docker构建文件
│   │   ├── Dockerfile.aot        # AOT Docker构建文件
│   │   ├── GlobalSuppressions.cs # 警告抑制
│   │   └── Program.cs            # 程序入口
│   │
│   ├── CatCat.Gateway/           # YARP API网关
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json      # 网关配置
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Production.json
│   │   ├── CatCat.Gateway.csproj
│   │   ├── Dockerfile.gateway    # Docker构建文件
│   │   ├── Dockerfile.gateway.aot# AOT Docker构建文件
│   │   └── Program.cs            # 程序入口
│   │
│   ├── CatCat.Infrastructure/    # 基础设施层（统一）
│   │   ├── Common/               # 通用类型
│   │   │   └── Result.cs         # Result模式
│   │   ├── Database/             # 数据库
│   │   │   ├── DatabaseConcurrencyLimiter.cs # 并发限制器
│   │   │   ├── DatabaseMetrics.cs# 数据库指标
│   │   │   └── DbConnectionFactory.cs # 连接工厂
│   │   ├── Entities/             # 实体类
│   │   │   ├── OrderStatusHistory.cs # 订单状态历史
│   │   │   ├── Payment.cs        # 支付
│   │   │   ├── Pet.cs            # 宠物
│   │   │   ├── Review.cs         # 评价
│   │   │   ├── ServiceOrder.cs   # 订单
│   │   │   ├── ServicePackage.cs # 服务套餐
│   │   │   └── User.cs           # 用户
│   │   ├── IdGenerator/          # ID生成器
│   │   │   └── SnowflakeIdGenerator.cs # Yitter雪花ID
│   │   ├── Messages/             # 消息定义
│   │   │   └── OrderCreatedMessage.cs # 订单创建消息
│   │   ├── MessageQueue/         # 消息队列
│   │   │   └── NatsService.cs    # NATS服务
│   │   ├── Payment/              # 支付服务
│   │   │   └── StripePaymentService.cs # Stripe支付
│   │   ├── Repositories/         # 数据仓储（Sqlx）
│   │   │   ├── OrderStatusHistoryRepository.cs
│   │   │   ├── PaymentRepository.cs
│   │   │   ├── PetRepository.cs
│   │   │   ├── ReviewRepository.cs
│   │   │   ├── ServiceOrderRepository.cs
│   │   │   ├── ServicePackageRepository.cs
│   │   │   └── UserRepository.cs
│   │   ├── Services/             # 业务服务
│   │   │   ├── OrderService.cs   # 订单服务
│   │   │   └── ReviewService.cs  # 评价服务
│   │   └── CatCat.Infrastructure.csproj
│   │
│   └── CatCat.Web/               # Vue 3前端应用
│       ├── public/               # 静态资源
│       ├── src/
│       │   ├── api/              # API调用
│       │   │   ├── auth.ts       # 认证API
│       │   │   ├── orders.ts     # 订单API
│       │   │   ├── packages.ts   # 套餐API
│       │   │   ├── pets.ts       # 宠物API
│       │   │   └── request.ts    # Axios封装
│       │   ├── assets/           # 静态资源
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
│       │   │   ├── MyOrders.vue
│       │   │   ├── MyPets.vue
│       │   │   ├── OrderDetail.vue
│       │   │   ├── Payment.vue
│       │   │   ├── Profile.vue
│       │   │   └── Reviews.vue
│       │   ├── App.vue           # 根组件
│       │   ├── main.ts           # 应用入口
│       │   └── style.css         # 全局样式
│       ├── Dockerfile.web        # Docker构建文件
│       ├── index.html
│       ├── nginx.conf            # Nginx配置
│       ├── package.json
│       ├── tsconfig.json
│       └── vite.config.ts
│
├── Directory.Build.props          # 项目通用属性
├── Directory.Packages.props       # 中央包管理
├── .dockerignore
├── .gitignore
├── CatCat.sln                     # Visual Studio解决方案
├── docker-compose.yml             # Docker Compose配置
├── LICENSE                        # MIT许可证
└── README.md                      # 项目说明
```

## 层次架构（简化后）

项目采用**3层架构**，从5个项目简化到3个项目：

### 1. API层 (CatCat.API)

**职责**：
- 接收HTTP请求
- 参数验证和绑定
- 调用业务服务
- 返回统一格式响应
- 认证授权
- 限流保护
- 异常处理

**技术栈**：
- ASP.NET Core 9 Minimal API
- JWT Authentication
- Rate Limiting (Fixed Window, Sliding Window, Token Bucket, Concurrency)
- OpenTelemetry (Tracing + Metrics)
- FusionCache (L1 Memory + L2 Redis)
- System.Text.Json Source Generators (AOT兼容)

**关键特性**：
- ✅ 100% AOT兼容
- ✅ Minimal API（轻量、高性能）
- ✅ 统一API响应格式 (`ApiResult<T>`)
- ✅ 显式类型（无匿名类型）
- ✅ 条件编译Swagger（仅Debug）

### 2. 网关层 (CatCat.Gateway)

**职责**：
- 反向代理
- 负载均衡
- 限流
- 路由转发
- OpenTelemetry集成

**技术栈**：
- YARP (Yet Another Reverse Proxy)
- OpenTelemetry
- 支持AOT编译

**优势**：
- 统一.NET技术栈
- 原生OpenTelemetry支持
- 配置热更新
- 集群友好

### 3. 基础设施层 (CatCat.Infrastructure)

**职责**（合并了Domain + Core + Infrastructure）：
- **实体定义** (Entities/): 领域模型
- **数据访问** (Repositories/): Sqlx源生成仓储
- **业务服务** (Services/): OrderService, ReviewService
- **消息队列** (MessageQueue/): NATS
- **支付服务** (Payment/): Stripe
- **缓存** (通过FusionCache直接使用)
- **ID生成** (IdGenerator/): Yitter Snowflake
- **数据库保护** (Database/): 并发限制 + 性能监控

**技术栈**：
- Npgsql (PostgreSQL)
- **Sqlx** (编译时源生成ORM，零反射)
- NATS.Client.Core
- Stripe.net
- FusionCache
- Yitter.IdGenerator

**仓储实现**：
- 使用Sqlx的`[Sqlx]`特性定义SQL
- 使用`[RepositoryFor(typeof(IXxxRepository))]`标记接口实现
- 编译时生成代码，零运行时反射
- 完全AOT兼容

### 4. 前端层 (CatCat.Web)

**技术栈**：
- Vue 3 (Composition API)
- Vue Router 4
- Pinia (状态管理)
- Vuestic UI (移动+桌面自适应)
- Axios
- TypeScript

**响应式设计**：
- 手机端: 320px - 767px
- 平板: 768px - 1023px
- 桌面: 1024px+

## 项目简化总结

| 优化项 | 优化前 | 优化后 | 说明 |
|--------|--------|--------|------|
| 项目数量 | 5个 | 3个 | 合并Domain+Core到Infrastructure |
| 命名空间 | 多层级 | 简化 | `CatCat.Infrastructure.*` |
| 依赖关系 | 复杂 | 清晰 | API → Infrastructure, Gateway独立 |
| 代码行数 | ~1,112 | ~964 | 减少13.3% |
| 编译警告 | 11个 | 0个 | 100%消除 |
| AOT兼容性 | 部分 | 100% | 完全支持 |

## 数据流向

### 1. 用户登录流程

```
用户输入 → Login.vue
    ↓
store.loginUser()
    ↓
api/auth.ts (login)
    ↓
Gateway (YARP) → API
    ↓
AuthEndpoints.Login
    ↓
UserRepository.GetByPhoneAsync (Sqlx)
    ↓
PostgreSQL
    ↓
生成JWT Token
    ↓
返回Token + UserInfo
    ↓
存储到localStorage
    ↓
跳转到首页
```

### 2. 创建订单流程

```
用户下单 → Home.vue
    ↓
POST /api/orders
    ↓
Gateway → API
    ↓
OrderEndpoints.CreateOrder
    ↓
OrderService.CreateOrderAsync
    ↓
├── 1. 验证用户和套餐（FusionCache缓存）
├── 2. 创建订单（Sqlx Repository）
├── 3. 创建支付意图（Stripe）
├── 4. 异步记录状态历史（NATS消息队列）
└── 5. 发布订单创建事件（NATS）
    ↓
订单创建成功
    ↓
后台消费者处理通知
```

### 3. 订单状态流转

```
订单状态：
  Pending (待接单)
    ↓ AcceptOrderAsync
  Accepted (已接单)
    ↓ StartServiceAsync
  InProgress (服务中)
    ↓ CompleteServiceAsync
  Completed (已完成)
    ↓ CreateReviewAsync
  Reviewed (已评价)

状态变更：
  - 每次状态变更通过NATS异步记录历史
  - 清除相关缓存
  - 推送通知（NATS）
```

## 配置管理

### 中央包管理 (Directory.Packages.props)

```xml
<PackageVersion Include="Npgsql" Version="9.0.1" />
<PackageVersion Include="Sqlx" Version="0.0.4" />
<PackageVersion Include="ZiggyCreatures.FusionCache" Version="2.0.0" />
<PackageVersion Include="NATS.Client.Core" Version="2.5.3" />
<PackageVersion Include="Stripe.net" Version="47.3.0" />
<PackageVersion Include="Yitter.IdGenerator" Version="1.0.14" />
```

### 应用配置 (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=catcat;...",
    "Redis": "localhost:6379",
    "Nats": "nats://localhost:4222"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "CatCat",
    "Audience": "CatCat.API",
    "ExpiryDays": 7
  },
  "Database": {
    "MaxConcurrency": 40,
    "SlowQueryThresholdMs": 1000
  },
  "Stripe": {
    "ApiKey": "sk_test_..."
  }
}
```

## 开发规范

### 命名约定

- **C# 类名**: PascalCase (e.g., `OrderService`)
- **C# 接口**: IPascalCase (e.g., `IOrderRepository`)
- **C# 方法**: PascalCase (e.g., `CreateOrderAsync`)
- **C# 参数**: camelCase (e.g., `orderId`)
- **TypeScript 文件**: kebab-case (e.g., `user-service.ts`)
- **Vue 组件**: PascalCase (e.g., `OrderDetail.vue`)
- **数据库表**: snake_case (e.g., `service_orders`)

### 代码原则

- ✅ DRY (Don't Repeat Yourself)
- ✅ SOLID原则
- ✅ 依赖注入
- ✅ 异步优先
- ✅ Result模式（避免异常）
- ✅ 显式类型（避免匿名类型）
- ✅ AOT优先

## 扩展点

### 1. 添加新实体

1. 在 `CatCat.Infrastructure/Entities/` 创建实体类
2. 在 `database/init.sql` 添加表结构
3. 在 `CatCat.Infrastructure/Repositories/` 创建Repository接口和类
4. 在 `CatCat.API/Endpoints/` 创建Endpoints
5. 在 `CatCat.API/Json/AppJsonContext.cs` 注册类型
6. 在前端创建对应的API和页面

### 2. 添加新服务

1. 在 `CatCat.Infrastructure/Services/` 创建服务接口和实现
2. 在 `ServiceCollectionExtensions.cs` 注册服务
3. 在Endpoints中注入使用

### 3. 集成第三方服务

在 `CatCat.Infrastructure` 中添加新目录，如：
- `Sms/` - 短信服务
- `Storage/` - 对象存储
- `Map/` - 地图服务

## 相关文档

- [架构设计](ARCHITECTURE.md)
- [API文档](API.md)
- [快速启动](QUICK_START.md)
- [部署指南](DEPLOYMENT.md)
- [优化总结](OPTIMIZATION_SUMMARY.md)
- [AOT和集群](AOT_AND_CLUSTER.md)
- [限流指南](RATE_LIMITING_GUIDE.md)
- [OpenTelemetry指南](OPENTELEMETRY_GUIDE.md)

---

*最后更新: 2025-10-02 - 项目已简化到3层架构*
