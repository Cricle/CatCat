```

BenchmarkDotNet v0.15.4, Windows 10 (10.0.19045.6332/22H2/2022Update)
AMD Ryzen 7 5800H with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
  [Host]     : .NET 9.0.8, X64 NativeAOT x86-64-v3
  Job-WQWZZQ : .NET 9.0.8, X64 NativeAOT x86-64-v3

Runtime=.NET 9.0  IterationCount=10  RunStrategy=Throughput  
WarmupCount=3  

```
| Method            | Mean | Error |
|------------------ |-----:|------:|
| 单次简单事务            |   NA |    NA |
| 单次复杂事务            |   NA |    NA |
| &#39;批量简单事务 (100)&#39;    |   NA |    NA |
| &#39;高并发事务 (1000)&#39;    |   NA |    NA |
| &#39;幂等性测试 (100 次重复)&#39; |   NA |    NA |

Benchmarks with issues:
  CatGaBenchmarks.单次简单事务: Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  CatGaBenchmarks.单次复杂事务: Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  CatGaBenchmarks.'批量简单事务 (100)': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  CatGaBenchmarks.'高并发事务 (1000)': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
  CatGaBenchmarks.'幂等性测试 (100 次重复)': Job-WQWZZQ(Runtime=.NET 9.0, IterationCount=10, RunStrategy=Throughput, WarmupCount=3)
