using CatCat.Transit.Idempotency;
using CatCat.Transit.Results;

namespace CatCat.Transit.Tests.Idempotency;

public class IdempotencyTests
{
    [Fact]
    public async Task MarkAsProcessedAsync_StoresResult()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 4, retentionPeriod: TimeSpan.FromHours(1));
        var messageId = Guid.NewGuid().ToString();

        // Act
        await store.MarkAsProcessedAsync(messageId, "test result");

        // Assert
        var result = await store.GetCachedResultAsync<string>(messageId);
        result.Should().NotBeNull();
        result.Should().Be("test result");
    }

    [Fact]
    public async Task GetCachedResultAsync_NotProcessed_ReturnsNull()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 4, retentionPeriod: TimeSpan.FromHours(1));
        var messageId = Guid.NewGuid().ToString();

        // Act
        var result = await store.GetCachedResultAsync<string>(messageId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCachedResultAsync_ReturnsStoredResult()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 4, retentionPeriod: TimeSpan.FromHours(1));
        var messageId = Guid.NewGuid().ToString();
        var expectedValue = "cached value";

        // Act
        await store.MarkAsProcessedAsync(messageId, expectedValue);
        var cachedResult = await store.GetCachedResultAsync<string>(messageId);

        // Assert
        cachedResult.Should().NotBeNull();
        cachedResult.Should().Be(expectedValue);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_WithComplexObject_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 4, retentionPeriod: TimeSpan.FromHours(1));
        var messageId = Guid.NewGuid().ToString();
        var complexResult = new TestResult
        {
            Id = 123,
            Name = "Test",
            Values = new List<int> { 1, 2, 3 }
        };

        // Act
        await store.MarkAsProcessedAsync(messageId, complexResult);
        var cachedResult = await store.GetCachedResultAsync<TestResult>(messageId);

        // Assert
        cachedResult.Should().NotBeNull();
        cachedResult!.Id.Should().Be(123);
        cachedResult.Name.Should().Be("Test");
        cachedResult.Values.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task ShardedStore_DistributesAcrossShards()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 8, retentionPeriod: TimeSpan.FromHours(1));
        var messageIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid().ToString()).ToList();

        // Act
        foreach (var messageId in messageIds)
        {
            await store.MarkAsProcessedAsync(messageId, $"result-{messageId}");
        }

        // Assert - all should be retrievable
        foreach (var messageId in messageIds)
        {
            var result = await store.GetCachedResultAsync<string>(messageId);
            result.Should().NotBeNull();
            result.Should().Be($"result-{messageId}");
        }
    }

    [Fact]
    public async Task Parallel_MarkAsProcessed_ThreadSafe()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 4, retentionPeriod: TimeSpan.FromHours(1));
        var messageIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid().ToString()).ToList();

        // Act
        var tasks = messageIds.Select(id => store.MarkAsProcessedAsync(id, $"result-{id}"));
        await Task.WhenAll(tasks);

        // Assert
        foreach (var messageId in messageIds)
        {
            var result = await store.GetCachedResultAsync<string>(messageId);
            result.Should().NotBeNull();
            result.Should().Be($"result-{messageId}");
        }
    }

    [Fact]
    public async Task ExpiredEntries_AreCleanedUp()
    {
        // Arrange
        var store = new ShardedIdempotencyStore(shardCount: 2, retentionPeriod: TimeSpan.FromMilliseconds(100));
        var messageId = Guid.NewGuid().ToString();

        // Act
        await store.MarkAsProcessedAsync(messageId, "result");
        
        // Verify it's there
        var resultBefore = await store.GetCachedResultAsync<string>(messageId);
        resultBefore.Should().NotBeNull();

        // Wait for expiration
        await Task.Delay(150);

        // Assert - GetCachedResultAsync checks expiry and removes expired entries
        var resultAfter = await store.GetCachedResultAsync<string>(messageId);
        resultAfter.Should().BeNull(); // Should be cleaned up on access
    }

    [Fact]
    public void Constructor_InvalidShardCount_ThrowsException()
    {
        // Act & Assert - shard count must be power of 2
        Action act = () => new ShardedIdempotencyStore(shardCount: 3, retentionPeriod: TimeSpan.FromHours(1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ValidShardCount_Succeeds()
    {
        // Act & Assert - powers of 2 should work
        var store1 = new ShardedIdempotencyStore(shardCount: 2);
        var store2 = new ShardedIdempotencyStore(shardCount: 4);
        var store3 = new ShardedIdempotencyStore(shardCount: 16);
        var store4 = new ShardedIdempotencyStore(shardCount: 32);

        store1.Should().NotBeNull();
        store2.Should().NotBeNull();
        store3.Should().NotBeNull();
        store4.Should().NotBeNull();
    }

    [Fact]
    public async Task MultipleStores_AreIndependent()
    {
        // Arrange
        var store1 = new ShardedIdempotencyStore(shardCount: 4);
        var store2 = new ShardedIdempotencyStore(shardCount: 4);
        var messageId = Guid.NewGuid().ToString();

        // Act
        await store1.MarkAsProcessedAsync(messageId, "store1-result");
        await store2.MarkAsProcessedAsync(messageId, "store2-result");

        // Assert
        var result1 = await store1.GetCachedResultAsync<string>(messageId);
        var result2 = await store2.GetCachedResultAsync<string>(messageId);

        result1.Should().Be("store1-result");
        result2.Should().Be("store2-result");
    }

    // Helper class for testing complex objects
    private class TestResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> Values { get; set; } = new();
    }
}

