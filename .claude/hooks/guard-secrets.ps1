# Guard Secrets Hook (PreToolUse: Edit|Write)
# Protocolo de hook do Claude Code: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Bloqueia escrita/edicao de conteudo que pareca um secret REAL (cloud keys, private keys,
# provider tokens, passwords hardcoded). Defensivo: qualquer ambiguidade de parse/formato -> exit 0
# (nunca bloqueia a tool por causa de uma falha do hook).
#
# Scope: apenas o conteudo NOVO sendo escrito (Write.content / Edit.new_string), entao editar um
# arquivo legado que ja contem um match nao gera ruido.
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

# Pattern -> label legivel, para que a mensagem de block seja acionavel.
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
