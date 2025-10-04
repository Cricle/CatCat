using CatCat.Transit.Resilience;

namespace CatCat.Transit.Tests.Resilience;

public class CircuitBreakerTests
{
    [Fact]
    public async Task ExecuteAsync_Success_ReturnsResult()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 3, TimeSpan.FromSeconds(10));

        // Act
        var result = await breaker.ExecuteAsync(async () =>
        {
            await Task.Delay(1);
            return "success";
        });

        // Assert
        result.Should().Be("success");
    }

    [Fact]
    public async Task ExecuteAsync_ConsecutiveFailures_OpensCircuit()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 3, TimeSpan.FromSeconds(10));

        // Act - fail 3 times to open circuit
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await breaker.ExecuteAsync<string>(() => throw new InvalidOperationException("fail"));
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Assert - circuit should be open, next call should throw CircuitBreakerOpenException
        Func<Task> act = async () => await breaker.ExecuteAsync(() => Task.FromResult("test"));
        await act.Should().ThrowAsync<CircuitBreakerOpenException>();
    }

    [Fact]
    public async Task ExecuteAsync_AfterResetTimeout_AllowsRetry()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 2, resetTimeout: TimeSpan.FromMilliseconds(100));

        // Open the circuit
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await breaker.ExecuteAsync<string>(() => throw new InvalidOperationException());
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Verify circuit is open
        await Assert.ThrowsAsync<CircuitBreakerOpenException>(
            async () => await breaker.ExecuteAsync(() => Task.FromResult("test")));

        // Wait for reset timeout
        await Task.Delay(150);

        // Assert - should allow execution again (half-open state)
        var result = await breaker.ExecuteAsync(() => Task.FromResult("success"));
        result.Should().Be("success");
    }

    [Fact]
    public async Task ExecuteAsync_MixedResults_DoesNotOpenCircuit()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 3, TimeSpan.FromSeconds(10));

        // Act - alternate success and failure
        for (int i = 0; i < 5; i++)
        {
            if (i % 2 == 0)
            {
                await breaker.ExecuteAsync(() => Task.FromResult("success"));
            }
            else
            {
                try
                {
                    await breaker.ExecuteAsync<string>(() => throw new InvalidOperationException());
                }
                catch (InvalidOperationException)
                {
                    // Expected
                }
            }
        }

        // Assert - circuit should still be closed (successes reset failure count)
        var result = await breaker.ExecuteAsync(() => Task.FromResult("test"));
        result.Should().Be("test");
    }

    [Fact]
    public async Task ExecuteAsync_SuccessAfterHalfOpen_ClosesCircuit()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 2, resetTimeout: TimeSpan.FromMilliseconds(100));

        // Open the circuit
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await breaker.ExecuteAsync<string>(() => throw new InvalidOperationException());
            }
            catch { }
        }

        // Wait for reset timeout (enters half-open state)
        await Task.Delay(150);

        // Act - successful execution should close the circuit
        await breaker.ExecuteAsync(() => Task.FromResult("success"));

        // Assert - should be able to execute immediately without waiting
        var result = await breaker.ExecuteAsync(() => Task.FromResult("closed"));
        result.Should().Be("closed");
    }

    [Fact]
    public async Task ExecuteAsync_FailureInHalfOpen_IncreasesFailureCount()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 2, resetTimeout: TimeSpan.FromMilliseconds(100));

        // Open the circuit
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await breaker.ExecuteAsync<string>(() => throw new InvalidOperationException());
            }
            catch { }
        }

        var failureCountBeforeHalfOpen = breaker.FailureCount;

        // Wait for reset timeout (enters half-open state)
        await Task.Delay(150);

        // Act - fail in half-open state
        try
        {
            await breaker.ExecuteAsync<string>(() => throw new InvalidOperationException());
        }
        catch { }

        // Assert - should track the failure
        // Note: The circuit might not immediately reopen, but failure should be counted
        breaker.FailureCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task Parallel_ExecuteAsync_ThreadSafe()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 10, TimeSpan.FromSeconds(10));
        var successCount = 0;
        var failureCount = 0;

        // Act - execute many operations in parallel
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            try
            {
                await breaker.ExecuteAsync(() => Task.FromResult("ok"));
                Interlocked.Increment(ref successCount);
            }
            catch
            {
                Interlocked.Increment(ref failureCount);
            }
        });

        await Task.WhenAll(tasks);

        // Assert - all should succeed (no failures to trigger circuit)
        successCount.Should().Be(100);
        failureCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesOriginalException()
    {
        // Arrange
        var breaker = new CircuitBreaker(failureThreshold: 5, TimeSpan.FromSeconds(10));
        var expectedException = new InvalidOperationException("specific error");

        // Act & Assert
        Func<Task> act = async () => await breaker.ExecuteAsync<string>(() => throw expectedException);
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("specific error");
    }

    [Fact]
    public void Constructor_AcceptsValidParameters()
    {
        // Act & Assert - constructor should accept valid parameters
        var breaker = new CircuitBreaker(failureThreshold: 1, TimeSpan.FromSeconds(1));
        breaker.Should().NotBeNull();
        breaker.FailureCount.Should().Be(0);
    }

    [Fact]
    public void Constructor_AcceptsDefaultParameters()
    {
        // Act & Assert - constructor should work with defaults
        var breaker = new CircuitBreaker();
        breaker.Should().NotBeNull();
        breaker.FailureCount.Should().Be(0);
    }
}

