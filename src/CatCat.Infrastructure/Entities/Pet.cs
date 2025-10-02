namespace CatCat.Infrastructure.Entities;

/// <summary>
/// 宠物信息
/// </summary>
public class Pet
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetType Type { get; set; }
    public string? Breed { get; set; } // 品种
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? Avatar { get; set; }
    public string? Character { get; set; } // 性格特点
    public string? DietaryHabits { get; set; } // 饮食习惯
    public string? HealthStatus { get; set; } // 健康状况
    public string? Remarks { get; set; } // 备注
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum PetType
{
    Cat = 1,
    Dog = 2,
    Other = 99
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Unknown = 0
}

