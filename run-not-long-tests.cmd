@ECHO off
dotnet test --filter "Duration!~Long" -- xunit.parallelizeAssembly=true xunit.parallelizeTestCollections=true xunit.maxParallelThreads=-1
