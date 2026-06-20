# Runtime Code Guard Hook (PostToolUse: Edit|Write)
# Claude Code hook protocol: JSON via stdin; exit 0 = silent, exit 2 = feedback to Claude (stderr).
# PostToolUse nao consegue desfazer o edit - exit 2 aqui significa "corrija isto antes de fechar a tarefa".
#
# Verifica APENAS o conteudo recem-escrito (Edit.new_string / Write.content), entao editar um
# arquivo legado que ja contem violacoes nao gera ruido.
#
#   1. APIs de runtime scene search proibidas em Assets/_Game/Scripts (nao Editor/)
#      (rule: unity-architecture / no-runtime-global-search)
#   2. Namespaces proibidos CindarsHope.Debug / CindarsHope.Temp
#   3. Nome de classe duplicado em Write de novo arquivo (previne sistemas paralelos como
#      Craft/ vs Crafting/, StatusEffectSO / SkillActionSO duplicados)

$ErrorActionPreference = "Stop"

try {
    $raw = [Console]::In.ReadToEnd()
    $data = $raw | ConvertFrom-Json
}
catch {
    exit 0
}

if (-not $data -or -not $data.tool_input) { exit 0 }

$filePath = $data.tool_input.file_path
if (-not $filePath -or $filePath -notmatch '\.cs$') { exit 0 }

$path = $filePath -replace '\\', '/'
$root = (Get-Location).Path -replace '\\', '/'
if ($path.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
    $path = $path.Substring($root.Length).TrimStart('/')
}

$isRuntime = ($path -match '^Assets/_Game/Scripts/') -and ($path -notmatch '/Editor/')
$isWrite = ($data.tool_name -eq 'Write')

$newContent = $null
if ($isWrite) { $newContent = $data.tool_input.content }
else { $newContent = $data.tool_input.new_string }
if (-not $newContent) { exit 0 }

$problems = @()

# --- 1. APIs de runtime scene search proibidas ---------------------------------
if ($isRuntime) {
    # [(<] tambem captura a forma generica FindObjectOfType<T>()
    $searchPatterns = 'GameObject\.Find\s*\(', 'FindObjectOfType\s*[(<]', 'FindObjectsOfType\s*[(<]', 'FindObjectsByType\s*[(<]'
    foreach ($p in $searchPatterns) {
        if ($newContent -match $p) {
            $problems += "Runtime scene search API matching '$p' added to '$path'. Rule: unity-architecture / no-runtime-global-search. Use serialized refs, GameBootstrap injection, or explicit configuration. If the spec authorizes an exception, document it in the execution report."
        }
    }
}

# --- 2. Namespaces proibidos -------------------------------------------------
if ($newContent -match 'namespace\s+CindarsHope\.(Debug|Temp)\b') {
    $problems += "Forbidden namespace CindarsHope.Debug/CindarsHope.Temp added to '$path'."
}

# --- 3. Nomes de classe duplicados em novo Write ----------------------------
if ($isWrite -and $path -match '^Assets/_Game/(Scripts|Tests)/') {
    $classMatches = [regex]::Matches($newContent, '(?m)^\s*(?:public|internal)?\s*(?:sealed|abstract|static|partial)?\s*class\s+([A-Za-z_]\w+)')
    foreach ($m in $classMatches) {
        $className = $m.Groups[1].Value
        $existing = & git grep -l --untracked -E "class\s+$className\b" -- 'Assets/_Game/Scripts/*.cs' 'Assets/_Game/Tests/*.cs' 2>$null
        if ($existing) {
            $others = @($existing | Where-Object { ($_ -replace '\\', '/') -ne $path })
            if ($others.Count -gt 0) {
                $problems += "Class '$className' written to '$path' already exists in: $($others -join ', '). Check for a parallel/duplicate system (see skill: system-reuse-audit). Reuse or extend the existing type, or justify the new one in the execution report."
            }
        }
    }
}

if ($problems.Count -gt 0) {
    foreach ($problem in $problems) {
        [Console]::Error.WriteLine("RUNTIME-CODE-GUARD: $problem")
    }
    exit 2
}

exit 0
