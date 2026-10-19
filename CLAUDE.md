# CLAUDE.md — Cindar's Hope

2D pixel-art RPG + farm sim in Unity/C#. World: Vaalara / Cindar's Hope / Dornecia. Specs live in `.specs/`; never recreate `specs/` or `spec/`.

## Initial reading

Read this router, `docs/project/CURRENT_STATE.md` and the files relevant to the selected journey below.
For spec execution, also read the active spec and its referenced files. The invariants in `AGENTS.md` remain binding.
Select only the needed workflow from the [harness catalog](.claude/HARNESS_INDEX.md); references are conditional, never a mandatory reading queue.
Spec planning uses `spec-authoring`; execution uses `spec-execution`; closeout uses `/finish-spec`. These workflows point to applicable sources and gates.
For staged SDD authoring: `refinement-authoring` → `spec-authoring` → `spec-planning` → `spec-task-authoring` (including consistency review). Existing complete specs remain valid.
Do not load PROJECT_LOG, ROADMAP, the full GDD, docs_old, unrelated refinements/reports or archived specs by default. History is for audit/reconciliation/regression; roadmap is for planning.

## Proportionate execution

Choose by intended behavior and risk, not file count:

| Journey | Entry and minimum evidence |
|---|---|
| Local correction of an existing contract; typo or bounded bug | `/bugfix`: reproduce/inspect → smallest fix → relevant check → short result. No new spec, separate plan/report, delegation or commit by default. |
| New feature or cohesive planned maintenance | Approved spec; use the short maintenance format when sufficient, then `spec-execution`. |
| Save/IDs, cross-system, lifecycle or structural contract change | Review affected contracts and integration risks before editing; use the existing spec or amend/create the necessary scoped spec. A one-line diff can take this journey. |

Escalate when discovery changes the risk or intended contract; continue independent authorized
work. A bugfix request does not authorize a new feature. Reuse existing tests/evidence and add
only missing coverage. A reversible wording fix needs inspection, not a new test suite.

The owner may complete a small fix, bounded edit and its behavior test in the same context. Delegate when an independent slice, specialization or risk-based review justifies coordination; do not delegate every edit automatically.
Define ownership, inputs, acceptance criteria and evidence when delegating. Preserve concurrent work; inspect artifacts and results before accepting summaries. Reuse valid gates over equivalent inputs.
Use independent review for higher-risk changes (save contracts, cross-system gameplay, architecture, substantial visual integration). Auditors are read-only; `unity-validator` may run tools and write evidence, without fixing runtime.
Before execution or delegation, apply [task model routing](.claude/rules/task-model-routing.md): choose the most economical available model suited to the slice's risk and validation, with escalation when needed. This project authorizes that selection; an explicit user model choice takes precedence. Apply overrides only through supported host controls; otherwise retain the configured model and report the limitation. Reasoning effort is not model selection. Do not translate Claude aliases into Codex models.

## Invariants and conflicts

Full rules live in `AGENTS.md` and [.claude/rules/RULES.md](.claude/rules/RULES.md). Preserve Git/file safety, architecture, IDs/save, pixel-art imports and validation truth.
Specs override roadmap/refinements; conflicts with CURRENT_STATE must be reported to the user. Do not expand scope, promote a spec without evidence, delete documents without authorization/candidate status or edit Unity YAML without the explicit conditions.
A failed mandatory gate needs a documented resolution path; runtime requires a test, Play Mode scenario or justified residual risk. Compile does not prove human/visual acceptance.
Commits remain in Portuguese. Push, PR/MR and prohibited Git operations remain subject to AGENTS authorization requirements.

## Language and discovery

Use English for new or migrated AI instructions and supporting resources; preserve identifiers and validator-consumed literals. Keep conversation, user-facing game text and human documentation in the requested language; this session uses Portuguese.
Select skills by the task, then load only relevant references, templates or examples. Discoverability is not a guarantee of correct selection or of every configured agent being available in the current session.
For working-tree discovery, start with `pwsh -File tools/codex/Get-ChangeSummary.ps1`; inspect exact affected paths before editing. Bound search/log output, report omitted counts, and retrieve details when needed. Never truncate skill instructions, decisive errors or evidence required for a conclusion.
