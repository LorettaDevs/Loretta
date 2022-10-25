[CmdletBinding()]
param (
    # Path of the project to move unshipped entries to shipped
    [Parameter(Mandatory = $true, Position = 0, HelpMessage = "Path to one or more locations.")]
    [string]$Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$NULLABLE_HEADER = "#nullable enable"

$unshippedPath = Join-Path $Path "PublicAPI.Unshipped.txt"
$shippedPath = Join-Path $Path "PublicAPI.Shipped.txt"

Write-Output "Unshipped: $unshippedPath"
Write-Output "Shipped:   $unshippedPath"

$unshipped = Get-Content $unshippedPath | Select-Object -Skip 1
$shipped = Get-Content $shippedPath | Select-Object -Skip 1

$removed = $unshipped | Where-Object { $_.StartsWith("*REMOVED*") } | ForEach-Object { $_.Substring("*REMOVED*".Length) }
$added = $unshipped | Where-Object { -not $_.StartsWith("*REMOVED*") }

$shipped = $shipped | Where-Object { $removed -cnotcontains $_ }
$shipped += $added

$shipped = (@($NULLABLE_HEADER) + $shipped) | Sort-Object

Set-Content $unshippedPath $NULLABLE_HEADER
Set-Content $shippedPath $shipped