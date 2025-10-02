#!/usr/bin/env pwsh

Write-Host "🐱 Starting CatCat Production Environment..." -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
try {
    docker info | Out-Null
} catch {
    Write-Host "❌ Docker is not running. Please start Docker first." -ForegroundColor Red
    exit 1
}

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "⚠️  Warning: .env file not found. Using default values." -ForegroundColor Yellow
    Write-Host "   Please create .env file with production credentials!" -ForegroundColor Yellow
    Write-Host ""
}

# Pull latest images
Write-Host "📥 Pulling latest images..." -ForegroundColor Green
docker-compose pull

# Build custom images
Write-Host "🔨 Building application images..." -ForegroundColor Green
docker-compose build

# Start all services
Write-Host "🚀 Starting all services..." -ForegroundColor Green
docker-compose up -d

# Wait for services to be healthy
Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Show service status
Write-Host ""
Write-Host "📊 Service Status:" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "✅ Production environment is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Available Services:" -ForegroundColor Cyan
Write-Host "  API:           http://localhost:5000"
Write-Host "  Web:           http://localhost:3000"
Write-Host "  Jaeger UI:     http://localhost:16686"
Write-Host ""
Write-Host "📋 Logs:" -ForegroundColor Cyan
Write-Host "  View all:      docker-compose logs -f"
Write-Host "  View API:      docker-compose logs -f api"
Write-Host "  View Web:      docker-compose logs -f web"
Write-Host ""
Write-Host "🛑 To stop all services: docker-compose down" -ForegroundColor Yellow
Write-Host "🗑️  To remove volumes:    docker-compose down -v" -ForegroundColor Red

