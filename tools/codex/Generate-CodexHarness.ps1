# Generate-CodexHarness.ps1
#
# Reads .claude/ (single source of truth for the Claude Code harness) and materializes a
# Codex-native harness so OpenAI Codex CLI operates like our Claude Code setup.
#
# NEVER modifies anything under .claude/. NEVER touches *.unity/*.prefab/*.asset. Runs no git
# commands. Idempotent: safe to re-run; wipes and rebuilds only the generated output paths below.
#
# Outputs (generated, do not hand-edit):
#   .agents/skills/<name>/SKILL.md      (64 skills + 16 commands-as-skills = 80 dirs)
#   .codex/agents/<name>.toml           (10 agents)
#   .codex/rules/<name>.md              (21 rules, raw bytes copy)
#   .codex/hooks.json
#   .codex/config.toml
#   AGENTS.md                           (only the generated block between markers is refreshed)
#
# PowerShell 5.1 compatible. ASCII-only source (rule: windows_powershell_only). Reads/writes
# UTF-8 explicitly since source .claude/ content is UTF-8 (with some legacy-corrupt exceptions
# that are copied through as raw bytes rather than "fixed").
#
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File tools\codex\Generate-CodexHarness.ps1

param(
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

# --- Paths -------------------------------------------------------------------

$RepoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $RepoRoot

$ClaudeDir      = Join-Path $RepoRoot ".claude"
$SkillsSrcDir   = Join-Path $ClaudeDir "skills"
$CommandsSrcDir = Join-Path $ClaudeDir "commands"
$AgentsSrcDir   = Join-Path $ClaudeDir "agents"
$RulesSrcDir    = Join-Path $ClaudeDir "rules"
$SettingsPath   = Join-Path $ClaudeDir "settings.json"

$AgentsSkillsOut = Join-Path $RepoRoot ".agents\skills"
$CodexDir        = Join-Path $RepoRoot ".codex"
$CodexAgentsOut  = Join-Path $CodexDir "agents"
$CodexRulesOut   = Join-Path $CodexDir "rules"
$CodexHooksJson  = Join-Path $CodexDir "hooks.json"
$CodexConfigToml = Join-Path $CodexDir "config.toml"
$AgentsMdPath    = Join-Path $RepoRoot "AGENTS.md"

$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Write-Utf8NoBom {
    param([string]$Path, [string]$Content)
    [System.IO.File]::WriteAllText($Path, $Content, $Utf8NoBom)
}

function Read-Utf8 {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    return [System.Text.Encoding]::UTF8.GetString($bytes)
}

# --- 0. Reset generated output dirs (idempotent rebuild) ----------------------

Write-Host "[codex-harness] Resetting generated output directories..."

if (-not $WhatIf) {
    if (Test-Path $AgentsSkillsOut) { Remove-Item -Recurse -Force $AgentsSkillsOut }
    if (Test-Path $CodexAgentsOut)  { Remove-Item -Recurse -Force $CodexAgentsOut }
    if (Test-Path $CodexRulesOut)   { Remove-Item -Recurse -Force $CodexRulesOut }

    New-Item -ItemType Directory -Force -Path $AgentsSkillsOut | Out-Null
    New-Item -ItemType Directory -Force -Path $CodexAgentsOut  | Out-Null
    New-Item -ItemType Directory -Force -Path $CodexRulesOut   | Out-Null
}

$Report = [ordered]@{
    SkillsCopied     = @()
    SynthesizedSkillFrontmatter = @()
    CommandsCopied   = @()
    AgentsCopied     = @()
    RulesCopied      = @()
    FrontmatterFails = @()
    CopyFails        = @()
}

# --- 1. Skills: .claude/skills/<name>/SKILL.md -> .agents/skills/<name>/SKILL.md -------------

Write-Host "[codex-harness] Generating skills from .claude/skills ..."

$skillDirs = Get-ChildItem -Path $SkillsSrcDir -Directory
foreach ($dir in $skillDirs) {
    $srcFile = Join-Path $dir.FullName "SKILL.md"
    if (-not (Test-Path $srcFile)) {
        $Report.CopyFails += "skill:$($dir.Name) (no SKILL.md found)"
        continue
    }

    $destDir = Join-Path $AgentsSkillsOut $dir.Name
    $destFile = Join-Path $destDir "SKILL.md"

    $srcText = $null
    try { $srcText = Read-Utf8 $srcFile } catch { $srcText = $null }

    # Case A: unreadable or binary-corrupt source -> emit a VALID placeholder so Codex can parse it.
    if (($null -eq $srcText) -or ($srcText -match "[\x00-\x08\x0e-\x1f]")) {
        $phDesc = "Skill com origem corrompida em .claude/skills/$($dir.Name) -- recuperar versao limpa do historico git. Placeholder gerado automaticamente."
        $ph = "---`nname: $($dir.Name)`ndescription: `"$phDesc`"`n---`n`n> AVISO: o SKILL.md de origem esta corrompido no repositorio (.claude/skills/$($dir.Name)/SKILL.md). Recupere a versao limpa do historico git e re-execute este gerador.`n"
        if (-not $WhatIf) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Write-Utf8NoBom -Path $destFile -Content $ph
        }
        $Report.SkillsCopied += $dir.Name
        $Report.FrontmatterFails += "skill:$($dir.Name) (origem corrompida -> placeholder emitido)"
        continue
    }

    # Case B: clean source but no YAML frontmatter -> synthesize name + description.
    if ($srcText -notmatch '(?m)^\s*name\s*:' -or $srcText -notmatch '(?m)^\s*description\s*:') {
        $lines = $srcText -split "`r?`n"
        $firstHeading = ""; $firstPara = ""
        foreach ($l in $lines) {
            $t = $l.Trim()
            if (-not $firstHeading -and $t -match '^#') { $firstHeading = ($t -replace '^#+\s*', '') }
            if (-not $firstPara -and $t -and $t -notmatch '^#' -and $t -notmatch '^>') { $firstPara = $t }
        }
        $descBase = if ($firstPara) { $firstPara } elseif ($firstHeading) { $firstHeading } else { $dir.Name }
        if ($descBase.Length -gt 300) { $descBase = $descBase.Substring(0, 297) + "..." }
        $descSafe = $descBase -replace '\\', '\\\\' -replace '"', '\"'
        $fm = "---`nname: $($dir.Name)`ndescription: `"$descSafe`"`n---`n`n"
        if (-not $WhatIf) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Write-Utf8NoBom -Path $destFile -Content ($fm + $srcText)
        }
        $Report.SkillsCopied += $dir.Name
        $Report.SynthesizedSkillFrontmatter += $dir.Name
        continue
    }

    # Case C: has frontmatter -> copy verbatim.
    if (-not $WhatIf) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        Copy-Item -Path $srcFile -Destination $destFile -Force
    }
    $Report.SkillsCopied += $dir.Name
}

# --- 2. Commands: .claude/commands/<name>.md -> .agents/skills/<name>/SKILL.md (synthesized) --

Write-Host "[codex-harness] Generating command-skills from .claude/commands ..."

$commandFiles = Get-ChildItem -Path $CommandsSrcDir -Filter "*.md"
foreach ($cmd in $commandFiles) {
    $cmdName = [System.IO.Path]::GetFileNameWithoutExtension($cmd.Name)
    $destDir = Join-Path $AgentsSkillsOut $cmdName

    $bodyText = $null
    $firstLine = ""
    try {
        $bodyText = Read-Utf8 $cmd.FullName
        if ($bodyText -match "[\x00-\x08\x0e-\x1f]") {
            throw "binary/control characters detected"
        }
        $lines = $bodyText -split "`r?`n"
        foreach ($l in $lines) {
            $trimmed = $l.Trim()
            if ($trimmed -and $trimmed -notmatch '^#') {
                $firstLine = $trimmed
                break
            }
        }
        if (-not $firstLine -and $lines.Length -gt 0) {
            # fall back to first non-empty line even if it is a heading
            foreach ($l in $lines) {
                if ($l.Trim()) { $firstLine = $l.Trim(); break }
            }
        }
    }
    catch {
        $Report.CopyFails += "command:$cmdName (unreadable source: $_)"
        $phDesc = "Command com origem corrompida em .claude/commands/$($cmd.Name) -- recuperar versao limpa do historico git. Placeholder gerado automaticamente."
        $ph = "---`nname: $cmdName`ndescription: `"$phDesc`"`n---`n`n> AVISO: o command de origem esta corrompido no repositorio (.claude/commands/$($cmd.Name)). Recupere a versao limpa do historico git e re-execute este gerador.`n"
        if (-not $WhatIf) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Write-Utf8NoBom -Path (Join-Path $destDir "SKILL.md") -Content $ph
        }
        $Report.CommandsCopied += $cmdName
        continue
    }

    # Sanitize firstLine for a YAML double-quoted scalar (escape backslash then quote).
    $firstLineSafe = $firstLine -replace '\\', '\\\\' -replace '"', '\"'
    $description = "Comando de workflow do projeto (equivalente ao /$cmdName do Claude Code). $firstLineSafe"

    $frontmatter = "---`nname: $cmdName`ndescription: `"$description`"`n---`n`n"
    $synthesized = $frontmatter + $bodyText

    if (-not $WhatIf) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        Write-Utf8NoBom -Path (Join-Path $destDir "SKILL.md") -Content $synthesized
    }
    $Report.CommandsCopied += $cmdName
}

