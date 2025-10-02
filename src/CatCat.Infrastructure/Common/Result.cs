namespace CatCat.Infrastructure.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have error message");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have error message");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? ValueOrDefault() => IsSuccess ? Value : default;

    public T ValueOr(T defaultValue) => IsSuccess && Value != null ? Value : defaultValue;
}

public static class ResultExtensions
{
    public static Result<TOut> Then<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> func)
    {
        return result.IsSuccess && result.Value != null
            ? func(result.Value)
            : Result.Failure<TOut>(result.Error!);
    }

    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> func)
    {
        return result.IsSuccess && result.Value != null
            ? await func(result.Value)
            : Result.Failure<TOut>(result.Error!);
    }

    public static Result<T> OnFailure<T>(
        this Result<T> result,
        Action<string> action)
    {
        if (result.IsFailure)
            action(result.Error!);
        return result;
    }
}

