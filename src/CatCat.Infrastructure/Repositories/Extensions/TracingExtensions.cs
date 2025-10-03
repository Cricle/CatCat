using System.Diagnostics;
using CatCat.Infrastructure.Tracing;

namespace CatCat.Infrastructure.Repositories.Extensions;

/// <summary>
/// 数据库追踪扩展方法
/// </summary>
public static class TracingExtensions
{
    /// <summary>
    /// 执行带追踪的数据库操作
    /// </summary>
    public static async Task<T> ExecuteWithTracingAsync<T>(
        this TracingService tracing,
        string operation,
        string table,
        Func<Task<T>> action,
        string? query = null)
    {
        using var activity = tracing.StartDatabaseActivity(operation, table, query);
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            var result = await action();
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            
            activity?.SetTag("db.duration_ms", elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return result;
        }
        catch (Exception ex)
        {
            tracing.RecordException(ex, activity);
            throw;
        }
    }

    /// <summary>
    /// 执行带追踪的缓存操作
    /// </summary>
    public static async Task<T> ExecuteWithCacheTracingAsync<T>(
        this TracingService tracing,
        string operation,
        string cacheKey,
        Func<Task<T>> action)
    {
        using var activity = tracing.StartCacheActivity(operation, cacheKey);
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            var result = await action();
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            
            activity?.SetTag("cache.duration_ms", elapsed.TotalMilliseconds);
            activity?.SetTag("cache.hit", result != null);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return result;
        }
        catch (Exception ex)
        {
            tracing.RecordException(ex, activity);
            throw;
        }
    }

    /// <summary>
    /// 执行带追踪的消息发布
    /// </summary>
    public static async Task PublishWithTracingAsync<T>(
        this TracingService tracing,
        string subject,
        T message,
        Func<Task> action) where T : class
    {
        using var activity = tracing.StartMessagingActivity("publish", subject, typeof(T).Name);
        try
        {
            activity?.SetTag("messaging.message_id", Guid.NewGuid().ToString());
            activity?.SetTag("messaging.payload_size", System.Text.Json.JsonSerializer.Serialize(message).Length);
            
            await action();
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            tracing.RecordException(ex, activity);
            throw;
        }
    }

    /// <summary>
    /// 执行带追踪的外部 API 调用
    /// </summary>
    public static async Task<T> CallExternalApiWithTracingAsync<T>(
        this TracingService tracing,
        string service,
        string operation,
        Func<Task<T>> action,
        string? endpoint = null)
    {
        using var activity = tracing.StartExternalApiActivity(service, operation, endpoint);
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            var result = await action();
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            
            activity?.SetTag("external.duration_ms", elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return result;
        }
        catch (Exception ex)
        {
            tracing.RecordException(ex, activity);
            throw;
        }
    }
}

