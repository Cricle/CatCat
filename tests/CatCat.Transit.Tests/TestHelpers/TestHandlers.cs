using CatCat.Transit.Handlers;
using CatCat.Transit.Results;

namespace CatCat.Transit.Tests.TestHelpers;

public class TestCommandHandler : IRequestHandler<TestCommand, string>
{
    public int ExecutionCount { get; private set; }
    public bool ShouldFail { get; set; }

    public Task<TransitResult<string>> HandleAsync(TestCommand request, CancellationToken cancellationToken = default)
    {
        ExecutionCount++;

        if (ShouldFail)
        {
            return Task.FromResult(TransitResult<string>.Failure("Handler failed intentionally"));
        }

        return Task.FromResult(TransitResult<string>.Success($"Processed: {request.Data}"));
    }
}

public class TestCommandWithoutResponseHandler : IRequestHandler<TestCommandWithoutResponse>
{
    public int ExecutionCount { get; private set; }

    public Task<TransitResult> HandleAsync(TestCommandWithoutResponse request, CancellationToken cancellationToken = default)
    {
        ExecutionCount++;
        return Task.FromResult(TransitResult.Success());
    }
}

public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<TransitResult<string>> HandleAsync(TestQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TransitResult<string>.Success($"Result for ID: {request.Id}"));
    }
}

public class TestEventHandler : IEventHandler<TestEvent>
{
    public int ExecutionCount { get; private set; }
    public List<string> ReceivedMessages { get; } = new();

    public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
    {
        ExecutionCount++;
        ReceivedMessages.Add(@event.Message);
        return Task.CompletedTask;
    }
}

public class SlowHandler : IRequestHandler<TestCommand, string>
{
    public async Task<TransitResult<string>> HandleAsync(TestCommand request, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return TransitResult<string>.Success("Slow result");
    }
}

