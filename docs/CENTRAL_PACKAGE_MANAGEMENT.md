# 中央包管理配置说明

## 🎯 什么是中央包管理？

中央包管理（Central Package Management）是 .NET 的一项功能，允许在一个地方集中管理所有项目的 NuGet 包版本。

### 优势

1. **版本统一** - 所有项目使用相同版本的包
2. **简化维护** - 只需在一个地方更新版本
3. **避免冲突** - 减少版本不一致导致的问题
4. **清晰明了** - 项目文件更简洁

---

## 📁 配置文件

### 1. Directory.Packages.props

**位置：** 解决方案根目录

**作用：** 定义所有 NuGet 包的版本

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- 定义包版本 -->
    <PackageVersion Include="Npgsql" Version="9.0.2" />
    <PackageVersion Include="StackExchange.Redis" Version="2.8.16" />
    <PackageVersion Include="Sqlx" Version="0.3.0" />
    <!-- ... 更多包 -->
  </ItemGroup>
</Project>
```

### 2. Directory.Build.props

**位置：** 解决方案根目录

**作用：** 定义所有项目的通用属性

```xml
<Project>
  <PropertyGroup>
    <!-- 通用配置 -->
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>

    <!-- AOT 支持 -->
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>false</InvariantGlobalization>
  </PropertyGroup>
</Project>
```

### 3. 项目文件（简化版）

**之前（没有中央包管理）：**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Npgsql" Version="9.0.2" />
    <PackageReference Include="StackExchange.Redis" Version="2.8.16" />
    <PackageReference Include="Sqlx" Version="0.3.0" />
  </ItemGroup>
</Project>
```

**之后（使用中央包管理）：**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- 属性从 Directory.Build.props 继承 -->

  <ItemGroup>
    <!-- 无需指定版本，从 Directory.Packages.props 读取 -->
    <PackageReference Include="Npgsql" />
    <PackageReference Include="StackExchange.Redis" />
    <PackageReference Include="Sqlx" />
  </ItemGroup>
</Project>
```

**对比：**
- ✅ 更简洁
- ✅ 无需重复配置
- ✅ 版本统一管理

---

## 🔧 如何使用

### 1. 添加新包

只需在 `Directory.Packages.props` 中添加：

```xml
<PackageVersion Include="新包名" Version="版本号" />
```

然后在项目中引用（无需版本号）：

```xml
<PackageReference Include="新包名" />
```

### 2. 更新包版本

只需修改 `Directory.Packages.props` 中的版本号：

```xml
<!-- 之前 -->
<PackageVersion Include="Npgsql" Version="9.0.2" />

<!-- 之后 -->
<PackageVersion Include="Npgsql" Version="9.0.3" />
```

所有项目自动使用新版本！

### 3. 特殊情况：某个项目需要不同版本

可以在项目文件中覆盖：

```xml
<PackageReference Include="Npgsql" VersionOverride="9.0.1" />
```

---

## 📦 CatCat 项目的包管理

### 当前配置的包

| 包名 | 版本 | 用途 |
|------|------|------|
| Npgsql | 9.0.2 | PostgreSQL 驱动 |
| StackExchange.Redis | 2.8.16 | Redis 客户端 |
| NATS.Client.Core | 2.6.5 | NATS 消息队列 |
| Stripe.net | 45.20.0 | Stripe 支付 |
| Sqlx | 0.3.0 | ORM |
| Sqlx.Generator | 0.3.0 | Source Generator |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.0 | JWT 认证 |

### 项目结构

```
CatCat/
├── Directory.Packages.props         # 包版本管理
├── Directory.Build.props            # 通用属性
├── src/
│   ├── CatCat.API/
│   │   └── CatCat.API.csproj       # 简洁的项目文件
│   ├── CatCat.Core/
│   │   └── CatCat.Core.csproj
│   ├── CatCat.Domain/
│   │   └── CatCat.Domain.csproj
│   └── CatCat.Infrastructure/
│       └── CatCat.Infrastructure.csproj
```

---

## ✅ 优势总结

### 1. 代码简化

**之前：**
- 每个项目重复配置 `TargetFramework`
- 每个项目重复配置 `Nullable`
- 每个包引用需要指定版本

**之后：**
- 配置在一个地方
- 项目文件极简
- 版本统一管理

### 2. 维护效率

**场景：更新所有项目的 Npgsql 版本**

**之前：**
1. 打开 CatCat.Infrastructure.csproj，修改版本
2. 打开 CatCat.Core.csproj，修改版本
3. 打开 CatCat.API.csproj，修改版本
4. ... 重复操作

**之后：**
1. 打开 Directory.Packages.props
2. 修改一行：`<PackageVersion Include="Npgsql" Version="9.0.3" />`
3. 完成！

### 3. 避免版本冲突

**问题：**
- ProjectA 使用 Npgsql 9.0.2
- ProjectB 使用 Npgsql 9.0.1
- ProjectC 引用 A 和 B → 版本冲突！

**解决：**
- 中央包管理确保所有项目使用相同版本
- 编译时就能发现版本不一致

---

## 🚀 迁移到中央包管理

### 步骤

1. **创建 Directory.Packages.props**
   ```bash
   touch Directory.Packages.props
   ```

2. **创建 Directory.Build.props**
   ```bash
   touch Directory.Build.props
   ```

3. **提取所有包版本到 Directory.Packages.props**

4. **简化所有项目文件**
   - 移除 `<PropertyGroup>` 中的通用属性
   - 移除 `<PackageReference>` 中的 Version 属性

5. **验证编译**
   ```bash
   dotnet restore
   dotnet build
   ```

---

## 📚 参考资料

- [Central Package Management 官方文档](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- [Directory.Build.props 文档](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)

---

**总结：中央包管理让项目更简洁、更易维护、更不容易出错！** ✨

