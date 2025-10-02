namespace CatCat.Infrastructure.Entities;

public class ServiceOrder
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long? ServiceProviderId { get; set; }
    public long PetId { get; set; }
    public long ServicePackageId { get; set; }
    public DateTime ServiceDate { get; set; }
    public TimeSpan ServiceTime { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? AddressDetail { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public string? CustomerRemark { get; set; }
    public string? ServiceRemark { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum OrderStatus
{
    Queued = -1,
    Pending = 0,
    Accepted = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5
}

