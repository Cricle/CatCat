# CatCat 分布式追踪指南

> 基于 OpenTelemetry 的全链路追踪实现
> 更新时间: 2025-10-03

---

## 📊 追踪架构

### 追踪技术栈

```
Application (活动产生)
    ↓
OpenTelemetry SDK (追踪收集)
    ↓
OTLP Exporter (导出追踪数据)
    ↓
Jaeger (追踪存储和可视化)
```

### ActivitySource 层次

```
CatCat.API              # API 层追踪
    ├── HTTP Requests   # ASP.NET Core 自动追踪
    ├── HTTP Clients    # HttpClient 自动追踪
    └── Business Logic  # 手动追踪

CatCat.Infrastructure   # 基础设施层追踪
    ├── Database        # 数据库操作追踪
    ├── Cache           # 缓存操作追踪
    ├── MessageQueue    # 消息队列追踪
    └── External APIs   # 外部服务调用追踪
```

---

## 🚀 快速开始

### 1. 访问 Jaeger UI

**地址**: http://localhost:16686

**搜索追踪**:
1. 选择 Service: `CatCat.API`
2. 选择 Operation (可选): 如 `POST /api/orders`
3. 点击 **Find Traces**

### 2. 分析追踪数据

#### 追踪详情包含:
- **Span Timeline**: 可视化时间线显示各操作耗时
- **Tags**: 操作的元数据（用户ID、数据库表、缓存键等）
- **Events**: 操作过程中的关键事件
- **Logs**: 相关日志信息

---

## 📈 追踪覆盖范围

### 自动追踪（OpenTelemetry 自动仪器）

#### 1. HTTP 请求
```csharp
// 自动追踪所有 ASP.NET Core 请求
// Span 名称: HTTP {method} {route}
// 标签:
//   - http.method
//   - http.route
//   - http.status_code
//   - http.request_id
//   - user.id (如果已认证)
//   - user.role
```

#### 2. HTTP 客户端调用
```csharp
// 自动追踪所有 HttpClient 调用
// Span 名称: HTTP {method}
// 标签:
//   - http.method
//   - http.url
//   - http.status_code
```

### 手动追踪（TracingService）

#### 1. 数据库操作

```csharp
using var activity = _tracing.StartDatabaseActivity("SELECT", "users", "SELECT * FROM users WHERE id = @id");
try
{
    var user = await _repository.GetByIdAsync(userId);
    activity?.SetStatus(ActivityStatusCode.Ok);
    return user;
}
catch (Exception ex)
{
    _tracing.RecordException(ex, activity);
    throw;
}
```

**追踪标签**:
- `db.system`: postgresql
- `db.operation`: SELECT/INSERT/UPDATE/DELETE
- `db.table`: 表名
- `db.statement`: SQL 语句（截断至500字符）
- `db.duration_ms`: 执行时长

#### 2. 缓存操作

```csharp
using var activity = _tracing.StartCacheActivity("GET", $"user:{userId}");
try
{
    var cached = await _cache.GetOrDefaultAsync<User>($"user:{userId}");
    activity?.SetTag("cache.hit", cached != null);
    activity?.SetStatus(ActivityStatusCode.Ok);
    return cached;
}
catch (Exception ex)
{
    _tracing.RecordException(ex, activity);
    throw;
}
```

**追踪标签**:
- `cache.system`: redis
- `cache.operation`: GET/SET/DELETE
- `cache.key`: 缓存键
- `cache.hit`: 是否命中
- `cache.duration_ms`: 操作时长

#### 3. 消息队列操作

```csharp
using var activity = _tracing.StartMessagingActivity("publish", "order.queue", "OrderQueueMessage");
try
{
    await _messageQueue.PublishAsync("order.queue", message);
    activity?.SetTag("messaging.message_id", Guid.NewGuid().ToString());
    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception ex)
{
    _tracing.RecordException(ex, activity);
    throw;
}
```

**追踪标签**:
- `messaging.system`: nats
- `messaging.operation`: publish/consume
- `messaging.destination`: 主题名称
- `messaging.message_type`: 消息类型
- `messaging.message_id`: 消息ID

#### 4. 外部 API 调用

```csharp
using var activity = _tracing.StartExternalApiActivity("Stripe", "CreatePaymentIntent");
try
{
    var result = await _stripeClient.CreatePaymentIntentAsync(request);
    activity?.SetStatus(ActivityStatusCode.Ok);
    return result;
}
catch (Exception ex)
{
    _tracing.RecordException(ex, activity);
    throw;
}
```

