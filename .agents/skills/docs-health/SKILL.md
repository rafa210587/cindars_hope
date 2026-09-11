---
name: docs-health
description: "Valida a estrutura e a consistência da documentação."
---

# /docs-health

Valida a estrutura e a consistência da documentação.

## Quando usar

- Depois de mover specs ou refinements para implementados/
- Depois de atualizar status files
- Depois de alterar PROJECT_LOG.md ou IMPLEMENTATION_STATUS.md
- Como parte de um health check geral

## Checks obrigatórios

### 1. Spec Source of Truth

- [ ] `.specs/` existe e contém specs
- [ ] Nenhum diretório `specs/` ou `spec/` no root
- [ ] `.specs/a_implementar/` contém specs futuras
- [ ] `.specs/implementados/` contém specs concluídas
- [ ] `.specs/SPEC_EXECUTION_ORDER.md` existe (se aplicável)
- [ ] `.specs/SPEC_SOURCE_OF_TRUTH.md` existe ou não é crítico

### 2. Refinement Structure

- [ ] `docs/refinements/a_implementar/pre_refinamentos/` contém refinements futuros
- [ ] `docs/refinements/implementados/` contém refinements concluídos
- [ ] Os maps são internamente consistentes

### 3. Active Documentation

- [ ] `docs/design/` existe (design specs)
- [ ] `docs/architecture/` existe (architecture)
- [ ] `docs/operations/` existe (docs operacionais)
- [ ] `docs_old/` preservado mas não editado
- [ ] `docs/IMPLEMENTATION_STATUS.md` rastreia o estado atual

### 4. Status & Tracking

- [ ] Entradas de `docs/IMPLEMENTATION_STATUS.md` têm evidência (spec file ou log validado)
- [ ] `PROJECT_LOG.md` tem entrada recente quando a tarefa foi significativa
- [ ] Nenhuma capacity entry órfã (declarada mas sem spec file)
- [ ] Nenhuma contradição entre IMPLEMENTATION_STATUS e o estado real das specs

### 5. Registries & Maps (se existirem)

- [ ] `.specs/SPEC_REGISTRY_IMPLEMENTED.md` consistente com `.specs/implementados/`
- [ ] `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` consistente com `.specs/a_implementar/`
- [ ] `docs/refinements/implementados/ref_implementados_map.md` consistente com a pasta
- [ ] `docs/refinements/a_implementar/ref_futuro_map.md` consistente com a pasta
- [ ] Nenhum ID duplicado ou spec listada duas vezes

### 6. Links & References

- [ ] Links nos docs apontam para arquivos existentes (verificar file paths)
- [ ] Nenhum cross-reference quebrado entre specs/refinements
- [ ] Nenhuma referência a documentos removidos ou arquivados

### 7. Tools Validation Script

Rode a validação de documentação:

```powershell
.\tools\docs\validate_docs.ps1
```

**Esperado:** PASS (ou lista de issues específicos a corrigir)

## Saída esperada

```text
Status: PASS | WARNING | FAIL

Spec Structure:
  ✓ .specs/ is sole source of truth
  ✓ No root specs/ or spec/
  ✓ a_implementar/ and implementados/ consistent
  ✓ SPEC_EXECUTION_ORDER.md coherent

Refinement Structure:
  ✓ pre_refinamentos/ path correct
  ✓ implementados/ folder consistent
  ✓ Maps up to date

Active Documentation:
  ✓ docs/design/, docs/architecture/, docs/operations/ all present
  ✓ docs_old/ preserved, not edited

Status & Tracking:
  ✓ IMPLEMENTATION_STATUS.md has evidence for all claims
  ✓ PROJECT_LOG.md current
  ⚠️ WARNING: Spec X marked implemented but spec file missing

Registries & Maps:
  ✓ Consistent with source directories
  ✓ No duplicate IDs

Links & References:
  ✓ All cross-links valid

Tools Validation:
  ✓ tools/docs/validate_docs.ps1: PASS

Issues found:
  [if any]

Corrective actions:
  [if any]

Residual risk:
  [if any]
```

## Exemplos

### PASS

```text
Status: PASS

All checks passed:
- Source of truth structure: valid
- Specs consistent (a_implementar + implementados)
- Refinements consistent
- Status and tracking coherent with evidence
- No broken links
- Tools validation: PASS
```

### WARNING

```text
Status: WARNING

Issues found:
  - IMPLEMENTATION_STATUS.md line 42: "Spec 15 implemented" but spec file missing
  - PROJECT_LOG.md has 2 days without entry (acceptable gap)

Corrective actions:
  - Verify Spec 15 status: moved to implementados or deferred?
  - Update IMPLEMENTATION_STATUS if status changed

Residual risk:
  Minor: Tracking inconsistency, no broken functionality
```

### FAIL

```text
Status: FAIL

Issues found:
  - Root-level specs/ directory detected (should not exist)
  - SPEC_REGISTRY_IMPLEMENTED.md lists 20 specs but implementados/ contains 19

Corrective actions REQUIRED:
  1. Remove root-level specs/ directory
  2. Reconcile SPEC_REGISTRY_IMPLEMENTED.md with actual implementados/ folder
  3. Re-run validation to confirm fix

Residual risk:
  CRITICAL: Inconsistent state may confuse future spec execution
```

---

## Não faça

- Alegar PASS sem rodar `tools/docs/validate_docs.ps1`
- Ignorar issues de nível WARNING (elas podem virar bugs)
- Deixar um estado FAIL sem resolução
- Editar docs_old/** (é apenas archive)
- Criar `specs/` ou `spec/` no root

**O health check está completo. O Status determina se a tarefa pode prosseguir.**
