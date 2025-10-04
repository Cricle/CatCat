using CatCat.Transit.Results;
using Microsoft.Extensions.Logging;

namespace CatCat.Transit.Saga;

/// <summary>
/// Saga 编排器 - 管理 Saga 的执行和补偿
/// </summary>
public class SagaOrchestrator<TData> where TData : class, new()
{
    private readonly List<ISagaStep<TData>> _steps = new();
    private readonly ISagaRepository _repository;
    private readonly ILogger<SagaOrchestrator<TData>> _logger;

    public SagaOrchestrator(
        ISagaRepository repository,
        ILogger<SagaOrchestrator<TData>> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// 添加步骤
    /// </summary>
    public SagaOrchestrator<TData> AddStep(ISagaStep<TData> step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// 执行 Saga
    /// </summary>
    public async Task<TransitResult> ExecuteAsync(
        ISaga<TData> saga,
        CancellationToken cancellationToken = default)
    {
        saga.State = SagaState.Running;
        saga.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveAsync(saga, cancellationToken);

        var executedSteps = new Stack<ISagaStep<TData>>();

        try
        {
            // 依次执行所有步骤
            foreach (var step in _steps)
            {
                _logger.LogInformation("Executing saga step: {StepName} for {CorrelationId}", 
                    step.Name, saga.CorrelationId);

                var result = await step.ExecuteAsync(saga, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Saga step {StepName} failed, starting compensation", step.Name);
                    
                    // 步骤失败，开始补偿
                    await CompensateAsync(saga, executedSteps, cancellationToken);
                    
                    saga.State = SagaState.Compensated;
                    saga.UpdatedAt = DateTime.UtcNow;
                    await _repository.SaveAsync(saga, cancellationToken);
                    
                    return TransitResult.Failure($"Saga step {step.Name} failed: {result.Error}");
                }

                executedSteps.Push(step);
                saga.UpdatedAt = DateTime.UtcNow;
                await _repository.SaveAsync(saga, cancellationToken);
            }

            // 所有步骤成功
            saga.State = SagaState.Completed;
            saga.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveAsync(saga, cancellationToken);

            _logger.LogInformation("Saga {CorrelationId} completed successfully", saga.CorrelationId);
            return TransitResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {CorrelationId} failed with exception", saga.CorrelationId);
            
            // 异常，开始补偿
            await CompensateAsync(saga, executedSteps, cancellationToken);
            
            saga.State = SagaState.Failed;
            saga.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveAsync(saga, cancellationToken);
            
            return TransitResult.Failure($"Saga failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 补偿已执行的步骤
    /// </summary>
    private async Task CompensateAsync(
        ISaga<TData> saga,
        Stack<ISagaStep<TData>> executedSteps,
        CancellationToken cancellationToken)
    {
        saga.State = SagaState.Compensating;
        saga.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveAsync(saga, cancellationToken);

        // 反向补偿
        while (executedSteps.Count > 0)
        {
            var step = executedSteps.Pop();
            
            try
            {
                _logger.LogInformation("Compensating saga step: {StepName}", step.Name);
                await step.CompensateAsync(saga, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compensate step {StepName}", step.Name);
                // 继续补偿其他步骤
            }
        }
    }
}

