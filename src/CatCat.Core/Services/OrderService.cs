using CatCat.Core.Common;
using CatCat.Domain.Entities;
using CatCat.Domain.Messages;
using CatCat.Infrastructure.MessageQueue;
using CatCat.Infrastructure.Payment;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Core.Services;

/// <summary>
/// 优化的订单服务 - 使用 Result 模式，避免异常抛出
/// </summary>
public interface IOrderService
{
    Task<Result<long>> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken = default);
    Task<Result<ServiceOrder>> GetOrderDetailAsync(long orderId, CancellationToken cancellationToken = default);
    Task<Result<(IEnumerable<ServiceOrder> Items, int Total)>> GetCustomerOrdersAsync(long customerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<(IEnumerable<ServiceOrder> Items, int Total)>> GetProviderOrdersAsync(long providerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default);
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
    private readonly IPaymentService _paymentService;
    private readonly IFusionCache _cache;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IServiceOrderRepository orderRepository,
        IOrderStatusHistoryRepository historyRepository,
        IPaymentRepository paymentRepository,
        IServicePackageRepository packageRepository,
        IMessageQueueService messageQueue,
        IPaymentService paymentService,
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
            // 1. 获取服务套餐
            var package = await _cache.GetOrSetAsync(
                $"package:{command.ServicePackageId}",
                _ => _packageRepository.GetByIdAsync(command.ServicePackageId),
                new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(1) });

            if (package == null)
                return Result.Failure<long>("服务套餐不存在");

            // 2. 创建订单
            var order = new ServiceOrder
            {
                Id = YitIdHelper.NextId(),
                OrderNo = GenerateOrderNo(),
                CustomerId = command.CustomerId,
                ServicePackageId = command.ServicePackageId,
                PetId = command.PetId,
                ServiceDate = command.ServiceDate,
                Address = command.ServiceAddress,
                Price = package.Price,
                Status = OrderStatus.Pending,
                CustomerRemark = command.Remark,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var orderId = await _orderRepository.CreateAsync(order);

            // 3. 创建支付意图
            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
                orderId,
                order.Price,
                "cny");

            var payment = new Payment
            {
                Id = YitIdHelper.NextId(),
                OrderId = orderId,
                Amount = order.Price,
                Currency = "CNY",
                PaymentIntentId = paymentIntent.PaymentIntentId!,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.CreateAsync(payment);

            // 4. 异步记录状态历史（使用NATS）
            await _messageQueue.PublishAsync(
                "order.status_changed",
                new { OrderId = orderId, Status = OrderStatus.Pending.ToString(), Notes = "订单创建成功" },
                cancellationToken);

            // 5. 发送订单创建消息
            await _messageQueue.PublishAsync(
                "order.created",
                new OrderCreatedMessage { OrderId = orderId },
                cancellationToken);

            return Result.Success((long)orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建订单失败");
            return Result.Failure<long>("创建订单失败：" + ex.Message);
        }
    }

    public async Task<Result<ServiceOrder>> GetOrderDetailAsync(long orderId, CancellationToken cancellationToken = default)
    {
        // 订单状态变化频繁，强一致性要求，不使用缓存
        var order = await _orderRepository.GetByIdAsync(orderId);

        return order != null
            ? Result.Success(order)
            : Result.Failure<ServiceOrder>("订单不存在");
    }

    public async Task<Result<(IEnumerable<ServiceOrder> Items, int Total)>> GetCustomerOrdersAsync(
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
        return Result.Success((items.AsEnumerable(), total));
    }

    public async Task<Result<(IEnumerable<ServiceOrder> Items, int Total)>> GetProviderOrdersAsync(
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
        return Result.Success((items.AsEnumerable(), total));
    }

    public async Task<Result> CancelOrderAsync(long orderId, long userId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure("订单不存在");

        if (order.CustomerId != userId)
            return Result.Failure("无权操作此订单");

        if (order.Status != OrderStatus.Pending)
            return Result.Failure($"订单状态不允许取消（当前状态：{order.Status}）");

        var affectedRows = await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled.ToString(), DateTime.UtcNow);
        if (affectedRows <= 0)
            return Result.Failure("取消订单失败");

        // 异步记录状态历史
        await _messageQueue.PublishAsync(
            "order.status_changed",
            new { OrderId = orderId, Status = OrderStatus.Cancelled.ToString(), Notes = "用户取消订单" },
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AcceptOrderAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure("订单不存在");

        if (order.Status != OrderStatus.Pending)
            return Result.Failure($"订单状态不允许接单（当前状态：{order.Status}）");

        order.ServiceProviderId = providerId;
        order.Status = OrderStatus.Accepted;
        var affectedRows = await _orderRepository.UpdateAsync(order);

        if (affectedRows > 0)
        {
            // 异步记录状态历史
            await _messageQueue.PublishAsync(
                "order.status_changed",
                new { OrderId = orderId, Status = OrderStatus.Accepted.ToString(), Notes = "服务商接单" },
                cancellationToken);
            return Result.Success();
        }

        return Result.Failure("接单失败");
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

