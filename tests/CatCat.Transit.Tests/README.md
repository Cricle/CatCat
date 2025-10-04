# CatCat.Transit 单元测试

全面的单元测试套件，覆盖 CatCat.Transit 的所有功能。

## 测试覆盖

### 核心功能
- ✅ **TransitMediator** - CQRS Mediator 核心功能
- ✅ **TransitResult** - 结果类型和错误处理
- ✅ **Messages** - Command/Query/Event 消息类型

### Pipeline Behaviors
- ✅ **LoggingBehavior** - 日志记录行为测试
- ✅ **IdempotencyBehavior** - 幂等性行为测试（使用 Moq）
- ✅ **RetryBehavior** - 重试行为测试
- ✅ **ValidationBehavior** - 验证行为测试（使用 Moq）
- ✅ **TracingBehavior** - 追踪行为测试

### 弹性机制
- ✅ **ConcurrencyLimiter** - 并发控制测试
- ✅ **CircuitBreaker** - 熔断器测试
- ✅ **TokenBucketRateLimiter** - 限流测试

### 存储与队列
- ✅ **ShardedIdempotencyStore** - 分片幂等存储测试
- ✅ **InMemoryDeadLetterQueue** - 死信队列测试（使用 Moq）

### 配置
- ✅ **TransitOptions** - 配置选项测试

### 集成测试
- ✅ **End-to-End** - 完整流程集成测试
- ✅ **并发场景** - 并发请求测试
- ✅ **幂等性验证** - 端到端幂等性测试

## 测试技术栈

- **xUnit** - 测试框架
- **FluentAssertions** - 断言库
- **Moq** - Mock 对象库
- **.NET 9** - 目标框架

## 运行测试

### 运行所有测试
```bash
dotnet test tests/CatCat.Transit.Tests/CatCat.Transit.Tests.csproj
```

### 运行特定测试类
```bash
dotnet test --filter "FullyQualifiedName~TransitMediatorTests"
```

### 运行特定测试
```bash
dotnet test --filter "FullyQualifiedName~SendAsync_WithResponse_ReturnsSuccessResult"
```

### 生成代码覆盖率报告
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 测试组织

```
tests/CatCat.Transit.Tests/
├── TestHelpers/
│   ├── TestMessages.cs       # 测试消息定义
│   └── TestHandlers.cs       # 测试处理器（简单实现）
├── Results/
│   └── TransitResultTests.cs
├── Pipeline/
│   ├── LoggingBehaviorTests.cs
│   ├── IdempotencyBehaviorTests.cs    # 使用 Moq
│   ├── RetryBehaviorTests.cs
│   └── ValidationBehaviorTests.cs     # 使用 Moq
├── Concurrency/
│   └── ConcurrencyLimiterTests.cs
├── Resilience/
│   └── CircuitBreakerTests.cs
├── RateLimiting/
│   └── TokenBucketRateLimiterTests.cs
├── Idempotency/
│   └── IdempotencyTests.cs
├── DeadLetter/
│   └── DeadLetterQueueTests.cs        # 使用 Moq
├── Configuration/
│   └── TransitOptionsTests.cs
├── Integration/
│   └── EndToEndTests.cs
└── TransitMediatorTests.cs
```

## Mock 使用示例

### 使用 Moq 模拟 ILogger
```csharp
var loggerMock = new Mock<ILogger<LoggingBehavior<TestCommand, string>>>();
var behavior = new LoggingBehavior<TestCommand, string>(loggerMock.Object);

// 验证日志调用
loggerMock.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

### 使用 Moq 模拟 IIdempotencyStore
```csharp
var storeMock = new Mock<IIdempotencyStore>();
storeMock.Setup(x => x.IsProcessedAsync(It.IsAny<string>()))
    .ReturnsAsync(false);

storeMock.Verify(x => x.MarkAsProcessedAsync(
    It.IsAny<string>(),
    It.IsAny<string>()),
    Times.Once);
```

### 使用 Moq 模拟 IValidator
```csharp
var validatorMock = new Mock<IValidator<TestCommand>>();
validatorMock.Setup(x => x.ValidateAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((true, Array.Empty<string>()));
```

## 测试覆盖率目标

- ✅ 代码覆盖率 > 80%
- ✅ 所有公共 API 都有测试
- ✅ 边界条件测试
- ✅ 异常场景测试
- ✅ 并发安全测试

## 测试原则

1. **AAA 模式** - Arrange, Act, Assert
2. **单一职责** - 每个测试只验证一个行为
3. **独立性** - 测试之间不相互依赖
4. **可重复性** - 测试结果稳定可靠
5. **Fast** - 快速执行
6. **使用 Moq** - 简化依赖项模拟

## 持续改进

- 🔄 增加更多边界条件测试
- 🔄 增加性能基准测试
- 🔄 增加压力测试
- 🔄 提高代码覆盖率到 90%+

