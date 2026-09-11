# Generate-CodexHarness.ps1
#
# Reads .claude/ (single source of truth for the Claude Code harness) and materializes a
# Codex-native harness so OpenAI Codex CLI operates like our Claude Code setup.
#
# NEVER modifies anything under .claude/. NEVER touches *.unity/*.prefab/*.asset. Runs no git
# commands. Idempotent: safe to re-run; updates generated files without deleting user-owned files.
#
# Outputs (generated, do not hand-edit):
#   .agents/skills/<name>/SKILL.md      (skills and commands-as-skills)
#   .codex/agents/<name>.toml           (agents)
#   .codex/rules/<name>.md              (rules, raw bytes copy)
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
    if (Test-Path -LiteralPath $Path) {
        $existing = [IO.File]::ReadAllBytes($Path)
        if ([Convert]::ToBase64String($existing) -eq [Convert]::ToBase64String($Utf8NoBom.GetBytes($Content))) { return }
    }
    [System.IO.File]::WriteAllText($Path, $Content, $Utf8NoBom)
}

function Copy-SourceFile {
    param([string]$Source, [string]$Destination)
    # Avoid rewriting identical files, which the host may currently memory-map.
    if (Test-Path -LiteralPath $Destination) {
        if ([Convert]::ToBase64String([IO.File]::ReadAllBytes($Source)) -eq [Convert]::ToBase64String([IO.File]::ReadAllBytes($Destination))) { return }
    }
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
}
function Read-Utf8 {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    return [System.Text.Encoding]::UTF8.GetString($bytes)
}

# Validar a origem antes de remover qualquer saida gerada. Preservar eventos/opcoes/ordem.
$settings = (Read-Utf8 $SettingsPath) | ConvertFrom-Json -ErrorAction Stop
if ($null -eq $settings.hooks -or $settings.hooks -isnot [pscustomobject]) {
    throw 'settings.json must contain a hooks object.'
}
foreach ($eventProperty in $settings.hooks.PSObject.Properties) {
    foreach ($route in @($eventProperty.Value)) {
        if ($null -eq $route -or $route -isnot [pscustomobject] -or $null -eq $route.hooks) {
            throw 'Invalid hook route in settings.json.'
        }
        foreach ($hook in @($route.hooks)) {
            if ($null -eq $hook -or $hook -isnot [pscustomobject] -or -not $hook.type) {
                throw 'Invalid hook definition in settings.json.'
            }
        }
        if ($route.PSObject.Properties.Name -contains 'matcher') {
            if ($route.matcher -isnot [string]) { throw 'Hook matcher must be a string.' }
            # Traduzir alternativas de nomes conhecidas; regex/custom matchers ficam intactos.
            $names = [System.Collections.Generic.List[string]]::new()
            foreach ($name in ($route.matcher -split '\|')) { $names.Add($name) }
            if ($names.Contains('Edit') -or $names.Contains('Write')) {
                foreach ($name in @('apply_patch', 'write_file')) {
                    if (-not $names.Contains($name)) { $names.Add($name) }
                }
            }
            if ($names.Contains('Bash') -or $names.Contains('PowerShell')) {
                foreach ($name in @('shell', 'exec_command')) {
                    if (-not $names.Contains($name)) { $names.Add($name) }
                }
            }
            $route.matcher = [string]::Join('|', $names)
        }
    }
}
$hooksObj = [ordered]@{ hooks = $settings.hooks }

$catalogPath = Join-Path $ClaudeDir 'HARNESS_INDEX.md'
if (-not (Test-Path -LiteralPath $catalogPath)) { throw 'Missing .claude/HARNESS_INDEX.md canonical catalog.' }
$catalog = Read-Utf8 $catalogPath
foreach ($folder in @($SkillsSrcDir, $CommandsSrcDir, $AgentsSrcDir)) {
    foreach ($entry in (Get-ChildItem -LiteralPath $folder)) {
        $id = if ($entry.PSIsContainer) { $entry.Name } else { [IO.Path]::GetFileNameWithoutExtension($entry.Name) }
        if ($catalog -notmatch ('`/?' + [regex]::Escape($id) + '`')) { throw "Catalog missing ID: $id" }
    }
}
# Never follow links from sources or destination trees during recursive copy.
foreach ($folder in @($SkillsSrcDir, $AgentsSkillsOut, $CodexAgentsOut, $CodexRulesOut)) {
    if (Test-Path -LiteralPath $folder) {
        foreach ($entry in (Get-ChildItem -LiteralPath $folder -Recurse -Force)) {
            if ($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Reparse point not allowed: $($entry.FullName)" }
        }
    }
}
# --- 0. Prepare generated output dirs (non-destructive) ----------------------

Write-Host "[codex-harness] Preparing generated output directories..."

if (-not $WhatIf) {
    $rootPrefix = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    foreach ($target in @($AgentsSkillsOut, $CodexAgentsOut, $CodexRulesOut)) {
        $resolvedTarget = [System.IO.Path]::GetFullPath($target)
        if (-not $resolvedTarget.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Generated output target must remain inside the repository.'
        }
        $ancestor = $resolvedTarget
        while ($ancestor.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            if (Test-Path -LiteralPath $ancestor) {
                $item = Get-Item -LiteralPath $ancestor -Force
                if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                    throw 'Generated output target must not traverse a reparse point.'
                }
            }
            $ancestor = Split-Path -Parent $ancestor
        }
    }
    foreach ($target in @($AgentsSkillsOut, $CodexAgentsOut, $CodexRulesOut)) {
        # Preserve extra local files; refresh only source-owned destinations below.
    }

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

    if (-not $WhatIf) {
        foreach ($support in (Get-ChildItem -LiteralPath $dir.FullName -Recurse -File | Where-Object { $_.FullName -ne $srcFile })) {
            $relative = $support.FullName.Substring($dir.FullName.Length).TrimStart('\', '/')
            $supportDest = Join-Path $destDir $relative
            [void][IO.Directory]::CreateDirectory((Split-Path -Parent $supportDest))
            Copy-SourceFile -Source $support.FullName -Destination $supportDest
        }
    }
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
        Copy-SourceFile -Source $srcFile -Destination $destFile
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
    # The host may truncate catalog descriptions. Put the functional trigger first instead of
    # spending the visible prefix on Claude/Codex compatibility information.
    $description = if ($firstLineSafe) { $firstLineSafe } else { "Workflow command: $cmdName." }

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
    "pixel-art-scene-reviewer"
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
        Copy-SourceFile -Source $ruleFile.FullName -Destination (Join-Path $CodexRulesOut $ruleFile.Name)
    }
    $Report.RulesCopied += $ruleFile.Name
}

