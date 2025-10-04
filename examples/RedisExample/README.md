# Redis 持久化示例

演示如何使用 CatCat.Transit.Redis 实现 Saga 和幂等性的 Redis 持久化。

## 🚀 运行示例

### 1. 启动 Redis

使用 Docker 启动 Redis：

```bash
docker run -d -p 6379:6379 redis:latest
```

或者使用已有的 Redis 实例。

### 2. 运行示例

```bash
cd examples/RedisExample
dotnet run
```

## 📋 示例内容

### 1. Saga 持久化

```csharp
// 创建 Saga
var saga = new TestSaga
{
    Data = new TestSagaData
    {
        OrderId = Guid.NewGuid(),
        Amount = 99.99m
    }
};

// 保存到 Redis
await repository.SaveAsync(saga);

// 从 Redis 恢复
var recovered = await repository.GetAsync<TestSagaData>(saga.CorrelationId);
```

### 2. 幂等性检查

```csharp
var messageId = Guid.NewGuid().ToString();

// 首次处理
if (!await idempotencyStore.HasBeenProcessedAsync(messageId))
{
    var result = ProcessMessage();
    await idempotencyStore.MarkAsProcessedAsync(messageId, result);
}

// 重复消息
if (await idempotencyStore.HasBeenProcessedAsync(messageId))
{
    var cachedResult = await idempotencyStore.GetCachedResultAsync<string>(messageId);
    // 返回缓存结果
}
```

## 🔍 查看 Redis 数据

使用 Redis CLI 查看存储的数据：

```bash
# 连接到 Redis
redis-cli

# 查看所有示例键
KEYS "example:*"

# 查看 Saga 数据
HGETALL "example:saga:{correlationId}"

# 查看幂等性数据
GET "example:idempotency:{messageId}"

# 查看状态索引
SMEMBERS "example:saga:state:Running"
```

## 📊 预期输出

```
🚀 CatCat.Transit.Redis 示例

📝 注意：此示例需要 Redis 运行在 localhost:6379
   启动 Redis: docker run -d -p 6379:6379 redis:latest

📦 示例 1: Saga Redis 持久化
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📝 创建 Saga: 12345678-...
   ✓ Saga 已保存到 Redis
   ✓ Saga 已更新 (版本 2)

📖 从 Redis 恢复 Saga:
   - CorrelationId: 12345678-...
   - State: Running
   - Version: 2
   - Data.Step: 处理中
   - Data.Amount: $99.99

✅ Saga 已完成并持久化到 Redis
🗑️  Saga 已从 Redis 删除

🔒 示例 2: Redis 幂等性检查
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📧 消息 abcd1234...
   已处理？False
   ✓ 已标记为已处理，结果已缓存

🔄 重复消息 abcd1234...
   已处理？True
   📦 缓存结果: 订单创建成功
   ✅ 幂等性检查通过，返回缓存结果

✨ 所有示例执行完成！

💡 提示：可以使用 Redis CLI 查看存储的数据
   redis-cli KEYS "example:*"
```

## 🔧 配置选项

修改 `Program.cs` 中的配置：

```csharp
services.AddRedisTransit(options =>
{
    // Redis 连接
    options.ConnectionString = "localhost:6379";
    
    // 过期时间
    options.SagaExpiry = TimeSpan.FromDays(7);
    options.IdempotencyExpiry = TimeSpan.FromHours(24);
    
    // 键前缀
    options.SagaKeyPrefix = "example:saga:";
    options.IdempotencyKeyPrefix = "example:idempotency:";
});
```

## 🐛 故障排除

### Redis 连接失败

```
❌ 错误: It was not possible to connect to the redis server(s)
```

**解决方案**：
1. 确保 Redis 正在运行
2. 检查端口是否正确（默认 6379）
3. 检查防火墙设置

### 权限问题

```
❌ 错误: NOAUTH Authentication required
```

**解决方案**：
更新连接字符串包含密码：
```csharp
options.ConnectionString = "localhost:6379,password=your_password";
```

## 📚 相关文档

- [Redis 持久化文档](../../src/CatCat.Transit.Redis/README.md)
- [Saga 使用指南](../../docs/SAGA_AND_STATE_MACHINE.md)

