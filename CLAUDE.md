# CLAUDE.md — Cindar's Hope

> Short router for Claude Code. Full rules in `AGENTS.md` and `.claude/rules/`.

---

## Project Identity

2D pixel art RPG + farm sim in Unity LTS/C#. World: Vaalara / Cindar's Hope / Dornecia.
Specs live in `docs/specs/`. Spec is the execution contract. Refinement is fallback only.

---

## Default Reads (implementation task)

Read ONLY:

1. `CLAUDE.md` (this file)
2. `docs/00_PROJECT/CURRENT_STATE.md` — active queue, blockers, key paths (~80 lines)
3. The active spec (`docs/specs/a_implementar/spec_*.md`)
4. Files explicitly listed in the spec scope

## Conditional Reads (only if spec or prompt cites them)

- A specific refinement
- The immediately prior validation report listed as a dependency
- A specific architecture document needed by the spec
- A specific backlog item

## Do NOT Read By Default

```
PROJECT_LOG.md         — audit / reconciliation / regression only
ROADMAP.md             — planning new waves / creating specs only
docs/IMPLEMENTATION_STATUS.md  — audit only; use CURRENT_STATE.md instead
AGENTS.md (full)       — rules apply without reading whole file; stop conditions are enforced
GDD complete           — never by default
all refinements        — conditional only
archived/superseded specs
docs_old/**
unrelated validation reports
```

---

## Commands

| Command | When to use |
|---------|-------------|
| `/start-spec` | Plan a spec (no code) |
| `/audit-spec` | Phase 0 only — map what exists, risks, delta |
| `/implement-spec` | Execute a spec |
| `/validate-spec` | Run validations after implementation |
| `/finish-spec` | Closeout — phase-aware, no auto-promote |
| `/reconcile-status` | Audit inconsistencies (may read PROJECT_LOG) |
| `/plan-wave` | Plan next FASE/wave (reads ROADMAP) |
| `/bugfix` | Fix a specific bug |
| `/docs-health` | Run docs validation only |
| `/review-non-regression` | Audit diff for violations |

---

## Skills

Use `.claude/skills/<name>/SKILL.md` when task matches:

| Skill | When |
|-------|------|
| `spec-execution` | Implementing any spec |
| `docs-governance` | Organizing/archiving docs |
| `unity-validation` | Compile + validator flow |
| `bootstrap-wiring` | GameBootstrap / manager wiring |
| `combat-data-wiring` | Weapon/Spell/StatusEffect databases |
| `ui-modal-stack` | ModalManager / input blocking |
| `save-load-pattern` | Save/load data |
| `event-bus-pattern` | GameEventBus communication |
| `non-regression-review` | Pre-closeout audit |
| `cave-stable-run-guard` | Any cave procedural change |

---

## Agents

Delegate via `.claude/agents/`:

| Agent | Role |
|-------|------|
| `spec-implementer` | Code specs |
| `docs-curator` | Document governance |
| `unity-validator` | Validation only |
| `non-regression-auditor` | Regression audit |
| `architecture-reviewer` | Pre-wave architecture |
| `bugfix-investigator` | Bug investigation |
| `asset-wiring-specialist` | Unity data / prefab wiring |

---

## Stop Conditions

Stop and report to human if:

- Spec and `CURRENT_STATE.md` conflict
- Spec requires files outside its declared scope
- A mandatory validation fails with no documented path forward
- Task would move a spec to `implementados/` without required evidence
- Task would delete a document not in `DOCUMENT_DELETE_CANDIDATES.md`
- Task would manually edit `.unity` / `.prefab` / `.asset` YAML without spec authorization
- Context requires reading PROJECT_LOG without audit/reconciliation/regression justification

---

## Invariant Rules

See `.claude/rules/RULES.md` for full list. Key non-negotiables:

- No `GameObject.Find()` / `FindObjectOfType()` at runtime
- No gameplay communication without `GameEventBus`
- No Unity refs in save DTOs
- No `CindarsHope.Debug` namespace
- No spec promoted without evidence
- No MVP/Play Mode PASS claim without evidence
- Commits in Portuguese

---

## Conflict Resolution

- Spec vs. roadmap → follow spec
- Spec vs. refinement → follow spec
- Spec vs. CURRENT_STATE → stop and report
- PROJECT_LOG vs. CURRENT_STATE → prefer CURRENT_STATE, report mismatch

---

*Updated: 2026-06-01 (SPEC_CLAUDE_31)*
