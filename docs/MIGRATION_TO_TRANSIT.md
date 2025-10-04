# CatCat.Infrastructure 迁移到 CatCat.Transit

## 迁移概述

CatCat.Infrastructure 已从自定义 CQRS 实现迁移到统一的 `CatCat.Transit` 库。

## 删除的文件

### CQRS 接口
- ❌ `src/CatCat.Infrastructure/CQRS/ICommand.cs`
- ❌ `src/CatCat.Infrastructure/CQRS/ICommandHandler.cs`
- ❌ `src/CatCat.Infrastructure/CQRS/IQuery.cs`
- ❌ `src/CatCat.Infrastructure/CQRS/IQueryHandler.cs`

### 事件接口
- ❌ `src/CatCat.Infrastructure/Events/IDomainEvent.cs`
- ❌ `src/CatCat.Infrastructure/Events/IEventHandler.cs`
- ❌ `src/CatCat.Infrastructure/Events/IEventPublisher.cs`
- ❌ `src/CatCat.Infrastructure/Events/InMemoryEventPublisher.cs`

## 新的依赖

在 `CatCat.Infrastructure.csproj` 中添加了：
```xml
<ProjectReference Include="..\CatCat.Transit\CatCat.Transit.csproj" />
```

## 迁移指南

### 旧代码（使用 Infrastructure.CQRS）

```csharp
using CatCat.Infrastructure.CQRS;
using CatCat.Infrastructure.Events;

// 命令
public record CreateOrderCommand(long UserId, long PackageId) : ICommand<long>;

// 查询
public record GetOrderQuery(long OrderId) : IQuery<Order>;

// 事件
public record OrderCreatedEvent(long OrderId) : DomainEvent;

// 处理器
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, long>
{
    public async Task<Result<long>> HandleAsync(
        CreateOrderCommand command, 
        CancellationToken cancellationToken)
    {
        // ...
        return Result<long>.Success(orderId);
    }
}
```

### 新代码（使用 CatCat.Transit）

```csharp
using CatCat.Transit.Messages;
using CatCat.Transit.Handlers;
using CatCat.Transit.Results;

// 命令（使用 Transit）
public record CreateOrderCommand(long UserId, long PackageId) : ICommand<long>;

// 查询（使用 Transit）
public record GetOrderQuery(long OrderId) : IQuery<Order>;

// 事件（使用 Transit）
public record OrderCreatedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public long OrderId { get; init; }
}

// 处理器（使用 Transit）
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, long>
{
    public async Task<TransitResult<long>> HandleAsync(
        CreateOrderCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            // ... 业务逻辑
            return TransitResult<long>.Success(orderId);
        }
        catch (Exception ex)
        {
            return TransitResult<long>.Failure("Failed to create order", ex);
        }
    }
}
```

## 主要变化

### 1. 命名空间变化

| 旧命名空间 | 新命名空间 |
|-----------|-----------|
| `CatCat.Infrastructure.CQRS` | `CatCat.Transit.Messages` |
| `CatCat.Infrastructure.Events` | `CatCat.Transit.Messages` |
| N/A | `CatCat.Transit.Handlers` |
| N/A | `CatCat.Transit.Results` |

### 2. 接口变化

| 旧接口 | 新接口 |
|-------|-------|
| `ICommand<TResult>` | `ICommand<TResult>` (Transit) |
| `IQuery<TResult>` | `IQuery<TResult>` (Transit) |
| `ICommandHandler<TCommand, TResult>` | `IRequestHandler<TCommand, TResult>` |
| `IQueryHandler<TQuery, TResult>` | `IRequestHandler<TQuery, TResult>` |
| `IDomainEvent` | `IEvent` |
| `IEventHandler<TEvent>` | `IEventHandler<TEvent>` (Transit) |
| `IEventPublisher` | `ITransitMediator` |

### 3. 返回类型变化

