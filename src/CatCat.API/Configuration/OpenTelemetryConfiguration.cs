using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CatCat.API.Configuration;

// OpenTelemetry observability configuration (AOT-compatible)
public static class OpenTelemetryConfiguration
{
    [RequiresUnreferencedCode("OpenTelemetry instrumentation may require unreferenced code")]
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName = "CatCat.API")
    {
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        // Configure Resource (Service identification)
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

        // Get OTLP export configuration
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";
        var useConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:UseConsoleExporter", false);

        // Add OpenTelemetry Tracing (distributed tracing)
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddAttributes(resourceBuilder.Build().Attributes))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    // ASP.NET Core request tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            // Exclude health check and Swagger endpoints
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
                    // HTTP Client tracing
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.FilterHttpRequestMessage = request =>
                        {
                            // Exclude requests to OpenTelemetry Collector
                            return !request.RequestUri?.ToString().Contains("otlp") ?? true;
                        };
                    })
                    // Custom source (application internal tracing)
                    .AddSource("CatCat.*");

                // Export to OTLP (recommended for production)
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }

                // Console export (for development and debugging)
                if (useConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    // ASP.NET Core metrics
                    .AddAspNetCoreInstrumentation()
                    // HTTP Client metrics
                    .AddHttpClientInstrumentation()
                    // .NET Runtime metrics (memory, GC, thread pool, etc.)
                    .AddRuntimeInstrumentation()
                    // Custom Meter
                    .AddMeter("CatCat.*");

                // Export to OTLP
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }

                // Console export
                if (useConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });

        return services;
    }

    public static IServiceCollection AddCustomActivitySources(this IServiceCollection services)
    {
        // Register custom ActivitySource (for manual tracing)
        services.AddSingleton(new System.Diagnostics.ActivitySource("CatCat.API"));
        services.AddSingleton(new System.Diagnostics.ActivitySource("CatCat.Infrastructure"));
        services.AddSingleton(new System.Diagnostics.ActivitySource("CatCat.Core"));

        return services;
    }

    public static IServiceCollection AddCustomMetrics(this IServiceCollection services)
    {
        // Register custom Meter (for business metrics)
        services.AddSingleton(new System.Diagnostics.Metrics.Meter("CatCat.API", "1.0.0"));

        return services;
    }
}
