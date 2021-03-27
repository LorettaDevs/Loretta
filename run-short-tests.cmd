@ECHO off
dotnet test --filter "(Duration~Short)" -- xunit.parallelizeAssembly=true xunit.parallelizeTestCollections=true xunit.maxParallelThreads=-1
