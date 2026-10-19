# Context Policy Check Hook
# Avisa quando um plano de execucao inclui leituras historicas pesadas sem justificativa.
# Desabilitado por padrao. Rode manualmente via /validate-spec ou invocacao explicita.

# Arquivos pesados que nao devem ser leituras padrao em tarefas de implementacao
$HeavyDefaultReads = @(
    "PROJECT_LOG.md",
    "docs/IMPLEMENTATION_STATUS.md",
    "SPEC_EXECUTION_ORDER.md",
    "ROADMAP.md"
)

# Palavras-chave de justificativa que tornam as leituras pesadas aceitaveis
$AcceptedJustifications = @(
    "audit",
    "reconciliation",
    "reconcile",
    "regression investigation",
    "wave planning",
    "plan-wave",
    "explicit human request"
)

param(
    [string]$PlanText = ""
)

if ($PlanText -eq "") {
    Write-Host "Context Policy Check: No plan text provided. Skipping."
    exit 0
}

$warnings = @()

foreach ($heavyFile in $HeavyDefaultReads) {
    if ($PlanText -match [regex]::Escape($heavyFile)) {
        $hasJustification = $false
        foreach ($justification in $AcceptedJustifications) {
            if ($PlanText -match $justification) {
                $hasJustification = $true
                break
            }
        }
        if (-not $hasJustification) {
            $warnings += "WARNING: Plan reads '$heavyFile' without audit/reconciliation/regression justification."
        }
    }
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "==================================================="
    Write-Host "Context Policy Check - WARNINGS"
    Write-Host "==================================================="
    foreach ($w in $warnings) {
        Write-Host "  $w"
    }
    Write-Host ""
    Write-Host "  Rule: context-reading-policy.md"
    Write-Host "  Use docs/project/CURRENT_STATE.md instead for execution context."
    Write-Host "==================================================="
    Write-Host ""
    exit 1
}

Write-Host "Context Policy Check: PASS - no heavy default reads detected."
exit 0
