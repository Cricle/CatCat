namespace CatCat.Infrastructure.Entities;

public class Pet
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetType Type { get; set; }
    public string? Breed { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? Avatar { get; set; }
    public string? Character { get; set; }
    public string? DietaryHabits { get; set; }
    public string? HealthStatus { get; set; }
    public string? Remarks { get; set; }
    
    // Service Information (解决上门服务痛点)
    public string? FoodLocationImage { get; set; }       // 猫粮位置照片
    public string? FoodLocationDesc { get; set; }        // 猫粮位置描述
    public string? WaterLocationImage { get; set; }      // 水盆位置照片
    public string? WaterLocationDesc { get; set; }       // 水盆位置描述
    public string? LitterBoxLocationImage { get; set; }  // 猫砂盆位置照片
    public string? LitterBoxLocationDesc { get; set; }   // 猫砂盆位置描述
    public string? CleaningSuppliesImage { get; set; }   // 清洁用品位置照片
    public string? CleaningSuppliesDesc { get; set; }    // 清洁用品位置描述
    public bool NeedsWaterRefill { get; set; }           // 是否需要备水
    public string? SpecialInstructions { get; set; }     // 特殊说明
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

