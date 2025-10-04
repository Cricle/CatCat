# 项目结构

## 📁 解决方案结构

```
CatCat/
├── src/
│   ├── CatCat.API/              # 主 API 服务
│   ├── CatCat.Infrastructure/   # 基础设施层
│   ├── CatCat.AppHost/          # Aspire 编排
│   ├── CatCat.Transit/          # CQRS 库（内存传输）
│   ├── CatCat.Transit.Nats/     # CQRS 库（NATS 传输）
│   └── CatCat.Web/              # Vue 前端
├── docs/                        # 文档
├── docker-compose.yml           # Docker 编排
└── CatCat.sln                   # 解决方案文件
```

## 🏗️ 项目依赖关系

```
CatCat.API
  ├─> CatCat.Infrastructure
  └─> CatCat.Transit (可选)

CatCat.Infrastructure
  └─> 无依赖（独立基础设施层）

CatCat.Transit
  └─> 无依赖（独立 CQRS 库）

CatCat.Transit.Nats
  └─> CatCat.Transit

CatCat.AppHost
  └─> CatCat.API
```

## 📦 CatCat.API - 主 API 服务

### 核心功能
- ✅ 宠物托管服务 API
- ✅ JWT 认证授权
- ✅ Stripe 支付集成
- ✅ MinIO 对象存储
- ✅ OpenTelemetry 可观测性
- ✅ Prometheus + Grafana 监控
- ✅ AOT 编译支持

### 目录结构
```
CatCat.API/
├── Endpoints/          # Minimal API 端点
├── Middleware/         # 中间件（异常处理、追踪）
├── Configuration/      # 服务配置扩展
├── Observability/      # 监控指标
├── Models/            # API 模型（请求/响应）
└── Program.cs         # 启动入口
```

## 🧱 CatCat.Infrastructure - 基础设施层

### 核心功能
- ✅ 数据库访问（PostgreSQL + Dapper）
- ✅ 缓存服务（Redis）
- ✅ 消息队列（NATS JetStream）
- ✅ 对象存储（MinIO）
- ✅ 支付服务（Stripe）
- ✅ 雪花 ID 生成器
- ✅ 弹性策略（Polly）
- ✅ 领域事件（内存）
- ✅ CQRS 接口定义

### 目录结构
```
CatCat.Infrastructure/
├── Entities/           # 数据实体
├── Repositories/       # 仓储实现
├── Services/           # 业务服务
├── Database/           # 数据库工厂、指标
├── Cache/             # 缓存配置
├── MessageQueue/      # NATS 配置
├── Storage/           # MinIO 服务
├── Payment/           # Stripe 服务
├── Events/            # 领域事件
├── CQRS/              # CQRS 接口
└── Common/            # 通用类型
```

## 🚀 CatCat.Transit - CQRS 库

### 核心功能
- ✅ 100% AOT 兼容
- ✅ 无锁设计（原子操作）
- ✅ 非阻塞异步
- ✅ Pipeline Behaviors
- ✅ 内存传输
- ✅ 高性能（分片存储）

### 目录结构
```
CatCat.Transit/
├── Messages/           # 消息接口（IQuery, ICommand, IEvent）
├── Handlers/           # 处理器接口
├── Pipeline/           # Pipeline 行为
│   └── Behaviors/      # Logging, Tracing, Idempotency, Retry, Validation
├── Configuration/      # 配置选项
├── Concurrency/        # 并发限制器
├── RateLimiting/       # 限流器
├── Resilience/         # 熔断器
├── Idempotency/        # 幂等存储
├── DeadLetter/         # 死信队列
├── Results/            # 结果类型
└── TransitMediator.cs  # 核心 Mediator
```

## 🌐 CatCat.Transit.Nats - NATS 传输

### 核心功能
- ✅ NATS 客户端集成
- ✅ Request-Reply 模式
- ✅ Pub-Sub 模式
- ✅ 同样的弹性机制（熔断器、限流、并发控制）
- ⚠️ **待增强**：Pipeline Behaviors 集成

### 目录结构
```
CatCat.Transit.Nats/
├── NatsTransitMediator.cs      # NATS Mediator
└── DependencyInjection/        # 服务注册
```

## 🎨 CatCat.Web - Vue 前端

### 技术栈
- Vue 3 + Vite
- Vuestic UI
- TypeScript
- Pinia
- Vue Router
- Axios
- i18n（中英文）

### 目录结构
```
CatCat.Web/src/
├── components/         # 组件
├── pages/             # 页面
├── layouts/           # 布局
├── stores/            # Pinia 状态
├── services/          # API 服务
├── router/            # 路由
└── i18n/              # 国际化
```

## 🐳 CatCat.AppHost - Aspire 编排

### 功能
- ✅ 服务编排
- ✅ 依赖管理
- ✅ 健康检查
- ✅ 服务发现

## 📊 技术栈总结

| 层级 | 技术 |
|------|------|
| **API** | ASP.NET Core 9, Minimal API, AOT |
| **数据库** | PostgreSQL, Dapper |
| **缓存** | Redis, Bloom Filter |
| **消息队列** | NATS JetStream |
| **存储** | MinIO (S3) |
| **支付** | Stripe.NET |
| **监控** | OpenTelemetry, Prometheus, Grafana |
| **CQRS** | CatCat.Transit (自研) |
| **前端** | Vue 3, Vuestic UI, TypeScript |
| **编排** | .NET Aspire, Docker Compose |

## 🔧 开发命令

```bash
# 恢复依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行 API
dotnet run --project src/CatCat.API

# 运行 Aspire
dotnet run --project src/CatCat.AppHost

# 运行前端
cd src/CatCat.Web && npm run dev
```

