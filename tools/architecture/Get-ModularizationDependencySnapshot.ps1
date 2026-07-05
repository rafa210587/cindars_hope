param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

$scriptsRoot = Join-Path $ProjectRoot 'Assets\_Game\Scripts'
$testsRoot = Join-Path $ProjectRoot 'Assets\_Game\Tests'
$runtimeFiles = @(
    Get-ChildItem -LiteralPath $scriptsRoot -Recurse -Filter '*.cs' |
        Where-Object { $_.FullName -notmatch '[\\/]Editor[\\/]' }
)
$allCodeFiles = @(
    Get-ChildItem -LiteralPath $scriptsRoot -Recurse -Filter '*.cs'
    Get-ChildItem -LiteralPath $testsRoot -Recurse -Filter '*.cs'
)

$predefinedAssemblyName = 'Assembly' + '-CSharp'
$hardcodedFiles = @(
    $allCodeFiles | Where-Object {
        [System.IO.File]::ReadAllText($_.FullName).Contains($predefinedAssemblyName)
    }
)

$internalDeclarations = [System.Collections.Generic.List[object]]::new()
$internalPattern = [regex]::new(
    '(?m)^\s*internal\s+(?:(?:sealed|static|abstract|partial|readonly)\s+)*' +
    '(?:class|struct|interface|enum|record(?:\s+struct|\s+class)?)\s+([A-Za-z_][A-Za-z0-9_]*)')

foreach ($file in Get-ChildItem -LiteralPath $scriptsRoot -Recurse -Filter '*.cs') {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    foreach ($match in $internalPattern.Matches($text)) {
        $internalDeclarations.Add([PSCustomObject]@{
            Name = $match.Groups[1].Value
            Path = $file.FullName.Substring($ProjectRoot.Length + 1).Replace('\', '/')
        })
    }
}

$testCrossings = [System.Collections.Generic.List[object]]::new()
foreach ($declaration in $internalDeclarations) {
    foreach ($testFile in Get-ChildItem -LiteralPath $testsRoot -Recurse -Filter '*.cs') {
        $hasCompiledReference = $false
        foreach ($line in [System.IO.File]::ReadAllLines($testFile.FullName)) {
            $trimmed = $line.TrimStart()
            if ($trimmed.StartsWith('//') -or $trimmed.StartsWith('*')) {
                continue
            }

            if ([regex]::IsMatch($line, "\b$([regex]::Escape($declaration.Name))\s*\.")) {
                $hasCompiledReference = $true
                break
            }
        }

        if ($hasCompiledReference) {
            $testCrossings.Add([PSCustomObject]@{
                Type = $declaration.Name
                Definition = $declaration.Path
                Consumer = $testFile.FullName.Substring($ProjectRoot.Length + 1).Replace('\', '/')
            })
        }
    }
}

$edges = @{}
foreach ($file in $runtimeFiles) {
    $relativeToScripts = $file.FullName.Substring($scriptsRoot.Length + 1)
    $sourceModule = ($relativeToScripts -split '[\\/]')[0]
    $text = [System.IO.File]::ReadAllText($file.FullName)
    foreach ($match in [regex]::Matches($text, '(?m)^using\s+CindarsHope\.([A-Za-z_][A-Za-z0-9_]*)')) {
        $targetModule = $match.Groups[1].Value
        if ($targetModule -ne $sourceModule) {
            $edges["$sourceModule|$targetModule"] = $true
        }
    }
}

$cycles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($key in $edges.Keys) {
    $parts = $key.Split('|')
    if ($edges.ContainsKey("$($parts[1])|$($parts[0])")) {
        $ordered = @($parts[0], $parts[1]) | Sort-Object
        [void]$cycles.Add("$($ordered[0])|$($ordered[1])")
    }
}

Write-Output "CSharpFiles=$($allCodeFiles.Count)"
Write-Output "HardcodedPredefinedAssemblyFiles=$($hardcodedFiles.Count)"
Write-Output "InternalTypes=$($internalDeclarations.Count)"
Write-Output "InternalTypeTestCrossings=$($testCrossings.Count)"
foreach ($crossing in $testCrossings | Sort-Object Type, Consumer) {
    Write-Output "  INTERNAL_TEST|$($crossing.Type)|$($crossing.Definition)|$($crossing.Consumer)"
}
Write-Output "RuntimeModuleEdges=$($edges.Count)"
Write-Output "MutualModulePairs=$($cycles.Count)"
foreach ($cycle in $cycles | Sort-Object) {
    Write-Output "  MUTUAL|$cycle"
}

if ($hardcodedFiles.Count -gt 0) {
    foreach ($file in $hardcodedFiles) {
        Write-Error "Hardcoded predefined assembly name: $($file.FullName)"
    }
    exit 1
}

exit 0
