using CatCat.API.Json;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace CatCat.API.Configuration;

/// <summary>
/// 限流配置 - 防止接口击穿
/// 支持多种限流算法：固定窗口、滑动窗口、令牌桶、并发限制
/// </summary>
public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // 1. 全局默认策略 - 固定窗口
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,              // 每个窗口最多100个请求
                        Window = TimeSpan.FromMinutes(1), // 1分钟窗口
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10                 // 队列最多10个请求
                    });
            });

            // 2. API 策略 - 滑动窗口（更平滑）
            options.AddPolicy("api", context =>
            {
                var userId = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 60,                   // 每个窗口60个请求
                    Window = TimeSpan.FromMinutes(1),   // 1分钟窗口
                    SegmentsPerWindow = 6,              // 分成6段（每10秒一段）
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                });
            });

            // 3. 登录/注册策略 - 固定窗口（严格限制）
            options.AddPolicy("auth", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,                    // 每分钟最多5次登录尝试
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0                      // 不允许排队
                });
            });

            // 4. 创建订单策略 - 令牌桶（允许突发）
            options.AddPolicy("order-create", context =>
            {
                var userId = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,                    // 桶容量10个令牌
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 3,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10), // 每10秒补充
                    TokensPerPeriod = 2,                // 每次补充2个令牌
                    AutoReplenishment = true
                });
            });

            // 5. 支付接口策略 - 并发限制（严格控制）
            options.AddPolicy("payment", context =>
            {
                var userId = context.User.Identity?.Name ?? "anonymous";

                return RateLimitPartition.GetConcurrencyLimiter(userId, _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = 1,                    // 同一用户同时只能有1个支付请求
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2                      // 最多排队2个
                });
            });

            // 6. 查询接口策略 - 滑动窗口（较宽松）
            options.AddPolicy("query", context =>
            {
                var userId = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 200,                  // 每分钟200个请求
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 12,             // 分成12段（每5秒一段）
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
            });

            // 全局拒绝响应处理
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var response = new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = "请求过于频繁，请稍后再试",
                        ["code"] = 429,
                        ["retryAfter"] = retryAfter.TotalSeconds
                    };
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(response, AppJsonContext.Default.DictionaryStringObject),
                        token);
                }
                else
                {
                    var response = new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = "请求过于频繁，请稍后再试",
                        ["code"] = 429
                    };
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(response, AppJsonContext.Default.DictionaryStringObject),
                        token);
                }
            };
        });

        return services;
    }
}

