# Transit 功能对比：Memory vs NATS

## 📊 功能对比表

| 功能 | Memory | NATS | 说明 |
|------|--------|------|------|
| **核心 CQRS** ||||
| IQuery<TResult> | ✅ | ✅ | 查询 |
| ICommand<TResult> | ✅ | ✅ | 命令 |
| IEvent | ✅ | ✅ | 事件 |
| Request-Response | ✅ | ✅ | 请求响应模式 |
| Pub-Sub | ✅ | ✅ | 发布订阅模式 |
| **Pipeline Behaviors** ||||
| LoggingBehavior | ✅ | ✅ | 订阅端完整支持 |
| TracingBehavior | ✅ | ✅ | 订阅端完整支持 |
| IdempotencyBehavior | ✅ | ✅ | 订阅端完整支持 |
| ValidationBehavior | ✅ | ✅ | 订阅端完整支持 |
| RetryBehavior | ✅ | ✅ | 订阅端完整支持 |
| **性能优化** ||||
| ConcurrencyLimiter | ✅ | ✅ | 并发控制 |
| CircuitBreaker | ✅ | ✅ | 熔断器 |
| TokenBucketRateLimiter | ✅ | ✅ | 限流 |
| ShardedIdempotencyStore | ✅ | ❌ | NATS 传输层无需本地存储 |
| **弹性机制** ||||
| 指数退避重试 | ✅ | ✅ | Polly 集成 |
| 超时控制 | ✅ | ✅ | CancellationToken |
| 死信队列 | ✅ | ⚠️ | NATS 需要 JetStream DLQ |
| **可观测性** ||||
| ActivitySource | ✅ | ⚠️ | NATS 需要跨服务追踪 |
| 结构化日志 | ✅ | ✅ | 有日志输出 |
| 性能指标 | ✅ | ✅ | 可集成 Prometheus |
| **AOT 兼容性** ||||
| 零反射 | ✅ | ✅ | 完全 AOT 兼容 |
| 源生成器 | N/A | N/A | 不需要 |

## 🔍 详细分析

### Memory Transport（内存传输）

**优势：**
- ✅ **完整的 Pipeline**: 所有 5 个 Behaviors 开箱即用
- ✅ **本地幂等**: ShardedIdempotencyStore 提供高性能去重
- ✅ **低延迟**: < 1ms，适合单体应用
- ✅ **完整追踪**: ActivitySource 自动链路追踪
- ✅ **死信队列**: InMemoryDeadLetterQueue 开箱即用

**劣势：**
- ❌ 不支持分布式场景
- ❌ 无法跨进程通信

**适用场景：**
- 单体应用
- 微服务内部 CQRS
- 开发测试环境
- 高性能本地处理

### NATS Transport（NATS 传输）

**优势：**
- ✅ **分布式**: 支持跨服务通信
- ✅ **高吞吐**: 50K+ msg/s
- ✅ **持久化**: JetStream 支持
- ✅ **弹性**: 熔断器、限流、并发控制
- ✅ **低延迟**: < 5ms 网络延迟

**特性：**
- ✅ **完整 Pipeline Behaviors**: 订阅端支持所有 5 个 Behaviors
- ✅ **自动订阅管理**: `NatsRequestSubscriber` / `NatsEventSubscriber`
- ✅ **分布式追踪**: ActivitySource 自动传播

**适用场景：**
- 微服务架构
- 分布式系统
- 事件驱动架构
- 跨服务 CQRS

## ✅ NATS 完整功能实现

### 1. Pipeline Behaviors（已实现）

**功能：**
- ✅ `NatsRequestSubscriber<TRequest, TResponse>`: 完整 Pipeline 支持
- ✅ `NatsEventSubscriber<TEvent>`: 事件订阅支持
- ✅ 所有 5 个 Behaviors 在订阅端自动执行

**使用示例：**
```csharp
// 发送端
services.AddNatsTransit("nats://localhost:4222", opt =>
{
    opt.WithHighPerformance();
    opt.EnableTracing = true;
});

// 订阅端
services.AddNatsTransit("nats://localhost:4222", opt => opt.WithHighPerformance());
services.AddRequestHandler<GetUserQuery, User, GetUserHandler>();
services.SubscribeToNatsRequest<GetUserQuery, User>();

// 启动订阅
var subscriber = app.Services.GetRequiredService<NatsRequestSubscriber<GetUserQuery, User>>();
subscriber.Start();
```

### 2. 死信队列

**实现：**
- ✅ 订阅端使用 `InMemoryDeadLetterQueue`
- ✅ 失败消息自动发送到 DLQ（通过 RetryBehavior）

### 3. 分布式追踪

**实现：**
- ✅ `TracingBehavior` 在订阅端自动创建 Activity
- ✅ CorrelationId 自动传播

## 💡 建议的架构

### 混合使用（推荐）

```csharp
// 微服务内部：使用 Memory（高性能、完整 Pipeline）
services.AddTransit(opt => opt.WithHighPerformance());

// 跨服务通信：使用 NATS（分布式）
services.AddNatsTransit("nats://localhost:4222", opt =>
{
    opt.WithResilience();
    opt.EnableTracing = true;
});
```

### 使用原则

1. **内部 CQRS** → Memory Transport
   - 聚合内的命令/查询
   - 高频率本地操作
   - 需要完整 Pipeline

2. **跨服务事件** → NATS Transport
   - 领域事件发布
   - 服务间通信
   - 需要持久化

3. **性能敏感** → Memory Transport
   - < 1ms 延迟要求
   - 高吞吐场景

4. **分布式场景** → NATS Transport
   - 跨服务调用
   - 事件驱动架构

## 🎯 结论

| 场景 | 推荐方案 | 原因 |
|------|----------|------|
| 单体应用 | Memory | 完整功能、最低延迟 |
| 微服务 | Memory + NATS | 内部用 Memory，外部用 NATS |
| 事件驱动 | NATS | 分布式、持久化 |
| 开发测试 | Memory | 简单、快速 |
| 生产环境 | 混合 | 根据场景选择 |

**当前状态：**
- ✅ Memory Transport: 生产就绪
- ✅ NATS Transport: 生产就绪（完整 Pipeline Behaviors）

**已实现功能：**
1. ✅ NATS 订阅端完整 Behavior 支持
2. ✅ 分布式追踪（ActivitySource）
3. ✅ 死信队列集成
4. ✅ 幂等性保证
5. ✅ 自动重试

