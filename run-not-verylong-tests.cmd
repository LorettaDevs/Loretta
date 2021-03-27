@ECHO off
dotnet test --filter "Duration!=Very Long" -- xunit.parallelizeAssembly=true xunit.parallelizeTestCollections=true xunit.maxParallelThreads=-1
