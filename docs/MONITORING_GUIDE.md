# CatCat 监控和可观测性指南

> 完整的性能监控、问题诊断和指标收集方案
> 更新时间: 2025-10-03

---

## 📊 监控架构

### 监控技术栈

```
Application (CatCat API)
    ↓
Prometheus (指标收集)
    ↓
Grafana (可视化)
    ↓
OpenTelemetry (分布式追踪)
    ↓
Jaeger (追踪可视化)
```

### 关键组件

| 组件 | 端口 | 用途 | 访问地址 |
|------|------|------|----------|
| **Prometheus** | 9090 | 指标收集和存储 | http://localhost:9090 |
| **Grafana** | 3001 | 指标可视化 | http://localhost:3001 |
| **Jaeger** | 16686 | 分布式追踪 | http://localhost:16686 |
| **API Metrics** | /metrics | Prometheus 端点 | http://localhost:5000/metrics |

---

## 🚀 快速开始

### 1. 启动监控服务

```bash
# 启动所有服务（包括监控）
docker-compose up -d

# 验证服务状态
docker-compose ps
```

### 2. 访问 Grafana

1. 打开浏览器访问: http://localhost:3001
2. 登录凭据:
   - 用户名: `admin`
   - 密码: `catcat_grafana_password`

3. 查看预配置的 Dashboard:
   - **CatCat API Overview** - API 性能概览
   - **CatCat Business Metrics** - 业务指标监控

### 3. 查看 Prometheus 指标

访问 http://localhost:9090 查看原始指标数据和执行 PromQL 查询。

---

## 📈 核心指标说明

### API 性能指标

#### 1. 请求速率
```promql
# 每秒请求数
rate(catcat_api_requests_total[5m])

# 按端点分组
sum by (endpoint) (rate(catcat_api_requests_total[5m]))
```

#### 2. 响应时间
```promql
# P95 响应时间
histogram_quantile(0.95, sum(rate(catcat_api_request_duration_seconds_bucket[5m])) by (le, endpoint))

# P99 响应时间
histogram_quantile(0.99, sum(rate(catcat_api_request_duration_seconds_bucket[5m])) by (le, endpoint))
```

#### 3. 错误率
```promql
# 错误率（4xx + 5xx）
sum(rate(catcat_api_requests_total{status_code=~"4..|5.."}[5m])) / sum(rate(catcat_api_requests_total[5m]))
```

#### 4. 并发请求数
```promql
catcat_api_concurrent_requests
```

---

### 数据库性能指标

#### 1. 查询速率
```promql
# 每秒查询数
sum by (operation, table) (rate(catcat_database_queries_total{status="success"}[5m]))
```

#### 2. 查询延迟
```promql
# P95 查询延迟
histogram_quantile(0.95, sum(rate(catcat_database_query_duration_seconds_bucket[5m])) by (le, operation))
```

#### 3. 慢查询
```promql
# 慢查询速率（>1秒）
rate(catcat_database_slow_queries_total[5m])
```

#### 4. 连接池
```promql
# 活动连接数
catcat_database_active_connections

# 连接错误
rate(catcat_database_connection_errors_total[5m])
```

---

### 缓存性能指标

#### 1. 缓存命中率
```promql
# 总体命中率
sum(catcat_cache_hits_total) / (sum(catcat_cache_hits_total) + sum(catcat_cache_misses_total))

# 按类型分组
sum by (cache_type) (catcat_cache_hits_total) / (sum by (cache_type) (catcat_cache_hits_total) + sum by (cache_type) (catcat_cache_misses_total))
```

#### 2. 缓存操作速率
```promql
# 缓存命中速率
rate(catcat_cache_hits_total[5m])

# 缓存失效速率
rate(catcat_cache_misses_total[5m])

# 缓存逐出速率
rate(catcat_cache_evictions_total[5m])
```

---

### 业务指标

#### 1. 订单指标
```promql
# 活跃订单数
catcat_active_orders

# 订单创建速率
rate(catcat_orders_created_total[5m])

# 订单完成速率
rate(catcat_orders_completed_total[5m])

# 订单取消速率
rate(catcat_orders_cancelled_total[5m])

# 订单处理时长 P95
histogram_quantile(0.95, sum(rate(catcat_order_processing_duration_seconds_bucket[5m])) by (le))
```

