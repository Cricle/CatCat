using CatCat.Transit.Messages;
using CatCat.Transit.StateMachine;
using Microsoft.Extensions.Logging.Abstractions;

namespace CatCat.Transit.Tests.StateMachine;

public class StateMachineTests
{
    [Fact]
    public async Task StateMachine_ValidTransition_ShouldSucceed()
    {
        // Arrange
        var logger = NullLogger<OrderStateMachine>.Instance;
        var stateMachine = new OrderStateMachine(logger);

        // Act
        var result = await stateMachine.FireAsync(new OrderPlacedEvent());

        // Assert
        result.IsSuccess.Should().BeTrue();
        stateMachine.CurrentState.Should().Be(OrderState.Processing);
    }

    [Fact]
    public async Task StateMachine_InvalidTransition_ShouldFail()
    {
        // Arrange
        var logger = NullLogger<OrderStateMachine>.Instance;
        var stateMachine = new OrderStateMachine(logger);

        // Act - try to complete without placing order first
        var result = await stateMachine.FireAsync(new OrderCompletedEvent());

        // Assert
        result.IsSuccess.Should().BeFalse();
        stateMachine.CurrentState.Should().Be(OrderState.New);
    }

    [Fact]
    public async Task StateMachine_CompleteWorkflow_ShouldTransitionCorrectly()
    {
        // Arrange
        var logger = NullLogger<OrderStateMachine>.Instance;
        var stateMachine = new OrderStateMachine(logger);

        // Act & Assert
        stateMachine.CurrentState.Should().Be(OrderState.New);

        await stateMachine.FireAsync(new OrderPlacedEvent());
        stateMachine.CurrentState.Should().Be(OrderState.Processing);
        stateMachine.Data.OrderPlaced.Should().BeTrue();

        await stateMachine.FireAsync(new OrderShippedEvent());
        stateMachine.CurrentState.Should().Be(OrderState.Shipped);
        stateMachine.Data.OrderShipped.Should().BeTrue();

        await stateMachine.FireAsync(new OrderCompletedEvent());
        stateMachine.CurrentState.Should().Be(OrderState.Completed);
        stateMachine.Data.OrderCompleted.Should().BeTrue();
    }
}

// Test State Machine
public enum OrderState
{
    New,
    Processing,
    Shipped,
    Completed,
    Cancelled
}

public class OrderStateMachineData
{
    public bool OrderPlaced { get; set; }
    public bool OrderShipped { get; set; }
    public bool OrderCompleted { get; set; }
}

public class OrderStateMachine : StateMachineBase<OrderState, OrderStateMachineData>
{
    public OrderStateMachine(Microsoft.Extensions.Logging.ILogger<OrderStateMachine> logger) : base(logger)
    {
        CurrentState = OrderState.New;

        // Configure transitions
        ConfigureTransition<OrderPlacedEvent>(OrderState.New, async (@event) =>
        {
            Data.OrderPlaced = true;
            return OrderState.Processing;
        });

        ConfigureTransition<OrderShippedEvent>(OrderState.Processing, async (@event) =>
        {
            Data.OrderShipped = true;
            return OrderState.Shipped;
        });

        ConfigureTransition<OrderCompletedEvent>(OrderState.Shipped, async (@event) =>
        {
            Data.OrderCompleted = true;
            return OrderState.Completed;
        });
    }
}

// Test Events
public record OrderPlacedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record OrderShippedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record OrderCompletedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

