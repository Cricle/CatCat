using CatCat.API.Configuration;
using CatCat.API.Endpoints;
using CatCat.API.Middleware;
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

// 初始化雪花ID生成器（集群环境需配置不同的 WorkerId）
var workerId = builder.Configuration.GetValue<ushort>("IdGenerator:WorkerId", 1);
SnowflakeIdGenerator.Initialize(workerId);

// 添加限流配置（防止接口击穿）
builder.Services.AddRateLimiting();

// 添加 OpenTelemetry 可观察性（支持 AOT）
builder.Services.AddOpenTelemetryObservability(builder.Configuration, "CatCat.API");
builder.Services.AddCustomActivitySources();
builder.Services.AddCustomMetrics();
builder.Services.AddSingleton<CustomMetrics>();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CatCat API",
        Version = "v1",
        Description = "上门喂猫服务平台 API"
    });
});

// Configure Database with Connection Pool and Protection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString!));

// 数据库并发限流器（保护数据库）
var maxConcurrency = builder.Configuration.GetValue<int>("Database:MaxConcurrency", 40);
builder.Services.AddSingleton(sp => new DatabaseConcurrencyLimiter(
    maxConcurrency: maxConcurrency,
    waitTimeout: TimeSpan.FromSeconds(5),
    logger: sp.GetRequiredService<ILogger<DatabaseConcurrencyLimiter>>()
));

// 数据库性能监控
var slowQueryThreshold = builder.Configuration.GetValue<double>("Database:SlowQueryThresholdMs", 1000);
builder.Services.AddSingleton(sp => new DatabaseMetrics(
    meterFactory: sp.GetRequiredService<IMeterFactory>(),
    logger: sp.GetRequiredService<ILogger<DatabaseMetrics>>(),
    slowQueryThresholdMs: slowQueryThreshold
));

// Configure FusionCache (混合缓存：内存 + Redis + Backplane)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
var redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.AddFusionCache();

// Configure NATS with AOT-compatible JSON serialization
var natsConnection = new NatsConnection(new NatsOpts
{
    Url = builder.Configuration.GetConnectionString("Nats")!
});
builder.Services.AddSingleton(natsConnection);
builder.Services.AddSingleton<IMessageQueueService>(sp =>
    new NatsService(natsConnection, CatCat.API.Json.AppJsonContext.Default));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Register Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// Configure JWT Authentication
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

// Configure CORS
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// 启用限流中间件（必须在 UseAuthentication 之前）
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Map Minimal API Endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPetEndpoints();
app.MapOrderEndpoints();
app.MapReviewEndpoints();

// Health Check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithTags("Health")
    .WithOpenApi();

app.Run();

