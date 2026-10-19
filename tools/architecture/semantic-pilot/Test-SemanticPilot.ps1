#requires -Version 7.0
$ErrorActionPreference='Stop'
$entry=Join-Path $PSScriptRoot 'Get-SemanticReferences.ps1'
$fixture=Join-Path $PSScriptRoot '../../../docs/validation/efficiency_20260912/semantic/fixture'
function Check([string]$file,[string]$symbol,[int]$max,[int]$exit,[bool]$complete,[int]$count) {
    $raw=& pwsh -NoProfile -File $entry -Project (Join-Path $fixture $file) -Symbol $symbol -MaxResults $max
    $actualExit=$LASTEXITCODE
    $data=$raw | ConvertFrom-Json
    if($actualExit -ne $exit -or $data.complete -ne $complete -or $data.totalMatches -ne $count) { throw "FAIL $file $symbol exit=$actualExit complete=$($data.complete) count=$($data.totalMatches) diagnostics=$($data.diagnostics -join ';')" }
    "PASS $file $symbol"
    return $data
}
function Places($d) { @($d.matches | ForEach-Object { "$($_.path):$($_.line):$($_.column):$($_.kind)" }) -join ',' }
$int=Check 'Fixture.csproj' 'M:Pilot.Thing.Work(System.Int32)' 20 0 $true 2 | Select-Object -Last 1
if((Places $int) -ne 'PartB.cs:10:17:invocation,PartB.cs:14:17:invocation') { throw 'FAIL int overload positions/kind' }
$string=Check 'Fixture.csproj' 'M:Pilot.Thing.Work(System.String)' 20 0 $true 1 | Select-Object -Last 1
if((Places $string) -ne 'PartB.cs:11:17:invocation') { throw 'FAIL string overload position/kind' }
$extension=Check 'Fixture.csproj' 'M:Pilot.Extensions.Extend(Pilot.Thing)' 20 0 $true 1 | Select-Object -Last 1
if((Places $extension) -ne 'PartB.cs:12:17:invocation') { throw 'FAIL extension position/kind' }
$type=Check 'Fixture.csproj' 'T:Pilot.Thing' 20 0 $true 6 | Select-Object -Last 1
if((Places $type) -ne 'PartA.cs:7:40:type,PartB.cs:6:16:type,PartB.cs:7:16:type,PartB.cs:7:27:type,PartB.cs:9:13:type,PartB.cs:9:27:type') { throw 'FAIL type positions, including return/property' }
$outer=Check 'Fixture.csproj' 'T:Pilot.Outer' 20 0 $true 1 | Select-Object -Last 1
if((Places $outer) -ne 'PartB.cs:5:16:type') { throw 'FAIL qualified outer type' }
$ctor=Check 'Fixture.csproj' 'M:Pilot.Thing.#ctor' 20 2 $false 0 | Select-Object -Last 1
if(-not @($ctor.diagnostics | Where-Object { $_ -like 'UNSUPPORTED_SYMBOL_KIND:*' }).Count) { throw 'FAIL ctor fail-closed' }
$truncated=Check 'Fixture.csproj' 'M:Pilot.Thing.Work(System.Int32)' 1 2 $false 2 | Select-Object -Last 1
if($truncated.matches.Count -ne 1 -or $truncated.omittedMatches -ne 1 -or (Places $truncated) -ne 'PartB.cs:10:17:invocation') { throw 'FAIL truncation count/position' }
$repeat=Check 'Fixture.csproj' 'M:Pilot.Thing.Work(System.Int32)' 20 0 $true 2 | Select-Object -Last 1
if($repeat.inputHash -ne $int.inputHash -or $int.inputHash -notmatch '^[0-9a-f]{64}$') { throw 'FAIL deterministic hash' }
$missing=Check 'Missing.csproj' 'T:Pilot.Thing' 20 2 $false 1 | Select-Object -Last 1
if(-not @($missing.diagnostics | Where-Object { $_ -like 'MISSING_INPUT:*' }).Count) { throw 'FAIL missing metadata diagnostic' }
$compileError=Check 'Error.csproj' 'T:Pilot.Broken' 20 2 $false 0 | Select-Object -Last 1
if(-not @($compileError.diagnostics | Where-Object { $_ -like 'COMPILATION_ERROR:*' }).Count) { throw 'FAIL compilation diagnostic' }
$conditional=Check 'Conditional.csproj' 'T:Pilot.Thing' 20 2 $false 1 | Select-Object -Last 1
if(-not @($conditional.diagnostics | Where-Object { $_ -like 'UNSUPPORTED_CONDITION:*' }).Count) { throw 'FAIL condition diagnostic' }
$sdk=Check 'Sdk.csproj' 'T:Pilot.Thing' 20 2 $false 1 | Select-Object -Last 1
if(-not @($sdk.diagnostics | Where-Object { $_ -like 'UNSUPPORTED_PROJECT_ROOT_OR_SDK:*' }).Count) { throw 'FAIL SDK diagnostic' }
$unresolved=Check 'Fixture.csproj' 'T:Pilot.DoesNotExist' 20 2 $false 0 | Select-Object -Last 1
if(-not @($unresolved.diagnostics | Where-Object { $_ -like 'UNRESOLVED_SYMBOL:*' }).Count) { throw 'FAIL unresolved diagnostic' }
'PASS 13 query cases with position/kind, bounded-output, hash, and failure-semantic assertions'
