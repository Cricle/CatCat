#!/usr/bin/env pwsh
# CatCat Full Stack Development
# Start both API and Web in parallel

Write-Host ""
Write-Host "🐱 CatCat Full Stack Development" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found! Please install .NET 9+" -ForegroundColor Red
    exit 1
}

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found! Please install Node.js 18+" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🚀 Starting services..." -ForegroundColor Yellow
Write-Host ""

# Function to start API
$apiJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    Set-Location src/CatCat.API
    dotnet run
}

Write-Host "✅ API Server: http://localhost:5000" -ForegroundColor Green
Write-Host "   Job ID: $($apiJob.Id)" -ForegroundColor Gray

# Function to start Web
$webJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    Set-Location src/CatCat.Web
    npm run dev
}

Write-Host "✅ Web Server: http://localhost:5173" -ForegroundColor Green
Write-Host "   Job ID: $($webJob.Id)" -ForegroundColor Gray

Write-Host ""
Write-Host "🎨 New UI Features:" -ForegroundColor Magenta
Write-Host "  • Beautiful gradient design" -ForegroundColor White
Write-Host "  • Card-based layout" -ForegroundColor White
Write-Host "  • Responsive for all devices" -ForegroundColor White
Write-Host "  • Inspired by Meituan & Vuestic Admin" -ForegroundColor White
Write-Host ""
Write-Host "📝 Press Ctrl+C to stop all services" -ForegroundColor Yellow
Write-Host ""

# Monitor jobs
try {
    while ($true) {
        Start-Sleep -Seconds 1
        
        # Check if jobs are still running
        $apiRunning = (Get-Job -Id $apiJob.Id).State -eq 'Running'
        $webRunning = (Get-Job -Id $webJob.Id).State -eq 'Running'
        
        if (-not $apiRunning -or -not $webRunning) {
            Write-Host ""
            Write-Host "⚠️  One or more services stopped" -ForegroundColor Red
            break
        }
    }
} finally {
    Write-Host ""
    Write-Host "Stopping services..." -ForegroundColor Yellow
    Stop-Job -Id $apiJob.Id, $webJob.Id -ErrorAction SilentlyContinue
    Remove-Job -Id $apiJob.Id, $webJob.Id -ErrorAction SilentlyContinue
    Write-Host "✅ All services stopped" -ForegroundColor Green
}

