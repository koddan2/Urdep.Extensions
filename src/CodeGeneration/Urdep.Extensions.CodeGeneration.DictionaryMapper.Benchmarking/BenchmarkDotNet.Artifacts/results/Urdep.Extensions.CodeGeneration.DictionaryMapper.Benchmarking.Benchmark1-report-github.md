``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19044.2846/21H2/November2021Update)
11th Gen Intel Core i7-11700KF 3.60GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|            Method |       Mean |    Error |   StdDev |
|------------------ |-----------:|---------:|---------:|
|       FromObject1 |   691.5 ns |  9.57 ns |  8.48 ns |
| FromToDictionary1 | 1,649.9 ns | 14.14 ns | 11.81 ns |
|   FromDictionary1 |   984.9 ns |  9.44 ns |  7.89 ns |
|       FromObject2 |   985.6 ns | 14.16 ns | 13.24 ns |
| FromToDictionary2 | 1,325.6 ns |  7.32 ns |  5.71 ns |
|   FromDictionary2 |   862.3 ns |  9.13 ns |  8.54 ns |
