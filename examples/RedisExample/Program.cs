using CatCat.Transit.Configuration;
using CatCat.Transit.DependencyInjection;
using CatCat.Transit.Saga;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("🚀 CatCat.Transit.Redis 示例\n");
Console.WriteLine("📝 注意：此示例需要 Redis 运行在 localhost:6379");
Console.WriteLine("   启动 Redis: docker run -d -p 6379:6379 redis:latest\n");

try
{
    // 配置服务
    var services = new ServiceCollection();

    // 添加日志
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });

    // 添加 Transit
    services.AddTransit(options =>
    {
        options.WithHighPerformance();
    });

    // 添加 Redis 持久化
    services.AddRedisTransit(options =>
    {
        options.ConnectionString = "localhost:6379";
        options.SagaExpiry = TimeSpan.FromDays(7);
        options.IdempotencyExpiry = TimeSpan.FromHours(24);
        options.SagaKeyPrefix = "example:saga:";
        options.IdempotencyKeyPrefix = "example:idempotency:";
    });

    var serviceProvider = services.BuildServiceProvider();

    // 示例 1: Saga 持久化
    Console.WriteLine("📦 示例 1: Saga Redis 持久化");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    await RunSagaPersistenceExample(serviceProvider);

    // 示例 2: 幂等性
    Console.WriteLine("\n🔒 示例 2: Redis 幂等性检查");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    await RunIdempotencyExample(serviceProvider);

    Console.WriteLine("\n✨ 所有示例执行完成！");
    Console.WriteLine("\n💡 提示：可以使用 Redis CLI 查看存储的数据");
    Console.WriteLine("   redis-cli KEYS \"example:*\"");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ 错误: {ex.Message}");
    Console.WriteLine("\n请确保 Redis 正在运行：");
    Console.WriteLine("  docker run -d -p 6379:6379 redis:latest");
}

static async Task RunSagaPersistenceExample(ServiceProvider serviceProvider)
{
    var repository = serviceProvider.GetRequiredService<ISagaRepository>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    // 创建 Saga
    var saga = new TestSaga
    {
        Data = new TestSagaData
        {
            OrderId = Guid.NewGuid(),
            Amount = 99.99m,
            Step = "初始化"
        }
    };

    Console.WriteLine($"📝 创建 Saga: {saga.CorrelationId}");
    saga.State = SagaState.Running;
    await repository.SaveAsync(saga);
    Console.WriteLine($"   ✓ Saga 已保存到 Redis");

    // 更新 Saga
    saga.Data.Step = "处理中";
    saga.Version++;
    await repository.SaveAsync(saga);
    Console.WriteLine($"   ✓ Saga 已更新 (版本 {saga.Version})");

    // 恢复 Saga
    var recovered = await repository.GetAsync<TestSagaData>(saga.CorrelationId);
    if (recovered != null)
    {
        Console.WriteLine($"\n📖 从 Redis 恢复 Saga:");
        Console.WriteLine($"   - CorrelationId: {recovered.CorrelationId}");
        Console.WriteLine($"   - State: {recovered.State}");
        Console.WriteLine($"   - Version: {recovered.Version}");
        Console.WriteLine($"   - Data.Step: {recovered.Data.Step}");
        Console.WriteLine($"   - Data.Amount: ${recovered.Data.Amount}");
    }

    // 完成 Saga
    saga.State = SagaState.Completed;
    saga.Version++;
    await repository.SaveAsync(saga);
    Console.WriteLine($"\n✅ Saga 已完成并持久化到 Redis");

    // 清理
    await repository.DeleteAsync(saga.CorrelationId);
    Console.WriteLine($"🗑️  Saga 已从 Redis 删除");
}

static async Task RunIdempotencyExample(ServiceProvider serviceProvider)
{
    var idempotencyStore = serviceProvider.GetRequiredService<CatCat.Transit.Idempotency.IIdempotencyStore>();

    var messageId = Guid.NewGuid().ToString();
    var result = "订单创建成功";

    // 首次处理
    var isProcessed = await idempotencyStore.HasBeenProcessedAsync(messageId);
    Console.WriteLine($"📧 消息 {messageId[..8]}...");
    Console.WriteLine($"   已处理？{isProcessed}");

    // 标记为已处理
    await idempotencyStore.MarkAsProcessedAsync(messageId, result);
    Console.WriteLine($"   ✓ 已标记为已处理，结果已缓存");

    // 再次检查
    isProcessed = await idempotencyStore.HasBeenProcessedAsync(messageId);
    Console.WriteLine($"\n🔄 重复消息 {messageId[..8]}...");
    Console.WriteLine($"   已处理？{isProcessed}");

    // 获取缓存的结果
    var cachedResult = await idempotencyStore.GetCachedResultAsync<string>(messageId);
    Console.WriteLine($"   📦 缓存结果: {cachedResult}");
    Console.WriteLine($"   ✅ 幂等性检查通过，返回缓存结果");
}

// 测试 Saga
public class TestSaga : SagaBase<TestSagaData>
{
}

public class TestSagaData
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Step { get; set; } = string.Empty;
}

