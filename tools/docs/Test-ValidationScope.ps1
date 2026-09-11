param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/')
$fixture = Join-Path $tempParent ('cindars-scope-contract-' + [guid]::NewGuid().ToString('N'))
$utf8 = New-Object Text.UTF8Encoding($false)
$script:checks = 0
function Assert-Contract([bool]$Condition,[string]$Name) {
    if (-not $Condition) { throw "Scope contract failed: $Name" }
    $script:checks++
}
function Write-Fixture([string]$RelativePath,[string]$Content) {
    $path = Join-Path $fixture $RelativePath
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
    [IO.File]::WriteAllText($path,$Content,$utf8)
}
function Invoke-Gate([string]$Name,[string[]]$Parameters=@()) {
    $previous = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $captured = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $repo ('tools/docs/'+$Name)) @Parameters 2>&1
        return [pscustomobject]@{ Exit=$LASTEXITCODE; Output=$captured -join "`n" }
    } finally { $ErrorActionPreference = $previous }
}
try {
    [IO.Directory]::CreateDirectory($fixture) | Out-Null
    Push-Location -LiteralPath $fixture
    & git init --quiet
    if ($LASTEXITCODE -ne 0) { throw 'Fixture git init failed.' }
    Write-Fixture 'Assets/Scenes/Concurrent.unity' 'fixture scene'
    Write-Fixture 'Assets/Scenes/Authorized Scene.unity' 'fixture scene'
    & git add -- 'Assets/Scenes/Concurrent.unity' 'Assets/Scenes/Authorized Scene.unity'
    $global = Invoke-Gate 'check_spec_quality.ps1'
    Assert-Contract ($global.Exit -ne 0 -and $global.Output.Contains('Forbidden files altered')) 'global keeps existing forbidden asset check'
    $manifestPath = Join-Path $fixture 'scope.json'
    Write-Fixture 'scope.json' '{"changedFiles":["Assets/Scenes/Authorized Scene.unity"],"allowedPaths":["Assets/Scenes/Authorized Scene.unity"]}'
    $scoped = Invoke-Gate 'check_spec_quality.ps1' @('-ScopePath',$manifestPath)
    Assert-Contract ($scoped.Exit -eq 0 -and $scoped.Output.Contains('SCOPED')) ('authorized scene with spaces ignores unrelated dirty: '+$scoped.Output)
    foreach ($body in @(
        '{"changedFiles":["Assets/Scenes/Concurrent.unity"],"allowedPaths":["Assets/Scenes/Authorized Scene.unity"]}',
        '{"changedFiles":["../outside.md"],"allowedPaths":["../outside.md"]}',
        '{"changedFiles":[],"allowedPaths":[]}',
        '{"changedFiles":[null],"allowedPaths":[null]}',
        '{"changedFiles":[null,null],"allowedPaths":[null,null]}',
        '{"changedFiles":["note.md",null],"allowedPaths":["note.md"]}',
        '{"changedFiles":["note.md"],"allowedPaths":["note.md",null]}',
        '{"changedFiles":["note.md"],"allowedPaths":["note.md"],"reportPaths":[null]}',
        '{"changedFiles":["note.md"],"allowedPaths":["note.md"],"reportPaths":[null,null]}',
        '{"changedFiles":["note.md"],"allowedPaths":["note.md"],"reportPaths":null}',
        '{"changedFiles":["note.md"],"allowedPaths":["*.md"]}',
        '{broken'
    )) {
        Write-Fixture 'scope.json' $body
        $scoped = Invoke-Gate 'check_spec_quality.ps1' @('-ScopePath',$manifestPath)
        Assert-Contract ($scoped.Exit -ne 0) ('invalid/unauthorized scope rejected: '+$body)
    }
    Write-Fixture '.claude/operational.lock' ''
    Write-Fixture 'scope.json' '{"changedFiles":[".claude/operational.lock"],"allowedPaths":[".claude/operational.lock"]}'
    $scoped = Invoke-Gate 'check_spec_quality.ps1' @('-ScopePath',$manifestPath)
    Assert-Contract ($scoped.Exit -ne 0) 'scope cannot permit operational lock artifact'

    # Fixture documental propositalmente incompleta: assertar o diagnostico especifico,
    # nao fingir que todos os requisitos globais de docs passaram.
    Write-Fixture 'README.md' 'Legitimate variable name in documentation: $referenceCount.'
    foreach ($documentName in @('AGENTS.md','CLAUDE.md','PROJECT_LOG.md')) { Write-Fixture $documentName 'Fixture document.' }
    Write-Fixture 'tools/example.ps1' '$source = 1; $ref = 2; $dest = 3'
    $docs = Invoke-Gate 'validate_docs.ps1'
    Assert-Contract ($docs.Output.Contains('No template placeholders found.') -and -not $docs.Output.Contains('Placeholder found:')) 'PowerShell variables and longer names are not document placeholders'
    Write-Fixture 'README.md' 'Broken evidence link: [evidence]($evidence)'
    $docs = Invoke-Gate 'validate_docs.ps1'
    Assert-Contract ($docs.Exit -ne 0 -and $docs.Output.Contains('Placeholder found:') -and $docs.Output.Contains('README.md')) ('actual markdown placeholder still fails: '+$docs.Output)

    Write-Fixture 'Assets/_Game/Scripts/Example.cs' 'fixture only'
    Write-Fixture 'docs/validation/example_execution_report.md' @'
## Acceptance criteria extracted
Preserve transaction quantity.
## Existing systems audit
Existing tests cover quantity.
## Spec Compliance Matrix
Quantity -> existing fixture.
## Validation
run_strict_validation.ps1 scoped; evidence reviewed separately.
Existing tests executed: YES
Test command: RunUnityEditModeTests.ps1 -TestFilter Existing
Test evidence: existing-results.xml
Test result: PASS
## Honest status rationale
Scoped contract fixture only.
'@
    Write-Fixture 'scope.json' '{"changedFiles":["Assets/_Game/Scripts/Example.cs"],"allowedPaths":["Assets/_Game/Scripts/Example.cs","docs/validation/example_execution_report.md"],"reportPaths":["docs/validation/example_execution_report.md"]}'
    $diff = Invoke-Gate 'check_spec_diff_completeness.ps1' @('-ScopePath',$manifestPath)
    Assert-Contract ($diff.Exit -eq 0 -and $diff.Output.Contains('Evidence declared for existing tests') -and -not $diff.Output.Contains('no test files found')) 'existing tests evidence accepted without new test file'
    Assert-Contract ($diff.Output.Contains('not a test PASS claim')) 'declaration is not promoted into verified test pass'
    Write-Fixture 'scope.json' '{"changedFiles":["Assets/_Game/Scripts/Example.cs"],"allowedPaths":["Assets/_Game/Scripts/Example.cs","docs/validation/missing_execution_report.md"],"reportPaths":["docs/validation/missing_execution_report.md"]}'
    $diff = Invoke-Gate 'check_spec_diff_completeness.ps1' @('-ScopePath',$manifestPath)
    Assert-Contract ($diff.Exit -ne 0 -and $diff.Output.Contains('Scope report does not exist')) 'explicit missing report rejected before counting'
    Write-Fixture 'scope.json' '{"changedFiles":["Assets/_Game/Scripts/Example.cs","docs/validation/missing_execution_report.md"],"allowedPaths":["Assets/_Game/Scripts/Example.cs","docs/validation/missing_execution_report.md"]}'
    $diff = Invoke-Gate 'check_spec_diff_completeness.ps1' @('-ScopePath',$manifestPath)
    Assert-Contract ($diff.Exit -ne 0 -and $diff.Output.Contains('Runtime code changed but no execution report found')) 'nonexistent report in changedFiles does not satisfy runtime report requirement'
    Remove-Item -LiteralPath (Join-Path $fixture 'docs/validation/example_execution_report.md')
    Write-Fixture 'scope.json' '{"changedFiles":["Assets/_Game/Scripts/Example.cs","docs/validation/example_execution_report.md"],"allowedPaths":["Assets/_Game/Scripts/Example.cs","docs/validation/example_execution_report.md"]}'
    $diff = Invoke-Gate 'check_spec_diff_completeness.ps1' @('-ScopePath',$manifestPath)
    Assert-Contract ($diff.Exit -ne 0 -and $diff.Output.Contains('Runtime code changed but no execution report found')) 'deleted report does not satisfy runtime report requirement'
    Write-Output "VALIDATION_SCOPE_TESTS: PASS ($script:checks assertions; isolated fixtures)"
    exit 0
} finally {
    if ((Get-Location).Path -eq $fixture) { Pop-Location }
    $resolved = [IO.Path]::GetFullPath($fixture)
    if (-not $resolved.StartsWith($tempParent+[IO.Path]::DirectorySeparatorChar+'cindars-scope-contract-',[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup.' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
