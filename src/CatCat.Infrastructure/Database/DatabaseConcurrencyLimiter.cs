using Microsoft.Extensions.Logging;

namespace CatCat.Infrastructure.Database;

/// <summary>
/// 数据库并发限流器 - 保护数据库不被高并发打垮
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
    /// 执行数据库操作（带并发限制）
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string operationName = "Unknown",
        CancellationToken cancellationToken = default)
    {
        var acquired = await _semaphore.WaitAsync(_waitTimeout, cancellationToken);

        if (!acquired)
        {
            _logger.LogWarning("数据库并发限制已满，操作被拒绝: {OperationName}", operationName);
            throw new InvalidOperationException("数据库繁忙，请稍后重试");
        }

        try
        {
            _logger.LogDebug("执行数据库操作: {OperationName}, 当前并发: {CurrentConcurrency}",
                operationName, _semaphore.CurrentCount);

            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 执行数据库操作（无返回值）
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
    /// 获取当前可用并发数
    /// </summary>
    public int AvailableConcurrency => _semaphore.CurrentCount;

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}