#### 2. 用户指标
```promql
# 在线用户数
catcat_online_users

# 用户注册速率
rate(catcat_users_registered_total[5m])

# 用户登录速率
rate(catcat_user_logins_total[5m])
```

#### 3. 支付指标
```promql
# 支付处理速率
sum by (status) (rate(catcat_payments_processed_total[5m]))

# 支付金额分位数
catcat_payment_amount_yuan{quantile="0.5"}  # 中位数
catcat_payment_amount_yuan{quantile="0.95"} # P95
```

#### 4. 文件存储指标
```promql
# 文件上传速率
rate(catcat_files_uploaded_total[5m])

# 上传时长 P95
histogram_quantile(0.95, sum(rate(catcat_file_upload_duration_seconds_bucket[5m])) by (le))

# 文件大小中位数
catcat_file_size_bytes{quantile="0.5"}
```

---

## 🔍 性能问题诊断

### 1. API 响应慢

**症状**: P95 响应时间 > 1秒

**诊断步骤**:

1. 检查 API 响应时间分布:
```promql
histogram_quantile(0.95, sum(rate(catcat_api_request_duration_seconds_bucket[5m])) by (le, endpoint))
```

2. 检查数据库查询延迟:
```promql
histogram_quantile(0.95, sum(rate(catcat_database_query_duration_seconds_bucket[5m])) by (le, operation))
```

3. 检查慢查询:
```promql
rate(catcat_database_slow_queries_total[5m])
```

4. 检查缓存命中率:
```promql
sum(catcat_cache_hits_total) / (sum(catcat_cache_hits_total) + sum(catcat_cache_misses_total))
```

**解决方案**:
- ✅ 优化慢查询（添加索引）
- ✅ 提升缓存命中率
- ✅ 减少数据库查询次数
- ✅ 使用连接池复用

---

### 2. 缓存击穿/雪崩

**症状**: 缓存命中率突然下降，数据库负载激增

**诊断步骤**:

1. 检查缓存命中率趋势:
```promql
sum by (cache_type) (rate(catcat_cache_hits_total[1m])) / (sum by (cache_type) (rate(catcat_cache_hits_total[1m])) + sum by (cache_type) (rate(catcat_cache_misses_total[1m])))
```

2. 检查缓存逐出:
```promql
rate(catcat_cache_evictions_total[5m])
```

3. 检查数据库查询激增:
```promql
sum(rate(catcat_database_queries_total[1m]))
```

**解决方案**:
- ✅ **防击穿**: FusionCache Fail-Safe 模式（已启用）
- ✅ **防雪崩**: 缓存过期时间随机抖动（Jitter，已配置）
- ✅ **防穿透**: Redis Sets Bloom Filter（已集成）
- ✅ **背景刷新**: EagerRefreshThreshold = 0.8（已配置）

---

### 3. 数据库连接池耗尽

**症状**: 大量 503 错误，`catcat_database_active_connections` 达到上限

**诊断步骤**:

1. 检查活动连接数:
```promql
catcat_database_active_connections
```

2. 检查连接错误:
```promql
rate(catcat_database_connection_errors_total[5m])
```

3. 检查并发请求数:
```promql
catcat_api_concurrent_requests
```

**解决方案**:
- ✅ 增加连接池大小（`Database:MaxConcurrency`）
- ✅ 减少慢查询
- ✅ 使用限流保护（Rate Limiting 已启用）
- ✅ 熔断器保护（Polly Circuit Breaker 已集成）

---

### 4. 熔断器打开

**症状**: 某些 API 请求返回 503，熔断器状态 = 1 (Open)

**诊断步骤**:

1. 检查熔断器状态:
```promql
catcat_circuit_breaker_state{name="database"}
```

2. 检查状态变化:
```promql
rate(catcat_circuit_breaker_state_changes_total[5m])
```

**熔断器状态**:
- `0` = Closed (正常)
- `1` = Open (熔断)
- `2` = Half-Open (半开，测试中)

**解决方案**:
- ✅ 检查下游服务（数据库、Redis、MinIO）健康状态
- ✅ 等待自动恢复（30-60秒）
- ✅ 修复根本原因（慢查询、网络问题等）

