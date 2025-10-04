using System.Collections.Concurrent;
using System.Text.Json;

namespace CatCat.Transit.Idempotency;

/// <summary>
/// High-performance sharded idempotency store using ConcurrentDictionary (AOT-compatible)
/// Reduces lock contention by sharding across multiple partitions
/// </summary>
public class ShardedIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, (DateTime ProcessedAt, Type? ResultType, string? ResultJson)>[] _shards;
    private readonly TimeSpan _retentionPeriod;
    private readonly int _shardCount;
    private readonly Timer _cleanupTimer;

    public ShardedIdempotencyStore(int shardCount = 32, TimeSpan? retentionPeriod = null)
    {
        if (shardCount <= 0 || (shardCount & (shardCount - 1)) != 0)
            throw new ArgumentException("Shard count must be a power of 2", nameof(shardCount));

        _shardCount = shardCount;
        _retentionPeriod = retentionPeriod ?? TimeSpan.FromHours(24);
        _shards = new ConcurrentDictionary<string, (DateTime, Type?, string?)>[_shardCount];

        for (int i = 0; i < _shardCount; i++)
        {
            _shards[i] = new ConcurrentDictionary<string, (DateTime, Type?, string?)>();
        }

        // Background cleanup every 5 minutes
        _cleanupTimer = new Timer(_ => CleanupExpiredEntries(), null,
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    private ConcurrentDictionary<string, (DateTime, Type?, string?)> GetShard(string messageId)
    {
        // Fast hash-based sharding (bit mask for power-of-2 shard count)
        var hash = messageId.GetHashCode();
        var shardIndex = hash & (_shardCount - 1);
        return _shards[shardIndex];
    }

    public Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var shard = GetShard(messageId);
        return Task.FromResult(shard.ContainsKey(messageId));
    }

    public Task MarkAsProcessedAsync<TResult>(string messageId, TResult? result = default, CancellationToken cancellationToken = default)
    {
        var shard = GetShard(messageId);

        string? resultJson = null;
        Type? resultType = null;

        if (result != null)
        {
            resultType = typeof(TResult);
            resultJson = JsonSerializer.Serialize(result);
        }

        shard[messageId] = (DateTime.UtcNow, resultType, resultJson);
        return Task.CompletedTask;
    }

    public Task<TResult?> GetCachedResultAsync<TResult>(string messageId, CancellationToken cancellationToken = default)
    {
        var shard = GetShard(messageId);

        if (shard.TryGetValue(messageId, out var entry))
        {
            if (entry.Item3 != null && entry.Item2 == typeof(TResult))
            {
                return Task.FromResult(JsonSerializer.Deserialize<TResult>(entry.Item3));
            }
        }

        return Task.FromResult<TResult?>(default);
    }

    private void CleanupExpiredEntries()
    {
        var cutoff = DateTime.UtcNow - _retentionPeriod;

        // Parallel cleanup across shards
        Parallel.ForEach(_shards, shard =>
        {
            var expiredKeys = shard
                .Where(kvp => kvp.Value.Item1 < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                shard.TryRemove(key, out _);
            }
        });
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

