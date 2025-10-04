# CatCat.Transit 性能基准测试指南

## 📊 快速开始

### Windows (PowerShell)

```powershell
# 运行所有测试
./benchmarks/run-benchmarks.ps1

# 快速测试（较少迭代）
./benchmarks/run-benchmarks.ps1 -Quick

# 只测试 CQRS
./benchmarks/run-benchmarks.ps1 -Filter "*CqrsBenchmarks*"

# 只测试 CatGa
./benchmarks/run-benchmarks.ps1 -Filter "*CatGaBenchmarks*"

# 启用内存诊断
./benchmarks/run-benchmarks.ps1 -Memory

# 导出报告
./benchmarks/run-benchmarks.ps1 -Export
```

### Linux/macOS (Bash)

```bash
# 运行所有测试
chmod +x benchmarks/run-benchmarks.sh
./benchmarks/run-benchmarks.sh

# 快速测试
./benchmarks/run-benchmarks.sh --quick

# 只测试 CQRS
./benchmarks/run-benchmarks.sh --filter "*CqrsBenchmarks*"

# 启用内存诊断 + 导出报告
./benchmarks/run-benchmarks.sh --memory --export
```

### 直接使用 dotnet

```bash
# 运行所有测试
dotnet run -c Release --project benchmarks/CatCat.Benchmarks

# 运行特定测试
dotnet run -c Release --project benchmarks/CatCat.Benchmarks --filter "*Single*"

# 生成报告
dotnet run -c Release --project benchmarks/CatCat.Benchmarks --exporters html json
```

## 🎯 测试分类

### 1. CQRS 性能测试

测试命令、查询、事件的性能特征。

```powershell
./benchmarks/run-benchmarks.ps1 -Filter "*CqrsBenchmarks*"
```

**测试场景**:
- `SendCommand_Single` - 单次命令处理延迟
- `SendQuery_Single` - 单次查询处理延迟
- `PublishEvent_Single` - 单次事件发布延迟
- `SendCommand_Batch100` - 100 个命令的吞吐量
- `SendCommand_HighConcurrency1000` - 1000 个命令的高并发性能

### 2. CatGa 性能测试

测试分布式事务的性能特征。

```powershell
./benchmarks/run-benchmarks.ps1 -Filter "*CatGaBenchmarks*"
```

**测试场景**:
- `ExecuteTransaction_Simple` - 简单事务延迟
- `ExecuteTransaction_Complex` - 复杂事务延迟（带补偿）
- `ExecuteTransaction_Batch100` - 批量事务吞吐量
- `ExecuteTransaction_HighConcurrency1000` - 高并发事务性能
- `ExecuteTransaction_Idempotency100` - 幂等性验证

### 3. 并发控制性能测试

测试并发控制组件的性能。

```powershell
./benchmarks/run-benchmarks.ps1 -Filter "*ConcurrencyBenchmarks*"
```

**测试场景**:
- `ConcurrencyLimiter_*` - 并发限制器性能
- `IdempotencyStore_*` - 幂等性存储性能
- `RateLimiter_*` - 限流器性能
- `CircuitBreaker_*` - 熔断器性能

## 📈 性能目标

### CQRS 目标

| 指标 | 目标 | 说明 |
|------|------|------|
| **单次延迟** | < 0.1ms | P99 百分位 |
| **批量吞吐** | > 50K ops/s | 100 个并发操作 |
| **高并发吞吐** | > 30K ops/s | 1000 个并发操作 |
| **内存分配** | < 1KB/op | 单次操作 |

### CatGa 目标

| 指标 | 目标 | 说明 |
|------|------|------|
| **简单事务延迟** | < 0.2ms | P99 百分位 |
| **复杂事务延迟** | < 1ms | P99 百分位 |
| **批量吞吐** | > 20K txn/s | 100 个并发事务 |
| **幂等命中率** | 100% | 重复请求 |

### 并发控制目标

| 组件 | 吞吐量目标 | 延迟目标 |
|------|-----------|---------|
| **ConcurrencyLimiter** | > 100K ops/s | < 10μs |
| **IdempotencyStore (写)** | > 80K ops/s | < 15μs |
| **IdempotencyStore (读)** | > 200K ops/s | < 5μs |
| **RateLimiter** | > 500K ops/s | < 2μs |
| **CircuitBreaker** | > 150K ops/s | < 7μs |

## 📊 报告解读

### 输出示例

```
| Method                             | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------- |------------:|----------:|----------:|-------:|----------:|
| SendCommand_Single                 |    45.32 us |  0.891 us |  0.833 us | 0.0610 |     528 B |
| IdempotencyStore_Write             |     6.23 us |  0.122 us |  0.114 us | 0.0229 |     192 B |
| RateLimiter_TryAcquire             |     0.85 ns |  0.017 ns |  0.016 ns | -      |       - B |
```

### 关键指标说明

