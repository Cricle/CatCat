using CatCat.Infrastructure.BloomFilter;
using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Services;

public interface IAdminService
{
    // User Management
    Task<Result<PagedResult<User>>> GetUsersAsync(int page, int pageSize, UserRole? role, UserStatus? status, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserStatusAsync(long userId, UserStatus status, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserRoleAsync(long userId, UserRole role, CancellationToken cancellationToken = default);
    
    // Pet Management
    Task<Result<PagedResult<Pet>>> GetAllPetsAsync(int page, int pageSize, long? userId, CancellationToken cancellationToken = default);
    Task<Result> DeletePetAsync(long petId, CancellationToken cancellationToken = default);
    
    // Service Package Management
    Task<Result<ServicePackage>> CreatePackageAsync(CreatePackageCommand command, CancellationToken cancellationToken = default);
    Task<Result> UpdatePackageAsync(UpdatePackageCommand command, CancellationToken cancellationToken = default);
    Task<Result> DeletePackageAsync(long packageId, CancellationToken cancellationToken = default);
    Task<Result> TogglePackageStatusAsync(long packageId, bool isActive, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<Result<AdminStatistics>> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

public record CreatePackageCommand(
    string Name,
    string Description,
    decimal Price,
    int Duration,
    string? IconUrl,
    string ServiceItems,
    int SortOrder);

public record UpdatePackageCommand(
    long Id,
    string Name,
    string Description,
    decimal Price,
    int Duration,
    string? IconUrl,
    string ServiceItems,
    bool IsActive,
    int SortOrder);

public record AdminStatistics(
    int TotalUsers,
    int ActiveUsers,
    int TotalPets,
    int TotalOrders,
    int PendingOrders,
    int TotalPackages,
    int ActivePackages);

public class AdminService(
    IUserRepository userRepository,
    IPetRepository petRepository,
    IServicePackageRepository packageRepository,
    IServiceOrderRepository orderRepository,
    IFusionCache cache,
    IBloomFilterService bloomFilter,
    ILogger<AdminService> logger) : IAdminService
{
    private const string ActivePackagesCacheKey = "packages:active";
    private const string StatsCacheKey = "admin:stats";

    public async Task<Result<PagedResult<User>>> GetUsersAsync(
        int page, int pageSize, UserRole? role, UserStatus? status, CancellationToken cancellationToken = default)
    {
        var offset = (page - 1) * pageSize;
        
        // Cache key for filtered results
        var cacheKey = $"admin:users:{page}:{pageSize}:{role}:{status}";
        
        var result = await cache.GetOrSetAsync<PagedResult<User>>(
            cacheKey,
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for admin users, fetching from DB");
                
                // Get paginated users (repository should handle filtering if needed)
                var users = await userRepository.GetPagedAsync(offset, pageSize);
                
                // Apply filters in memory (TODO: move to repository for better performance)
                var filtered = users.AsEnumerable();
                if (role.HasValue)
                    filtered = filtered.Where(u => u.Role == role.Value);
                if (status.HasValue)
                    filtered = filtered.Where(u => u.Status == status.Value);
                
                var items = filtered.ToList();
                var total = await userRepository.GetCountAsync();
                
                return new PagedResult<User>(items, total);
            },
            options => options.SetDuration(TimeSpan.FromMinutes(2)),
            cancellationToken);
        
        return Result.Success(result);
    }

    public async Task<Result> UpdateUserStatusAsync(long userId, UserStatus status, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for status update", userId);
            return Result.Failure("User not found");
        }

        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;
        
        var affectedRows = await userRepository.UpdateAsync(user);
        if (affectedRows > 0)
        {
            // Invalidate related caches
            await cache.RemoveAsync($"user:{userId}", token: cancellationToken);
            await cache.RemoveAsync(StatsCacheKey, token: cancellationToken);
            // Note: Admin user list cache will expire naturally (2 min TTL)
            
            logger.LogInformation("User {UserId} status updated to {Status}", userId, status);
            return Result.Success();
        }

        return Result.Failure("Update user status failed");
    }

    public async Task<Result> UpdateUserRoleAsync(long userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for role update", userId);
            return Result.Failure("User not found");
        }

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        
        var affectedRows = await userRepository.UpdateAsync(user);
        if (affectedRows > 0)
        {
            // Invalidate related caches
            await cache.RemoveAsync($"user:{userId}", token: cancellationToken);
            await cache.RemoveAsync(StatsCacheKey, token: cancellationToken);
            // Note: Admin user list cache will expire naturally (2 min TTL)
            
            logger.LogInformation("User {UserId} role updated to {Role}", userId, role);
            return Result.Success();
        }

        return Result.Failure("Update user role failed");
    }

    public async Task<Result<PagedResult<Pet>>> GetAllPetsAsync(
        int page, int pageSize, long? userId, CancellationToken cancellationToken = default)
    {
        // For now, use simple pagination (consider adding filtered query to repository)
        var offset = (page - 1) * pageSize;
        
        // This is a simplified implementation - in production, add proper filtering in repository
        List<Pet> pets;
        if (userId.HasValue)
        {
            pets = await petRepository.GetByUserIdAsync(userId.Value);
            pets = pets.Skip(offset).Take(pageSize).ToList();
        }
        else
        {
            // Note: This requires adding GetPagedAsync to IPetRepository
            // For now, return empty (implement in repository if needed)
            pets = new List<Pet>();
        }
        
        return Result.Success(new PagedResult<Pet>(pets, pets.Count));
    }

    public async Task<Result> DeletePetAsync(long petId, CancellationToken cancellationToken = default)
    {
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            logger.LogWarning("Pet {PetId} not found for deletion", petId);
            return Result.Failure("Pet not found");
        }

        var affectedRows = await petRepository.DeleteAsync(petId);
        if (affectedRows > 0)
        {
            // Invalidate caches
            await cache.RemoveAsync($"pet:{petId}", token: cancellationToken);
            await cache.RemoveAsync($"user:pets:{pet.UserId}", token: cancellationToken);
            
            logger.LogInformation("Pet {PetId} deleted by admin", petId);
            return Result.Success();
        }

        return Result.Failure("Delete pet failed");
    }

