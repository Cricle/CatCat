# 多阶段构建 - Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# 复制项目文件
COPY Directory.Packages.props .
COPY Directory.Build.props .
COPY src/CatCat.Domain/CatCat.Domain.csproj src/CatCat.Domain/
COPY src/CatCat.Infrastructure/CatCat.Infrastructure.csproj src/CatCat.Infrastructure/
COPY src/CatCat.Core/CatCat.Core.csproj src/CatCat.Core/
COPY src/CatCat.API/CatCat.API.csproj src/CatCat.API/

# 还原依赖
RUN dotnet restore src/CatCat.API/CatCat.API.csproj

# 复制所有源代码
COPY src/ src/

# 构建和发布
WORKDIR /src/src/CatCat.API
RUN dotnet publish -c Release -o /app/publish

# 多阶段构建 - Frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app

# 复制前端项目
COPY src/CatCat.Web/package*.json ./
RUN npm ci

COPY src/CatCat.Web/ ./
RUN npm run build

# 最终运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app

# 安装必要的运行时依赖
RUN apk add --no-cache icu-libs

# 设置环境变量
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:80 \
    ASPNETCORE_ENVIRONMENT=Production

# 复制后端
COPY --from=backend-build /app/publish .

# 复制前端（静态文件）
COPY --from=frontend-build /app/dist ./wwwroot

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

# 暴露端口
EXPOSE 80

# 启动应用
ENTRYPOINT ["dotnet", "CatCat.API.dll"]

