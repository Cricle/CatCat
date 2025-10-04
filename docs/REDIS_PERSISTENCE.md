# Redis 持久化完全指南

## 🎯 概述

`CatCat.Transit.Redis` 提供生产级的 Saga 和幂等性 Redis 持久化支持。

## ✨ 核心特性

### 1. Saga 持久化

- ✅ **完整状态管理**：支持所有 6 种 Saga 状态
- ✅ **乐观锁**：基于版本的并发控制
- ✅ **状态索引**：快速按状态查询
- ✅ **自动过期**：可配置的数据保留期
- ✅ **类型安全**：泛型支持

### 2. 幂等性存储

- ✅ **高性能**：基于 Redis 字符串的快速检查
- ✅ **结果缓存**：支持任意类型的结果缓存
- ✅ **自动过期**：基于业务需求的灵活配置
- ✅ **类型匹配**：验证缓存结果类型

### 3. 性能优化

- ✅ **连接池**：复用 Redis 连接
- ✅ **管道/事务**：批量操作优化
- ✅ **最小网络往返**：优化的数据结构

## 🚀 快速开始

### 1. 安装

```bash
dotnet add package CatCat.Transit.Redis
```

### 2. 基础配置

```csharp
services.AddTransit();

services.AddRedisTransit(options =>
{
    options.ConnectionString = "localhost:6379";
    options.SagaExpiry = TimeSpan.FromDays(7);
    options.IdempotencyExpiry = TimeSpan.FromHours(24);
});
```

### 3. 使用

Saga 和幂等性会自动使用 Redis 持久化，无需修改业务代码！

## 📊 数据结构详解

### Saga 数据结构

```
类型: Hash
键: saga:{correlationId}

字段:
├── correlationId: string (Guid)
├── state: int (SagaState 枚举)
├── version: int (乐观锁版本)
├── createdAt: long (Ticks)
├── updatedAt: long (Ticks)
├── type: string (类型全名)
└── data: string (JSON 序列化的 Saga 数据)

示例:
HGETALL saga:12345678-abcd-...
1) "correlationId"
2) "12345678-abcd-1234-5678-1234567890ab"
3) "state"
4) "2"  # Running
5) "version"
6) "5"
7) "data"
8) "{\"orderId\":\"...\",\"amount\":99.99}"
```

### Saga 状态索引

```
类型: Set
键: saga:state:{state}

成员: correlationId 列表

示例:
SMEMBERS saga:state:Running
1) "12345678-abcd-..."
2) "87654321-dcba-..."
```

### 幂等性数据结构

```
类型: String (JSON)
键: idempotency:{messageId}

内容:
{
  "messageId": "msg-12345",
  "processedAt": "2024-01-01T12:00:00Z",
  "resultType": "System.Guid, ...",
  "resultJson": "\"abc123-...\""
}

示例:
GET idempotency:msg-12345
```

## ⚙️ 配置详解

### 完整配置选项

```csharp
services.AddRedisTransit(options =>
{
    // === 连接配置 ===
    options.ConnectionString = "localhost:6379,password=secret";
    options.ConnectTimeout = 5000;  // 5 秒
    options.SyncTimeout = 5000;     // 5 秒
    options.ConnectRetry = 3;
    options.KeepAlive = 60;         // 60 秒
    
    // === SSL 配置 ===
    options.UseSsl = true;
    options.SslHost = "redis.example.com";
    options.AllowAdmin = false;
    
    // === Saga 配置 ===
    options.SagaKeyPrefix = "myapp:saga:";
    options.SagaExpiry = TimeSpan.FromDays(30);  // 保留 30 天
    
    // === 幂等性配置 ===
    options.IdempotencyKeyPrefix = "myapp:idempotency:";
    options.IdempotencyExpiry = TimeSpan.FromHours(48);  // 保留 48 小时
});
```

### 环境特定配置

