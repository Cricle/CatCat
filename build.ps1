#!/usr/bin/env pwsh
# CatCat 项目一键编译脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " CatCat 项目编译脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查 .NET SDK
Write-Host "[1/5] 检查 .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ 未找到 .NET SDK，请先安装 .NET 9 SDK" -ForegroundColor Red
    exit 1
}

# 清理旧的编译输出
Write-Host ""
Write-Host "[2/5] 清理旧的编译输出..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ 清理完成" -ForegroundColor Green
} else {
    Write-Host "✗ 清理失败" -ForegroundColor Red
    exit 1
}

# 还原 NuGet 包
Write-Host ""
Write-Host "[3/5] 还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ 包还原完成" -ForegroundColor Green
} else {
    Write-Host "✗ 包还原失败" -ForegroundColor Red
    exit 1
}

# 编译项目
Write-Host ""
Write-Host "[4/5] 编译项目..." -ForegroundColor Yellow
dotnet build --no-restore --configuration Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ 编译成功" -ForegroundColor Green
} else {
    Write-Host "✗ 编译失败，请查看上方错误信息" -ForegroundColor Red
    exit 1
}

# 运行测试（如果存在测试项目）
Write-Host ""
Write-Host "[5/5] 检查测试项目..." -ForegroundColor Yellow
$testProjects = Get-ChildItem -Path . -Recurse -Filter "*.Tests.csproj"
if ($testProjects.Count -gt 0) {
    Write-Host "✓ 找到测试项目，运行测试..." -ForegroundColor Yellow
    dotnet test --no-build --configuration Release --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ 所有测试通过" -ForegroundColor Green
    } else {
        Write-Host "⚠ 部分测试失败" -ForegroundColor Yellow
    }
} else {
    Write-Host "⊘ 未找到测试项目" -ForegroundColor Gray
}

# 完成
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " 编译完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "输出目录: src/CatCat.API/bin/Release/net9.0/" -ForegroundColor Cyan
Write-Host ""

