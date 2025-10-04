using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatCat.Transit.Saga;

/// <summary>
/// 内存 Saga 仓储（用于开发和测试）
/// </summary>
public class InMemorySagaRepository : ISagaRepository
{
    private readonly ConcurrentDictionary<Guid, string> _sagas = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public InMemorySagaRepository()
    {
        // For development/testing, use reflection-based serialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc/>
    public Task SaveAsync(ISaga saga, CancellationToken cancellationToken = default)
    {
        saga.Version++;
        saga.UpdatedAt = DateTime.UtcNow;
        
        var json = JsonSerializer.Serialize(saga, saga.GetType(), _jsonOptions);
        _sagas[saga.CorrelationId] = json;
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<ISaga?> GetAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        if (_sagas.TryGetValue(correlationId, out var json))
        {
            // Note: 实际实现需要存储类型信息
            return Task.FromResult<ISaga?>(JsonSerializer.Deserialize<ISaga>(json, _jsonOptions));
        }
        
        return Task.FromResult<ISaga?>(null);
    }

    /// <inheritdoc/>
    public Task<ISaga<TData>?> GetAsync<TData>(Guid correlationId, CancellationToken cancellationToken = default) 
        where TData : class, new()
    {
        if (_sagas.TryGetValue(correlationId, out var json))
        {
            var saga = JsonSerializer.Deserialize<SagaBase<TData>>(json, _jsonOptions);
            return Task.FromResult<ISaga<TData>?>(saga);
        }
        
        return Task.FromResult<ISaga<TData>?>(null);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        _sagas.TryRemove(correlationId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<ISaga>> QueryByStateAsync(SagaState state, CancellationToken cancellationToken = default)
    {
        // Note: 简化实现，实际需要反序列化所有 Saga 并筛选
        return Task.FromResult(Enumerable.Empty<ISaga>());
    }
}

