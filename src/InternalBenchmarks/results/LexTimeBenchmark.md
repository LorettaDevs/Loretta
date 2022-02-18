``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19041.1415 (2004/May2020Update/20H1)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]   : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  .NET 6.0 : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
| Method |                 File |         Mean |      Error |     StdDev | Mean Throughput | Median Throughput |       Median |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|------- |--------------------- |-------------:|-----------:|-----------:|----------------:|------------------:|-------------:|---------:|---------:|---------:|----------:|
|    **Lex** | **sampl(...)m.lua [25]** |     **30.80 μs** |   **0.607 μs** |   **0.944 μs** |      **89.95MiB/s** |        **90.24MiB/s** |     **30.70 μs** |   **2.6245** |   **0.0305** |        **-** |     **43 KB** |
|    **Lex** | **sampl(...)c.lua [27]** | **57,572.13 μs** | **618.243 μs** | **516.261 μs** |     **104.30MiB/s** |       **104.00MiB/s** | **57,738.16 μs** | **555.5556** | **555.5556** | **555.5556** | **47,821 KB** |
