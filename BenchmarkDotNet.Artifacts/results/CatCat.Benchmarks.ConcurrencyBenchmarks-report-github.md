```

BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6332/22H2/2022Update)
AMD Ryzen 7 5800H with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
  [Host]     : .NET 9.0.8, X64 NativeAOT x86-64-v3
  Job-WQWZZQ : .NET 9.0.8, X64 NativeAOT x86-64-v3
  ShortRun   : .NET 9.0.8, X64 NativeAOT x86-64-v3

WarmupCount=3  

```
| Method                          | Job        | Runtime       | IterationCount | LaunchCount | RunStrategy | Mean         | Error        | StdDev     | Gen0   | Allocated |
|-------------------------------- |----------- |-------------- |--------------- |------------ |------------ |-------------:|-------------:|-----------:|-------:|----------:|
| &#39;IdempotencyStore - 写入&#39;         | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |           NA |           NA |         NA |     NA |        NA |
| &#39;IdempotencyStore - 读取&#39;         | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |     77.68 ns |     3.748 ns |   2.479 ns | 0.0038 |      32 B |
| &#39;IdempotencyStore - 批量写入 (100)&#39; | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |           NA |           NA |         NA |     NA |        NA |
| &#39;IdempotencyStore - 批量读取 (100)&#39; | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  | 13,131.05 ns |   614.561 ns | 406.494 ns | 1.0681 |    8984 B |
| &#39;IdempotencyStore - 写入&#39;         | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |           NA |           NA |         NA |     NA |        NA |
| &#39;IdempotencyStore - 读取&#39;         | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |     97.34 ns |    37.220 ns |   2.040 ns | 0.0038 |      32 B |
| &#39;IdempotencyStore - 批量写入 (100)&#39; | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |           NA |           NA |         NA |     NA |        NA |
| &#39;IdempotencyStore - 批量读取 (100)&#39; | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     | 13,764.75 ns | 4,022.989 ns | 220.514 ns | 1.0681 |    8984 B |

Benchmarks with issues:
  ConcurrencyBenchmarks.'IdempotencyStore - 写入': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  ConcurrencyBenchmarks.'IdempotencyStore - 批量写入 (100)': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  ConcurrencyBenchmarks.'IdempotencyStore - 写入': ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  ConcurrencyBenchmarks.'IdempotencyStore - 批量写入 (100)': ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
