# Protected Path Guard Hook (PreToolUse: Edit|Write)
# Claude Code hook protocol: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Blocks edits/writes that are NEVER acceptable:
#   1. docs_old/** (read-only archive; rule: docs-governance)
#   2. Legacy numbered docs folders docs/00_PROJECT..07_RELEASES (rule: docs-governance)
#   3. Root specs/ or spec/ folders (rule: spec-lifecycle / spec source of truth)
#   4. *Tests.cs under Assets/_Game/Scripts/** (tests belong in Assets/_Game/Tests/EditMode/**)
#
# .unity/.prefab/.asset and Packages/ProjectSettings are handled by permissions.ask
# in settings.json (human authorizes per instance) — not blocked here.

$ErrorActionPreference = "Stop"

try {
    $raw = [Console]::In.ReadToEnd()
    $data = $raw | ConvertFrom-Json
}
catch {
    exit 0
}

$filePath = $null
if ($data -and $data.tool_input) { $filePath = $data.tool_input.file_path }
if (-not $filePath) { exit 0 }

# Normalize to forward slashes and strip the project root if absolute
$path = $filePath -replace '\\', '/'
$root = (Get-Location).Path -replace '\\', '/'
if ($path.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
    $path = $path.Substring($root.Length).TrimStart('/')
}

# --- 1. docs_old is read-only ----------------------------------------------
if ($path -match '(^|/)docs_old/') {
    [Console]::Error.WriteLine("BLOCKED: '$path' is under docs_old/ (read-only historical archive). Rule: docs-governance / legacy-doc-paths-forbidden.")
    exit 2
}

# --- 2. Legacy numbered docs folders ----------------------------------------
if ($path -match '(^|/)docs/0[0-7]_(PROJECT|PRODUCT|ARCHITECTURE|SPECS|REFINEMENTS|VALIDATION|BACKLOG|RELEASES)/') {
    [Console]::Error.WriteLine("BLOCKED: '$path' uses a legacy numbered docs folder. Use canonical paths (docs/project/, .specs/, docs/validation/, ...). Rule: docs-governance / legacy-doc-paths-forbidden.")
    exit 2
}

# --- 3. Root specs/ or spec/ ------------------------------------------------
if ($path -match '^(specs|spec)/') {
    [Console]::Error.WriteLine("BLOCKED: '$path' recreates a root specs/ folder. The only active spec source is .specs/. Rule: spec-lifecycle / spec-source-of-truth.")
    exit 2
}

# --- 4. Tests must live in Assets/_Game/Tests/EditMode/ ----------------------
if ($path -match '^Assets/_Game/Scripts/.*Tests\.cs$') {
    [Console]::Error.WriteLine("BLOCKED: test file '$path' inside Assets/_Game/Scripts/. Tests must live under Assets/_Game/Tests/EditMode/<Domain>/ (rule: spec-lifecycle; skill: editmode-test-authoring).")
    exit 2
}

exit 0
