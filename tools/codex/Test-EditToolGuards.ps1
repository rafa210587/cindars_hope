param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
. (Join-Path $repo '.claude/hooks/edit-tool-payload.ps1')
$script:assertions = 0

function Assert-GuardContract {
    param([bool]$Condition, [string]$Name)
    if (-not $Condition) { throw "Guard contract failed: $Name" }
    $script:assertions++
}

function Invoke-GuardFixture {
    param([string]$Guard, [string]$Payload, [int]$ExpectedExit)
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo.FileName = (Get-Command powershell.exe).Source
    $guardPath = Join-Path $repo ('.claude/hooks/' + $Guard)
    $process.StartInfo.Arguments = '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $guardPath + '"'
    $process.StartInfo.WorkingDirectory = $repo
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.CreateNoWindow = $true
    $process.StartInfo.RedirectStandardInput = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    try {
        [void]$process.Start()
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $process.StandardInput.Write($Payload)
        $process.StandardInput.Close()
        $process.WaitForExit()
        Assert-GuardContract ($process.ExitCode -eq $ExpectedExit) "$Guard expected exit $ExpectedExit, got $($process.ExitCode): $($stderr.Result)"
        if ($ExpectedExit -eq 2) { Assert-GuardContract (-not [string]::IsNullOrWhiteSpace($stderr.Result)) "$Guard diagnostic required" }
        return $stderr.Result
    } finally { $process.Dispose() }
}

function ConvertTo-Payload {
    param([string]$Tool, $InputData)
    return (@{ tool_name = $Tool; tool_input = $InputData } | ConvertTo-Json -Depth 10 -Compress)
}

