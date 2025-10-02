var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres", "16-alpine")
    .WithEnvironment("POSTGRES_DB", "catcat")
    .WithEnvironment("POSTGRES_USER", "catcat")
    .WithEnvironment("POSTGRES_PASSWORD", "catcat_dev_password")
    .WithDataVolume("catcat-postgres-data")
    .WithHealthCheck();

var database = postgres.AddDatabase("database", "catcat");

// Redis Cache
var redis = builder.AddRedis("redis")
    .WithImage("redis", "7-alpine")
    .WithRedisCommander()
    .WithDataVolume("catcat-redis-data")
    .WithHealthCheck();

// NATS with JetStream
var nats = builder.AddNats("nats")
    .WithImage("nats", "2.10-alpine")
    .WithArgs("-js", "-sd", "/data", "-m", "8222")
    .WithDataVolume("catcat-nats-data")
    .WithHealthCheck();

// CatCat API
var api = builder.AddProject<Projects.CatCat_API>("api")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(nats)
    .WithEnvironment("Jwt__SecretKey", "dev-secret-key-change-in-production-at-least-32-chars")
    .WithEnvironment("Jwt__Issuer", "CatCat.API")
    .WithEnvironment("Jwt__Audience", "CatCat.Client")
    .WithEnvironment("Jwt__ExpiryMinutes", "1440")
    .WithEnvironment("Stripe__SecretKey", "sk_test_your_key_here")
    .WithEnvironment("Stripe__PublishableKey", "pk_test_your_key_here")
    .WithHttpsEndpoint(env: "ASPNETCORE_HTTPS_PORTS")
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS");

builder.Build().Run();