    public async Task<Result<ServicePackage>> CreatePackageAsync(CreatePackageCommand command, CancellationToken cancellationToken = default)
    {
        var package = new ServicePackage
        {
            Id = YitIdHelper.NextId(),
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Duration = command.Duration,
            IconUrl = command.IconUrl,
            ServiceItems = command.ServiceItems,
            IsActive = true,
            SortOrder = command.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        var affectedRows = await packageRepository.CreateAsync(package);
        if (affectedRows > 0)
        {
            // Add to Bloom Filter
            bloomFilter.AddPackage(package.Id);
            
            // Invalidate cache
            await cache.RemoveAsync(ActivePackagesCacheKey, token: cancellationToken);
            await cache.RemoveAsync("packages:count", token: cancellationToken);
            
            logger.LogInformation("Service package {PackageId} created", package.Id);
            return Result.Success(package);
        }

        return Result.Failure<ServicePackage>("Create service package failed");
    }

    public async Task<Result> UpdatePackageAsync(UpdatePackageCommand command, CancellationToken cancellationToken = default)
    {
        var package = await packageRepository.GetByIdAsync(command.Id);
        if (package == null)
        {
            logger.LogWarning("Package {PackageId} not found for update", command.Id);
            return Result.Failure("Service package not found");
        }

        package.Name = command.Name;
        package.Description = command.Description;
        package.Price = command.Price;
        package.Duration = command.Duration;
        package.IconUrl = command.IconUrl;
        package.ServiceItems = command.ServiceItems;
        package.IsActive = command.IsActive;
        package.SortOrder = command.SortOrder;
        package.UpdatedAt = DateTime.UtcNow;

        var affectedRows = await packageRepository.UpdateAsync(package);
        if (affectedRows > 0)
        {
            // Invalidate caches
            await cache.RemoveAsync($"package:{package.Id}", token: cancellationToken);
            await cache.RemoveAsync(ActivePackagesCacheKey, token: cancellationToken);
            
            logger.LogInformation("Service package {PackageId} updated", package.Id);
            return Result.Success();
        }

        return Result.Failure("Update service package failed");
    }

    public async Task<Result> DeletePackageAsync(long packageId, CancellationToken cancellationToken = default)
    {
        var package = await packageRepository.GetByIdAsync(packageId);
        if (package == null)
        {
            logger.LogWarning("Package {PackageId} not found for deletion", packageId);
            return Result.Failure("Service package not found");
        }

        var affectedRows = await packageRepository.DeleteAsync(packageId);
        if (affectedRows > 0)
        {
            // Invalidate caches
            await cache.RemoveAsync($"package:{packageId}", token: cancellationToken);
            await cache.RemoveAsync(ActivePackagesCacheKey, token: cancellationToken);
            await cache.RemoveAsync("packages:count", token: cancellationToken);
            
            logger.LogInformation("Service package {PackageId} deleted", packageId);
            return Result.Success();
        }

        return Result.Failure("Delete service package failed");
    }

    public async Task<Result> TogglePackageStatusAsync(long packageId, bool isActive, CancellationToken cancellationToken = default)
    {
        var package = await packageRepository.GetByIdAsync(packageId);
        if (package == null)
        {
            logger.LogWarning("Package {PackageId} not found for status toggle", packageId);
            return Result.Failure("Service package not found");
        }

        package.IsActive = isActive;
        package.UpdatedAt = DateTime.UtcNow;

        var affectedRows = await packageRepository.UpdateAsync(package);
        if (affectedRows > 0)
        {
            // Invalidate caches
            await cache.RemoveAsync($"package:{packageId}", token: cancellationToken);
            await cache.RemoveAsync(ActivePackagesCacheKey, token: cancellationToken);
            
            logger.LogInformation("Service package {PackageId} status toggled to {IsActive}", packageId, isActive);
            return Result.Success();
        }

        return Result.Failure("Toggle service package status failed");
    }

    public async Task<Result<AdminStatistics>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await cache.GetOrSetAsync<AdminStatistics>(
            StatsCacheKey,
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for admin statistics, fetching from DB");
                
                var totalUsers = await userRepository.GetCountAsync();
                var totalPets = (await petRepository.GetAllIdsAsync()).Count;
                var totalOrders = (await orderRepository.GetAllIdsAsync()).Count;
                var totalPackages = await packageRepository.GetCountAsync();
                
                // For active users, we'd need to add a query (simplified here)
                var activeUsers = totalUsers; // TODO: Add proper query
                
                // For pending orders, we'd need to add a filtered query
                var pendingOrders = 0; // TODO: Add proper query
                
                // For active packages
                var activePackages = (await packageRepository.GetActivePackagesAsync()).Count;
                
                return new AdminStatistics(
                    totalUsers,
                    activeUsers,
                    totalPets,
                    totalOrders,
                    pendingOrders,
                    totalPackages,
                    activePackages);
            },
            options => options.SetDuration(TimeSpan.FromMinutes(5)),
            cancellationToken);

        return Result.Success(stats);
    }
}

