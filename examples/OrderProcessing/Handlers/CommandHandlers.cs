using CatCat.Transit.Handlers;
using CatCat.Transit.Results;
using Microsoft.Extensions.Logging;
using OrderProcessing.Commands;
using OrderProcessing.Events;
using OrderProcessing.Services;

namespace OrderProcessing.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly CatCat.Transit.ITransitMediator _mediator;

    public CreateOrderCommandHandler(
        ILogger<CreateOrderCommandHandler> logger,
        CatCat.Transit.ITransitMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<TransitResult<Guid>> HandleAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建订单: {ProductId} x {Quantity}", request.ProductId, request.Quantity);
        
        // 模拟订单创建
        await Task.Delay(50, cancellationToken);
        var orderId = Guid.NewGuid();
        
        // 发布订单创建事件
        await _mediator.PublishAsync(new OrderCreatedEvent
        {
            OrderId = orderId,
            Amount = request.Amount,
            CorrelationId = request.CorrelationId
        }, cancellationToken);
        
        return TransitResult<Guid>.Success(orderId);
    }
}

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, bool>
{
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;
    private readonly IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(
        ILogger<ProcessPaymentCommandHandler> logger,
        IPaymentService paymentService)
    {
        _logger = logger;
        _paymentService = paymentService;
    }

    public async Task<TransitResult<bool>> HandleAsync(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("处理支付: 订单 {OrderId}, 金额 {Amount}", request.OrderId, request.Amount);
        
        var paymentId = await _paymentService.ProcessPaymentAsync(request.OrderId, request.Amount, cancellationToken);
        var success = !string.IsNullOrEmpty(paymentId);
        
        if (success)
        {
            return TransitResult<bool>.Success(true);
        }
        
        return TransitResult<bool>.Failure("支付处理失败");
    }
}

