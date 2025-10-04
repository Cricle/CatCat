using CatCat.Transit.CatGa;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CatCat.Transit.Tests.CatGa;

public class CatGaTests
{
    [Fact]
    public async Task ExecuteAsync_SuccessfulTransaction_ReturnsSuccess()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatGa();
        services.AddCatGaTransaction<TestRequest, TestResponse, TestSuccessTransaction>();

        var serviceProvider = services.BuildServiceProvider();
        var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();

        var request = new TestRequest { Value = 42 };

        // Act
        var result = await executor.ExecuteAsync<TestRequest, TestResponse>(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Result.Should().Be(84);
    }

    [Fact]
    public async Task ExecuteAsync_WithIdempotencyKey_ReturnsCachedResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatGa();
        
        // Create a shared instance
        var sharedTransaction = new TestCounterTransaction();
        services.AddSingleton<ICatGaTransaction<TestRequest>>(sharedTransaction);

        var serviceProvider = services.BuildServiceProvider();
        var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();

        var request = new TestRequest { Value = 1 };
        var context = new CatGaContext { IdempotencyKey = "test-key-1" };

        // Act
        var result1 = await executor.ExecuteAsync<TestRequest>(request, context);
        var result2 = await executor.ExecuteAsync<TestRequest>(request, context);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        sharedTransaction.ExecutionCount.Should().Be(1); // Only executed once
    }

    [Fact]
    public async Task ExecuteAsync_FailedTransaction_ExecutesCompensation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatGa();
        
        // Create a shared instance
        var sharedTransaction = new TestFailingTransaction();
        services.AddSingleton<ICatGaTransaction<TestRequest, TestResponse>>(sharedTransaction);

        var serviceProvider = services.BuildServiceProvider();
        var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();

        var request = new TestRequest { Value = -1 };

        // Act
        var result = await executor.ExecuteAsync<TestRequest, TestResponse>(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsCompensated.Should().BeTrue();
        sharedTransaction.WasCompensated.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithRetry_RetriesOnFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatGa(options =>
        {
            options.MaxRetryAttempts = 3;
            options.InitialRetryDelay = TimeSpan.FromMilliseconds(10);
        });
        
        // Create a shared instance
        var sharedTransaction = new TestRetryTransaction();
        services.AddSingleton<ICatGaTransaction<TestRequest, TestResponse>>(sharedTransaction);

        var serviceProvider = services.BuildServiceProvider();
        var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();

        var request = new TestRequest { Value = 1 };

        // Act
        var result = await executor.ExecuteAsync<TestRequest, TestResponse>(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sharedTransaction.AttemptCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task ExecuteAsync_ConcurrentRequests_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCatGa();
        services.AddCatGaTransaction<TestRequest, TestResponse, TestSuccessTransaction>();

        var serviceProvider = services.BuildServiceProvider();
        var executor = serviceProvider.GetRequiredService<ICatGaExecutor>();

        // Act
        var tasks = Enumerable.Range(1, 100).Select(i =>
            executor.ExecuteAsync<TestRequest, TestResponse>(new TestRequest { Value = i })
        );

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results.Should().HaveCount(100);
    }

    // Test Models and Transactions

    public record TestRequest
    {
        public int Value { get; init; }
    }

    public record TestResponse
    {
        public int Result { get; init; }
    }

    public class TestSuccessTransaction : ICatGaTransaction<TestRequest, TestResponse>
    {
        public Task<TestResponse> ExecuteAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TestResponse { Result = request.Value * 2 });
        }

        public Task CompensateAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCounterTransaction : ICatGaTransaction<TestRequest>
    {
        public int ExecutionCount { get; private set; }

        public Task ExecuteAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }

        public Task CompensateAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestFailingTransaction : ICatGaTransaction<TestRequest, TestResponse>
    {
        public bool WasCompensated { get; private set; }

        public Task<TestResponse> ExecuteAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test failure");
        }

        public Task CompensateAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            WasCompensated = true;
            return Task.CompletedTask;
        }
    }

    public class TestRetryTransaction : ICatGaTransaction<TestRequest, TestResponse>
    {
        public int AttemptCount { get; private set; }

        public Task<TestResponse> ExecuteAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            AttemptCount++;
            if (AttemptCount < 3)
            {
                throw new InvalidOperationException("Temporary failure");
            }
            return Task.FromResult(new TestResponse { Result = request.Value });
        }

        public Task CompensateAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

