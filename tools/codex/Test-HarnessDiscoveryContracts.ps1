#requires -Version 7.0
$ErrorActionPreference='Stop'
$checker=Join-Path $PSScriptRoot 'Test-HarnessDiscovery.ps1'
$temp=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('/','\')
$fixture=Join-Path $temp ('cindars-discovery-'+[guid]::NewGuid().ToString('N'))
$script:assertions=0
$utf8=[Text.UTF8Encoding]::new($false)
function Put([string]$Path,[string]$Text) {
    $full=Join-Path $fixture $Path
    [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($full))
    [IO.File]::WriteAllText($full,$Text,$utf8)
}
function Check([bool]$Condition,[string]$Message) {
    if (-not $Condition) { throw "DISCOVERY CONTRACT FAILED: $Message" }
    $script:assertions++
}
$skill=@'
---
name: demo
description: A bounded fixture skill.
---
# Demo
[Local](references/local.md) [External](https://example.invalid/anything)
(skill: fix — command-as-skill) (rule: safe, agent: reviewer)
```markdown
[Example missing](missing.md) (skill: nonexistent)
```
~~~markdown
[Another example](absent.md)
~~~
'@
$catalog=@'
## Skills
| Skill | When |
| `demo` | Fixture |
## Commands
| Command | When |
| `/fix` | Fixture |
## Agents
| Agent | When |
| `reviewer` | Fixture |
## Rules
[Rules](rules/RULES.md)
'@
function Reset-Fixture {
    Put 'CLAUDE.md' '[Catalog](.claude/HARNESS_INDEX.md)'
    Put '.claude/HARNESS_INDEX.md' $catalog
    Put '.claude/HARNESS_AUTHORING_STANDARD.md' '# Fixture standard'
    Put '.claude/skills/demo/SKILL.md' $skill
    Put '.claude/skills/demo/references/local.md' '# Local reference'
    Put '.claude/skills/demo/assets/templates/example.md' '[Template]({{target}}) (skill: {{id}})'
    Put '.claude/commands/fix.md' '# /fix'
    Put '.claude/agents/reviewer.md' "---`nname: reviewer`ndescription: Fixture review.`n---`n# Reviewer"
    Put '.claude/rules/RULES.md' '[Safe](safe.md)'
    Put '.claude/rules/safe.md' '# Safe rule'
    Put '.claude/hooks/guard.ps1' 'exit 0'
    Put '.claude/settings.json' '{"hooks":{"Stop":[{"hooks":[{"type":"command","command":"pwsh -File .claude/hooks/guard.ps1"}]}]}}'
    # Out-of-scope malformed files must not expand the audit into gameplay/history.
    Put 'Assets/Broken.cs' 'invalid code and [missing](missing.md)'
    Put 'docs/old.md' '(skill: missing)'
    Put '.agents/skills/generated/SKILL.md' 'not a canonical skill'
}
function Run([string[]]$Extra=@(),[string]$Root=$fixture) {
    $raw=& pwsh -NoProfile -File $checker -ProjectRoot $Root @Extra
    $exitCode=$LASTEXITCODE
    $text=$raw -join "`n"
    try { $data=$text | ConvertFrom-Json } catch { throw "Checker did not emit JSON: $text" }
    [pscustomobject]@{Exit=$exitCode;Text=$text;Data=$data}
}
function Expect-Failure([string]$Code) {
    $r=Run
    Check ($r.Exit -ne 0 -and $r.Data.result -eq 'FAIL' -and $Code -in $r.Data.findings.code) ($Code+': '+$r.Text)
}
try {
    Reset-Fixture
    $first=Run; $second=Run
    Check ($first.Exit -eq 0 -and $first.Data.result -eq 'PASS') ('valid canonical fixture: '+$first.Text)
    Check ($first.Text -ceq $second.Text) 'byte-identical JSON for identical inputs'
    Check ($first.Data.counts.skills -eq 1 -and $first.Data.counts.rules -eq 2 -and $first.Data.counts.hookScripts -eq 1 -and $first.Data.counts.settingsHooks -eq 1) 'complete canonical counts and scope boundaries'
    Check ($first.Data.counts.typedReferences -eq 4 -and $first.Data.counts.externalLinks -eq 1) 'typed references and external links inventoried without network'
    Put '.claude/HARNESS_INDEX.md' ($catalog+"`n## Skills`n| ``orphan`` | Missing |")
    Expect-Failure 'CATALOG_ORPHAN'
    Reset-Fixture
    Put '.claude/HARNESS_INDEX.md' ($catalog.Replace('| `demo` | Fixture |',''))
    Expect-Failure 'UNCATALOGUED'
    Reset-Fixture
    Put '.claude/HARNESS_INDEX.md' ($catalog+"`n## Skills`n| ``demo`` | Duplicate |")
    Expect-Failure 'CATALOG_DUPLICATE'
    Reset-Fixture
    Put '.claude/skills/demo/references/local.md' '[Broken](absent.md) (agent: absent)'
    Expect-Failure 'BROKEN_LINK'; Expect-Failure 'BROKEN_TYPED_REFERENCE'
    Put '.claude/skills/demo/references/local.md' '[reference]: missing.md'
    Expect-Failure 'BROKEN_LINK'
    Reset-Fixture
    Put '.claude/skills/demo/SKILL.md' ($skill.Replace('name: demo','name: Wrong Name'))
    Expect-Failure 'IDENTITY_MISMATCH'
    Reset-Fixture
    Put '.claude/skills/duplicate/SKILL.md' $skill
    Expect-Failure 'DUPLICATE_ID'
    Remove-Item -LiteralPath (Join-Path $fixture '.claude/skills/duplicate/SKILL.md')
    Remove-Item -LiteralPath (Join-Path $fixture '.claude/skills/duplicate')
    Reset-Fixture
    Put '.claude/skills/demo/SKILL.md' "---`nname: demo`ndescription: unterminated"
    Expect-Failure 'INVALID_FRONTMATTER'
    Reset-Fixture
    Put '.claude/skills/demo/SKILL.md' ($skill.Replace('description: A bounded fixture skill.','description: "unterminated'))
    Expect-Failure 'INVALID_FRONTMATTER'
    Reset-Fixture
    Put '.claude/skills/demo/references/local.md' '[Placeholder]({{target}})'
    Expect-Failure 'PLACEHOLDER_REFERENCE'
    Reset-Fixture
    Put '.claude/settings.json' '{"hooks":{"Stop":[{"hooks":[{"type":"command","command":"pwsh -File .claude/hooks/missing.ps1"}]}]}}'
    Expect-Failure 'BROKEN_HOOK_SCRIPT'
    Put '.claude/settings.json' '{"hooks":{"Stop":[{"hooks":[{"type":"command","command":"pwsh -File \".claude/hooks/guard.ps1\"; pwsh -File \".claude/hooks/missing.ps1\""}]}]}}'
    Expect-Failure 'BROKEN_HOOK_SCRIPT'
    Put '.claude/settings.json' '{"hooks":{"Stop":[{"hooks":[{"type":"command","command":"pwsh -File https://example.invalid/hook.ps1"}]}]}}'
    Expect-Failure 'BROKEN_HOOK_SCRIPT'
    Put '.claude/settings.json' '{"hooks":{"Stop":[{"hooks":[{"type":"command","command":"pwsh -File .claude/hooks"}]}]}}'
    Expect-Failure 'BROKEN_HOOK_SCRIPT'
    foreach ($invalid in @('{broken','{}','{"hooks":{}}','{"hooks":{"Stop":[]}}',
        '{"hooks":{"Stop":[{"hooks":[{"type":"command","command":["pwsh -File .claude/hooks/guard.ps1"]}]}]}}',
        '{"hooks":{"Stop":[{"hooks":[{"type":["command"],"command":"pwsh -File .claude/hooks/guard.ps1"}]}]}}')) {
        Put '.claude/settings.json' $invalid
        Expect-Failure 'INVALID_INPUT'
    }
    Reset-Fixture
    Put '.claude/HARNESS_INDEX.md' ''
    Expect-Failure 'INVALID_INPUT'
    Reset-Fixture
    Remove-Item -LiteralPath (Join-Path $fixture '.claude/skills/demo/SKILL.md')
    Expect-Failure 'MISSING_SKILL_ENTRY'
    Reset-Fixture
    $missing=Run -Root (Join-Path $fixture 'missing')
    Check ($missing.Exit -ne 0 -and 'INVALID_INPUT' -in $missing.Data.findings.code) 'nonexistent ProjectRoot fails with JSON'
    Put '.claude/skills/demo/references/local.md' '[Escape](../../../../outside.md)'
    Expect-Failure 'BROKEN_LINK'
    Reset-Fixture
    Put '.claude/skills/demo/references/local.md' ((1..30 | ForEach-Object { '[Missing](missing'+$_+'.md)' }) -join "`n")
    $bounded=Run
    Check ($bounded.Exit -ne 0 -and $bounded.Data.totalFindings -eq 30 -and $bounded.Data.findings.Count -eq 20 -and $bounded.Data.omittedFindings -eq 10) 'default bound preserves full count'
    $small=Run -Extra @('-MaxFindings','2')
    Check ($small.Data.findings.Count -eq 2 -and $small.Data.omittedFindings -eq 28) 'explicit bound'
    Reset-Fixture
    foreach ($unsafe in @('Assets/report.json','.claude/report.json','.agents/report.json','.claude/settings.json')) {
        $r=Run -Extra @('-OutputPath',(Join-Path $fixture $unsafe))
        Check ($r.Exit -ne 0 -and 'INVALID_OUTPUT' -in $r.Data.findings.code) ('reject protected/existing output '+$unsafe)
    }
    $report=Join-Path $fixture 'report.json'
    $r=Run -Extra @('-OutputPath',$report)
    Check ($r.Exit -eq 0 -and [IO.File]::ReadAllText($report).Replace("`r`n","`n").TrimEnd() -ceq $r.Text.TrimEnd()) 'optional output equals stdout modulo native line endings'
    $before=[IO.File]::ReadAllBytes($report)
    $r=Run -Extra @('-OutputPath',$report)
    Check ($r.Exit -ne 0 -and [Convert]::ToBase64String($before) -ceq [Convert]::ToBase64String([IO.File]::ReadAllBytes($report))) 'existing output never overwritten'
    $link=Join-Path $fixture '.claude/skills/demo/references/linked'
    try {
        $linkType=if ($IsWindows) { 'Junction' } else { 'SymbolicLink' }
        New-Item -ItemType $linkType -Path $link -Target (Join-Path $fixture 'docs') | Out-Null
        Expect-Failure 'INVALID_INPUT'
    } finally {
        if (Test-Path -LiteralPath $link) { Remove-Item -LiteralPath $link -Force }
    }
    Write-Output "HARNESS_DISCOVERY_CONTRACTS: PASS ($script:assertions assertions; isolated real checker fixtures)"
} finally {
    $full=[IO.Path]::GetFullPath($fixture)
    if (-not $full.StartsWith($temp+[IO.Path]::DirectorySeparatorChar+'cindars-discovery-',[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup.' }
    if (Test-Path -LiteralPath $full) { Remove-Item -LiteralPath $full -Recurse -Force }
}
