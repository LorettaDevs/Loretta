``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19041.1415 (2004/May2020Update/20H1)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]   : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  .NET 6.0 : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
| Method |   FileName |         Mean |       Error |      StdDev |      Gen 0 |     Gen 1 |  Gen 2 |  Allocated |
|------- |----------- |-------------:|------------:|------------:|-----------:|----------:|-------:|-----------:|
|  **Parse** |   **anim.lua** |     **244.9 μs** |     **2.30 μs** |     **1.80 μs** |     **7.3242** |    **2.6855** | **0.7324** |     **112 KB** |
|  **Parse** | **rustic.lua** | **639,261.6 μs** | **9,565.01 μs** | **8,479.13 μs** | **13000.0000** | **4000.0000** |      **-** | **240,143 KB** |
