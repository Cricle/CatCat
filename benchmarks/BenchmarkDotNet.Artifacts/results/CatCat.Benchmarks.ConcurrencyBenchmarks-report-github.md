```

BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6332/22H2/2022Update)
AMD Ryzen 7 5800H with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
  [Host]     : .NET 9.0.8, X64 NativeAOT x86-64-v3
  Job-WQWZZQ : .NET 9.0.8, X64 NativeAOT x86-64-v3

Runtime=.NET 9.0  IterationCount=10  RunStrategy=Throughput  
WarmupCount=3  

```
| Method                          | Mean         | Error      | StdDev     | Gen0   | Gen1   | Allocated |
|-------------------------------- |-------------:|-----------:|-----------:|-------:|-------:|----------:|
| &#39;ConcurrencyLimiter - 单次&#39;       |    101.05 ns |   4.570 ns |   3.023 ns | 0.0257 |      - |     216 B |
| &#39;ConcurrencyLimiter - 批量 (100)&#39; | 10,834.13 ns | 442.712 ns | 263.451 ns | 2.8687 | 0.0763 |   24064 B |
| &#39;IdempotencyStore - 写入&#39;         |           NA |         NA |         NA |     NA |     NA |        NA |
| &#39;IdempotencyStore - 读取&#39;         |     77.37 ns |   3.372 ns |   2.230 ns | 0.0038 |      - |      32 B |
| &#39;IdempotencyStore - 批量写入 (100)&#39; |           NA |         NA |         NA |     NA |     NA |        NA |
| &#39;IdempotencyStore - 批量读取 (100)&#39; | 13,231.26 ns | 540.785 ns | 357.696 ns | 1.0681 |      - |    8984 B |
| &#39;RateLimiter - 获取令牌&#39;            |     47.43 ns |   2.991 ns |   1.979 ns |      - |      - |         - |
| &#39;RateLimiter - 批量获取 (100)&#39;      |  4,874.84 ns | 341.978 ns | 203.506 ns |      - |      - |         - |
| &#39;CircuitBreaker - 成功操作&#39;         |     58.24 ns |   3.459 ns |   2.058 ns | 0.0258 |      - |     216 B |
| &#39;CircuitBreaker - 批量 (100)&#39;     |  6,751.86 ns | 309.535 ns | 204.739 ns | 2.8763 | 0.0763 |   24064 B |

Benchmarks with issues:
  ConcurrencyBenchmarks.'IdempotencyStore - 写入': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  ConcurrencyBenchmarks.'IdempotencyStore - 批量写入 (100)': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
