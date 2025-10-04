using CatCat.Transit.CatGa;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// CatGa DI 扩展方法
/// </summary>
public static class CatGaServiceCollectionExtensions
{
    /// <summary>
    /// 添加 CatGa 支持
    /// </summary>
    public static IServiceCollection AddCatGa(
        this IServiceCollection services,
        Action<CatGaOptions>? configureOptions = null)
    {
        var options = new CatGaOptions();
        configureOptions?.Invoke(options);

        // 注册选项
        services.TryAddSingleton(options);

        // 注册幂等性存储
        services.TryAddSingleton(sp =>
            new CatGaIdempotencyStore(
                options.IdempotencyShardCount,
                options.IdempotencyExpiry));

        // 注册执行器
        services.TryAddSingleton<ICatGaExecutor, CatGaExecutor>();

        return services;
    }

    /// <summary>
    /// 注册 CatGa 事务（带返回值）
    /// </summary>
    public static IServiceCollection AddCatGaTransaction<TRequest, TResponse, TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, ICatGaTransaction<TRequest, TResponse>
    {
        services.TryAddTransient<ICatGaTransaction<TRequest, TResponse>, TImplementation>();
        return services;
    }

    /// <summary>
    /// 注册 CatGa 事务（无返回值）
    /// </summary>
    public static IServiceCollection AddCatGaTransaction<TRequest, TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, ICatGaTransaction<TRequest>
    {
        services.TryAddTransient<ICatGaTransaction<TRequest>, TImplementation>();
        return services;
    }
}

