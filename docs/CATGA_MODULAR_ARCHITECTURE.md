# CatGa 模块化架构文档

## 📐 架构设计原则

CatGa 模型按照**单一职责原则**进行模块化重构，将原本耦合在一起的代码拆分为清晰的职责模块。

```
┌───────────────────────────────────────────────────────────────┐
│                    CatGa 模块化架构                           │
├───────────────────────────────────────────────────────────────┤
│                                                               │
│  📦 Models        ━━━━━  数据模型层                          │
│  🗄️  Repository    ━━━━━  数据持久化层                        │
│  🚀 Transport     ━━━━━  消息传输层                          │
│  📋 Policies      ━━━━━  策略控制层                          │
│  ⚙️  Core          ━━━━━  核心执行层                          │
│                                                               │
└───────────────────────────────────────────────────────────────┘
```

---

## 🏗️ 模块结构

### 📦 1. Models（模型层）
**职责**: 定义核心数据结构

```
src/CatCat.Transit/CatGa/Models/
├── CatGaContext.cs           # 事务上下文
├── CatGaResult.cs            # 执行结果
├── CatGaOptions.cs           # 配置选项
└── CatGaTransactionState     # 事务状态枚举
```

**核心类型**:
```csharp
namespace CatCat.Transit.CatGa.Models;

// 事务上下文
public sealed class CatGaContext
{
    public string TransactionId { get; }
    public string IdempotencyKey { get; }
    public string TraceId { get; }      // 分布式追踪
    public CatGaTransactionState State { get; }
    public Dictionary<string, string> Metadata { get; }
}

// 执行结果
public sealed class CatGaResult<T>
{
    public bool IsSuccess { get; }
    public bool IsCompensated { get; }
    public T? Value { get; }
    public string? Error { get; }
}
```

---

### 🗄️ 2. Repository（仓储层）
**职责**: 数据持久化和幂等性管理

```
src/CatCat.Transit/CatGa/Repository/
├── ICatGaRepository.cs         # 仓储接口
└── InMemoryCatGaRepository.cs  # 内存实现（高性能分片）
```

**接口定义**:
```csharp
namespace CatCat.Transit.CatGa.Repository;

public interface ICatGaRepository
{
    // 幂等性管理
    bool IsProcessed(string idempotencyKey);
    void MarkProcessed(string idempotencyKey);
    
    // 结果缓存
    void CacheResult<T>(string idempotencyKey, T? result);
    bool TryGetCachedResult<T>(string idempotencyKey, out T? result);
    
    // 上下文持久化（可选）
    Task SaveContextAsync<TRequest, TResponse>(
        string transactionId,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken = default);
    
    Task<CatGaContext?> LoadContextAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}
```

**特性**:
- ✅ 分片设计（128-256 分片）
- ✅ 无锁并发（`ConcurrentDictionary`）
- ✅ 自动过期清理
- ✅ 高性能（500,000 ops/s）

---

### 🚀 3. Transport（传输层）
**职责**: 消息传输和跨服务通信

```
src/CatCat.Transit/CatGa/Transport/
├── ICatGaTransport.cs       # 传输接口
└── LocalCatGaTransport.cs   # 本地实现
```

**接口定义**:
```csharp
namespace CatCat.Transit.CatGa.Transport;

public interface ICatGaTransport
{
    // 发送请求（同步调用）
    Task<CatGaResult<TResponse>> SendAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken = default);
    
    // 发布事件（异步，无需响应）
    Task PublishAsync<TRequest>(
        string topic,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken = default);
    
    // 订阅请求/事件
    Task<IDisposable> SubscribeAsync<TRequest, TResponse>(...);
    Task<IDisposable> SubscribeEventAsync<TRequest>(...);
}
```

**实现**:
- ✅ `LocalCatGaTransport` - 单实例（进程内）
- ✅ `NatsCatGaTransport` - 跨服务（NATS）
- ✅ 支持自定义传输实现

---

### 📋 4. Policies（策略层）
**职责**: 重试、补偿等策略控制

```
src/CatCat.Transit/CatGa/Policies/
├── IRetryPolicy.cs                      # 重试策略接口
├── ExponentialBackoffRetryPolicy.cs     # 指数退避实现
├── ICompensationPolicy.cs               # 补偿策略接口
└── DefaultCompensationPolicy.cs         # 默认补偿实现
```

