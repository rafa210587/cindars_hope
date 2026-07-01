---
name: reconcile-status
description: "Comando de workflow do projeto (equivalente ao /reconcile-status do Claude Code). Audita inconsistências entre arquivos de documentação. Pode ler PROJECT_LOG.md, validation reports e IMPLEMENTATION_STATUS.md — apenas em modo audit."
---

# /reconcile-status

Audita inconsistências entre arquivos de documentação. Pode ler PROJECT_LOG.md, validation reports e IMPLEMENTATION_STATUS.md — apenas em modo audit.

**Arguments:** `$ARGUMENTS` — scope opcional (ex.: `SPEC_18-28` ou `MVP`)

---

## Objetivo

Encontrar e documentar discrepâncias entre: execution reports, CURRENT_STATE.md, PROJECT_LOG.md, IMPLEMENTATION_STATUS.md, release reports, localização das specs. NÃO corrigir código.

---

## Leitura mínima

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. Validation reports relevantes ao scope
4. `docs/release/MVP_ACCEPTANCE_REPORT.md` (se scope MVP)

## Leitura condicional (modo audit — permitida aqui)

- `PROJECT_LOG.md` — para a timeline de eventos
- `docs/IMPLEMENTATION_STATUS.md` — para claims de status de spec
- `.specs/SPEC_EXECUTION_ORDER.md` — para o dependency graph
- Validation reports específicos no scope

---

## Procedimento

1. Identifique o scope de reconciliação
2. Leia todos os reports relevantes para esse scope
3. Para cada claim, verifique se existe evidência:
   - Claim: "Phase 1 PASS" → check: resultado do dotnet build no report
   - Claim: "Phase 2 PASS" → check: output do Unity validator no report
   - Claim: "Phase 3 PASS" → check: Play Mode checklist com marcas ✓
   - Claim: "spec implemented" → check: spec em `implementados/` com evidence header
4. Crie a audit matrix
5. Liste inconsistências com severity: CRITICAL / HIGH / MEDIUM / INFO
6. Proponha correções (NÃO aplicar automaticamente)
7. Produza o reconciliation report

---

## Edições permitidas

Apenas: criar `docs/validation/<scope>_reconciliation_audit_matrix.md`

## Edições proibidas

- Nenhuma movimentação de spec
- Nenhuma mudança de código
- Nenhuma atualização de status sem confirmação humana
- Nenhuma deleção de documento

---

## Saída esperada

```markdown
# Reconciliation Audit — <Scope>

## Claims vs. Evidence

| Claim | Location | Evidence Found? | Inconsistency | Severity |
|-------|----------|----------------|---------------|---------|
| <claim> | <file> | YES/NO | <description> | CRITICAL/HIGH/MED/INFO |

## Proposed Corrections

1. <correction> — requires: <human action / agent action>

## No Action Needed

[Claims that are consistent]
```
