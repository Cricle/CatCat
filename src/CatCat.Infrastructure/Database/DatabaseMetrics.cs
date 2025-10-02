using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CatCat.Infrastructure.Database;

/// <summary>
/// 数据库性能监控指标
/// </summary>
public class DatabaseMetrics
{
    private readonly Histogram<double> _queryDuration;
    private readonly Counter<long> _queryCount;
    private readonly Counter<long> _slowQueryCount;
    private readonly Counter<long> _queryErrorCount;
    private readonly ILogger<DatabaseMetrics> _logger;
    private readonly double _slowQueryThresholdMs;

    public DatabaseMetrics(
        IMeterFactory meterFactory,
        ILogger<DatabaseMetrics> logger,
        double slowQueryThresholdMs = 1000)
    {
        _logger = logger;
        _slowQueryThresholdMs = slowQueryThresholdMs;

        var meter = meterFactory.Create("CatCat.Database");

        _queryDuration = meter.CreateHistogram<double>(
            "database.query.duration",
            unit: "ms",
            description: "数据库查询耗时");

        _queryCount = meter.CreateCounter<long>(
            "database.query.count",
            description: "数据库查询次数");

        _slowQueryCount = meter.CreateCounter<long>(
            "database.slow_query.count",
            description: "慢查询次数");

        _queryErrorCount = meter.CreateCounter<long>(
            "database.query.error.count",
            description: "查询错误次数");
    }

    /// <summary>
    /// 记录查询性能指标
    /// </summary>
    public async Task<T> RecordQueryAsync<T>(
        string queryName,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            sw.Stop();

            var elapsedMs = sw.Elapsed.TotalMilliseconds;

            // 记录查询耗时
            _queryDuration.Record(elapsedMs,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("status", "success"));

            // 记录查询次数
            _queryCount.Add(1,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("status", "success"));

            // 慢查询告警
            if (elapsedMs > _slowQueryThresholdMs)
            {
                _slowQueryCount.Add(1,
                    new KeyValuePair<string, object?>("query", queryName));

                _logger.LogWarning(
                    "慢查询检测: {QueryName}, 耗时: {ElapsedMs}ms (阈值: {ThresholdMs}ms)",
                    queryName, elapsedMs, _slowQueryThresholdMs);
            }

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();

            // 记录错误
            _queryErrorCount.Add(1,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("error_type", ex.GetType().Name));

            _queryCount.Add(1,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("status", "error"));

            _logger.LogError(ex,
                "数据库查询失败: {QueryName}, 耗时: {ElapsedMs}ms",
                queryName, sw.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}