# --- 3. Agents: .claude/agents/<name>.md -> .codex/agents/<name>.toml ------------------------

Write-Host "[codex-harness] Generating Codex subagents from .claude/agents ..."

# Reviewer/auditor agents get higher reasoning effort and read-only sandbox.
$ReadOnlyAgents = @(
    "architecture-reviewer",
    "game-design-reviewer",
    "non-regression-auditor",
    "performance-auditor",
    "unity-validator"
)

function ConvertTo-TomlLiteralTripleQuoted {
    param([string]$Text)
    # TOML literal triple-quoted strings (''' ... ''') take content byte-for-byte: no escaping of
    # backslashes or double quotes needed (unlike triple-quoted basic "" strings, where every
    # backslash in Markdown/PowerShell snippets like .\tools\... would otherwise have to be
    # escaped). The only forbidden sequence is a literal ''' inside the body. Break it up by
    # inserting a single space in the middle so the delimiter never appears verbatim (ASCII-only,
    # visible text is changed only in the rare/never-observed case where source contains ''').
    if ($Text.Contains("'''")) {
        $Text = $Text.Replace("'''", "'' '")
    }
    return $Text
}

function ConvertTo-TomlBasicString {
    param([string]$Text)
    $escaped = $Text -replace '\\', '\\\\' -replace '"', '\"'
    return $escaped
}

