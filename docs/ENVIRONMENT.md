# 环境变量配置说明

## 创建配置文件

在项目根目录创建 `.env` 文件（不要提交到Git）：

```bash
# 复制下面的内容到 .env 文件
```

## 环境变量列表

### 数据库配置

```bash
# PostgreSQL配置
POSTGRES_DB=catcat
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_this_password_in_production

# 数据库连接字符串
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=catcat;Username=postgres;Password=postgres
```

### Redis配置

```bash
# Redis连接
REDIS_CONNECTION_STRING=localhost:6379
REDIS_PASSWORD=  # 如果Redis设置了密码
```

### NATS配置

```bash
# NATS连接
NATS_URL=nats://localhost:4222
```

### JWT认证配置

```bash
# JWT密钥（生产环境必须修改，至少32个字符）
JWT_SECRET_KEY=your-secret-key-min-32-chars-long-change-this-in-production

# JWT签发者
JWT_ISSUER=CatCat

# JWT受众
JWT_AUDIENCE=CatCat.Client

# Token过期天数
JWT_EXPIRY_DAYS=7
```

### 安全配置

```bash
# 密码加密盐值（生产环境必须修改）
PASSWORD_SALT=your-password-salt-change-in-production
```

### 应用配置

```bash
# 运行环境: Development, Staging, Production
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# 允许的跨域源（逗号分隔）
ALLOWED_ORIGINS=http://localhost,https://your-domain.com

# API监听地址
ASPNETCORE_URLS=http://+:8080
```

### 文件存储配置

```bash
# 存储类型: local, oss, cos, s3
STORAGE_TYPE=local

# 本地存储路径
STORAGE_PATH=/app/uploads

# 最大文件大小（MB）
MAX_FILE_SIZE=10
```

### 阿里云OSS配置（可选）

```bash
# OSS配置
OSS_ENDPOINT=oss-cn-beijing.aliyuncs.com
OSS_ACCESS_KEY_ID=your_access_key_id
OSS_ACCESS_KEY_SECRET=your_access_key_secret
OSS_BUCKET_NAME=your_bucket_name
OSS_CDN_DOMAIN=https://cdn.your-domain.com
```

### 腾讯云COS配置（可选）

```bash
# COS配置
COS_APP_ID=your_app_id
COS_SECRET_ID=your_secret_id
COS_SECRET_KEY=your_secret_key
COS_REGION=ap-beijing
COS_BUCKET=your-bucket
COS_CDN_DOMAIN=https://cdn.your-domain.com
```

### 短信服务配置（可选）

```bash
# 短信服务提供商: aliyun, tencent, twilio
SMS_PROVIDER=aliyun

# 阿里云短信配置
SMS_ACCESS_KEY_ID=your_access_key_id
SMS_ACCESS_KEY_SECRET=your_access_key_secret
SMS_SIGN_NAME=your_sign_name
SMS_TEMPLATE_CODE=SMS_123456789

# 验证码有效期（分钟）
SMS_CODE_EXPIRY_MINUTES=5
```

### 支付配置（可选）

```bash
# 微信支付
WECHAT_PAY_APP_ID=your_app_id
WECHAT_PAY_MCH_ID=your_mch_id
WECHAT_PAY_API_KEY=your_api_key
WECHAT_PAY_CERT_PATH=/path/to/cert.p12

# 支付宝
ALIPAY_APP_ID=your_app_id
ALIPAY_PRIVATE_KEY=your_private_key
ALIPAY_PUBLIC_KEY=alipay_public_key
```

### 地图服务配置（可选）

```bash
# 高德地图
AMAP_KEY=your_amap_key

# 腾讯地图
TENCENT_MAP_KEY=your_tencent_map_key
```

### 监控配置（可选）

```bash
# 是否启用性能监控
ENABLE_METRICS=false

# Prometheus端口
PROMETHEUS_PORT=9090

# 健康检查端点
HEALTH_CHECK_ENABLED=true
```

### 日志配置

```bash
# 日志级别: Trace, Debug, Information, Warning, Error, Critical
LOG_LEVEL=Information

# 日志输出路径
LOG_PATH=/var/log/catcat
```

### 限流配置

```bash
# API限流 - 每分钟最大请求数
RATE_LIMIT_PER_MINUTE=100

# IP黑名单（逗号分隔）
IP_BLACKLIST=

# IP白名单（逗号分隔）
IP_WHITELIST=
```

