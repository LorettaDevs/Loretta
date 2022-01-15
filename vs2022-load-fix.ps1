param(
    [switch]$Silent = $false
)

function Write-Log ([psobject] $Object, [System.ConsoleColor] $ForegroundColor = (Get-Host).ui.rawui.ForegroundColor, [System.ConsoleColor] $BackgroundColor = (Get-Host).ui.rawui.BackgroundColor, [switch] $NoNewline = $false)
{
    if (-not $Silent)
    {
        Write-Host $Object -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor -NoNewline:$NoNewline
    }
}

function Remove-Directory ([string] $Path, [int] $Indentation)
{
    $indent = [string]::new(' ', $Indentation)
    Write-Log "${indent}Deleting ${Path}... " -NoNewline
    if (Test-Path -Path $Path)
    {
        Remove-Item -Path $Path -Recurse -Force -ErrorAction Continue;
        Write-Log "deleted" -ForegroundColor ([System.ConsoleColor]::Green)
    }
    else
    {
        Write-Log "not found" -ForegroundColor ([System.ConsoleColor]::Red)
    }
}

Remove-Directory -Path ".vs" -Indentation 0
Remove-Directory -Path "src/Compilers/Core/Test/Utilities/bin" -Indentation 0
Remove-Directory -Path "src/Compilers/Core/Test/Utilities/obj" -Indentation 0
Remove-Directory -Path "src/Compilers/Lua/Test/Portable/bin" -Indentation 0
Remove-Directory -Path "src/Compilers/Lua/Test/Portable/obj" -Indentation 0