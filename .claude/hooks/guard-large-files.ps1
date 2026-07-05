# Guard Large Files Hook (PreToolUse: Edit|Write)
# Protocolo de hook do Claude Code: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Bloqueia um unico Write que criaria um arquivo de texto muito grande (default > 1 MB), o que
# quase sempre e um acidente (dump de dados, blob gerado, log colado) e nao codigo-fonte.
# Defensivo: qualquer ambiguidade de parse/formato -> exit 0.
#
# Inspeciona apenas Write.content (write de arquivo inteiro). Edit.new_string (incremental) nao e gated.

$ErrorActionPreference = "Stop"

$maxBytes = 1MB   # ajuste aqui se um arquivo gerado legitimo precisar de mais

try {
    $raw = [Console]::In.ReadToEnd()
    $data = $raw | ConvertFrom-Json
}
catch {
    exit 0
}

if (-not $data -or -not $data.tool_input) { exit 0 }

$content = $data.tool_input.content
if ($null -eq $content) { exit 0 }

$bytes = [System.Text.Encoding]::UTF8.GetByteCount([string]$content)
if ($bytes -gt $maxBytes) {
    $kb = [math]::Round($bytes / 1KB)
    $maxKb = [math]::Round($maxBytes / 1KB)
    [Console]::Error.WriteLine("BLOCKED: Write would create a $kb KB file (limit $maxKb KB). Large blobs are usually accidental. Split into smaller files, generate via a script/asset pipeline, or raise the limit in .claude/hooks/guard-large-files.ps1 if this file is genuinely needed.")
    exit 2
}

exit 0
