using CatCat.Infrastructure.Entities;

namespace CatCat.API.Models;

// Simple message responses
public record MessageResponse(string Message);

// Health check response
public record HealthResponse(string Status, DateTime Timestamp);

// Auth responses
public record AuthResponse(string Token, UserInfo User);
public record UserInfo(long Id, string Phone, string? NickName, string? Avatar, UserRole Role);

// Pet list response
public record PetListResponse(List<Pet> Items, int Total);

// Pet create response
public record PetCreateResponse(long Id, string Message);

// Review create response
public record ReviewCreateResponse(long ReviewId, string Message);

// Review list response
public record ReviewListResponse(IEnumerable<Review> Items, int Total, decimal AverageRating, int Page, int PageSize);

// User list response
public record UserListResponse(List<User> Items, int Total, int Page, int PageSize);

// Rate limit response
public record RateLimitResponse(bool Success, string Message, int Code, double? RetryAfter = null);

