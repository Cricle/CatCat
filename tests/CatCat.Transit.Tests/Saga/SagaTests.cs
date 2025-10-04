using CatCat.Transit.Results;
using CatCat.Transit.Saga;
using Microsoft.Extensions.Logging.Abstractions;

namespace CatCat.Transit.Tests.Saga;

public class SagaTests
{
    [Fact]
    public async Task Saga_AllStepsSucceed_ShouldComplete()
    {
        // Arrange
        var repository = new InMemorySagaRepository();
        var logger = NullLogger<SagaOrchestrator<OrderSagaData>>.Instance;
        var orchestrator = new SagaOrchestrator<OrderSagaData>(repository, logger);
        
        orchestrator
            .AddStep(new CreateOrderStep())
            .AddStep(new ReserveInventoryStep())
            .AddStep(new ProcessPaymentStep());

        var saga = new OrderSaga();

        // Act
        var result = await orchestrator.ExecuteAsync(saga);

        // Assert
        result.IsSuccess.Should().BeTrue();
        saga.State.Should().Be(SagaState.Completed);
        saga.Data.OrderCreated.Should().BeTrue();
        saga.Data.InventoryReserved.Should().BeTrue();
        saga.Data.PaymentProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task Saga_StepFails_ShouldCompensate()
    {
        // Arrange
        var repository = new InMemorySagaRepository();
        var logger = NullLogger<SagaOrchestrator<OrderSagaData>>.Instance;
        var orchestrator = new SagaOrchestrator<OrderSagaData>(repository, logger);
        
        orchestrator
            .AddStep(new CreateOrderStep())
            .AddStep(new ReserveInventoryStep())
            .AddStep(new FailingPaymentStep()); // This will fail

        var saga = new OrderSaga();

        // Act
        var result = await orchestrator.ExecuteAsync(saga);

        // Assert
        result.IsSuccess.Should().BeFalse();
        saga.State.Should().Be(SagaState.Compensated);
        saga.Data.OrderCreated.Should().BeTrue();
        saga.Data.InventoryReserved.Should().BeTrue();
        saga.Data.PaymentProcessed.Should().BeFalse();
        // Compensation should have run
        saga.Data.InventoryReleased.Should().BeTrue();
        saga.Data.OrderCancelled.Should().BeTrue();
    }
}

// Test Saga
public class OrderSaga : SagaBase<OrderSagaData>
{
}

public class OrderSagaData
{
    public bool OrderCreated { get; set; }
    public bool InventoryReserved { get; set; }
    public bool PaymentProcessed { get; set; }
    public bool OrderCancelled { get; set; }
    public bool InventoryReleased { get; set; }
}

// Test Steps
public class CreateOrderStep : SagaStepBase<OrderSagaData>
{
    public override async Task<TransitResult> ExecuteAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        saga.Data.OrderCreated = true;
        return TransitResult.Success();
    }

    public override async Task<TransitResult> CompensateAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        saga.Data.OrderCancelled = true;
        return TransitResult.Success();
    }
}

public class ReserveInventoryStep : SagaStepBase<OrderSagaData>
{
    public override async Task<TransitResult> ExecuteAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        saga.Data.InventoryReserved = true;
        return TransitResult.Success();
    }

    public override async Task<TransitResult> CompensateAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        saga.Data.InventoryReleased = true;
        return TransitResult.Success();
    }
}

public class ProcessPaymentStep : SagaStepBase<OrderSagaData>
{
    public override async Task<TransitResult> ExecuteAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        saga.Data.PaymentProcessed = true;
        return TransitResult.Success();
    }

    public override async Task<TransitResult> CompensateAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        // Refund would happen here
        return TransitResult.Success();
    }
}

public class FailingPaymentStep : SagaStepBase<OrderSagaData>
{
    public override Task<TransitResult> ExecuteAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        // Simulate payment failure
        return Task.FromResult(TransitResult.Failure("Payment declined"));
    }

    public override Task<TransitResult> CompensateAsync(ISaga<OrderSagaData> saga, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TransitResult.Success());
    }
}

