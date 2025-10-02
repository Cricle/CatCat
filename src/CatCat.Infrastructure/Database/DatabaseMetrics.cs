using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CatCat.Infrastructure.Database;

/// <summary>
/// Database performance monitoring metrics
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
            description: "Database query duration");

        _queryCount = meter.CreateCounter<long>(
            "database.query.count",
            description: "Database query count");

        _slowQueryCount = meter.CreateCounter<long>(
            "database.slow_query.count",
            description: "Slow query count");

        _queryErrorCount = meter.CreateCounter<long>(
            "database.query.error.count",
            description: "Query error count");
    }

    /// <summary>
    /// Record query performance metrics
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

            // Record query duration
            _queryDuration.Record(elapsedMs,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("status", "success"));

            // Record query count
            _queryCount.Add(1,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("status", "success"));

            // Slow query warning
            if (elapsedMs > _slowQueryThresholdMs)
            {
                _slowQueryCount.Add(1,
                    new KeyValuePair<string, object?>("query", queryName));

                _logger.LogWarning(
                    "Slow query detected: {QueryName}, Duration: {ElapsedMs}ms (Threshold: {ThresholdMs}ms)",
                    queryName, elapsedMs, _slowQueryThresholdMs);
            }

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();

            // Record error
            _queryErrorCount.Add(1,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("error_type", ex.GetType().Name));

            _queryCount.Add(1,
                new KeyValuePair<string, object?>("query", queryName),
                new KeyValuePair<string, object?>("status", "error"));

            _logger.LogError(ex,
                "Database query failed: {QueryName}, Duration: {ElapsedMs}ms",
                queryName, sw.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}