# --- 5. hooks.json ------------------------------------------------------------------------

Write-Host "[codex-harness] Generating .codex/hooks.json ..."

# $hooksObj vem de settings.json validado antes do reset. Guards de edicao compartilham
# edit-tool-payload.ps1; equivalencia de matcher nao implica equivalencia de payload.

if (-not $WhatIf) {
    New-Item -ItemType Directory -Force -Path $CodexDir | Out-Null
    $hooksJsonText = $hooksObj | ConvertTo-Json -Depth 10
    Write-Utf8NoBom -Path $CodexHooksJson -Content $hooksJsonText
}

# --- 6. config.toml -----------------------------------------------------------------------

Write-Host "[codex-harness] Generating .codex/config.toml ..."

$configToml = @"
# Initial scaffold from tools/codex/Generate-CodexHarness.ps1.
# Local preferences: future generations preserve this file byte-for-byte.

# Model defaults. Codex uses its own model family (not opus/sonnet/haiku) so no explicit
# model pin is set here by default -- override locally if desired.
# model = "your-configured-model"
model_reasoning_effort = "medium"

# Approval / sandbox preferences for a new local config only.
# Protected edit paths and unsafe Git have configured PreToolUse guards. Enforcement depends
# on the host loading .codex/hooks.json and supplying supported payloads; workspace-write
# alone does not restrict particular file extensions. Existing local config is preserved.
approval_policy = "on-request"
sandbox_mode = "workspace-write"

# MCP servers placeholder -- mirror any MCP servers the Claude Code harness relies on here.
# [mcp_servers.example]
# command = "npx"
# args = ["-y", "@example/mcp-server"]

# Hooks live in .codex/hooks.json (sibling file), not inline here, to keep this file short and
# to mirror .claude/settings.json's separate hooks block as closely as possible.
"@

if (-not $WhatIf -and -not (Test-Path -LiteralPath $CodexConfigToml)) {
    Write-Utf8NoBom -Path $CodexConfigToml -Content $configToml
}

# --- 7. AGENTS.md generated block ----------------------------------------------------------

Write-Host "[codex-harness] Refreshing generated block in AGENTS.md ..."

$BeginMarker = "<!-- BEGIN CODEX-HARNESS (generated) -->"
$EndMarker   = "<!-- END CODEX-HARNESS (generated) -->"

$generatedBlockLines = @(
    $BeginMarker,
    '',
    '<!-- Generated by tools/codex/Generate-CodexHarness.ps1; do not hand-edit. -->',
    '## Codex Harness',
    '',
    'Leia CLAUDE.md para roteamento proporcional e .claude/HARNESS_INDEX.md para descoberta sob demanda.',
    'As invariantes acima continuam vinculantes. Leia apenas skills/rules/referencias aplicaveis ao escopo.',
    'Skills e commands: .agents/skills/<id>/SKILL.md; agentes: .codex/agents/<id>.toml;',
    'rules: .codex/rules/. Fontes canonicas: .claude/. Referencias das skills sao copiadas recursivamente.',
    'Tarefa pequena e seu teste podem ficar com o implementador. Delegue fatias independentes ou por risco.',
    'Revisores sao read-only; unity-validator escreve apenas evidencia e executa runners, sem corrigir runtime.',
    'Modelos seguem a configuracao do provedor/usuario; reasoning effort nao equivale a modelo barato.',
    'Hooks .codex/hooks.json reutilizam .claude/hooks/. Consulte tools/codex/README.md quando necessario.',
    '',
    $EndMarker
)
$generatedBlock = [string]::Join("`n", $generatedBlockLines)

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

if ($Report.FrontmatterFails.Count -gt 0 -or $Report.CopyFails.Count -gt 0) { exit 1 }
