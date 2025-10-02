namespace CatCat.Infrastructure.Entities;

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

public enum UserRole
{
    Customer = 1,
    ServiceProvider = 2,
    Admin = 99
}

public enum UserStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Banned = 3
}

