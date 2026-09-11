param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tempParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd('\', '/')
$fixtureRoot = Join-Path $tempParent ('cindars-harness-test-' + [guid]::NewGuid().ToString('N'))
$utf8 = [System.Text.UTF8Encoding]::new($false)
$script:assertions = 0

function Assert-HarnessContract {
    param([bool]$Condition, [string]$Name)
    if (-not $Condition) { throw "Harness contract failed: $Name" }
    $script:assertions++
}
function Write-FixtureFile {
    param([string]$RelativePath, [string]$Content)
    $path = Join-Path $fixtureRoot $RelativePath
    [void][System.IO.Directory]::CreateDirectory((Split-Path -Parent $path))
    [System.IO.File]::WriteAllText($path, $Content, $utf8)
}
function Invoke-GeneratorFixture {
    param([switch]$WhatIf)
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo.FileName = (Get-Command powershell.exe).Source
    $process.StartInfo.Arguments = '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + (Join-Path $fixtureRoot 'tools/codex/Generate-CodexHarness.ps1') + '"'
    if ($WhatIf) { $process.StartInfo.Arguments += ' -WhatIf' }
    $process.StartInfo.WorkingDirectory = $fixtureRoot
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
function Get-GeneratedSnapshot {
    $paths = @((Join-Path $fixtureRoot 'AGENTS.md'))
    foreach ($folder in @('.agents', '.codex')) {
        $paths += @(Get-ChildItem -LiteralPath (Join-Path $fixtureRoot $folder) -Recurse -File | Select-Object -ExpandProperty FullName)
    }
    return (($paths | Sort-Object | ForEach-Object {
        $_.Substring($fixtureRoot.Length) + ':' + (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash
    }) -join "`n")
}

try {
    [void][System.IO.Directory]::CreateDirectory($fixtureRoot)
    Write-FixtureFile 'tools/codex/Generate-CodexHarness.ps1' ([System.IO.File]::ReadAllText((Join-Path $repo 'tools/codex/Generate-CodexHarness.ps1')))
    Write-FixtureFile '.claude/skills/probe/SKILL.md' "---`nname: probe`ndescription: Fixture skill.`n---`n# Probe`n"
    Write-FixtureFile '.claude/commands/probe-command.md' "# /probe-command`nFixture command.`n"
    Write-FixtureFile '.claude/rules/probe.md' "# Rule: Fixture`n**Invariante:** Preserve fixture.`n"
    Write-FixtureFile '.claude/agents/architecture-reviewer.md' "---`nname: architecture-reviewer`ndescription: Fixture reviewer.`ntools: Read`n---`n# Reviewer`nAudit only.`n"
    Write-FixtureFile 'AGENTS.md' "Hand-written instructions remain.`n"
    Write-FixtureFile '.claude/HARNESS_INDEX.md' '| `probe` | fixture | /`probe-command` | `architecture-reviewer` | `unity-validator` |'
    Write-FixtureFile '.claude/skills/probe/references/nested/check.md' 'Nested reference bytes.'
    Write-FixtureFile '.claude/agents/unity-validator.md' "---`nname: unity-validator`ndescription: Evidence only.`n---`nOnly evidence writes.`n"
    $settings = Get-Content -LiteralPath (Join-Path $repo '.claude/settings.json') -Raw | ConvertFrom-Json
    $settings.hooks.PreToolUse += [pscustomobject]@{
        matcher='CustomTool'; note='route option retained'; hooks=@([pscustomobject]@{type='command'; command='fixture-command'; timeout=17})
    }
    $settings.hooks | Add-Member -NotePropertyName 'CustomEvent' -NotePropertyValue @([pscustomobject]@{
        hooks=@([pscustomobject]@{type='command'; command='fixture-event'; async=$true})
    })
    $fixtureSettingsJson = $settings | ConvertTo-Json -Depth 20
    Write-FixtureFile '.claude/settings.json' $fixtureSettingsJson

    $result = Invoke-GeneratorFixture -WhatIf
    Assert-HarnessContract ($result.ExitCode -eq 0) 'WhatIf succeeds'
    Assert-HarnessContract (-not (Test-Path -LiteralPath (Join-Path $fixtureRoot '.codex'))) 'WhatIf does not generate outputs'
    Assert-HarnessContract ([System.IO.File]::ReadAllText((Join-Path $fixtureRoot 'AGENTS.md')) -eq "Hand-written instructions remain.`n") 'WhatIf preserves AGENTS'
    $result = Invoke-GeneratorFixture
    Assert-HarnessContract ($result.ExitCode -eq 0) ("generation succeeds: " + $result.Output)
    $generated = Get-Content -LiteralPath (Join-Path $fixtureRoot '.codex/hooks.json') -Raw | ConvertFrom-Json
    Assert-HarnessContract ($generated.hooks.PreToolUse[0].matcher -eq 'Bash|PowerShell|shell|exec_command') 'shell matcher translated'
    Assert-HarnessContract ($generated.hooks.PreToolUse[1].matcher -eq 'Edit|Write|apply_patch|write_file') 'edit matcher translated'
    foreach ($eventProperty in $settings.hooks.PSObject.Properties) {
        $generatedRoutes = @($generated.hooks.($eventProperty.Name))
        $fixtureRoutes = @($eventProperty.Value)
        Assert-HarnessContract ($generatedRoutes.Count -eq $fixtureRoutes.Count) ('event route count: ' + $eventProperty.Name)
        for ($index=0; $index -lt $fixtureRoutes.Count; $index++) {
            $expected = $fixtureRoutes[$index].hooks | ConvertTo-Json -Depth 20 -Compress
            $actual = $generatedRoutes[$index].hooks | ConvertTo-Json -Depth 20 -Compress
            Assert-HarnessContract ($expected -eq $actual) ('hook order/options: ' + $eventProperty.Name)
        }
    }
    Assert-HarnessContract ($generated.hooks.PreToolUse[2].matcher -eq 'CustomTool' -and $generated.hooks.PreToolUse[2].note -eq 'route option retained') 'custom matcher and route properties retained'
    Assert-HarnessContract ((($generated.hooks.Stop[0].hooks.command) -join "`n") -match 'sync-harness-and-tracing.ps1') 'Stop sync retained'
    Assert-HarnessContract ([System.IO.File]::ReadAllText((Join-Path $fixtureRoot 'AGENTS.md')).StartsWith('Hand-written instructions remain.')) 'hand-written AGENTS preserved'
    Assert-HarnessContract ([System.IO.File]::ReadAllText((Join-Path $fixtureRoot '.codex/agents/architecture-reviewer.toml')).Contains('sandbox_mode = "read-only"')) 'reviewer routing preserved'
    $configPath = Join-Path $fixtureRoot '.codex/config.toml'
    $config = [System.IO.File]::ReadAllText($configPath)
    Assert-HarnessContract ($config -match '(?m)^model_reasoning_effort = "medium"' -and $config -notmatch '(?m)^\[model\]') 'new config uses root-level preferences'
    $customConfig = "# User preferences`r`nmodel = 'fixture-model'`r`nmodel_reasoning_effort = 'high'`r`napproval_policy = 'never'`r`n"
    [System.IO.File]::WriteAllText($configPath, $customConfig, [System.Text.UTF8Encoding]::new($true))
    $customConfigHash = (Get-FileHash -LiteralPath $configPath).Hash
    Assert-HarnessContract ([IO.File]::ReadAllText((Join-Path $fixtureRoot '.agents/skills/probe/references/nested/check.md')) -eq 'Nested reference bytes.') 'transitive reference copied'
    $generatedCommand = [IO.File]::ReadAllText((Join-Path $fixtureRoot '.agents/skills/probe-command/SKILL.md'))
    Assert-HarnessContract ($generatedCommand -match '(?m)^description: "Fixture command\."$') 'command description starts with functional trigger'
    Assert-HarnessContract ($generatedCommand -notmatch 'equivalente ao') 'command description omits repeated compatibility prefix'
    Assert-HarnessContract ([IO.File]::ReadAllText((Join-Path $fixtureRoot '.codex/agents/unity-validator.toml')).Contains('sandbox_mode = "workspace-write"')) 'validator can write evidence'
    Assert-HarnessContract ([IO.File]::ReadAllText((Join-Path $fixtureRoot 'AGENTS.md')).Contains('.claude/HARNESS_INDEX.md')) 'compact block discovers catalog'
    Write-FixtureFile '.agents/skills/local-user/note.md' 'User file survives.'
    $referencePath = Join-Path $fixtureRoot '.agents/skills/probe/references/nested/check.md'
    $referenceWriteTime = (Get-Item -LiteralPath $referencePath).LastWriteTimeUtc
    $firstSnapshot = Get-GeneratedSnapshot
    $result = Invoke-GeneratorFixture
    Assert-HarnessContract ($result.ExitCode -eq 0 -and $firstSnapshot -eq (Get-GeneratedSnapshot)) 'second generation idempotent'
    Assert-HarnessContract ((Get-Item -LiteralPath $referencePath).LastWriteTimeUtc -eq $referenceWriteTime) 'identical reference is not rewritten'
    Assert-HarnessContract ((Get-FileHash -LiteralPath $configPath).Hash -eq $customConfigHash) 'existing user config preserved byte-for-byte including BOM/CRLF'
    Assert-HarnessContract ([System.IO.File]::ReadAllText((Join-Path $fixtureRoot '.claude/settings.json')) -eq $fixtureSettingsJson) 'source settings unchanged'

    Assert-HarnessContract ([IO.File]::ReadAllText((Join-Path $fixtureRoot '.agents/skills/local-user/note.md')) -eq 'User file survives.') 'generation preserves user extras'
    Write-FixtureFile '.claude/HARNESS_INDEX.md' 'Empty catalog'
    $result = Invoke-GeneratorFixture
    Assert-HarnessContract ($result.ExitCode -ne 0 -and $firstSnapshot -eq (Get-GeneratedSnapshot)) 'missing catalog entry fails before modifying outputs'
    foreach ($badSettings in @('{', '{"hooks":null}', '{"hooks":{"Stop":[{}]}}')) {
        Write-FixtureFile '.claude/settings.json' $badSettings
        $result = Invoke-GeneratorFixture
        Assert-HarnessContract ($result.ExitCode -ne 0) 'invalid settings fail'
        Assert-HarnessContract ($firstSnapshot -eq (Get-GeneratedSnapshot)) 'invalid settings preserve generated output'
    }
    Write-Output "CODEX_HARNESS_TESTS: PASS ($script:assertions assertions; isolated temporary repository)"
    exit 0
} finally {
    $resolvedFixture = [System.IO.Path]::GetFullPath($fixtureRoot)
    $expectedPrefix = $tempParent + [System.IO.Path]::DirectorySeparatorChar + 'cindars-harness-test-'
    if (-not $resolvedFixture.StartsWith($expectedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup target.' }
    if (Test-Path -LiteralPath $resolvedFixture) { Remove-Item -LiteralPath $resolvedFixture -Recurse -Force }
}
