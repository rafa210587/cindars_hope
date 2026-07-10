param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path

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

function Remove-CSharpTrivia {
    param([string]$Text)

    $builder = [System.Text.StringBuilder]::new($Text.Length)
    $state = 'Code'
    $i = 0

    while ($i -lt $Text.Length) {
        $ch = $Text[$i]
        $next = if ($i + 1 -lt $Text.Length) { $Text[$i + 1] } else { [char]0 }

        switch ($state) {
            'Code' {
                if ($ch -eq '/' -and $next -eq '/') {
                    [void]$builder.Append('  ')
                    $i += 2
                    $state = 'LineComment'
                    continue
                }

                if ($ch -eq '/' -and $next -eq '*') {
                    [void]$builder.Append('  ')
                    $i += 2
                    $state = 'BlockComment'
                    continue
                }

                if ($ch -eq '@' -and $next -eq '"') {
                    [void]$builder.Append('  ')
                    $i += 2
                    $state = 'VerbatimString'
                    continue
                }

                if ($ch -eq '$' -and $next -eq '@' -and $i + 2 -lt $Text.Length -and $Text[$i + 2] -eq '"') {
                    [void]$builder.Append('   ')
                    $i += 3
                    $state = 'VerbatimString'
                    continue
                }

                if ($ch -eq '"') {
                    [void]$builder.Append(' ')
                    $i++
                    $state = 'String'
                    continue
                }

                if ($ch -eq "'") {
                    [void]$builder.Append(' ')
                    $i++
                    $state = 'Char'
                    continue
                }

                [void]$builder.Append($ch)
                $i++
                continue
            }

            'LineComment' {
                if ($ch -eq "`r" -or $ch -eq "`n") {
                    [void]$builder.Append($ch)
                    $state = 'Code'
                }
                else {
                    [void]$builder.Append(' ')
                }
                $i++
                continue
            }

            'BlockComment' {
                if ($ch -eq '*' -and $next -eq '/') {
                    [void]$builder.Append('  ')
                    $i += 2
                    $state = 'Code'
                    continue
                }

                [void]$builder.Append($(if ($ch -eq "`r" -or $ch -eq "`n") { $ch } else { ' ' }))
                $i++
                continue
            }

            'String' {
                if ($ch -eq '\' -and $i + 1 -lt $Text.Length) {
                    [void]$builder.Append('  ')
                    $i += 2
                    continue
                }

                [void]$builder.Append($(if ($ch -eq "`r" -or $ch -eq "`n") { $ch } else { ' ' }))
                $i++
                if ($ch -eq '"') {
                    $state = 'Code'
                }
                continue
            }

            'VerbatimString' {
                if ($ch -eq '"' -and $next -eq '"') {
                    [void]$builder.Append('  ')
                    $i += 2
                    continue
                }

                [void]$builder.Append($(if ($ch -eq "`r" -or $ch -eq "`n") { $ch } else { ' ' }))
                $i++
                if ($ch -eq '"') {
                    $state = 'Code'
                }
                continue
            }

            'Char' {
                if ($ch -eq '\' -and $i + 1 -lt $Text.Length) {
                    [void]$builder.Append('  ')
                    $i += 2
                    continue
                }

                [void]$builder.Append($(if ($ch -eq "`r" -or $ch -eq "`n") { $ch } else { ' ' }))
                $i++
                if ($ch -eq "'") {
                    $state = 'Code'
                }
                continue
            }
        }
    }

    return $builder.ToString()
}

$edges = @{}
$usingOnlyEdges = @{}
foreach ($file in $runtimeFiles) {
    $relativeToScripts = $file.FullName.Substring($scriptsRoot.Length + 1)
    $originModule = ($relativeToScripts -split '[\\/]')[0]
    $text = Remove-CSharpTrivia -Text ([System.IO.File]::ReadAllText($file.FullName))
    foreach ($match in [regex]::Matches($text, '(?m)^using\s+CindarsHope\.([A-Za-z_][A-Za-z0-9_]*)')) {
        $targetModule = $match.Groups[1].Value
        if ($targetModule -ne $originModule) {
            $edges["$originModule|$targetModule"] = $true
            $usingOnlyEdges["$originModule|$targetModule"] = $true
        }
    }

    foreach ($match in [regex]::Matches($text, '\bCindarsHope\.([A-Za-z_][A-Za-z0-9_]*)\b')) {
        $targetModule = $match.Groups[1].Value
        if ($targetModule -ne $originModule) {
            $edges["$originModule|$targetModule"] = $true
        }
    }
}

function Get-MutualPairs {
    param([hashtable]$EdgeSet)

    $result = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($key in $EdgeSet.Keys) {
        $parts = $key.Split('|')
        if ($EdgeSet.ContainsKey("$($parts[1])|$($parts[0])")) {
            $ordered = @($parts[0], $parts[1]) | Sort-Object
            [void]$result.Add("$($ordered[0])|$($ordered[1])")
        }
    }

    return $result
}

$cycles = Get-MutualPairs -EdgeSet $edges
$usingOnlyCycles = Get-MutualPairs -EdgeSet $usingOnlyEdges

Write-Output "CSharpFiles=$($allCodeFiles.Count)"
Write-Output "HardcodedPredefinedAssemblyFiles=$($hardcodedFiles.Count)"
Write-Output "InternalTypes=$($internalDeclarations.Count)"
Write-Output "InternalTypeTestCrossings=$($testCrossings.Count)"
foreach ($crossing in $testCrossings | Sort-Object Type, Consumer) {
    Write-Output "  INTERNAL_TEST|$($crossing.Type)|$($crossing.Definition)|$($crossing.Consumer)"
}
Write-Output "UsingOnlyModuleEdges=$($usingOnlyEdges.Count)"
Write-Output "UsingOnlyMutualModulePairs=$($usingOnlyCycles.Count)"
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
