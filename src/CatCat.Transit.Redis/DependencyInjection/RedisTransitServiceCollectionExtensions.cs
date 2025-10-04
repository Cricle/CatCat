using CatCat.Transit.Idempotency;
using CatCat.Transit.Redis;
using CatCat.Transit.Saga;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Redis Transit 依赖注入扩展
/// </summary>
public static class RedisTransitServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Redis 持久化支持
    /// </summary>
    public static IServiceCollection AddRedisTransit(
        this IServiceCollection services,
        Action<RedisTransitOptions>? configureOptions = null)
    {
        var options = new RedisTransitOptions();
        configureOptions?.Invoke(options);

        // 注册 Redis 连接
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = ConfigurationOptions.Parse(options.ConnectionString);
            config.ConnectTimeout = options.ConnectTimeout;
            config.SyncTimeout = options.SyncTimeout;
            config.AllowAdmin = options.AllowAdmin;
            config.KeepAlive = options.KeepAlive;
            config.ConnectRetry = options.ConnectRetry;
            config.Ssl = options.UseSsl;
            
            if (!string.IsNullOrEmpty(options.SslHost))
            {
                config.SslHost = options.SslHost;
            }

            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
            logger.LogInformation("Connecting to Redis at {ConnectionString}", options.ConnectionString);

            return ConnectionMultiplexer.Connect(config);
        });

        // 注册选项
        services.TryAddSingleton(options);

        // 注册 Redis Saga 仓储
        services.TryAddSingleton<ISagaRepository>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<ILogger<RedisSagaRepository>>();
            return new RedisSagaRepository(redis, logger, options);
        });

        // 注册 Redis 幂等性存储
        services.TryAddSingleton<IIdempotencyStore>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<ILogger<RedisIdempotencyStore>>();
            return new RedisIdempotencyStore(redis, logger, options);
        });

        return services;
    }

    /// <summary>
    /// 添加 Redis Saga 仓储（单独使用）
    /// </summary>
    public static IServiceCollection AddRedisSagaRepository(
        this IServiceCollection services,
        Action<RedisTransitOptions>? configureOptions = null)
    {
        var options = new RedisTransitOptions();
        configureOptions?.Invoke(options);

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = ConfigurationOptions.Parse(options.ConnectionString);
            config.ConnectTimeout = options.ConnectTimeout;
            config.SyncTimeout = options.SyncTimeout;
            return ConnectionMultiplexer.Connect(config);
        });

        services.TryAddSingleton(options);

        services.TryAddSingleton<ISagaRepository>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<ILogger<RedisSagaRepository>>();
            return new RedisSagaRepository(redis, logger, options);
        });

        return services;
    }

    /// <summary>
    /// 添加 Redis 幂等性存储（单独使用）
    /// </summary>
    public static IServiceCollection AddRedisIdempotencyStore(
        this IServiceCollection services,
        Action<RedisTransitOptions>? configureOptions = null)
    {
        var options = new RedisTransitOptions();
        configureOptions?.Invoke(options);

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = ConfigurationOptions.Parse(options.ConnectionString);
            config.ConnectTimeout = options.ConnectTimeout;
            config.SyncTimeout = options.SyncTimeout;
            return ConnectionMultiplexer.Connect(config);
        });

        services.TryAddSingleton(options);

        services.TryAddSingleton<IIdempotencyStore>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<ILogger<RedisIdempotencyStore>>();
            return new RedisIdempotencyStore(redis, logger, options);
        });

        return services;
    }
}