$agentFiles = Get-ChildItem -Path $AgentsSrcDir -Filter "*.md"
foreach ($agentFile in $agentFiles) {
    $agentName = [System.IO.Path]::GetFileNameWithoutExtension($agentFile.Name)

    $raw = $null
    try {
        $raw = Read-Utf8 $agentFile.FullName
    }
    catch {
        $Report.CopyFails += "agent:$agentName (unreadable source: $_)"
        continue
    }

    # Split frontmatter (--- ... ---) from body.
    $fmMatch = [regex]::Match($raw, '(?s)^---\r?\n(.*?)\r?\n---\r?\n(.*)$')
    if (-not $fmMatch.Success) {
        $Report.CopyFails += "agent:$agentName (no frontmatter found)"
        continue
    }
    $frontRaw = $fmMatch.Groups[1].Value
    $body     = $fmMatch.Groups[2].Value

    $descMatch = [regex]::Match($frontRaw, '(?m)^description:\s*(.+)$')
    $description = if ($descMatch.Success) { $descMatch.Groups[1].Value.Trim() } else { "" }
    if (-not $description) {
        $Report.FrontmatterFails += "agent:$agentName (missing description)"
    }

    $isReadOnly = $ReadOnlyAgents -contains $agentName
    $reasoningEffort = if ($isReadOnly) { "high" } else { "medium" }
    $sandboxMode = if ($isReadOnly) { "read-only" } else { "workspace-write" }

    $descToml = ConvertTo-TomlBasicString $description
    $bodyToml = ConvertTo-TomlLiteralTripleQuoted $body

    $toml = @"
# Generated from .claude/agents/$agentName.md -- do not hand-edit; re-run tools/codex/Generate-CodexHarness.ps1
name = "$agentName"
description = "$descToml"
model_reasoning_effort = "$reasoningEffort"
sandbox_mode = "$sandboxMode"
developer_instructions = '''
$bodyToml
'''
"@

    if (-not $WhatIf) {
        Write-Utf8NoBom -Path (Join-Path $CodexAgentsOut "$agentName.toml") -Content $toml
    }
    $Report.AgentsCopied += $agentName
}