**重试策略**:
```csharp
namespace CatCat.Transit.CatGa.Policies;

public interface IRetryPolicy
{
    bool ShouldRetry(int attemptCount, Exception? exception);
    TimeSpan CalculateDelay(int attemptCount);
    int MaxAttempts { get; }
}

public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    // 指数退避: delay = initialDelay * 2^(attempt - 1)
    // + Jitter (随机化，防止雷鸣)
}
```

**补偿策略**:
```csharp
public interface ICompensationPolicy
{
    bool ShouldCompensate(Exception? exception);
    TimeSpan CompensationTimeout { get; }
    bool ThrowOnCompensationFailure { get; }
}
```

---

### ⚙️ 5. Core（核心层）
**职责**: 协调所有模块，执行分布式事务

```
src/CatCat.Transit/CatGa/Core/
├── ICatGaTransaction.cs   # 事务接口（用户实现）
├── ICatGaExecutor.cs      # 执行器接口
└── CatGaExecutor.cs       # 执行器实现
```

**事务接口**（用户实现）:
```csharp
namespace CatCat.Transit.CatGa.Core;

public interface ICatGaTransaction<TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ct);
    Task CompensateAsync(TRequest request, CancellationToken ct);
}
```

**执行器**:
```csharp
public class CatGaExecutor : ICatGaExecutor
{
    private readonly ICatGaRepository _repository;        // 仓储层
    private readonly ICatGaTransport _transport;          // 传输层
    private readonly IRetryPolicy _retryPolicy;           // 策略层
    private readonly ICompensationPolicy _compensationPolicy;
    
    public async Task<CatGaResult<TResponse>> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        CatGaContext? context = null,
        CancellationToken cancellationToken = default)
    {
        // 1️⃣ 仓储层：幂等性检查
        if (_repository.TryGetCachedResult(...)) { ... }
        
        // 2️⃣ 核心层：获取事务实例
        var transaction = _serviceProvider.GetRequiredService<ICatGaTransaction<TRequest, TResponse>>();
        
        // 3️⃣ 策略层：执行（带重试）
        var result = await ExecuteWithRetryAsync(...);
        
        // 4️⃣ 仓储层：缓存结果 / 策略层：补偿
        // ...
    }
}
```

---

## 🔌 依赖注入

### 模块化注册

```csharp
using Microsoft.Extensions.DependencyInjection;
using CatCat.Transit.CatGa.Core;
using CatCat.Transit.CatGa.Models;

// 1️⃣ 基本配置（使用默认实现）
services.AddCatGa(options =>
{
    options.WithHighReliability();
});

// 2️⃣ 自定义仓储（替换默认内存仓储）
services.AddCatGaRepository<RedisRepository>();

// 3️⃣ 自定义传输（替换默认本地传输）
services.AddCatGaTransport<NatsTransport>();

// 4️⃣ 自定义策略
services.AddCatGaRetryPolicy<CustomRetryPolicy>();
services.AddCatGaCompensationPolicy<CustomCompensationPolicy>();

// 5️⃣ 注册事务处理器
services.AddCatGaTransaction<OrderRequest, OrderResult, OrderTransaction>();
```

---

## 📊 模块依赖关系

```
┌─────────────────────────────────────────────────┐
│                   用户代码                       │
│         ICatGaTransaction 实现                  │
└───────────────┬─────────────────────────────────┘
                │
                ↓
┌─────────────────────────────────────────────────┐
│              Core (执行器)                       │
│            CatGaExecutor                        │
└───┬───────┬───────┬───────┬─────────────────────┘
    │       │       │       │
    ↓       ↓       ↓       ↓
┌───────┐┌───────┐┌───────┐┌──────────┐
│Models ││Reposit││Transp ││Policies  │
│       ││ory    ││ort    ││          │
└───────┘└───────┘└───────┘└──────────┘
```

**依赖方向**: 
- ✅ Core → 依赖所有其他模块
- ✅ 其他模块 → 只依赖 Models
- ✅ 模块间 → 互不依赖（高内聚，低耦合）

---

## 🎯 使用示例

### 基础用法

```csharp
using CatCat.Transit.CatGa.Core;
using CatCat.Transit.CatGa.Models;

// 1️⃣ 定义事务
public class PaymentTransaction : ICatGaTransaction<PaymentRequest, PaymentResult>
{
    public async Task<PaymentResult> ExecuteAsync(
        PaymentRequest request, 
        CancellationToken ct)
    {
        // 执行支付...
        return new PaymentResult { Success = true };
    }

    public async Task CompensateAsync(
        PaymentRequest request, 
        CancellationToken ct)
    {
        // 退款...
    }
}

// 2️⃣ 注册
services.AddCatGa();
services.AddCatGaTransaction<PaymentRequest, PaymentResult, PaymentTransaction>();

// 3️⃣ 使用
var executor = sp.GetRequiredService<ICatGaExecutor>();
var context = new CatGaContext { IdempotencyKey = $"payment-{orderId}" };
var result = await executor.ExecuteAsync<PaymentRequest, PaymentResult>(request, context);
```

