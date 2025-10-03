using CatCat.Infrastructure.BloomFilter;
using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Services;

public interface IUserService
{
    Task<Result<User>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<User>> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
}

public class UserService(
    IUserRepository repository,
    IFusionCache cache,
    IBloomFilterService bloomFilter,
    ILogger<UserService> logger) : IUserService
{
    // Cache keys
    private const string UserCacheKeyPrefix = "user:";
    private const string UserPhoneCacheKeyPrefix = "user:phone:";
    
    // Cache durations (user info changes occasionally)
    private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(20);

    public async Task<Result<User>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        // Redis-based Bloom filter: quickly reject non-existent IDs (prevent cache penetration)
        if (!await bloomFilter.MightContainUserAsync(id))
        {
            logger.LogDebug("User {UserId} blocked by Bloom Filter (not exist)", id);
            return Result.Failure<User>("User not found");
        }

        var user = await cache.GetOrSetAsync<User?>(
            $"{UserCacheKeyPrefix}{id}",
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for user {UserId}, fetching from DB", id);
                return await repository.GetByIdAsync(id);
            },
            options => options.SetDuration(UserCacheDuration),
            cancellationToken);

        return user != null
            ? Result.Success(user)
            : Result.Failure<User>("User not found");
    }

    public async Task<Result<User>> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var user = await cache.GetOrSetAsync<User?>(
            $"{UserPhoneCacheKeyPrefix}{phone}",
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for phone {Phone}, fetching from DB", phone);
                return await repository.GetByPhoneAsync(phone);
            },
            options => options.SetDuration(UserCacheDuration),
            cancellationToken);

        return user != null
            ? Result.Success(user)
            : Result.Failure<User>("User not found");
    }
}

