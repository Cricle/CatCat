# CatCat 部署指南

本文档详细说明如何在不同环境中部署CatCat应用。

## 目录

1. [单机Docker部署](#单机docker部署)
2. [Kubernetes集群部署](#kubernetes集群部署)
3. [生产环境配置](#生产环境配置)
4. [性能优化](#性能优化)
5. [监控和日志](#监控和日志)
6. [备份和恢复](#备份和恢复)

---

## 单机Docker部署

适用于小型应用、开发测试环境。

### 1. 准备工作

```bash
# 克隆代码
git clone https://github.com/your-username/CatCat.git
cd CatCat

# 复制环境变量文件
cp .env.example .env

# 修改配置（数据库密码、JWT密钥等）
vi .env
```

### 2. 启动服务

```bash
# 构建并启动所有服务
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看日志
docker-compose logs -f
```

### 3. 验证部署

```bash
# 测试API
curl http://localhost:5000/health

# 访问前端
open http://localhost
```

### 4. 更新部署

```bash
# 拉取最新代码
git pull

# 重新构建并启动
docker-compose up -d --build
```

---

## Kubernetes集群部署

适用于生产环境、需要高可用和自动扩展的场景。

### 1. 准备K8s集群

确保你有可用的Kubernetes集群（阿里云ACK、腾讯云TKE、自建等）。

```bash
# 验证集群连接
kubectl cluster-info
kubectl get nodes
```

### 2. 创建命名空间和配置

```bash
# 创建命名空间
kubectl create namespace catcat

# 创建Secret（存储敏感信息）
kubectl create secret generic catcat-secrets \
  --from-literal=db-password='your-db-password' \
  --from-literal=jwt-secret='your-jwt-secret' \
  --from-literal=redis-password='your-redis-password' \
  -n catcat

# 创建ConfigMap
kubectl create configmap catcat-config \
  --from-literal=ASPNETCORE_ENVIRONMENT='Production' \
  -n catcat
```

### 3. 部署PostgreSQL (推荐使用云数据库)

```bash
# 如果使用自建PostgreSQL
kubectl apply -f deploy/kubernetes/postgres.yml
```

或使用云数据库（推荐）：
- 阿里云RDS for PostgreSQL
- 腾讯云云数据库PostgreSQL
- AWS RDS

### 4. 部署Redis和NATS

```bash
# Redis
kubectl apply -f deploy/kubernetes/redis.yml

# NATS
kubectl apply -f deploy/kubernetes/nats.yml
```

### 5. 部署API服务

```bash
# 修改镜像地址
vi deploy/kubernetes/deployment.yml

# 应用配置
kubectl apply -f deploy/kubernetes/deployment.yml

# 查看部署状态
kubectl get deployments -n catcat
kubectl get pods -n catcat
```

### 6. 部署Web前端

```bash
kubectl apply -f deploy/kubernetes/web-deployment.yml

# 查看Service
kubectl get svc -n catcat
```

### 7. 配置Ingress（可选）

```bash
# 安装Nginx Ingress Controller（如未安装）
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# 应用Ingress配置
kubectl apply -f deploy/kubernetes/ingress.yml
```

### 8. 配置域名

```bash
# 获取Ingress IP
kubectl get ingress -n catcat

# 配置DNS记录
# A记录: catcat.example.com -> Ingress IP
# A记录: api.catcat.example.com -> Ingress IP
```

---

## 生产环境配置

### 1. 安全配置

#### 修改默认密码和密钥

```bash
# 生成安全的JWT密钥
openssl rand -base64 32

# 生成密码盐
openssl rand -base64 16
```

在 `appsettings.Production.json` 中更新：

```json
{
  "JwtSettings": {
    "SecretKey": "your-generated-secret-key",
    "Issuer": "CatCat",
    "Audience": "CatCat.Client"
  },
  "Security": {
    "PasswordSalt": "your-generated-salt"
  }
}
```

#### 配置HTTPS

```yaml
# Ingress配置示例
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: catcat-ingress
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - catcat.example.com
    secretName: catcat-tls
  rules:
  - host: catcat.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: catcat-web-service
            port:
              number: 80
```

### 2. 数据库配置

#### 主从复制

```sql
-- 主库配置
ALTER SYSTEM SET wal_level = replica;
ALTER SYSTEM SET max_wal_senders = 10;
ALTER SYSTEM SET max_replication_slots = 10;

-- 创建复制用户
CREATE ROLE replicator WITH REPLICATION LOGIN PASSWORD 'repl_password';
```

#### 备份策略

```bash
# 每日自动备份
0 2 * * * /usr/local/bin/pg_dump -h localhost -U postgres catcat | gzip > /backup/catcat_$(date +\%Y\%m\%d).sql.gz

# 保留最近30天的备份
find /backup -name "catcat_*.sql.gz" -mtime +30 -delete
```

### 3. Redis配置

#### 持久化配置

```conf
# redis.conf
appendonly yes
appendfsync everysec
save 900 1
save 300 10
save 60 10000
```

#### 集群模式（高可用）

使用Redis Sentinel或Redis Cluster实现高可用。

### 4. 应用配置

#### 连接池优化

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Port=5432;Database=catcat;Username=postgres;Password=***;Maximum Pool Size=100;Minimum Pool Size=10;"
  }
}
```

#### 日志配置

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/logs/catcat-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

---

## 性能优化

### 1. 数据库优化

```sql
-- 创建索引
CREATE INDEX CONCURRENTLY idx_orders_customer_status ON service_orders(customer_id, status);
CREATE INDEX CONCURRENTLY idx_orders_provider_status ON service_orders(service_provider_id, status);
CREATE INDEX CONCURRENTLY idx_orders_date ON service_orders(service_date);

-- 定期vacuum
VACUUM ANALYZE;

-- 更新统计信息
ANALYZE;
```

### 2. Redis缓存策略

```csharp
// 热点数据缓存
public async Task<ServicePackage> GetServicePackageAsync(long id)
{
    var cacheKey = $"package:{id}";
    var cached = await _cache.GetAsync<ServicePackage>(cacheKey);
    if (cached != null) return cached;

    var package = await _repository.GetByIdAsync(id);
    await _cache.SetAsync(cacheKey, package, TimeSpan.FromHours(1));
    return package;
}
```

### 3. AOT编译优化

```bash
# 构建AOT版本
dotnet publish -c Release -r linux-x64 -p:PublishAot=true

# 减小镜像大小
# 使用多阶段构建，仅包含运行时所需文件
```

### 4. CDN配置

将静态资源（图片、CSS、JS）部署到CDN：
- 阿里云OSS + CDN
- 腾讯云COS + CDN
- CloudFlare

---

## 监控和日志

### 1. Prometheus监控

```yaml
# prometheus-config.yml
scrape_configs:
  - job_name: 'catcat-api'
    static_configs:
      - targets: ['catcat-api:8080']
    metrics_path: '/metrics'
```

### 2. Grafana仪表盘

导入预配置的仪表盘监控以下指标：
- API请求数
- 响应时间
- 错误率
- 数据库连接数
- Redis命中率

### 3. 日志聚合

使用ELK Stack或Loki收集和分析日志：

```yaml
# Fluentd配置
<source>
  @type tail
  path /var/log/containers/catcat-*.log
  pos_file /var/log/fluentd-containers.log.pos
  tag kubernetes.*
  format json
</source>

<match kubernetes.**>
  @type elasticsearch
  host elasticsearch
  port 9200
  logstash_format true
</match>
```

### 4. 告警配置

```yaml
# alertmanager配置
route:
  receiver: 'team-notifications'

receivers:
- name: 'team-notifications'
  webhook_configs:
  - url: 'https://your-webhook-url'
```

---

## 备份和恢复

### 1. 数据库备份

```bash
# 全量备份
pg_dump -h localhost -U postgres -Fc catcat > catcat_backup.dump

# 备份到远程存储
pg_dump -h localhost -U postgres catcat | \
  gzip | \
  aws s3 cp - s3://your-bucket/backups/catcat_$(date +%Y%m%d).sql.gz
```

### 2. 数据库恢复

```bash
# 从备份文件恢复
pg_restore -h localhost -U postgres -d catcat catcat_backup.dump

# 从S3恢复
aws s3 cp s3://your-bucket/backups/catcat_20251002.sql.gz - | \
  gunzip | \
  psql -h localhost -U postgres -d catcat
```

### 3. 灾难恢复计划

1. **定期演练恢复流程**
2. **多地域备份**
3. **RPO/RTO目标定义**
   - RPO (Recovery Point Objective): 1小时
   - RTO (Recovery Time Objective): 2小时

---

## 常见问题

### Q1: Pod一直处于CrashLoopBackOff状态
```bash
# 查看日志
kubectl logs <pod-name> -n catcat

# 查看事件
kubectl describe pod <pod-name> -n catcat
```

### Q2: 数据库连接数耗尽
- 增加 `max_connections` 配置
- 优化应用连接池设置
- 检查是否有连接泄漏

### Q3: API响应慢
- 检查数据库查询性能
- 增加Redis缓存
- 优化N+1查询问题
- 考虑水平扩展

---

## 安全检查清单

- [ ] 修改所有默认密码
- [ ] 启用HTTPS
- [ ] 配置防火墙规则
- [ ] 启用数据库加密
- [ ] 配置定期备份
- [ ] 设置日志审计
- [ ] 限制API访问频率
- [ ] 配置安全头部（CSP、HSTS等）
- [ ] 定期更新依赖包
- [ ] 进行渗透测试

---

## 扩展阅读

- [Kubernetes官方文档](https://kubernetes.io/docs/)
- [PostgreSQL性能优化](https://wiki.postgresql.org/wiki/Performance_Optimization)
- [Redis最佳实践](https://redis.io/topics/best-practices)
- [ASP.NET Core部署](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)

