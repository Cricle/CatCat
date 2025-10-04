using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderProcessing;
using OrderProcessing.Commands;
using OrderProcessing.Events;
using OrderProcessing.Handlers;
using OrderProcessing.Sagas;
using OrderProcessing.StateMachines;
using CatCat.Transit.DependencyInjection;
using CatCat.Transit.Configuration;

Console.WriteLine("🚀 CatCat.Transit - 订单处理示例\n");

// 配置依赖注入
var services = new ServiceCollection();

// 添加日志
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// 添加 Transit（高性能 + 弹性配置）
services.AddTransit(options =>
{
    options.WithHighPerformance()
           .WithResilience();
});

// 注册 Handlers
services.AddRequestHandler<CreateOrderCommand, Guid, CreateOrderCommandHandler>();
services.AddRequestHandler<ProcessPaymentCommand, bool, ProcessPaymentCommandHandler>();
services.AddEventHandler<OrderCreatedEvent, OrderCreatedEventHandler>();
services.AddEventHandler<PaymentProcessedEvent, PaymentProcessedEventHandler>();

// 注册业务服务
services.AddSingleton<IPaymentService, PaymentService>();
services.AddSingleton<IInventoryService, InventoryService>();
services.AddSingleton<IShippingService, ShippingService>();

var serviceProvider = services.BuildServiceProvider();

// 示例 1: CQRS 基础用法
Console.WriteLine("📝 示例 1: CQRS 基础用法");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

var mediator = serviceProvider.GetRequiredService<CatCat.Transit.ITransitMediator>();

var createOrderCommand = new CreateOrderCommand
{
    CustomerId = Guid.NewGuid(),
    ProductId = "PROD-001",
    Quantity = 2,
    Amount = 199.98m
};

var orderResult = await mediator.SendAsync<CreateOrderCommand, Guid>(createOrderCommand);

if (orderResult.IsSuccess)
{
    Console.WriteLine($"✅ 订单创建成功！订单ID: {orderResult.Value}\n");
}

// 示例 2: Saga 编排
Console.WriteLine("📦 示例 2: Saga 长事务编排");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

await RunSagaExample(serviceProvider, orderResult.Value);

// 示例 3: 状态机
Console.WriteLine("\n🔄 示例 3: 订单状态机");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

await RunStateMachineExample(serviceProvider);

// 示例 4: 性能和弹性
Console.WriteLine("\n⚡ 示例 4: 性能和弹性组件");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

await RunPerformanceExample(serviceProvider);

Console.WriteLine("\n✨ 所有示例执行完成！");

// Saga 示例
static async Task RunSagaExample(ServiceProvider serviceProvider, Guid orderId)
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var sagaRepository = serviceProvider.GetRequiredService<CatCat.Transit.Saga.ISagaRepository>();
    var paymentService = serviceProvider.GetRequiredService<IPaymentService>();
    var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
    var shippingService = serviceProvider.GetRequiredService<IShippingService>();

    var orchestrator = new CatCat.Transit.Saga.SagaOrchestrator<OrderSagaData>(
        sagaRepository,
        serviceProvider.GetRequiredService<ILogger<CatCat.Transit.Saga.SagaOrchestrator<OrderSagaData>>>()
    );

    orchestrator
        .AddStep(new ProcessPaymentSagaStep(paymentService))
        .AddStep(new ReserveInventorySagaStep(inventoryService))
        .AddStep(new ScheduleShipmentSagaStep(shippingService));

    var saga = new OrderProcessingSaga
    {
        Data = new OrderSagaData
        {
            OrderId = orderId,
            Amount = 199.98m,
            ProductId = "PROD-001",
            Quantity = 2
        }
    };

    Console.WriteLine($"🔄 开始执行 Saga (CorrelationId: {saga.CorrelationId})...");
    var result = await orchestrator.ExecuteAsync(saga);

    if (result.IsSuccess)
    {
        Console.WriteLine($"✅ Saga 执行成功！");
        Console.WriteLine($"   - 支付已处理: {saga.Data.PaymentProcessed}");
        Console.WriteLine($"   - 库存已预留: {saga.Data.InventoryReserved}");
        Console.WriteLine($"   - 发货已安排: {saga.Data.ShipmentScheduled}");
    }
    else
    {
        Console.WriteLine($"❌ Saga 执行失败并已补偿: {result.Error}");
    }
}

// 状态机示例
static async Task RunStateMachineExample(ServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILogger<OrderStateMachine>>();
    var stateMachine = new OrderStateMachine(logger);

    var orderId = Guid.NewGuid();
    Console.WriteLine($"📋 订单ID: {orderId}");
    Console.WriteLine($"📊 初始状态: {stateMachine.CurrentState}\n");

    // 下单
    var result1 = await stateMachine.FireAsync(new OrderPlacedEvent
    {
        OrderId = orderId,
        Amount = 99.99m
    });
    Console.WriteLine($"➡️  下单 -> 状态: {stateMachine.CurrentState}");

    // 确认支付
    var result2 = await stateMachine.FireAsync(new PaymentConfirmedEvent
    {
        TransactionId = "TXN-" + Guid.NewGuid().ToString("N")[..8]
    });
    Console.WriteLine($"➡️  支付确认 -> 状态: {stateMachine.CurrentState}");

    // 等待自动转换到 Processing
    await Task.Delay(200);
    Console.WriteLine($"➡️  自动处理 -> 状态: {stateMachine.CurrentState}");

    // 发货
    var result3 = await stateMachine.FireAsync(new OrderShippedEvent
    {
        TrackingNumber = "TRACK-" + Guid.NewGuid().ToString("N")[..8]
    });
    Console.WriteLine($"➡️  发货 -> 状态: {stateMachine.CurrentState}");

    Console.WriteLine($"\n✅ 状态机流转完成！最终状态: {stateMachine.CurrentState}");
}

// 性能示例
static async Task RunPerformanceExample(ServiceProvider serviceProvider)
{
    var mediator = serviceProvider.GetRequiredService<CatCat.Transit.ITransitMediator>();

    Console.WriteLine("⚡ 发送 10 个并发订单（展示并发限流和速率限制）...\n");

    var tasks = Enumerable.Range(1, 10).Select(async i =>
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            ProductId = $"PROD-{i:000}",
            Quantity = 1,
            Amount = 19.99m
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await mediator.SendAsync<CreateOrderCommand, Guid>(command);
        sw.Stop();

        if (result.IsSuccess)
        {
            Console.WriteLine($"  ✓ 订单 {i:00} 完成 - 耗时: {sw.ElapsedMilliseconds}ms");
        }

        return result;
    });

    var results = await Task.WhenAll(tasks);
    var successCount = results.Count(r => r.IsSuccess);

    Console.WriteLine($"\n✅ 完成 {successCount}/10 个订单");
    Console.WriteLine("   （性能组件正在工作：并发限流、速率限制、幂等性）");
}

