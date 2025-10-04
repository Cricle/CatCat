using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Cache;

/// <summary>
/// 缓存配置（防击穿、防雪崩、防穿透）
/// </summary>
public static class CacheConfiguration
{
    /// <summary>
    /// 用户缓存配置
    /// - 过期时间: 10分钟
    /// - 软超时: 8分钟（背景刷新）
    /// - 防击穿: Fail-Safe 模式
    /// </summary>
    public static FusionCacheEntryOptions UserCacheOptions => new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(10),
        
        // 防击穿：Fail-Safe 模式
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(1),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(5),
        
        // 背景刷新（防止缓存过期导致大量数据库查询）
        EagerRefreshThreshold = 0.8f, // 剩余20%时触发背景刷新
        
        // Factory 超时
        FactoryHardTimeout = TimeSpan.FromSeconds(5),
        FactorySoftTimeout = TimeSpan.FromSeconds(3),
        
        // 允许后台刷新
        AllowBackgroundBackplaneOperations = true,
        AllowBackgroundDistributedCacheOperations = true
    };

    /// <summary>
    /// 服务套餐缓存配置
    /// - 过期时间: 1小时
    /// - 软超时: 50分钟
    /// - 长期缓存（数据变化少）
    /// </summary>
    public static FusionCacheEntryOptions ServicePackageCacheOptions => new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromHours(1),
        
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(6),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(10),
        
        EagerRefreshThreshold = 0.8f,
        
        FactoryHardTimeout = TimeSpan.FromSeconds(5),
        FactorySoftTimeout = TimeSpan.FromSeconds(3),
        
        AllowBackgroundBackplaneOperations = true,
        AllowBackgroundDistributedCacheOperations = true
    };

    /// <summary>
    /// 订单缓存配置
    /// - 过期时间: 5分钟
    /// - 短期缓存（数据变化频繁）
    /// </summary>
    public static FusionCacheEntryOptions OrderCacheOptions => new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(5),
        
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromMinutes(30),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(3),
        
        EagerRefreshThreshold = 0.8f,
        
        FactoryHardTimeout = TimeSpan.FromSeconds(3),
        FactorySoftTimeout = TimeSpan.FromSeconds(2),
        
        AllowBackgroundBackplaneOperations = true,
        AllowBackgroundDistributedCacheOperations = true
    };

    /// <summary>
    /// 宠物信息缓存配置
    /// - 过期时间: 30分钟
    /// - 中期缓存
    /// </summary>
    public static FusionCacheEntryOptions PetCacheOptions => new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(30),
        
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(2),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(5),
        
        EagerRefreshThreshold = 0.8f,
        
        FactoryHardTimeout = TimeSpan.FromSeconds(5),
        FactorySoftTimeout = TimeSpan.FromSeconds(3),
        
        AllowBackgroundBackplaneOperations = true,
        AllowBackgroundDistributedCacheOperations = true
    };

    /// <summary>
    /// 评价缓存配置
    /// - 过期时间: 15分钟
    /// </summary>
    public static FusionCacheEntryOptions ReviewCacheOptions => new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(15),
        
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(1),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(5),
        
        EagerRefreshThreshold = 0.8f,
        
        FactoryHardTimeout = TimeSpan.FromSeconds(5),
        FactorySoftTimeout = TimeSpan.FromSeconds(3),
        
        AllowBackgroundBackplaneOperations = true,
        AllowBackgroundDistributedCacheOperations = true
    };

    /// <summary>
    /// 获取缓存 Key 前缀
    /// </summary>
    public static string GetCacheKey(string prefix, params object[] keys)
    {
        return $"{prefix}:{string.Join(":", keys)}";
    }

    // 缓存 Key 前缀常量
    public const string UserPrefix = "user";
    public const string ServicePackagePrefix = "package";
    public const string OrderPrefix = "order";
    public const string PetPrefix = "pet";
    public const string ReviewPrefix = "review";
    public const string BloomFilterPrefix = "bloom";
}
