using CatCat.API.Configuration;
using CatCat.API.Endpoints;
using CatCat.API.Extensions;
using CatCat.API.Middleware;
using CatCat.API.Models;
using CatCat.API.Observability;
using CatCat.Infrastructure.Services;
using CatCat.Infrastructure.Database;
using CatCat.Infrastructure.IdGenerator;
using CatCat.Infrastructure.MessageQueue;
using CatCat.Infrastructure.Payment;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NATS.Client.Core;
using StackExchange.Redis;
using System.Diagnostics.Metrics;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);

// Initialize Snowflake ID generator (configure different WorkerId for cluster)
var workerId = builder.Configuration.GetValue<ushort>("IdGenerator:WorkerId", 1);
SnowflakeIdGenerator.Initialize(workerId);

// Rate limiting
builder.Services.AddRateLimiting();

// OpenTelemetry observability
builder.Services.AddOpenTelemetryObservability(builder.Configuration, "CatCat.API");
builder.Services.AddCustomActivitySources();
builder.Services.AddCustomMetrics();
builder.Services.AddSingleton<CustomMetrics>();

builder.Services.AddEndpointsApiExplorer();

// OpenAPI (AOT-compatible)
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "CatCat API",
            Version = "v1",
            Description = "Cat-sitting service platform API - AOT Compiled"
        };
        return Task.CompletedTask;
    });
});

#if DEBUG
// Swagger UI (Debug only)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CatCat API",
        Version = "v1",
        Description = "Cat-sitting service platform API (Debug Mode)"
    });
});
#endif

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString!));

// Database concurrency limiter
var maxConcurrency = builder.Configuration.GetValue<int>("Database:MaxConcurrency", 40);
builder.Services.AddSingleton(sp => new DatabaseConcurrencyLimiter(
    maxConcurrency: maxConcurrency,
    waitTimeout: TimeSpan.FromSeconds(5),
    logger: sp.GetRequiredService<ILogger<DatabaseConcurrencyLimiter>>()
));

// Database metrics
var slowQueryThreshold = builder.Configuration.GetValue<double>("Database:SlowQueryThresholdMs", 1000);
builder.Services.AddSingleton(sp => new DatabaseMetrics(
    meterFactory: sp.GetRequiredService<IMeterFactory>(),
    logger: sp.GetRequiredService<ILogger<DatabaseMetrics>>(),
    slowQueryThresholdMs: slowQueryThreshold
));

// FusionCache: L2 (Redis Only) - No memory cache for clustering
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
var redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "CatCat:";
});

// FusionCache with Redis-based distributed cache
// Note: We use Redis Sets for Bloom Filter (zero memory usage)
// FusionCache uses L2 (Redis) primarily, with minimal L1 (memory) usage

builder.Services.AddFusionCache()
    .WithSystemTextJsonSerializer(new System.Text.Json.JsonSerializerOptions
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = CatCat.API.Json.AppJsonContext.Default
    });

// NATS JetStream message queue
var natsConnection = new NatsConnection(new NatsOpts
{
    Url = builder.Configuration.GetConnectionString("Nats")!
});
builder.Services.AddSingleton(natsConnection);
builder.Services.AddSingleton<JetStreamConfiguration>();
builder.Services.AddSingleton<IMessageQueueService, JetStreamService>(sp =>
    new JetStreamService(
        natsConnection,
        CatCat.API.Json.AppJsonContext.Default,
        sp.GetRequiredService<ILogger<JetStreamService>>()));

// MinIO Object Storage
builder.Services.AddSingleton<CatCat.Infrastructure.Storage.IStorageService, CatCat.Infrastructure.Storage.MinioStorageService>();

// Repositories & Services
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// JWT Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
#if DEBUG
    app.UseSwagger();
    app.UseSwaggerUI();
#endif
}

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPetEndpoints();
app.MapOrderEndpoints();
app.MapReviewEndpoints();
app.MapAdminEndpoints();
app.MapServiceProgressEndpoints();
app.MapStorageEndpoints();

app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy", DateTime.UtcNow)))
    .WithTags("Health");

// Redis-based Bloom Filter requires no initialization
// All IDs are persisted in Redis Sets automatically

// Initialize JetStream streams on startup
var jetStreamConfig = app.Services.GetRequiredService<JetStreamConfiguration>();
await jetStreamConfig.InitializeStreamsAsync();

// Initialize MinIO bucket on startup
var storageService = app.Services.GetRequiredService<CatCat.Infrastructure.Storage.IStorageService>();
if (storageService is CatCat.Infrastructure.Storage.MinioStorageService minioService)
{
    await minioService.InitializeAsync();
}

app.Run();

