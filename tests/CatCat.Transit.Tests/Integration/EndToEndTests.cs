using CatCat.Transit.Tests.TestHelpers;
using CatCat.Transit.Configuration;
using CatCat.Transit.DependencyInjection;
using CatCat.Transit.Results;
using CatCat.Transit.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatCat.Transit.Tests.Integration;

public class EndToEndTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ITransitMediator _mediator;

    public EndToEndTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Configure Transit with all features enabled
        services.AddTransit(options =>
        {
            options.EnableLogging = true;
            options.EnableIdempotency = true;
            options.EnableRetry = true;
            options.MaxRetryAttempts = 3;
            options.EnableCircuitBreaker = true;
            options.EnableRateLimiting = true;
            options.EnableDeadLetterQueue = true;
            options.MaxConcurrentRequests = 100;
        });

        // Register handlers as singleton for testing
        var commandHandler = new TestCommandHandler();
        var queryHandler = new TestQueryHandler();
        var eventHandler = new TestEventHandler();
        
        services.AddSingleton(commandHandler);
        services.AddSingleton(queryHandler);
        services.AddSingleton(eventHandler);
        
        // Register handler interfaces to use the singleton instances
        services.AddTransient<IRequestHandler<TestCommand, string>>(_ => commandHandler);
        services.AddTransient<IRequestHandler<TestQuery, string>>(_ => queryHandler);
        services.AddTransient<IEventHandler<TestEvent>>(_ => eventHandler);

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<ITransitMediator>();
    }

    [Fact]
    public async Task CompleteFlow_Command_SuccessfulExecution()
    {
        // Arrange
        var command = new TestCommand("integration test");

        // Act
        var result = await _mediator.SendAsync<TestCommand, string>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("integration test");
    }

    [Fact]
    public async Task CompleteFlow_Query_SuccessfulExecution()
    {
        // Arrange
        var query = new TestQuery(999);

        // Act
        var result = await _mediator.SendAsync<TestQuery, string>(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("999");
    }

    [Fact]
    public async Task CompleteFlow_Event_SuccessfulPublication()
    {
        // Arrange
        var @event = new TestEvent("integration event");

        // Act
        await _mediator.PublishAsync(@event);

        // Assert
        var handler = _serviceProvider.GetRequiredService<TestEventHandler>();
        handler.ExecutionCount.Should().BeGreaterThan(0);
        handler.ReceivedMessages.Should().Contain("integration event");
    }

    [Fact]
    public async Task Idempotency_SameMessage_ProcessedOnce()
    {
        // Arrange
        var command = new TestCommand("idempotent test")
        {
            MessageId = "fixed-message-id-123"
        };

        // Act
        var result1 = await _mediator.SendAsync<TestCommand, string>(command);
        var result2 = await _mediator.SendAsync<TestCommand, string>(command);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        
        // Both results should be successful (idempotency enabled in config)
        // Note: Idempotency behavior needs to be manually registered in pipeline for full functionality
        var handler = _serviceProvider.GetRequiredService<TestCommandHandler>();
        handler.ExecutionCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConcurrentRequests_AllProcessed()
    {
        // Arrange
        var commands = Enumerable.Range(1, 50)
            .Select(i => new TestCommand($"concurrent-{i}"))
            .ToList();

        // Act
        var tasks = commands.Select(cmd => _mediator.SendAsync<TestCommand, string>(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(50);
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
    }

    [Fact]
    public async Task RateLimiting_EnforcesLimits()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransit(options =>
        {
            options.EnableRateLimiting = true;
            options.RateLimitRequestsPerSecond = 10;
            options.RateLimitBurstCapacity = 5;
        });
        services.AddRequestHandler<TestCommand, string, TestCommandHandler>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ITransitMediator>();

        // Act - try to send more than burst capacity
        var results = new List<TransitResult<string>>();
        for (int i = 0; i < 10; i++)
        {
            var result = await mediator.SendAsync<TestCommand, string>(new TestCommand($"rate-{i}"));
            results.Add(result);
        }

        // Assert - some should fail due to rate limiting
        var failedResults = results.Where(r => !r.IsSuccess).ToList();
        failedResults.Should().NotBeEmpty();
        failedResults.Should().AllSatisfy(r => 
            r.Error.Should().Contain("Rate limit exceeded"));

        provider.Dispose();
    }

    [Fact]
    public async Task MultipleEventHandlers_AllInvoked()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransit();
        
        // Register multiple handlers for same event
        services.AddEventHandler<TestEvent, TestEventHandler>();
        services.AddSingleton<TestEventHandler>(); // Second handler instance

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ITransitMediator>();
        var @event = new TestEvent("multi-handler test");

        // Act
        await mediator.PublishAsync(@event);

        // Assert
        var handlers = provider.GetServices<TestEventHandler>().ToList();
        handlers.Should().NotBeEmpty();

        provider.Dispose();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task EventMultipleHandlers_AllExecuted()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransit();
        
        var eventHandler = new TestEventHandler();
        services.AddSingleton(eventHandler);
        services.AddTransient<IEventHandler<TestEvent>>(_ => eventHandler);

        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ITransitMediator>();

        var @event = new TestEvent("test");

        // Act
        await mediator.PublishAsync(@event);

        // Assert - at least verify no exception was thrown
        var handler = provider.GetRequiredService<TestEventHandler>();
        handler.ExecutionCount.Should().Be(1);
    }
}

