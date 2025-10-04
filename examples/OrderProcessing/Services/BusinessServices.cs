using Microsoft.Extensions.Logging;

namespace OrderProcessing;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount);
    Task RefundAsync(Guid orderId, decimal amount);
}

public interface IInventoryService
{
    Task<bool> ReserveInventoryAsync(string productId, int quantity);
    Task ReleaseInventoryAsync(string productId, int quantity);
}

public interface IShippingService
{
    Task<string> ScheduleShipmentAsync(Guid orderId, string productId, int quantity);
    Task CancelShipmentAsync(string trackingNumber);
}

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount)
    {
        _logger.LogInformation("💳 处理支付: {Amount:C} (订单 {OrderId})", amount, orderId);
        await Task.Delay(100);
        return true;
    }

    public async Task RefundAsync(Guid orderId, decimal amount)
    {
        _logger.LogInformation("💰 退款: {Amount:C} (订单 {OrderId})", amount, orderId);
        await Task.Delay(50);
    }
}

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ILogger<InventoryService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ReserveInventoryAsync(string productId, int quantity)
    {
        _logger.LogInformation("📦 预留库存: {ProductId} x {Quantity}", productId, quantity);
        await Task.Delay(100);
        return true;
    }

    public async Task ReleaseInventoryAsync(string productId, int quantity)
    {
        _logger.LogInformation("📤 释放库存: {ProductId} x {Quantity}", productId, quantity);
        await Task.Delay(50);
    }
}

public class ShippingService : IShippingService
{
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(ILogger<ShippingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ScheduleShipmentAsync(Guid orderId, string productId, int quantity)
    {
        var trackingNumber = $"TRACK-{Guid.NewGuid():N}"[..16];
        _logger.LogInformation("🚚 安排发货: {ProductId} x {Quantity}, 快递单号: {TrackingNumber}", 
            productId, quantity, trackingNumber);
        await Task.Delay(100);
        return trackingNumber;
    }

    public async Task CancelShipmentAsync(string trackingNumber)
    {
        _logger.LogInformation("🚫 取消发货: {TrackingNumber}", trackingNumber);
        await Task.Delay(50);
    }
}

