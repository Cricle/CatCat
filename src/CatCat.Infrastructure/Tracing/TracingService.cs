using System.Diagnostics;

namespace CatCat.Infrastructure.Tracing;

/// <summary>
/// 分布式追踪服务 - 全链路跟踪
/// </summary>
public class TracingService
{
    private readonly ActivitySource _activitySource;

    public TracingService(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }

    /// <summary>
    /// 创建数据库操作追踪
    /// </summary>
    public Activity? StartDatabaseActivity(string operation, string table, string? query = null)
    {
        var activity = _activitySource.StartActivity($"DB.{operation}", ActivityKind.Client);
        activity?.SetTag("db.system", "postgresql");
        activity?.SetTag("db.operation", operation);
        activity?.SetTag("db.table", table);
        if (query != null)
        {
            // 限制查询长度避免过大
            var truncatedQuery = query.Length > 500 ? query[..500] + "..." : query;
            activity?.SetTag("db.statement", truncatedQuery);
        }
        return activity;
    }

    /// <summary>
    /// 创建缓存操作追踪
    /// </summary>
    public Activity? StartCacheActivity(string operation, string cacheKey, string? cacheType = "redis")
    {
        var activity = _activitySource.StartActivity($"Cache.{operation}", ActivityKind.Client);
        activity?.SetTag("cache.system", cacheType);
        activity?.SetTag("cache.operation", operation);
        activity?.SetTag("cache.key", cacheKey);
        return activity;
    }

    /// <summary>
    /// 创建消息队列操作追踪
    /// </summary>
    public Activity? StartMessagingActivity(string operation, string subject, string? messageType = null)
    {
        var activityKind = operation == "publish" ? ActivityKind.Producer : ActivityKind.Consumer;
        var activity = _activitySource.StartActivity($"MQ.{operation}", activityKind);
        activity?.SetTag("messaging.system", "nats");
        activity?.SetTag("messaging.operation", operation);
        activity?.SetTag("messaging.destination", subject);
        if (messageType != null)
        {
            activity?.SetTag("messaging.message_type", messageType);
        }
        return activity;
    }

    /// <summary>
    /// 创建外部 API 调用追踪
    /// </summary>
    public Activity? StartExternalApiActivity(string service, string operation, string? endpoint = null)
    {
        var activity = _activitySource.StartActivity($"ExternalAPI.{service}.{operation}", ActivityKind.Client);
        activity?.SetTag("external.service", service);
        activity?.SetTag("external.operation", operation);
        if (endpoint != null)
        {
            activity?.SetTag("external.endpoint", endpoint);
        }
        return activity;
    }

    /// <summary>
    /// 创建业务操作追踪
    /// </summary>
    public Activity? StartBusinessActivity(string operationName, string? entityType = null, string? entityId = null)
    {
        var activity = _activitySource.StartActivity($"Business.{operationName}", ActivityKind.Internal);
        if (entityType != null)
        {
            activity?.SetTag("business.entity_type", entityType);
        }
        if (entityId != null)
        {
            activity?.SetTag("business.entity_id", entityId);
        }
        return activity;
    }

    /// <summary>
    /// 创建后台任务追踪
    /// </summary>
    public Activity? StartBackgroundTaskActivity(string taskName, string? taskType = null)
    {
        var activity = _activitySource.StartActivity($"BackgroundTask.{taskName}", ActivityKind.Internal);
        activity?.SetTag("task.name", taskName);
        if (taskType != null)
        {
            activity?.SetTag("task.type", taskType);
        }
        return activity;
    }

    /// <summary>
    /// 记录异常到当前 Activity
    /// </summary>
    public void RecordException(Exception exception, Activity? activity = null)
    {
        var currentActivity = activity ?? Activity.Current;
        if (currentActivity != null)
        {
            currentActivity.SetStatus(ActivityStatusCode.Error, exception.Message);
            currentActivity.AddTag("exception.type", exception.GetType().FullName);
            currentActivity.AddTag("exception.message", exception.Message);
            currentActivity.AddTag("exception.stacktrace", exception.StackTrace);
        }
    }

    /// <summary>
    /// 添加事件到当前 Activity
    /// </summary>
    public void AddEvent(string name, Dictionary<string, object>? attributes = null)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            var tags = attributes?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
            activity.AddEvent(new ActivityEvent(name, tags: tags != null ? new ActivityTagsCollection(tags) : null));
        }
    }

    /// <summary>
    /// 设置当前 Activity 的标签
    /// </summary>
    public void SetTag(string key, object? value)
    {
        Activity.Current?.SetTag(key, value);
    }

    /// <summary>
    /// 设置当前 Activity 的状态
    /// </summary>
    public void SetStatus(ActivityStatusCode status, string? description = null)
    {
        Activity.Current?.SetStatus(status, description);
    }
}