| 旧类型 | 新类型 |
|-------|-------|
| `Result<T>` | `TransitResult<T>` |
| `Result` | `TransitResult` |

### 4. 事件变化

旧的 `DomainEvent` 基类：
```csharp
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventId { get; init; } = Guid.NewGuid().ToString();
}
```

新的 `IEvent` 接口（继承自 `IMessage`）：
```csharp
public interface IEvent : IMessage
{
    // MessageId 和 CorrelationId 来自 IMessage
}
```

## 服务注册变化

### 旧方式（Infrastructure）

```csharp
// 没有统一的 Mediator，需要直接注入 Handler
services.AddTransient<ICommandHandler<CreateOrderCommand, long>, CreateOrderHandler>();
```

### 新方式（Transit）

```csharp
// 使用 Transit Mediator
services.AddTransit();

// 显式注册 Handler
services.AddRequestHandler<CreateOrderCommand, long, CreateOrderHandler>();
services.AddEventHandler<OrderCreatedEvent, OrderCreatedEventHandler>();
```

## 额外功能

使用 `CatCat.Transit` 后，自动获得以下功能：

### 1. Pipeline Behaviors
- ✅ **LoggingBehavior** - 自动日志记录
- ✅ **TracingBehavior** - 分布式追踪
- ✅ **IdempotencyBehavior** - 消息去重
- ✅ **ValidationBehavior** - 请求验证
- ✅ **RetryBehavior** - 自动重试

### 2. 弹性机制
- ✅ **并发控制** - ConcurrencyLimiter
- ✅ **熔断器** - CircuitBreaker
- ✅ **限流** - TokenBucketRateLimiter
- ✅ **死信队列** - DeadLetterQueue

### 3. 传输选项
- ✅ **Memory** - 进程内高性能传输
- ✅ **NATS** - 分布式消息传输

## 使用示例

### 发送命令/查询

```csharp
public class OrderController
{
    private readonly ITransitMediator _mediator;

    public OrderController(ITransitMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IResult> CreateOrder(CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request.UserId, request.PackageId);
        
        var result = await _mediator.SendAsync<CreateOrderCommand, long>(command);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
```

### 发布事件

```csharp
public class OrderService
{
    private readonly ITransitMediator _mediator;

    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        // 创建订单...
        var orderId = 123;

        // 发布事件
        await _mediator.PublishAsync(new OrderCreatedEvent { OrderId = orderId });
    }
}
```

## 配置选项

```csharp
// 默认配置
services.AddTransit();

// 高性能配置
services.AddTransit(opt => opt.WithHighPerformance());

// 完整弹性配置
services.AddTransit(opt => opt.WithResilience());

// 自定义配置
services.AddTransit(opt =>
{
    opt.MaxConcurrentRequests = 2000;
    opt.EnableCircuitBreaker = true;
    opt.EnableRateLimiting = true;
    opt.EnableIdempotency = true;
    opt.EnableTracing = true;
});
```

## 迁移检查清单

- [x] 删除 Infrastructure 中的 CQRS 接口
- [x] 删除 Infrastructure 中的 Events 接口
- [x] 添加 CatCat.Transit 项目引用
- [ ] 更新所有使用 CQRS 的代码
- [ ] 更新服务注册
- [ ] 更新单元测试
- [ ] 验证编译
- [ ] 验证运行时行为

## 好处

1. **统一性**: 整个项目使用同一套 CQRS 库
2. **功能丰富**: 自动获得 Pipeline Behaviors 和弹性机制
3. **AOT 兼容**: 100% NativeAOT 支持
4. **高性能**: 无锁设计，非阻塞异步
5. **可观测性**: 内置追踪、日志、死信队列
6. **可扩展性**: 支持 Memory 和 NATS 传输

## 参考文档

- [CatCat.Transit README](../src/CatCat.Transit/README.md)
- [Transit 功能对比](TRANSIT_COMPARISON.md)
- [项目结构](PROJECT_STRUCTURE.md)

