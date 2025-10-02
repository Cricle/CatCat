# CatCat 快速启动指南

## 前置要求

### 开发环境
- **.NET 9 SDK**: [下载安装](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+**: [下载安装](https://nodejs.org/)
- **Docker Desktop**: [下载安装](https://www.docker.com/products/docker-desktop)
- **Git**: [下载安装](https://git-scm.com/)

### 可选工具
- **Visual Studio 2022** 或 **VS Code**
- **PostgreSQL客户端**: DBeaver、pgAdmin等
- **Redis客户端**: RedisInsight、Another Redis Desktop Manager

## 快速启动（推荐）

### 1. 克隆项目
```bash
git clone https://github.com/your-username/CatCat.git
cd CatCat
```

### 2. 使用Docker Compose一键启动
```bash
# 启动所有服务（数据库、缓存、消息队列、API、前端）
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看日志
docker-compose logs -f
```

### 3. 访问应用
- **前端页面**: http://localhost
- **API文档**: http://localhost:5000/swagger
- **NATS监控**: http://localhost:8222

### 4. 停止服务
```bash
docker-compose down

# 如果要删除数据卷
docker-compose down -v
```

## 本地开发模式

如果你需要修改代码并实时调试，建议使用本地开发模式：

### 1. 启动基础服务
```bash
# 只启动PostgreSQL、Redis、NATS
docker-compose up -d postgres redis nats
```

### 2. 启动后端API

#### Windows (PowerShell)
```powershell
cd src\CatCat.API
dotnet restore
dotnet run
```

#### Linux/macOS
```bash
cd src/CatCat.API
dotnet restore
dotnet run
```

后端将运行在: http://localhost:5000

### 3. 启动前端

打开新的终端窗口：

#### Windows (PowerShell)
```powershell
cd src\CatCat.Web
npm install
npm run dev
```

#### Linux/macOS
```bash
cd src/CatCat.Web
npm install
npm run dev
```

前端将运行在: http://localhost:5173

## 初始化数据

### 数据库自动初始化
使用Docker Compose启动时，数据库会自动执行 `database/init.sql` 脚本，创建所有表和初始数据。

### 手动初始化（如果需要）
```bash
# 连接到PostgreSQL
psql -h localhost -U postgres -d catcat -f database/init.sql
```

### 默认账号
- **管理员账号**: 13800138000
- **默认密码**: admin123（生产环境请修改）

## 开发调试

### 后端调试（Visual Studio）
1. 打开 `CatCat.sln`
2. 设置 `CatCat.API` 为启动项目
3. 按 F5 开始调试

### 后端调试（VS Code）
1. 打开项目根目录
2. 安装C# Dev Kit扩展
3. 按 F5，选择 `.NET Core Launch`

### 前端调试
1. 在浏览器中打开 http://localhost:5173
2. 按 F12 打开开发者工具
3. 使用Vue DevTools扩展

## 常见命令

### 后端相关
```bash
# 恢复依赖
dotnet restore

# 编译项目
dotnet build

# 运行项目
dotnet run

# 发布项目（生产）
dotnet publish -c Release -o ./publish

# 发布项目（AOT）
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
```

### 前端相关
```bash
# 安装依赖
npm install

# 开发模式
npm run dev

# 构建生产版本
npm run build

# 预览生产构建
npm run preview

# 代码检查
npm run lint
```

### Docker相关
```bash
# 构建镜像
docker-compose build

# 启动服务
docker-compose up -d

# 停止服务
docker-compose stop

# 查看日志
docker-compose logs -f [service-name]

# 重启服务
docker-compose restart [service-name]

# 进入容器
docker-compose exec [service-name] sh
```

## 数据库管理

### 连接信息
- **Host**: localhost
- **Port**: 5432
- **Database**: catcat
- **Username**: postgres
- **Password**: postgres

### 常用SQL
```sql
-- 查看所有用户
SELECT * FROM users;

-- 查看所有订单
SELECT * FROM service_orders ORDER BY created_at DESC;

-- 查看服务人员统计
SELECT
  sp.id,
  u.nick_name,
  sp.rating,
  sp.service_count
FROM service_providers sp
JOIN users u ON sp.user_id = u.id
ORDER BY sp.rating DESC;
```

## 配置说明

### 后端配置文件
文件位置: `src/CatCat.API/appsettings.json`

关键配置项：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PostgreSQL连接字符串",
    "Redis": "Redis连接字符串",
    "Nats": "NATS连接字符串"
  },
  "JwtSettings": {
    "SecretKey": "JWT密钥（生产环境必改）",
    "Issuer": "签发者",
    "Audience": "受众"
  }
}
```

### 前端配置文件
文件位置: `src/CatCat.Web/vite.config.ts`

API代理配置：
```typescript
server: {
  port: 5173,
  proxy: {
    '/api': {
      target: 'http://localhost:5000',
      changeOrigin: true
    }
  }
}
```

## 测试

### 测试账号创建
```bash
# 使用API注册新用户
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "phone": "13900000001",
    "code": "123456",
    "password": "password123",
    "nickName": "测试用户"
  }'
```

### API测试
访问 Swagger UI: http://localhost:5000/swagger

## 故障排查

### 问题1: 端口被占用
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <进程ID> /F

# Linux/macOS
lsof -i :5000
kill -9 <进程ID>
```

### 问题2: Docker容器启动失败
```bash
# 查看详细日志
docker-compose logs [service-name]

# 重建容器
docker-compose up -d --force-recreate [service-name]
```

### 问题3: 数据库连接失败
- 检查PostgreSQL是否启动: `docker-compose ps`
- 检查连接字符串配置
- 检查防火墙设置

### 问题4: 前端无法连接后端API
- 确认后端API已启动
- 检查 `vite.config.ts` 中的代理配置
- 检查浏览器控制台错误信息

## 下一步

- 阅读 [架构设计文档](./ARCHITECTURE.md)
- 查看 [API文档](http://localhost:5000/swagger)
- 了解 [部署指南](./DEPLOYMENT.md)

## 技术支持

遇到问题？
- 查看项目 [Issues](https://github.com/your-username/CatCat/issues)
- 提交新的 Issue
- 联系项目维护者

## 贡献指南

欢迎贡献代码！请阅读 [CONTRIBUTING.md](../CONTRIBUTING.md) 了解详情。

