#!/usr/bin/env pwsh
# Aspire Deploy to Kubernetes Script (Windows/PowerShell)

param(
    [string]$Registry = "your-registry.azurecr.io",
    [string]$Namespace = "catcat",
    [switch]$Build = $true,
    [switch]$Push = $true,
    [switch]$Deploy = $true
)

$ErrorActionPreference = "Stop"

Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   🚀 Aspire Deploy to Kubernetes              ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Check prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$commands = @("dotnet", "docker", "kubectl")
foreach ($cmd in $commands) {
    if (!(Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "❌ Error: $cmd is not installed" -ForegroundColor Red
        exit 1
    }
}
Write-Host "✅ All prerequisites installed`n" -ForegroundColor Green

# Check .NET Aspire workload
Write-Host "🔍 Checking .NET Aspire workload..." -ForegroundColor Yellow
$aspireInstalled = dotnet workload list | Select-String "aspire"
if (!$aspireInstalled) {
    Write-Host "⚠️ .NET Aspire workload not installed. Installing..." -ForegroundColor Yellow
    dotnet workload install aspire
    Write-Host "✅ Aspire workload installed`n" -ForegroundColor Green
} else {
    Write-Host "✅ Aspire workload already installed`n" -ForegroundColor Green
}

# Build container images
if ($Build) {
    Write-Host "🔨 Building container images..." -ForegroundColor Yellow
    
    # Build API
    Write-Host "  📦 Building CatCat.API..." -ForegroundColor Cyan
    docker build -t "$Registry/catcat-api:latest" -f src/CatCat.API/Dockerfile .
    
    # Build Web
    Write-Host "  📦 Building CatCat.Web..." -ForegroundColor Cyan
    docker build -t "$Registry/catcat-web:latest" -f src/CatCat.Web/Dockerfile .
    
    Write-Host "✅ Images built successfully`n" -ForegroundColor Green
}

# Push images to registry
if ($Push) {
    Write-Host "📤 Pushing images to registry..." -ForegroundColor Yellow
    
    docker push "$Registry/catcat-api:latest"
    docker push "$Registry/catcat-web:latest"
    
    Write-Host "✅ Images pushed successfully`n" -ForegroundColor Green
}

# Deploy to Kubernetes using Aspire
if ($Deploy) {
    Write-Host "🚀 Deploying to Kubernetes with Aspire..." -ForegroundColor Yellow
    
    # Create namespace if not exists
    kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -
    
    # Generate Kubernetes manifests using Aspire
    Write-Host "  📝 Generating Kubernetes manifests..." -ForegroundColor Cyan
    Push-Location src/CatCat.AppHost
    
    # Publish Aspire project to generate K8s manifests
    dotnet publish /t:GenerateKubernetesManifests `
        /p:Configuration=Release `
        /p:ContainerRegistry=$Registry `
        /p:KubernetesNamespace=$Namespace
    
    Pop-Location
    
    # Apply manifests
    Write-Host "  🔧 Applying Kubernetes manifests..." -ForegroundColor Cyan
    $manifestPath = "src/CatCat.AppHost/bin/Release/net9.0/manifest.yaml"
    
    if (Test-Path $manifestPath) {
        kubectl apply -f $manifestPath -n $Namespace
        Write-Host "✅ Manifests applied successfully`n" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Manifest file not found. Using dotnet run to deploy..." -ForegroundColor Yellow
        
        # Alternative: Use Aspire CLI to deploy
        Push-Location src/CatCat.AppHost
        dotnet run --publisher kubernetes --output-path ./k8s-manifests
        Pop-Location
        
        if (Test-Path "src/CatCat.AppHost/k8s-manifests") {
            kubectl apply -f src/CatCat.AppHost/k8s-manifests -n $Namespace
            Write-Host "✅ Deployed using Aspire CLI`n" -ForegroundColor Green
        }
    }
}

# Show deployment status
Write-Host "📊 Deployment Status:" -ForegroundColor Yellow
kubectl get all -n $Namespace

Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   ✅ Deployment Complete!                     ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "🔗 Useful commands:" -ForegroundColor Cyan
Write-Host "  • View pods:     kubectl get pods -n $Namespace" -ForegroundColor White
Write-Host "  • View services: kubectl get svc -n $Namespace" -ForegroundColor White
Write-Host "  • View logs:     kubectl logs -f deployment/catcat-api -n $Namespace" -ForegroundColor White
Write-Host "  • Port forward:  kubectl port-forward svc/catcat-api 8080:80 -n $Namespace`n" -ForegroundColor White

