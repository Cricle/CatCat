# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/CatCat.API/CatCat.API.csproj", "src/CatCat.API/"]
COPY ["src/CatCat.Infrastructure/CatCat.Infrastructure.csproj", "src/CatCat.Infrastructure/"]
RUN dotnet restore "src/CatCat.API/CatCat.API.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build
WORKDIR "/src/src/CatCat.API"
RUN dotnet build "CatCat.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "CatCat.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install diagnostic tools (optional, for debugging)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "CatCat.API.dll"]