```csharp
// 开发环境
if (builder.Environment.IsDevelopment())
{
    services.AddRedisTransit(options =>
    {
        options.ConnectionString = "localhost:6379";
        options.SagaExpiry = TimeSpan.FromHours(1);  // 短期保留
    });
}
// 生产环境
else
{
    services.AddRedisTransit(options =>
    {
        options.ConnectionString = builder.Configuration["Redis:ConnectionString"];
        options.SagaExpiry = TimeSpan.FromDays(30);
        options.IdempotencyExpiry = TimeSpan.FromHours(24);
        options.UseSsl = true;
        options.ConnectRetry = 5;
    });
}
```

## 🔧 高级用法

### 1. 单独使用组件

```csharp
// 只使用 Saga 仓储
services.AddRedisSagaRepository(options =>
{
    options.ConnectionString = "localhost:6379";
});

// 或只使用幂等性存储
services.AddRedisIdempotencyStore(options =>
{
    options.ConnectionString = "localhost:6379";
});
```

### 2. 自定义 Redis 连接

```csharp
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse("localhost:6379");
    config.DefaultDatabase = 1;  // 使用数据库 1
    config.AbortOnConnectFail = false;
    config.ReconnectRetryPolicy = new LinearRetry(5000);
    return ConnectionMultiplexer.Connect(config);
});

services.AddSingleton<ISagaRepository, RedisSagaRepository>();
services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
```

### 3. Redis 集群配置

```csharp
services.AddRedisTransit(options =>
{
    options.ConnectionString = "redis-node1:6379,redis-node2:6379,redis-node3:6379";
    options.ConnectRetry = 5;
    options.KeepAlive = 30;
});
```

### 4. Redis Sentinel 配置

```csharp
var config = ConfigurationOptions.Parse("sentinel1:26379,sentinel2:26379");
config.ServiceName = "mymaster";
config.TieBreaker = "";
config.CommandMap = CommandMap.Sentinel;

services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(config));
```

## 📈 性能优化

### 1. 过期时间策略

```csharp
// Saga 过期策略：基于审计需求
options.SagaExpiry = TimeSpan.FromDays(30);  // 审计：30 天
// 或
options.SagaExpiry = TimeSpan.FromDays(7);   // 标准：7 天

// 幂等性过期策略：基于重试窗口
options.IdempotencyExpiry = TimeSpan.FromHours(24);  // 24 小时重试窗口
// 或
options.IdempotencyExpiry = TimeSpan.FromHours(1);   // 1 小时（快速失败）
```

### 2. 键前缀策略

```csharp
// 多环境隔离
options.SagaKeyPrefix = $"{environment}:saga:";
options.IdempotencyKeyPrefix = $"{environment}:idempotency:";

// 多租户隔离
options.SagaKeyPrefix = $"tenant:{tenantId}:saga:";
```

### 3. 连接池优化

```csharp
var config = ConfigurationOptions.Parse(connectionString);
config.KeepAlive = 30;        // 30 秒心跳
config.ConnectRetry = 5;      // 5 次重试
config.ConnectTimeout = 3000; // 3 秒超时
config.SyncTimeout = 3000;    // 3 秒操作超时
```

## 🔍 监控和诊断

### 1. 健康检查

```csharp
services.AddHealthChecks()
    .AddRedis(
        redisConnectionString: "localhost:6379",
        name: "redis",
        timeout: TimeSpan.FromSeconds(3));
```

### 2. Redis 命令监控

```csharp
// 使用 Redis CLI 监控
MONITOR  # 实时查看所有命令

// 查看键数量
DBSIZE

// 查看内存使用
INFO memory

// 查看特定键
KEYS saga:*
KEYS idempotency:*
```

### 3. 性能指标

```csharp
var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();

// 获取服务器信息
var server = connection.GetServer("localhost:6379");
var info = await server.InfoAsync();

// 统计信息
var stats = server.GetCounters();
Console.WriteLine($"Total operations: {stats.TotalOutstanding}");
```

## 🐛 故障排除

### 1. 连接失败

**问题**：
```
It was not possible to connect to the redis server(s)
```

