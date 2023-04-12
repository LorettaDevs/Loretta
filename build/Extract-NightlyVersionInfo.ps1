Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Get the latest stable release flag
$latestStableVersion = $(& git tag --sort=taggerdate | Where-Object { $_ -match '^v\d+.\d+.\d+$' } | Select-Object -Last 1)
$commitsSinceLastRelease = $(& git log --oneline "$latestStableVersion.." | Where-Object { -not $_.Contains("[nightly ignore]") } | Measure-Object -Line).Lines

# Fetch the unreleased version
$unreleasedVersion = if ((Get-Content ./build/LibraryProject.props -Raw -ErrorAction SilentlyContinue) -cmatch '<Version>(\d+\.\d+\.\d+)(-[\w.+]+)?</Version>') {
    "v" + $Matches[1]
}
else {
    $False
}

if (-not $unreleasedVersion) {
    Write-Host "::error title=Invalid Version::Unable to read version from LibraryProject.props"
    exit 1
}
elseif ($latestStableVersion -eq $unreleasedVersion) {
    Write-Host "::error title=Invalid Version::Version in LibraryProject.props is the same as the last stable release"
    exit 1
}

# Get the latest nightly for our unreleased version
$latestNightlyVersion = $(& git tag --sort=taggerdate | Where-Object { $_.StartsWith("$unreleasedVersion-nightly.") } | Select-Object -Last 1)
$nightlyVersionToPublish = "$unreleasedVersion-nightly.$commitsSinceLastRelease"

if ($latestNightlyVersion -and $nightlyVersionToPublish -eq $latestNightlyVersion) {
    Write-Host "::notice title=No Changes::This nightly build is being skipped due to no changes having been made since the last nightly"
    Write-Output "::set-output name=HAS_NIGHTLY::no"
    exit 0
}

# Set script outputs
Write-Output "Latest Stable: $latestStableVersion"
Write-Output "Commits Since Last Stable: $commitsSinceLastRelease"
Write-Output "Next Stable Release: $unreleasedVersion"
Write-Output "Latest Nightly Release: $latestNightlyVersion"
Write-Output "Next Nightly Version: $nightlyVersionToPublish (no v: $($nightlyVersionToPublish.Substring(1)))"

"VERSION=$nightlyVersionToPublish" >> $env:GITHUB_OUTPUT
"NO_PREFIX_VERSION=$($nightlyVersionToPublish.Substring(1))" >> $env:GITHUB_OUTPUT
"HAS_NIGHTLY=yes" >> $env:GITHUB_OUTPUT
exit 0