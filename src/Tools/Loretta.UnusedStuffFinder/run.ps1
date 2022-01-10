# Enable OSR
$env:COMPlus_TC_OnStackReplacement=1

# Enable PGO
$env:COMPlus_TieredCompilation=1
$env:COMPlus_TieredPGO=1
$env:COMPlus_JitClassProfiling=1
$env:COMPlus_JitEnableGuardedDevirtualization=1
$env:COMPlus_JitInlinePolicyProfile=1
$env:COMPlus_TC_QuickJitForLoops=1

dotnet run -c Release