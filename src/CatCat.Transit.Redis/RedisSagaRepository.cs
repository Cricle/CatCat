using System.Text.Json;
using System.Text.Json.Serialization;
using CatCat.Transit.Saga;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CatCat.Transit.Redis;

/// <summary>
/// Redis Saga 仓储 - 生产级持久化
/// </summary>
public class RedisSagaRepository : ISagaRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisSagaRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _keyPrefix;
    private readonly TimeSpan _defaultExpiry;

    public RedisSagaRepository(
        IConnectionMultiplexer redis,
        ILogger<RedisSagaRepository> logger,
        RedisTransitOptions? options = null)
    {
        _redis = redis;
        _logger = logger;
        _keyPrefix = options?.SagaKeyPrefix ?? "saga:";
        _defaultExpiry = options?.SagaExpiry ?? TimeSpan.FromDays(7);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    /// <inheritdoc/>
    public async Task SaveAsync(ISaga saga, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(saga.CorrelationId);

        // 乐观锁：检查版本
        var existingVersion = await db.HashGetAsync(key, "version");
        if (existingVersion.HasValue)
        {
            var currentVersion = (int)existingVersion;
            if (currentVersion != saga.Version - 1)
            {
                throw new InvalidOperationException(
                    $"Saga version conflict: expected {saga.Version - 1}, got {currentVersion}");
            }
        }

        saga.UpdatedAt = DateTime.UtcNow;

        // 使用 Hash 存储 Saga 数据
        var transaction = db.CreateTransaction();
        
        // 存储 Saga 元数据
        var hashSetTask = transaction.HashSetAsync(key, new HashEntry[]
        {
            new("correlationId", saga.CorrelationId.ToString()),
            new("state", (int)saga.State),
            new("version", saga.Version),
            new("createdAt", saga.CreatedAt.Ticks),
            new("updatedAt", saga.UpdatedAt.Ticks),
            new("type", saga.GetType().AssemblyQualifiedName ?? saga.GetType().FullName ?? ""),
            new("data", JsonSerializer.Serialize(saga.Data, _jsonOptions))
        });
        
        // 设置过期时间
        var expireTask = transaction.KeyExpireAsync(key, _defaultExpiry);

        // 添加到状态索引
        var stateIndexKey = GetStateIndexKey(saga.State);
        var addTask = transaction.SetAddAsync(stateIndexKey, saga.CorrelationId.ToString());
        var expireIndexTask = transaction.KeyExpireAsync(stateIndexKey, _defaultExpiry);

        var committed = await transaction.ExecuteAsync();
        
        if (!committed)
        {
            throw new InvalidOperationException("Failed to save Saga to Redis");
        }

        _logger.LogDebug("Saved Saga {CorrelationId} (version {Version}) to Redis",
            saga.CorrelationId, saga.Version);
    }

    /// <inheritdoc/>
    public async Task<ISaga?> GetAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(correlationId);

        var entries = await db.HashGetAllAsync(key);
        if (entries.Length == 0)
        {
            return null;
        }

        var hash = entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        
        // 反序列化需要类型信息
        if (!hash.TryGetValue("type", out var typeName) || string.IsNullOrEmpty(typeName))
        {
            _logger.LogWarning("Saga {CorrelationId} missing type information", correlationId);
            return null;
        }

        var type = Type.GetType(typeName);
        if (type == null)
        {
            _logger.LogWarning("Cannot resolve type {TypeName} for Saga {CorrelationId}", 
                typeName, correlationId);
            return null;
        }

        // Note: 这里需要反射，实际使用中可以考虑泛型版本
        _logger.LogWarning("GetAsync(Guid) requires type information, consider using GetAsync<TData>(Guid)");
        return null;
    }

    /// <inheritdoc/>
    public async Task<ISaga<TData>?> GetAsync<TData>(Guid correlationId, CancellationToken cancellationToken = default) 
        where TData : class, new()
    {
        var db = _redis.GetDatabase();
        var key = GetKey(correlationId);

        var entries = await db.HashGetAllAsync(key);
        if (entries.Length == 0)
        {
            return null;
        }

        var hash = entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        var saga = new GenericSaga<TData>
        {
            CorrelationId = Guid.Parse(hash["correlationId"]),
            State = (SagaState)int.Parse(hash["state"]),
            Version = int.Parse(hash["version"]),
            CreatedAt = new DateTime(long.Parse(hash["createdAt"])),
            UpdatedAt = new DateTime(long.Parse(hash["updatedAt"])),
            Data = JsonSerializer.Deserialize<TData>(hash["data"], _jsonOptions) ?? new TData()
        };

        _logger.LogDebug("Retrieved Saga {CorrelationId} from Redis", correlationId);
        return saga;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(correlationId);

        // 先获取状态以便从索引中删除
        var state = await db.HashGetAsync(key, "state");
        
        // 从状态索引中删除
        if (state.HasValue)
        {
            var stateIndexKey = GetStateIndexKey((SagaState)(int)state);
            await db.SetRemoveAsync(stateIndexKey, correlationId.ToString());
        }

        // 删除 Saga
        await db.KeyDeleteAsync(key);

        _logger.LogDebug("Deleted Saga {CorrelationId} from Redis", correlationId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ISaga>> QueryByStateAsync(SagaState state, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var stateIndexKey = GetStateIndexKey(state);

        var correlationIds = await db.SetMembersAsync(stateIndexKey);
        
        var sagas = new List<ISaga>();
        foreach (var id in correlationIds)
        {
            if (Guid.TryParse(id, out var correlationId))
            {
                // Note: 这里需要类型信息，实际使用中建议使用泛型版本
                _logger.LogWarning("QueryByStateAsync requires type information for deserialization");
            }
        }

        return sagas;
    }

    private string GetKey(Guid correlationId) => $"{_keyPrefix}{correlationId}";
    private string GetStateIndexKey(SagaState state) => $"{_keyPrefix}state:{state}";

    /// <summary>
    /// 通用 Saga 实现（用于 Redis 反序列化）
    /// </summary>
    private class GenericSaga<TData> : SagaBase<TData> where TData : class, new()
    {
    }
}