**解决方案**：
```csharp
// 1. 检查连接字符串
options.ConnectionString = "localhost:6379";  // 确保端口正确

// 2. 增加超时时间
options.ConnectTimeout = 10000;  // 10 秒

// 3. 增加重试次数
options.ConnectRetry = 10;

// 4. 检查防火墙和网络
```

### 2. 认证失败

**问题**：
```
NOAUTH Authentication required
```

**解决方案**：
```csharp
options.ConnectionString = "localhost:6379,password=your_password";
```

### 3. SSL/TLS 错误

**问题**：
```
SSL connection error
```

**解决方案**：
```csharp
options.UseSsl = true;
options.SslHost = "redis.example.com";  // 指定 SSL 主机名
```

### 4. 性能问题

**问题**：Redis 操作缓慢

**解决方案**：
```csharp
// 1. 检查网络延迟
PING  // 在 Redis CLI 中

// 2. 优化超时设置
options.SyncTimeout = 2000;  // 降低到 2 秒

// 3. 使用管道/事务
// (已自动使用)

// 4. 检查 Redis 内存
INFO memory
```

### 5. 内存溢出

**问题**：Redis 内存不足

**解决方案**：
```csharp
// 1. 调整过期时间
options.SagaExpiry = TimeSpan.FromDays(7);  // 从 30 天减少到 7 天
options.IdempotencyExpiry = TimeSpan.FromHours(12);  // 从 24 小时减少到 12 小时

// 2. 定期清理
// 使用 Redis 的 SCAN 和 DELETE

// 3. 配置 Redis 最大内存和驱逐策略
// 在 redis.conf 中:
# maxmemory 2gb
# maxmemory-policy allkeys-lru
```

## 📚 最佳实践

### 1. 连接管理

```csharp
// ✅ 正确：复用连接
services.AddSingleton<IConnectionMultiplexer>(...);

// ❌ 错误：每次创建新连接
// ConnectionMultiplexer.Connect(...) 在业务代码中
```

### 2. 过期时间

```csharp
// ✅ 正确：根据业务需求设置
options.SagaExpiry = TimeSpan.FromDays(30);  // 审计需求
options.IdempotencyExpiry = TimeSpan.FromHours(24);  // 重试窗口

// ❌ 错误：过长的保留时间
options.SagaExpiry = TimeSpan.FromDays(365);  // 太长，浪费内存
```

### 3. 键命名

```csharp
// ✅ 正确：使用前缀隔离
options.SagaKeyPrefix = "myapp:prod:saga:";
options.IdempotencyKeyPrefix = "myapp:prod:idempotency:";

// ❌ 错误：不使用前缀
options.SagaKeyPrefix = "";  // 可能与其他应用冲突
```

### 4. 错误处理

```csharp
// ✅ 正确：配置重试和超时
options.ConnectRetry = 3;
options.ConnectTimeout = 5000;

try
{
    await repository.SaveAsync(saga);
}
catch (RedisConnectionException ex)
{
    // 记录错误并使用降级策略
    logger.LogError(ex, "Redis connection failed");
    // 可能回退到内存存储或重试队列
}
```

### 5. 监控

```csharp
// ✅ 添加健康检查
services.AddHealthChecks().AddRedis(...);

// ✅ 记录关键操作
logger.LogInformation("Saved Saga {SagaId} to Redis", saga.CorrelationId);

// ✅ 定期检查 Redis 健康状态
var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
if (!connection.IsConnected)
{
    logger.LogWarning("Redis connection lost, attempting to reconnect...");
}
```

## 📖 示例代码

完整示例请参考：`examples/RedisExample/`

## 🔗 相关文档

- [Saga 使用指南](SAGA_AND_STATE_MACHINE.md)
- [性能优化](FINAL_FEATURES.md)
- [Redis 官方文档](https://redis.io/documentation)
- [StackExchange.Redis 文档](https://stackexchange.github.io/StackExchange.Redis/)

