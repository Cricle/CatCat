using CatCat.Transit.StateMachine;
using Microsoft.Extensions.Logging;
using OrderProcessing.Events;

namespace OrderProcessing.StateMachines;

public enum OrderState
{
    New,
    PaymentPending,
    PaymentConfirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderStateMachineData
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public string? TrackingNumber { get; set; }
}

public class OrderStateMachine : StateMachineBase<OrderState, OrderStateMachineData>
{
    public OrderStateMachine(ILogger<OrderStateMachine> logger) : base(logger)
    {
        CurrentState = OrderState.New;
        ConfigureStateMachine();
    }

    private void ConfigureStateMachine()
    {
        // New -> PaymentPending
        ConfigureTransition<OrderPlacedEvent>(OrderState.New, async (@event) =>
        {
            Data.OrderId = @event.OrderId;
            Data.TotalAmount = @event.Amount;
            return OrderState.PaymentPending;
        });

        // PaymentPending -> PaymentConfirmed
        ConfigureTransition<PaymentConfirmedEvent>(OrderState.PaymentPending, async (@event) =>
        {
            Data.PaymentDate = DateTime.UtcNow;
            return OrderState.PaymentConfirmed;
        });

        // PaymentConfirmed -> Processing (自动)
        OnEnter(OrderState.PaymentConfirmed, async (data) =>
        {
            await Task.Delay(100); // 模拟处理
            await TransitionToAsync(OrderState.Processing);
        });

        // Processing -> Shipped
        ConfigureTransition<OrderShippedEvent>(OrderState.Processing, async (@event) =>
        {
            Data.ShipmentDate = DateTime.UtcNow;
            Data.TrackingNumber = @event.TrackingNumber;
            return OrderState.Shipped;
        });
    }
}

