using CatCat.Transit.CatGa.Core;
using CatCat.Transit.CatGa.Models;
using OrderProcessing.Services;

namespace OrderProcessing.Transactions;

/// <summary>
/// 订单处理 CatGa 事务
/// 包含支付、库存、发货的完整流程
/// </summary>
public record OrderRequest(Guid OrderId, decimal Amount, string ProductId, int Quantity, string ShippingAddress);
public record OrderResult(Guid OrderId, string Status, string PaymentId, string ShipmentId);

public class OrderProcessingTransaction : ICatGaTransaction<OrderRequest, OrderResult>
{
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;

    public OrderProcessingTransaction(
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IShippingService shippingService)
    {
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _shippingService = shippingService;
    }

    public async Task<OrderResult> ExecuteAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        // 1. 处理支付
        var paymentId = await _paymentService.ProcessPaymentAsync(
            request.OrderId,
            request.Amount,
            cancellationToken);

        // 2. 预留库存
        await _inventoryService.ReserveInventoryAsync(
            request.ProductId,
            request.Quantity,
            cancellationToken);

        // 3. 创建发货单
        var shipmentId = await _shippingService.CreateShipmentAsync(
            request.OrderId,
            request.ShippingAddress,
            cancellationToken);

        return new OrderResult(
            request.OrderId,
            "Completed",
            paymentId,
            shipmentId);
    }

    public async Task CompensateAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        // 按相反顺序补偿

        // 3. 取消发货
        try
        {
            await _shippingService.CancelShipmentAsync(request.OrderId, cancellationToken);
        }
        catch (Exception ex)
        {
            // 记录但不抛出
            Console.WriteLine($"Failed to cancel shipment: {ex.Message}");
        }

        // 2. 释放库存
        try
        {
            await _inventoryService.ReleaseInventoryAsync(request.ProductId, request.Quantity, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to release inventory: {ex.Message}");
        }

        // 1. 退款
        try
        {
            await _paymentService.RefundPaymentAsync(request.OrderId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to refund payment: {ex.Message}");
        }
    }
}

