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

public class OrderService(
    IServiceOrderRepository orderRepository,
    IOrderStatusHistoryRepository historyRepository,
    IPaymentRepository paymentRepository,
    IServicePackageRepository packageRepository,
    IMessageQueueService messageQueue,
    PaymentService.IPaymentService paymentService,
    IFusionCache cache,
    ILogger<OrderService> logger) : IOrderService
{

    public async Task<Result<long>> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Idempotency check (prevent duplicate submissions within 5 minutes)
            var idempotencyKey = $"order:create:{command.CustomerId}:{command.ServicePackageId}:{command.ServiceDate:yyyyMMddHHmm}";
            var existingOrderId = await cache.GetOrDefaultAsync<long?>(idempotencyKey, token: cancellationToken);
            if (existingOrderId.HasValue && existingOrderId.Value > 0)
            {
                logger.LogInformation("Duplicate order creation attempt detected for customer {CustomerId}, returning existing order {OrderId}", 
                    command.CustomerId, existingOrderId.Value);
                return Result.Success(existingOrderId.Value);
            }

            // 2. Pre-validate package exists (cached)
            var package = await cache.GetOrSetAsync(
                $"package:{command.ServicePackageId}",
                _ => packageRepository.GetByIdAsync(command.ServicePackageId),
                new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(1) });

            if (package == null)
            {
                logger.LogWarning("Service package {PackageId} not found", command.ServicePackageId);
                return Result.Failure<long>("Service package not found");
            }

            // 3. Check time conflict (prevent double booking)
            // For simplicity, we'll just check orders on the same day
            // TODO: Implement more sophisticated time conflict detection
            // var hasConflict = await CheckTimeConflictAsync(command.CustomerId, command.ServiceDate);

            // 4. Validate service date (not in the past, not too far in future)
            var now = DateTime.UtcNow;
            if (command.ServiceDate < now.AddHours(2))
            {
                logger.LogWarning("Service date {ServiceDate} is too soon (need 2 hours notice)", command.ServiceDate);
                return Result.Failure<long>("Service must be scheduled at least 2 hours in advance");
            }

            if (command.ServiceDate > now.AddDays(30))
            {
                logger.LogWarning("Service date {ServiceDate} is too far in future (max 30 days)", command.ServiceDate);
                return Result.Failure<long>("Service can only be scheduled within 30 days");
            }

            // 5. Generate order ID and number
            var orderId = YitIdHelper.NextId();
            var orderNo = GenerateOrderNo();

            // 6. Cache order ID for idempotency (5 minutes)
            await cache.SetAsync(
                idempotencyKey,
                orderId,
                new FusionCacheEntryOptions { Duration = TimeSpan.FromMinutes(5) },
                cancellationToken);

            // 7. Send to JetStream queue instead of immediate DB insert
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

            await messageQueue.PublishAsync("order.queue", queueMessage, cancellationToken);

            logger.LogInformation("Order {OrderId} queued for processing", orderId);

            return Result.Success(orderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Order creation failed");
            return Result.Failure<long>("Order creation failed: " + ex.Message);
        }
    }

    public async Task<Result<ServiceOrder>> GetOrderDetailAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId);

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
            ? await orderRepository.GetByCustomerIdAndStatusPagedAsync(customerId, status.Value.ToString(), offset, pageSize)
            : await orderRepository.GetByCustomerIdPagedAsync(customerId, offset, pageSize);
        var total = status.HasValue
            ? await orderRepository.CountByCustomerIdAndStatusAsync(customerId, status.Value.ToString())
            : await orderRepository.CountByCustomerIdAsync(customerId);
        return Result.Success(new PagedResult<ServiceOrder>(items, total));
    }

    public async Task<Result<PagedResult<ServiceOrder>>> GetProviderOrdersAsync(
        long providerId, int? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // TODO: Optimize with window function (COUNT(*) OVER() in single query)
        var offset = (page - 1) * pageSize;
        var items = status.HasValue
            ? await orderRepository.GetByServiceProviderIdAndStatusPagedAsync(providerId, status.Value.ToString(), offset, pageSize)
            : await orderRepository.GetByServiceProviderIdPagedAsync(providerId, offset, pageSize);
        var total = status.HasValue
            ? await orderRepository.CountByServiceProviderIdAndStatusAsync(providerId, status.Value.ToString())
            : await orderRepository.CountByServiceProviderIdAsync(providerId);
        return Result.Success(new PagedResult<ServiceOrder>(items, total));
    }

    public async Task<Result> CancelOrderAsync(long orderId, long userId, CancellationToken cancellationToken = default)
    {
        // 1. Check if order exists in DB
        var order = await orderRepository.GetByIdAsync(orderId);

        // If order not found in DB, it might still be in queue
        if (order == null)
        {
            // Mark order as cancelled in cache so processing service can skip it
            await cache.SetAsync(
                $"order:cancelled:{orderId}",
                true,
                new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(24) },
                cancellationToken);

            logger.LogInformation("Order {OrderId} marked for cancellation (in queue)", orderId);
            return Result.Success();
        }

        // 2. Verify ownership
        if (order.CustomerId != userId)
        {
            logger.LogWarning("User {UserId} attempted to cancel order {OrderId} owned by {CustomerId}", 
                userId, orderId, order.CustomerId);
            return Result.Failure("Access denied: You can only cancel your own orders");
        }

        // 3. Validate state transition
        if (!OrderStateMachine.IsValidTransition(order.Status, OrderStatus.Cancelled))
        {
            logger.LogWarning("Invalid status transition from {From} to Cancelled for order {OrderId}", 
                order.Status, orderId);
            return Result.Failure($"Cannot cancel order from status: {order.Status}");
        }

        // 5. Process refund (if payment was made)
        // TODO: Implement refund logic with payment intent ID
        if (order.Status != OrderStatus.Queued && order.Status != OrderStatus.Pending)
        {
            logger.LogInformation("Refund needed for order {OrderId} (payment logic pending)", orderId);
            // var refundResult = await paymentService.RefundPaymentAsync(paymentIntentId);
            // Handle refund result
        }

        // 6. Update order status
        var affectedRows = await orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled.ToString(), DateTime.UtcNow);
        if (affectedRows <= 0)
        {
            logger.LogError("Failed to update order {OrderId} status to Cancelled", orderId);
            return Result.Failure("Cancel order failed");
        }

        // 7. Notify service provider (if assigned)
        if (order.ServiceProviderId.HasValue)
        {
            await messageQueue.PublishAsync(
                "order.provider_notification",
                new { OrderId = orderId, ProviderId = order.ServiceProviderId.Value, Type = "OrderCancelled" },
                cancellationToken);
        }

        // 8. Publish status change event
        await messageQueue.PublishAsync(
            "order.status_changed",
            new { OrderId = orderId, Status = OrderStatus.Cancelled.ToString(), Notes = "User cancelled order" },
            cancellationToken);

        logger.LogInformation("Order {OrderId} cancelled successfully by user {UserId}", orderId, userId);

        return Result.Success();
    }

    public async Task<Result> AcceptOrderAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        // 1. Get order
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found", orderId);
            return Result.Failure("Order not found");
        }

        // 2. Validate state transition
        if (!OrderStateMachine.IsValidTransition(order.Status, OrderStatus.Accepted))
        {
            logger.LogWarning("Invalid status transition from {From} to Accepted for order {OrderId}", 
                order.Status, orderId);
            return Result.Failure($"Cannot accept order from status: {order.Status}");
        }

        // 3. Check if already assigned
        if (order.ServiceProviderId.HasValue)
        {
            logger.LogWarning("Order {OrderId} already assigned to provider {ProviderId}", 
                orderId, order.ServiceProviderId.Value);
            return Result.Failure("Order already assigned to another service provider");
        }

        // 4. Update order
        order.ServiceProviderId = providerId;
        order.Status = OrderStatus.Accepted;
        var affectedRows = await orderRepository.UpdateAsync(order);

        if (affectedRows <= 0)
        {
            logger.LogError("Failed to update order {OrderId} when accepting", orderId);
            return Result.Failure("Accept order failed");
        }

        // 5. Notify customer
        await messageQueue.PublishAsync(
            "order.customer_notification",
            new { OrderId = orderId, CustomerId = order.CustomerId, Type = "OrderAccepted", ProviderId = providerId },
            cancellationToken);

        // 6. Publish status change event
        await messageQueue.PublishAsync(
            "order.status_changed",
            new { OrderId = orderId, Status = OrderStatus.Accepted.ToString(), Notes = $"Provider {providerId} accepted order" },
            cancellationToken);

        logger.LogInformation("Order {OrderId} accepted by provider {ProviderId}", orderId, providerId);

        return Result.Success();
    }

    public async Task<Result> StartServiceAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        return await UpdateOrderStatusWithValidationAsync(
            orderId,
            OrderStatus.InProgress,
            OrderStatus.Accepted,
            "Service started",
            order => order.ServiceProviderId == providerId ? null : "Unauthorized operation",
            cancellationToken);
    }

    public async Task<Result> CompleteServiceAsync(long orderId, long providerId, CancellationToken cancellationToken = default)
    {
        return await UpdateOrderStatusWithValidationAsync(
            orderId,
            OrderStatus.Completed,
            OrderStatus.InProgress,
            "Service completed",
            order => order.ServiceProviderId == providerId ? null : "Unauthorized operation",
            cancellationToken);
    }

    public async Task<Result> PayOrderAsync(long orderId, string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByPaymentIntentIdAsync(paymentIntentId);
        if (payment == null)
            return Result.Failure("Payment record not found");

        if (payment.OrderId != orderId)
            return Result.Failure("Payment order mismatch");

        var confirmed = await paymentService.ConfirmPaymentAsync(paymentIntentId);
        if (confirmed)
        {
            await paymentRepository.UpdateStatusSuccessAsync(payment.Id, PaymentStatus.Succeeded.ToString(), DateTime.UtcNow, DateTime.UtcNow);
            return Result.Success();
        }

        return Result.Failure("Payment confirmation failed");
    }

    private async Task<Result> UpdateOrderStatusWithValidationAsync(
        long orderId,
        OrderStatus newStatus,
        OrderStatus expectedCurrentStatus,
        string notes,
        Func<ServiceOrder, string?> additionalValidation,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure("Order not found");

        var validationError = additionalValidation(order);
        if (validationError != null)
            return Result.Failure(validationError);

        if (order.Status != expectedCurrentStatus)
            return Result.Failure($"Invalid order status (Current: {order.Status})");

        var affectedRows = await orderRepository.UpdateStatusAsync(orderId, newStatus.ToString(), DateTime.UtcNow);
        if (affectedRows > 0)
        {
            await messageQueue.PublishAsync(
                "order.status_changed",
                new { OrderId = orderId, Status = newStatus.ToString(), Notes = notes },
                cancellationToken);
            return Result.Success();
        }

        return Result.Failure("Operation failed");
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

