namespace CatCat.Infrastructure.Entities;

public class OrderStatusHistory
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

