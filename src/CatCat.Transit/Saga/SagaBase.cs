namespace CatCat.Transit.Saga;

/// <summary>
/// Saga 基类
/// </summary>
public abstract class SagaBase<TData> : ISaga<TData> where TData : class, new()
{
    /// <inheritdoc/>
    public Guid CorrelationId { get; set; }
    
    /// <inheritdoc/>
    public SagaState State { get; set; }
    
    /// <inheritdoc/>
    public TData Data { get; set; } = new();
    
    /// <inheritdoc/>
    object? ISaga.Data 
    { 
        get => Data; 
        set => Data = (value as TData) ?? new TData(); 
    }
    
    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }
    
    /// <inheritdoc/>
    public DateTime UpdatedAt { get; set; }
    
    /// <inheritdoc/>
    public int Version { get; set; }

    protected SagaBase()
    {
        CorrelationId = Guid.NewGuid();
        State = SagaState.New;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Version = 0;
    }
}

