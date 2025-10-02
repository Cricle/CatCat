namespace CatCat.Domain.Entities;

/// <summary>
/// 支付记录
/// </summary>
public class Payment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;  // Stripe Payment Intent ID
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
    Pending = 0,      // 待支付
    Processing = 1,   // 处理中
    Succeeded = 2,    // 支付成功
    Failed = 3,       // 支付失败
    Cancelled = 4,    // 已取消
    Refunded = 5      // 已退款
}

public enum PaymentMethod
{
    Card = 1,         // 信用卡
    Alipay = 2,       // 支付宝
    Wechat = 3        // 微信支付
}

