using CatCat.Transit.Concurrency;

namespace CatCat.Transit.Tests.Concurrency;

public class ConcurrencyLimiterTests
{
    [Fact]
    public async Task ExecuteAsync_UnderLimit_Succeeds()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 5);

        // Act
        var result = await limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(10);
                return "success";
            },
            timeout: TimeSpan.FromSeconds(1));

        // Assert
        result.Should().Be("success");
        limiter.CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_AtLimit_ThrowsConcurrencyLimitException()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 2);
        var task1 = limiter.ExecuteAsync(async () =>
        {
            await Task.Delay(500);
            return "task1";
        }, TimeSpan.FromSeconds(2));

        var task2 = limiter.ExecuteAsync(async () =>
        {
            await Task.Delay(500);
            return "task2";
        }, TimeSpan.FromSeconds(2));

        // Give tasks time to start
        await Task.Delay(50);

        // Act - try to execute beyond limit with short timeout
        Func<Task> act = async () => await limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(10);
                return "task3";
            },
            timeout: TimeSpan.FromMilliseconds(100));

        // Assert
        await act.Should().ThrowAsync<ConcurrencyLimitException>();

        // Cleanup
        await Task.WhenAll(task1, task2);
    }

    [Fact]
    public async Task ExecuteAsync_ReleasesSlotAfterCompletion()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 1);

        // Act & Assert - first execution
        var result1 = await limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(10);
                return "first";
            },
            timeout: TimeSpan.FromSeconds(1));
        
        result1.Should().Be("first");
        limiter.AvailableSlots.Should().Be(1);

        // Second execution should succeed after first completes
        var result2 = await limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(10);
                return "second";
            },
            timeout: TimeSpan.FromSeconds(1));

        result2.Should().Be("second");
        limiter.AvailableSlots.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_LimitsParallelExecution()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 3);
        var currentConcurrency = 0;
        var maxObservedConcurrency = 0;
        var lockObj = new object();

        async Task<int> Worker(int id)
        {
            return await limiter.ExecuteAsync(async () =>
            {
                lock (lockObj)
                {
                    currentConcurrency++;
                    maxObservedConcurrency = Math.Max(maxObservedConcurrency, currentConcurrency);
                }

                await Task.Delay(50);

                lock (lockObj)
                {
                    currentConcurrency--;
                }

                return id;
            }, TimeSpan.FromSeconds(5));
        }

        // Act
        var tasks = Enumerable.Range(0, 10).Select(i => Worker(i));
        var results = await Task.WhenAll(tasks);

        // Assert
        maxObservedConcurrency.Should().BeLessOrEqualTo(3);
        results.Should().HaveCount(10);
        limiter.AvailableSlots.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_TracksRejectedCount()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 1);
        var longRunningTask = limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(500);
                return "long";
            },
            TimeSpan.FromSeconds(2));

        await Task.Delay(50); // Ensure first task has acquired the slot

        // Act - try multiple times with short timeout
        for (int i = 0; i < 5; i++)
        {
            try
            {
                await limiter.ExecuteAsync(
                    async () =>
                    {
                        await Task.Delay(10);
                        return "rejected";
                    },
                    timeout: TimeSpan.FromMilliseconds(50));
            }
            catch (ConcurrencyLimitException)
            {
                // Expected
            }
        }

        // Assert
        limiter.RejectedCount.Should().BeGreaterThan(0);

        // Cleanup
        await longRunningTask;
    }

    [Fact]
    public async Task ExecuteAsync_ReleasesSlotOnException()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 1);

        // Act - first call throws exception
        try
        {
            await limiter.ExecuteAsync<string>(
                () => throw new InvalidOperationException("test error"),
                timeout: TimeSpan.FromSeconds(1));
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert - slot should be released
        limiter.AvailableSlots.Should().Be(1);

        // Second call should succeed
        var result = await limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(10);
                return "success";
            },
            timeout: TimeSpan.FromSeconds(1));

        result.Should().Be("success");
    }

    [Fact]
    public void Constructor_ZeroMaxConcurrency_ThrowsException()
    {
        // Act & Assert
        Action act = () => new ConcurrencyLimiter(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_NegativeMaxConcurrency_ThrowsException()
    {
        // Act & Assert
        Action act = () => new ConcurrencyLimiter(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Properties_ReflectCurrentState()
    {
        // Arrange
        var limiter = new ConcurrencyLimiter(maxConcurrency: 3);

        // Act & Assert - initially all slots available
        limiter.MaxConcurrency.Should().Be(3);
        limiter.AvailableSlots.Should().Be(3);
        limiter.CurrentCount.Should().Be(0);
        limiter.RejectedCount.Should().Be(0);

        // Start a long-running task
        var task = limiter.ExecuteAsync(
            async () =>
            {
                await Task.Delay(200);
                return "result";
            },
            TimeSpan.FromSeconds(1));

        await Task.Delay(50); // Give it time to start

        // Assert - one slot in use
        limiter.AvailableSlots.Should().Be(2);
        limiter.CurrentCount.Should().Be(1);

        // Cleanup
        await task;

        // Assert - back to initial state
        limiter.AvailableSlots.Should().Be(3);
        limiter.CurrentCount.Should().Be(0);
    }
}

