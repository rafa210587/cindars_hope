# Roadmap — Cindar's Hope

> **Planning-only document. Do NOT use as implementation contract.**  
> **Specs are the execution source of truth.**  
> **Agents executing a spec must NOT read this file by default.**

---

## Current Position

**Phase:** MVP Closeout (SPEC_18-29)  
**Status:** Code-complete and build-validated. Phase 2-3 Play Mode pending human execution.

---

## Near Term: Complete MVP Acceptance

**Priority 0 — Must complete before FASE 10+:**

1. **Phase 2-3 Human Acceptance** (~2-2.5 hours in Unity Editor)
   - Run validators: CindarsHope/Repair and Validate Project
   - Execute Play Mode checklist (SPEC_18-28 systems)
   - Fill Phase 2-3 results in SPEC_29 execution report
   - Promote specs to implementados after passing

2. **SPEC_DOCS_31 — Safe Archive and Delete Execution** (post-Phase 2-3)
   - Move superseded specs from `a_implementar/reorg/` to archive
   - Delete confirmed candidates from `DOCUMENT_DELETE_CANDIDATES.md`
   - Clean up stale a_implementar entries

---

## FASE 10 — Post-MVP Foundation (planning only)

Blocked until MVP is accepted (Phase 2-3 complete or explicit human decision to skip).

| Wave | Theme | Notes |
|------|-------|-------|
| 10A | Save system scaling | Add ISaveSectionProvider implementations |
| 10B | StatusEffectDatabase wiring | Wire StatusEffectDatabase.asset in GameBootstrap |
| 10C | Editor validator expansion | Add validators for inventory, save/load, equipment |
| 10D | Unit test suite initialization | Core systems: SaveManager, event bus, bootstrap |
| 10E | Gameplay balance | Economy, difficulty, enemy scaling |

---

## FASE 11 — Polish & Content (planning only)

| Wave | Theme | Notes |
|------|-------|-------|
| 11A | Sound design | Soundtrack + SFX |
| 11B | Visual polish | Particles, screen shake, UI animations |
| 11C | UI/UX refinement | Tooltips, tutorial overlays, settings persistence |
| 11D | Enemy roster expansion | +20-30 enemies, new AI archetypes |
| 11E | Skill tree expansion | Skill interactions, legendary skills |

---

## FASE 12 — Expansion (planning only)

| Wave | Theme | Notes |
|------|-------|-------|
| 12A | World expansion | New areas, fast travel |
| 12B | Input System migration | Controller support prerequisite |
| 12C | Localization | String extraction + translation pipeline |
| 12D | Advanced graphics | Shaders, post-processing, lighting |

---

## FASE 13+ — Future Phases (undefined)

- Multiplayer (200+ hours, needs architecture decision)
- Procedural storytelling
- Modding support
- Console platform ports

---

## Rules for This Document

1. This roadmap is updated by **human decision**, not agent implementation
2. Adding a wave here does NOT mean it will be implemented in that order
3. Before creating a wave's specs, verify dependencies and resource availability
4. Roadmap waves become specs via: planning session → refinement → spec in `a_implementar/`
5. If a spec and this roadmap conflict → **follow the spec**

---

*Created: 2026-06-01 (SPEC_DOCS_30)*  
*Based on: docs/backlog/post_mvp_backlog.md, docs/backlog/NEXT_WAVES_ROADMAP_v1.1*
