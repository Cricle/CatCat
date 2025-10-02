namespace CatCat.API.Models;

/// <summary>
/// 统一 API 返回结果
/// </summary>
public class ApiResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int Code { get; set; }

    public static ApiResult Ok(string? message = null) => new()
    {
        Success = true,
        Message = message,
        Code = 200
    };

    public static ApiResult<T> Ok<T>(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        Code = 200
    };

    public static ApiResult Fail(string message, int code = 400) => new()
    {
        Success = false,
        Message = message,
        Code = code
    };

    public static ApiResult<T> Fail<T>(string message, int code = 400) => new()
    {
        Success = false,
        Message = message,
        Code = code
    };

    public static ApiResult NotFound(string message = "资源不存在") => new()
    {
        Success = false,
        Message = message,
        Code = 404
    };

    public static ApiResult Unauthorized(string message = "未授权") => new()
    {
        Success = false,
        Message = message,
        Code = 401
    };

    public static ApiResult Forbidden(string message = "禁止访问") => new()
    {
        Success = false,
        Message = message,
        Code = 403
    };
}

/// <summary>
/// 带数据的 API 返回结果
/// </summary>
public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }
}

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    public static PagedResult<T> Create(IEnumerable<T> items, int total, int page, int pageSize) => new()
    {
        Items = items,
        Total = total,
        Page = page,
        PageSize = pageSize
    };
}