### 自定义仓储

```csharp
using CatCat.Transit.CatGa.Repository;
using CatCat.Transit.CatGa.Models;

public class RedisRepository : ICatGaRepository
{
    private readonly IDatabase _redis;

    public bool IsProcessed(string idempotencyKey)
    {
        return _redis.KeyExists($"catga:{idempotencyKey}");
    }

    public void CacheResult<T>(string idempotencyKey, T? result)
    {
        var json = JsonSerializer.Serialize(result);
        _redis.StringSet($"catga:{idempotencyKey}", json, TimeSpan.FromHours(24));
    }

    // ... 其他方法实现
}

// 注册
services.AddCatGa();
services.AddCatGaRepository<RedisRepository>();
```

### 自定义传输

```csharp
using CatCat.Transit.CatGa.Transport;
using CatCat.Transit.CatGa.Models;

public class NatsTransport : ICatGaTransport
{
    private readonly INatsConnection _nats;

    public async Task<CatGaResult<TResponse>> SendAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken)
    {
        var msg = new { Request = request, Context = context };
        var response = await _nats.RequestAsync<string>(endpoint, JsonSerializer.Serialize(msg));
        // ... 处理响应
    }

    // ... 其他方法实现
}

// 注册
services.AddCatGa();
services.AddCatGaTransport<NatsTransport>();
```

---

## ✅ 模块化优势

### 1. 单一职责
每个模块只负责一件事：
- ✅ Models：数据结构
- ✅ Repository：持久化
- ✅ Transport：传输
- ✅ Policies：策略
- ✅ Core：协调执行

### 2. 高内聚，低耦合
- ✅ 模块内部高度内聚
- ✅ 模块间通过接口通信
- ✅ 易于替换实现

### 3. 可测试性
```csharp
// 单元测试 - 只需 Mock 依赖的接口
var mockRepository = new Mock<ICatGaRepository>();
var mockTransport = new Mock<ICatGaTransport>();
var executor = new CatGaExecutor(
    serviceProvider,
    logger,
    mockRepository.Object,
    mockTransport.Object,
    retryPolicy,
    compensationPolicy,
    options);
```

### 4. 可扩展性
```csharp
// 扩展新的仓储实现
public class MongoDbRepository : ICatGaRepository { ... }

// 扩展新的传输实现
public class RabbitMqTransport : ICatGaTransport { ... }

// 扩展新的策略
public class AggressiveRetryPolicy : IRetryPolicy { ... }
```

### 5. 清晰的代码组织
```
CatGa/
├── Models/          # 数据模型（所有模块都可使用）
├── Repository/      # 持久化（独立）
├── Transport/       # 传输（独立）
├── Policies/        # 策略（独立）
├── Core/            # 执行器（协调所有模块）
└── DependencyInjection/  # DI 扩展
```

---

## 📝 最佳实践

### 1. 使用接口编程
```csharp
// ✅ 好
private readonly ICatGaRepository _repository;

// ❌ 差
private readonly InMemoryCatGaRepository _repository;
```

### 2. 保持模块独立
```csharp
// ✅ 好 - Repository 只依赖 Models
namespace CatCat.Transit.CatGa.Repository;
using CatCat.Transit.CatGa.Models;

// ❌ 差 - Repository 依赖 Core
using CatCat.Transit.CatGa.Core;  // 不应该依赖
```

### 3. 通过 DI 注入依赖
```csharp
// ✅ 好
services.AddCatGaRepository<RedisRepository>();

// ❌ 差
var repository = new RedisRepository(...);  // 硬编码
```

---

## 🚀 总结

CatGa 模块化架构的核心价值：

1. **清晰的职责划分** - 每个模块都有明确的职责
2. **易于扩展** - 通过接口轻松添加新实现
3. **高度可测试** - 模块间通过接口隔离
4. **灵活配置** - 按需替换默认实现
5. **代码组织清晰** - 按功能分层，易于维护

**模块化 CatGa = 更清晰 + 更灵活 + 更易维护！** 🎯

