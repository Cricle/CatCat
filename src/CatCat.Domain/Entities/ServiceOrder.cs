namespace CatCat.Domain.Entities;

/// <summary>
/// 服务订单
/// </summary>
public class ServiceOrder
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long? ServiceProviderId { get; set; }
    public long PetId { get; set; }
    public long ServicePackageId { get; set; }
    public DateTime ServiceDate { get; set; } // 预约服务日期
    public TimeSpan ServiceTime { get; set; } // 预约服务时间
    public string Address { get; set; } = string.Empty;
    public string? AddressDetail { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public string? CustomerRemark { get; set; } // 客户备注
    public string? ServiceRemark { get; set; } // 服务人员备注
    public DateTime? AcceptedAt { get; set; } // 接单时间
    public DateTime? StartedAt { get; set; } // 开始服务时间
    public DateTime? CompletedAt { get; set; } // 完成服务时间
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 订单状态
/// </summary>
public enum OrderStatus
{
    Pending = 0,      // 待接单
    Accepted = 1,     // 已接单
    InProgress = 2,   // 服务中
    Completed = 3,    // 已完成
    Cancelled = 4,    // 已取消
    Refunded = 5      // 已退款
}

