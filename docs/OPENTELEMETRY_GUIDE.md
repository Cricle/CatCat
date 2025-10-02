# OpenTelemetry 可观察性指南

## 📊 概述

CatCat 项目已集成 **OpenTelemetry**，提供完整的可观察性支持（Traces、Metrics、Logs），并且**完全支持 AOT 编译**。

---

## 🎯 核心功能

### 1. 分布式追踪 (Tracing)

自动追踪以下内容：
- ✅ ASP.NET Core HTTP 请求
- ✅ HTTP Client 调用（Stripe、外部 API）
- ✅ 自定义业务操作

**自动收集的信息：**
- 请求路径、方法、状态码
- 客户端 IP、User-Agent
- 响应时间、内容长度
- 异常信息

### 2. 指标收集 (Metrics)

#### 系统指标
- ASP.NET Core 请求指标（请求数、响应时间、错误率）
- HTTP Client 指标（外部调用统计）
- .NET Runtime 指标（内存、GC、线程池）

#### 业务指标
- **订单指标**
  - `catcat.orders.created` - 订单创建总数
  - `catcat.orders.completed` - 订单完成总数
  - `catcat.orders.cancelled` - 订单取消总数
  - `catcat.orders.amount` - 订单金额分布
  - `catcat.orders.processing_time` - 订单处理时间

- **用户指标**
  - `catcat.users.registrations` - 用户注册总数
  - `catcat.users.logins` - 用户登录总数
  - `catcat.users.login_failures` - 登录失败总数

- **支付指标**
  - `catcat.payments.success` - 支付成功总数
  - `catcat.payments.failed` - 支付失败总数
  - `catcat.payments.amount` - 支付金额分布

- **缓存指标**
  - `catcat.cache.hits` - 缓存命中总数
  - `catcat.cache.misses` - 缓存未命中总数
  - `catcat.cache.operation_duration` - 缓存操作耗时

---

## 🔧 配置

### appsettings.json

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317",
    "UseConsoleExporter": true,
    "ServiceName": "CatCat.API",
    "ServiceVersion": "1.0.0"
  }
}
```

#### 配置项说明

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `OtlpEndpoint` | OTLP Collector 地址 | `http://localhost:4317` |
| `UseConsoleExporter` | 是否输出到控制台（开发用） | `true` |
| `ServiceName` | 服务名称 | `CatCat.API` |
| `ServiceVersion` | 服务版本 | `1.0.0` |

---

## 🚀 部署

### 使用 Docker Compose 部署 OpenTelemetry Collector

创建 `docker-compose.observability.yml`：

```yaml
version: '3.8'

services:
  # OpenTelemetry Collector
  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4317:4317"   # OTLP gRPC receiver
      - "4318:4318"   # OTLP HTTP receiver
      - "8888:8888"   # Prometheus metrics
      - "8889:8889"   # Prometheus exporter metrics
      - "13133:13133" # health_check extension
    networks:
      - catcat-network

  # Jaeger - 分布式追踪 UI
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"  # Jaeger UI
      - "14250:14250"  # gRPC
    environment:
      - COLLECTOR_OTLP_ENABLED=true
    networks:
      - catcat-network

  # Prometheus - 指标存储
  prometheus:
    image: prom/prometheus:latest
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    ports:
      - "9090:9090"
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
    networks:
      - catcat-network

  # Grafana - 可视化仪表板
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3001:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-data:/var/lib/grafana
    networks:
      - catcat-network

networks:
  catcat-network:
    driver: bridge

volumes:
  prometheus-data:
  grafana-data:
```

### OpenTelemetry Collector 配置

创建 `otel-collector-config.yaml`：

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 1s
    send_batch_size: 1024
  memory_limiter:
    check_interval: 1s
    limit_mib: 512

exporters:
  # 导出到 Jaeger
  jaeger:
    endpoint: jaeger:14250
    tls:
      insecure: true

  # 导出到 Prometheus
  prometheus:
    endpoint: "0.0.0.0:8889"

  # 控制台输出（开发调试）
  logging:
    loglevel: debug

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [jaeger, logging]

    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus, logging]
```

### Prometheus 配置

创建 `prometheus.yml`：

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'otel-collector'
    static_configs:
      - targets: ['otel-collector:8889']
```

---

## 📈 使用示例

### 1. 在代码中使用自定义追踪

```csharp
using System.Diagnostics;

public class OrderService
{
    private readonly ActivitySource _activitySource;
    private readonly CustomMetrics _metrics;

    public OrderService(ActivitySource activitySource, CustomMetrics metrics)
    {
        _activitySource = activitySource;
        _metrics = metrics;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        // 创建自定义 Span
        using var activity = _activitySource.StartActivity("CreateOrder");
        activity?.SetTag("order.id", order.Id);
        activity?.SetTag("order.amount", order.Amount);

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // 业务逻辑
            await SaveOrderToDatabase(order);

            stopwatch.Stop();

            // 记录指标
            _metrics.RecordOrderCreated(order.Status, order.ServiceType);
            _metrics.RecordOrderProcessingTime(stopwatch.ElapsedMilliseconds, order.Status);

            return order;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

### 2. 记录业务指标

```csharp
// 注入 CustomMetrics
public class PaymentService
{
    private readonly CustomMetrics _metrics;

