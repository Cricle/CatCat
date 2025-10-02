# Docker Compose & .NET Aspire 使用指南

本文档介绍如何使用 Docker Compose 和 .NET Aspire 运行 CatCat 项目。

## 📋 前置要求

### Docker Compose
- Docker Desktop 4.20+
- Docker Compose v2.20+

### .NET Aspire
- .NET 9 SDK
- Visual Studio 2022 17.9+ 或 VS Code with C# Dev Kit
- Docker Desktop (用于容器编排)

## 🚀 快速开始

### 选项 1: 使用 Docker Compose (生产环境)

#### 1. 克隆项目
```bash
git clone https://github.com/your-org/CatCat.git
cd CatCat
```

#### 2. 配置环境变量
```bash
cp env.example .env
# 编辑 .env 文件，填入生产环境的配置
```

#### 3. 启动所有服务
```bash
# Windows PowerShell
.\scripts\prod-start.ps1

# 或手动启动
docker-compose up -d
```

#### 4. 访问应用
- **Web 前端**: http://localhost:3000
- **API 后端**: http://localhost:5000
- **API Swagger**: http://localhost:5000/swagger (仅 Debug 模式)
- **Jaeger UI**: http://localhost:16686

#### 5. 查看日志
```bash
# 所有服务日志
docker-compose logs -f

# 特定服务日志
docker-compose logs -f api
docker-compose logs -f web
```

#### 6. 停止服务
```bash
docker-compose down

# 同时删除数据卷（慎用！）
docker-compose down -v
```

---

### 选项 2: 使用 .NET Aspire (开发环境)

#### 1. 安装 .NET Aspire 工作负载
```bash
dotnet workload update
dotnet workload install aspire
```

#### 2. 启动基础设施服务
```bash
# 仅启动 PostgreSQL, Redis, NATS
.\scripts\dev-start.ps1
```

#### 3. 运行 Aspire AppHost
```bash
# 使用 Aspire Dashboard
dotnet run --project src/CatCat.AppHost

# 或使用 Visual Studio
# 设置 CatCat.AppHost 为启动项目，按 F5
```

#### 4. 访问 Aspire Dashboard
- **Aspire Dashboard**: http://localhost:15000 (默认端口)
- 可查看所有服务状态、日志、指标、追踪

#### 5. 独立运行服务（可选）
```bash
# 运行 API
dotnet run --project src/CatCat.API

# 运行 Web
cd src/CatCat.Web
npm install
npm run dev
```

---

## 📦 服务端口映射

### 生产环境 (docker-compose.yml)
| 服务 | 容器端口 | 主机端口 | 描述 |
|------|---------|---------|------|
| PostgreSQL | 5432 | 5432 | 数据库 |
| Redis | 6379 | 6379 | 缓存 |
| NATS | 4222 | 4222 | 消息队列（客户端） |
| NATS | 8222 | 8222 | 消息队列（监控） |
| API | 8080 | 5000 | 后端 API |
| Web | 80 | 3000 | 前端应用 |
| Jaeger UI | 16686 | 16686 | 追踪界面 |
| Jaeger OTLP | 4317 | 4317 | 追踪收集 |

### 开发环境 (docker-compose.override.yml)
| 服务 | 容器端口 | 主机端口 | 描述 |
|------|---------|---------|------|
| PostgreSQL | 5432 | 15432 | 数据库 |
| Redis | 6379 | 16379 | 缓存 |
| NATS | 4222 | 14222 | 消息队列 |
| NATS | 8222 | 18222 | 监控 |
| Adminer | 8080 | 8080 | 数据库管理 |
| Redis Commander | 8081 | 8081 | Redis 管理 |

---

## 🔧 配置说明

### Docker Compose 配置

**docker-compose.yml** - 生产环境配置
- 使用生产级镜像
- 数据持久化
- 健康检查
- 资源限制

**docker-compose.override.yml** - 开发环境覆盖
- 热重载支持
- 开发工具（Adminer, Redis Commander）
- 使用不同端口避免冲突
- 挂载源代码卷

### Aspire 配置

**src/CatCat.AppHost/Program.cs** - Aspire 编排配置
- 服务依赖声明
- 环境变量配置
- 资源引用
- 健康检查

---

## 🛠️ 开发工具

