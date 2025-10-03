#!/usr/bin/env pwsh
# CatCat Web Development Server
# Quick start script for frontend development

Write-Host ""
Write-Host "🐱 CatCat Web Development Server" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Check Node.js
Write-Host "Checking Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found! Please install Node.js 18+" -ForegroundColor Red
    exit 1
}

# Navigate to web directory
Set-Location src/CatCat.Web

# Check if node_modules exists
if (!(Test-Path "node_modules")) {
    Write-Host ""
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    npm install
    Write-Host "✅ Dependencies installed!" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎨 Starting development server..." -ForegroundColor Yellow
Write-Host ""
Write-Host "🌐 Local:   http://localhost:5173" -ForegroundColor Cyan
Write-Host "🌐 Network: Use --host for network access" -ForegroundColor Cyan
Write-Host ""
Write-Host "📚 New UI Features:" -ForegroundColor Magenta
Write-Host "  • Gradient Hero Banner" -ForegroundColor White
Write-Host "  • Floating Quick Actions" -ForegroundColor White
Write-Host "  • Large Icon Service Cards" -ForegroundColor White
Write-Host "  • Beautiful Color Gradients" -ForegroundColor White
Write-Host "  • Fully Responsive Design" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Design Inspired by:" -ForegroundColor Magenta
Write-Host "  • Meituan App" -ForegroundColor White
Write-Host "  • Vuestic Admin (github.com/epicmaxco/vuestic-admin)" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
Write-Host ""

# Start development server
npm run dev

