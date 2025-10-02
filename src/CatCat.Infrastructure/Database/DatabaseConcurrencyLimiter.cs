using Microsoft.Extensions.Logging;

namespace CatCat.Infrastructure.Database;

/// <summary>
/// Database concurrency limiter - Protect database from high concurrency overload
/// </summary>
public class DatabaseConcurrencyLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<DatabaseConcurrencyLimiter> _logger;
    private readonly TimeSpan _waitTimeout;

    public DatabaseConcurrencyLimiter(
        int maxConcurrency,
        TimeSpan waitTimeout,
        ILogger<DatabaseConcurrencyLimiter> logger)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _waitTimeout = waitTimeout;
        _logger = logger;
    }

    /// <summary>
    /// Execute database operation with concurrency limit
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string operationName = "Unknown",
        CancellationToken cancellationToken = default)
    {
        var acquired = await _semaphore.WaitAsync(_waitTimeout, cancellationToken);

        if (!acquired)
        {
            _logger.LogWarning("Database concurrency limit reached, operation rejected: {OperationName}", operationName);
            throw new InvalidOperationException("Database is busy, please try again later");
        }

        try
        {
            _logger.LogDebug("Execute database operation: {OperationName}, Current concurrency: {CurrentConcurrency}",
                operationName, _semaphore.CurrentCount);

            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Execute database operation (no return value)
    /// </summary>
    public async Task ExecuteAsync(
        Func<Task> operation,
        string operationName = "Unknown",
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return 0;
        }, operationName, cancellationToken);
    }

    /// <summary>
    /// Get current available concurrency
    /// </summary>
    public int AvailableConcurrency => _semaphore.CurrentCount;

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}

