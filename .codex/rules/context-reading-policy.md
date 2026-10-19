# Rule: Política de Leitura de Contexto

**Read by journey:** `CLAUDE.md`, `docs/project/CURRENT_STATE.md` and the target files.
Spec execution also reads the active spec and its referenced files. A local correction uses
`/bugfix`; read an existing spec only when needed to establish the intended contract. Do not
create a spec or load execution/promotion protocols merely to fix a typo or bounded defect.

**Não leia por padrão:** `PROJECT_LOG.md`, `ROADMAP.md`, GDD completo, `IMPLEMENTATION_STATUS.md`, refinements antigos, archived specs, validation reports não relacionados.

**Exceções permitidas** (declare a justificativa antes de ler):

- `/reconcile-status` → pode ler `PROJECT_LOG.md` e `IMPLEMENTATION_STATUS.md`
- Investigação de regressão → pode ler validation reports anteriores
- `/plan-wave` → pode ler `ROADMAP.md` e `current_backlog.md`
- Pedido humano explícito

## Bounded discovery output

Start working-tree discovery with `pwsh -File tools/codex/Get-ChangeSummary.ps1`.
The summary is navigation, not attribution or authorization; inspect exact scoped diffs
and prior dirty content before editing. Use filtered searches and bounded result lists;
keep complete evidence in an explicit artifact when necessary and disclose omitted counts.
Read omitted details when they can change the decision. Do not trim required skill instructions,
hide failing diagnostics or substitute a truncated log for validation evidence.
Reuse already loaded context and inspect only the relevant catalog entry when its ID is known.

## Optional C# semantic navigation

If the project-local Serena integration is enabled and available, use bounded symbol/reference
queries for the covered project graph. Use `rg` for simple text searches and as fallback when
Serena is unavailable, reports drift or cannot load dependencies. Never treat a failed or
partial query as proof that no consumers exist. XML references are not necessarily callers.
Snapshot results do not replace reading the current source before edits; reconnect after drift.
See `tools/serena/README.md` for actual enablement status and coverage. No token savings are
assumed without measured comparison.
