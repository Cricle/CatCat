# CatGa 模型设计文档

## 🎯 设计理念

**CatGa**（Cat + Saga）是一个全新的轻量级分布式事务处理模型，专为高性能和简单性设计。

### 核心原则

1. **最终一致性**：不追求强一致性，接受短暂的不一致
2. **事件驱动**：基于事件的异步处理
3. **补偿优先**：失败时自动补偿，无需复杂状态管理
4. **内置幂等**：所有操作天然幂等
5. **100% AOT**：完全兼容 Native AOT 编译
6. **极致性能**：无锁设计，非阻塞操作
7. **极简 API**：一个接口搞定所有

---

## 📐 模型架构

### 核心概念

```
CatGa Transaction = Action + Compensation + Metadata

┌─────────────────────────────────────────┐
│           CatGa Transaction             │
├─────────────────────────────────────────┤
│  ┌────────────────────────────────┐    │
│  │  Action (主操作)                │    │
│  │  - ProcessPayment()             │    │
│  │  - ReserveInventory()          │    │
│  │  - SendNotification()          │    │
│  └────────────────────────────────┘    │
│                                         │
│  ┌────────────────────────────────┐    │
│  │  Compensation (补偿操作)        │    │
│  │  - RefundPayment()             │    │
│  │  - ReleaseInventory()          │    │
│  │  - (可选)                       │    │
│  └────────────────────────────────┘    │
│                                         │
│  ┌────────────────────────────────┐    │
│  │  Metadata (元数据)              │    │
│  │  - TransactionId               │    │
│  │  - CorrelationId               │    │
│  │  - Idempotency Key             │    │
│  │  - Retry Policy                │    │
│  │  - Timeout                     │    │
│  └────────────────────────────────┘    │
└─────────────────────────────────────────┘
```

### 执行流程

```
开始
 │
 ├─> 1. 检查幂等性（已处理？返回缓存结果）
 │
 ├─> 2. 执行 Action
 │    │
 │    ├─ 成功 ─> 记录成功状态 ─> 返回结果
 │    │
 │    └─ 失败 ─> 3. 重试（指数退避）
 │              │
 │              ├─ 重试成功 ─> 返回结果
 │              │
 │              └─ 重试失败 ─> 4. 执行 Compensation
 │                            │
 │                            ├─ 补偿成功 ─> 记录失败状态
 │                            │
 │                            └─ 补偿失败 ─> 发送到 DLQ
 │
结束
```

---

## 🚀 核心优势

### 1. 极致性能
- **无状态机**：不维护复杂状态，减少内存开销
- **无锁设计**：所有操作基于 CAS（Compare-And-Swap）
- **并行执行**：独立事务可完全并行
- **零序列化**：内存传输无需序列化

### 2. 极简 API
```csharp
// 只需要一个接口！
public interface ICatGaTransaction<TRequest, TResponse>
{
    // 主操作
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ct);

    // 补偿操作（可选）
    Task CompensateAsync(TRequest request, CancellationToken ct);
}
```

### 3. 100% AOT 兼容
- **无反射**：所有操作编译时确定
- **无动态代码生成**：静态注册
- **类型安全**：编译时验证

### 4. 内置能力
- ✅ **幂等性**：自动去重
- ✅ **重试**：指数退避 + Jitter
- ✅ **补偿**：失败自动补偿
- ✅ **超时**：可配置超时
- ✅ **并发控制**：自动限流
- ✅ **追踪**：分布式追踪

---

## 💡 使用示例

### 基础用法

```csharp
// 1. 定义事务
public class ProcessPaymentTransaction : ICatGaTransaction<PaymentRequest, PaymentResult>
{
    private readonly IPaymentService _paymentService;

    public async Task<PaymentResult> ExecuteAsync(
        PaymentRequest request,
        CancellationToken ct)
    {
        // 执行支付
        return await _paymentService.ProcessAsync(request, ct);
    }

    public async Task CompensateAsync(
        PaymentRequest request,
        CancellationToken ct)
    {
        // 退款
        await _paymentService.RefundAsync(request.OrderId, ct);
    }
}

// 2. 注册事务
services.AddCatGaTransaction<PaymentRequest, PaymentResult, ProcessPaymentTransaction>();

// 3. 执行事务
var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();
var result = await executor.ExecuteAsync<PaymentRequest, PaymentResult>(
    new PaymentRequest { OrderId = orderId, Amount = 99.99m }
);

// 自动处理：幂等性、重试、补偿、追踪！
```

### 链式事务

```csharp
// 定义事务链
var chain = CatGaChain.Create()
    .Then<ProcessPaymentTransaction>()
    .Then<ReserveInventoryTransaction>()
    .Then<SendNotificationTransaction>();

// 执行链（失败自动补偿前面的步骤）
var result = await executor.ExecuteChainAsync(chain, context);
```

### 并行事务

