@ECHO off
dotnet test -- xunit.parallelizeAssembly=true xunit.parallelizeTestCollections=true xunit.maxParallelThreads=-1
