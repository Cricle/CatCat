using CatCat.API.Configuration;
using CatCat.API.Endpoints;
using CatCat.API.Middleware;
using CatCat.API.Models;
using CatCat.API.Observability;
using CatCat.Core.Services;
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

// FusionCache: L1 (Memory) + L2 (Redis)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
var redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "CatCat:";
});

builder.Services.AddFusionCache()
    .WithSystemTextJsonSerializer(new System.Text.Json.JsonSerializerOptions
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = CatCat.API.Json.AppJsonContext.Default
    });

// NATS message queue
var natsConnection = new NatsConnection(new NatsOpts
{
    Url = builder.Configuration.GetConnectionString("Nats")!
});
builder.Services.AddSingleton(natsConnection);
builder.Services.AddSingleton<IMessageQueueService>(sp =>
    new NatsService(natsConnection, CatCat.API.Json.AppJsonContext.Default));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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

app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy", DateTime.UtcNow)))
    .WithTags("Health");

app.Run();

