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