- **Mean**: 平均执行时间
- **Error**: 标准误差
- **StdDev**: 标准差（数值越小越稳定）
- **Gen0/Gen1/Gen2**: GC 收集次数
- **Allocated**: 每次操作分配的内存

### 性能等级

#### 延迟评估

- ✅ **优秀**: < 1ms
- ⚠️ **良好**: 1-10ms
- ❌ **需优化**: > 10ms

#### 内存评估

- ✅ **优秀**: < 1KB
- ⚠️ **良好**: 1-10KB
- ❌ **需优化**: > 10KB

#### GC 评估

- ✅ **优秀**: Gen0 < 0.1
- ⚠️ **良好**: Gen0 < 1.0
- ❌ **需优化**: Gen0 > 1.0 或 Gen1/Gen2 > 0

## 🔧 高级用法

### 比较不同配置

```powershell
# 测试不同分片数的性能
# 修改 ConcurrencyBenchmarks.cs 中的 shardCount
# 然后运行测试并比较结果
```

### 导出详细报告

```powershell
# 导出 HTML 和 JSON
./benchmarks/run-benchmarks.ps1 -Export

# 查看报告
Start-Process "benchmarks/CatCat.Benchmarks/BenchmarkDotNet.Artifacts/results/index.html"
```

### 性能分析

```bash
# 使用 dotMemory 进行内存分析
dotnet run -c Release --project benchmarks/CatCat.Benchmarks -- --profiler NativeMemory

# 使用 ETW 进行性能分析（Windows）
dotnet run -c Release --project benchmarks/CatCat.Benchmarks -- --profiler ETW
```

## 📝 注意事项

### ⚠️ 必须遵守

1. **必须在 Release 模式运行**
   ```bash
   dotnet run -c Release  # ✅ 正确
   dotnet run             # ❌ 错误（Debug 模式）
   ```

2. **关闭调试器**
   - Visual Studio: 使用 Ctrl+F5（不调试）
   - VS Code: 直接在终端运行

3. **关闭其他应用**
   - 减少 CPU 和内存竞争
   - 避免系统噪声

### 💡 最佳实践

1. **预热系统**
   ```bash
   # 先运行一次快速测试预热
   ./benchmarks/run-benchmarks.ps1 -Quick
   
   # 再运行完整测试
   ./benchmarks/run-benchmarks.ps1
   ```

2. **多次运行**
   - 至少运行 3 次
   - 取平均值或中位数

3. **记录环境信息**
   - CPU 型号和频率
   - 内存大小
   - 操作系统版本
   - .NET 版本

## 🎉 示例结果

### CatCat.Transit 典型性能

**测试环境**: AMD Ryzen 9 5900X, 32GB RAM, Windows 11, .NET 9.0

```
| Method                             | Mean        | Allocated |
|----------------------------------- |------------:|----------:|
| SendCommand_Single                 |    42.1 us  |     512 B |
| SendQuery_Single                   |    40.3 us  |     512 B |
| PublishEvent_Single                |    38.7 us  |     512 B |
| ExecuteTransaction_Simple          |    48.5 us  |     624 B |
| ConcurrencyLimiter_Single          |     7.2 us  |     128 B |
| IdempotencyStore_Write             |     5.8 us  |     192 B |
| IdempotencyStore_Read              |     4.1 us  |      96 B |
| RateLimiter_TryAcquire             |     0.7 ns  |       - B |
| CircuitBreaker_Success             |     6.9 us  |     128 B |
```

**吞吐量计算**:
- CQRS: ~23,800 ops/s (单次)
- CatGa: ~20,600 txn/s (单次)
- IdempotencyStore: ~172,000 ops/s (读取)
- RateLimiter: ~1,400,000,000 ops/s (14 亿！)

## 🚀 性能优化建议

基于基准测试结果，可以进行以下优化：

### 1. 减少内存分配

```csharp
// ❌ 每次分配新对象
var result = new TestResponse { ... };

// ✅ 使用对象池
var result = _pool.Rent();
```

### 2. 使用 ValueTask

```csharp
// ❌ 返回 Task (分配堆内存)
Task<int> GetValueAsync();

// ✅ 返回 ValueTask (可能栈分配)
ValueTask<int> GetValueAsync();
```

### 3. 批量处理

```csharp
// ❌ 逐个处理
foreach (var item in items)
    await ProcessAsync(item);

// ✅ 批量处理
await ProcessBatchAsync(items);
```

### 4. 调整分片数

```csharp
// 低并发
new ShardedIdempotencyStore(shardCount: 16);

// 高并发
new ShardedIdempotencyStore(shardCount: 128);
```

## 📚 相关文档

- [性能优化文档](./PERFORMANCE_OPTIMIZATION.md)
- [优化总结文档](./OPTIMIZATION_SUMMARY.md)
- [项目结构文档](./PROJECT_STRUCTURE.md)

---

**CatCat.Transit** - 高性能、高并发、AOT 友好的 CQRS 框架 🚀

