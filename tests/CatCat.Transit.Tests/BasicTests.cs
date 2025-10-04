using CatCat.Transit.Tests.TestHelpers;
using CatCat.Transit.DependencyInjection;
using CatCat.Transit.Results;
using CatCat.Transit.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatCat.Transit.Tests;

/// <summary>
/// Basic integration tests for CatCat.Transit core functionality
/// </summary>
public class BasicTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ITransitMediator _mediator;

    public BasicTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransit();
        
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
    public async Task SendAsync_Command_Success()
    {
        // Arrange
        var command = new TestCommand("test");

        // Act
        var result = await _mediator.SendAsync<TestCommand, string>(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("test");
    }

    [Fact]
    public async Task SendAsync_Query_Success()
    {
        // Arrange
        var query = new TestQuery(123);

        // Act
        var result = await _mediator.SendAsync<TestQuery, string>(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("123");
    }

    [Fact]
    public async Task PublishAsync_Event_Success()
    {
        // Arrange
        var @event = new TestEvent("test message");

        // Act
        await _mediator.PublishAsync(@event);

        // Assert
        var handler = _serviceProvider.GetRequiredService<TestEventHandler>();
        handler.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public void TransitResult_Success_CreatesCorrectly()
    {
        // Act
        var result = TransitResult<string>.Success("value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void TransitResult_Failure_CreatesCorrectly()
    {
        // Act
        var result = TransitResult<string>.Failure("error");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

