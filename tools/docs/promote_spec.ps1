param(
    [Parameter(Mandatory=$true)] [string] $FutureSpec,
    [Parameter(Mandatory=$true)] [string] $FutureRef,
    [Parameter(Mandatory=$true)] [string] $ImplementedSpec,
    [Parameter(Mandatory=$true)] [string] $ImplementedRef
)

$ErrorActionPreference = "Stop"

Write-Host "== Promote Spec Helper =="

if (-not (Test-Path $FutureSpec)) {
    throw "Future spec not found: $FutureSpec"
}

if (-not (Test-Path $FutureRef)) {
    throw "Future refinement not found: $FutureRef"
}

if (Test-Path $ImplementedSpec) {
    throw "Implemented spec already exists: $ImplementedSpec"
}

if (Test-Path $ImplementedRef) {
    throw "Implemented refinement already exists: $ImplementedRef"
}

$futureSpecContent = Get-Content $FutureSpec -Raw
$futureRefContent = Get-Content $FutureRef -Raw

$implementedSpecHeader = @"
# SPEC IMPLEMENTADA — TODO: preencher título

> Origem futura: `$FutureSpec`
> Refinement futuro: `$FutureRef`
> Status: Implementado parcial
> Evidência principal: TODO
> Validação Unity: TODO

---

## Estado real implementado

TODO: descrever o que foi implementado de fato.

## Escopo preservado da spec futura

"@

$implementedRefHeader = @"
# REF IMPLEMENTADO — TODO: preencher título

> Origem futura: `$FutureRef`
> Spec futura: `$FutureSpec`
> Spec implementada: `$ImplementedSpec`
> Status: Refinement implementado

---

## Decisões preservadas

"@

New-Item -ItemType Directory -Force -Path (Split-Path $ImplementedSpec) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $ImplementedRef) | Out-Null

Set-Content -Path $ImplementedSpec -Value ($implementedSpecHeader + "`n" + $futureSpecContent) -Encoding UTF8
Set-Content -Path $ImplementedRef -Value ($implementedRefHeader + "`n" + $futureRefContent) -Encoding UTF8

Write-Host "Created implemented spec: $ImplementedSpec"
Write-Host "Created implemented refinement: $ImplementedRef"

Write-Host ""
Write-Host "Next manual steps:"
Write-Host "1. Edit implemented spec with real evidence."
Write-Host "2. Edit implemented refinement with executed scope."
Write-Host "3. Update .specs/SPEC_REGISTRY_IMPLEMENTED.md."
Write-Host "4. Update .specs/SPEC_REGISTRY_TO_IMPLEMENT.md."
Write-Host "5. Update docs/refinements/implementados/ref_implementados_map.md."
Write-Host "6. Update docs/refinements/a_implementar/ref_futuro_map.md."
Write-Host "7. Update docs/IMPLEMENTATION_STATUS.md."
Write-Host "8. Update PROJECT_LOG.md."
Write-Host "9. Run tools/docs/validate_docs.ps1."
