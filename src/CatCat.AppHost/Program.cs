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

// MinIO Object Storage
var minio = builder.AddContainer("minio", "minio/minio", "latest")
    .WithEnvironment("MINIO_ROOT_USER", "catcat")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "catcat_dev_password")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "console")
    .WithDataVolume("catcat-minio-data");

// CatCat API
var api = builder.AddProject<Projects.CatCat_API>("api")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(nats)
    .WithReference(minio)
    .WithEnvironment("Jwt__SecretKey", "dev-secret-key-change-in-production-at-least-32-chars")
    .WithEnvironment("Jwt__Issuer", "CatCat.API")
    .WithEnvironment("Jwt__Audience", "CatCat.Client")
    .WithEnvironment("Jwt__ExpiryMinutes", "1440")
    .WithEnvironment("Stripe__SecretKey", "sk_test_your_key_here")
    .WithEnvironment("Stripe__PublishableKey", "pk_test_your_key_here");

builder.Build().Run();
