#!/usr/bin/env pwsh
<#
.SYNOPSIS
Corruption guard — detecta arquivos de fonte/dados/assembly sobrescritos por lixo binario.

.DESCRIPTION
Em 2026-06-23 um evento (cloud sync / antivirus / disco) sobrescreveu 18 arquivos in-place
com lixo binario de MESMO TAMANHO (assinatura inicial '$'/0x24), incluindo 16 .cs, a DLL
Library/ScriptAssemblies/Unity.TextMeshPro.dll e 1 .asset. O git status mascarou os .cs
(stat-cache: mesmo tamanho + mtime) e o compile so falhou com erros confusos (CS1056 '$').

Este guard varre cedo e falha rapido, com instrucoes de recuperacao. Roda como Step 0 do
run_strict_validation.ps1 e pode ser usado manualmente ou como pre-commit hook.

Sem acentos (compativel com Windows PowerShell 5.1).

.EXAMPLE
.\tools\validate_no_corruption.ps1

Exit codes:
  0 - nenhum arquivo corrompido
  1 - corrupcao detectada (lista + recuperacao impressas)
#>
param(
    [string]$Root = "Assets",
    [double]$BadRatioThreshold = 0.30
)

$ErrorActionPreference = "Stop"
Write-Host "CORRUPTION_GUARD" -ForegroundColor Cyan

function Test-TextCorrupt {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -eq 0) { return $false }
    $start = 0
    # pula BOM UTF-8 (EF BB BF)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191) { $start = 3 }
    $n = [Math]::Min(160, $bytes.Length - $start)
    if ($n -le 0) { return $false }
    $bad = 0
    for ($i = $start; $i -lt ($start + $n); $i++) {
        $b = $bytes[$i]
        if ($b -eq 0) { return $true }                 # NUL nunca aparece em fonte de texto
        if ($b -gt 126 -or $b -lt 9) { $bad++ }        # controle/alto-byte concentrado = lixo
    }
    return (($bad / $n) -gt $BadRatioThreshold)
}

$corrupt = New-Object System.Collections.Generic.List[string]

# 1) Fontes .cs em todo o Root (compilam em qualquer assembly)
Get-ChildItem -Path $Root -Recurse -Include '*.cs' -File -ErrorAction SilentlyContinue | ForEach-Object {
    if (Test-TextCorrupt -Path $_.FullName) { $corrupt.Add(((Resolve-Path -Relative $_.FullName) -replace '^\.\\','')) }
}

# 2) Dados YAML do projeto (so sob _Game para evitar binarios legitimos de terceiros)
$dataRoot = Join-Path $Root "_Game"
if (Test-Path $dataRoot) {
    Get-ChildItem -Path $dataRoot -Recurse -Include '*.asset','*.unity','*.prefab','*.asmdef' -File -ErrorAction SilentlyContinue | ForEach-Object {
        if (Test-TextCorrupt -Path $_.FullName) { $corrupt.Add(((Resolve-Path -Relative $_.FullName) -replace '^\.\\','')) }
    }
}

# 3) DLLs compiladas devem ser PE valido (cabecalho 'MZ')
$dllCorrupt = New-Object System.Collections.Generic.List[string]
$scriptAssemblies = "Library/ScriptAssemblies"
if (Test-Path $scriptAssemblies) {
    Get-ChildItem -Path $scriptAssemblies -Filter '*.dll' -File -ErrorAction SilentlyContinue | ForEach-Object {
        $b = [System.IO.File]::ReadAllBytes($_.FullName)
        if ($b.Length -lt 2 -or -not ($b[0] -eq 77 -and $b[1] -eq 90)) { $dllCorrupt.Add($_.Name) }
    }
}

$total = $corrupt.Count + $dllCorrupt.Count
if ($total -eq 0) {
    Write-Host "   PASS: nenhum arquivo de fonte/dados/assembly corrompido" -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "CORRUPTION_DETECTED: $total arquivo(s)" -ForegroundColor Red
foreach ($f in $corrupt)    { Write-Host "  [fonte/dado] $f" -ForegroundColor Red }
foreach ($f in $dllCorrupt) { Write-Host "  [assembly]   Library/ScriptAssemblies/$f" -ForegroundColor Red }
Write-Host ""
Write-Host "RECUPERACAO:" -ForegroundColor Yellow
Write-Host "  rastreado no git : Remove-Item <f>; git checkout HEAD -- <f>   (o rm forca reescrita; stat-cache pode mascarar)"
Write-Host "  nao-rastreado    : reconstruir a partir dos consumidores ou regenerar via gerador de editor"
Write-Host "  Library *.dll    : Remove-Item e deixar o Unity recompilar (cache regeneravel)"
Write-Host ""
Write-Host "PREVENCAO: excluir a pasta do projeto de cloud sync (OneDrive/Drive/Dropbox) e do scan em tempo real do antivirus; rodar este guard antes de commit/validacao." -ForegroundColor Yellow
exit 1