# --- 4. Rules: .claude/rules/<name>.md -> .codex/rules/<name>.md (verbatim raw bytes) --------

Write-Host "[codex-harness] Copying rules from .claude/rules (raw bytes, verbatim) ..."

$ruleFiles = Get-ChildItem -Path $RulesSrcDir -Filter "*.md"
foreach ($ruleFile in $ruleFiles) {
    if (-not $WhatIf) {
        Copy-Item -Path $ruleFile.FullName -Destination (Join-Path $CodexRulesOut $ruleFile.Name) -Force
    }
    $Report.RulesCopied += $ruleFile.Name
}

# --- 5. hooks.json ------------------------------------------------------------------------

Write-Host "[codex-harness] Generating .codex/hooks.json ..."

# Reuse path: all inspected .claude/hooks/*.ps1 read stdin generically via
#   [Console]::In.ReadToEnd() | ConvertFrom-Json
# and key off $data.tool_input / $data.tool_name / $data.stop_hook_active, exiting 2 to block.
# This is the same stdin JSON shape Codex sends (session_id, cwd, hook_event_name, tool_name,
# tool_input, tool_response, turn_id, permission_mode). No adapter is needed: the SAME
# .claude/hooks/*.ps1 scripts are invoked directly by .codex/hooks.json. Single source of truth
# for guard logic; zero duplication.
#
# Tool-name matcher mapping (Claude -> Codex):
#   Bash|PowerShell               -> Bash|PowerShell|shell   (shell execution)
#   Edit|Write                    -> apply_patch|Edit|Write|write_file  (file edits)

$hooksObj = [ordered]@{
    hooks = [ordered]@{
        PreToolUse = @(
            [ordered]@{
                matcher = "Bash|PowerShell|shell"
                hooks = @(
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-bash-guard.ps1"
                    }
                )
            },
            [ordered]@{
                matcher = "apply_patch|Edit|Write|write_file"
                hooks = @(
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/protected-path-guard.ps1"
                    },
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/guard-secrets.ps1"
                    },
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/guard-large-files.ps1"
                    }
                )
            }
        )
        PostToolUse = @(
            [ordered]@{
                matcher = "apply_patch|Edit|Write|write_file"
                hooks = @(
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/runtime-code-guard.ps1"
                    }
                )
            }
        )
        Stop = @(
            [ordered]@{
                hooks = @(
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/detect-change-scope.ps1"
                    },
                    [ordered]@{
                        type = "command"
                        command = "powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/stop-summary-check.ps1"
                    }
                )
            }
        )
    }
}

if (-not $WhatIf) {
    New-Item -ItemType Directory -Force -Path $CodexDir | Out-Null
    $hooksJsonText = $hooksObj | ConvertTo-Json -Depth 10
    Write-Utf8NoBom -Path $CodexHooksJson -Content $hooksJsonText
}

# --- 6. config.toml -----------------------------------------------------------------------

Write-Host "[codex-harness] Generating .codex/config.toml ..."

$configToml = @"
# Generated by tools/codex/Generate-CodexHarness.ps1 -- single source of truth is .claude/.
# Re-run the generator after any .claude/ change; do not hand-edit generated sections.

# Model defaults. Codex uses its own model family (not opus/sonnet/haiku) so no explicit
# model pin is set here by default -- override locally if desired.
[model]
# name = "gpt-5-codex"
reasoning_effort = "medium"

# Approval / sandbox policy. Claude Code enforces the unsafe-git / .unity-.prefab-.asset /
# Packages/** / ProjectSettings/** allowlist via permissions.ask (human approves per instance).
# Codex's approval model is coarser (workspace-write / read-only / danger-full-access, plus an
# approval_policy of untrusted/on-failure/on-request/never) so parity here is split:
#   - Unsafe git commands and the dotnet-build-truth-gate pattern are HOOK-ENFORCED via
#     PreToolUse -> .claude/hooks/pre-bash-guard.ps1 (see hooks.json). This blocks unconditionally;
#     it does not merely ask.
#   - Edits to *.unity / *.prefab / *.asset / Packages/** / ProjectSettings/** are NOT
#     hook-blocked (Claude asks per-instance rather than blocking outright). For Codex, the
#     closest equivalent is to run with a non-"never" approval_policy so Codex prompts before
#     writing those paths, OR to keep sandbox_mode at "workspace-write" (not full-access) so the
#     human is in the loop for anything outside the ordinary code/docs surface.
approval_policy = "on-request"
sandbox_mode = "workspace-write"

