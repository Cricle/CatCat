namespace CatCat.Transit.Saga;

/// <summary>
/// Saga 接口 - 长事务编排
/// </summary>
public interface ISaga
{
    /// <summary>
    /// Saga 唯一标识
    /// </summary>
    Guid CorrelationId { get; }
    
    /// <summary>
    /// Saga 当前状态
    /// </summary>
    SagaState State { get; set; }
    
    /// <summary>
    /// Saga 数据（用于在步骤间传递数据）
    /// </summary>
    object? Data { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 版本号（用于乐观锁）
    /// </summary>
    int Version { get; set; }
}

/// <summary>
/// 泛型 Saga 接口
/// </summary>
public interface ISaga<TData> : ISaga where TData : class, new()
{
    /// <summary>
    /// 强类型 Saga 数据
    /// </summary>
    new TData Data { get; set; }
}

/// <summary>
/// Saga 状态
/// </summary>
public enum SagaState
{
    /// <summary>
    /// 新建
    /// </summary>
    New = 0,
    
    /// <summary>
    /// 执行中
    /// </summary>
    Running = 1,
    
    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// 补偿中
    /// </summary>
    Compensating = 3,
    
    /// <summary>
    /// 已补偿
    /// </summary>
    Compensated = 4,
    
    /// <summary>
    /// 失败
    /// </summary>
    Failed = 5
}

