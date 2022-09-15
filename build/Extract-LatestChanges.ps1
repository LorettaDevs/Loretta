Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$changeLines = Get-Content ./CHANGES.md
$currentLines = @()

$adding = $False
foreach ($line in $changeLines) {
    if ($line.StartsWith("## ")) {
        if ($adding) {
            $adding = $False;
            break;
        }
        else {
            $adding = $True;
        }
    }
    elseif ($adding) {
        $currentLines += $line;
    }
}

$currentLines = [string]::Join([System.Environment]::NewLine, $currentLines).Trim().Split([System.Environment]::NewLine)

Set-Content ./LATESTCHANGES.md $currentLines