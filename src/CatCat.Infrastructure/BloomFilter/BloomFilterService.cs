using BloomFilter;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace CatCat.Infrastructure.BloomFilter;

/// <summary>
/// Bloom Filter service to prevent cache penetration
/// Uses XXHash3 for best performance (30μs per add operation)
/// </summary>
public interface IBloomFilterService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    bool MightContainUser(long userId);
    bool MightContainPet(long petId);
    bool MightContainOrder(long orderId);
    bool MightContainPackage(long packageId);
    void AddUser(long userId);
    void AddPet(long petId);
    void AddOrder(long orderId);
    void AddPackage(long packageId);
}

public class BloomFilterService(
    IUserRepository userRepository,
    IPetRepository petRepository,
    IServiceOrderRepository orderRepository,
    IServicePackageRepository packageRepository,
    ILogger<BloomFilterService> logger) : IBloomFilterService
{
    // Bloom filters with XXHash3 (fastest, 30μs per operation)
    // Error rate: 0.01 (1%), capacity based on expected data volume
    private readonly IBloomFilter _userFilter = FilterBuilder.Build(
        1_000_000,      // capacity: Support 1M users
        0.01,           // errorRate: 1% false positive rate
        HashMethod.XXHash3);

    private readonly IBloomFilter _petFilter = FilterBuilder.Build(
        5_000_000,       // capacity: Support 5M pets
        0.01,
        HashMethod.XXHash3);

    private readonly IBloomFilter _orderFilter = FilterBuilder.Build(
        10_000_000,      // capacity: Support 10M orders
        0.01,
        HashMethod.XXHash3);

    private readonly IBloomFilter _packageFilter = FilterBuilder.Build(
        10_000,          // capacity: Support 10K packages
        0.001,          // errorRate: 0.1% false positive (packages are fewer)
        HashMethod.XXHash3);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Initializing Bloom Filters...");
        var startTime = DateTime.UtcNow;

        try
        {
            // Load all existing IDs into bloom filters (parallel for better performance)
            await Task.WhenAll(
                InitializeUserFilterAsync(cancellationToken),
                InitializePetFilterAsync(cancellationToken),
                InitializeOrderFilterAsync(cancellationToken),
                InitializePackageFilterAsync(cancellationToken)
            );

            var elapsed = DateTime.UtcNow - startTime;
            logger.LogInformation(
                "Bloom Filters initialized successfully in {ElapsedMs}ms",
                elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Bloom Filters");
            throw;
        }
    }

    private async Task InitializeUserFilterAsync(CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllIdsAsync();
        foreach (var userId in users)
        {
            _userFilter.Add(userId);
        }
        logger.LogDebug("Loaded {Count} user IDs into Bloom Filter", users.Count);
    }

    private async Task InitializePetFilterAsync(CancellationToken cancellationToken)
    {
        var pets = await petRepository.GetAllIdsAsync();
        foreach (var petId in pets)
        {
            _petFilter.Add(petId);
        }
        logger.LogDebug("Loaded {Count} pet IDs into Bloom Filter", pets.Count);
    }

    private async Task InitializeOrderFilterAsync(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllIdsAsync();
        foreach (var orderId in orders)
        {
            _orderFilter.Add(orderId);
        }
        logger.LogDebug("Loaded {Count} order IDs into Bloom Filter", orders.Count);
    }

    private async Task InitializePackageFilterAsync(CancellationToken cancellationToken)
    {
        var packages = await packageRepository.GetAllIdsAsync();
        foreach (var packageId in packages)
        {
            _packageFilter.Add(packageId);
        }
        logger.LogDebug("Loaded {Count} package IDs into Bloom Filter", packages.Count);
    }

    public bool MightContainUser(long userId) => _userFilter.Contains(userId);
    public bool MightContainPet(long petId) => _petFilter.Contains(petId);
    public bool MightContainOrder(long orderId) => _orderFilter.Contains(orderId);
    public bool MightContainPackage(long packageId) => _packageFilter.Contains(packageId);

    public void AddUser(long userId) => _userFilter.Add(userId);
    public void AddPet(long petId) => _petFilter.Add(petId);
    public void AddOrder(long orderId) => _orderFilter.Add(orderId);
    public void AddPackage(long packageId) => _packageFilter.Add(packageId);
}

