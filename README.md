# 🐱 CatCat - 上门喂猫服务平台

> 安全可靠可控的 B2C 上门喂猫服务平台
> **技术栈**: ASP.NET Core 9 + Vue 3 + PostgreSQL + Redis + NATS

---

## 🚀 项目特点

- ✅ **极简代码**: Repository 层仅 200 行（使用 Sqlx Source Generator）
- ✅ **完全类型安全**: 编译时检查，零运行时错误
- ✅ **AOT 就绪**: 零反射，极快启动，极小体积
- ✅ **高性能**: FusionCache + NATS + Snowflake ID
- ✅ **可观察**: OpenTelemetry 分布式追踪
- ✅ **一键部署**: Docker Compose + GitHub Actions CI/CD

---

## 📦 技术栈

### 后端
- **框架**: ASP.NET Core 9 (Minimal API)
- **ORM**: Sqlx (Source Generator)
- **数据库**: PostgreSQL 16
- **缓存**: FusionCache + Redis 7
- **消息队列**: NATS 2
- **支付**: Stripe
- **ID生成**: Yitter Snowflake
- **可观察性**: OpenTelemetry

### 前端
- **框架**: Vue 3 + TypeScript
- **UI库**: Vuestic UI
- **状态管理**: Pinia
- **路由**: Vue Router 4
- **构建**: Vite

### DevOps
- **容器**: Docker + Docker Compose
- **CI/CD**: GitHub Actions
- **监控**: Jaeger + Prometheus + Grafana
- **反向代理**: Nginx

---

## 🏗️ 项目结构

```
CatCat/
├── src/
│   ├── CatCat.API/              # Minimal API 端点
│   ├── CatCat.Core/             # 业务逻辑层
│   ├── CatCat.Domain/           # 领域实体
│   ├── CatCat.Infrastructure/   # 基础设施层
│   └── CatCat.Web/              # Vue 3 前端
├── .github/workflows/           # CI/CD 配置
├── database/                    # 数据库脚本
├── nginx/                       # Nginx 配置
├── Directory.Packages.props     # 中央包管理
├── Directory.Build.props        # 统一项目配置
├── docker-compose.yml           # 完整服务编排
├── Dockerfile                   # 常规镜像
├── Dockerfile.aot               # AOT 优化镜像
└── build.ps1 / build.sh         # 一键编译脚本
```

---

## ⚡ 快速开始

### 前置要求
- .NET 9.0 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (或使用 Docker)

### 本地开发

```bash
# 1. 克隆项目
git clone https://github.com/your-org/CatCat.git
cd CatCat

# 2. 启动基础设施（PostgreSQL + Redis + NATS）
docker-compose up -d postgres redis nats

# 3. 编译后端
# Windows
.\build.ps1
# Linux/Mac
chmod +x build.sh && ./build.sh

# 4. 运行后端
cd src/CatCat.API
dotnet run

# 5. 启动前端（新终端）
cd src/CatCat.Web
npm install
npm run dev
```

访问:
- **前端**: http://localhost:5173
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

### Docker 部署

```bash
# 完整部署（PostgreSQL + Redis + NATS + API + Nginx）
docker-compose up -d

# 访问
http://localhost
```

### AOT 编译部署

```bash
# 构建 AOT 镜像（极致性能，最小体积）
docker build -f Dockerfile.aot -t catcat-aot .
docker run -p 80:80 catcat-aot
```

---

## 📊 性能指标

| 指标 | 常规模式 | AOT 模式 |
|------|----------|----------|
| 启动时间 | ~2 秒 | ~0.5 秒 |
| 内存占用 | ~200MB | ~50MB |
| 程序大小 | ~80MB | ~15MB |
| 首次请求 | ~50ms | ~10ms |

---

## 🎯 核心功能

### C 端（客户）
- ✅ 手机号登录/注册
- ✅ 猫咪档案管理
- ✅ 浏览服务套餐
- ✅ 预约上门喂猫
- ✅ 实时订单跟踪
- ✅ 在线支付（Stripe）
- ✅ 服务评价

### B 端（服务商）
- ✅ 接单管理
- ✅ 订单状态更新
- ✅ 服务记录上传
- ✅ 收入统计
- ✅ 客户评价回复

### 管理端
- ✅ 用户管理
- ✅ 订单监控
- ✅ 服务包管理
- ✅ 数据统计
- ✅ 系统配置

---

## 🔧 开发指南

### 编译项目

```bash
# Windows
.\build.ps1

# Linux/Mac
./build.sh
```

### 运行测试

```bash
dotnet test
```

### 代码格式化

```bash
dotnet format
```

### 前端开发

```bash
cd src/CatCat.Web
npm run dev      # 开发服务器
npm run build    # 生产构建
npm run lint     # 代码检查
```

---

## 📈 架构亮点

### 1. Sqlx Source Generator
使用 Source Generator 在编译时生成数据访问代码，实现：
- ✅ 零运行时反射
- ✅ 完全类型安全
- ✅ 极简代码（接口 + 空类型）
- ✅ 完美支持 AOT

```csharp
// 只需定义接口和空类型
public interface IUserRepository
{
    [Sqlx("SELECT * FROM users WHERE id = @id")]
    Task<User?> GetByIdAsync(long id);
}

[RepositoryFor(typeof(IDbConnectionFactory))]
public partial class UserRepository : IUserRepository
{
    // Sqlx Source Generator 自动生成实现
}
```

### 2. FusionCache 混合缓存
三层缓存架构：
- **L1**: 内存缓存（超快访问）
- **L2**: Redis 缓存（集群共享）
- **Backplane**: 集群间缓存同步

### 3. NATS 消息队列
异步处理高并发：
- ✅ 订单创建削峰
- ✅ 评价异步处理
- ✅ 事件驱动架构

### 4. OpenTelemetry 可观察性
完整的分布式追踪：
- ✅ 请求链路追踪
- ✅ 性能指标监控
- ✅ 自定义业务指标

---

## 📚 文档

- [全面优化报告](COMPREHENSIVE_OPTIMIZATION.md)
- [限流配置指南](RATE_LIMITING_GUIDE.md)
- [OpenTelemetry 指南](OPENTELEMETRY_GUIDE.md)
- [中央包管理说明](CENTRAL_PACKAGE_MANAGEMENT.md)
- [贡献指南](CONTRIBUTING.md)

---

## 🔒 安全特性

- ✅ JWT 认证授权
- ✅ API 限流防护
- ✅ HTTPS 强制
- ✅ SQL 注入防护
- ✅ XSS 防护
- ✅ CSRF 防护

---

## 📱 多端适配

响应式设计，完美支持：
- ✅ 桌面浏览器
- ✅ 平板设备
- ✅ 手机浏览器

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

---

## 📄 开源协议

本项目采用 MIT 协议开源。

---

## 🙏 致谢

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Sqlx](https://github.com/Cricle/Sqlx)
- [FusionCache](https://github.com/ZiggyCreatures/FusionCache)
- [Vue.js](https://vuejs.org/)
- [Vuestic UI](https://vuestic.dev/)
- [OpenTelemetry](https://opentelemetry.io/)

---

**Made with ❤️ by CatCat Team**
