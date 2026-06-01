# Post-MVP Backlog — Cindar's Hope

**Date:** 2026-06-01  
**Status:** MVP Accepted (Phase 0-1 Complete)  
**Phase 2-3 Status:** Pending Human Play Mode Execution  
**Next Phases:** FASE 10+  

---

## Overview

This backlog captures residual work identified during MVP closeout (SPEC_18-29) that did not block MVP acceptance but should be prioritized for post-MVP phases.

**MVP Completion Criteria Met:** All systems code-ready and build-validated.  
**Phase 2-3 Outcome (Pending):** Will determine if any blockers emerge from Play Mode testing.

---

## Priority 1: Post-MVP Phase Start Blockers (If Phase 2-3 Reveals Issues)

### 1.1 Play Mode Validation Issues

**If any SPEC_18-28 systems fail Phase 3 Play Mode testing:**

- [ ] **Triage:** Separate regression bugs from feature gaps
- [ ] **Regression Fixes:** Any system that worked in prior SPEC but broke → fix immediately (hotfix)
- [ ] **Acceptable Gaps:** Missing polish or edge cases → add to backlog (not blocker)
- [ ] **Update Execution Reports:** Record FAIL reason + resolution plan

**Owner:** Human tester (Phase 3 execution)  
**Trigger:** Phase 3 FAIL condition on any system  
**Acceptance:** All Phase 2-3 results documented in SPEC_29 execution report

---

## Priority 2: Code Architecture Improvements (Non-Blocking)

### 2.1 Save System Scaling

**Current State:** HotbarSectionProvider implemented as pilot; save/load infrastructure designed for scaling.

**Residual Work:**
- [ ] Implement additional `ISaveSectionProvider` implementations (inventory, equipment, spells)
- [ ] Extend `SaveManager` to dynamically discover providers via reflection
- [ ] Test backward compatibility with v4→v5 migrations when new providers added
- [ ] Document save provider pattern for future extensions

**Rationale:** Sectional save system prevents SaveManager monolithic growth; pilot worked; scaling pending.

**Estimated Effort:** 20-30 hours (design, implement 4-5 providers, test migrations)

---

### 2.2 StatusEffectDatabase Wiring

**Current State:** StatusEffectDatabaseSO designed; SpellCastService + EnemyStatusRuntimeTicker implement fallback Resources.Load; CombatDatabaseValidator added validation checks.

**Residual Work:**
- [ ] Create StatusEffectDatabase.asset in `Assets/_Game/Data/Combat/`
- [ ] Wire StatusEffectDatabase in GameBootstrap.Inspector
- [ ] Populate database with all status effects (burn, poison, stun, etc.)
- [ ] Remove StatusEffectId Resources.Load fallback (after confirmation database is live)
- [ ] Update CombatDatabaseValidator to require StatusEffectDatabase present

**Rationale:** Registry pattern established in SPEC_04-11 architecture reorg; one registry remains incomplete.

**Estimated Effort:** 5-10 hours (asset creation, testing, documentation)

---

### 2.3 CombatRuntimeInstallContext Simplification

**Current State:** CombatRuntimeInstallContext POCO [Serializable]; CombatRuntimeInstaller.Install() validates all fields; GameBootstrap calls Install() in InitializeManagers().

**Residual Work:**
- [ ] Extract CombatRuntimeInstallContext field resolution logic into separate builder class
- [ ] Add logging/diagnostics for field resolution (for debugging wiring issues)
- [ ] Document field requirements + responsibility matrix (GameBootstrap vs scene)
- [ ] Consider DI container refactor (currently static installer pattern)

**Rationale:** Installer pattern works; but monolithic validation logic can be clearer with builder separation.

**Estimated Effort:** 10-15 hours (refactor, testing, documentation)

---

## Priority 3: Testing & Validation Coverage

### 3.1 Editor Validator Expansion

**Current State:** Validators exist for shop, combat, visual scale, enemy roster; not all systems have dedicated validators.

**Residual Work:**
- [ ] Create `ValidateInventorySystem.cs` (slots, capacity, item persistence)
- [ ] Create `ValidateSaveLoadSystem.cs` (cross-scene save/load, migration validation)
- [ ] Create `ValidateEquipmentSystem.cs` (slots, durability, stat application)
- [ ] Create `ValidateSkillTreeSystem.cs` (tree structure, purchase rules, respec)
- [ ] Integrate all validators into `CindarsHope/Repair and Validate Project` menu

**Rationale:** Validators catch errors early; coverage gaps leave edge cases untested.

**Estimated Effort:** 30-40 hours (design, implement, test 5 validators)

---

### 3.2 Unit Test Suite Initialization

**Current State:** No unit tests (all validation in editor validators and Play Mode).

**Residual Work:**
- [ ] Create test project structure (`Assets/_Game/Tests/`)
- [ ] Start with core systems: SaveManager, GameBootstrap, event bus
- [ ] Add tests for save migrations (v2→v3→v4→v5)
- [ ] Add tests for registry lookups (item, spell, skill databases)
- [ ] Add tests for damage formula, status effect application

