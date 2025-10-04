using CatCat.Transit.Tests.TestHelpers;
using CatCat.Transit.DependencyInjection;
using CatCat.Transit.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatCat.Transit.Tests;

public class TransitMediatorTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ITransitMediator _mediator;

    public TransitMediatorTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransit();
        
        // Register handlers as singleton for testing
        var commandHandler = new TestCommandHandler();
        var commandWithoutResponseHandler = new TestCommandWithoutResponseHandler();
        var queryHandler = new TestQueryHandler();
        var eventHandler = new TestEventHandler();
        
        services.AddSingleton(commandHandler);
        services.AddSingleton(commandWithoutResponseHandler);
        services.AddSingleton(queryHandler);
        services.AddSingleton(eventHandler);
        
        // Register handler interfaces to use the singleton instances
        services.AddTransient<IRequestHandler<TestCommand, string>>(_ => commandHandler);
        services.AddTransient<IRequestHandler<TestCommandWithoutResponse>>(_ => commandWithoutResponseHandler);
        services.AddTransient<IRequestHandler<TestQuery, string>>(_ => queryHandler);
        services.AddTransient<IEventHandler<TestEvent>>(_ => eventHandler);

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<ITransitMediator>();
    }

    [Fact]
    public async Task SendAsync_WithResponse_ReturnsSuccessResult()
    {
        // Arrange
        var command = new TestCommand("test data");

        // Act
        var result = await _mediator.SendAsync<TestCommand, string>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Processed: test data");
    }

    [Fact]
    public async Task SendAsync_WithoutResponse_ReturnsSuccessResult()
    {
        // Arrange
        var command = new TestCommandWithoutResponse("test");

        // Act
        var result = await _mediator.SendAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_Query_ReturnsResult()
    {
        // Arrange
        var query = new TestQuery(123);

        // Act
        var result = await _mediator.SendAsync<TestQuery, string>(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Result for ID: 123");
    }

    [Fact]
    public async Task PublishAsync_Event_InvokesHandler()
    {
        // Arrange
        var @event = new TestEvent("test message");

        // Act
        await _mediator.PublishAsync(@event);

        // Assert
        var handler = _serviceProvider.GetRequiredService<TestEventHandler>();
        handler.ExecutionCount.Should().Be(1);
        handler.ReceivedMessages.Should().Contain("test message");
    }

    [Fact]
    public async Task SendAsync_HandlerNotFound_ReturnsFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransit();
        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<ITransitMediator>();

        var command = new TestCommand("test");

        // Act
        var result = await mediator.SendAsync<TestCommand, string>(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No handler");
    }

    [Fact]
    public async Task SendAsync_MultipleRequests_ExecutedSequentially()
    {
        // Arrange
        var commands = Enumerable.Range(1, 10)
            .Select(i => new TestCommand($"data{i}"))
            .ToList();

        // Act
        var results = new List<CatCat.Transit.Results.TransitResult<string>>();
        foreach (var command in commands)
        {
            var result = await _mediator.SendAsync<TestCommand, string>(command);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        var handler = _serviceProvider.GetRequiredService<TestCommandHandler>();
        handler.ExecutionCount.Should().Be(10);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

