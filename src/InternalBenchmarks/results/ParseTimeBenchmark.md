``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19041.1415 (2004/May2020Update/20H1)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]   : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  .NET 6.0 : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
| Method |                 File |         Mean |        Error |       StdDev | Mean Throughput | Median Throughput |       Median |      Gen 0 |     Gen 1 |  Gen 2 |  Allocated |
|------- |--------------------- |-------------:|-------------:|-------------:|----------------:|------------------:|-------------:|-----------:|----------:|-------:|-----------:|
|  **Parse** | **sampl(...)m.lua [25]** |     **233.1 μs** |      **4.09 μs** |      **3.42 μs** |      **11.89MiB/s** |        **11.96MiB/s** |     **231.6 μs** |     **6.8359** |    **2.4414** | **0.4883** |     **108 KB** |
|  **Parse** | **sampl(...)c.lua [27]** | **713,273.8 μs** | **14,043.04 μs** | **15,608.80 μs** |       **8.42MiB/s** |         **8.39MiB/s** | **715,620.1 μs** | **13000.0000** | **5000.0000** |      **-** | **236,732 KB** |
