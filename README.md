# 🐱 CatCat - 上门喂猫服务平台

> 现代化 B2C 上门喂猫服务平台
> **ASP.NET Core 9 + Vue 3 + PostgreSQL + Redis + NATS**

---

## ⚡ 快速开始

```bash
# 使用 Aspire 启动（推荐）
dotnet run --project src/CatCat.AppHost

# 或使用 Docker Compose
docker-compose up -d

# 启动前端
cd src/CatCat.Web && npm install && npm run dev
```

**访问**: http://localhost:5173 (前端) | http://localhost:15000 (Aspire Dashboard)

---

## 🚀 核心特性

### 技术亮点
- ✅ **极简代码**: Sqlx Source Generator，Repository 层仅 200 行
- ✅ **AOT 就绪**: 零反射，启动快，体积小（~15MB）
- ✅ **高性能**: Redis 缓存 + NATS 异步队列 + Snowflake ID
- ✅ **可观测**: OpenTelemetry 分布式追踪 + Prometheus + Grafana
- ✅ **现代架构**: Clean Architecture + Result Pattern + C# 12 主构造函数

### 业务功能
#### C 端（客户）
- ✅ 手机号登录注册
- ✅ 宠物档案管理
- ✅ 浏览服务套餐
- ✅ 预约上门服务
- ✅ 实时订单跟踪
- ✅ 在线支付（Stripe）
- ✅ 服务评价

#### B 端（服务商）
- ✅ 接单管理
- ✅ 订单状态更新
- ✅ 服务记录上传
- ✅ 收入统计

#### 管理端
- ✅ 用户管理
- ✅ 订单监控
- ✅ 服务包管理
- ✅ 数据统计

---

## 📦 技术栈

### 后端
| 组件 | 技术 |
|------|------|
| 框架 | ASP.NET Core 9 (Minimal API) |
| ORM | Sqlx (Source Generator) |
| 数据库 | PostgreSQL 16 |
| 缓存 | FusionCache + Redis |
| 消息队列 | NATS JetStream |
| 对象存储 | MinIO (S3 兼容) |
| 支付 | Stripe |
| 可观测 | OpenTelemetry, Prometheus, Grafana |

### 前端
| 组件 | 技术 |
|------|------|
| 框架 | Vue 3.5 + TypeScript |
| UI 库 | Vuestic Admin (10.9k+ Stars) |
| 状态 | Pinia |
| 路由 | Vue Router 4 |
| 国际化 | Vue I18n (中/英) |
| 构建 | Vite |

---

## 🏗️ 架构亮点

### 1. Sqlx Source Generator
零运行时反射，完全类型安全：

```csharp
public interface IUserRepository
{
    [Sqlx("SELECT * FROM users WHERE id = @id")]
    Task<User?> GetByIdAsync(long id);
}

[RepositoryFor(typeof(IUserRepository))]
public partial class UserRepository : IUserRepository
{
    // Sqlx 自动生成实现
}
```

### 2. 异步订单处理
削峰填谷，快速响应：

```
Client → API (立即返回 OrderId, 50-100ms)
         ↓
   NATS Queue (持久化)
         ↓
Background Worker (异步处理)
```

### 3. Redis 缓存策略
- **服务套餐**: 2小时缓存（~90% 命中率）
- **用户信息**: 20分钟缓存（~80% 命中率）
- **宠物信息**: 30分钟缓存（~70% 命中率）

### 4. C# 12 主构造函数
简化代码 80+ 行：

```csharp
// ❌ 传统方式
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IFusionCache _cache;
    
    public UserService(IUserRepository repository, IFusionCache cache)
    {
        _repository = repository;
        _cache = cache;
    }
}

// ✅ C# 12 主构造函数
public class UserService(
    IUserRepository repository,
    IFusionCache cache,
    ILogger<UserService> logger) : IUserService
{
    // 直接使用参数
}
```

---

## 📊 性能指标

| 指标 | 常规模式 | AOT 模式 |
|------|----------|----------|
| 启动时间 | ~2 秒 | ~0.5 秒 |
| 内存占用 | ~200MB | ~50MB |
| 程序大小 | ~80MB | ~15MB |
| Docker 镜像 | ~220MB | ~30MB |

---

## 🎨 UI/UX 设计

### Vuestic Admin 企业级模板
✅ **已采用** [Vuestic Admin](https://github.com/epicmaxco/vuestic-admin)

- ⭐ 10.9k+ GitHub Stars
- 📄 MIT License（可商用）
- 📦 60+ Vuestic UI 组件
- 📱 完美响应式设计
- 🌙 深色模式支持
- 🌐 多语言支持（中/英/葡/波斯/西班牙）

---

## 📚 文档

- **[📖 完整文档索引](docs/README.md)** - 所有文档导航
- **[🏗️ 架构设计](docs/ARCHITECTURE.md)** - 系统架构详解
- **[📡 API 文档](docs/API.md)** - REST API 接口
- **[⚙️ 环境配置](docs/ENVIRONMENT.md)** - 配置说明
- **[📈 可观测性](docs/OPENTELEMETRY_GUIDE.md)** - 追踪和监控
- **[📦 MinIO 存储](docs/MINIO_STORAGE_GUIDE.md)** - 对象存储
- **[🛡️ 限流配置](docs/RATE_LIMITING_GUIDE.md)** - API 防护
- **[🌐 国际化](docs/I18N_GUIDE.md)** - 多语言支持

---

## 🔧 开发指南

### 前置要求
- .NET 9.0 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (或使用 Docker)

### 本地开发

#### 选项 1: Aspire（推荐）
```bash
# 安装 Aspire 工作负载
dotnet workload install aspire

# 启动所有服务
dotnet run --project src/CatCat.AppHost

# 启动前端（新终端）
cd src/CatCat.Web
npm install
npm run dev
```

#### 选项 2: Docker Compose
```bash
docker-compose up -d
cd src/CatCat.Web
npm install
npm run dev
```

### 编译和测试
```bash
# 编译
.\build.ps1  # Windows
./build.sh   # Linux/Mac

# 测试
dotnet test

# 格式化
dotnet format
```

---

## 📂 项目结构

```
CatCat/
├── src/
│   ├── CatCat.API/                # Minimal API
│   ├── CatCat.Infrastructure/     # 基础设施层
│   ├── CatCat.AppHost/            # Aspire 编排
│   └── CatCat.Web/                # Vue 3 前端
├── docs/                          # 文档
├── Directory.Packages.props       # 中央包管理
├── docker-compose.yml             # Docker 编排
└── build.ps1/build.sh             # 编译脚本
```

---

## 🔒 安全特性

- ✅ JWT 认证授权
- ✅ API 限流防护
- ✅ HTTPS 强制
- ✅ SQL 注入防护
- ✅ XSS 防护
- ✅ CSRF 防护

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

---

## 📄 开源协议

MIT License - 可商用

---

## 🙏 致谢

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Sqlx](https://github.com/Cricle/Sqlx)
- [FusionCache](https://github.com/ZiggyCreatures/FusionCache)
- [Vue.js](https://vuejs.org/)
- [Vuestic Admin](https://github.com/epicmaxco/vuestic-admin)
- [OpenTelemetry](https://opentelemetry.io/)

---

**Made with ❤️ by CatCat Team**