**追踪标签**:
- `external.service`: Stripe/MinIO/等
- `external.operation`: 操作名称
- `external.endpoint`: API 端点
- `external.duration_ms`: 调用时长

#### 5. 业务操作

```csharp
using var activity = _tracing.StartBusinessActivity("CreateOrder", "Order", orderId.ToString());
try
{
    // 业务逻辑
    activity?.SetTag("order.amount", amount);
    activity?.SetTag("order.customer_id", customerId);
    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception ex)
{
    _tracing.RecordException(ex, activity);
    throw;
}
```

**追踪标签**:
- `business.entity_type`: Order/User/Pet/等
- `business.entity_id`: 实体ID
- 自定义业务标签

---

## 🔍 使用扩展方法简化追踪

### 数据库操作追踪

```csharp
// 使用扩展方法
var user = await _tracing.ExecuteWithTracingAsync(
    "SELECT",
    "users",
    async () => await _repository.GetByIdAsync(userId),
    query: "SELECT * FROM users WHERE id = @id"
);
```

### 缓存操作追踪

```csharp
var user = await _tracing.ExecuteWithCacheTracingAsync(
    "GET",
    $"user:{userId}",
    async () => await _cache.GetOrDefaultAsync<User>($"user:{userId}")
);
```

### 消息发布追踪

```csharp
await _tracing.PublishWithTracingAsync(
    "order.queue",
    message,
    async () => await _messageQueue.PublishAsync("order.queue", message)
);
```

### 外部 API 调用追踪

```csharp
var result = await _tracing.CallExternalApiWithTracingAsync(
    "Stripe",
    "CreatePaymentIntent",
    async () => await _stripeClient.CreatePaymentIntentAsync(request),
    endpoint: "https://api.stripe.com/v1/payment_intents"
);
```

---

## 📊 追踪示例场景

### 场景 1: 创建订单全链路追踪

```
HTTP POST /api/orders (100ms)
├── Business.CreateOrder (95ms)
│   ├── Cache.GET package:123 (2ms) [HIT]
│   ├── DB.INSERT service_orders (15ms)
│   ├── DB.INSERT order_status_history (8ms)
│   └── MQ.publish order.queue (5ms)
└── HTTP Response 200 OK
```

**追踪信息**:
- 总耗时: 100ms
- 数据库操作: 2次（23ms）
- 缓存操作: 1次命中（2ms）
- 消息发布: 1次（5ms）
- 用户ID: 1001
- 订单ID: 123456789

### 场景 2: 支付流程全链路追踪

```
HTTP POST /api/orders/123/pay (2500ms)
├── DB.SELECT service_orders (10ms)
├── ExternalAPI.Stripe.CreatePaymentIntent (2200ms)
│   └── HTTP POST stripe.com/v1/payment_intents (2195ms)
├── DB.UPDATE service_orders (15ms)
├── DB.INSERT payments (12ms)
└── MQ.publish order.paid (5ms)
```

**追踪信息**:
- 总耗时: 2500ms
- 外部 API 调用: Stripe (2200ms) - 性能瓶颈
- 数据库操作: 3次（37ms）
- 消息发布: 1次（5ms）

---

## 🎯 性能分析

### 识别慢操作

在 Jaeger UI 中:
1. 点击 **Operations** 选项卡
2. 按 **P95 Duration** 排序
3. 查找耗时超过 1 秒的操作

### 识别错误

在 Jaeger UI 中:
1. 使用 Tags 过滤: `error=true`
2. 查看 Span Events 中的异常详情
3. 分析错误堆栈和上下文

### 识别性能瓶颈

查看追踪时间线:
- **长条 Span**: 性能瓶颈
- **并行 Span**: 可优化为异步并行
- **重复 Span**: 可能需要缓存

---

## 🔧 高级用法

### 添加自定义事件

```csharp
_tracing.AddEvent("OrderValidated", new Dictionary<string, object>
{
    ["order_id"] = orderId,
    ["validation_result"] = "success"
});
```

### 添加自定义标签

```csharp
_tracing.SetTag("order.priority", "high");
_tracing.SetTag("customer.vip", true);
```

### 设置 Span 状态

