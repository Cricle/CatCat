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

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IFusionCache _cache;
    private readonly ILogger<UserService> _logger;

    // Cache keys
    private const string UserCacheKeyPrefix = "user:";
    private const string UserPhoneCacheKeyPrefix = "user:phone:";
    
    // Cache durations (user info changes occasionally)
    private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(20);

    public UserService(
        IUserRepository repository,
        IFusionCache cache,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<User>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _cache.GetOrSetAsync<User?>(
            $"{UserCacheKeyPrefix}{id}",
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for user {UserId}, fetching from DB", id);
                return await _repository.GetByIdAsync(id);
            },
            options => options.SetDuration(UserCacheDuration),
            cancellationToken);

        return user != null
            ? Result.Success(user)
            : Result.Failure<User>("User not found");
    }

    public async Task<Result<User>> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var user = await _cache.GetOrSetAsync<User?>(
            $"{UserPhoneCacheKeyPrefix}{phone}",
            async (ctx, ct) =>
            {
                _logger.LogDebug("Cache miss for phone {Phone}, fetching from DB", phone);
                return await _repository.GetByPhoneAsync(phone);
            },
            options => options.SetDuration(UserCacheDuration),
            cancellationToken);

        return user != null
            ? Result.Success(user)
            : Result.Failure<User>("User not found");
    }
}