**Rationale:** Unit tests prevent regression; critical for future refactors.

**Estimated Effort:** 40-60 hours (setup, write 20-30 core tests, CI integration)

---

## Priority 4: Gameplay Balance & Tuning

### 4.1 Economy Rebalancing

**Current State:** Economy implemented (shop, crafting, pricing, loot); values placeholders set in MVP.

**Residual Work:**
- [ ] Audit farming income vs. spending (plot yield → gold per day)
- [ ] Audit combat income vs. spending (monster loot → gold per encounter)
- [ ] Audit shop pricing (markup, profitability)
- [ ] Adjust crafting costs (recipes, workstation fees)
- [ ] Balance progression pacing (level requirements, skill point availability)
- [ ] Document economy tuning rationale + balance targets

**Rationale:** MVP uses placeholder values; economy tuning critical for retention.

**Estimated Effort:** 20-30 hours (analysis, tuning, testing, documentation)

---

### 4.2 Difficulty Tuning

**Current State:** Enemy damage, HP, spawn density set in MVP; difficulty modes not implemented.

**Residual Work:**
- [ ] Audit enemy damage per level (should scale with player progression)
- [ ] Audit cave boss difficulty (gate level requirements, HP scaling)
- [ ] Add difficulty modes (Normal, Hard, Hardcore)
- [ ] Scale enemy stats + spawn density per mode
- [ ] Tune status effect durations + damage values

**Rationale:** Difficulty balance critical for engaging gameplay; MVP baseline sufficient.

**Estimated Effort:** 15-20 hours (analysis, tuning, testing, implementation)

---

## Priority 5: Polish & Presentation

### 5.1 Sound Design

**Current State:** Game loop functional with placeholder SFX; no music, minimal ambience.

**Residual Work:**
- [ ] Compose/license game soundtrack (farm, town, cave, boss themes)
- [ ] Add SFX for: combat (hit, miss, crit), farming (plant, harvest), status effects, UI (click, open, close)
- [ ] Add ambience loops (farm wind, cave drip, town chatter)
- [ ] Implement audio manager (volume control, muting, fade transitions)
- [ ] Integrate into pause menu settings

**Rationale:** Audio is 50% of player immersion; MVP is playable without it.

**Estimated Effort:** 40-60 hours (composition, implementation, QA)

---

### 5.2 Visual Polish

**Current State:** Visual scale system implemented; sprites and animations functional.

**Residual Work:**
- [ ] Expand particle effects (combat: hit sparkles, spells; farming: dust, water)
- [ ] Add screen shake on impact events
- [ ] Implement camera zoom/pan on special events (boss spawn, level up)
- [ ] Polish UI transitions (fade, slide, bounce animations)
- [ ] Add visual feedback for status effects (color overlay, sprite tint)
- [ ] Optimize sprite assets (compression, atlasing)

**Rationale:** Polish improves perceived quality; MVP gameplay is not compromised without it.

**Estimated Effort:** 50-70 hours (asset creation, implementation, optimization)

---

### 5.3 UI/UX Refinement

**Current State:** UI system is functional; all modals work; input routing correct.

**Residual Work:**
- [ ] Add tooltips to all buttons (what does this do?)
- [ ] Add help/tutorial overlays (first-time player guidance)
- [ ] Improve error messages (clear action + reason for rejection)
- [ ] Add animations to inventory/equipment updates
- [ ] Refactor pause menu layout (grouping, visual hierarchy)
- [ ] Add settings persistence (audio volume, keybinds)

**Rationale:** UX polish improves usability; MVP navigation works without refinements.

**Estimated Effort:** 25-35 hours (design, implementation, QA)

---

## Priority 6: Content Expansion (Post-Launch)

### 6.1 Enemy Roster Expansion

**Current State:** 40 enemies implemented; roster covers 6 size profiles + 5 progression bands.

**Residual Work:**
- [ ] Design + implement 20-30 additional enemy types (boss variants, elite versions)
- [ ] Create new AI archetypes (ranged, healing, buff-granting)
- [ ] Balance new encounters + difficulty scaling
- [ ] Create unique loot tables for new enemies

**Rationale:** Content expansion drives engagement; MVP roster sufficient for initial playthrough.

**Estimated Effort:** 60-80 hours (design, implementation, art, balancing)

---

### 6.2 Skill Tree Expansion

**Current State:** 5 skill trees with 55 nodes; nodes are functional but basic.

**Residual Work:**
- [ ] Design + implement skill interactions (combos, synergies)
- [ ] Add legendary skills (high-tier nodes with unique mechanics)
- [ ] Implement skill gem system (modifiers for gems in equipment slots)
- [ ] Expand passive trees (more nodes, new mechanics)
- [ ] Balance new skills + progression pacing

**Rationale:** Skill variety drives replay value; MVP trees sufficient for initial run.

**Estimated Effort:** 50-70 hours (design, implementation, balancing)