### 使用 Docker Compose 时
```bash
# 进入 API 容器
docker-compose exec api bash

# 查看数据库
# 访问 http://localhost:8080 (Adminer)

# 查看 Redis
# 访问 http://localhost:8081 (Redis Commander)

# 查看 NATS 监控
# 访问 http://localhost:18222 (开发) 或 http://localhost:8222 (生产)
```

### 使用 Aspire 时
```bash
# 访问 Aspire Dashboard
http://localhost:15000

# Dashboard 提供:
# - 服务列表和状态
# - 实时日志查看
# - OpenTelemetry 追踪
# - 环境变量查看
# - 资源依赖图
```

---

## 📊 数据持久化

### Docker Volumes
```bash
# 查看所有卷
docker volume ls | grep catcat

# 备份 PostgreSQL 数据
docker-compose exec postgres pg_dump -U catcat catcat > backup.sql

# 恢复 PostgreSQL 数据
docker-compose exec -T postgres psql -U catcat catcat < backup.sql

# 删除所有数据（谨慎！）
docker-compose down -v
```

### Aspire 数据
- Aspire 使用 Docker 卷，与 Docker Compose 共享
- 数据存储在 Docker Desktop 管理的卷中

---

## 🐛 故障排查

### Docker Compose 问题

**服务无法启动**
```bash
# 查看服务日志
docker-compose logs service-name

# 重建并重启服务
docker-compose up -d --force-recreate service-name
```

**数据库连接失败**
```bash
# 确认数据库已就绪
docker-compose ps postgres

# 检查健康状态
docker inspect catcat-postgres | grep Health
```

**端口冲突**
```bash
# 查看占用端口的进程
netstat -ano | findstr :5432

# 修改 docker-compose.override.yml 中的端口映射
```

### Aspire 问题

**Dashboard 无法访问**
- 确认 AppHost 正在运行
- 检查 `dotnet run` 输出中的 Dashboard URL
- 默认 URL: http://localhost:15000

**服务无法启动**
- 确认 Docker Desktop 正在运行
- 检查端口是否被占用
- 查看 Aspire Dashboard 中的日志

**依赖服务未就绪**
- Aspire 会自动等待依赖服务健康检查
- 在 Dashboard 中查看服务状态
- 必要时手动重启服务

---

## 🔒 安全建议

### 生产环境
1. **更改所有默认密码**
   ```bash
   # 在 .env 文件中设置强密码
   POSTGRES_PASSWORD=<strong-password>
   REDIS_PASSWORD=<strong-password>
   JWT_SECRET_KEY=<at-least-32-characters>
   ```

2. **使用 secrets 管理敏感信息**
   ```bash
   # 不要将 .env 提交到 Git
   echo ".env" >> .gitignore
   ```

3. **启用 HTTPS**
   - 使用反向代理（Nginx/Caddy）
   - 配置 SSL 证书
   - 强制 HTTPS 重定向

4. **限制网络访问**
   ```yaml
   # 在 docker-compose.yml 中限制暴露的端口
   ports:
     - "127.0.0.1:5432:5432"  # 仅本地访问
   ```

### 开发环境
- 使用 `docker-compose.override.yml` 覆盖配置
- 不要在开发环境使用生产密钥
- 使用开发专用的 Stripe 测试密钥

---

## 📈 性能优化

### Docker Compose
```yaml
# 在 docker-compose.yml 中添加资源限制
services:
  api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

### Aspire
- Aspire 自动处理服务编排
- 使用 Aspire Dashboard 监控资源使用
- 根据需要调整服务副本数

---

## 🔄 CI/CD 集成

### GitHub Actions 示例
```yaml
# .github/workflows/docker.yml
name: Docker Build

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build images
        run: docker-compose build

      - name: Push to registry
        run: |
          echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
          docker-compose push
```

---

## 📚 更多资源

- [Docker Compose 官方文档](https://docs.docker.com/compose/)
- [.NET Aspire 官方文档](https://learn.microsoft.com/dotnet/aspire/)
- [NATS JetStream 文档](https://docs.nats.io/nats-concepts/jetstream)
- [PostgreSQL 官方文档](https://www.postgresql.org/docs/)
- [Redis 官方文档](https://redis.io/documentation)

---

## 🤝 贡献

如有问题或建议，请提交 Issue 或 Pull Request。

