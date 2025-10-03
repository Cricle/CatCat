# CatCat 监控快速开始

> 5分钟快速启动完整的监控系统
> 更新时间: 2025-10-03

---

## 🚀 启动监控服务

### 1. 启动所有服务

```bash
# 在项目根目录执行
docker-compose up -d
```

等待所有服务启动（约 30-60 秒）。

### 2. 验证服务状态

```bash
docker-compose ps
```

确保以下服务状态为 `Up (healthy)`:
- ✅ catcat-postgres
- ✅ catcat-redis
- ✅ catcat-nats
- ✅ catcat-minio
- ✅ catcat-api
- ✅ catcat-prometheus
- ✅ catcat-grafana

---

## 📊 访问监控界面

### Grafana (主要监控界面)

**地址**: http://localhost:3001

**登录凭据**:
- 用户名: `admin`
- 密码: `catcat_grafana_password`

**首次登录**:
1. 登录后会自动加载 Prometheus 数据源
2. 点击左侧 **Dashboard** → **Browse**
3. 选择 **CatCat** 文件夹
4. 查看预配置的 Dashboard:
   - **CatCat API Overview** - API 性能监控
   - **CatCat Business Metrics** - 业务指标监控

---

### Prometheus (原始指标查询)

**地址**: http://localhost:9090

**使用方式**:
1. 点击顶部 **Graph**
2. 在查询框输入 PromQL 查询
3. 点击 **Execute**

**示例查询**:
```promql
# API 请求速率
rate(catcat_api_requests_total[5m])

# 缓存命中率
sum(catcat_cache_hits_total) / (sum(catcat_cache_hits_total) + sum(catcat_cache_misses_total))

# 数据库活动连接数
catcat_database_active_connections
```

---

### Jaeger (分布式追踪)

**地址**: http://localhost:16686

**使用方式**:
1. 选择 Service: `CatCat.API`
2. 点击 **Find Traces**
3. 查看请求追踪详情

---

### MinIO Console (对象存储管理)

**地址**: http://localhost:9001

**登录凭据**:
- 用户名: `catcat`
- 密码: `catcat_minio_password_change_in_production`

---

## 📈 核心指标一览

### API 性能

| 指标 | 查询 | 正常范围 |
|------|------|---------|
| **请求速率** | `rate(catcat_api_requests_total[5m])` | 因业务而异 |
| **P95 响应时间** | `histogram_quantile(0.95, ...)` | < 1s |
| **错误率** | `rate(catcat_api_errors_total[5m])` | < 1% |
| **并发请求** | `catcat_api_concurrent_requests` | < 1000 |

### 数据库

| 指标 | 查询 | 正常范围 |
|------|------|---------|
| **查询速率** | `sum(rate(catcat_database_queries_total[5m]))` | 因业务而异 |
| **活动连接** | `catcat_database_active_connections` | < 35 (87.5%) |
| **慢查询** | `rate(catcat_database_slow_queries_total[5m])` | < 0.1/s |

### 缓存

| 指标 | 查询 | 正常范围 |
|------|------|---------|
| **命中率** | `sum(...) / (sum(...) + sum(...))` | > 80% |
| **命中速率** | `rate(catcat_cache_hits_total[5m])` | 因业务而异 |
| **失效速率** | `rate(catcat_cache_misses_total[5m])` | < 命中速率的 20% |

---

## 🛠️ 常见操作

### 查看实时日志

```bash
# API 日志
docker logs -f catcat-api

# Prometheus 日志
docker logs -f catcat-prometheus

# Grafana 日志
docker logs -f catcat-grafana
```

### 重启服务

```bash
# 重启 API（重新加载配置）
docker-compose restart api

# 重启 Prometheus（重新加载配置）
docker-compose restart prometheus

# 重启 Grafana
docker-compose restart grafana
```

### 停止所有服务

```bash
docker-compose down
```

### 清理数据（重置）

```bash
# 停止并删除所有容器和卷
docker-compose down -v

# 重新启动
docker-compose up -d
```

---

## 🔍 问题诊断

### 问题: Grafana 无法连接 Prometheus

**症状**: Dashboard 显示 "No data"

**解决方案**:
1. 检查 Prometheus 是否运行:
   ```bash
   docker-compose ps prometheus
   ```
2. 访问 http://localhost:9090 验证 Prometheus
3. 在 Grafana 中检查数据源配置:
   - URL 应为: `http://prometheus:9090`

---

### 问题: API Metrics 端点无数据

**症状**: Prometheus 无法采集 API 指标

**解决方案**:
1. 访问 http://localhost:5000/metrics 验证端点可访问
2. 检查 Prometheus targets:
   - 访问 http://localhost:9090/targets
   - 确认 `catcat-api` target 状态为 `UP`
3. 检查 API 容器日志:
   ```bash
   docker logs catcat-api
   ```

---

### 问题: Dashboard 无数据

**症状**: 图表显示 "No data"

**可能原因**:
1. **系统刚启动**: 等待 1-2 分钟让指标积累
2. **无请求流量**: 发送一些 API 请求生成数据
3. **时间范围错误**: 调整 Dashboard 右上角时间范围

---

## 📚 下一步

- **[完整监控指南](../docs/MONITORING_GUIDE.md)** - 详细的监控和告警配置
- **[性能调优](../docs/AOT_AND_CLUSTER.md)** - 性能优化建议
- **[缓存策略](../docs/CACHE_OPTIMIZATION_SUMMARY.md)** - 缓存配置详解

---

## 🎯 性能基线

### 首次启动后预期指标

**在无负载情况下**:
- API 并发请求数: 0
- 数据库活动连接: 0-2
- 缓存命中率: N/A (无请求)
- 内存使用: ~500MB (所有容器总和)

**轻度负载 (10 req/s)**:
- P95 响应时间: < 200ms
- 缓存命中率: > 70%
- 数据库活动连接: 2-5
- CPU 使用率: < 30%

**中度负载 (100 req/s)**:
- P95 响应时间: < 500ms
- 缓存命中率: > 85%
- 数据库活动连接: 10-20
- CPU 使用率: < 60%

---

**快速开始完成！🎉**

现在您可以通过 Grafana 实时监控 CatCat 应用的性能和健康状况了。

