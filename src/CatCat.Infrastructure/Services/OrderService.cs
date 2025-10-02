using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Messages;
using CatCat.Infrastructure.MessageQueue;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using PaymentService = CatCat.Infrastructure.Payment;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Services;

public interface IOrderService
{
    Task<Result<long>> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken = default);
    Task<Result<ServiceOrder>> GetOrderDetailAsync(long orderId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ServiceOrder>>> GetCustomerOrdersAsync(long customerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ServiceOrder>>> GetProviderOrdersAsync(long providerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result> CancelOrderAsync(long orderId, long userId, CancellationToken cancellationToken = default);
    Task<Result> AcceptOrderAsync(long orderId, long providerId, CancellationToken cancellationToken = default);
    Task<Result> StartServiceAsync(long orderId, long providerId, CancellationToken cancellationToken = default);
    Task<Result> CompleteServiceAsync(long orderId, long providerId, CancellationToken cancellationToken = default);
    Task<Result> PayOrderAsync(long orderId, string paymentIntentId, CancellationToken cancellationToken = default);
}

public record CreateOrderCommand(
    long CustomerId,
    long ServicePackageId,
    long PetId,
    DateTime ServiceDate,
    string ServiceAddress,
    string? Remark);

public class OrderService : IOrderService
{
    private readonly IServiceOrderRepository _orderRepository;
    private readonly IOrderStatusHistoryRepository _historyRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IServicePackageRepository _packageRepository;
    private readonly IMessageQueueService _messageQueue;
    private readonly PaymentService.IPaymentService _paymentService;
    private readonly IFusionCache _cache;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IServiceOrderRepository orderRepository,
        IOrderStatusHistoryRepository historyRepository,
        IPaymentRepository paymentRepository,
        IServicePackageRepository packageRepository,
        IMessageQueueService messageQueue,
        PaymentService.IPaymentService paymentService,
        IFusionCache cache,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _paymentRepository = paymentRepository;
        _packageRepository = packageRepository;
        _messageQueue = messageQueue;
        _paymentService = paymentService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<long>> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Pre-validate package exists (cached)
            var package = await _cache.GetOrSetAsync(
                $"package:{command.ServicePackageId}",
                _ => _packageRepository.GetByIdAsync(command.ServicePackageId),
                new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(1) });

            if (package == null)
                return Result.Failure<long>("Service package not found");

            // Generate order ID and number
            var orderId = YitIdHelper.NextId();
            var orderNo = GenerateOrderNo();

            // Send to JetStream queue instead of immediate DB insert
            var queueMessage = new OrderQueueMessage(
                orderId,
                orderNo,
                command.CustomerId,
                command.ServicePackageId,
                command.PetId,
                command.ServiceDate,
                command.ServiceAddress,
                command.Remark,
                DateTime.UtcNow);

            await _messageQueue.PublishAsync("order.queue", queueMessage, cancellationToken);

            _logger.LogInformation("Order {OrderId} queued for processing", orderId);

            return Result.Success(orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order creation failed");
            return Result.Failure<long>("Order creation failed: " + ex.Message);
        }
    }

    public async Task<Result<ServiceOrder>> GetOrderDetailAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        return order != null
            ? Result.Success(order)
            : Result.Failure<ServiceOrder>("Order not found");
    }

    public async Task<Result<PagedResult<ServiceOrder>>> GetCustomerOrdersAsync(
        long customerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // TODO: Optimize with window function (COUNT(*) OVER() in single query)
        var offset = (page - 1) * pageSize;
        var items = status.HasValue
            ? await _orderRepository.GetByCustomerIdAndStatusPagedAsync(customerId, status.Value.ToString(), offset, pageSize)
            : await _orderRepository.GetByCustomerIdPagedAsync(customerId, offset, pageSize);
        var total = status.HasValue
            ? await _orderRepository.CountByCustomerIdAndStatusAsync(customerId, status.Value.ToString())
            : await _orderRepository.CountByCustomerIdAsync(customerId);
        return Result.Success(new PagedResult<ServiceOrder>(items, total));
    }

    public async Task<Result<PagedResult<ServiceOrder>>> GetProviderOrdersAsync(
        long providerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // TODO: Optimize with window function (COUNT(*) OVER() in single query)
        var offset = (page - 1) * pageSize;
        var items = status.HasValue
            ? await _orderRepository.GetByServiceProviderIdAndStatusPagedAsync(providerId, status.Value.ToString(), offset, pageSize)
            : await _orderRepository.GetByServiceProviderIdPagedAsync(providerId, offset, pageSize);
        var total = status.HasValue
            ? await _orderRepository.CountByServiceProviderIdAndStatusAsync(providerId, status.Value.ToString())
            : await _orderRepository.CountByServiceProviderIdAsync(providerId);
        return Result.Success(new PagedResult<ServiceOrder>(items, total));
    }

    public async Task<Result> CancelOrderAsync(long orderId, long userId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        
        // If order not found in DB, it might still be in queue
        if (order == null)
        {
            // Mark order as cancelled in cache so processing service can skip it
            await _cache.SetAsync(
                $"order:cancelled:{orderId}",
                true,
                new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(24) },
                cancellationToken);

            _logger.LogInformation("Order {OrderId} marked for cancellation (in queue)", orderId);
            return Result.Success();
        }

        if (order.CustomerId != userId)
            return Result.Failure("Unauthorized");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Queued)
            return Result.Failure($"Cannot cancel order (Status: {order.Status})");

        var affectedRows = await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled.ToString(), DateTime.UtcNow);
        if (affectedRows <= 0)
            return Result.Failure("Cancel order failed");

        await _messageQueue.PublishAsync(
            "order.status_changed",
            new { OrderId = orderId, Status = OrderStatus.Cancelled.ToString(), Notes = "User cancelled order" },
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AcceptOrderAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure("Order not found");

        if (order.Status != OrderStatus.Pending)
            return Result.Failure($"Cannot accept order (Status: {order.Status})");

        order.ServiceProviderId = providerId;
        order.Status = OrderStatus.Accepted;
        var affectedRows = await _orderRepository.UpdateAsync(order);

        if (affectedRows > 0)
        {
            await _messageQueue.PublishAsync(
                "order.status_changed",
                new { OrderId = orderId, Status = OrderStatus.Accepted.ToString(), Notes = "Provider accepted order" },
                cancellationToken);
            return Result.Success();
        }

        return Result.Failure("Accept order failed");
    }

    public async Task<Result> StartServiceAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        return await UpdateOrderStatusWithValidationAsync(
            orderId,
            OrderStatus.InProgress,
            OrderStatus.Accepted,
            "开始服务",
            order => order.ServiceProviderId == providerId ? null : "无权操作此订单",
            cancellationToken);
    }

    public async Task<Result> CompleteServiceAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        return await UpdateOrderStatusWithValidationAsync(
            orderId,
            OrderStatus.Completed,
            OrderStatus.InProgress,
            "服务完成",
            order => order.ServiceProviderId == providerId ? null : "无权操作此订单",
            cancellationToken);
    }

    public async Task<Result> PayOrderAsync(long orderId, string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByPaymentIntentIdAsync(paymentIntentId);
        if (payment == null)
            return Result.Failure("支付记录不存在");

        if (payment.OrderId != orderId)
            return Result.Failure("支付记录与订单不匹配");

        var confirmed = await _paymentService.ConfirmPaymentAsync(paymentIntentId);
        if (confirmed)
        {
            await _paymentRepository.UpdateStatusSuccessAsync(payment.Id, PaymentStatus.Succeeded.ToString(), DateTime.UtcNow, DateTime.UtcNow);
            // 清除订单缓存（因为支付状态改变会影响订单）
            return Result.Success();
        }

        return Result.Failure("支付确认失败");
    }

    private async Task<Result> UpdateOrderStatusWithValidationAsync(
        long orderId,
        OrderStatus newStatus,
        OrderStatus expectedCurrentStatus,
        string notes,
        Func<ServiceOrder, string?> additionalValidation,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure("订单不存在");

        var validationError = additionalValidation(order);
        if (validationError != null)
            return Result.Failure(validationError);

        if (order.Status != expectedCurrentStatus)
            return Result.Failure($"订单状态不允许此操作（当前状态：{order.Status}）");

        var affectedRows = await _orderRepository.UpdateStatusAsync(orderId, newStatus.ToString(), DateTime.UtcNow);
        if (affectedRows > 0)
        {
            // 异步记录状态历史（使用NATS削峰）
            await _messageQueue.PublishAsync(
                "order.status_changed",
                new { OrderId = orderId, Status = newStatus.ToString(), Notes = notes },
                cancellationToken);
            return Result.Success();
        }

        return Result.Failure("操作失败");
    }

    private string GenerateOrderNo()
    {
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }
}

public record OrderQueueMessage(
    long OrderId,
    string OrderNo,
    long CustomerId,
    long ServicePackageId,
    long PetId,
    DateTime ServiceDate,
    string ServiceAddress,
    string? Remark,
    DateTime CreatedAt);

