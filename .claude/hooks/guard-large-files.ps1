# Guard Large Files Hook (PreToolUse: Edit|Write)
# Claude Code hook protocol: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Blocks a single Write that would create a very large text file (default > 1 MB), which is
# almost always an accident (dumped data, generated blob, pasted log) rather than source.
# Defensive: any parse/shape ambiguity -> exit 0.
#
# Only inspects Write.content (a full-file write). Edit.new_string (incremental) is not gated.

$ErrorActionPreference = "Stop"

$maxBytes = 1MB   # adjust here if a legitimate generated file needs more

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
