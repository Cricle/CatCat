namespace CatCat.Domain.Entities;

/// <summary>
/// 用户实体
/// </summary>
public class User
{
    public long Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string NickName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 用户角色
/// </summary>
public enum UserRole
{
    Customer = 1,      // 客户
    ServiceProvider = 2, // 服务人员
    Admin = 99         // 管理员
}

/// <summary>
/// 用户状态
/// </summary>
public enum UserStatus
{
    Pending = 0,    // 待审核
    Active = 1,     // 正常
    Suspended = 2,  // 停用
    Banned = 3      // 封禁
}

