# OrderProcessing - CatCat.Transit 完整示例

这是一个完整的订单处理示例，展示了 CatCat.Transit 的所有核心功能。

## 🎯 功能演示

### 1. CQRS 基础
- ✅ Command 处理（创建订单）
- ✅ Event 发布（订单创建事件）
- ✅ Event 处理（发送通知）

### 2. Saga 长事务编排
- ✅ 支付处理步骤
- ✅ 库存预留步骤
- ✅ 发货安排步骤
- ✅ 自动补偿机制

### 3. 状态机
- ✅ 订单状态流转
- ✅ 事件驱动转换
- ✅ 生命周期钩子
- ✅ 自动状态转换

### 4. 性能和弹性
- ✅ 并发限流
- ✅ 速率限制
- ✅ 幂等性保证
- ✅ 断路器

## 🚀 运行示例

```bash
cd examples/OrderProcessing
dotnet run
```

## 📖 预期输出

```
🚀 CatCat.Transit - 订单处理示例

📝 示例 1: CQRS 基础用法
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

info: OrderProcessing.Handlers.CreateOrderCommandHandler[0]
      创建订单: PROD-001 x 2
info: OrderProcessing.Handlers.OrderCreatedEventHandler[0]
      📧 发送订单确认邮件: 订单 "12345678-..."
✅ 订单创建成功！订单ID: 12345678-...

📦 示例 2: Saga 长事务编排
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔄 开始执行 Saga (CorrelationId: abcd1234-...)...
info: OrderProcessing.PaymentService[0]
      💳 处理支付: $199.98 (订单 12345678-...)
info: OrderProcessing.InventoryService[0]
      📦 预留库存: PROD-001 x 2
info: OrderProcessing.ShippingService[0]
      🚚 安排发货: PROD-001 x 2, 快递单号: TRACK-...
✅ Saga 执行成功！
   - 支付已处理: True
   - 库存已预留: True
   - 发货已安排: True

🔄 示例 3: 订单状态机
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📋 订单ID: 87654321-...
📊 初始状态: New

➡️  下单 -> 状态: PaymentPending
➡️  支付确认 -> 状态: PaymentConfirmed
➡️  自动处理 -> 状态: Processing
➡️  发货 -> 状态: Shipped

✅ 状态机流转完成！最终状态: Shipped

⚡ 示例 4: 性能和弹性组件
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚡ 发送 10 个并发订单（展示并发限流和速率限制）...

  ✓ 订单 01 完成 - 耗时: 52ms
  ✓ 订单 02 完成 - 耗时: 54ms
  ✓ 订单 03 完成 - 耗时: 56ms
  ...
  ✓ 订单 10 完成 - 耗时: 78ms

✅ 完成 10/10 个订单
   （性能组件正在工作：并发限流、速率限制、幂等性）

✨ 所有示例执行完成！
```

## 📂 项目结构

```
OrderProcessing/
├── Commands/           # 命令定义
│   └── CreateOrderCommand.cs
├── Events/             # 事件定义
│   └── OrderEvents.cs
├── Handlers/           # 处理器实现
│   ├── CommandHandlers.cs
│   └── EventHandlers.cs
├── Sagas/              # Saga 实现
│   └── OrderProcessingSaga.cs
├── StateMachines/      # 状态机实现
│   └── OrderStateMachine.cs
├── Services/           # 业务服务
│   └── BusinessServices.cs
├── Program.cs          # 主程序
└── OrderProcessing.csproj
```

## 🎓 学习要点

### CQRS 模式
- 使用 `IRequest<T>` 定义命令
- 使用 `IRequestHandler<T, TResponse>` 处理命令
- 使用 `IEvent` 定义事件
- 使用 `IEventHandler<T>` 处理事件

### Saga 模式
- 继承 `SagaBase<TData>` 创建 Saga
- 实现 `SagaStepBase<TData>` 创建步骤
- 使用 `SagaOrchestrator` 编排执行流程
- 自动补偿失败的步骤

### 状态机模式
- 继承 `StateMachineBase<TState, TData>` 创建状态机
- 使用 `ConfigureTransition` 配置状态转换
- 使用 `OnEnter/OnExit` 添加生命周期钩子
- 使用 `FireAsync` 触发事件

### 性能和弹性
- `AddTransit` 配置 Transit 选项
- `WithHighPerformance()` 启用高性能模式
- `WithResilience()` 启用弹性组件
- 自动应用并发限流、速率限制等

## 💡 扩展建议

1. **添加验证**：实现 `IValidator<TRequest>` 进行请求验证
2. **添加重试**：配置 `RetryBehavior` 自动重试失败操作
3. **添加追踪**：启用 `TracingBehavior` 进行分布式追踪
4. **持久化 Saga**：实现自定义 `ISagaRepository` 持久化到数据库
5. **NATS 传输**：切换到 NATS 实现分布式消息传递

## 📚 相关文档

- [Saga 和状态机文档](../../docs/SAGA_AND_STATE_MACHINE.md)
- [功能完整清单](../../docs/FINAL_FEATURES.md)
- [与 MassTransit 对比](../../docs/COMPARISON_WITH_MASSTRANSIT.md)

