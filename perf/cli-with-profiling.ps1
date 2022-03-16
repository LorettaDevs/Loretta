param (
	[ValidateNotNullOrEmpty()]
	[string] $VSPath = "B:\Program Files\Microsoft Visual Studio 2022 Preview"
)
$Modules = @(
	"Loretta.CLI.dll",
	"Loretta.CodeAnalysis.dll",
	"Loretta.CodeAnalysis.Lua.dll",
	"Loretta.CodeAnalysis.Lua.Experimental.dll"
)

dotnet build --configuration Release --project "$PSScriptRoot\..\src\Compilers\Lua\CommandLine\Loretta.CLI.csproj"

$env:COR_ENABLE_PROFILING = 1
$env:CORECLR_ENABLE_PROFILING = 1
$env:COR_PROFILER = "{324F817A-7420-4E6D-B3C1-143FBED6D855}"
$env:CORECLR_PROFILER = "{324F817A-7420-4E6D-B3C1-143FBED6D855}"
$env:COR_PROFILER_PATH_32 = "$VSPath\Common7\IDE\CommonExtensions\Platform\InstrumentationEngine\x86\MicrosoftInstrumentationEngine_x86.dll"
$env:CORECLR_PROFILER_PATH_32 = "$VSPath\Common7\IDE\CommonExtensions\Platform\InstrumentationEngine\x86\MicrosoftInstrumentationEngine_x86.dll"
$env:COR_PROFILER_PATH_64 = "$VSPath\Common7\IDE\CommonExtensions\Platform\InstrumentationEngine\x64\MicrosoftInstrumentationEngine_x64.dll"
$env:CORECLR_PROFILER_PATH_64 = "$VSPath\Common7\IDE\CommonExtensions\Platform\InstrumentationEngine\x64\MicrosoftInstrumentationEngine_x64.dll"
$env:MicrosoftInstrumentationEngine_ConfigPath32_PerfProfiler = "$VSPath\Common7\IDE\CommonExtensions\Platform\DiagnosticsHub\x86\PerfProfiler.config"
$env:MicrosoftInstrumentationEngine_ConfigPath64_PerfProfiler = "$VSPath\Common7\IDE\CommonExtensions\Platform\DiagnosticsHub\amd64\PerfProfiler.config"
$env:VSPERF_MODULES = [string]::Join(";", $Modules)
$env:VSPERF_EXCLUDE_SMALL_FUNCTIONS = 0
$env:COR_INTERACTION_PROFILING = 0
$env:COR_GC_PROFILING = 0

& "$PSScriptRoot\..\src\Compilers\Lua\CommandLine\bin\Release\net6.0\Loretta.CLI.exe"