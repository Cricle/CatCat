using System.Diagnostics.Metrics;

namespace CatCat.API.Observability;

/// <summary>
/// 自定义业务指标
/// </summary>
public class CustomMetrics
{
    private readonly Meter _meter;

    // 订单相关指标
    private readonly Counter<long> _orderCreatedCounter;
    private readonly Counter<long> _orderCompletedCounter;
    private readonly Counter<long> _orderCancelledCounter;
    private readonly Histogram<double> _orderAmountHistogram;
    private readonly Histogram<double> _orderProcessingTimeHistogram;

    // 用户相关指标
    private readonly Counter<long> _userRegistrationCounter;
    private readonly Counter<long> _userLoginCounter;
    private readonly Counter<long> _userLoginFailedCounter;

    // 支付相关指标
    private readonly Counter<long> _paymentSuccessCounter;
    private readonly Counter<long> _paymentFailedCounter;
    private readonly Histogram<double> _paymentAmountHistogram;

    // 缓存相关指标
    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;
    private readonly Histogram<double> _cacheOperationDurationHistogram;

    public CustomMetrics(Meter meter)
    {
        _meter = meter;

        // 初始化订单指标
        _orderCreatedCounter = _meter.CreateCounter<long>(
            "catcat.orders.created",
            description: "订单创建总数");

        _orderCompletedCounter = _meter.CreateCounter<long>(
            "catcat.orders.completed",
            description: "订单完成总数");

        _orderCancelledCounter = _meter.CreateCounter<long>(
            "catcat.orders.cancelled",
            description: "订单取消总数");

        _orderAmountHistogram = _meter.CreateHistogram<double>(
            "catcat.orders.amount",
            unit: "CNY",
            description: "订单金额分布");

        _orderProcessingTimeHistogram = _meter.CreateHistogram<double>(
            "catcat.orders.processing_time",
            unit: "ms",
            description: "订单处理时间分布");

        // 初始化用户指标
        _userRegistrationCounter = _meter.CreateCounter<long>(
            "catcat.users.registrations",
            description: "用户注册总数");

        _userLoginCounter = _meter.CreateCounter<long>(
            "catcat.users.logins",
            description: "用户登录总数");

        _userLoginFailedCounter = _meter.CreateCounter<long>(
            "catcat.users.login_failures",
            description: "用户登录失败总数");

        // 初始化支付指标
        _paymentSuccessCounter = _meter.CreateCounter<long>(
            "catcat.payments.success",
            description: "支付成功总数");

        _paymentFailedCounter = _meter.CreateCounter<long>(
            "catcat.payments.failed",
            description: "支付失败总数");

        _paymentAmountHistogram = _meter.CreateHistogram<double>(
            "catcat.payments.amount",
            unit: "CNY",
            description: "支付金额分布");

        // 初始化缓存指标
        _cacheHitCounter = _meter.CreateCounter<long>(
            "catcat.cache.hits",
            description: "缓存命中总数");

        _cacheMissCounter = _meter.CreateCounter<long>(
            "catcat.cache.misses",
            description: "缓存未命中总数");

        _cacheOperationDurationHistogram = _meter.CreateHistogram<double>(
            "catcat.cache.operation_duration",
            unit: "ms",
            description: "缓存操作耗时分布");
    }

    // 订单指标方法
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

    // 用户指标方法
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

    // 支付指标方法
    public void RecordPaymentSuccess(double amount, string paymentMethod)
    {
        _paymentSuccessCounter.Add(1, new KeyValuePair<string, object?>("method", paymentMethod));
        _paymentAmountHistogram.Record(amount, new KeyValuePair<string, object?>("method", paymentMethod), new KeyValuePair<string, object?>("status", "success"));
    }

    public void RecordPaymentFailed(string reason, string paymentMethod)
    {
        _paymentFailedCounter.Add(1, new KeyValuePair<string, object?>("reason", reason), new KeyValuePair<string, object?>("method", paymentMethod));
    }

    // 缓存指标方法
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

