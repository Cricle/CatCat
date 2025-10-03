using CatCat.Infrastructure.Entities;

namespace CatCat.API.Models;

// Auth requests
public record SendCodeRequest(string Phone);
public record RegisterRequest(string Phone, string Code, string Password, string? NickName);
public record LoginRequest(string Phone, string Password);
public record RefreshTokenRequest(string RefreshToken);

// User requests
public record UpdateUserRequest(string? NickName, string? Email, string? Avatar);

// Pet requests
public record CreatePetRequest(
    string Name,
    PetType Type,
    string? Breed,
    int Age,
    Gender Gender,
    string? Avatar,
    string? Character,
    string? DietaryHabits,
    string? HealthStatus,
    string? Remarks,
    string? FoodLocationImage,
    string? FoodLocationDesc,
    string? WaterLocationImage,
    string? WaterLocationDesc,
    string? LitterBoxLocationImage,
    string? LitterBoxLocationDesc,
    string? CleaningSuppliesImage,
    string? CleaningSuppliesDesc,
    bool NeedsWaterRefill,
    string? SpecialInstructions);

public record UpdatePetRequest(
    string? Name,
    string? Breed,
    int? Age,
    string? Avatar,
    string? Character,
    string? DietaryHabits,
    string? HealthStatus,
    string? Remarks,
    string? FoodLocationImage,
    string? FoodLocationDesc,
    string? WaterLocationImage,
    string? WaterLocationDesc,
    string? LitterBoxLocationImage,
    string? LitterBoxLocationDesc,
    string? CleaningSuppliesImage,
    string? CleaningSuppliesDesc,
    bool? NeedsWaterRefill,
    string? SpecialInstructions);

// Order requests
public record CreateOrderRequest(
    long CustomerId,
    long ServicePackageId,
    long PetId,
    DateTime ServiceDate,
    string ServiceAddress,
    string? Remark);

public record PayOrderRequest(string PaymentIntentId);

// Review requests
public record CreateReviewRequest(
    long OrderId,
    int Rating,
    string? Content,
    string? PhotoUrls);

public record ReplyReviewRequest(string Reply);

