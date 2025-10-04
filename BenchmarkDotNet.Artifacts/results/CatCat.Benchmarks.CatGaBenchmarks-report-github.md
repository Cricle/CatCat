```

BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6332/22H2/2022Update)
AMD Ryzen 7 5800H with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
  [Host]     : .NET 9.0.8, X64 NativeAOT x86-64-v3
  Job-WQWZZQ : .NET 9.0.8, X64 NativeAOT x86-64-v3
  ShortRun   : .NET 9.0.8, X64 NativeAOT x86-64-v3

WarmupCount=3  

```
| Method            | Job        | Runtime       | IterationCount | LaunchCount | RunStrategy | Mean          | Error      | StdDev     | Gen0     | Gen1    | Allocated  |
|------------------ |----------- |-------------- |--------------- |------------ |------------ |--------------:|-----------:|-----------:|---------:|--------:|-----------:|
| 单次简单事务            | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |      1.105 μs |  0.0706 μs |  0.0369 μs |   0.1297 |       - |    1.07 KB |
| 单次复杂事务            | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  | 15,812.226 μs | 69.8420 μs | 46.1962 μs |        - |       - |    1.84 KB |
| &#39;批量简单事务 (100)&#39;    | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |    107.207 μs |  7.1420 μs |  4.7240 μs |  12.4512 |  1.9531 |  102.15 KB |
| &#39;高并发事务 (1000)&#39;    | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |  1,111.087 μs | 44.7346 μs | 29.5892 μs | 125.0000 | 93.7500 | 1023.24 KB |
| &#39;幂等性测试 (100 次重复)&#39; | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  |     20.234 μs |  0.8196 μs |  0.5421 μs |   1.9531 |  0.0610 |    16.2 KB |
| 单次简单事务            | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |            NA |         NA |         NA |       NA |      NA |         NA |
| 单次复杂事务            | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |            NA |         NA |         NA |       NA |      NA |         NA |
| &#39;批量简单事务 (100)&#39;    | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |            NA |         NA |         NA |       NA |      NA |         NA |
| &#39;高并发事务 (1000)&#39;    | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |            NA |         NA |         NA |       NA |      NA |         NA |
| &#39;幂等性测试 (100 次重复)&#39; | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |            NA |         NA |         NA |       NA |      NA |         NA |

Benchmarks with issues:
  CatGaBenchmarks.单次简单事务: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  CatGaBenchmarks.单次复杂事务: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  CatGaBenchmarks.'批量简单事务 (100)': ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  CatGaBenchmarks.'高并发事务 (1000)': ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  CatGaBenchmarks.'幂等性测试 (100 次重复)': ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
