param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path

$rulesPath = Join-Path $PSScriptRoot 'architecture-ratchet-rules.tsv'
$baselinePath = Join-Path $PSScriptRoot 'architecture-ratchet-baseline.tsv'
$serializedGuidBaselinePath = Join-Path $PSScriptRoot 'serialized-guid-baseline.tsv'
$runtimeRoot = Join-Path $ProjectRoot 'Assets\_Game\Scripts'

function Read-DataLines {
    param([string]$Path)

    Get-Content -LiteralPath $Path -Encoding UTF8 |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and -not $_.StartsWith('#') }
}

$rules = [ordered]@{}
foreach ($line in Read-DataLines -Path $rulesPath) {
    $parts = $line -split "`t", 2
    if ($parts.Count -ne 2) {
        throw "Invalid architecture rule line: $line"
    }

    $rules[$parts[0]] = [regex]::new(
        $parts[1],
        [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)
}

$baseline = @{}
foreach ($line in Read-DataLines -Path $baselinePath) {
    $parts = $line -split "`t"
    if ($parts.Count -ne 3) {
        throw "Invalid architecture baseline line: $line"
    }

    if (-not $rules.Contains($parts[0])) {
        throw "Baseline references unknown rule '$($parts[0])'."
    }

    $baseline["$($parts[0])`n$($parts[1])"] = [int]$parts[2]
}

$files = Get-ChildItem -LiteralPath $runtimeRoot -Recurse -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '[\\/]Editor[\\/]' }

$violations = [System.Collections.Generic.List[string]]::new()
$summary = [System.Collections.Generic.List[string]]::new()

foreach ($ruleEntry in $rules.GetEnumerator()) {
    $currentTotal = 0
    $allowedTotal = 0

    foreach ($entry in $baseline.GetEnumerator()) {
        if ($entry.Key.StartsWith("$($ruleEntry.Key)`n", [System.StringComparison]::Ordinal)) {
            $allowedTotal += $entry.Value
        }
    }

    foreach ($file in $files) {
        $rootPrefix = $ProjectRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
        $relativePath = $file.FullName.Substring($rootPrefix.Length).Replace('\', '/')
        $currentCount = $ruleEntry.Value.Matches([System.IO.File]::ReadAllText($file.FullName)).Count
        $currentTotal += $currentCount
        $key = "$($ruleEntry.Key)`n$relativePath"
        $allowedCount = if ($baseline.ContainsKey($key)) { $baseline[$key] } else { 0 }

        if ($currentCount -gt $allowedCount) {
            $violations.Add(
                "$($ruleEntry.Key): $relativePath has $currentCount occurrence(s), baseline allows $allowedCount.")
        }
    }

    $summary.Add("$($ruleEntry.Key): current=$currentTotal baseline_max=$allowedTotal")
}

$serializedGuidCount = 0
foreach ($line in Read-DataLines -Path $serializedGuidBaselinePath) {
    $parts = $line -split "`t"
    if ($parts.Count -ne 3) {
        throw "Invalid serialized GUID baseline line: $line"
    }

    $serializedGuidCount++
    $assetPath = Join-Path $ProjectRoot $parts[1]
    $metaPath = "$assetPath.meta"
    if (-not (Test-Path -LiteralPath $assetPath) -or -not (Test-Path -LiteralPath $metaPath)) {
        $violations.Add("$($parts[0]): missing asset or meta: $($parts[1]).")
        continue
    }

    $guidMatch = Select-String -LiteralPath $metaPath -Pattern '^guid:\s*(\S+)' | Select-Object -First 1
    $actualGuid = if ($guidMatch) { $guidMatch.Matches[0].Groups[1].Value } else { '<none>' }
    if ($actualGuid -cne $parts[2]) {
        $violations.Add(
            "$($parts[0]): GUID changed for $($parts[1]); expected $($parts[2]), actual $actualGuid.")
    }
}

$summary.Add("SerializedGuidBaseline: verified=$serializedGuidCount")

$summary | ForEach-Object { Write-Output $_ }

if ($violations.Count -gt 0) {
    Write-Error ("Architecture ratchet failed:`n- " + ($violations -join "`n- "))
    exit 1
}

Write-Output 'Architecture ratchet PASS: no tracked debt increased.'
exit 0
