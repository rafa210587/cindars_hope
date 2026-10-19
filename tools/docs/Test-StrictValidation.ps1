param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$strictPath = Join-Path $repo 'tools/docs/run_strict_validation.ps1'
$tempParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd('\', '/')
$fixtureRoot = Join-Path $tempParent ('cindars-strict-test-' + [guid]::NewGuid().ToString('N'))
$utf8 = [System.Text.UTF8Encoding]::new($false)
$script:assertions = 0
$paths = @(
    'tools/validate_no_corruption.ps1', 'tools/docs/validate_docs.ps1',
    'tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1', 'tools/architecture/Test-ArchitectureRatchet.ps1',
    'tools/docs/check_spec_diff_completeness.ps1', 'tools/docs/check_spec_quality.ps1'
)

function Assert-StrictContract {
    param([bool]$Condition, [string]$Name)
    if (-not $Condition) { throw "Strict contract failed: $Name" }
    $script:assertions++
}
function Set-StepFixture {
    param([int]$Index, [string]$Body)
    $path = Join-Path $fixtureRoot $paths[$Index]
    [void][System.IO.Directory]::CreateDirectory((Split-Path -Parent $path))
    $trace = "Add-Content -LiteralPath (Join-Path (Get-Location).Path 'trace.txt') -Value '$Index'`n"
    [System.IO.File]::WriteAllText($path, $trace + $Body, $utf8)
}
function Reset-Fixtures {
    [System.IO.File]::WriteAllText((Join-Path $fixtureRoot 'trace.txt'), '', $utf8)
    for ($index=0; $index -lt $paths.Count; $index++) {
        Set-StepFixture $index "Write-Output 'noisy success line'; Write-Output '0'; Write-Warning 'fixture warning'; exit 0"
    }
}
function Invoke-StrictFixture {
    param([string]$ExtraArguments = '')
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo.FileName = (Get-Command powershell.exe).Source
    $process.StartInfo.Arguments = '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $strictPath + '" -ProjectRoot "' + $fixtureRoot + '"'
    $process.StartInfo.Arguments += ' ' + $ExtraArguments
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.CreateNoWindow = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    try {
        [void]$process.Start()
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        return [pscustomobject]@{ ExitCode=$process.ExitCode; Output=$stdout.Result + $stderr.Result }
    } finally { $process.Dispose() }
}

try {
    [void][System.IO.Directory]::CreateDirectory($fixtureRoot)
    Reset-Fixtures
    $result = Invoke-StrictFixture
    Assert-StrictContract ($result.ExitCode -eq 0 -and $result.Output.Contains('STRICT_VALIDATION_RESULT: GLOBAL_PASS')) 'noisy success passes globally'
    Assert-StrictContract (((Get-Content -LiteralPath (Join-Path $fixtureRoot 'trace.txt')) -join ',') -eq '0,1,2,3,4,5') 'all gates including ratchet run in order'
    Assert-StrictContract ($result.Output.Contains('noisy success line')) 'logs remain visible'

    $failures = @(
        @{Index=0; Body="Write-Output 'noisy failure'; exit 7"; Token='CORRUPTION_DETECTED'},
        @{Index=1; Body="Write-Output 'new docs failure'; exit 1"; Token='DOCS_VALIDATION_FAILURE'},
        @{Index=2; Body="Write-Output 'before throw'; throw 'fixture exception'"; Token='UNITY_PROJECT_BUILD_FAILURE'},
        @{Index=3; Body="Write-Output 'debt increased'; exit 9"; Token='ARCHITECTURE_RATCHET_FAILURE'},
        @{Index=4; Body="exit 2"; Token='DIFF_COMPLETENESS_FAILURE'},
        @{Index=5; Body="exit 3"; Token='QUALITY_CHECK_FAILURE'}
    )
    foreach ($failure in $failures) {
        Reset-Fixtures
        Set-StepFixture $failure.Index $failure.Body
        $result = Invoke-StrictFixture
        Assert-StrictContract ($result.ExitCode -eq 1 -and $result.Output.Contains('STRICT_VALIDATION_RESULT: ' + $failure.Token)) ('failure propagated: ' + $failure.Token)
        $expectedTrace = (0..$failure.Index) -join ','
        Assert-StrictContract (((Get-Content -LiteralPath (Join-Path $fixtureRoot 'trace.txt')) -join ',') -eq $expectedTrace) ('fail-fast: ' + $failure.Token)
        Assert-StrictContract (-not $result.Output.Contains('STRICT_VALIDATION_RESULT: GLOBAL_PASS') -and -not $result.Output.Contains('EXPECTED_FAIL_LEGACY_ONLY')) 'failure never masquerades as success/legacy'
    }
    Reset-Fixtures
    $missingPath = Join-Path $fixtureRoot $paths[1]
    Remove-Item -LiteralPath $missingPath
    $result = Invoke-StrictFixture
    Assert-StrictContract ($result.ExitCode -eq 1 -and $result.Output.Contains('VALIDATION_SCRIPT_MISSING')) 'missing script fails'
    Assert-StrictContract (((Get-Content -LiteralPath (Join-Path $fixtureRoot 'trace.txt')) -join ',') -eq '0') 'missing script stops later gates'
    Reset-Fixtures
    $result = Invoke-StrictFixture '-Gates docs,architecture'
    Assert-StrictContract ($result.ExitCode -eq 0 -and $result.Output.Contains('SCOPED_PASS') -and -not $result.Output.Contains('GLOBAL_PASS')) 'scoped pass is distinct'
    Assert-StrictContract (((Get-Content -LiteralPath (Join-Path $fixtureRoot 'trace.txt')) -join ',') -eq '1,3') 'scoped gates execute only selection in canonical order'
    foreach ($invalid in @('-Gates typo', '-Gates ""', '-Gates docs,,build')) {
        $result = Invoke-StrictFixture $invalid
        Assert-StrictContract ($result.ExitCode -ne 0) ('invalid selection rejected: ' + $invalid)
    }
    $manifestPath = Join-Path $fixtureRoot 'scope.json'
    [IO.File]::WriteAllText($manifestPath, '{"changedFiles":["note.md"],"allowedPaths":["note.md"]}', $utf8)
    $result = Invoke-StrictFixture ('-ScopePath "' + $manifestPath + '"')
    Assert-StrictContract ($result.ExitCode -eq 1 -and $result.Output.Contains('SCOPE_REQUIRES_EXPLICIT_GATES')) 'global cannot be disguised by scope'
    # Param deve ser a primeira instrucao no fixture.
    [IO.File]::WriteAllText((Join-Path $fixtureRoot $paths[5]), 'param([string]$ScopePath) if (-not (Test-Path -LiteralPath $ScopePath)) { exit 7 }; Write-Output "scope received"; exit 0', $utf8)
    $result = Invoke-StrictFixture ('-Gates quality -ScopePath "' + $manifestPath + '"')
    Assert-StrictContract ($result.ExitCode -eq 0 -and $result.Output.Contains('scope received')) 'scope reaches compatible gate'
    [IO.File]::WriteAllText($manifestPath, '{"changedFiles":["note.md"],"allowedPaths":["other.md"]}', $utf8)
    $result = Invoke-StrictFixture ('-Gates quality -ScopePath "' + $manifestPath + '"')
    Assert-StrictContract ($result.ExitCode -eq 1 -and $result.Output.Contains('INVALID_SCOPE')) 'unauthorized manifest fails before execution'
    Write-Output "STRICT_VALIDATION_TESTS: PASS ($script:assertions assertions; isolated temporary scripts)"
    exit 0
} finally {
    $resolvedFixture = [System.IO.Path]::GetFullPath($fixtureRoot)
    $expectedPrefix = $tempParent + [System.IO.Path]::DirectorySeparatorChar + 'cindars-strict-test-'
    if (-not $resolvedFixture.StartsWith($expectedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup target.' }
    if (Test-Path -LiteralPath $resolvedFixture) { Remove-Item -LiteralPath $resolvedFixture -Recurse -Force }
}
