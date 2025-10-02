#!/bin/bash
# CatCat 项目一键编译脚本（Linux/Mac）

echo "========================================"
echo " CatCat 项目编译脚本"
echo "========================================"
echo ""

# 检查 .NET SDK
echo "[1/5] 检查 .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "✗ 未找到 .NET SDK，请先安装 .NET 9 SDK"
    exit 1
fi
dotnet_version=$(dotnet --version)
echo "✓ .NET SDK 版本: $dotnet_version"

# 清理旧的编译输出
echo ""
echo "[2/5] 清理旧的编译输出..."
dotnet clean --verbosity quiet
if [ $? -eq 0 ]; then
    echo "✓ 清理完成"
else
    echo "✗ 清理失败"
    exit 1
fi

# 还原 NuGet 包
echo ""
echo "[3/5] 还原 NuGet 包..."
dotnet restore --verbosity quiet
if [ $? -eq 0 ]; then
    echo "✓ 包还原完成"
else
    echo "✗ 包还原失败"
    exit 1
fi

# 编译项目
echo ""
echo "[4/5] 编译项目..."
dotnet build --no-restore --configuration Release
if [ $? -eq 0 ]; then
    echo "✓ 编译成功"
else
    echo "✗ 编译失败，请查看上方错误信息"
    exit 1
fi

# 运行测试（如果存在测试项目）
echo ""
echo "[5/5] 检查测试项目..."
test_projects=$(find . -name "*.Tests.csproj" 2>/dev/null)
if [ -n "$test_projects" ]; then
    echo "✓ 找到测试项目，运行测试..."
    dotnet test --no-build --configuration Release --verbosity quiet
    if [ $? -eq 0 ]; then
        echo "✓ 所有测试通过"
    else
        echo "⚠ 部分测试失败"
    fi
else
    echo "⊘ 未找到测试项目"
fi

# 完成
echo ""
echo "========================================"
echo " 编译完成！"
echo "========================================"
echo ""
echo "输出目录: src/CatCat.API/bin/Release/net9.0/"
echo ""

