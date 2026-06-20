# Protected Path Guard Hook (PreToolUse: Edit|Write)
# Protocolo de hook do Claude Code: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Bloqueia edits/writes que NUNCA sao aceitaveis:
#   1. docs_old/** (archive read-only; rule: docs-governance)
#   2. Pastas legadas numeradas de docs docs/00_PROJECT..07_RELEASES (rule: docs-governance)
#   3. Pastas root specs/ ou spec/ (rule: spec-lifecycle / spec source of truth)
#   4. *Tests.cs sob Assets/_Game/Scripts/** (tests pertencem a Assets/_Game/Tests/EditMode/**)
#
# .unity/.prefab/.asset e Packages/ProjectSettings sao tratados por permissions.ask
# em settings.json (humano autoriza por instancia) -- nao sao bloqueados aqui.

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

# Normaliza para forward slashes e remove o root do projeto se for absoluto
$path = $filePath -replace '\\', '/'
$root = (Get-Location).Path -replace '\\', '/'
if ($path.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
    $path = $path.Substring($root.Length).TrimStart('/')
}

# --- 1. docs_old e read-only -----------------------------------------------
if ($path -match '(^|/)docs_old/') {
    [Console]::Error.WriteLine("BLOCKED: '$path' is under docs_old/ (read-only historical archive). Rule: docs-governance / legacy-doc-paths-forbidden.")
    exit 2
}

# --- 2. Pastas legadas numeradas de docs ------------------------------------
if ($path -match '(^|/)docs/0[0-7]_(PROJECT|PRODUCT|ARCHITECTURE|SPECS|REFINEMENTS|VALIDATION|BACKLOG|RELEASES)/') {
    [Console]::Error.WriteLine("BLOCKED: '$path' uses a legacy numbered docs folder. Use canonical paths (docs/project/, .specs/, docs/validation/, ...). Rule: docs-governance / legacy-doc-paths-forbidden.")
    exit 2
}

# --- 3. Root specs/ ou spec/ ------------------------------------------------
if ($path -match '^(specs|spec)/') {
    [Console]::Error.WriteLine("BLOCKED: '$path' recreates a root specs/ folder. The only active spec source is .specs/. Rule: spec-lifecycle / spec-source-of-truth.")
    exit 2
}

# --- 4. Tests devem ficar em Assets/_Game/Tests/EditMode/ --------------------
if ($path -match '^Assets/_Game/Scripts/.*Tests\.cs$') {
    [Console]::Error.WriteLine("BLOCKED: test file '$path' inside Assets/_Game/Scripts/. Tests must live under Assets/_Game/Tests/EditMode/<Domain>/ (rule: spec-lifecycle; skill: editmode-test-authoring).")
    exit 2
}

exit 0
