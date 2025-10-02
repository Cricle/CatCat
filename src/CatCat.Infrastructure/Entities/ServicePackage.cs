namespace CatCat.Infrastructure.Entities;

/// <summary>
/// 服务套餐
/// </summary>
public class ServicePackage
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Duration { get; set; } // 服务时长（分钟）
    public string? IconUrl { get; set; }
    public string ServiceItems { get; set; } = string.Empty; // JSON格式的服务项目
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

