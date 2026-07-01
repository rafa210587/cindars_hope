# Rule: Governança de Docs

**1. Nenhum documento deletado sem candidato.** Delete só se constar em `docs/project/DOCUMENT_DELETE_CANDIDATES.md` E a tarefa autorizar explicitamente. Permitido sem aprovação: mover para archive, adicionar `status: delete_candidate`, adicionar à lista.

**2. Apenas paths canônicos.**

Proibidos (nunca recriar ou editar): `docs_old/`, `docs/00_PROJECT/` … `docs/07_RELEASES/`, `specs/` ou `spec/` na raiz, `docs/specs/`.

Canônicos: `docs/project/`, `.specs/`, `docs/refinements/`, `docs/validation/`, `docs/backlog/`, `docs/architecture/`, `docs/decisions/`, `docs/game_rules/`, `docs/release/`.

Fonte ativa de spec: `.specs/a_implementar/` (fila) e `.specs/implementados/` (concluídas).

**3. ADRs e game_rules são canônicos.** Decisões → `docs/decisions/ADR-NNNN-*.md`; comportamento atual → `docs/game_rules/*.md`. Amendments são registro histórico — nunca cite como canônicos. Conflito code vs. ADR/game_rule → pare e reporte.

## Enforcement

Hook `protected-path-guard.ps1` bloqueia Edit/Write em paths proibidos.
