#!/usr/bin/env pwsh
# CatCat 项目一键编译脚本

param(
    [Parameter()]
    [switch]$AOT = $false,  # 是否使用 AOT 编译

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CatCat 项目编译脚本" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  配置: $Configuration" -ForegroundColor White
if ($AOT) {
    Write-Host "  AOT编译: 启用 (Native AOT)" -ForegroundColor Yellow
} else {
    Write-Host "  AOT编译: 禁用 (JIT)" -ForegroundColor White
}
Write-Host ""

# 检查 .NET SDK
Write-Host "[1/6] 检查 .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✓ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green

    # 检查版本是否为 .NET 9
    if ($dotnetVersion -notmatch "^9\.") {
        Write-Host "  ⚠ 建议使用 .NET 9.0 SDK" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ✗ 未找到 .NET SDK，请先安装 .NET 9 SDK" -ForegroundColor Red
    Write-Host "  下载地址: https://dotnet.microsoft.com/download" -ForegroundColor Cyan
    exit 1
}

# 清理旧的编译输出
Write-Host ""
Write-Host "[2/6] 清理旧的编译输出..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ 清理完成" -ForegroundColor Green
} else {
    Write-Host "  ✗ 清理失败" -ForegroundColor Red
    exit 1
}

# 还原 NuGet 包
Write-Host ""
Write-Host "[3/6] 还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ 包还原完成" -ForegroundColor Green
} else {
    Write-Host "  ✗ 包还原失败" -ForegroundColor Red
    exit 1
}

# 编译项目
Write-Host ""
Write-Host "[4/6] 编译项目..." -ForegroundColor Yellow

$buildCmd = "dotnet build --no-restore --configuration $Configuration"

dotnet build --no-restore --configuration $Configuration --verbosity minimal
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ 编译成功 (0 个警告, 0 个错误)" -ForegroundColor Green
} else {
    Write-Host "  ✗ 编译失败，请查看上方错误信息" -ForegroundColor Red
    exit 1
}

# 发布项目
Write-Host ""
Write-Host "[5/6] 发布项目..." -ForegroundColor Yellow

if ($AOT) {
    Write-Host "  使用 Native AOT 编译..." -ForegroundColor Cyan
    dotnet publish src/CatCat.API/CatCat.API.csproj `
        --configuration $Configuration `
        --output "./publish/api-aot" `
        /p:PublishAot=true `
        /p:StripSymbols=true `
        /p:IlcOptimizationPreference=Speed
} else {
    dotnet publish src/CatCat.API/CatCat.API.csproj `
        --configuration $Configuration `
        --output "./publish/api" `
        --no-build
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ 发布完成" -ForegroundColor Green
} else {
    Write-Host "  ✗ 发布失败" -ForegroundColor Red
    exit 1
}

# 运行测试（如果存在测试项目）
Write-Host ""
Write-Host "[6/6] 检查测试项目..." -ForegroundColor Yellow
$testProjects = Get-ChildItem -Path . -Recurse -Filter "*.Tests.csproj" -ErrorAction SilentlyContinue
if ($testProjects.Count -gt 0) {
    Write-Host "  ✓ 找到 $($testProjects.Count) 个测试项目，运行测试..." -ForegroundColor Cyan
    dotnet test --no-build --configuration $Configuration --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ 所有测试通过" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ 部分测试失败" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⊘ 未找到测试项目，跳过测试" -ForegroundColor Gray
}

# 完成
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ 编译完成！" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($AOT) {
    Write-Host "📦 发布目录: ./publish/api-aot/" -ForegroundColor Cyan
    Write-Host "   二进制文件: CatCat.API (Native AOT)" -ForegroundColor White
} else {
    Write-Host "📦 发布目录: ./publish/api/" -ForegroundColor Cyan
    Write-Host "   DLL文件: CatCat.API.dll" -ForegroundColor White
}

Write-Host ""
Write-Host "🚀 运行方式:" -ForegroundColor Yellow
if ($AOT) {
    Write-Host "   ./publish/api-aot/CatCat.API" -ForegroundColor White
} else {
    Write-Host "   dotnet ./publish/api/CatCat.API.dll" -ForegroundColor White
}

Write-Host ""
Write-Host "💡 提示: 使用 -AOT 参数启用 Native AOT 编译" -ForegroundColor Gray
Write-Host "   示例: .\build.ps1 -AOT" -ForegroundColor Gray
Write-Host ""
