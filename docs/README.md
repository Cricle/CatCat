# 📚 CatCat Documentation

Welcome to CatCat project documentation! This directory contains comprehensive guides and references for developers, operators, and contributors.

## 📖 Documentation Index

### 🚀 Getting Started
- **[Quick Start Guide](../README.md#⚡-快速开始)** - Start using CatCat in minutes
- **[Project Structure](PROJECT_STRUCTURE.md)** - Understand the codebase organization
- **[API Documentation](API.md)** - REST API endpoints and usage

### 🏗️ Architecture & Design
- **[Architecture Overview](ARCHITECTURE.md)** - System design and components
- **[NATS Peak Clipping](NATS_PEAK_CLIPPING.md)** - Async order processing with JetStream
- **[JWT Dual Token](JWT_DUAL_TOKEN.md)** - Authentication and token refresh mechanism
- **[AOT & Clustering](AOT_AND_CLUSTER.md)** - Ahead-of-Time compilation and scalability

### 🔧 Development
- **[Central Package Management](CENTRAL_PACKAGE_MANAGEMENT.md)** - NuGet package versioning
- **[Environment Configuration](ENVIRONMENT.md)** - Environment variables and settings
- **[MinIO Storage Guide](MINIO_STORAGE_GUIDE.md)** - Object storage for media files

### 📊 Observability & Performance
- **[OpenTelemetry Guide](OPENTELEMETRY_GUIDE.md)** - Distributed tracing and metrics
- **[Rate Limiting Guide](RATE_LIMITING_GUIDE.md)** - API protection and throttling

### 🚢 Deployment
- **[Docker & Aspire Guide](DOCKER_ASPIRE_GUIDE.md)** - Local development and Docker deployment
- **[Aspire K8s Deployment](ASPIRE_K8S_DEPLOYMENT.md)** - Production Kubernetes deployment

## 🗂️ Documentation by Role

### For Developers
Start here if you're contributing code:
1. [Quick Start](../README.md#⚡-快速开始) - Set up local environment
2. [Project Structure](PROJECT_STRUCTURE.md) - Navigate the codebase
3. [Architecture](ARCHITECTURE.md) - Understand design patterns
4. [API Documentation](API.md) - Learn API contracts
5. [Environment Config](ENVIRONMENT.md) - Configure your setup

### For DevOps Engineers
For deployment and operations:
1. [Docker & Aspire](DOCKER_ASPIRE_GUIDE.md) - Container deployment
2. [Kubernetes](ASPIRE_K8S_DEPLOYMENT.md) - Production orchestration
3. [OpenTelemetry](OPENTELEMETRY_GUIDE.md) - Monitoring setup
4. [AOT & Clustering](AOT_AND_CLUSTER.md) - Performance optimization

### For Architects
For system design and decision making:
1. [Architecture Overview](ARCHITECTURE.md) - System design
2. [NATS Peak Clipping](NATS_PEAK_CLIPPING.md) - Message queue patterns
3. [JWT Dual Token](JWT_DUAL_TOKEN.md) - Security architecture
4. [Rate Limiting](RATE_LIMITING_GUIDE.md) - API protection strategies
5. [Bloom Filter](BLOOM_FILTER_GUIDE.md) - Cache penetration protection

## 📋 Key Features Documentation

### Async Order Processing
Learn about the order queue system:
- [NATS Peak Clipping Guide](NATS_PEAK_CLIPPING.md)
- [Architecture - Order Flow](ARCHITECTURE.md#订单流程)

### Authentication & Security
Understand the auth system:
- [JWT Dual Token Mechanism](JWT_DUAL_TOKEN.md)
- [Rate Limiting Configuration](RATE_LIMITING_GUIDE.md)

### Observability
Monitor and debug the system:
- [OpenTelemetry Integration](OPENTELEMETRY_GUIDE.md)
- [Custom Metrics](OPENTELEMETRY_GUIDE.md#custom-metrics)

### Deployment Options
Choose your deployment strategy:
- **Local Development**: [Docker Compose](DOCKER_ASPIRE_GUIDE.md#docker-compose-部署)
- **Aspire Dashboard**: [.NET Aspire](DOCKER_ASPIRE_GUIDE.md#aspire-orchestration)
- **Production**: [Kubernetes](ASPIRE_K8S_DEPLOYMENT.md)

## 🔍 Quick References

### Technology Stack
- **Backend**: ASP.NET Core 9 (Minimal API), Sqlx (Source Generator), C# 12
- **Database**: PostgreSQL 16, Redis 7
- **Caching**: FusionCache (L1+L2 hybrid, ~85% hit rate), Bloom Filter (cache penetration protection)
- **Message Queue**: NATS JetStream 2.10
- **Object Storage**: MinIO (S3-compatible, media files)
- **Frontend**: Vue 3, TypeScript, Vuestic UI + Vant
- **Observability**: OpenTelemetry, Jaeger
- **Deployment**: Docker, .NET Aspire, Kubernetes, YARP Gateway

### Important Links
- [Main README](../README.md) - Project overview
- [Contributing Guide](../CONTRIBUTING.md) - Contribution guidelines
- [License](../LICENSE) - MIT License

### Useful Commands
```bash
# Build project
./build.sh  # or .\build.ps1 on Windows

# Start with Aspire
dotnet run --project src/CatCat.AppHost

# Start with Docker Compose
docker-compose up -d

# Deploy to Kubernetes
./scripts/deploy-to-k8s.sh your-registry.io
```

## 🆕 Recent Updates

### Latest Documentation (October 2024)
- ✅ **Aspire K8s Deployment** - Complete Kubernetes deployment guide
- ✅ **JWT Dual Token** - Enhanced authentication mechanism
- ✅ **NATS Peak Clipping** - Async order processing patterns
- ✅ **OpenTelemetry** - Distributed tracing guide

### Latest Code Improvements
- ✅ **C# 12 Primary Constructors** - All service classes refactored, 80+ lines reduced
- ✅ **FusionCache Integration** - L1+L2 hybrid cache with 85% hit rate
- ✅ **Web UI/UX Optimization** - Flat design, skeleton loading, consistent interactions
- ✅ **Static Endpoint Methods** - Clearer API routing definitions
- ✅ **Result Pattern** - Unified error handling, no exception throwing

### Deprecated Documentation
The following documents have been removed as they're outdated:
- ❌ `optimization-history/*` - Superseded by current implementation
- ❌ `OPTIMIZATION_SUMMARY.md` - Outdated optimization notes
- ❌ `DEPLOYMENT.md` - Replaced by Docker/Aspire guides

## 📝 Documentation Standards

When contributing documentation:
1. **Use English** for all technical content
2. **Add examples** for complex concepts
3. **Keep it updated** when code changes
4. **Use clear headings** for navigation
5. **Include diagrams** for architecture

## 🤝 Contributing to Docs

Found an issue or want to improve docs?
1. Open an issue describing the problem
2. Submit a PR with your improvements
3. Follow the documentation standards above

## 📞 Support

Need help?
- 💬 [GitHub Discussions](https://github.com/your-org/CatCat/discussions)
- 🐛 [Report Issues](https://github.com/your-org/CatCat/issues)
- 📧 Contact maintainers

---

**Last Updated**: 2024-10
**Maintained by**: CatCat Team

