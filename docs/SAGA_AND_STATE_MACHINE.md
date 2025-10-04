# Saga 和状态机文档

## 概述

CatCat.Transit 现在支持两种高级模式：**Saga（长事务编排）** 和 **状态机（State Machine）**。这两种模式为复杂业务流程提供了强大且可靠的解决方案。

---

## ✨ 核心特性

### Saga 模式
- ✅ **分布式事务编排**：支持跨服务的长事务
- ✅ **自动补偿机制**：任何步骤失败时自动回滚已执行步骤
- ✅ **持久化支持**：Saga 状态可持久化（内存/数据库）
- ✅ **乐观锁**：版本控制防止并发冲突
- ✅ **AOT 兼容**：完全支持 Native AOT 编译

### 状态机模式
- ✅ **类型安全的状态转换**：编译时验证状态和事件
- ✅ **生命周期钩子**：OnEnter / OnExit 支持
- ✅ **事件驱动**：基于领域事件的状态转换
- ✅ **线程安全**：所有操作都是线程安全的
- ✅ **AOT 兼容**：完全支持 Native AOT 编译

---

## 🔥 Saga 模式

### 什么是 Saga？

Saga 是一种用于管理分布式事务的模式，将长事务分解为一系列本地事务，每个本地事务更新数据并触发下一个事务。如果某个事务失败，Saga 会执行补偿事务来回滚之前的更改。

### 基本使用

#### 1. 定义 Saga 数据

```csharp
public class OrderSagaData
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool InventoryReserved { get; set; }
    public bool ShipmentScheduled { get; set; }
}
```

#### 2. 创建 Saga

```csharp
public class OrderSaga : SagaBase<OrderSagaData>
{
    public OrderSaga()
    {
        // Correlation ID 自动生成
    }
}
```

#### 3. 实现 Saga 步骤

```csharp
public class ProcessPaymentStep : SagaStepBase<OrderSagaData>
{
    private readonly IPaymentService _paymentService;

    public ProcessPaymentStep(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // 执行步骤
    public override async Task<TransitResult> ExecuteAsync(
        ISaga<OrderSagaData> saga, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _paymentService.ChargeAsync(saga.Data.OrderId, saga.Data.Amount);
            saga.Data.PaymentProcessed = true;
            return TransitResult.Success();
        }
        catch (PaymentException ex)
        {
            return TransitResult.Failure($"Payment failed: {ex.Message}");
        }
    }

    // 补偿步骤（回滚）
    public override async Task<TransitResult> CompensateAsync(
        ISaga<OrderSagaData> saga, 
        CancellationToken cancellationToken = default)
    {
        if (saga.Data.PaymentProcessed)
        {
            await _paymentService.RefundAsync(saga.Data.OrderId, saga.Data.Amount);
            saga.Data.PaymentProcessed = false;
        }
        return TransitResult.Success();
    }
}
```

#### 4. 编排 Saga

```csharp
// 配置 Saga
var orchestrator = new SagaOrchestrator<OrderSagaData>(repository, logger);

orchestrator
    .AddStep(new ProcessPaymentStep(paymentService))
    .AddStep(new ReserveInventoryStep(inventoryService))
    .AddStep(new ScheduleShipmentStep(shippingService));

// 执行 Saga
var saga = new OrderSaga
{
    Data = new OrderSagaData
    {
        OrderId = orderId,
        Amount = 99.99m
    }
};

var result = await orchestrator.ExecuteAsync(saga);

if (result.IsSuccess)
{
    Console.WriteLine($"Order {saga.Data.OrderId} processed successfully!");
}
else
{
    Console.WriteLine($"Order failed and was compensated: {result.Error}");
}
```

### Saga 状态

Saga 在执行过程中会经历以下状态：

```csharp
public enum SagaState
{
    New,          // 新建
    Running,      // 执行中
    Completed,    // 已完成
    Compensating, // 补偿中
    Compensated,  // 已补偿
    Failed        // 失败
}
```

### Saga 持久化

#### 内存存储（开发/测试）

```csharp
services.AddSingleton<ISagaRepository, InMemorySagaRepository>();
```

#### 自定义持久化

实现 `ISagaRepository` 接口以支持数据库持久化：

