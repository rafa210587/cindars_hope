param(
    [string]$LogFile = ".\Logs\unity-compile-validation.log"
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$Path)

    $expanded = [Environment]::ExpandEnvironmentVariables($Path)
    if ([System.IO.Path]::IsPathRooted($expanded)) {
        return [System.IO.Path]::GetFullPath($expanded)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $expanded))
}

$logFullPath = Resolve-FullPath $LogFile

if (-not (Test-Path -LiteralPath $logFullPath -PathType Leaf)) {
    Write-Host "Unity log scan FAILED."
    Write-Host "Reason: Log file not found."
    Write-Host "Log: $LogFile"
    exit 1
}

$criticalPatterns = @(
    "error CS",
    "Compilation failed",
    "Script compilation failed",
    "Assembly-CSharp",
    "Unhandled exception",
    "Exception:",
    "Fatal error",
    "BuildFailedException",
    "The type or namespace name",
    "are you missing an assembly reference",
    "Application will terminate with return code 1"
)

$warningPatterns = @(
    "warning CS",
    "obsolete",
    "deprecated",
    "Asset import warning"
)

$criticalMatches = New-Object System.Collections.Generic.List[string]
$warningMatches = New-Object System.Collections.Generic.List[string]
$lines = Get-Content -LiteralPath $logFullPath

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    foreach ($pattern in $criticalPatterns) {
        if ($line -match [regex]::Escape($pattern)) {
            $criticalMatches.Add(("{0}: {1}" -f ($i + 1), $line.Trim()))
            break
        }
    }

    foreach ($pattern in $warningPatterns) {
        if ($line -match [regex]::Escape($pattern)) {
            $warningMatches.Add(("{0}: {1}" -f ($i + 1), $line.Trim()))
            break
        }
    }
}

if ($criticalMatches.Count -gt 0) {
    Write-Host "Unity log scan FAILED."
    Write-Host "Critical errors found:"
    Write-Host ""
    $criticalMatches | Select-Object -First 80 | ForEach-Object {
        Write-Host $_
    }

    if ($criticalMatches.Count -gt 80) {
        Write-Host ""
        Write-Host "Additional critical matches omitted: $($criticalMatches.Count - 80)"
    }

    if ($warningMatches.Count -gt 0) {
        Write-Host ""
        Write-Host "Warnings also found: $($warningMatches.Count)"
    }

    exit 1
}

Write-Host "Unity log scan PASSED."
Write-Host "No critical compile errors found."

if ($warningMatches.Count -gt 0) {
    Write-Host ""
    Write-Host "Warnings found:"
    $warningMatches | Select-Object -First 40 | ForEach-Object {
        Write-Host $_
    }

    if ($warningMatches.Count -gt 40) {
        Write-Host "Additional warnings omitted: $($warningMatches.Count - 40)"
    }
}

exit 0
