using CatCat.Transit.Messages;

namespace OrderProcessing.Events;

public record OrderCreatedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record PaymentProcessedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

public record OrderPlacedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record PaymentConfirmedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public string TransactionId { get; init; } = string.Empty;
}

public record OrderShippedEvent : IEvent
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    public string TrackingNumber { get; init; } = string.Empty;
}

