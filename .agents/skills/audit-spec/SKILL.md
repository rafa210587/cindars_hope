---
name: audit-spec
description: "Use para o audit de Phase 0: mapear o que existe, o que está ausente, os riscos e o menor delta seguro. NÃO implementa."
---

# /audit-spec

Use para o audit de Phase 0: mapear o que existe, o que está ausente, os riscos e o menor delta seguro. NÃO implementa.

**Arguments:** `$ARGUMENTS` — número da spec ou nome do arquivo

---

## Objetivo

Produzir uma audit matrix para o domínio da spec. Identificar o que já existe na codebase vs. o que a spec exige. Encontrar a mudança mínima necessária.

---

## Leitura mínima

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. Spec alvo
4. Source files relevantes (buscar na codebase pelo domínio da spec)

## Leitura opcional

- Prior validation reports listados como dependências
- Architecture docs citados pela spec

## Não ler por padrão

```
PROJECT_LOG.md
ROADMAP.md
docs/IMPLEMENTATION_STATUS.md
SPEC_EXECUTION_ORDER.md
all refinements
```

---

## Procedimento

1. Leia a spec e o CURRENT_STATE.md
2. Busque na codebase por:
   - Classes/files que a spec vai criar
   - Classes/files que a spec vai modificar
   - Implementações existentes da mesma responsabilidade
3. Classifique os achados:
   - `ALREADY_EXISTS` — requisito da spec já está atendido
   - `PARTIAL` — existe mas incompleto em relação à spec
   - `MISSING` — ainda não existe
   - `CONFLICT` — código existente contradiz o requisito da spec
4. Identifique riscos e o menor delta seguro
5. Produza a audit matrix (ver formato)

---

## Edições permitidas

Apenas uma: criar a audit matrix em `docs/validation/<spec_id>_phase0_audit_matrix.md`

---

## Edições proibidas

- Nenhuma mudança de código
- Nenhuma movimentação de spec
- Nenhuma atualização de status
- Nenhuma outra mudança de documentação

---

## Saída esperada

Crie `docs/validation/<spec_id>_phase0_audit_matrix.md`:

```markdown
# Phase 0 Audit — <SPEC_ID>

## Requirements vs. Codebase

| Requirement | Status | Notes |
|-------------|--------|-------|
| <requirement> | ALREADY_EXISTS / PARTIAL / MISSING / CONFLICT | <detail> |

## Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| <risk> | LOW/MED/HIGH | <approach> |

## Smallest Safe Delta

<description of minimum changes needed>

## Stop Conditions Found

<any blockers discovered>
```