## 开发环境配置示例

```bash
# .env.development
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development

POSTGRES_DB=catcat_dev
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

REDIS_CONNECTION_STRING=localhost:6379
NATS_URL=nats://localhost:4222

JWT_SECRET_KEY=dev-secret-key-change-in-production
JWT_ISSUER=CatCat
JWT_AUDIENCE=CatCat.Client

PASSWORD_SALT=dev-salt

ALLOWED_ORIGINS=http://localhost:5173,http://localhost:5174

STORAGE_TYPE=local
STORAGE_PATH=./uploads

ENABLE_METRICS=true
LOG_LEVEL=Debug
```

## 生产环境配置示例

```bash
# .env.production
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

POSTGRES_DB=catcat
POSTGRES_USER=catcat_user
POSTGRES_PASSWORD=StrongP@ssw0rd!2025

REDIS_CONNECTION_STRING=redis-cluster.example.com:6379
REDIS_PASSWORD=RedisP@ssw0rd

NATS_URL=nats://nats-cluster.example.com:4222

JWT_SECRET_KEY=generate_a_very_strong_secret_key_at_least_32_characters_long
JWT_ISSUER=CatCat
JWT_AUDIENCE=CatCat.Client
JWT_EXPIRY_DAYS=7

PASSWORD_SALT=generate_strong_salt_value

ALLOWED_ORIGINS=https://catcat.example.com,https://app.catcat.example.com

STORAGE_TYPE=oss
OSS_ENDPOINT=oss-cn-beijing.aliyuncs.com
OSS_ACCESS_KEY_ID=LTAI5tXXXXXXXXXXXXXX
OSS_ACCESS_KEY_SECRET=xxxxxxxxxxxxxxxxxxxxx
OSS_BUCKET_NAME=catcat-prod
OSS_CDN_DOMAIN=https://cdn.catcat.example.com

SMS_PROVIDER=aliyun
SMS_ACCESS_KEY_ID=LTAI5tYYYYYYYYYYYYYY
SMS_ACCESS_KEY_SECRET=yyyyyyyyyyyyyyyyyyyy
SMS_SIGN_NAME=喵喵上门
SMS_TEMPLATE_CODE=SMS_123456789

ENABLE_METRICS=true
PROMETHEUS_PORT=9090

LOG_LEVEL=Information
LOG_PATH=/var/log/catcat

RATE_LIMIT_PER_MINUTE=100
```

## 如何生成安全的密钥

### JWT密钥

```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell (Windows)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# 在线生成
https://generate-random.org/api-token-generator
```

### 密码盐值

```bash
# Linux/macOS
openssl rand -base64 16

# PowerShell (Windows)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(16))
```

## 配置优先级

应用读取配置的优先级（从高到低）：

1. 环境变量
2. `appsettings.{Environment}.json`
3. `appsettings.json`
4. 默认值

## 安全注意事项

⚠️ **重要提醒**:

1. **永远不要**将 `.env` 文件提交到Git
2. **必须修改**所有默认密码和密钥
3. **生产环境**使用强密码（至少16位，包含大小写字母、数字、特殊字符）
4. **定期轮换**密钥和密码
5. **使用密钥管理服务**（如AWS Secrets Manager、Azure Key Vault）存储敏感信息
6. **最小权限原则**配置数据库和服务账号权限

## 验证配置

启动应用后，检查配置是否正确：

```bash
# 检查API健康状态
curl http://localhost:5000/health

# 检查数据库连接
curl http://localhost:5000/health/db

# 检查Redis连接
curl http://localhost:5000/health/redis

# 查看当前环境
curl http://localhost:5000/api/system/info
```

## 故障排查

### 问题1: 数据库连接失败

检查：
- PostgreSQL是否启动
- 连接字符串是否正确
- 用户名密码是否正确
- 防火墙是否阻止连接

### 问题2: Redis连接失败

检查：
- Redis是否启动
- 连接字符串格式是否正确
- 是否需要密码认证

### 问题3: JWT认证失败

检查：
- JWT_SECRET_KEY是否配置
- 密钥长度是否足够（至少32字符）
- Issuer和Audience是否匹配

## 相关文档

- [快速启动指南](QUICK_START.md)
- [部署指南](DEPLOYMENT.md)
- [项目结构](PROJECT_STRUCTURE.md)

