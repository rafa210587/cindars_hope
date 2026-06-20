# Rule: Governança de Docs

Consolida: `no-doc-delete-without-candidate`, `legacy-doc-paths-forbidden`, `decision-and-game-rule-policy`, `spec-source-of-truth` (os originais são stubs apontando para cá).

## 1. Nenhum documento deletado sem candidato

Nenhum arquivo de doc pode ser deletado a menos que apareça em `docs/project/DOCUMENT_DELETE_CANDIDATES.md` E a tarefa autorize explicitamente a deleção. Sempre permitido sem aprovação prévia: mover para uma pasta de archive (rastreada no git), adicionar `status: delete_candidate`, adicionar à lista de candidates.

## 2. Apenas paths canônicos

Proibidos (nunca recriar ou editar): `docs_old/`, `docs/00_PROJECT/` … `docs/07_RELEASES/`.
Canônicos: `docs/project/`, `.specs/`, `docs/refinements/`, `docs/validation/`, `docs/backlog/`, `docs/architecture/`, `docs/decisions/`, `docs/game_rules/`, `docs/release/`.

A única fonte ativa de spec é `.specs/` (fila em `a_implementar/`, concluídas em `implementados/`). Nunca recriar `specs/` ou `spec/` na raiz, e não recriar `docs/specs/` (a árvore de specs foi relocada para `.specs/` — ver ADR-0015).

## 3. ADRs e game_rules são canônicos

- Decisões vivem em `docs/decisions/ADR-NNNN-*.md` (o *porquê*); o comportamento atual vive em `docs/game_rules/*.md` (o *o quê*).
- Amendments (`docs/amendments/`) são registro histórico apenas — nunca os cite como canônicos.
- Leia apenas os ADRs/game_rules citados pela spec (`required_adrs`, `required_game_rules`), não todos eles.
- Conflitos: code vs. ADR/game_rule → pare e reporte (um dos dois está desatualizado). Mudança de comportamento → superseding ADR.

## Enforcement

- Hook `protected-path-guard.ps1` (PreToolUse) bloqueia Edit/Write em `docs_old/`, pastas numeradas legadas e `specs|spec/` na raiz.
- Hook `stop-summary-check.ps1` (Stop) bloqueia a conclusão da tarefa se paths proibidos mudaram.
- Hook `delete-guard.ps1` (manual, invocado por tarefas de docs) checa a lista de candidates.
- `tools/docs/validate_docs.ps1` checa a criação de pastas legadas.
