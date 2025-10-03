using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Services;

public interface IServicePackageService
{
    Task<Result<ServicePackage>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<List<ServicePackage>>> GetActivePackagesAsync(CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ServicePackage>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public class ServicePackageService(
    IServicePackageRepository repository,
    IFusionCache cache,
    ILogger<ServicePackageService> logger) : IServicePackageService
{
    // Cache keys
    private const string PackageCacheKeyPrefix = "package:";
    private const string ActivePackagesCacheKey = "packages:active";
    
    // Cache durations (packages change infrequently)
    private static readonly TimeSpan PackageCacheDuration = TimeSpan.FromHours(2);
    private static readonly TimeSpan ActivePackagesCacheDuration = TimeSpan.FromHours(1);

    public async Task<Result<ServicePackage>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var package = await cache.GetOrSetAsync<ServicePackage?>(
            $"{PackageCacheKeyPrefix}{id}",
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for package {PackageId}, fetching from DB", id);
                return await repository.GetByIdAsync(id);
            },
            options => options.SetDuration(PackageCacheDuration),
            cancellationToken);

        return package != null
            ? Result.Success(package)
            : Result.Failure<ServicePackage>("Package not found");
    }

    public async Task<Result<List<ServicePackage>>> GetActivePackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = await cache.GetOrSetAsync<List<ServicePackage>>(
            ActivePackagesCacheKey,
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for active packages, fetching from DB");
                return await repository.GetActivePackagesAsync(true);
            },
            options => options.SetDuration(ActivePackagesCacheDuration),
            cancellationToken);

        return Result.Success(packages);
    }

    public async Task<Result<PagedResult<ServicePackage>>> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var offset = (page - 1) * pageSize;
        
        var items = await repository.GetPagedAsync(offset, pageSize);
        var total = await cache.GetOrSetAsync<int>(
            "packages:count",
            async (ctx, ct) => await repository.GetCountAsync(),
            options => options.SetDuration(TimeSpan.FromHours(1)),
            cancellationToken);

        return Result.Success(new PagedResult<ServicePackage>(items, total));
    }
}