---

## 🛡️ 防护机制总览

### 1. 缓存防护

| 防护类型 | 实现方式 | 配置位置 |
|---------|---------|----------|
| **防击穿** | FusionCache Fail-Safe | `CacheConfiguration.cs` |
| **防雪崩** | 随机过期时间 (Jitter) | `JitMaxDuration` |
| **防穿透** | Redis Sets Bloom Filter | `BloomFilterService.cs` |
| **背景刷新** | EagerRefreshThreshold | `EagerRefreshThreshold = 0.8` |

### 2. 数据库防护

| 防护类型 | 实现方式 | 配置位置 |
|---------|---------|----------|
| **连接池限制** | DatabaseConcurrencyLimiter | `maxConcurrency = 40` |
| **查询超时** | Polly Timeout Policy | `5秒` |
| **重试策略** | Polly Retry (指数退避) | `3次，100ms起` |
| **熔断器** | Polly Circuit Breaker | `10次失败，熔断30秒` |

### 3. API 防护

| 防护类型 | 实现方式 | 配置位置 |
|---------|---------|----------|
| **限流** | Sliding Window Rate Limiter | `100 req/min` |
| **并发控制** | ConcurrencyLimiter | `1000 concurrent` |
| **熔断器** | Polly Circuit Breaker | 各服务独立配置 |

---

## 📋 监控最佳实践

### 1. 告警阈值建议

| 指标 | 告警阈值 | 严重程度 |
|------|---------|---------|
| API P95 响应时间 | > 1s | Warning |
| API P95 响应时间 | > 3s | Critical |
| 缓存命中率 | < 80% | Warning |
| 缓存命中率 | < 50% | Critical |
| 数据库活动连接 | > 35 (87.5%) | Warning |
| 数据库活动连接 | > 39 (97.5%) | Critical |
| 错误率 (5xx) | > 1% | Warning |
| 错误率 (5xx) | > 5% | Critical |
| 并发请求数 | > 800 | Warning |
| 并发请求数 | > 950 | Critical |

### 2. Dashboard 使用建议

#### CatCat API Overview
- **用途**: 实时监控 API 性能和系统健康状态
- **刷新频率**: 5秒
- **关注指标**: 请求速率、响应时间、缓存命中率、数据库连接

#### CatCat Business Metrics
- **用途**: 监控业务指标和用户行为
- **刷新频率**: 5秒
- **关注指标**: 订单量、用户活跃度、支付成功率

### 3. 定期检查清单

**每日检查**:
- [ ] 查看错误率趋势
- [ ] 检查响应时间 P95/P99
- [ ] 确认缓存命中率 > 80%
- [ ] 验证数据库连接池健康

**每周检查**:
- [ ] 分析慢查询日志
- [ ] 审查熔断器历史记录
- [ ] 检查存储空间使用情况
- [ ] 评估缓存策略有效性

**每月检查**:
- [ ] 性能基线对比
- [ ] 容量规划评估
- [ ] 告警规则优化
- [ ] Dashboard 更新

---

## 🔧 配置调优

### Prometheus 配置

**scrape_interval 调整**:
```yaml
# 高频监控（开发/调试）
scrape_interval: 5s

# 生产环境（节省资源）
scrape_interval: 15s
```

**存储保留期**:
```yaml
# 保留 30 天
--storage.tsdb.retention.time=30d

# 保留 7 天（节省磁盘）
--storage.tsdb.retention.time=7d
```

### Grafana 优化

**自动刷新**:
- 开发环境: 5秒
- 生产环境: 30秒（减少负载）

**数据源查询超时**:
```yaml
# datasources.yml
timeout: 30
```

---

## 📚 相关资源

- **[Prometheus 查询语法](https://prometheus.io/docs/prometheus/latest/querying/basics/)**
- **[Grafana Dashboard 最佳实践](https://grafana.com/docs/grafana/latest/dashboards/)**
- **[OpenTelemetry 指南](OPENTELEMETRY_GUIDE.md)**
- **[限流配置](RATE_LIMITING_GUIDE.md)**

---

**最后更新**: 2025-10-03  
**维护者**: CatCat Team

