<#
.SYNOPSIS
  Gera um indice compacto de todas as specs de .specs/ para reduzir custo de
  descoberta (token) de agentes: 1 leitura pequena em vez de glob + abrir N specs.

.DESCRIPTION
  Varre .specs/**/*.md, deriva o bucket de status pela pasta, e extrai campos do
  blockquote de cabecalho (Status/Wave/Priority/Type/Domain) tolerando os dois
  formatos existentes ('> **Status:** X' e '> Status: X'). Emite:
    - .specs/SPEC_INDEX.md   (tabela densa, legivel)
    - .specs/SPEC_INDEX.json (machine-readable)

.NOTES
  Idempotente, somente leitura sobre as specs. Nao usa Play Mode nem Unity.
  Rode da raiz do repo:  pwsh -File tools/generate_spec_index.ps1

  Saida byte-identica em Windows PowerShell 5.1 e PowerShell 7. Nao usar
  ConvertTo-Json nem Set-Content -Encoding utf8 aqui: o 5.1 escapa "&" e "'"
  como sequencias unicode e grava BOM; o 7 nao. Como o hook
  sync-harness-and-tracing chama 'powershell', isso gerava churn de milhares
  de linhas a cada Stop, alternando conforme o shell que rodou por ultimo.
#>

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$specsRoot = Join-Path $repo '.specs'

if (-not (Test-Path $specsRoot)) { throw "Pasta .specs nao encontrada em $specsRoot" }

function Get-Bucket([string]$relPath) {
    if ($relPath -match 'implementados[\\/]')                 { return 'Implementado' }
    if ($relPath -match 'executadas_build_validated[\\/]')    { return 'Build-Validated' }
    if ($relPath -match 'closeout_mvp[\\/]')                  { return 'Closeout-MVP' }
    if ($relPath -match 'a_implementar[\\/]fable[\\/]')       { return 'Fila-FABLE' }
    if ($relPath -match 'a_implementar[\\/]')                 { return 'Fila' }
    if ($relPath -match 'automaticas[\\/]')                   { return 'Automatica' }
    if ($relPath -match '_templates[\\/]')                    { return 'Template' }
    return 'Outro'
}

function Get-Field([string]$text, [string]$name) {
    # Casa '> **Name:** valor' (negrito envolve "Name:") ou '> Name: valor'
    $rx = "(?m)^>\s*\*{0,2}\s*$([regex]::Escape($name))\s*:\s*\*{0,2}\s*(.+?)\s*$"
    $m = [regex]::Match($text, $rx)
    if ($m.Success) {
        return ($m.Groups[1].Value -replace '`','' -replace '\*+\s*$','' -replace '^\s*\*+','').Trim()
    }
    return ''
}

function ConvertTo-JsonStringLiteral([string]$value) {
    # Escapa apenas o minimo exigido por RFC 8259. Deliberadamente NAO escapa
    # caracteres nao-ASCII nem "&"/"'": o arquivo e UTF-8 e o acento/simbolo
    # literal e valido, alem de manter o diff legivel.
    $sb = [System.Text.StringBuilder]::new()
    foreach ($ch in $value.ToCharArray()) {
        switch ($ch) {
            '"'      { [void]$sb.Append('\"');  continue }
            '\'      { [void]$sb.Append('\\');  continue }
            "`b"     { [void]$sb.Append('\b');  continue }
            "`f"     { [void]$sb.Append('\f');  continue }
            "`n"     { [void]$sb.Append('\n');  continue }
            "`r"     { [void]$sb.Append('\r');  continue }
            "`t"     { [void]$sb.Append('\t');  continue }
            default  {
                if ([int]$ch -lt 0x20) { [void]$sb.Append('\u{0:x4}' -f [int]$ch) }
                else                   { [void]$sb.Append($ch) }
            }
        }
    }
    return $sb.ToString()
}

function Write-Utf8NoBom([string]$path, [string]$text) {
    # Set-Content -Encoding utf8 grava BOM no 5.1 e sem BOM no 7; fixamos sem BOM.
    [System.IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding $false))
}

$rows = @()
$files = Get-ChildItem -Path $specsRoot -Recurse -Filter *.md -File |
         Where-Object { $_.Name -notmatch '^(README|EXAMPLE|SPEC_.*TEMPLATE|SPEC_EXECUTION_REPORT).*' }

