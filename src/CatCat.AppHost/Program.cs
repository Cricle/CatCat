var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("catcat-postgres-data");

var database = postgres.AddDatabase("catcat");

// Redis Cache
var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataVolume("catcat-redis-data");

// NATS with JetStream
var nats = builder.AddNats("nats")
    .WithDataVolume("catcat-nats-data");

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
    .WithEnvironment("Stripe__PublishableKey", "pk_test_your_key_here");

builder.Build().Run();