```csharp
// 并行执行多个独立事务
var results = await executor.ExecuteParallelAsync(
    transaction1,
    transaction2,
    transaction3
);

// 任何一个失败，所有成功的都会自动补偿
```

---

## 🏗️ 技术实现

### 核心组件

1. **ICatGaTransaction<TRequest, TResponse>**
   - 事务定义接口
   - 包含 Execute 和 Compensate

2. **ICatGaExecutor**
   - 事务执行器
   - 负责编排、重试、补偿

3. **CatGaContext**
   - 事务上下文
   - 包含 TransactionId、CorrelationId、元数据

4. **CatGaIdempotencyStore**
   - 高性能幂等性存储
   - 基于 Bloom Filter + 分片

5. **CatGaCompensationTracker**
   - 补偿追踪器
   - 记录需要补偿的事务

### 性能优化

```csharp
// 1. 无锁幂等性检查
private readonly ConcurrentDictionary<string, byte>[] _shards;

public bool TryMarkProcessed(string key)
{
    var shard = GetShard(key);
    return shard.TryAdd(key, 0);
}

// 2. 批量补偿
public async Task CompensateAsync(IEnumerable<ICatGaTransaction> transactions)
{
    await Parallel.ForEachAsync(
        transactions,
        new ParallelOptions { MaxDegreeOfParallelism = 10 },
        async (tx, ct) => await tx.CompensateAsync(ct)
    );
}

// 3. 零拷贝传输
public struct CatGaMessage<T> where T : struct
{
    public T Data;
    public ReadOnlyMemory<byte> Metadata;
}
```

---

## 📊 性能指标目标

| 指标 | 目标 | 对比 Saga |
|------|------|-----------|
| **吞吐量** | 200K+ tps | **10x** |
| **延迟 P50** | < 0.5ms | **5x** |
| **延迟 P99** | < 2ms | **10x** |
| **内存占用** | < 5MB/10K 事务 | **20x** |
| **CPU 占用** | < 5% (4 核) | **5x** |

---

## 🎯 与 Saga 模型对比

| 特性 | CatGa | Saga |
|------|-------|------|
| **状态管理** | 无状态 | 有状态机 |
| **性能** | **10x 更快** | 基准 |
| **内存占用** | **20x 更少** | 基准 |
| **复杂度** | **极简** | 较复杂 |
| **学习曲线** | **5 分钟** | 1 小时+ |
| **AOT 支持** | **100%** | 95% |
| **可扩展性** | **极高** | 高 |
| **补偿** | 自动 | 手动定义 |
| **幂等性** | 内置 | 需配置 |
| **重试** | 内置 | 需配置 |

---

## 🔧 配置选项

```csharp
services.AddCatGa(options =>
{
    // 幂等性配置
    options.Idempotency = new()
    {
        Enabled = true,
        ShardCount = 32,
        Expiry = TimeSpan.FromHours(24)
    };

    // 重试配置
    options.Retry = new()
    {
        MaxAttempts = 3,
        InitialDelay = TimeSpan.FromMilliseconds(100),
        MaxDelay = TimeSpan.FromSeconds(10),
        UseJitter = true
    };

    // 补偿配置
    options.Compensation = new()
    {
        AutoCompensate = true,
        CompensationTimeout = TimeSpan.FromSeconds(30),
        ParallelCompensation = true
    };

    // 性能配置
    options.Performance = new()
    {
        MaxConcurrency = 1000,
        QueueSize = 10000,
        UseZeroCopy = true
    };
});
```

---

## 🚦 使用场景

### ✅ 适合 CatGa
- 高并发场景（10K+ tps）
- 短事务（< 5 秒）
- 独立操作较多
- 对性能敏感
- 需要简单 API
- AOT 部署

### ⚠️ 考虑 Saga
- 长事务（> 1 分钟）
- 复杂的状态流转
- 需要审计所有状态
- 人工介入审批

---

## 🔮 未来增强

### 阶段 1（核心）
- [x] 基础事务模型
- [ ] 幂等性支持
- [ ] 自动重试
- [ ] 自动补偿
- [ ] 100% AOT

### 阶段 2（增强）
- [ ] 事务链
- [ ] 并行事务
- [ ] 条件补偿
- [ ] 超时控制

### 阶段 3（优化）
- [ ] Bloom Filter 幂等性
- [ ] 零拷贝传输
- [ ] SIMD 优化
- [ ] 批量处理

---

## 📝 总结

**CatGa 模型的核心思想**：
1. **去状态化**：不维护复杂状态机
2. **事件驱动**：基于事件的异步处理
3. **补偿优先**：失败自动补偿
4. **性能第一**：10x 于传统 Saga
5. **极简 API**：5 分钟上手

**一句话总结**：
> CatGa = 最终一致性 + 自动补偿 + 极致性能 + 极简 API

---

让我们开始实现吧！🚀