# MCP servers placeholder -- mirror any MCP servers the Claude Code harness relies on here.
# [mcp_servers.example]
# command = "npx"
# args = ["-y", "@example/mcp-server"]

# Hooks live in .codex/hooks.json (sibling file), not inline here, to keep this file short and
# to mirror .claude/settings.json's separate hooks block as closely as possible.
"@

if (-not $WhatIf) {
    Write-Utf8NoBom -Path $CodexConfigToml -Content $configToml
}

# --- 7. AGENTS.md generated block ----------------------------------------------------------

Write-Host "[codex-harness] Refreshing generated block in AGENTS.md ..."

$BeginMarker = "<!-- BEGIN CODEX-HARNESS (generated) -->"
$EndMarker   = "<!-- END CODEX-HARNESS (generated) -->"

# Parse rule invariants: prefer the line starting with **Invariante:**; else first non-heading line.
$ruleIndexLines = @()
foreach ($ruleFile in ($ruleFiles | Sort-Object Name)) {
    $ruleName = [System.IO.Path]::GetFileNameWithoutExtension($ruleFile.Name)
    $invariant = ""
    try {
        $ruleText = Read-Utf8 $ruleFile.FullName
        $invMatch = [regex]::Match($ruleText, '(?m)^\*\*Invariante:\*\*\s*(.+)$')
        if ($invMatch.Success) {
            $invariant = $invMatch.Groups[1].Value.Trim()
        }
        else {
            $lines2 = $ruleText -split "`r?`n"
            foreach ($l in $lines2) {
                $t = $l.Trim()
                if ($t -and $t -notmatch '^#' -and $t -notmatch '^>') { $invariant = $t; break }
            }
        }
    }
    catch {
        $invariant = "(source file unreadable/corrupt in .claude/rules -- copied as raw bytes; see README)"
    }
    if ($invariant.Length -gt 220) { $invariant = $invariant.Substring(0, 217) + "..." }
    $ruleIndexLines += "- ``$ruleName`` -- $invariant"
}

$skillIndexLines = @()
foreach ($dir in ($skillDirs | Sort-Object Name)) {
    $srcFile = Join-Path $dir.FullName "SKILL.md"
    $desc = ""
    if (Test-Path $srcFile) {
        try {
            $t = Read-Utf8 $srcFile
            $dm = [regex]::Match($t, '(?m)^description:\s*(.+)$')
            if ($dm.Success) { $desc = $dm.Groups[1].Value.Trim() }
        }
        catch {
            $desc = "(source corrupt in .claude/skills -- see README)"
        }
    }
    if ($desc.Length -gt 160) { $desc = $desc.Substring(0, 157) + "..." }
    $skillIndexLines += "- ``$($dir.Name)`` -- $desc"
}

$commandIndexLines = @()
foreach ($cmd in ($commandFiles | Sort-Object Name)) {
    $cmdName = [System.IO.Path]::GetFileNameWithoutExtension($cmd.Name)
    $commandIndexLines += "- ``$cmdName`` (skill; equivalent of /$cmdName)"
}

$agentIndexLines = @()
foreach ($agentFile in ($agentFiles | Sort-Object Name)) {
    $agentName = [System.IO.Path]::GetFileNameWithoutExtension($agentFile.Name)
    $desc = ""
    try {
        $t = Read-Utf8 $agentFile.FullName
        $dm = [regex]::Match($t, '(?m)^description:\s*(.+)$')
        if ($dm.Success) { $desc = $dm.Groups[1].Value.Trim() }
    }
    catch {}
    if ($desc.Length -gt 160) { $desc = $desc.Substring(0, 157) + "..." }
    $agentIndexLines += "- ``$agentName`` -- $desc"
}

