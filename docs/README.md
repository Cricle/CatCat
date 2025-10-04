# 📚 CatCat Documentation

Welcome to CatCat! This directory contains essential guides for developers and operators.

## 📖 Core Documentation

### Quick Start
- **[Main README](../README.md)** - Project overview and quick start
- **[API Documentation](API.md)** - REST API endpoints reference
- **[Architecture](ARCHITECTURE.md)** - System design and patterns

### Configuration
- **[Environment](ENVIRONMENT.md)** - Environment variables and settings
- **[Central Packages](CENTRAL_PACKAGE_MANAGEMENT.md)** - NuGet package management
- **[Rate Limiting](RATE_LIMITING_GUIDE.md)** - API protection configuration
- **[I18n](I18N_GUIDE.md)** - Multi-language support

### Infrastructure
- **[MinIO Storage](MINIO_STORAGE_GUIDE.md)** - Object storage for media files
- **[OpenTelemetry](OPENTELEMETRY_GUIDE.md)** - Distributed tracing and metrics

## 🚀 Quick Commands

```bash
# Development
dotnet run --project src/CatCat.AppHost  # Start with Aspire
docker-compose up -d                     # Start with Docker

# Build
dotnet build
dotnet test

# Frontend
cd src/CatCat.Web
npm install
npm run dev
```

## 📋 Technology Stack

- **Backend**: ASP.NET Core 9, PostgreSQL, Redis, NATS
- **Frontend**: Vue 3, Vuestic Admin, TypeScript
- **Observability**: OpenTelemetry, Prometheus, Grafana
- **Deployment**: Docker, Aspire, Kubernetes

## 📞 Support

- 💬 [GitHub Discussions](https://github.com/your-org/CatCat/discussions)
- 🐛 [Report Issues](https://github.com/your-org/CatCat/issues)

---

**Last Updated**: 2024-10
