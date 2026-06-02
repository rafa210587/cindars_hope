# Current Backlog — Cindar's Hope

> **Operational backlog. Agents read only if creating specs.**  
> **Do NOT read during implementation tasks.**

---

## Priority 0 — Blocking (must complete before FASE 10+)

### 0.1 Phase 2-3 Human Acceptance

**Status: NOT YET EXECUTED**

- [ ] Phase 2: Run validators in Unity Editor (~15-20 min)
  - CindarsHope/Repair and Validate Project
  - CindarsHope/Validate/Combat/Validate Combat Databases
  - CindarsHope/Advanced/Legacy/Validate/Validate Spec 17A - Scale Config
- [ ] Phase 3: Execute Play Mode checklist (~1.5-2 hours)
  - See: `docs/validation/spec_mvp_closeout_29_final_mvp_acceptance_and_promotion_execution_report.md`
- [ ] Fill Phase 2-3 result tables in SPEC_29 execution report
- [ ] Update `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md`
- [ ] Update `docs/00_PROJECT/CURRENT_STATE.md`

**Owner:** Human tester with local Unity Editor  
**Estimated:** 2-2.5 hours

---

### 0.2 SPEC_DOCS_31 — Safe Archive and Delete Execution

**Status: PENDING — Blocked on Phase 2-3**

- [ ] Move superseded specs from `a_implementar/reorg/` to archive folder
- [ ] Delete candidates from `docs/00_PROJECT/DOCUMENT_DELETE_CANDIDATES.md` after human review
- [ ] Update links and indexes
- [ ] Run docs validation

**Owner:** Agent (after human approves delete candidates)  
**Estimated:** 1-2 hours

---

## Priority 1 — High (after Phase 2-3 completes)

### 1.1 Post-Phase 3 Bug Fixes

If Phase 3 reveals Play Mode bugs:

- Triage: regression vs. known gap
- Regression bugs: fix immediately as hotfix spec
- Gap bugs: add to backlog priority 2

---

### 1.2 MVP Spec Promotion

After Phase 2-3 evidence collected:

- Move SPEC_18-28 from `a_implementar/closeout_mvp/` to `implementados/`
- Update SPEC_EXECUTION_ORDER.md
- Update IMPLEMENTATION_STATUS.md
- Update CURRENT_STATE.md to reflect final acceptance

---

## Priority 2 — FASE 10 Architecture

| Item | Effort | Notes |
|------|--------|-------|
| StatusEffectDatabase wiring | 5-10h | Wire database.asset in GameBootstrap inspector |
| Save system scaling (ISaveSectionProvider) | 20-30h | Add providers beyond HotbarSectionProvider pilot |
| Editor validator expansion | 30-40h | Validators for inventory, save/load, equipment |
| Unit test suite init | 40-60h | Core systems: SaveManager, event bus, bootstrap |

---

## Priority 3 — FASE 10 Gameplay Balance

| Item | Effort | Notes |
|------|--------|-------|
| Economy rebalancing | 20-30h | Farm income vs. spending |
| Difficulty tuning | 15-20h | Enemy scaling, difficulty modes |
| Cave density rebalancing | 10-15h | Spawn density vs. player progression |

---

## Priority 4 — FASE 11 Polish

| Item | Effort | Notes |
|------|--------|-------|
| Sound design | 40-60h | Soundtrack + SFX |
| Visual polish | 50-70h | Particles, screen shake, animations |
| UI/UX refinement | 25-35h | Tooltips, tutorials, settings persistence |

---

## Priority 5 — FASE 11+ Content

| Item | Effort | Notes |
|------|--------|-------|
| Enemy roster expansion (+20-30) | 60-80h | New AI archetypes |
| Skill tree expansion | 50-70h | Skill interactions, legendary skills |
| World expansion (new areas) | 80-120h | Forest, coastal zone, mountain |

---

## Not in Scope (deferred to FASE 12+)

- Multiplayer (200+ hours)
- Input System migration (30-50h; needed before multiplayer)
- Localization (100+ hours)
- Console ports
- Advanced graphics / shaders

---

*Last updated: 2026-06-01 (SPEC_DOCS_30)*  
*Full post-MVP details: `docs/backlog/post_mvp_backlog.md`*
