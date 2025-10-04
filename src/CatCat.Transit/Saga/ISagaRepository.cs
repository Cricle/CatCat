namespace CatCat.Transit.Saga;

/// <summary>
/// Saga 持久化仓储接口
/// </summary>
public interface ISagaRepository
{
    /// <summary>
    /// 保存 Saga
    /// </summary>
    Task SaveAsync(ISaga saga, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据 CorrelationId 获取 Saga
    /// </summary>
    Task<ISaga?> GetAsync(Guid correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据 CorrelationId 获取强类型 Saga
    /// </summary>
    Task<ISaga<TData>?> GetAsync<TData>(Guid correlationId, CancellationToken cancellationToken = default) 
        where TData : class, new();
    
    /// <summary>
    /// 删除 Saga
    /// </summary>
    Task DeleteAsync(Guid correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 查询处于特定状态的 Saga
    /// </summary>
    Task<IEnumerable<ISaga>> QueryByStateAsync(SagaState state, CancellationToken cancellationToken = default);
}