---

### 6.3 World Expansion

**Current State:** Farm, Town, Cave as playable areas; world map exists but incomplete.

**Residual Work:**
- [ ] Design + implement 2-3 new world areas (forest, coastal zone, mountain)
- [ ] Create area-specific mechanics (weather, resources, enemies)
- [ ] Implement fast travel system (portal network)
- [ ] Add hidden/secret locations (bonus content)
- [ ] Create multi-area questline

**Rationale:** World expansion drives exploration; MVP three-area loop sufficient.

**Estimated Effort:** 80-120 hours (design, implementation, art, balancing)

---

## Priority 7: Known Technical Debt

### 7.1 Input Manager Migration

**Current State:** Legacy Input Manager in use; Input System available but migration not attempted.

**Residual Work:**
- [ ] Create separate SPEC for Input System migration (to avoid gameplay regression)
- [ ] Implement Input System bindings for all actions
- [ ] Test controller support (not in MVP)
- [ ] Migrate codebase incrementally (gameplay → UI → editor tools)
- [ ] Document migration status + fallback plan

**Rationale:** Input System is modern standard; legacy manager works but limits extensibility.

**Estimated Effort:** 30-50 hours (migration, testing, documentation)

---

### 7.2 Save System v6 (Optional Future Schema)

**Current State:** Save schema v5 stable; migrations v2→v5 working.

**Residual Work:**
- [ ] Monitor for breaking changes that require v6 (unlikely in near future)
- [ ] If needed: design v6 migration strategy (backward compatible)
- [ ] Document migration testing approach
- [ ] Plan version bump + release notes

**Rationale:** v5 is sufficient; documented for future extensibility.

**Estimated Effort:** 5-10 hours (if migration becomes necessary)

---

### 7.3 Cave Snapshot Persistence Optimization

**Current State:** Cave snapshots capture per-level state; reloading works; but large caves may have memory overhead.

**Residual Work:**
- [ ] Profile snapshot memory usage per cave level
- [ ] Consider compression strategy (if overhead is significant)
- [ ] Add snapshot pruning (delete old snapshots after cave exit)
- [ ] Document memory limits + expected sizes

**Rationale:** MVP snapshots work fine; optimization only needed if profiling reveals issues.

**Estimated Effort:** 10-20 hours (profiling, optimization, testing)

---

## Priority 8: Deferred to Future Phases

### 8.1 Multiplayer (FASE 11+)

- Networking architecture (peer-to-peer or dedicated server)
- Session management + synchronization
- Shared world state
- Player collision + interaction

**Estimated Effort:** 200+ hours (depends on architecture choice)

---

### 8.2 Controller Support (FASE 11+)

- Input System migration prerequisite
- Controller button layout + haptic feedback
- UI navigation (d-pad, analog sticks)
- Menu interaction polish

**Estimated Effort:** 30-40 hours (after Input System migration)

---

### 8.3 Localization (FASE 11+)

- String extraction from all scenes + code
- Translation pipeline + management
- RTL language support
- Font fallback for special characters

**Estimated Effort:** 100+ hours (depends on language count)

---

### 8.4 Advanced Graphics (FASE 12+)

- Shader improvements (lighting, water, effects)
- Post-processing pipeline (bloom, color grading, AA)
- Dynamic shadows + lighting
- Advanced particle systems

**Estimated Effort:** 80-120 hours (depends on scope + artist availability)

---

## Summary: Post-MVP Roadmap

| Priority | Category | Effort | Timeline | Blocker |
|----------|----------|--------|----------|---------|
| **0** | **Phase 2-3 Validation** | 2-3 hours | ASAP (human) | MVP acceptance |
| 1 | Play Mode Issue Fixes | Variable | Immediately post-Phase 3 | Depends on test results |
| 2 | Code Architecture | 50-75 hours | FASE 10 Q1 | No |
| 3 | Testing Coverage | 70-100 hours | FASE 10 Q1-Q2 | No |
| 4 | Gameplay Balance | 35-50 hours | FASE 10 Q2 | No |
| 5 | Polish & Presentation | 115-165 hours | FASE 10 Q2-Q3 | No |
| 6 | Content Expansion | 190-270 hours | FASE 11+ | No |
| 7 | Technical Debt | 45-80 hours | FASE 10-11 | No (Input migration recommended pre-multiplayer) |
| 8 | Future Phases | 500+ hours | FASE 11+ | No |

---

## Decision: Backlog Acceptance

**Backlog Status:** Approved as post-MVP residual work ✓

**Rationale:** No priority-1 items block MVP acceptance. Phase 2-3 outcome will determine if any fixes needed.

**Next Step:** Execute Phase 2-3 (human Play Mode validation) → update backlog with any failures → begin FASE 10 planning.

---

**Backlog Created:** 2026-06-01  
**Owner:** Project Lead + SPEC_29 Closure  
**Review Cycle:** Post-Phase 3 execution (update backlog if new issues found)  
**Archive Date:** To be determined post-Phase 3  

