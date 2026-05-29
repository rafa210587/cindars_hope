# Check runtime scripts for prohibited scene search APIs.

param(
    [switch]$FailOnViolation
)

$ErrorActionPreference = "Stop"

$runtimeFiles = @()
try {
    $status = git status --porcelain
    if ($status) {
        $runtimeFiles = $status |
            ForEach-Object { $_.Substring(3) -replace '\\', '/' } |
            Where-Object {
                $_ -match '^Assets/_Game/Scripts/.*\.cs$' -and
                $_ -notmatch '/Editor/'
            }
    }
}
catch {
    Write-Warning "Could not inspect git status: $($_.Exception.Message)"
    exit 0
}

if (-not $runtimeFiles -or $runtimeFiles.Count -eq 0) {
    Write-Host "[RUNTIME_SEARCH] No changed runtime C# files detected."
    exit 0
}

$patterns = @(
    'GameObject\.Find\s*\(',
    'FindObjectOfType\s*\(',
    'FindObjectsOfType\s*\(',
    'FindObjectsByType\s*\('
)

$violations = @()
foreach ($file in $runtimeFiles) {
    if (-not (Test-Path $file)) {
        continue
    }

    $lines = Get-Content $file
    for ($i = 0; $i -lt $lines.Count; $i++) {
        foreach ($pattern in $patterns) {
            if ($lines[$i] -match $pattern) {
                $violations += "${file}:$($i + 1): $($lines[$i].Trim())"
            }
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Warning "[RUNTIME_SEARCH] Prohibited runtime scene search detected:"
    foreach ($violation in $violations) {
        Write-Warning "  $violation"
    }

    if ($FailOnViolation) {
        exit 1
    }
}
else {
    Write-Host "[RUNTIME_SEARCH] No prohibited runtime scene search in changed runtime files."
}

exit 0
