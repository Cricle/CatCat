namespace CatCat.Infrastructure.Common;

/// <summary>
/// 操作结果 - 避免异常抛出，提升性能
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("成功结果不能有错误信息");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("失败结果必须有错误信息");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// 带返回值的操作结果
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// 如果成功则返回值，否则返回默认值
    /// </summary>
    public T? ValueOrDefault() => IsSuccess ? Value : default;

    /// <summary>
    /// 如果成功则返回值，否则返回指定的默认值
    /// </summary>
    public T ValueOr(T defaultValue) => IsSuccess && Value != null ? Value : defaultValue;
}

/// <summary>
/// Result 扩展方法
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// 链式处理成功结果
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> func)
    {
        return result.IsSuccess && result.Value != null
            ? func(result.Value)
            : Result.Failure<TOut>(result.Error!);
    }

    /// <summary>
    /// 链式处理成功结果（异步）
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> func)
    {
        return result.IsSuccess && result.Value != null
            ? await func(result.Value)
            : Result.Failure<TOut>(result.Error!);
    }

    /// <summary>
    /// 处理失败情况
    /// </summary>
    public static Result<T> OnFailure<T>(
        this Result<T> result,
        Action<string> action)
    {
        if (result.IsFailure)
            action(result.Error!);
        return result;
    }

    // ToApiResult 扩展方法已移至 CatCat.API 项目，避免循环引用
}

