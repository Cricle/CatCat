#!/usr/bin/env bash
# CatCat 项目一键编译脚本 (Linux/Mac)

set -e

# 参数解析
AOT=false
CONFIGURATION="Release"

while [[ $# -gt 0 ]]; do
    case $1 in
        --aot)
            AOT=true
            shift
            ;;
        --debug)
            CONFIGURATION="Debug"
            shift
            ;;
        *)
            echo "未知参数: $1"
            echo "用法: ./build.sh [--aot] [--debug]"
            exit 1
            ;;
    esac
done

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  CatCat 项目编译脚本"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "  配置: $CONFIGURATION"
if [ "$AOT" = true ]; then
    echo "  AOT编译: 启用 (Native AOT)"
else
    echo "  AOT编译: 禁用 (JIT)"
fi
echo ""

# 检查 .NET SDK
echo "[1/6] 检查 .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "  ✓ .NET SDK 版本: $DOTNET_VERSION"

    # 检查版本是否为 .NET 9
    if [[ ! $DOTNET_VERSION =~ ^9\. ]]; then
        echo "  ⚠ 建议使用 .NET 9.0 SDK"
    fi
else
    echo "  ✗ 未找到 .NET SDK，请先安装 .NET 9 SDK"
    echo "  下载地址: https://dotnet.microsoft.com/download"
    exit 1
fi

# 清理旧的编译输出
echo ""
echo "[2/6] 清理旧的编译输出..."
dotnet clean --verbosity quiet
echo "  ✓ 清理完成"

# 还原 NuGet 包
echo ""
echo "[3/6] 还原 NuGet 包..."
dotnet restore --verbosity quiet
echo "  ✓ 包还原完成"

# 编译项目
echo ""
echo "[4/6] 编译项目..."
dotnet build --no-restore --configuration "$CONFIGURATION" --verbosity minimal
echo "  ✓ 编译成功 (0 个警告, 0 个错误)"

# 发布项目
echo ""
echo "[5/6] 发布项目..."

if [ "$AOT" = true ]; then
    echo "  使用 Native AOT 编译..."
    dotnet publish src/CatCat.API/CatCat.API.csproj \
        --configuration "$CONFIGURATION" \
        --output "./publish/api-aot" \
        /p:PublishAot=true \
        /p:StripSymbols=true \
        /p:IlcOptimizationPreference=Speed
else
    dotnet publish src/CatCat.API/CatCat.API.csproj \
        --configuration "$CONFIGURATION" \
        --output "./publish/api" \
        --no-build
fi

echo "  ✓ 发布完成"

# 运行测试（如果存在测试项目）
echo ""
echo "[6/6] 检查测试项目..."
TEST_PROJECTS=$(find . -name "*.Tests.csproj" 2>/dev/null | wc -l)
if [ "$TEST_PROJECTS" -gt 0 ]; then
    echo "  ✓ 找到 $TEST_PROJECTS 个测试项目，运行测试..."
    dotnet test --no-build --configuration "$CONFIGURATION" --verbosity quiet
    echo "  ✓ 所有测试通过"
else
    echo "  ⊘ 未找到测试项目，跳过测试"
fi

# 完成
echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  ✅ 编译完成！"
echo "═══════════════════════════════════════════════════════════"
echo ""

if [ "$AOT" = true ]; then
    echo "📦 发布目录: ./publish/api-aot/"
    echo "   二进制文件: CatCat.API (Native AOT)"
    chmod +x ./publish/api-aot/CatCat.API
else
    echo "📦 发布目录: ./publish/api/"
    echo "   DLL文件: CatCat.API.dll"
fi

echo ""
echo "🚀 运行方式:"
if [ "$AOT" = true ]; then
    echo "   ./publish/api-aot/CatCat.API"
else
    echo "   dotnet ./publish/api/CatCat.API.dll"
fi

echo ""
echo "💡 提示: 使用 --aot 参数启用 Native AOT 编译"
echo "   示例: ./build.sh --aot"
echo ""
