using CatCat.Transit.Handlers;
using Microsoft.Extensions.Logging;
using OrderProcessing.Events;

namespace OrderProcessing.Handlers;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 发送订单确认邮件: 订单 {@OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }
}

public class PaymentProcessedEventHandler : IEventHandler<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public PaymentProcessedEventHandler(ILogger<PaymentProcessedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(PaymentProcessedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 发送支付成功通知: 订单 {@OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }
}

