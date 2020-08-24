``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
AMD Ryzen 7 2700, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]     : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  Job-ULZLCD : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  Job-ZRYYQN : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT

RunStrategy=Throughput  

```
|          Method |        Job |       Runtime | Value |      Mean |     Error |    StdDev | Ratio | RatioSD | Code Size |
|---------------- |----------- |-------------- |------ |----------:|----------:|----------:|------:|--------:|----------:|
| **IsAlphaNumericA** | **Job-ULZLCD** | **.NET Core 3.1** |     **7** | **0.2661 ns** | **0.0127 ns** | **0.0119 ns** |  **1.00** |    **0.00** |      **41 B** |
| IsAlphaNumericB | Job-ULZLCD | .NET Core 3.1 |     7 | 0.5593 ns | 0.0130 ns | 0.0122 ns |  2.11 |    0.10 |      44 B |
| IsAlphaNumericC | Job-ULZLCD | .NET Core 3.1 |     7 | 0.1800 ns | 0.0070 ns | 0.0066 ns |  0.68 |    0.03 |      41 B |
| IsAlphaNumericD | Job-ULZLCD | .NET Core 3.1 |     7 | 0.5375 ns | 0.0147 ns | 0.0138 ns |  2.02 |    0.11 |      44 B |
| IsAlphaNumericA | Job-ZRYYQN | .NET Core 5.0 |     7 | 0.3166 ns | 0.0068 ns | 0.0061 ns |  1.19 |    0.07 |      41 B |
| IsAlphaNumericB | Job-ZRYYQN | .NET Core 5.0 |     7 | 0.5711 ns | 0.0120 ns | 0.0100 ns |  2.15 |    0.11 |      44 B |
| IsAlphaNumericC | Job-ZRYYQN | .NET Core 5.0 |     7 | 0.2624 ns | 0.0080 ns | 0.0075 ns |  0.99 |    0.05 |      41 B |
| IsAlphaNumericD | Job-ZRYYQN | .NET Core 5.0 |     7 | 0.5315 ns | 0.0088 ns | 0.0078 ns |  2.00 |    0.09 |      44 B |
|                 |            |               |       |           |           |           |       |         |           |
| **IsAlphaNumericA** | **Job-ULZLCD** | **.NET Core 3.1** |     **G** | **0.1840 ns** | **0.0094 ns** | **0.0088 ns** |  **1.00** |    **0.00** |      **41 B** |
| IsAlphaNumericB | Job-ULZLCD | .NET Core 3.1 |     G | 0.5568 ns | 0.0070 ns | 0.0065 ns |  3.03 |    0.14 |      44 B |
| IsAlphaNumericC | Job-ULZLCD | .NET Core 3.1 |     G | 0.2774 ns | 0.0079 ns | 0.0074 ns |  1.51 |    0.09 |      41 B |
| IsAlphaNumericD | Job-ULZLCD | .NET Core 3.1 |     G | 0.5350 ns | 0.0088 ns | 0.0078 ns |  2.92 |    0.14 |      44 B |
| IsAlphaNumericA | Job-ZRYYQN | .NET Core 5.0 |     G | 0.1893 ns | 0.0210 ns | 0.0164 ns |  1.03 |    0.09 |      41 B |
| IsAlphaNumericB | Job-ZRYYQN | .NET Core 5.0 |     G | 0.5595 ns | 0.0052 ns | 0.0041 ns |  3.04 |    0.15 |      44 B |
| IsAlphaNumericC | Job-ZRYYQN | .NET Core 5.0 |     G | 0.2832 ns | 0.0012 ns | 0.0010 ns |  1.54 |    0.07 |      41 B |
| IsAlphaNumericD | Job-ZRYYQN | .NET Core 5.0 |     G | 0.5430 ns | 0.0075 ns | 0.0062 ns |  2.95 |    0.16 |      44 B |
|                 |            |               |       |           |           |           |       |         |           |
| **IsAlphaNumericA** | **Job-ULZLCD** | **.NET Core 3.1** |     **\** | **0.1895 ns** | **0.0024 ns** | **0.0023 ns** |  **1.00** |    **0.00** |      **41 B** |
| IsAlphaNumericB | Job-ULZLCD | .NET Core 3.1 |     \ | 0.5625 ns | 0.0068 ns | 0.0064 ns |  2.97 |    0.06 |      44 B |
| IsAlphaNumericC | Job-ULZLCD | .NET Core 3.1 |     \ | 0.1835 ns | 0.0020 ns | 0.0017 ns |  0.97 |    0.02 |      41 B |
| IsAlphaNumericD | Job-ULZLCD | .NET Core 3.1 |     \ | 0.5377 ns | 0.0098 ns | 0.0082 ns |  2.84 |    0.07 |      44 B |
| IsAlphaNumericA | Job-ZRYYQN | .NET Core 5.0 |     \ | 0.1925 ns | 0.0045 ns | 0.0040 ns |  1.02 |    0.03 |      41 B |
| IsAlphaNumericB | Job-ZRYYQN | .NET Core 5.0 |     \ | 0.5765 ns | 0.0113 ns | 0.0094 ns |  3.05 |    0.06 |      44 B |
| IsAlphaNumericC | Job-ZRYYQN | .NET Core 5.0 |     \ | 0.2643 ns | 0.0012 ns | 0.0010 ns |  1.40 |    0.02 |      41 B |
| IsAlphaNumericD | Job-ZRYYQN | .NET Core 5.0 |     \ | 0.5461 ns | 0.0095 ns | 0.0088 ns |  2.88 |    0.05 |      44 B |
|                 |            |               |       |           |           |           |       |         |           |
| **IsAlphaNumericA** | **Job-ULZLCD** | **.NET Core 3.1** |     **g** | **0.1836 ns** | **0.0021 ns** | **0.0019 ns** |  **1.00** |    **0.00** |      **41 B** |
| IsAlphaNumericB | Job-ULZLCD | .NET Core 3.1 |     g | 0.5609 ns | 0.0053 ns | 0.0044 ns |  3.06 |    0.04 |      44 B |
| IsAlphaNumericC | Job-ULZLCD | .NET Core 3.1 |     g | 0.5623 ns | 0.0019 ns | 0.0017 ns |  3.06 |    0.03 |      41 B |
| IsAlphaNumericD | Job-ULZLCD | .NET Core 3.1 |     g | 0.5357 ns | 0.0062 ns | 0.0055 ns |  2.92 |    0.05 |      44 B |
| IsAlphaNumericA | Job-ZRYYQN | .NET Core 5.0 |     g | 0.1939 ns | 0.0065 ns | 0.0057 ns |  1.06 |    0.03 |      41 B |
| IsAlphaNumericB | Job-ZRYYQN | .NET Core 5.0 |     g | 0.5594 ns | 0.0058 ns | 0.0051 ns |  3.05 |    0.05 |      44 B |
| IsAlphaNumericC | Job-ZRYYQN | .NET Core 5.0 |     g | 0.2814 ns | 0.0051 ns | 0.0045 ns |  1.53 |    0.03 |      41 B |
| IsAlphaNumericD | Job-ZRYYQN | .NET Core 5.0 |     g | 0.5608 ns | 0.0153 ns | 0.0143 ns |  3.05 |    0.07 |      44 B |
