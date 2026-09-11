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

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Required ratchet data missing: $Path" }
    $dataLines = @(Get-Content -LiteralPath $Path -Encoding UTF8 |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and -not $_.TrimStart().StartsWith('#') })
    if ($dataLines.Count -eq 0) { throw "Required ratchet data empty: $Path" }
    return $dataLines
}

function Assert-RatchetRelativePath {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path) -or [IO.Path]::IsPathRooted($Path)) { throw "Invalid ratchet path: $Path" }
    $rootPrefix = $ProjectRoot.TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
    $fullPath = [IO.Path]::GetFullPath((Join-Path $ProjectRoot $Path))
    if (-not $fullPath.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        $Path -cne $fullPath.Substring($rootPrefix.Length).Replace('\','/')) { throw "Noncanonical ratchet path: $Path" }
}

$rules = [ordered]@{}
foreach ($line in Read-DataLines -Path $rulesPath) {
    $parts = $line -split "`t", 2
    if ($parts.Count -ne 2 -or [string]::IsNullOrWhiteSpace($parts[0]) -or [string]::IsNullOrWhiteSpace($parts[1])) {
        throw "Invalid architecture rule line: $line"
    }

    if ($rules.Contains($parts[0])) { throw "Duplicate architecture rule: $($parts[0])" }
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

    Assert-RatchetRelativePath $parts[1]
    $maximum = 0
    if (-not [int]::TryParse($parts[2], [ref]$maximum) -or $maximum -lt 0) { throw 'Invalid architecture baseline maximum.' }
    $baselineKey = "$($parts[0])`n$($parts[1])"
    if ($baseline.ContainsKey($baselineKey)) { throw "Duplicate architecture baseline: $baselineKey" }
    $baseline[$baselineKey] = $maximum
}

$files = @(Get-ChildItem -LiteralPath $runtimeRoot -Recurse -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '[\\/]Editor[\\/]' })
if ($files.Count -eq 0) { throw 'Runtime source set is empty; ratchet cannot certify missing inputs.' }

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
$serializedKeys = @{}
foreach ($line in Read-DataLines -Path $serializedGuidBaselinePath) {
    $parts = $line -split "`t"
    if ($parts.Count -ne 3) {
        throw "Invalid serialized GUID baseline line: $line"
    }

    $serializedGuidCount++
    Assert-RatchetRelativePath $parts[1]
    if ([string]::IsNullOrWhiteSpace($parts[0]) -or $parts[2] -notmatch '^[a-fA-F0-9]{32}$') { throw 'Invalid serialized GUID baseline value.' }
    if ($serializedKeys.ContainsKey($parts[1])) { throw "Duplicate serialized GUID path: $($parts[1])" }
    $serializedKeys[$parts[1]] = $true
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

# Fonte unica do check antes duplicado no EditMode; inclui Editor e testes.
$predefinedAssemblyName = 'Assembly' + '-CSharp'
$sourceRoots = @($runtimeRoot, (Join-Path $ProjectRoot 'Assets/_Game/Tests'))
$assemblyChecked = 0
foreach ($sourceRoot in $sourceRoots) {
    if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) { throw "Source root missing: $sourceRoot" }
    foreach ($sourceFile in Get-ChildItem -LiteralPath $sourceRoot -Recurse -Filter '*.cs' -File) {
        $assemblyChecked++
        if ([IO.File]::ReadAllText($sourceFile.FullName).Contains($predefinedAssemblyName)) {
            $violations.Add("PredefinedAssemblyName: $($sourceFile.FullName)")
        }
    }
}
$summary.Add("PredefinedAssemblyName: checked=$assemblyChecked")

$summary | ForEach-Object { Write-Output $_ }

if ($violations.Count -gt 0) {
    Write-Error ("Architecture ratchet failed:`n- " + ($violations -join "`n- "))
    exit 1
}

Write-Output 'Architecture ratchet PASS: no tracked debt increased.'
exit 0
