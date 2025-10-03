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
        // Verify order exists
        var order = await orderRepository.GetByIdAsync(command.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found", command.OrderId);
            return Result.Failure<long>("Order not found");
        }

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
        
        // Invalidate cache
        await cache.RemoveAsync($"{ProgressCacheKeyPrefix}{command.OrderId}", token: cancellationToken);
        
        logger.LogInformation("Service progress {ProgressId} created for order {OrderId}", progressId, command.OrderId);
        
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

