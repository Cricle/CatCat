var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres", "16-alpine")
    .WithEnvironment("POSTGRES_INITDB_ARGS", "--encoding=UTF8")
    .WithDataVolume();

var database = postgres.AddDatabase("catcat");

// Redis Cache
var redis = builder.AddRedis("redis")
    .WithImage("redis", "7-alpine")
    .WithRedisCommander()
    .WithDataVolume();

// NATS with JetStream
var nats = builder.AddNats("nats")
    .WithImage("nats", "2.10-alpine")
    .WithDataVolume();

// MinIO Object Storage
var minio = builder.AddContainer("minio", "minio/minio", "latest")
    .WithEnvironment("MINIO_ROOT_USER", "catcat")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "catcat_dev_password")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "console")
    .WithBindMount("minio-data", "/data");

// Jaeger for distributed tracing
var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one", "latest")
    .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
    .WithHttpEndpoint(port: 16686, targetPort: 16686, name: "ui")
    .WithHttpEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc")
    .WithHttpEndpoint(port: 4318, targetPort: 4318, name: "otlp-http");

// Prometheus for metrics
var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "latest")
    .WithArgs(
        "--config.file=/etc/prometheus/prometheus.yml",
        "--storage.tsdb.path=/prometheus",
        "--storage.tsdb.retention.time=30d")
    .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "web")
    .WithBindMount("./prometheus.yml", "/etc/prometheus/prometheus.yml")
    .WithBindMount("prometheus-data", "/prometheus");

// Grafana for visualization
var grafana = builder.AddContainer("grafana", "grafana/grafana", "latest")
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WithHttpEndpoint(port: 3001, targetPort: 3000, name: "web")
    .WithBindMount("grafana-data", "/var/lib/grafana");

// CatCat API
var api = builder.AddProject<Projects.CatCat_API>("api")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(nats)
    // JWT
    .WithEnvironment("Jwt__SecretKey", "dev-secret-key-change-in-production-at-least-32-chars")
    .WithEnvironment("Jwt__Issuer", "CatCat.API")
    .WithEnvironment("Jwt__Audience", "CatCat.Client")
    .WithEnvironment("Jwt__ExpiryMinutes", "1440")
    // Stripe
    .WithEnvironment("Stripe__SecretKey", "sk_test_your_key_here")
    .WithEnvironment("Stripe__PublishableKey", "pk_test_your_key_here")
    .WithEnvironment("Stripe__WebhookSecret", "whsec_your_secret_here")
    // MinIO
    .WithEnvironment("MinIO__Endpoint", "minio:9000")
    .WithEnvironment("MinIO__AccessKey", "catcat")
    .WithEnvironment("MinIO__SecretKey", "catcat_dev_password")
    .WithEnvironment("MinIO__BucketName", "catcat-media")
    .WithEnvironment("MinIO__UseSSL", "false")
    // OpenTelemetry
    .WithEnvironment("OpenTelemetry__ServiceName", "CatCat.API")
    .WithEnvironment("OpenTelemetry__Endpoint", "http://jaeger:4317");

builder.Build().Run();
