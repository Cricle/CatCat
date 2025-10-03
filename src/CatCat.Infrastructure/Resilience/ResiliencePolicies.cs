using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace CatCat.Infrastructure.Resilience;

/// <summary>
/// 弹性策略配置（熔断、重试、超时）
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// 数据库操作弹性策略
    /// - 重试: 3次，指数退避
    /// - 熔断: 连续10次失败则熔断，熔断时间30秒
    /// - 超时: 5秒
    /// </summary>
    public static ResiliencePipeline<T> CreateDatabasePolicy<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            // 超时策略（5秒）
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                OnTimeout = args =>
                {
                    logger.LogWarning("Database operation timed out after 5 seconds");
                    return default;
                }
            })
            // 重试策略（3次，指数退避）
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying database operation. Attempt: {Attempt}", args.AttemptNumber);
                    return default;
                }
            })
            // 熔断策略（连续10次失败，熔断30秒）
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    logger.LogError("Circuit breaker opened due to database failures");
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed, database connection restored");
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// 缓存操作弹性策略
    /// - 重试: 2次，固定延迟
    /// - 超时: 2秒
    /// </summary>
    public static ResiliencePipeline<T> CreateCachePolicy<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(2),
                OnTimeout = args =>
                {
                    logger.LogWarning("Cache operation timed out after 2 seconds");
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(50),
                BackoffType = DelayBackoffType.Constant,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying cache operation. Attempt: {Attempt}", args.AttemptNumber);
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// 外部 API 调用弹性策略（Stripe, MinIO 等）
    /// - 重试: 5次，指数退避
    /// - 熔断: 连续20次失败，熔断60秒
    /// - 超时: 10秒
    /// </summary>
    public static ResiliencePipeline<T> CreateExternalApiPolicy<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10),
                OnTimeout = args =>
                {
                    logger.LogWarning("External API call timed out after 10 seconds");
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                MaxDelay = TimeSpan.FromSeconds(5),
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying external API call. Attempt: {Attempt}", args.AttemptNumber);
                    return default;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 20,
                BreakDuration = TimeSpan.FromSeconds(60),
                OnOpened = args =>
                {
                    logger.LogError("Circuit breaker opened for external API");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Circuit breaker half-opened, testing external API");
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed, external API restored");
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// 消息队列操作弹性策略
    /// - 重试: 3次，指数退避
    /// - 超时: 5秒
    /// </summary>
    public static ResiliencePipeline<T> CreateMessageQueuePolicy<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                OnTimeout = args =>
                {
                    logger.LogWarning("Message queue operation timed out after 5 seconds");
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying message queue operation. Attempt: {Attempt}", args.AttemptNumber);
                    return default;
                }
            })
            .Build();
    }
}

