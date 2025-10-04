using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatCat.Transit.CatGa;

/// <summary>
/// CatGa 执行器 - 高性能实现
/// 特点：无锁、非阻塞、内置幂等性、自动重试、自动补偿
/// </summary>
public sealed class CatGaExecutor : ICatGaExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CatGaExecutor> _logger;
    private readonly CatGaIdempotencyStore _idempotencyStore;
    private readonly CatGaOptions _options;

    public CatGaExecutor(
        IServiceProvider serviceProvider,
        ILogger<CatGaExecutor> logger,
        CatGaIdempotencyStore idempotencyStore,
        CatGaOptions options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _idempotencyStore = idempotencyStore;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<CatGaResult<TResponse>> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        CatGaContext? context = null,
        CancellationToken cancellationToken = default)
    {
        context ??= new CatGaContext();

        // 1. 幂等性检查
        if (_options.IdempotencyEnabled && !string.IsNullOrEmpty(context.IdempotencyKey))
        {
            if (_idempotencyStore.TryGetCachedResult<TResponse>(context.IdempotencyKey, out var cachedResult))
            {
                _logger.LogDebug("Idempotency hit for key {Key}", context.IdempotencyKey);
                return CatGaResult<TResponse>.Success(cachedResult!, context);
            }
        }

        // 2. 获取事务实例
        var transaction = _serviceProvider.GetRequiredService<ICatGaTransaction<TRequest, TResponse>>();

        // 3. 执行主操作（带重试）
        var sw = Stopwatch.StartNew();
        var executeResult = await ExecuteWithRetryAsync(
            transaction,
            request,
            context,
            cancellationToken);

        sw.Stop();

        // 4. 处理结果
        if (executeResult.IsSuccess)
        {
            // 成功：缓存结果
            if (_options.IdempotencyEnabled && !string.IsNullOrEmpty(context.IdempotencyKey))
            {
                _idempotencyStore.CacheResult(context.IdempotencyKey, executeResult.Value);
            }

            _logger.LogInformation(
                "CatGa transaction {TransactionId} completed successfully in {Elapsed}ms",
                context.TransactionId, sw.ElapsedMilliseconds);

            return executeResult;
        }
        else
        {
            // 失败：尝试补偿
            _logger.LogWarning(
                "CatGa transaction {TransactionId} failed: {Error}, attempting compensation",
                context.TransactionId, executeResult.Error);

            var compensationResult = await CompensateAsync(
                transaction,
                request,
                context,
                cancellationToken);

            if (compensationResult)
            {
                return CatGaResult<TResponse>.Compensated(
                    executeResult.Error ?? "Unknown error",
                    context);
            }
            else
            {
                _logger.LogError(
                    "CatGa transaction {TransactionId} compensation failed",
                    context.TransactionId);

                return CatGaResult<TResponse>.Failure(
                    $"{executeResult.Error} (Compensation also failed)",
                    context);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<CatGaResult> ExecuteAsync<TRequest>(
        TRequest request,
        CatGaContext? context = null,
        CancellationToken cancellationToken = default)
    {
        context ??= new CatGaContext();

        // 幂等性检查
        if (_options.IdempotencyEnabled && !string.IsNullOrEmpty(context.IdempotencyKey))
        {
            if (_idempotencyStore.IsProcessed(context.IdempotencyKey))
            {
                _logger.LogDebug("Idempotency hit for key {Key}", context.IdempotencyKey);
                return CatGaResult.Success(context);
            }
        }

        var transaction = _serviceProvider.GetRequiredService<ICatGaTransaction<TRequest>>();

        var sw = Stopwatch.StartNew();
        var executeResult = await ExecuteWithRetryAsync(
            transaction,
            request,
            context,
            cancellationToken);

        sw.Stop();

        if (executeResult)
        {
            if (_options.IdempotencyEnabled && !string.IsNullOrEmpty(context.IdempotencyKey))
            {
                _idempotencyStore.MarkProcessed(context.IdempotencyKey);
            }

            _logger.LogInformation(
                "CatGa transaction {TransactionId} completed in {Elapsed}ms",
                context.TransactionId, sw.ElapsedMilliseconds);

            return CatGaResult.Success(context);
        }
        else
        {
            var compensationResult = await CompensateAsync(
                transaction,
                request,
                context,
                cancellationToken);

            return compensationResult
                ? CatGaResult.Compensated("Operation failed but compensated", context)
                : CatGaResult.Failure("Operation and compensation failed", context);
        }
    }

    // 带重试的执行
    private async Task<CatGaResult<TResponse>> ExecuteWithRetryAsync<TRequest, TResponse>(
        ICatGaTransaction<TRequest, TResponse> transaction,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= context.MaxRetries; attempt++)
        {
            try
            {
                context.RetryCount = attempt;

                var result = await transaction.ExecuteAsync(request, cancellationToken);
                return CatGaResult<TResponse>.Success(result, context);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex,
                    "CatGa transaction {TransactionId} attempt {Attempt} failed",
                    context.TransactionId, attempt + 1);

                if (attempt < context.MaxRetries)
                {
                    var delay = CalculateRetryDelay(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        return CatGaResult<TResponse>.Failure(
            lastException?.Message ?? "Unknown error",
            context);
    }

    // 带重试的执行（无返回值）
    private async Task<bool> ExecuteWithRetryAsync<TRequest>(
        ICatGaTransaction<TRequest> transaction,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt <= context.MaxRetries; attempt++)
        {
            try
            {
                context.RetryCount = attempt;
                await transaction.ExecuteAsync(request, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "CatGa transaction {TransactionId} attempt {Attempt} failed",
                    context.TransactionId, attempt + 1);

                if (attempt < context.MaxRetries)
                {
                    var delay = CalculateRetryDelay(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        return false;
    }

    // 补偿
    private async Task<bool> CompensateAsync<TRequest, TResponse>(
        ICatGaTransaction<TRequest, TResponse> transaction,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken)
    {
        if (!_options.AutoCompensate)
            return false;

        try
        {
            await transaction.CompensateAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CatGa compensation failed for transaction {TransactionId}",
                context.TransactionId);
            return false;
        }
    }

    private async Task<bool> CompensateAsync<TRequest>(
        ICatGaTransaction<TRequest> transaction,
        TRequest request,
        CatGaContext context,
        CancellationToken cancellationToken)
    {
        if (!_options.AutoCompensate)
            return false;

        try
        {
            await transaction.CompensateAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CatGa compensation failed for transaction {TransactionId}",
                context.TransactionId);
            return false;
        }
    }

    // 计算重试延迟（指数退避 + Jitter）
    private TimeSpan CalculateRetryDelay(int attempt)
    {
        var baseDelay = _options.InitialRetryDelay.TotalMilliseconds;
        var exponentialDelay = baseDelay * Math.Pow(2, attempt);
        var maxDelay = _options.MaxRetryDelay.TotalMilliseconds;

        var delay = Math.Min(exponentialDelay, maxDelay);

        if (_options.UseJitter)
        {
            var jitter = Random.Shared.NextDouble() * delay * 0.2; // ±20% jitter
            delay += jitter;
        }

        return TimeSpan.FromMilliseconds(delay);
    }
}

