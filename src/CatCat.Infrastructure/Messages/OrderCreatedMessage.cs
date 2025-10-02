namespace CatCat.Infrastructure.Messages;

/// <summary>
/// 订单创建消息
/// </summary>
public class OrderCreatedMessage
{
    public long OrderId { get; set; }
}

