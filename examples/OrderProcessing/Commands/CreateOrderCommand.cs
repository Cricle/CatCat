using CatCat.Transit.Messages;

namespace OrderProcessing.Commands;

public record CreateOrderCommand : IRequest<Guid>
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public Guid CustomerId { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Amount { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
}

public record ProcessPaymentCommand : IRequest<bool>
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

