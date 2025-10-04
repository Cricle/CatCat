```

BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6332/22H2/2022Update)
AMD Ryzen 7 5800H with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
  [Host]     : .NET 9.0.8, X64 NativeAOT x86-64-v3
  Job-WQWZZQ : .NET 9.0.8, X64 NativeAOT x86-64-v3
  ShortRun   : .NET 9.0.8, X64 NativeAOT x86-64-v3

WarmupCount=3  

```
| Method | Job        | Runtime       | IterationCount | LaunchCount | RunStrategy | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |----------- |-------------- |--------------- |------------ |------------ |---------:|----------:|----------:|-------:|----------:|
| 单次简单事务 | Job-WQWZZQ | .NET 9.0      | 10             | Default     | Throughput  | 1.129 μs | 0.0542 μs | 0.0358 μs | 0.1297 |   1.07 KB |
| 单次简单事务 | ShortRun   | NativeAOT 9.0 | 3              | 1           | Default     |       NA |        NA |        NA |     NA |        NA |

Benchmarks with issues:
  CatGaBenchmarks.单次简单事务: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