```csharp
// 成功
_tracing.SetStatus(ActivityStatusCode.Ok);

// 错误
_tracing.SetStatus(ActivityStatusCode.Error, "Order validation failed");
```

### 记录异常

```csharp
try
{
    // 操作
}
catch (Exception ex)
{
    _tracing.RecordException(ex);
    throw;
}
```

---

## 📋 最佳实践

### 1. 合理命名 Span

✅ **好的命名**:
- `DB.SELECT users` - 清晰的操作和目标
- `Business.CreateOrder` - 明确的业务操作
- `ExternalAPI.Stripe.CreatePaymentIntent` - 完整的服务和操作

❌ **不好的命名**:
- `Operation1` - 不明确
- `DoWork` - 太泛化
- `Process` - 缺乏上下文

### 2. 添加有价值的标签

✅ **有价值的标签**:
```csharp
activity.SetTag("user.id", userId);
activity.SetTag("order.amount", amount);
activity.SetTag("db.table", "orders");
```

❌ **无用的标签**:
```csharp
activity.SetTag("random_value", Guid.NewGuid());
activity.SetTag("timestamp", DateTime.Now); // 已有时间戳
```

### 3. 控制 Span 粒度

✅ **合适的粒度**:
- 关键业务操作
- 外部服务调用
- 数据库查询
- 缓存操作

❌ **过细的粒度**:
- 单个变量赋值
- 循环内的每次迭代
- 简单的计算操作

### 4. 异常处理

```csharp
using var activity = _tracing.StartDatabaseActivity("INSERT", "orders");
try
{
    await _repository.CreateAsync(order);
    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception ex)
{
    _tracing.RecordException(ex, activity);
    throw; // 重新抛出异常
}
```

---

## 🔍 故障排查

### 问题: 追踪数据未出现在 Jaeger

**检查清单**:
1. Jaeger 服务是否运行:
   ```bash
   docker ps | grep jaeger
   ```

2. OpenTelemetry 配置是否正确:
   ```csharp
   // appsettings.json
   "OpenTelemetry": {
     "OtlpEndpoint": "http://localhost:4317"
   }
   ```

3. ActivitySource 名称是否匹配:
   ```csharp
   // 必须以 "CatCat." 开头
   .AddSource("CatCat.*")
   ```

### 问题: 追踪数据不完整

**可能原因**:
1. **采样率过低**: 检查采样配置
2. **Span 未正确关闭**: 确保使用 `using` 语句
3. **异常未捕获**: Span 可能被提前终止

### 问题: 追踪性能开销过大

**优化建议**:
1. **降低采样率**: 在生产环境中使用 10-20% 采样
2. **减少标签数量**: 只保留关键标签
3. **批量导出**: 配置批量导出减少网络开销

---

## 📊 Jaeger UI 使用技巧

### 搜索技巧

**按服务搜索**:
```
Service: CatCat.API
```

**按操作搜索**:
```
Operation: POST /api/orders
```

**按标签搜索**:
```
Tags: user.id=1001
Tags: http.status_code=500
Tags: error=true
```

**按时间范围搜索**:
- Last Hour
- Last 24 Hours
- Custom Range

### 对比追踪

1. 选择两个追踪
2. 点击 **Compare**
3. 查看差异和性能对比

### 依赖关系图

1. 点击 **System Architecture**
2. 查看服务依赖关系
3. 分析调用模式

---

## 🎯 监控指标

### 关键追踪指标

| 指标 | 说明 | 告警阈值 |
|------|------|---------|
| **P95 Latency** | 95% 请求延迟 | > 1秒 |
| **P99 Latency** | 99% 请求延迟 | > 3秒 |
| **Error Rate** | 错误率 | > 1% |
| **Span Count** | 平均 Span 数量 | > 50 |

### PromQL 查询示例

```promql
# 追踪错误率
sum(rate(traces{status="error"}[5m])) / sum(rate(traces[5m]))

# P95 延迟
histogram_quantile(0.95, sum(rate(trace_duration_bucket[5m])) by (le, operation))
```

---

## 📚 相关文档

- **[OpenTelemetry 官方文档](https://opentelemetry.io/docs/)**
- **[Jaeger 官方文档](https://www.jaegertracing.io/docs/)**
- **[监控指南](MONITORING_GUIDE.md)**
- **[性能优化](AOT_AND_CLUSTER.md)**

---

**最后更新**: 2025-10-03  
**维护者**: CatCat Team

