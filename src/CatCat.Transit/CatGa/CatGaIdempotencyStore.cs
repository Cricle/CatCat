using System.Collections.Concurrent;

namespace CatCat.Transit.CatGa;

/// <summary>
/// CatGa 幂等性存储 - 极致性能
/// 使用分片 + Bloom Filter（未来优化）
/// </summary>
public sealed class CatGaIdempotencyStore
{
    private readonly ConcurrentDictionary<string, (DateTime ExpireAt, object? Result)>[] _shards;
    private readonly int _shardCount;
    private readonly TimeSpan _expiry;
    private readonly Timer _cleanupTimer;

    public CatGaIdempotencyStore(int shardCount = 64, TimeSpan? expiry = null)
    {
        // 分片数必须是 2 的幂
        if (shardCount <= 0 || (shardCount & (shardCount - 1)) != 0)
            throw new ArgumentException("Shard count must be a power of 2", nameof(shardCount));

        _shardCount = shardCount;
        _expiry = expiry ?? TimeSpan.FromHours(1);

        _shards = new ConcurrentDictionary<string, (DateTime, object?)>[_shardCount];
        for (int i = 0; i < _shardCount; i++)
        {
            _shards[i] = new ConcurrentDictionary<string, (DateTime, object?)>();
        }

        // 定期清理过期数据（每分钟）
        _cleanupTimer = new Timer(_ => CleanupExpired(), null,
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// 标记为已处理
    /// </summary>
    public void MarkProcessed(string key)
    {
        var shard = GetShard(key);
        shard[key] = (DateTime.UtcNow.Add(_expiry), null);
    }

    /// <summary>
    /// 缓存结果
    /// </summary>
    public void CacheResult<T>(string key, T? result)
    {
        var shard = GetShard(key);
        shard[key] = (DateTime.UtcNow.Add(_expiry), result);
    }

    /// <summary>
    /// 检查是否已处理
    /// </summary>
    public bool IsProcessed(string key)
    {
        var shard = GetShard(key);
        if (shard.TryGetValue(key, out var entry))
        {
            if (entry.Item1 > DateTime.UtcNow)
                return true;

            // 过期，删除
            shard.TryRemove(key, out _);
        }
        return false;
    }

    /// <summary>
    /// 尝试获取缓存的结果
    /// </summary>
    public bool TryGetCachedResult<T>(string key, out T? result)
    {
        var shard = GetShard(key);
        if (shard.TryGetValue(key, out var entry))
        {
            if (entry.Item1 > DateTime.UtcNow && entry.Item2 is T typedResult)
            {
                result = typedResult;
                return true;
            }

            // 过期或类型不匹配
            shard.TryRemove(key, out _);
        }

        result = default;
        return false;
    }

    // 获取分片（使用位运算，比取模快）
    private ConcurrentDictionary<string, (DateTime, object?)> GetShard(string key)
    {
        var hash = key.GetHashCode();
        var index = hash & (_shardCount - 1); // 等价于 hash % _shardCount
        return _shards[index];
    }

    // 清理过期数据
    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var shard in _shards)
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
        _cleanupTimer.Dispose();
    }
}

