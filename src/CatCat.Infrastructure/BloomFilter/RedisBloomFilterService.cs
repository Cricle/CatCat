using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace CatCat.Infrastructure.BloomFilter;

/// <summary>
/// Redis-based Bloom Filter replacement using Redis Sets
/// No memory consumption, distributed-friendly, cluster-safe
/// </summary>
public interface IBloomFilterService
{
    Task<bool> MightContainUserAsync(long userId);
    Task<bool> MightContainPetAsync(long petId);
    Task<bool> MightContainOrderAsync(long orderId);
    Task<bool> MightContainPackageAsync(long packageId);
    Task AddUserAsync(long userId);
    Task AddPetAsync(long petId);
    Task AddOrderAsync(long orderId);
    Task AddPackageAsync(long packageId);
}

public class RedisBloomFilterService(
    IConnectionMultiplexer redis,
    ILogger<RedisBloomFilterService> logger) : IBloomFilterService
{
    private readonly IDatabase _db = redis.GetDatabase();
    
    private const string UserSetKey = "bf:users";
    private const string PetSetKey = "bf:pets";
    private const string OrderSetKey = "bf:orders";
    private const string PackageSetKey = "bf:packages";

    // No initialization needed - Redis handles persistence
    // Use Redis Sets for existence checking (O(1) lookup)

    public async Task<bool> MightContainUserAsync(long userId)
    {
        try
        {
            return await _db.SetContainsAsync(UserSetKey, userId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis check failed for user {UserId}, assuming exists", userId);
            return true; // Fail-safe: assume exists if Redis fails
        }
    }

    public async Task<bool> MightContainPetAsync(long petId)
    {
        try
        {
            return await _db.SetContainsAsync(PetSetKey, petId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis check failed for pet {PetId}, assuming exists", petId);
            return true;
        }
    }

    public async Task<bool> MightContainOrderAsync(long orderId)
    {
        try
        {
            return await _db.SetContainsAsync(OrderSetKey, orderId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis check failed for order {OrderId}, assuming exists", orderId);
            return true;
        }
    }

    public async Task<bool> MightContainPackageAsync(long packageId)
    {
        try
        {
            return await _db.SetContainsAsync(PackageSetKey, packageId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis check failed for package {PackageId}, assuming exists", packageId);
            return true;
        }
    }

    public async Task AddUserAsync(long userId)
    {
        try
        {
            await _db.SetAddAsync(UserSetKey, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add user {UserId} to Redis set", userId);
        }
    }

    public async Task AddPetAsync(long petId)
    {
        try
        {
            await _db.SetAddAsync(PetSetKey, petId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add pet {PetId} to Redis set", petId);
        }
    }

    public async Task AddOrderAsync(long orderId)
    {
        try
        {
            await _db.SetAddAsync(OrderSetKey, orderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add order {OrderId} to Redis set", orderId);
        }
    }

    public async Task AddPackageAsync(long packageId)
    {
        try
        {
            await _db.SetAddAsync(PackageSetKey, packageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add package {PackageId} to Redis set", packageId);
        }
    }
}