```csharp
public interface ISagaRepository
{
    Task SaveAsync(ISaga saga, CancellationToken cancellationToken = default);
    Task<ISaga?> GetAsync(Guid correlationId, CancellationToken cancellationToken = default);
    Task<ISaga<TData>?> GetAsync<TData>(Guid correlationId, CancellationToken cancellationToken = default) 
        where TData : class, new();
    Task DeleteAsync(Guid correlationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ISaga>> QueryByStateAsync(SagaState state, CancellationToken cancellationToken = default);
}
```

示例：Entity Framework Core 实现

```csharp
public class EFSagaRepository : ISagaRepository
{
    private readonly DbContext _dbContext;

    public async Task SaveAsync(ISaga saga, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Sagas.FindAsync(saga.CorrelationId);
        if (entity == null)
        {
            _dbContext.Sagas.Add(MapToEntity(saga));
        }
        else
        {
            // 乐观锁检查
            if (entity.Version != saga.Version - 1)
            {
                throw new ConcurrencyException("Saga was modified by another process");
            }
            UpdateEntity(entity, saga);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    // ... 其他方法实现
}
```

### 高级场景

#### 并行步骤

虽然当前实现是顺序执行，但可以轻松扩展以支持并行步骤：

```csharp
orchestrator
    .AddParallelSteps(
        new SendEmailStep(),
        new SendSMSStep(),
        new LogAnalyticsStep()
    )
    .AddStep(new FinalizeOrderStep());
```

#### Saga 超时

```csharp
var saga = new OrderSaga
{
    Data = orderData
};

using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var result = await orchestrator.ExecuteAsync(saga, cts.Token);
```

#### Saga 恢复

```csharp
// 从持久化存储恢复 Saga
var saga = await repository.GetAsync<OrderSagaData>(correlationId);

if (saga != null && saga.State == SagaState.Failed)
{
    // 重试 Saga
    var result = await orchestrator.ExecuteAsync(saga);
}
```

---

## 🔥 状态机模式

### 什么是状态机？

状态机是一种用于建模复杂状态转换的模式。它定义了一组状态、触发状态转换的事件，以及每个状态转换时执行的操作。

### 基本使用

#### 1. 定义状态和数据

```csharp
public enum OrderState
{
    New,
    PaymentPending,
    PaymentConfirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderStateMachineData
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
}
```

#### 2. 定义事件

```csharp
public record OrderPlacedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record PaymentConfirmedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public string TransactionId { get; init; } = string.Empty;
}

public record OrderShippedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public string TrackingNumber { get; init; } = string.Empty;
}
```

#### 3. 实现状态机

```csharp
public class OrderStateMachine : StateMachineBase<OrderState, OrderStateMachineData>
{
    private readonly ILogger<OrderStateMachine> _logger;
    private readonly INotificationService _notificationService;

    public OrderStateMachine(
        ILogger<OrderStateMachine> logger,
        INotificationService notificationService) : base(logger)
    {
        _logger = logger;
        _notificationService = notificationService;
        
        CurrentState = OrderState.New;
        
        ConfigureStateMachine();
    }

    private void ConfigureStateMachine()
    {
        // New -> PaymentPending
        ConfigureTransition<OrderPlacedEvent>(OrderState.New, async (@event) =>
        {
            Data.OrderId = @event.OrderId;
            Data.TotalAmount = @event.Amount;
            _logger.LogInformation("Order {OrderId} placed, awaiting payment", @event.OrderId);
            return OrderState.PaymentPending;
        });

        // PaymentPending -> PaymentConfirmed
        ConfigureTransition<PaymentConfirmedEvent>(OrderState.PaymentPending, async (@event) =>
        {
            Data.PaymentDate = DateTime.UtcNow;
            _logger.LogInformation("Payment confirmed for order {OrderId}", Data.OrderId);
            await _notificationService.SendAsync($"Payment received: {Data.TotalAmount:C}");
            return OrderState.PaymentConfirmed;
        });

        // PaymentConfirmed -> Processing (自动)
        OnEnter(OrderState.PaymentConfirmed, async (data) =>
        {
            await Task.Delay(100); // 模拟处理
            await TransitionToAsync(OrderState.Processing);
        });

        // Processing -> Shipped
        ConfigureTransition<OrderShippedEvent>(OrderState.Processing, async (@event) =>
        {
            Data.ShipmentDate = DateTime.UtcNow;
            _logger.LogInformation("Order {OrderId} shipped with tracking {TrackingNumber}", 
                Data.OrderId, @event.TrackingNumber);
            return OrderState.Shipped;
        });

        // 离开 Shipped 状态时通知用户
        OnExit(OrderState.Shipped, async (data) =>
        {
            await _notificationService.SendAsync("Your order is on the way!");
        });
    }
}
```

