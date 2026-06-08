# WAVE 13 — Blocked Future Scope Report

**Wave:** WAVE 13 — Bestiary  
**Status:** BLOCKED_BY_FUTURE_SCOPE  
**Date:** 2026-06-08  
**Branch:** dev  
**Reason:** All 4 specs contain `_future_` in filenames and carry `Status: Future mapped`

---

## Blocked Specs

| Spec | File | Status | Reason |
|------|------|--------|--------|
| Bestiary Knowledge State Save/Load | `13_spec_bestiary_knowledge_state_save_load_future_runtime.md` | BLOCKED_BY_FUTURE_SCOPE | `_future_` suffix; `Status: Future mapped` |
| Bestiary Knowledge UI Projection | `13_spec_bestiary_knowledge_ui_projection_future_runtime.md` | BLOCKED_BY_FUTURE_SCOPE | `_future_` suffix; `Status: Future mapped` |
| Knowledge Discovery Event | `13_spec_knowledge_discovery_event_runtime_future.md` | BLOCKED_BY_FUTURE_SCOPE | `_future_` suffix; `Status: Future mapped` |
| Knowledge Research NPC/Books/Ruins | `13_spec_knowledge_research_npc_books_ruins_services_future_runtime.md` | BLOCKED_BY_FUTURE_SCOPE | `_future_` suffix; `Status: Future mapped` |

**Total: 4/4 specs BLOCKED_BY_FUTURE_SCOPE**

---

## Classification

Per `.claude/rules/spec_quality_gate.md`:

> `BLOCKED_BY_FUTURE_SCOPE` — depende de spec de wave futura  
> Final blocker. Não pode ser resolvido nesta wave. Para aqui, retorna ao user.

Per `.claude/rules/spec_dependency_resolution.md`:

> Stop resolution and mark the current spec as `BLOCKED_BY_FORBIDDEN_SCOPE` if dependency is:
> - `future` or `mapped` (future wave spec)

WAVE 13 specs were pre-generated with `_future_` suffixes indicating they are expansion content planned for a future wave, not the current MVP implementation scope.

---

## Evidence

Filename evidence:
- `13_spec_bestiary_knowledge_state_save_load_**future**_runtime.md`
- `13_spec_bestiary_knowledge_ui_projection_**future**_runtime.md`
- `13_spec_knowledge_discovery_event_runtime_**future**.md`
- `13_spec_knowledge_research_npc_books_ruins_services_**future**_runtime.md`

All four filenames contain `_future_` — consistent with `SPEC_REGISTRY_TO_IMPLEMENT.md` marking them as `Future mapped`.

---

## Required Action

**Do NOT execute any WAVE 13 spec** without explicit human authorization that:

1. Confirms these are no longer future/expansion scope
2. Removes the `_future_` suffix or updates spec headers to remove `Status: Future mapped`
3. Authorizes the specific spec(s) to execute

---

## No Impact on WAVE 00-12

WAVE 13 blocking does not affect any WAVE 00-12 results.  
All WAVE 00-12 specs are BUILD_VALIDATED.  
See: `docs/validation/WAVE_00_12_RECONCILIATION_AUDIT.md`

---

*Created: 2026-06-08 (Governance Reconciliation)*
