using CatCat.API.Models;
using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Services;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Messages;
using CatCat.Infrastructure.Payment;
using System.Text.Json.Serialization;

namespace CatCat.API.Json;

/// <summary>
/// AOT-friendly JSON serialization context - 完整的源生成配置
/// </summary>
// API Response Models
[JsonSerializable(typeof(ApiResult))]
[JsonSerializable(typeof(ApiResult<long>))]
[JsonSerializable(typeof(ApiResult<string>))]
[JsonSerializable(typeof(ApiResult<bool>))]
[JsonSerializable(typeof(ApiResult<User>))]
[JsonSerializable(typeof(ApiResult<ServiceOrder>))]
[JsonSerializable(typeof(ApiResult<Pet>))]
[JsonSerializable(typeof(ApiResult<object>))]

// Result Pattern Types
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<long>))]
[JsonSerializable(typeof(Result<string>))]
[JsonSerializable(typeof(Result<bool>))]
[JsonSerializable(typeof(Result<ServiceOrder>))]
[JsonSerializable(typeof(Result<User>))]

// Domain Entities
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Pet))]
[JsonSerializable(typeof(ServicePackage))]
[JsonSerializable(typeof(ServiceOrder))]
[JsonSerializable(typeof(Review))]
[JsonSerializable(typeof(Payment))]
[JsonSerializable(typeof(OrderStatusHistory))]

// Request Models
[JsonSerializable(typeof(SendCodeRequest))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(UpdateUserRequest))]
[JsonSerializable(typeof(CreatePetRequest))]
[JsonSerializable(typeof(UpdatePetRequest))]
[JsonSerializable(typeof(CreateOrderRequest))]
[JsonSerializable(typeof(PayOrderRequest))]
[JsonSerializable(typeof(CreateReviewRequest))]
[JsonSerializable(typeof(ReplyReviewRequest))]
[JsonSerializable(typeof(CreateOrderCommand))]
[JsonSerializable(typeof(CreateReviewCommand))]

// Response Models
[JsonSerializable(typeof(MessageResponse))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(AuthResponse))]
[JsonSerializable(typeof(UserInfo))]
[JsonSerializable(typeof(PetListResponse))]
[JsonSerializable(typeof(PetCreateResponse))]
[JsonSerializable(typeof(ReviewCreateResponse))]
[JsonSerializable(typeof(ReviewListResponse))]
[JsonSerializable(typeof(UserListResponse))]
[JsonSerializable(typeof(RateLimitResponse))]

// Payment Models
[JsonSerializable(typeof(PaymentIntentResult))]

// NATS Messages/Events
[JsonSerializable(typeof(OrderQueueMessage))]
[JsonSerializable(typeof(OrderCreatedMessage))]
[JsonSerializable(typeof(ReviewCreatedEvent))]
[JsonSerializable(typeof(ReviewRepliedEvent))]

// Collections
[JsonSerializable(typeof(List<User>))]
[JsonSerializable(typeof(List<Pet>))]
[JsonSerializable(typeof(List<ServicePackage>))]
[JsonSerializable(typeof(List<ServiceOrder>))]
[JsonSerializable(typeof(List<Review>))]
[JsonSerializable(typeof(List<OrderStatusHistory>))]
[JsonSerializable(typeof(IEnumerable<ServiceOrder>))]
[JsonSerializable(typeof(IEnumerable<Review>))]

// Paged results
[JsonSerializable(typeof(CatCat.Infrastructure.Common.PagedResult<ServiceOrder>))]
[JsonSerializable(typeof(CatCat.Infrastructure.Common.ReviewPagedResult))]

// Anonymous types equivalents
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, string>))]

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization)]
public partial class AppJsonContext : JsonSerializerContext
{
}