#### 4. 使用状态机

```csharp
// 创建状态机实例
var stateMachine = new OrderStateMachine(logger, notificationService);

// 触发事件
var result = await stateMachine.FireAsync(new OrderPlacedEvent
{
    OrderId = Guid.NewGuid(),
    Amount = 129.99m
});

if (result.IsSuccess)
{
    Console.WriteLine($"Current state: {stateMachine.CurrentState}");
    // Output: Current state: PaymentPending
}

// 继续处理
await stateMachine.FireAsync(new PaymentConfirmedEvent
{
    TransactionId = "TXN123456"
});

Console.WriteLine($"Current state: {stateMachine.CurrentState}");
// Output: Current state: Processing (自动转换到 PaymentConfirmed 后触发)

// 发货
await stateMachine.FireAsync(new OrderShippedEvent
{
    TrackingNumber = "TRACK789"
});
```

### 生命周期钩子

状态机支持两种生命周期钩子：

#### OnEnter - 进入状态时执行

```csharp
OnEnter(OrderState.Processing, async (data) =>
{
    _logger.LogInformation("Starting to process order {OrderId}", data.OrderId);
    
    // 启动处理任务
    await _orderProcessor.StartProcessingAsync(data.OrderId);
    
    // 可以触发自动转换
    if (await _orderProcessor.IsReadyToShipAsync(data.OrderId))
    {
        await TransitionToAsync(OrderState.ReadyToShip);
    }
});
```

#### OnExit - 离开状态时执行

```csharp
OnExit(OrderState.PaymentPending, async (data) =>
{
    _logger.LogInformation("Leaving payment pending state for order {OrderId}", data.OrderId);
    
    // 清理资源
    await _paymentService.ReleasePaymentLockAsync(data.OrderId);
});
```

### 错误处理

状态机会自动处理无效转换：

```csharp
var stateMachine = new OrderStateMachine(logger, notificationService);

// 尝试在 New 状态下发货（无效转换）
var result = await stateMachine.FireAsync(new OrderShippedEvent());

if (!result.IsSuccess)
{
    Console.WriteLine(result.Error);
    // Output: "No transition from New on OrderShippedEvent"
}

stateMachine.CurrentState.Should().Be(OrderState.New); // 状态未改变
```

---

## 🚀 性能优化

### Saga 性能优化

1. **批量持久化**
   ```csharp
   // 使用批量保存减少数据库往返
   public class BatchingSagaRepository : ISagaRepository
   {
       private readonly ConcurrentQueue<ISaga> _pendingSaves = new();
       
       public Task SaveAsync(ISaga saga, CancellationToken cancellationToken = default)
       {
           _pendingSaves.Enqueue(saga);
           return Task.CompletedTask;
       }
       
       public async Task FlushAsync()
       {
           // 批量写入数据库
           var batch = new List<ISaga>();
           while (_pendingSaves.TryDequeue(out var saga))
           {
               batch.Add(saga);
           }
           await _dbContext.Sagas.AddRangeAsync(batch);
           await _dbContext.SaveChangesAsync();
       }
   }
   ```

2. **快照优化**
   ```csharp
   // 定期创建 Saga 快照，减少恢复时间
   public class SnapshotSagaRepository : ISagaRepository
   {
       public async Task CreateSnapshotAsync(ISaga saga)
       {
           if (saga.Version % 10 == 0) // 每 10 个版本创建快照
           {
               await _snapshotStore.SaveAsync(saga);
           }
       }
   }
   ```

### 状态机性能优化

1. **无锁状态转换**（已实现）
   - 使用原子操作进行状态转换
   - 避免锁竞争

2. **事件批处理**
   ```csharp
   public async Task<TransitResult> FireManyAsync(IEnumerable<IEvent> events)
   {
       foreach (var @event in events)
       {
           var result = await FireAsync(@event);
           if (!result.IsSuccess)
           {
               return result;
           }
       }
       return TransitResult.Success();
   }
   ```

