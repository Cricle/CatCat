namespace CatCat.Transit.CatGa;

/// <summary>
/// CatGa 配置选项
/// </summary>
public sealed class CatGaOptions
{
    /// <summary>
    /// 是否启用幂等性
    /// </summary>
    public bool IdempotencyEnabled { get; set; } = true;

    /// <summary>
    /// 幂等性分片数（必须是 2 的幂）
    /// </summary>
    public int IdempotencyShardCount { get; set; } = 64;

    /// <summary>
    /// 幂等性过期时间
    /// </summary>
    public TimeSpan IdempotencyExpiry { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// 是否自动补偿
    /// </summary>
    public bool AutoCompensate { get; set; } = true;

    /// <summary>
    /// 补偿超时时间
    /// </summary>
    public TimeSpan CompensationTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 初始重试延迟
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// 最大重试延迟
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 是否使用 Jitter
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// 预设：极致性能
    /// </summary>
    public CatGaOptions WithExtremePerformance()
    {
        IdempotencyEnabled = true;
        IdempotencyShardCount = 128; // 更多分片
        AutoCompensate = true;
        UseJitter = false; // 减少计算
        return this;
    }

    /// <summary>
    /// 预设：高可靠性
    /// </summary>
    public CatGaOptions WithHighReliability()
    {
        IdempotencyEnabled = true;
        IdempotencyExpiry = TimeSpan.FromHours(24); // 更长保留
        AutoCompensate = true;
        UseJitter = true;
        return this;
    }

    /// <summary>
    /// 预设：简化模式
    /// </summary>
    public CatGaOptions WithSimpleMode()
    {
        IdempotencyEnabled = false; // 无幂等性开销
        AutoCompensate = false; // 无补偿开销
        return this;
    }
}

