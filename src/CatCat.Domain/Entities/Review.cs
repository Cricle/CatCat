namespace CatCat.Domain.Entities;

/// <summary>
/// 评价
/// </summary>
public class Review
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long ServiceProviderId { get; set; }
    public int Rating { get; set; } // 1-5分
    public string? Content { get; set; }
    public string? PhotoUrls { get; set; } // JSON数组
    public string? Reply { get; set; } // 服务人员回复
    public DateTime? RepliedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

