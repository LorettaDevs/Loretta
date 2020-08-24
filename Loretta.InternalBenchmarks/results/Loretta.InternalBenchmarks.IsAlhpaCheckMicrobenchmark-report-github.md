``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
AMD Ryzen 7 2700, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]     : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  Job-UGDYER : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  Job-XKXBJY : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT

RunStrategy=Throughput  

```
|    Method |        Job |       Runtime | Value |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD | Code Size |
|---------- |----------- |-------------- |------ |----------:|----------:|----------:|----------:|------:|--------:|----------:|
| **IsAlphaAA** | **Job-UGDYER** | **.NET Core 3.1** |     **0** | **0.5945 ns** | **0.0073 ns** | **0.0061 ns** | **0.5953 ns** |  **1.00** |    **0.00** |      **34 B** |
| IsAlphaAB | Job-UGDYER | .NET Core 3.1 |     0 | 0.2930 ns | 0.0037 ns | 0.0033 ns | 0.2941 ns |  0.49 |    0.01 |      36 B |
|  IsAlphaB | Job-UGDYER | .NET Core 3.1 |     0 | 0.0187 ns | 0.0045 ns | 0.0042 ns | 0.0200 ns |  0.03 |    0.01 |      24 B |
| IsAlphaAA | Job-XKXBJY | .NET Core 5.0 |     0 | 0.2856 ns | 0.0061 ns | 0.0054 ns | 0.2839 ns |  0.48 |    0.01 |      34 B |
| IsAlphaAB | Job-XKXBJY | .NET Core 5.0 |     0 | 0.3276 ns | 0.0059 ns | 0.0055 ns | 0.3261 ns |  0.55 |    0.01 |      36 B |
|  IsAlphaB | Job-XKXBJY | .NET Core 5.0 |     0 | 0.0362 ns | 0.0097 ns | 0.0075 ns | 0.0357 ns |  0.06 |    0.01 |      24 B |
|           |            |               |       |           |           |           |           |       |         |           |
| **IsAlphaAA** | **Job-UGDYER** | **.NET Core 3.1** |     **G** | **0.2612 ns** | **0.0074 ns** | **0.0069 ns** | **0.2631 ns** |  **1.00** |    **0.00** |      **34 B** |
| IsAlphaAB | Job-UGDYER | .NET Core 3.1 |     G | 0.2911 ns | 0.0061 ns | 0.0054 ns | 0.2924 ns |  1.12 |    0.03 |      36 B |
|  IsAlphaB | Job-UGDYER | .NET Core 3.1 |     G | 0.0150 ns | 0.0110 ns | 0.0103 ns | 0.0119 ns |  0.06 |    0.04 |      24 B |
| IsAlphaAA | Job-XKXBJY | .NET Core 5.0 |     G | 0.5808 ns | 0.0017 ns | 0.0014 ns | 0.5809 ns |  2.23 |    0.06 |      34 B |
| IsAlphaAB | Job-XKXBJY | .NET Core 5.0 |     G | 0.3268 ns | 0.0079 ns | 0.0066 ns | 0.3283 ns |  1.25 |    0.04 |      36 B |
|  IsAlphaB | Job-XKXBJY | .NET Core 5.0 |     G | 0.0476 ns | 0.0099 ns | 0.0077 ns | 0.0452 ns |  0.18 |    0.03 |      24 B |
|           |            |               |       |           |           |           |           |       |         |           |
| **IsAlphaAA** | **Job-UGDYER** | **.NET Core 3.1** |     **g** | **0.2488 ns** | **0.0047 ns** | **0.0039 ns** | **0.2480 ns** |  **1.00** |    **0.00** |      **34 B** |
| IsAlphaAB | Job-UGDYER | .NET Core 3.1 |     g | 0.2853 ns | 0.0045 ns | 0.0038 ns | 0.2844 ns |  1.15 |    0.02 |      36 B |
|  IsAlphaB | Job-UGDYER | .NET Core 3.1 |     g | 0.0346 ns | 0.0099 ns | 0.0077 ns | 0.0331 ns |  0.14 |    0.03 |      24 B |
| IsAlphaAA | Job-XKXBJY | .NET Core 5.0 |     g | 0.2912 ns | 0.0206 ns | 0.0172 ns | 0.2816 ns |  1.17 |    0.07 |      34 B |
| IsAlphaAB | Job-XKXBJY | .NET Core 5.0 |     g | 0.3232 ns | 0.0053 ns | 0.0044 ns | 0.3227 ns |  1.30 |    0.03 |      36 B |
|  IsAlphaB | Job-XKXBJY | .NET Core 5.0 |     g | 0.0389 ns | 0.0043 ns | 0.0036 ns | 0.0371 ns |  0.16 |    0.01 |      24 B |