---

## 📊 对比：Saga vs 状态机

| 特性 | Saga | 状态机 |
|------|------|--------|
| **用途** | 分布式长事务 | 业务流程状态管理 |
| **补偿** | ✅ 内置补偿机制 | ❌ 需要手动实现 |
| **持久化** | ✅ 强持久化需求 | ⚠️ 可选 |
| **复杂度** | 较高 | 较低 |
| **适用场景** | 跨服务事务 | 单一实体状态流转 |
| **性能** | 中等（需持久化） | 高（内存操作） |

### 何时使用 Saga？

- ✅ 跨多个服务的分布式事务
- ✅ 需要可靠的补偿机制
- ✅ 长时间运行的业务流程
- ✅ 需要审计和恢复能力

### 何时使用状态机？

- ✅ 单个聚合根的状态流转
- ✅ 业务规则明确的工作流
- ✅ 需要类型安全的状态转换
- ✅ 事件驱动的领域模型

---

## 🎯 最佳实践

### Saga 最佳实践

1. **保持步骤原子性**
   - 每个 Saga 步骤应该是一个原子操作
   - 避免在一个步骤中做太多事情

2. **实现幂等补偿**
   - 补偿操作应该是幂等的
   - 支持多次执行而不会产生副作用

3. **使用 Correlation ID**
   - 使用 Correlation ID 追踪 Saga 实例
   - 在所有相关操作中传递它

4. **监控和告警**
   - 监控 Saga 执行时间
   - 设置超时告警
   - 记录所有补偿操作

### 状态机最佳实践

1. **状态应该是互斥的**
   - 实体在任何时刻只能处于一个状态
   - 避免状态重叠

2. **明确转换条件**
   - 每个转换都应该有明确的触发条件
   - 使用类型安全的事件

3. **最小化状态数据**
   - 状态数据只保存必要信息
   - 避免在状态中存储过多数据

4. **测试所有转换**
   - 测试有效转换
   - 测试无效转换
   - 测试边界条件

---

## 🔧 与 MassTransit 对比

| 功能 | CatCat.Transit | MassTransit |
|------|----------------|-------------|
| **Saga 支持** | ✅ 基础实现 | ✅ 完整企业级实现 |
| **状态机** | ✅ 内置支持 | ✅ Automatonymous |
| **持久化** | ✅ 可扩展 | ✅ 多种存储 (SQL, MongoDB, Redis) |
| **AOT 兼容** | ✅ 100% | ⚠️ 部分支持 |
| **性能** | ✅ 高性能（简化设计） | ✅ 优秀（功能丰富） |
| **学习曲线** | ✅ 简单 | ⚠️ 较陡峭 |
| **并发处理** | ✅ 基础支持 | ✅ 完整并发控制 |

### CatCat.Transit Saga 优势

- ✅ **简单易用**：API 简洁，学习成本低
- ✅ **100% AOT**：完全支持 Native AOT 编译
- ✅ **轻量级**：无额外依赖，性能优异
- ✅ **灵活扩展**：易于自定义持久化策略

### MassTransit Saga 优势

- ✅ **功能完整**：企业级特性（超时、调度、并发控制）
- ✅ **生态成熟**：大量生产实践和社区支持
- ✅ **多存储支持**：开箱即用的持久化方案
- ✅ **高级模式**：支持并行步骤、子 Saga 等

---

## 📚 示例代码

完整示例代码请参见：

- **Saga**: `tests/CatCat.Transit.Tests/Saga/SagaTests.cs`
- **状态机**: `tests/CatCat.Transit.Tests/StateMachine/StateMachineTests.cs`

---

## 🎓 总结

CatCat.Transit 的 Saga 和状态机实现为分布式系统和复杂业务流程提供了强大、灵活且高性能的解决方案。

- **Saga** 适用于跨服务的分布式事务编排
- **状态机** 适用于单个实体的状态流转
- 两者都完全支持 AOT，性能优异
- 简单易用，易于扩展

选择 CatCat.Transit 或 MassTransit 取决于您的具体需求：
- 追求简单、性能和 AOT → **CatCat.Transit**
- 需要企业级功能和成熟生态 → **MassTransit**

