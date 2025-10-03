using System.Diagnostics.Metrics;

namespace CatCat.API.Observability;

// Custom business metrics
public class CustomMetrics
{
    private readonly Meter _meter;

    // Order metrics
    private readonly Counter<long> _orderCreatedCounter;
    private readonly Counter<long> _orderCompletedCounter;
    private readonly Counter<long> _orderCancelledCounter;
    private readonly Histogram<double> _orderAmountHistogram;
    private readonly Histogram<double> _orderProcessingTimeHistogram;

    // User metrics
    private readonly Counter<long> _userRegistrationCounter;
    private readonly Counter<long> _userLoginCounter;
    private readonly Counter<long> _userLoginFailedCounter;

    // Payment metrics
    private readonly Counter<long> _paymentSuccessCounter;
    private readonly Counter<long> _paymentFailedCounter;
    private readonly Histogram<double> _paymentAmountHistogram;

    // Cache metrics
    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;
    private readonly Histogram<double> _cacheOperationDurationHistogram;

    public CustomMetrics(Meter meter)
    {
        _meter = meter;

        // Initialize order metrics
        _orderCreatedCounter = _meter.CreateCounter<long>(
            "catcat.orders.created",
            description: "Total orders created");

        _orderCompletedCounter = _meter.CreateCounter<long>(
            "catcat.orders.completed",
            description: "Total orders completed");

        _orderCancelledCounter = _meter.CreateCounter<long>(
            "catcat.orders.cancelled",
            description: "Total orders cancelled");

        _orderAmountHistogram = _meter.CreateHistogram<double>(
            "catcat.orders.amount",
            unit: "CNY",
            description: "Order amount distribution");

        _orderProcessingTimeHistogram = _meter.CreateHistogram<double>(
            "catcat.orders.processing_time",
            unit: "ms",
            description: "Order processing time distribution");

        // Initialize user metrics
        _userRegistrationCounter = _meter.CreateCounter<long>(
            "catcat.users.registrations",
            description: "Total user registrations");

        _userLoginCounter = _meter.CreateCounter<long>(
            "catcat.users.logins",
            description: "Total user logins");

        _userLoginFailedCounter = _meter.CreateCounter<long>(
            "catcat.users.login_failures",
            description: "Total login failures");

        // Initialize payment metrics
        _paymentSuccessCounter = _meter.CreateCounter<long>(
            "catcat.payments.success",
            description: "Total successful payments");

        _paymentFailedCounter = _meter.CreateCounter<long>(
            "catcat.payments.failed",
            description: "Total failed payments");

        _paymentAmountHistogram = _meter.CreateHistogram<double>(
            "catcat.payments.amount",
            unit: "CNY",
            description: "Payment amount distribution");

        // Initialize cache metrics
        _cacheHitCounter = _meter.CreateCounter<long>(
            "catcat.cache.hits",
            description: "Total cache hits");

        _cacheMissCounter = _meter.CreateCounter<long>(
            "catcat.cache.misses",
            description: "Total cache misses");

        _cacheOperationDurationHistogram = _meter.CreateHistogram<double>(
            "catcat.cache.operation_duration",
            unit: "ms",
            description: "Cache operation duration distribution");
    }

    // Order metric methods
    public void RecordOrderCreated(string orderStatus, string serviceType)
    {
        _orderCreatedCounter.Add(1, new KeyValuePair<string, object?>("status", orderStatus), new KeyValuePair<string, object?>("service_type", serviceType));
    }

    public void RecordOrderCompleted(string serviceType, double amount)
    {
        _orderCompletedCounter.Add(1, new KeyValuePair<string, object?>("service_type", serviceType));
        _orderAmountHistogram.Record(amount, new KeyValuePair<string, object?>("service_type", serviceType));
    }

    public void RecordOrderCancelled(string reason)
    {
        _orderCancelledCounter.Add(1, new KeyValuePair<string, object?>("reason", reason));
    }

    public void RecordOrderProcessingTime(double durationMs, string orderStatus)
    {
        _orderProcessingTimeHistogram.Record(durationMs, new KeyValuePair<string, object?>("status", orderStatus));
    }

    // User metric methods
    public void RecordUserRegistration(string role)
    {
        _userRegistrationCounter.Add(1, new KeyValuePair<string, object?>("role", role));
    }

    public void RecordUserLogin(string role)
    {
        _userLoginCounter.Add(1, new KeyValuePair<string, object?>("role", role));
    }

    public void RecordUserLoginFailed(string reason)
    {
        _userLoginFailedCounter.Add(1, new KeyValuePair<string, object?>("reason", reason));
    }

    // Payment metric methods
    public void RecordPaymentSuccess(double amount, string paymentMethod)
    {
        _paymentSuccessCounter.Add(1, new KeyValuePair<string, object?>("method", paymentMethod));
        _paymentAmountHistogram.Record(amount, new KeyValuePair<string, object?>("method", paymentMethod), new KeyValuePair<string, object?>("status", "success"));
    }

    public void RecordPaymentFailed(string reason, string paymentMethod)
    {
        _paymentFailedCounter.Add(1, new KeyValuePair<string, object?>("reason", reason), new KeyValuePair<string, object?>("method", paymentMethod));
    }

    // Cache metric methods
    public void RecordCacheHit(string cacheKey)
    {
        _cacheHitCounter.Add(1, new KeyValuePair<string, object?>("key_prefix", GetKeyPrefix(cacheKey)));
    }

    public void RecordCacheMiss(string cacheKey)
    {
        _cacheMissCounter.Add(1, new KeyValuePair<string, object?>("key_prefix", GetKeyPrefix(cacheKey)));
    }

    public void RecordCacheOperationDuration(double durationMs, string operation)
    {
        _cacheOperationDurationHistogram.Record(durationMs, new KeyValuePair<string, object?>("operation", operation));
    }

    private static string GetKeyPrefix(string cacheKey)
    {
        var colonIndex = cacheKey.IndexOf(':');
        return colonIndex > 0 ? cacheKey[..colonIndex] : cacheKey;
    }
}
