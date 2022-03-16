``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19041.1415 (2004/May2020Update/20H1)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.100-preview.1.22110.4
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  Job-KUUXPS : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT

Runtime=.NET 6.0  RunStrategy=Monitoring  

```
|         Method |            File |           Mean |         Error |       StdDev |         Median |      Gen 0 |     Gen 1 |  Allocated |
|--------------- |---------------- |---------------:|--------------:|-------------:|---------------:|-----------:|----------:|-----------:|
| **Initialization** |        **anim.lua** |       **336.4 μs** |      **32.70 μs** |     **21.63 μs** |       **331.0 μs** |          **-** |         **-** |      **86 KB** |
| **Initialization** | **rustic-24mb.lua** | **3,835,911.7 μs** | **124,150.65 μs** | **82,117.97 μs** | **3,835,764.2 μs** | **19000.0000** | **9000.0000** | **327,881 KB** |
| **Initialization** |      **rustic.lua** |   **653,694.4 μs** |  **49,296.42 μs** | **32,606.53 μs** |   **639,193.4 μs** |  **4000.0000** | **2000.0000** |  **81,794 KB** |
