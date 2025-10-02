namespace CatCat.Domain.Entities;

/// <summary>
/// 订单状态变更历史 - 让流程透明可见
/// </summary>
public class OrderStatusHistory
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