    public PaymentService(CustomMetrics metrics)
    {
        _metrics = metrics;
    }

    public async Task ProcessPayment(Payment payment)
    {
        try
        {
            await StripeCharge(payment);

            // 记录支付成功指标
            _metrics.RecordPaymentSuccess(payment.Amount, "stripe");
        }
        catch (Exception ex)
        {
            // 记录支付失败指标
            _metrics.RecordPaymentFailed(ex.Message, "stripe");
            throw;
        }
    }
}
```

---

## 📊 可视化

### 访问 UI

启动 Docker Compose 后，访问以下地址：

| 服务 | 地址 | 用途 |
|------|------|------|
| **Jaeger UI** | http://localhost:16686 | 查看分布式追踪 |
| **Prometheus** | http://localhost:9090 | 查询指标数据 |
| **Grafana** | http://localhost:3001 | 可视化仪表板 |
| **OTLP Collector** | http://localhost:4317 | 接收遥测数据 |

### Grafana 仪表板

1. 登录 Grafana（admin/admin）
2. 添加 Prometheus 数据源：`http://prometheus:9090`
3. 导入预构建仪表板或创建自定义仪表板

**推荐仪表板：**
- ASP.NET Core Dashboard (ID: 10915)
- .NET Runtime Dashboard
- 自定义业务指标仪表板

---

## 🔍 查询示例

### Prometheus 查询

```promql
# 请求速率（每秒请求数）
rate(http_server_request_duration_seconds_count[5m])

# 平均响应时间
rate(http_server_request_duration_seconds_sum[5m]) / rate(http_server_request_duration_seconds_count[5m])

# 错误率
sum(rate(http_server_request_duration_seconds_count{http_status_code=~"5.."}[5m])) / sum(rate(http_server_request_duration_seconds_count[5m]))

# 订单创建速率
rate(catcat_orders_created_total[5m])

# 支付成功率
sum(rate(catcat_payments_success_total[5m])) / (sum(rate(catcat_payments_success_total[5m])) + sum(rate(catcat_payments_failed_total[5m])))

# 缓存命中率
sum(rate(catcat_cache_hits_total[5m])) / (sum(rate(catcat_cache_hits_total[5m])) + sum(rate(catcat_cache_misses_total[5m])))
```

---

## ✅ AOT 支持

OpenTelemetry 在本项目中的配置**完全支持 AOT 编译**：

✅ 使用编译时配置（无反射）
✅ 使用 Source Generator
✅ 无动态代码生成
✅ 无运行时类型检查

**验证 AOT 兼容性：**

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

---

## 🎯 最佳实践

### 1. Span 命名规范
- 使用动词 + 名词：`CreateOrder`、`GetUser`、`ProcessPayment`
- 层次化命名：`Database.Query`、`Cache.Get`、`External.StripeApi`

### 2. 标签（Tags）规范
- 使用小写和下划线：`order_id`、`user_role`
- 避免高基数标签（如用户ID作为标签）
- 使用语义化标签：`http.method`、`http.status_code`

### 3. 指标命名规范
- 使用点分隔：`catcat.orders.created`
- 包含单位：`catcat.orders.processing_time_ms`
- 使用复数：`catcat.users.logins`

### 4. 性能优化
- 使用批处理导出（已配置）
- 设置内存限制（已配置）
- 排除健康检查和 Swagger 端点（已配置）
- 使用采样策略（生产环境）

---

## 🐛 调试

### 查看控制台输出

开发环境下，OpenTelemetry 会输出到控制台：

```json
{
  "OpenTelemetry": {
    "UseConsoleExporter": true
  }
}
```

### 验证 OTLP Collector 连接

```bash
curl http://localhost:13133/health
```

### 查看 Prometheus 目标

访问：http://localhost:9090/targets

---

## 📚 参考资料

- [OpenTelemetry .NET 文档](https://opentelemetry.io/docs/instrumentation/net/)
- [OpenTelemetry Collector 文档](https://opentelemetry.io/docs/collector/)
- [Jaeger 文档](https://www.jaegertracing.io/docs/)
- [Prometheus 文档](https://prometheus.io/docs/)
- [Grafana 文档](https://grafana.com/docs/)

---

**生成时间**：2025-10-02
**项目状态**：✅ OpenTelemetry 已集成（支持 AOT）
**可观察性状态**：🟢 Traces + Metrics 已配置