$generatedBlockLines = New-Object System.Collections.Generic.List[string]
$generatedBlockLines.Add($BeginMarker)
$generatedBlockLines.Add("")
$generatedBlockLines.Add("<!-- This block is generated by tools/codex/Generate-CodexHarness.ps1 from .claude/. -->")
$generatedBlockLines.Add("<!-- Do not hand-edit; re-run the generator after any .claude/ change. -->")
$generatedBlockLines.Add("")
$generatedBlockLines.Add("## Codex Harness (generated parity block)")
$generatedBlockLines.Add("")
$generatedBlockLines.Add("### Routing")
$generatedBlockLines.Add("")
$generatedBlockLines.Add("Main loop (high reasoning effort) is reserved for thinking / debating / planning / proposing --")
$generatedBlockLines.Add("design, refinement and decisions. For execution work (build, edit, run, validate), delegate to a")
$generatedBlockLines.Add('Codex subagent defined under `.codex/agents/*.toml` (medium reasoning effort, workspace-write),')
$generatedBlockLines.Add('reserving `read-only` + high-effort subagents for the reviewer/auditor roles listed below.')
$generatedBlockLines.Add("")
$generatedBlockLines.Add('### Rules index (`.codex/rules/*.md`, copied verbatim from `.claude/rules/`)')
$generatedBlockLines.Add("")
$generatedBlockLines.AddRange([string[]]$ruleIndexLines)
$generatedBlockLines.Add("")
$generatedBlockLines.Add('### Skills index (`.agents/skills/*/SKILL.md`, copied from `.claude/skills/`)')
$generatedBlockLines.Add("")
$generatedBlockLines.AddRange([string[]]$skillIndexLines)
$generatedBlockLines.Add("")
$generatedBlockLines.Add('### Command-skills index (synthesized from `.claude/commands/`)')
$generatedBlockLines.Add("")
$generatedBlockLines.AddRange([string[]]$commandIndexLines)
$generatedBlockLines.Add("")
$generatedBlockLines.Add('### Agents index (`.codex/agents/*.toml`, converted from `.claude/agents/`)')
$generatedBlockLines.Add("")
$generatedBlockLines.AddRange([string[]]$agentIndexLines)
$generatedBlockLines.Add("")
$generatedBlockLines.Add("### Mechanical enforcement")
$generatedBlockLines.Add("")
$generatedBlockLines.Add('Hooks are wired in `.codex/hooks.json` and reuse the same `.claude/hooks/*.ps1` scripts as')
$generatedBlockLines.Add('Claude Code (single source of truth for guard logic -- see `tools/codex/README.md`).')
$generatedBlockLines.Add("")
$generatedBlockLines.Add($EndMarker)

$generatedBlock = [string]::Join("`n", $generatedBlockLines.ToArray())

$existingAgentsMd = ""
if (Test-Path $AgentsMdPath) {
    $existingAgentsMd = Read-Utf8 $AgentsMdPath
}

if ($existingAgentsMd -match [regex]::Escape($BeginMarker)) {
    $pattern = "(?s)" + [regex]::Escape($BeginMarker) + ".*?" + [regex]::Escape($EndMarker)
    $newAgentsMd = [regex]::Replace($existingAgentsMd, $pattern, [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $generatedBlock })
}
else {
    $sep = ""
    if ($existingAgentsMd -and -not $existingAgentsMd.EndsWith("`n")) { $sep = "`n" }
    $newAgentsMd = $existingAgentsMd + $sep + "`n" + $generatedBlock + "`n"
}

if (-not $WhatIf) {
    Write-Utf8NoBom -Path $AgentsMdPath -Content $newAgentsMd
}

# --- 8. Summary ----------------------------------------------------------------------------

Write-Host ""
Write-Host "[codex-harness] Done."
Write-Host "  Skills copied:       $($Report.SkillsCopied.Count)"
Write-Host "  Commands copied:     $($Report.CommandsCopied.Count)"
Write-Host "  Agents converted:    $($Report.AgentsCopied.Count)"
Write-Host "  Rules copied:        $($Report.RulesCopied.Count)"
Write-Host "  Frontmatter issues:  $($Report.FrontmatterFails.Count)"
Write-Host "  Copy failures:       $($Report.CopyFails.Count)"
if ($Report.FrontmatterFails.Count -gt 0) {
    Write-Host "  -- Frontmatter issues:"
    foreach ($f in $Report.FrontmatterFails) { Write-Host "     $f" }
}
if ($Report.CopyFails.Count -gt 0) {
    Write-Host "  -- Copy failures:"
    foreach ($f in $Report.CopyFails) { Write-Host "     $f" }
}
