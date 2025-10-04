using CatCat.Transit.Messages;

namespace CatCat.Transit.Handlers;

/// <summary>
/// Handler for domain events
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

