using CatCat.Transit.Saga;
using CatCat.Transit.Results;

namespace OrderProcessing.Sagas;

public class OrderSagaData
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    
    public bool PaymentProcessed { get; set; }
    public bool InventoryReserved { get; set; }
    public bool ShipmentScheduled { get; set; }
    public string? TrackingNumber { get; set; }
}

public class OrderProcessingSaga : SagaBase<OrderSagaData>
{
}

public class ProcessPaymentSagaStep : SagaStepBase<OrderSagaData>
{
    private readonly IPaymentService _paymentService;

    public ProcessPaymentSagaStep(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public override async Task<TransitResult> ExecuteAsync(
        ISaga<OrderSagaData> saga,
        CancellationToken cancellationToken = default)
    {
        var success = await _paymentService.ProcessPaymentAsync(saga.Data.OrderId, saga.Data.Amount);
        
        if (success)
        {
            saga.Data.PaymentProcessed = true;
            return TransitResult.Success();
        }
        
        return TransitResult.Failure("支付失败");
    }

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

public class ReserveInventorySagaStep : SagaStepBase<OrderSagaData>
{
    private readonly IInventoryService _inventoryService;

    public ReserveInventorySagaStep(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public override async Task<TransitResult> ExecuteAsync(
        ISaga<OrderSagaData> saga,
        CancellationToken cancellationToken = default)
    {
        var success = await _inventoryService.ReserveInventoryAsync(
            saga.Data.ProductId, 
            saga.Data.Quantity);
        
        if (success)
        {
            saga.Data.InventoryReserved = true;
            return TransitResult.Success();
        }
        
        return TransitResult.Failure("库存不足");
    }

    public override async Task<TransitResult> CompensateAsync(
        ISaga<OrderSagaData> saga,
        CancellationToken cancellationToken = default)
    {
        if (saga.Data.InventoryReserved)
        {
            await _inventoryService.ReleaseInventoryAsync(
                saga.Data.ProductId, 
                saga.Data.Quantity);
            saga.Data.InventoryReserved = false;
        }
        return TransitResult.Success();
    }
}

public class ScheduleShipmentSagaStep : SagaStepBase<OrderSagaData>
{
    private readonly IShippingService _shippingService;

    public ScheduleShipmentSagaStep(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    public override async Task<TransitResult> ExecuteAsync(
        ISaga<OrderSagaData> saga,
        CancellationToken cancellationToken = default)
    {
        var trackingNumber = await _shippingService.ScheduleShipmentAsync(
            saga.Data.OrderId,
            saga.Data.ProductId,
            saga.Data.Quantity);
        
        saga.Data.TrackingNumber = trackingNumber;
        saga.Data.ShipmentScheduled = true;
        return TransitResult.Success();
    }

    public override async Task<TransitResult> CompensateAsync(
        ISaga<OrderSagaData> saga,
        CancellationToken cancellationToken = default)
    {
        if (saga.Data.ShipmentScheduled && saga.Data.TrackingNumber != null)
        {
            await _shippingService.CancelShipmentAsync(saga.Data.TrackingNumber);
            saga.Data.ShipmentScheduled = false;
        }
        return TransitResult.Success();
    }
}

