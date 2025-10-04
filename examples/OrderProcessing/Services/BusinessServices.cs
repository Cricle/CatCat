namespace OrderProcessing.Services;

// Payment Service
public interface IPaymentService
{
    Task<string> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken);
    Task RefundPaymentAsync(Guid orderId, CancellationToken cancellationToken);
}

public class PaymentService : IPaymentService
{
    public async Task<string> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Invalid amount");
        }

        await Task.Delay(10, cancellationToken); // 模拟网络延迟
        var paymentId = $"PAY-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
        Console.WriteLine($"  ✅ 支付处理完成: {paymentId}, ${amount}");
        return paymentId;
    }

    public async Task RefundPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        Console.WriteLine($"  ↩️  退款处理完成: OrderId={orderId}");
    }
}

// Inventory Service
public interface IInventoryService
{
    Task ReserveInventoryAsync(string productId, int quantity, CancellationToken cancellationToken);
    Task ReleaseInventoryAsync(string productId, int quantity, CancellationToken cancellationToken);
}

public class InventoryService : IInventoryService
{
    public async Task ReserveInventoryAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        Console.WriteLine($"  ✅ 库存预留完成: ProductId={productId}, Quantity={quantity}");
    }

    public async Task ReleaseInventoryAsync(string productId, int quantity, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        Console.WriteLine($"  ↩️  库存释放完成: ProductId={productId}, Quantity={quantity}");
    }
}

// Shipping Service
public interface IShippingService
{
    Task<string> CreateShipmentAsync(Guid orderId, string address, CancellationToken cancellationToken);
    Task CancelShipmentAsync(Guid orderId, CancellationToken cancellationToken);
}

public class ShippingService : IShippingService
{
    public async Task<string> CreateShipmentAsync(Guid orderId, string address, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        var shipmentId = $"SHIP-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
        Console.WriteLine($"  ✅ 发货单创建完成: {shipmentId}, Address={address}");
        return shipmentId;
    }

    public async Task CancelShipmentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        Console.WriteLine($"  ↩️  发货单取消完成: OrderId={orderId}");
    }
}
