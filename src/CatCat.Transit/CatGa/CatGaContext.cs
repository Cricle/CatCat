namespace CatCat.Transit.CatGa;

/// <summary>
/// CatGa 事务上下文 - 轻量级，只包含必要信息
/// </summary>
public sealed class CatGaContext
{
    /// <summary>
    /// 事务 ID（唯一标识）
    /// </summary>
    public string TransactionId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联 ID（用于追踪相关事务）
    /// </summary>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 幂等性键（用于去重）
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 超时时间
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// 当前重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 是否需要补偿
    /// </summary>
    public bool NeedsCompensation { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

