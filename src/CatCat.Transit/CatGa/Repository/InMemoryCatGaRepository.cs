using System.Collections.Concurrent;
using CatCat.Transit.CatGa.Models;

namespace CatCat.Transit.CatGa.Repository;

/// <summary>
/// 内存实现的 CatGa 仓储 - 高性能分片设计
/// </summary>
public sealed class InMemoryCatGaRepository : ICatGaRepository, IDisposable
{
    private readonly ConcurrentDictionary<string, (DateTime ExpireAt, object? Result)>[] _idempotencyShards;
    private readonly ConcurrentDictionary<string, CatGaContext> _contextStore;
    private readonly int _shardCount;
    private readonly TimeSpan _expiry;
    private readonly Timer _cleanupTimer;

    public InMemoryCatGaRepository(int shardCount = 128, TimeSpan? expiry = null)
    {
        // 分片数必须是 2 的幂
        if (shardCount <= 0 || (shardCount & (shardCount - 1)) != 0)
            throw new ArgumentException("Shard count must be a power of 2", nameof(shardCount));

        _shardCount = shardCount;
        _expiry = expiry ?? TimeSpan.FromHours(1);

        // 初始化幂等性分片
        _idempotencyShards = new ConcurrentDictionary<string, (DateTime, object?)>[_shardCount];
        for (int i = 0; i < _shardCount; i++)
        {
            _idempotencyShards[i] = new ConcurrentDictionary<string, (DateTime, object?)>();
        }

        // 初始化上下文存储
        _contextStore = new ConcurrentDictionary<string, CatGaContext>();

        // 定期清理过期数据（每分钟）
        _cleanupTimer = new Timer(_ => CleanupExpired(), null,
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public bool IsProcessed(string idempotencyKey)
    {
        var shard = GetIdempotencyShard(idempotencyKey);
        if (shard.TryGetValue(idempotencyKey, out var entry))
        {
            if (entry.Item1 > DateTime.UtcNow)
                return true;

            // 过期，删除
            shard.TryRemove(idempotencyKey, out _);
        }
        return false;
    }

    public void MarkProcessed(string idempotencyKey)
    {
        var shard = GetIdempotencyShard(idempotencyKey);
        shard[idempotencyKey] = (DateTime.UtcNow.Add(_expiry), null);
    }

    public void CacheResult<T>(string idempotencyKey, T? result)
    {
        var shard = GetIdempotencyShard(idempotencyKey);
        shard[idempotencyKey] = (DateTime.UtcNow.Add(_expiry), result);
    }

    public bool TryGetCachedResult<T>(string idempotencyKey, out T? result)
    {
        var shard = GetIdempotencyShard(idempotencyKey);
        if (shard.TryGetValue(idempotencyKey, out var entry))
        {
            if (entry.Item1 > DateTime.UtcNow && entry.Item2 is T typedResult)
            {
                result = typedResult;
                return true;
            }

            // 过期或类型不匹配
            shard.TryRemove(idempotencyKey, out _);
        }

        result = default;
        return false;
    }

    public Task SaveContextAsync<TRequest, TResponse>(
        string transactionId,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken = default)
    {
        _contextStore[transactionId] = context;
        return Task.CompletedTask;
    }

    public Task<CatGaContext?> LoadContextAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        _contextStore.TryGetValue(transactionId, out var context);
        return Task.FromResult(context);
    }

    public Task DeleteContextAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        _contextStore.TryRemove(transactionId, out _);
        return Task.CompletedTask;
    }

    private ConcurrentDictionary<string, (DateTime, object?)> GetIdempotencyShard(string key)
    {
        var hash = key.GetHashCode();
        var shardIndex = (hash & int.MaxValue) % _shardCount;
        return _idempotencyShards[shardIndex];
    }

    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;

        // 清理幂等性存储
        foreach (var shard in _idempotencyShards)
        {
            var expiredKeys = shard
                .Where(kvp => kvp.Value.Item1 <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                shard.TryRemove(key, out _);
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

