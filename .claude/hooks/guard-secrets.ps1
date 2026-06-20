# Guard Secrets Hook (PreToolUse: Edit|Write)
# Claude Code hook protocol: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Blocks writing/editing content that looks like a REAL secret (cloud keys, private keys,
# provider tokens, hardcoded passwords). Defensive: any parse/shape ambiguity -> exit 0
# (never block the tool because of a hook failure).
#
# Scope: only the NEW content being written (Write.content / Edit.new_string), so editing a
# legacy file that already contains a match does not produce noise.
# Rule: security-and-files (no secrets in code, configs or logs).

$ErrorActionPreference = "Stop"

try {
    $raw = [Console]::In.ReadToEnd()
    $data = $raw | ConvertFrom-Json
}
catch {
    exit 0
}

if (-not $data -or -not $data.tool_input) { exit 0 }

$content = [string]$data.tool_input.content
$newString = [string]$data.tool_input.new_string
$combined = "$content`n$newString"
if ([string]::IsNullOrWhiteSpace($combined)) { exit 0 }

# Pattern -> human label, so the block message is actionable.
$patterns = [ordered]@{
    'AKIA[0-9A-Z]{16}'                                                      = 'AWS access key id'
    'ASIA[0-9A-Z]{16}'                                                      = 'AWS temporary access key id'
    '-----BEGIN (RSA |EC |OPENSSH |DSA |PGP )?PRIVATE KEY-----'             = 'private key block'
    'xox[baprs]-[0-9A-Za-z-]{10,}'                                          = 'Slack token'
    'gh[pousr]_[0-9A-Za-z_]{20,}'                                           = 'GitHub token'
    'sk-[A-Za-z0-9]{32,}'                                                   = 'provider secret key (sk-...)'
    'AIza[0-9A-Za-z_\-]{35}'                                                = 'Google API key'
    '(?i)(password|passwd|secret|api[_-]?key|token)\s*[:=]\s*["''][^"'']{12,}["'']' = 'hardcoded credential assignment'
}

foreach ($p in $patterns.Keys) {
    if ($combined -match $p) {
        $label = $patterns[$p]
        [Console]::Error.WriteLine("BLOCKED: content looks like a real secret ($label). Rule: security-and-files. Remove the secret, use a placeholder/fictional value, or load it from an untracked config/env var instead of committing it.")
        exit 2
    }
}

exit 0
