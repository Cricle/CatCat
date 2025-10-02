#!/usr/bin/env pwsh

Write-Host "🐱 Starting CatCat Development Environment..." -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
try {
    docker info | Out-Null
} catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Stop any running containers
Write-Host "🛑 Stopping any running containers..." -ForegroundColor Yellow
docker-compose down

# Start services with override for development
Write-Host "🚀 Starting services..." -ForegroundColor Green
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d postgres redis nats

# Wait for services to be healthy
Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Show service status
Write-Host ""
Write-Host "📊 Service Status:" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "✅ Development environment is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Available Services:" -ForegroundColor Cyan
Write-Host "  PostgreSQL:    localhost:15432 (user: catcat, password: dev_password)"
Write-Host "  Redis:         localhost:16379"
Write-Host "  NATS:          localhost:14222"
Write-Host "  NATS Monitor:  http://localhost:18222"
Write-Host "  Adminer:       http://localhost:8080"
Write-Host "  Redis UI:      http://localhost:8081"
Write-Host ""
Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Run API:    dotnet run --project src/CatCat.API"
Write-Host "  2. Run Web:    cd src/CatCat.Web && npm run dev"
Write-Host "  3. Or use:     dotnet run --project src/CatCat.AppHost (Aspire)"
Write-Host ""
Write-Host "🛑 To stop all services: docker-compose down" -ForegroundColor Yellow

