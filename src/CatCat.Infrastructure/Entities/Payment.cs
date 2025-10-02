namespace CatCat.Infrastructure.Entities;

public class Payment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}

public enum PaymentMethod
{
    Card = 1,
    Alipay = 2,
    Wechat = 3
}

