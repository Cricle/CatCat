namespace CatCat.Infrastructure.Entities;

public class Review
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long ServiceProviderId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
    public string? PhotoUrls { get; set; }
    public string? Reply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

