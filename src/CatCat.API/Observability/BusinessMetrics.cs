using Prometheus;

namespace CatCat.API.Observability;

/// <summary>
/// 业务指标收集
/// </summary>
public class BusinessMetrics
{
    // 订单相关指标
    public static readonly Counter OrdersCreated = Metrics.CreateCounter(
        "catcat_orders_created_total",
        "Total number of orders created",
        new CounterConfiguration
        {
            LabelNames = new[] { "status", "package_type" }
        });

    public static readonly Counter OrdersCancelled = Metrics.CreateCounter(
        "catcat_orders_cancelled_total",
        "Total number of orders cancelled",
        new CounterConfiguration
        {
            LabelNames = new[] { "reason" }
        });

    public static readonly Counter OrdersCompleted = Metrics.CreateCounter(
        "catcat_orders_completed_total",
        "Total number of orders completed");

    public static readonly Histogram OrderProcessingDuration = Metrics.CreateHistogram(
        "catcat_order_processing_duration_seconds",
        "Order processing duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 10),
            LabelNames = new[] { "status" }
        });

    public static readonly Gauge ActiveOrders = Metrics.CreateGauge(
        "catcat_active_orders",
        "Number of currently active orders");

    // 用户相关指标
    public static readonly Counter UsersRegistered = Metrics.CreateCounter(
        "catcat_users_registered_total",
        "Total number of users registered",
        new CounterConfiguration
        {
            LabelNames = new[] { "role" }
        });

    public static readonly Counter UserLogins = Metrics.CreateCounter(
        "catcat_user_logins_total",
        "Total number of user logins",
        new CounterConfiguration
        {
            LabelNames = new[] { "role", "status" }
        });

    public static readonly Gauge OnlineUsers = Metrics.CreateGauge(
        "catcat_online_users",
        "Number of currently online users");

    // 缓存相关指标
    public static readonly Counter CacheHits = Metrics.CreateCounter(
        "catcat_cache_hits_total",
        "Total number of cache hits",
        new CounterConfiguration
        {
            LabelNames = new[] { "cache_type" }
        });

    public static readonly Counter CacheMisses = Metrics.CreateCounter(
        "catcat_cache_misses_total",
        "Total number of cache misses",
        new CounterConfiguration
        {
            LabelNames = new[] { "cache_type" }
        });

    public static readonly Gauge CacheHitRatio = Metrics.CreateGauge(
        "catcat_cache_hit_ratio",
        "Cache hit ratio (0-1)",
        new GaugeConfiguration
        {
            LabelNames = new[] { "cache_type" }
        });

    public static readonly Counter CacheEvictions = Metrics.CreateCounter(
        "catcat_cache_evictions_total",
        "Total number of cache evictions",
        new CounterConfiguration
        {
            LabelNames = new[] { "cache_type", "reason" }
        });

    // 数据库相关指标
    public static readonly Histogram DatabaseQueryDuration = Metrics.CreateHistogram(
        "catcat_database_query_duration_seconds",
        "Database query duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 15),
            LabelNames = new[] { "operation", "table" }
        });

    public static readonly Counter DatabaseQueries = Metrics.CreateCounter(
        "catcat_database_queries_total",
        "Total number of database queries",
        new CounterConfiguration
        {
            LabelNames = new[] { "operation", "table", "status" }
        });

    public static readonly Counter DatabaseConnectionsOpened = Metrics.CreateCounter(
        "catcat_database_connections_opened_total",
        "Total number of database connections opened");

    public static readonly Counter DatabaseConnectionErrors = Metrics.CreateCounter(
        "catcat_database_connection_errors_total",
        "Total number of database connection errors",
        new CounterConfiguration
        {
            LabelNames = new[] { "error_type" }
        });

    public static readonly Gauge DatabaseActiveConnections = Metrics.CreateGauge(
        "catcat_database_active_connections",
        "Number of active database connections");

    public static readonly Counter DatabaseSlowQueries = Metrics.CreateCounter(
        "catcat_database_slow_queries_total",
        "Total number of slow queries (>1s)",
        new CounterConfiguration
        {
            LabelNames = new[] { "operation", "table" }
        });

    // API 相关指标
    public static readonly Histogram ApiRequestDuration = Metrics.CreateHistogram(
        "catcat_api_request_duration_seconds",
        "API request duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 12),
            LabelNames = new[] { "method", "endpoint", "status_code" }
        });

    public static readonly Counter ApiRequests = Metrics.CreateCounter(
        "catcat_api_requests_total",
        "Total number of API requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" }
        });

    public static readonly Counter ApiErrors = Metrics.CreateCounter(
        "catcat_api_errors_total",
        "Total number of API errors",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "error_type" }
        });

    public static readonly Gauge ApiConcurrentRequests = Metrics.CreateGauge(
        "catcat_api_concurrent_requests",
        "Number of concurrent API requests");

    // 文件存储相关指标
    public static readonly Counter FilesUploaded = Metrics.CreateCounter(
        "catcat_files_uploaded_total",
        "Total number of files uploaded",
        new CounterConfiguration
        {
            LabelNames = new[] { "file_type" }
        });

    public static readonly Counter FilesDeleted = Metrics.CreateCounter(
        "catcat_files_deleted_total",
        "Total number of files deleted");

    public static readonly Histogram FileUploadDuration = Metrics.CreateHistogram(
        "catcat_file_upload_duration_seconds",
        "File upload duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
        });

    public static readonly Summary FileSize = Metrics.CreateSummary(
        "catcat_file_size_bytes",
        "File size distribution in bytes",
        new SummaryConfiguration
        {
            Objectives = new[]
            {
                new QuantileEpsilonPair(0.5, 0.05),
                new QuantileEpsilonPair(0.9, 0.01),
                new QuantileEpsilonPair(0.99, 0.001)
            },
            LabelNames = new[] { "file_type" }
        });

    // 消息队列相关指标
    public static readonly Counter MessagesPublished = Metrics.CreateCounter(
        "catcat_messages_published_total",
        "Total number of messages published",
        new CounterConfiguration
        {
            LabelNames = new[] { "subject" }
        });

    public static readonly Counter MessagesConsumed = Metrics.CreateCounter(
        "catcat_messages_consumed_total",
        "Total number of messages consumed",
        new CounterConfiguration
        {
            LabelNames = new[] { "subject", "status" }
        });

    public static readonly Histogram MessageProcessingDuration = Metrics.CreateHistogram(
        "catcat_message_processing_duration_seconds",
        "Message processing duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
            LabelNames = new[] { "subject" }
        });

    // 支付相关指标
    public static readonly Counter PaymentsProcessed = Metrics.CreateCounter(
        "catcat_payments_processed_total",
        "Total number of payments processed",
        new CounterConfiguration
        {
            LabelNames = new[] { "status", "method" }
        });

    public static readonly Summary PaymentAmount = Metrics.CreateSummary(
        "catcat_payment_amount_yuan",
        "Payment amount distribution in CNY",
        new SummaryConfiguration
        {
            Objectives = new[]
            {
                new QuantileEpsilonPair(0.5, 0.05),
                new QuantileEpsilonPair(0.9, 0.01),
                new QuantileEpsilonPair(0.99, 0.001)
            }
        });

    // 熔断器相关指标
    public static readonly Counter CircuitBreakerStateChanges = Metrics.CreateCounter(
        "catcat_circuit_breaker_state_changes_total",
        "Total number of circuit breaker state changes",
        new CounterConfiguration
        {
            LabelNames = new[] { "name", "from_state", "to_state" }
        });

    public static readonly Gauge CircuitBreakerState = Metrics.CreateGauge(
        "catcat_circuit_breaker_state",
        "Circuit breaker state (0=closed, 1=open, 2=half-open)",
        new GaugeConfiguration
        {
            LabelNames = new[] { "name" }
        });
}