Push-Location -LiteralPath $repo
try {
    foreach ($tool in @('Write', 'write_file', 'Edit')) {
        $inputData = @{ file_path = 'Assets/_Game/Scripts/Probe.cs' }
        if ($tool -eq 'Edit') { $inputData.new_string = '' } else { $inputData.content = '' }
        $operations = @(ConvertFrom-EditToolPayload -RawJson (ConvertTo-Payload $tool $inputData))
        Assert-GuardContract ($operations.Count -eq 1 -and $operations[0].AddedContent -eq '') "$tool empty content supported"
    }

    $patch = @'
*** Begin Patch
*** Add File: tools/one.cs
+class One {}
*** Update File: tools/two.cs
@@
-old
+new
*** Update File: tools/three.cs
*** Move to: tools/four.cs
@@
-before
+after
*** Delete File: tools/five.cs
*** End Patch
'@
    foreach ($inputData in @($patch, @{patch = $patch}, @{input = $patch}, (@{patch = $patch} | ConvertTo-Json -Compress))) {
        $operations = @(ConvertFrom-EditToolPayload -RawJson (ConvertTo-Payload 'apply_patch' $inputData))
        Assert-GuardContract ($operations.Count -eq 4) 'multi-file operations preserved'
        Assert-GuardContract ($operations[0].Operation -eq 'Add' -and $operations[0].IsFullWrite) 'add is a full write'
        Assert-GuardContract ($operations[1].AddedContent -eq 'new') 'update excludes removed content'
        Assert-GuardContract ($operations[2].Path -eq 'tools/four.cs' -and $operations[2].PreviousPath -eq 'tools/three.cs') 'move preserves both paths'
        Assert-GuardContract ($operations[3].Operation -eq 'Delete' -and $operations[3].AddedContent -eq '') 'delete has no added content'
    }
    $operations = @(ConvertFrom-EditToolPayload -RawJson (ConvertTo-Json -InputObject $patch -Compress))
    Assert-GuardContract ($operations.Count -eq 4) 'raw JSON string patch supported'
    $operations = @(ConvertFrom-EditToolPayload -RawJson (ConvertTo-Payload 'write_file' @{path='tools/../specs/probe.md';content='x'}))
    Assert-GuardContract ($operations[0].Path -eq 'specs/probe.md') 'parent path segments normalized'

    $badPatch = "*** Begin Patch`n*** Update File: tools/x.cs`nnot-a-hunk`n*** End Patch"
    $guards = @('protected-path-guard.ps1', 'runtime-code-guard.ps1', 'guard-secrets.ps1', 'guard-large-files.ps1')
    foreach ($guard in $guards) {
        [void](Invoke-GuardFixture $guard '{' 2)
        [void](Invoke-GuardFixture $guard (ConvertTo-Payload 'apply_patch' @{patch=$badPatch}) 2)
        [void](Invoke-GuardFixture $guard (ConvertTo-Payload 'unknown' @{content='x'}) 2)
        [void](Invoke-GuardFixture $guard (ConvertTo-Payload 'Write' @{file_path='tools/safe.txt';content=''}) 0)
    }

    $forbidden = 'namespace CindarsHope.Debug { class Probe {} }'
    foreach ($tool in @('Write', 'Edit', 'write_file')) {
        $inputData = @{file_path='Assets/_Game/Scripts/Probe.cs'}
        if ($tool -eq 'Edit') { $inputData.new_string=$forbidden } else { $inputData.content=$forbidden }
        [void](Invoke-GuardFixture 'runtime-code-guard.ps1' (ConvertTo-Payload $tool $inputData) 2)
    }
    $runtimePatch = "*** Begin Patch`n*** Add File: tools/safe.txt`n+safe`n*** Update File: Assets/_Game/Scripts/Probe.cs`n@@`n+$forbidden`n*** End Patch"
    [void](Invoke-GuardFixture 'runtime-code-guard.ps1' (ConvertTo-Payload 'apply_patch' $runtimePatch) 2)
    $removedRuntime = "*** Begin Patch`n*** Update File: Assets/_Game/Scripts/Probe.cs`n@@`n-$forbidden`n+namespace CindarsHope.Runtime {}`n*** End Patch"
    [void](Invoke-GuardFixture 'runtime-code-guard.ps1' (ConvertTo-Payload 'apply_patch' @{input=$removedRuntime}) 0)
    [void](Invoke-GuardFixture 'protected-path-guard.ps1' (ConvertTo-Payload 'Write' @{file_path='tools/../specs/x.md';content='x'}) 2)
    foreach ($move in @('*** Update File: tools/safe.md', '*** Update File: docs_old/unsafe.md')) {
        $moveTarget = if ($move -match 'docs_old') { 'tools/safe.md' } else { 'docs_old/unsafe.md' }
        $movePatch = "*** Begin Patch`n$move`n*** Move to: $moveTarget`n*** End Patch"
        [void](Invoke-GuardFixture 'protected-path-guard.ps1' (ConvertTo-Payload 'apply_patch' $movePatch) 2)
    }
    [void](Invoke-GuardFixture 'protected-path-guard.ps1' (ConvertTo-Payload 'apply_patch' "*** Begin Patch`n*** Delete File: docs_old/x.md`n*** End Patch") 2)

    # Marcador sintetico, sem chave nem credencial real; nao deve ser ecoado pelo guard.
    $privateMarker = '-----BEGIN ' + 'PRIVATE KEY-----'
    $secretPatch = "*** Begin Patch`n*** Add File: tools/probe.txt`n+$privateMarker`n*** End Patch"
    $diagnostic = Invoke-GuardFixture 'guard-secrets.ps1' (ConvertTo-Payload 'apply_patch' @{patch=$secretPatch}) 2
    Assert-GuardContract (-not $diagnostic.Contains($privateMarker)) 'secret content never echoed'
    $removedSecret = "*** Begin Patch`n*** Update File: tools/probe.txt`n@@`n-$privateMarker`n+safe`n*** End Patch"
    [void](Invoke-GuardFixture 'guard-secrets.ps1' (ConvertTo-Payload 'apply_patch' $removedSecret) 0)
    [void](Invoke-GuardFixture 'guard-large-files.ps1' (ConvertTo-Payload 'Write' @{file_path='tools/x.txt';content=('x' * 1MB)}) 0)
    $largePatch = "*** Begin Patch`n*** Add File: tools/x.txt`n+" + ('x' * (1MB + 1)) + "`n*** End Patch"
    [void](Invoke-GuardFixture 'guard-large-files.ps1' (ConvertTo-Payload 'apply_patch' $largePatch) 2)
    Write-Output "EDIT_TOOL_GUARDS_TESTS: PASS ($script:assertions assertions; explicit process invocations)"
    exit 0
} finally { Pop-Location }
