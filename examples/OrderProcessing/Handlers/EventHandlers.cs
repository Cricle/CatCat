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
        _logger.LogInformation("✅ 订单创建事件: {OrderId}, 金额: {Amount}", @event.OrderId, @event.Amount);
        return Task.CompletedTask;
    }
}

public class OrderCompletedEventHandler : IEventHandler<OrderCompletedEvent>
{
    private readonly ILogger<OrderCompletedEventHandler> _logger;

    public OrderCompletedEventHandler(ILogger<OrderCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("✅ 订单完成事件: {OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }
}
