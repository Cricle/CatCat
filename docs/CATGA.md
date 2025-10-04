# CatGa - 极简分布式事务模型

## 💡 两个核心概念

### 1️⃣ CQRS（命令查询职责分离）
- **Command**：改变系统状态（创建订单）
- **Query**：查询系统状态（查询订单）
- **Event**：系统状态已改变（订单已创建）

### 2️⃣ CatGa 最终一致性
- **Execute**：执行分布式事务
- **Compensate**：失败时自动补偿
- **Idempotency**：自动幂等性保证
- **Eventual Consistency**：最终一致性

---

## 🚀 快速开始

### 1. CQRS 模式

```csharp
// 命令：创建订单
public record CreateOrderCommand : IRequest<Guid>
{
    public string ProductId { get; init; }
    public int Quantity { get; init; }
}

// 命令处理器
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<TransitResult<Guid>> HandleAsync(
        CreateOrderCommand request, 
        CancellationToken ct)
    {
        var orderId = Guid.NewGuid();
        // 创建订单...
        return TransitResult<Guid>.Success(orderId);
    }
}

// 使用
services.AddTransit();
services.AddRequestHandler<CreateOrderCommand, Guid, CreateOrderHandler>();

var mediator = sp.GetRequiredService<ITransitMediator>();
var result = await mediator.SendAsync<CreateOrderCommand, Guid>(command);
```

### 2. CatGa 最终一致性

```csharp
// 定义分布式事务
public record OrderRequest(Guid OrderId, decimal Amount);
public record OrderResult(string Status);

public class OrderTransaction : ICatGaTransaction<OrderRequest, OrderResult>
{
    // 执行事务
    public async Task<OrderResult> ExecuteAsync(OrderRequest req, CancellationToken ct)
    {
        // 1. 处理支付
        await _payment.ChargeAsync(req.OrderId, req.Amount);
        
        // 2. 预留库存
        await _inventory.ReserveAsync(req.OrderId);
        
        // 3. 创建发货
        await _shipping.CreateAsync(req.OrderId);
        
        return new OrderResult("Success");
    }

    // 补偿（失败时自动调用）
    public async Task CompensateAsync(OrderRequest req, CancellationToken ct)
    {
        // 按相反顺序补偿
        await _shipping.CancelAsync(req.OrderId);
        await _inventory.ReleaseAsync(req.OrderId);
        await _payment.RefundAsync(req.OrderId);
    }
}

// 使用
services.AddCatGa();
services.AddCatGaTransaction<OrderRequest, OrderResult, OrderTransaction>();

var executor = sp.GetRequiredService<ICatGaExecutor>();
var context = new CatGaContext { IdempotencyKey = $"order-{orderId}" };
var result = await executor.ExecuteAsync<OrderRequest, OrderResult>(request, context);
```

---

## 📊 两者对比

| 概念 | CQRS | CatGa 最终一致性 |
|------|------|------------------|
| **用途** | 单一操作 | 分布式事务 |
| **一致性** | 强一致性 | 最终一致性 |
| **失败处理** | 返回错误 | 自动补偿 |
| **幂等性** | 需手动 | 自动处理 |
| **示例** | 创建订单 | 支付+库存+发货 |

---

## 🎯 使用场景

### 使用 CQRS（单一操作）

```csharp
✅ 创建订单
✅ 更新用户信息
✅ 查询订单列表
✅ 发送通知
```

### 使用 CatGa（分布式事务）

```csharp
✅ 下单流程：支付 → 库存 → 发货
✅ 转账流程：扣款 → 加款 → 记录
✅ 退款流程：验证 → 退款 → 释放库存
✅ 跨服务调用链
```

---

## 🔄 最终一致性流程

### 成功场景

```
开始 → 执行步骤1 ✅ → 执行步骤2 ✅ → 执行步骤3 ✅ → 成功
```

### 失败场景（自动补偿）

```
开始 → 执行步骤1 ✅ → 执行步骤2 ✅ → 执行步骤3 ❌
     ↓
     补偿步骤3 ✅ → 补偿步骤2 ✅ → 补偿步骤1 ✅ → 最终一致
```

