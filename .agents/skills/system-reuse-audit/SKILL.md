---
name: system-reuse-audit
description: Check current owners, contracts and callers before adding a manager, service, data type or system. Use during Phase 0 or when a proposed abstraction may duplicate existing behavior.
---

# Skill: System reuse audit

**Find the current owner before creating another implementation.** Historical incidents and
similar folder names are search hints, not proof that two active systems are equivalent.

## Bounded discovery

1. Search exact type names and domain synonyms with `rg` in `Assets/_Game/Scripts`;
   use `rg --files` for paths. Include untracked work; `git grep` alone misses it.
2. Inspect candidate declarations, callers, composition wiring and relevant tests.
   Distinguish executor, unlock/catalog, adapter and legacy compatibility roles.
3. Record reuse/extend/new and the decisive path in the existing plan or task evidence.
   No separate report, universal type inventory or fixed time quota is required.

## Decision

| Finding | Action |
|---|---|
| An active owner already implements the contract | Reuse or extend it through existing wiring. |
| Several candidates | Follow the owner/migration already selected by the approved spec. If still materially ambiguous, report the conflict and continue independent authorized work. Do not add a third system. |
| Similar name, different responsibility | Preserve the existing type; choose an unambiguous name for the new responsibility. |
| No suitable implementation | Create only the smallest cohesive type needed by the approved contract. |

Before extracting, identify state ownership, consumer needs and failure behavior. A helper
that takes the entire manager hides dependencies instead of separating them. Do not create an
interface per class; use a port for a real boundary or substitutable policy.

`runtime-code-guard` flags some duplicate names after writes; it does not prove semantic
duplication or undo edits. Apply the canonical validation matrix; absence of a particular
report heading does not invalidate otherwise valid build evidence.
