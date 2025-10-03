using System.Diagnostics;

namespace CatCat.API.Middleware;

/// <summary>
/// 全链路追踪中间件 - 为每个请求添加追踪信息
/// </summary>
public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ActivitySource _activitySource;

    public TracingMiddleware(RequestDelegate next, ActivitySource activitySource)
    {
        _next = next;
        _activitySource = activitySource;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        
        if (activity != null)
        {
            // 添加用户信息
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("sub")?.Value
                           ?? context.User.FindFirst("user_id")?.Value
                           ?? context.User.FindFirst("id")?.Value;
                
                if (userId != null)
                {
                    activity.SetTag("user.id", userId);
                }

                var userName = context.User.Identity.Name;
                if (userName != null)
                {
                    activity.SetTag("user.name", userName);
                }

                var role = context.User.FindFirst("role")?.Value;
                if (role != null)
                {
                    activity.SetTag("user.role", role);
                }
            }

            // 添加请求信息
            activity.SetTag("http.request_id", context.TraceIdentifier);
            activity.SetTag("http.method", context.Request.Method);
            activity.SetTag("http.path", context.Request.Path);
            activity.SetTag("http.query_string", context.Request.QueryString.ToString());
            
            // 添加客户端信息
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (clientIp != null)
            {
                activity.SetTag("client.ip", clientIp);
            }

            var userAgent = context.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrEmpty(userAgent))
            {
                activity.SetTag("client.user_agent", userAgent);
            }
        }

        await _next(context);

        // 添加响应信息
        if (activity != null)
        {
            activity.SetTag("http.status_code", context.Response.StatusCode);
            
            // 根据状态码设置 Activity 状态
            if (context.Response.StatusCode >= 400)
            {
                activity.SetStatus(
                    context.Response.StatusCode >= 500 ? ActivityStatusCode.Error : ActivityStatusCode.Ok,
                    $"HTTP {context.Response.StatusCode}");
            }
        }
    }
}

