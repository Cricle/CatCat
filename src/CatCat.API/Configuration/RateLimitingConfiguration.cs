using CatCat.API.Json;
using CatCat.API.Models;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace CatCat.API.Configuration;

// Rate limiting configuration to prevent API abuse
// Supports: Fixed Window, Sliding Window, Token Bucket, Concurrency Limiter
public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // 1. Global default policy - Fixed Window
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,                          // Max 100 requests per window
                        Window = TimeSpan.FromMinutes(1),           // 1 minute window
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10                             // Max 10 requests in queue
                    });
            });

            // 2. API policy - Sliding Window (smoother)
            options.AddPolicy("api", context =>
            {
                var userId = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 60,                               // 60 requests per window
                    Window = TimeSpan.FromMinutes(1),               // 1 minute window
                    SegmentsPerWindow = 6,                          // 6 segments (10s each)
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                });
            });

            // 3. Auth policy - Fixed Window (strict)
            options.AddPolicy("auth", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,                                // Max 5 login attempts per minute
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0                                  // No queue allowed
                });
            });

            // 4. Order creation policy - Token Bucket (allows burst)
            options.AddPolicy("order-create", context =>
            {
                var userId = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,                                // Bucket capacity: 10 tokens
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 3,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10), // Refill every 10 seconds
                    TokensPerPeriod = 2,                            // 2 tokens per refill
                    AutoReplenishment = true
                });
            });

            // 5. Payment policy - Concurrency Limiter (strict control)
            options.AddPolicy("payment", context =>
            {
                var userId = context.User.Identity?.Name ?? "anonymous";

                return RateLimitPartition.GetConcurrencyLimiter(userId, _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = 1,                                // Only 1 concurrent payment per user
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2                                  // Max 2 requests in queue
                });
            });

            // 6. Query policy - Sliding Window (lenient)
            options.AddPolicy("query", context =>
            {
                var userId = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 200,                              // 200 requests per minute
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 12,                         // 12 segments (5s each)
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
            });

            // Global rejection response handler
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? new RateLimitResponse(false, "Too many requests, please try again later", 429, retryAfter.TotalSeconds)
                    : new RateLimitResponse(false, "Too many requests, please try again later", 429);
                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(response, AppJsonContext.Default.RateLimitResponse),
                    token);
            };
        });

        return services;
    }
}
