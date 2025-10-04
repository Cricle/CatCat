using CatCat.Transit.Results;

namespace CatCat.Transit.Saga;

/// <summary>
/// Saga 步骤接口
/// </summary>
public interface ISagaStep<TData> where TData : class, new()
{
    /// <summary>
    /// 步骤名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 执行步骤
    /// </summary>
    Task<TransitResult> ExecuteAsync(ISaga<TData> saga, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 补偿步骤（回滚）
    /// </summary>
    Task<TransitResult> CompensateAsync(ISaga<TData> saga, CancellationToken cancellationToken = default);
}

/// <summary>
/// Saga 步骤基类
/// </summary>
public abstract class SagaStepBase<TData> : ISagaStep<TData> where TData : class, new()
{
    /// <inheritdoc/>
    public virtual string Name => GetType().Name;
    
    /// <inheritdoc/>
    public abstract Task<TransitResult> ExecuteAsync(ISaga<TData> saga, CancellationToken cancellationToken = default);
    
    /// <inheritdoc/>
    public abstract Task<TransitResult> CompensateAsync(ISaga<TData> saga, CancellationToken cancellationToken = default);
}

