using System.Net;
using CatCat.API.Json;
using CatCat.API.Models;
using CatCat.Infrastructure.Common;
using System.Text.Json;

namespace CatCat.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            BusinessException => new ErrorResponse(HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException => new ErrorResponse(HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => new ErrorResponse(HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => new ErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized access"),
            KeyNotFoundException => new ErrorResponse(HttpStatusCode.NotFound, "Resource not found"),
            _ => new ErrorResponse(HttpStatusCode.InternalServerError, "Internal server error")
        };

        // Log at appropriate level
        if (errorResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("Business exception: {Type} - {Message}", exception.GetType().Name, exception.Message);
        }

        context.Response.StatusCode = (int)errorResponse.StatusCode;

        var response = ApiResult.Fail(errorResponse.Message);

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, AppJsonContext.Default.ApiResultObject));
    }

    private record ErrorResponse(HttpStatusCode StatusCode, string Message);
}

