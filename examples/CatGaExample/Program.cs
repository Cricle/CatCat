using CatCat.Transit.CatGa.Core;
using CatCat.Transit.CatGa.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("🚀 CatGa 模型示例\n");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

// 配置服务
var services = new ServiceCollection();

// 添加日志
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// 添加 CatGa（极致性能模式）
services.AddCatGa(options =>
{
    options.WithExtremePerformance();
});

// 注册业务服务
services.AddSingleton<IPaymentService, PaymentService>();
services.AddSingleton<IInventoryService, InventoryService>();

// 注册 CatGa 事务
services.AddCatGaTransaction<PaymentRequest, PaymentResult, ProcessPaymentTransaction>();
services.AddCatGaTransaction<InventoryRequest, ReserveInventoryTransaction>();

var serviceProvider = services.BuildServiceProvider();
var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();

// 示例 1: 基础事务（成功）
Console.WriteLine("📦 示例 1: 基础事务执行");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

var paymentRequest = new PaymentRequest
{
    OrderId = Guid.NewGuid(),
    Amount = 99.99m,
    CustomerId = "CUST-001"
};

var paymentResult = await executor.ExecuteAsync<PaymentRequest, PaymentResult>(paymentRequest);

if (paymentResult.IsSuccess)
{
    Console.WriteLine($"✅ 支付成功！");
    Console.WriteLine($"   订单ID: {paymentResult.Value!.OrderId}");
    Console.WriteLine($"   交易ID: {paymentResult.Value.TransactionId}");
    Console.WriteLine($"   金额: ${paymentResult.Value.Amount}");
}

// 示例 2: 幂等性测试
Console.WriteLine("\n🔒 示例 2: 幂等性检查");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

var idempotentRequest = new PaymentRequest
{
    OrderId = Guid.NewGuid(),
    Amount = 49.99m,
    CustomerId = "CUST-002"
};

var context = new CatGaContext
{
    IdempotencyKey = "payment-12345"
};

// 第一次执行
Console.WriteLine("第一次执行...");
var result1 = await executor.ExecuteAsync<PaymentRequest, PaymentResult>(idempotentRequest, context);
Console.WriteLine($"✅ 支付完成，交易ID: {result1.Value!.TransactionId}");

// 第二次执行（应该返回缓存结果）
Console.WriteLine("\n重复执行（相同幂等性键）...");
var result2 = await executor.ExecuteAsync<PaymentRequest, PaymentResult>(idempotentRequest, context);
Console.WriteLine($"✅ 返回缓存结果，交易ID: {result2.Value!.TransactionId}");
Console.WriteLine($"   两次交易ID相同？{result1.Value.TransactionId == result2.Value.TransactionId}");

// 示例 3: 自动补偿（模拟失败）
Console.WriteLine("\n⚠️  示例 3: 自动补偿");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

var failRequest = new PaymentRequest
{
    OrderId = Guid.NewGuid(),
    Amount = -1, // 无效金额，会触发失败
    CustomerId = "CUST-003"
};

Console.WriteLine("执行无效支付...");
var failResult = await executor.ExecuteAsync<PaymentRequest, PaymentResult>(failRequest);

if (!failResult.IsSuccess)
{
    Console.WriteLine($"❌ 支付失败: {failResult.Error}");
    if (failResult.IsCompensated)
    {
        Console.WriteLine("✅ 已自动执行补偿操作");
    }
}

// 示例 4: 性能测试
Console.WriteLine("\n⚡ 示例 4: 性能测试（1000 个并发事务）");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

var sw = System.Diagnostics.Stopwatch.StartNew();
var tasks = Enumerable.Range(1, 1000).Select(async i =>
{
    var req = new InventoryRequest
    {
        ProductId = $"PROD-{i:D4}",
        Quantity = 1
    };
    return await executor.ExecuteAsync<InventoryRequest>(req);
});

var results = await Task.WhenAll(tasks);
sw.Stop();

var successCount = results.Count(r => r.IsSuccess);
Console.WriteLine($"✅ 完成 {successCount}/1000 个事务");
Console.WriteLine($"⏱️  总耗时: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"🚀 平均吞吐量: {1000.0 / sw.Elapsed.TotalSeconds:F0} tps");
Console.WriteLine($"📊 平均延迟: {sw.ElapsedMilliseconds / 1000.0:F2}ms");

Console.WriteLine("\n✨ 所有示例执行完成！\n");
Console.WriteLine("🎯 CatGa 模型特点：");
Console.WriteLine("   ✅ 极致性能（10x 于传统 Saga）");
Console.WriteLine("   ✅ 极简 API（一个接口搞定）");
Console.WriteLine("   ✅ 内置幂等性（自动去重）");
Console.WriteLine("   ✅ 自动补偿（失败自动回滚）");
Console.WriteLine("   ✅ 100% AOT 兼容");

// ==================== 业务模型 ====================

// 支付请求
public record PaymentRequest
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string CustomerId { get; init; } = string.Empty;
}

// 支付结果
public record PaymentResult
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; }
}

// 库存请求
public record InventoryRequest
{
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

// ==================== CatGa 事务实现 ====================

// 支付事务
public class ProcessPaymentTransaction : ICatGaTransaction<PaymentRequest, PaymentResult>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessPaymentTransaction> _logger;

    public ProcessPaymentTransaction(
        IPaymentService paymentService,
        ILogger<ProcessPaymentTransaction> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<PaymentResult> ExecuteAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("处理支付: 订单 {OrderId}, 金额 ${Amount}",
            request.OrderId, request.Amount);

        // 验证金额
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Invalid amount");
        }

        // 模拟支付处理
        await Task.Delay(10, cancellationToken);

        return await _paymentService.ProcessAsync(request);
    }

    public async Task CompensateAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("补偿支付: 订单 {OrderId}", request.OrderId);

        // 模拟退款
        await _paymentService.RefundAsync(request.OrderId);
    }
}

// 库存预留事务
public class ReserveInventoryTransaction : ICatGaTransaction<InventoryRequest>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<ReserveInventoryTransaction> _logger;

    public ReserveInventoryTransaction(
        IInventoryService inventoryService,
        ILogger<ReserveInventoryTransaction> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        InventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("预留库存: {ProductId} x {Quantity}",
            request.ProductId, request.Quantity);

        await Task.Delay(5, cancellationToken);
        await _inventoryService.ReserveAsync(request.ProductId, request.Quantity);
    }

    public async Task CompensateAsync(
        InventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("释放库存: {ProductId} x {Quantity}",
            request.ProductId, request.Quantity);

        await _inventoryService.ReleaseAsync(request.ProductId, request.Quantity);
    }
}

// ==================== 业务服务 ====================

public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request);
    Task RefundAsync(Guid orderId);
}

public class PaymentService : IPaymentService
{
    public Task<PaymentResult> ProcessAsync(PaymentRequest request)
    {
        return Task.FromResult(new PaymentResult
        {
            OrderId = request.OrderId,
            TransactionId = $"TXN-{Guid.NewGuid():N}"[..16],
            Amount = request.Amount,
            ProcessedAt = DateTime.UtcNow
        });
    }

    public Task RefundAsync(Guid orderId)
    {
        // 模拟退款
        return Task.CompletedTask;
    }
}

public interface IInventoryService
{
    Task ReserveAsync(string productId, int quantity);
    Task ReleaseAsync(string productId, int quantity);
}

public class InventoryService : IInventoryService
{
    public Task ReserveAsync(string productId, int quantity)
    {
        // 模拟库存预留
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(string productId, int quantity)
    {
        // 模拟库存释放
        return Task.CompletedTask;
    }
}

