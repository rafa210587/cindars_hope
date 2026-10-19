# Guard Secrets Hook (PreToolUse: Edit|Write)
# Protocolo de hook do Claude Code: JSON via stdin; exit 0 = allow, exit 2 = block (stderr -> Claude).
#
# Bloqueia escrita/edicao de conteudo que pareca um secret REAL (cloud keys, private keys,
# provider tokens, passwords hardcoded). Envelope invalido -> diagnostico + exit 2.
#
# Scope: apenas o conteudo NOVO em Write/Edit/apply_patch, entao editar um
# arquivo legado que ja contem um match nao gera ruido.
# Rule: security-and-files (no secrets in code, configs or logs).

$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot 'edit-tool-payload.ps1')
$operations = @(Read-EditToolOperations)

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

foreach ($operation in $operations) {
    foreach ($p in $patterns.Keys) {
        if ($operation.AddedContent -match $p) {
            $label = $patterns[$p]
            [Console]::Error.WriteLine("BLOCKED: content looks like a real secret ($label). Rule: security-and-files. Remove the secret, use a placeholder/fictional value, or load it from an untracked config/env var instead of committing it.")
            exit 2
        }
    }
}

exit 0
