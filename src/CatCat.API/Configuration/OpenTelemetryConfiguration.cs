using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CatCat.API.Configuration;

/// <summary>
/// OpenTelemetry 可观察性配置（支持 AOT）
/// </summary>
public static class OpenTelemetryConfiguration
{
    /// <summary>
    /// 添加 OpenTelemetry 可观察性支持
    /// </summary>
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName = "CatCat.API")
    {
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        // 配置 Resource（服务标识）
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
                ["host.name"] = Environment.MachineName,
                ["process.runtime.name"] = ".NET",
                ["process.runtime.version"] = Environment.Version.ToString()
            });

        // 获取 OTLP 导出配置
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";
        var useConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:UseConsoleExporter", false);

        // 添加 OpenTelemetry Tracing（分布式追踪）
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddAttributes(resourceBuilder.Build().Attributes))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    // ASP.NET Core 请求追踪
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            // 排除健康检查和 Swagger 端点
                            var path = httpContext.Request.Path.Value ?? string.Empty;
                            return !path.StartsWith("/health") &&
                                   !path.StartsWith("/swagger");
                        };
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.client_ip", httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString());
                            activity.SetTag("http.user_agent", httpRequest.Headers.UserAgent.ToString());
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response_content_length", httpResponse.ContentLength);
                        };
                    })
                    // HTTP Client 追踪
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.FilterHttpRequestMessage = request =>
                        {
                            // 排除对 OpenTelemetry Collector 的请求
                            return !request.RequestUri?.ToString().Contains("otlp") ?? true;
                        };
                    })
                    // 自定义 Source（应用内部追踪）
                    .AddSource("CatCat.*");

                // 导出到 OTLP（推荐用于生产环境）
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }

                // 控制台导出（用于开发和调试）
                if (useConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    // ASP.NET Core 指标
                    .AddAspNetCoreInstrumentation()
                    // HTTP Client 指标
                    .AddHttpClientInstrumentation()
                    // .NET Runtime 指标（内存、GC、线程池等）
                    .AddRuntimeInstrumentation()
                    // 自定义 Meter
                    .AddMeter("CatCat.*");

                // 导出到 OTLP
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }

                // 控制台导出
                if (useConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });

        return services;
    }

    /// <summary>
    /// 添加自定义活动源（用于应用内部追踪）
    /// </summary>
    public static IServiceCollection AddCustomActivitySources(this IServiceCollection services)
    {
        // 注册自定义 ActivitySource（用于手动追踪）
        services.AddSingleton(new System.Diagnostics.ActivitySource("CatCat.API"));
        services.AddSingleton(new System.Diagnostics.ActivitySource("CatCat.Infrastructure"));
        services.AddSingleton(new System.Diagnostics.ActivitySource("CatCat.Core"));

        return services;
    }

    /// <summary>
    /// 添加自定义指标（Meter）
    /// </summary>
    public static IServiceCollection AddCustomMetrics(this IServiceCollection services)
    {
        // 注册自定义 Meter（用于业务指标）
        services.AddSingleton(new System.Diagnostics.Metrics.Meter("CatCat.API", "1.0.0"));

        return services;
    }
}