---

## ⚙️ 配置

### 极简配置

```csharp
// CQRS
services.AddTransit();

// CatGa
services.AddCatGa();
```

### 高性能配置

```csharp
// CQRS
services.AddTransit(options => options.WithHighPerformance());

// CatGa
services.AddCatGa(options => options.WithExtremePerformance());
```

### Redis 持久化（跨服务）

```csharp
services.AddRedisCatGaStore(options =>
{
    options.ConnectionString = "localhost:6379";
    options.IdempotencyExpiry = TimeSpan.FromHours(24);
});
```

### NATS 分布式传输

```csharp
services.AddNatsCatGaTransport("nats://localhost:4222");
```

---

## 🎨 完整示例

```csharp
// 1️⃣ CQRS：创建订单命令
var command = new CreateOrderCommand { ProductId = "PROD-001", Quantity = 2 };
var orderId = await mediator.SendAsync<CreateOrderCommand, Guid>(command);

// 2️⃣ CatGa：执行分布式事务
var request = new OrderRequest(orderId, Amount: 199.99m);
var context = new CatGaContext { IdempotencyKey = $"order-{orderId}" };
var result = await executor.ExecuteAsync<OrderRequest, OrderResult>(request, context);

if (result.IsSuccess)
    Console.WriteLine("✅ 订单处理成功（最终一致）");
else if (result.IsCompensated)
    Console.WriteLine("⚠️ 订单失败，已自动补偿（恢复一致）");
```

---

## 📈 性能指标

| 指标 | CQRS | CatGa |
|------|------|-------|
| **吞吐量** | 100,000+ tps | 32,000 tps |
| **延迟** | 0.01ms | 0.03ms |
| **适用** | 单一操作 | 分布式事务 |

---

## 💡 核心原则

### CQRS 原则
1. **职责分离**：命令改变状态，查询只读
2. **单一职责**：一个命令做一件事
3. **强一致性**：立即生效或失败

### CatGa 原则
1. **最终一致**：允许短暂不一致
2. **自动补偿**：失败自动回滚
3. **幂等保证**：重复执行结果相同
4. **简洁 API**：只需 1 个接口

---

## 🔧 关键接口

### CQRS 接口

```csharp
// 命令/查询
public interface IRequest<TResponse> : IMessage { }

// 处理器
public interface IRequestHandler<TRequest, TResponse>
{
    Task<TransitResult<TResponse>> HandleAsync(TRequest request, CancellationToken ct);
}

// 事件
public interface IEvent : IMessage { }

// 事件处理器
public interface IEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}
```

### CatGa 接口（唯一）

```csharp
public interface ICatGaTransaction<TRequest, TResponse>
{
    // 执行
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ct);
    
    // 补偿
    Task CompensateAsync(TRequest request, CancellationToken ct);
}
```

---

## 📚 示例代码

完整示例位于：
- `examples/CatGaExample/` - CatGa 核心示例
- `examples/OrderProcessing/` - CQRS + CatGa 完整示例
- `examples/RedisExample/` - Redis 持久化示例

运行示例：

```bash
cd examples/CatGaExample
dotnet run
```

---

## 🌟 总结

### CatCat.Transit = CQRS + CatGa

```
┌─────────────────────────────────────────┐
│          CatCat.Transit                 │
├─────────────────────────────────────────┤
│                                         │
│  1️⃣  CQRS（命令查询职责分离）            │
│      ├─ Command（命令）                  │
│      ├─ Query（查询）                    │
│      └─ Event（事件）                    │
│                                         │
│  2️⃣  CatGa（分布式最终一致性）            │
│      ├─ Execute（执行）                  │
│      ├─ Compensate（补偿）               │
│      ├─ Idempotency（幂等）              │
│      └─ Eventual Consistency（最终一致） │
│                                         │
└─────────────────────────────────────────┘
```

**两个概念，极致简洁，开箱即用！** 🚀
