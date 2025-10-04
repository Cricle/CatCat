using CatCat.Transit.RateLimiting;

namespace CatCat.Transit.Tests.RateLimiting;

public class TokenBucketRateLimiterTests
{
    [Fact]
    public void TryAcquire_UnderCapacity_ReturnsTrue()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 10, refillRatePerSecond: 10);

        // Act & Assert - should acquire all initial tokens
        for (int i = 0; i < 10; i++)
        {
            limiter.TryAcquire().Should().BeTrue($"token {i + 1} should be acquired");
        }

        limiter.AvailableTokens.Should().Be(0);
    }

    [Fact]
    public void TryAcquire_OverCapacity_ReturnsFalse()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 5, refillRatePerSecond: 5);

        // Act - consume all tokens
        for (int i = 0; i < 5; i++)
        {
            limiter.TryAcquire();
        }

        // Assert - next attempt should fail
        limiter.TryAcquire().Should().BeFalse();
        limiter.AvailableTokens.Should().Be(0);
    }

    [Fact]
    public async Task TryAcquire_AfterRefill_AllowsMore()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 5, refillRatePerSecond: 5);

        // Act - consume all tokens
        for (int i = 0; i < 5; i++)
        {
            limiter.TryAcquire().Should().BeTrue();
        }

        limiter.TryAcquire().Should().BeFalse();

        // Wait for refill (1 second should refill all tokens at 5 tokens/sec)
        await Task.Delay(1100);

        // Assert - should have refilled tokens
        limiter.TryAcquire().Should().BeTrue();
        limiter.AvailableTokens.Should().BeGreaterOrEqualTo(3); // Should have several tokens available
    }

    [Fact]
    public async Task TryAcquire_MultipleTokens_WorksCorrectly()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 10, refillRatePerSecond: 10);

        // Act & Assert - acquire 3 tokens at once
        limiter.TryAcquire(3).Should().BeTrue();
        limiter.AvailableTokens.Should().Be(7);

        // Acquire 7 more
        limiter.TryAcquire(7).Should().BeTrue();
        limiter.AvailableTokens.Should().Be(0);

        // Try to acquire 1 more - should fail
        limiter.TryAcquire(1).Should().BeFalse();
    }

    [Fact]
    public async Task WaitForTokenAsync_EventuallySucceeds()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 2, refillRatePerSecond: 5);

        // Consume all tokens
        limiter.TryAcquire(2).Should().BeTrue();

        // Act - wait for token with sufficient timeout (need at least 1 second for refill)
        var success = await limiter.WaitForTokenAsync(
            tokens: 1,
            timeout: TimeSpan.FromMilliseconds(1500));

        // Assert
        success.Should().BeTrue();
    }

    [Fact]
    public async Task WaitForTokenAsync_Timeout_ReturnsFalse()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 1, refillRatePerSecond: 1);

        // Consume the token
        limiter.TryAcquire().Should().BeTrue();

        // Act - wait with very short timeout (not enough time to refill)
        var success = await limiter.WaitForTokenAsync(
            tokens: 1,
            timeout: TimeSpan.FromMilliseconds(50));

        // Assert
        success.Should().BeFalse();
    }

    [Fact]
    public async Task Parallel_TryAcquire_ThreadSafe()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 50, refillRatePerSecond: 100);
        var successCount = 0;

        // Act - try to acquire from multiple threads
        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            if (limiter.TryAcquire())
            {
                Interlocked.Increment(ref successCount);
            }
        }));

        await Task.WhenAll(tasks);

        // Assert - should allow exactly capacity (50) acquisitions
        successCount.Should().BeLessOrEqualTo(50);
        successCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RefillRate_WorksAsExpected()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 10, refillRatePerSecond: 10);

        // Consume all tokens
        for (int i = 0; i < 10; i++)
        {
            limiter.TryAcquire();
        }

        limiter.AvailableTokens.Should().Be(0);

        // Wait for 1 second (should refill 10 tokens, but cap at capacity)
        await Task.Delay(1100);

        // Assert - should be back at capacity
        var availableTokens = limiter.AvailableTokens;
        availableTokens.Should().BeGreaterOrEqualTo(9); // Allow some timing variance
        availableTokens.Should().BeLessOrEqualTo(10); // Should not exceed capacity
    }

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsException()
    {
        // Act & Assert
        Action act = () => new TokenBucketRateLimiter(capacity: 0, refillRatePerSecond: 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_NegativeCapacity_ThrowsException()
    {
        // Act & Assert
        Action act = () => new TokenBucketRateLimiter(capacity: -1, refillRatePerSecond: 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ZeroRefillRate_ThrowsException()
    {
        // Act & Assert
        Action act = () => new TokenBucketRateLimiter(capacity: 10, refillRatePerSecond: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_NegativeRefillRate_ThrowsException()
    {
        // Act & Assert
        Action act = () => new TokenBucketRateLimiter(capacity: 10, refillRatePerSecond: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AvailableTokens_ReflectsCurrentState()
    {
        // Arrange
        var limiter = new TokenBucketRateLimiter(capacity: 5, refillRatePerSecond: 10);

        // Act & Assert - initially at capacity
        limiter.AvailableTokens.Should().Be(5);

        // Acquire some tokens
        limiter.TryAcquire(2);
        limiter.AvailableTokens.Should().Be(3);

        // Acquire more
        limiter.TryAcquire(3);
        limiter.AvailableTokens.Should().Be(0);
    }
}