foreach ($f in $files) {
    $rel = $f.FullName.Substring($repo.Length + 1)
    $text = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8

    $titleMatch = [regex]::Match($text, '(?m)^#\s+(.+?)\s*$')
    $title = if ($titleMatch.Success) { $titleMatch.Groups[1].Value.Trim() } else { $f.BaseName }

    $specId = Get-Field $text 'Spec ID'
    if (-not $specId) { $specId = $f.BaseName }

    $status = Get-Field $text 'Status'
    $wave   = Get-Field $text 'Wave'
    $prio   = Get-Field $text 'Priority'
    $type   = Get-Field $text 'Type'
    $domain = Get-Field $text 'Domain'

    $rows += [pscustomobject]@{
        id       = $specId
        bucket   = Get-Bucket $rel
        status   = $status
        wave     = $wave
        priority = $prio
        type     = $type
        domain   = $domain
        title    = $title
        path     = ($rel -replace '\\','/')
    }
}

# Ordena por bucket (fila primeiro) depois por id
$bucketOrder = @{ 'Fila-FABLE'=0; 'Fila'=1; 'Closeout-MVP'=2; 'Build-Validated'=3; 'Implementado'=4; 'Automatica'=5; 'Template'=6; 'Outro'=7 }
$rows = $rows | Sort-Object @{ Expression = { $bucketOrder[$_.bucket] } }, id

# ---- JSON ----
# Serializacao manual: ordem de chaves e espacamento fixos, independentes da
# versao do PowerShell (ver .NOTES).
$jsonKeys = @('id','bucket','status','wave','priority','type','domain','title','path')
$jb = [System.Text.StringBuilder]::new()
[void]$jb.Append("[`n")
for ($i = 0; $i -lt $rows.Count; $i++) {
    [void]$jb.Append("  {`n")
    for ($k = 0; $k -lt $jsonKeys.Count; $k++) {
        $key = $jsonKeys[$k]
        $val = ConvertTo-JsonStringLiteral ([string]$rows[$i].$key)
        $sep = if ($k -lt $jsonKeys.Count - 1) { ',' } else { '' }
        [void]$jb.Append("    ""$key"": ""$val""$sep`n")
    }
    $sep = if ($i -lt $rows.Count - 1) { ',' } else { '' }
    [void]$jb.Append("  }$sep`n")
}
[void]$jb.Append("]`n")
$jsonPath = Join-Path $specsRoot 'SPEC_INDEX.json'
Write-Utf8NoBom $jsonPath $jb.ToString()

# ---- Markdown ----
$counts = $rows | Group-Object bucket | Sort-Object @{ Expression = { $bucketOrder[$_.Name] } }
$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('# SPEC_INDEX (gerado)')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('> Gerado por `tools/generate_spec_index.ps1`. NAO editar a mao -- rode o script.')
[void]$sb.AppendLine('> Indice compacto para descoberta barata de specs (status por pasta + cabecalho).')
[void]$sb.AppendLine('')
[void]$sb.AppendLine("Total: **$($rows.Count)** specs")
foreach ($c in $counts) { [void]$sb.AppendLine("- $($c.Name): $($c.Count)") }
[void]$sb.AppendLine('')
[void]$sb.AppendLine('| ID | Bucket | Status | Wave | Domain | Path |')
[void]$sb.AppendLine('|---|---|---|---|---|---|')
foreach ($r in $rows) {
    $st = if ($r.status) { $r.status } else { '-' }
    $wv = if ($r.wave) { $r.wave } else { '-' }
    $dm = if ($r.domain) { $r.domain } else { '-' }
    [void]$sb.AppendLine("| $($r.id) | $($r.bucket) | $st | $wv | $dm | $($r.path) |")
}
$mdPath = Join-Path $specsRoot 'SPEC_INDEX.md'
# O "`n" final preserva a linha em branco que Set-Content acrescentava, mantendo
# diff zero contra o indice ja versionado.
Write-Utf8NoBom $mdPath ($sb.ToString() + "`n")

Write-Host "OK: $($rows.Count) specs indexadas"
Write-Host "  -> $($mdPath.Substring($repo.Length + 1))"
Write-Host "  -> $($jsonPath.Substring($repo.Length + 1))"
