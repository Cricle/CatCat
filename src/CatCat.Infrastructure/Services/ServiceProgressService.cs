using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Services;

public interface IServiceProgressService
{
    Task<Result<List<ServiceProgress>>> GetOrderProgressAsync(long orderId, CancellationToken cancellationToken = default);
    Task<Result<ServiceProgress>> GetLatestProgressAsync(long orderId, CancellationToken cancellationToken = default);
    Task<Result<long>> CreateProgressAsync(CreateProgressCommand command, CancellationToken cancellationToken = default);
}

public class ServiceProgressService(
    IServiceProgressRepository repository,
    IServiceOrderRepository orderRepository,
    IFusionCache cache,
    ILogger<ServiceProgressService> logger) : IServiceProgressService
{
    private const string ProgressCacheKeyPrefix = "progress:order:";
    private static readonly TimeSpan ProgressCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<Result<List<ServiceProgress>>> GetOrderProgressAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProgressCacheKeyPrefix}{orderId}";
        
        var progressList = await cache.GetOrSetAsync<List<ServiceProgress>>(
            cacheKey,
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for order progress {OrderId}, fetching from DB", orderId);
                return await repository.GetByOrderIdAsync(orderId);
            },
            options => options.SetDuration(ProgressCacheDuration),
            cancellationToken);

        return Result.Success(progressList);
    }

    public async Task<Result<ServiceProgress>> GetLatestProgressAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var progress = await repository.GetLatestByOrderIdAsync(orderId);
        
        if (progress == null)
        {
            logger.LogWarning("No progress found for order {OrderId}", orderId);
            return Result.Failure<ServiceProgress>("No service progress found");
        }

        return Result.Success(progress);
    }

    public async Task<Result<long>> CreateProgressAsync(CreateProgressCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Verify order exists
        var order = await orderRepository.GetByIdAsync(command.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found", command.OrderId);
            return Result.Failure<long>("Order not found");
        }

        // 2. Verify permission (only assigned service provider can update progress)
        if (order.ServiceProviderId != command.ServiceProviderId)
        {
            logger.LogWarning("Service provider {ProviderId} not authorized for order {OrderId}", 
                command.ServiceProviderId, command.OrderId);
            return Result.Failure<long>("Access denied: You are not assigned to this order");
        }

        // 3. Verify order status (only Accepted or InProgress orders can have progress updates)
        if (order.Status != OrderStatus.Accepted && order.Status != OrderStatus.InProgress)
        {
            logger.LogWarning("Order {OrderId} status {Status} invalid for progress update", 
                command.OrderId, order.Status);
            return Result.Failure<long>($"Order status must be Accepted or InProgress, current: {order.Status}");
        }

        // 4. Verify progress status transition
        var latestProgress = await repository.GetLatestByOrderIdAsync(command.OrderId);
        if (latestProgress != null)
        {
            if (!ProgressStateMachine.IsValidTransition(latestProgress.Status, command.Status))
            {
                logger.LogWarning("Invalid progress transition from {From} to {To} for order {OrderId}", 
                    latestProgress.Status, command.Status, command.OrderId);
                return Result.Failure<long>($"Invalid status transition from {latestProgress.Status} to {command.Status}");
            }
        }
        else
        {
            // First progress must be OnTheWay
            if (!ProgressStateMachine.IsValidFirstStatus(command.Status))
            {
                logger.LogWarning("First progress status must be OnTheWay, got {Status} for order {OrderId}", 
                    command.Status, command.OrderId);
                return Result.Failure<long>("First progress status must be 'On The Way'");
            }
        }

        // 5. Create progress record
        var progress = new ServiceProgress
        {
            Id = YitIdHelper.NextId(),
            OrderId = command.OrderId,
            ServiceProviderId = command.ServiceProviderId,
            Status = command.Status,
            Description = command.Description,
            Latitude = command.Latitude,
            Longitude = command.Longitude,
            Address = command.Address,
            ImageUrls = command.ImageUrls,
            CreatedAt = DateTime.UtcNow
        };

        var progressId = await repository.CreateAsync(progress);
        
        // 6. Update order status if needed (StartService -> InProgress, Completed -> Completed)
        if (command.Status == ServiceProgressStatus.StartService && order.Status == OrderStatus.Accepted)
        {
            logger.LogInformation("Updating order {OrderId} status to InProgress", command.OrderId);
            order.Status = OrderStatus.InProgress;
            await orderRepository.UpdateAsync(order);
        }
        else if (command.Status == ServiceProgressStatus.Completed && order.Status == OrderStatus.InProgress)
        {
            logger.LogInformation("Updating order {OrderId} status to Completed", command.OrderId);
            order.Status = OrderStatus.Completed;
            await orderRepository.UpdateAsync(order);
        }
        
        // 7. Invalidate cache
        await cache.RemoveAsync($"{ProgressCacheKeyPrefix}{command.OrderId}", token: cancellationToken);
        
        logger.LogInformation("Service progress {ProgressId} created for order {OrderId} with status {Status}", 
            progressId, command.OrderId, command.Status);
        
        return Result.Success(progressId);
    }
}

public record CreateProgressCommand(
    long OrderId,
    long ServiceProviderId,
    ServiceProgressStatus Status,
    string? Description,
    double? Latitude,
    double? Longitude,
    string? Address,
    string? ImageUrls);

