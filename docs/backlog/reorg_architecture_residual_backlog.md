# Residual Backlog — Architecture Reorganization SPEC_04-11

**Consolidated:** 2026-06-01 (SPEC_12 closeout)  
**Status:** Code-complete; validation and scaling pending  
**Owner:** Architecture team  
**Priority:** Medium (code solid; no critical blockers)  

---

## Overview

SPEC_04-11 reorganizacao arquitetural esta implementada em codigo, compila sem erro, e foi validada quanto a compilacao C#. Porem, validacoes interativas (Play Mode, editor validators) e escalacao do padrao para domínios adicionais ficam pendentes.

Este documento lista debitos remanescentes sem mascarar completude.

---

## Categoria 1: Validacoes Ambientais

### 1.1 Unity Batchmode Licensing Issue

**Item:** Unity batchmode exits with code 1 due to licensing callback abort  
**Status:** BLOCKED BY ENVIRONMENT  
**Details:** C# compilation completed successfully (no error CS messages); wrapper script reports failure due to networking/licensing issues in sandbox  
**Fix:** Run in interactive Unity editor or resolve licensing in environment  
**Effort:** N/A (environment-specific)  
**Priority:** Medium (blocking automated validation; manual editor run works)  
**Target:** Future CI/CD improvement

### 1.2 Editor Validators Not Executed

**Item:** CindarsHope/Validate/Combat/Validate Projectile Prefabs, Combat Databases, reorg validators  
**Status:** NOT RUN (requires active editor)  
**Details:** These are interactive editor menu items; cannot run headless  
**What needs validation:**
- Prefab asset consistency (projectile, bow, arrow, fireball)
- Database cross-references (ItemDatabase, WeaponDatabase, SpellDatabase, StatusEffectDatabase)
- Scene bootstrap wiring (GameBootstrap field assignments in FarmScene, TownScene, CaveScene)

**Fix:** Open project in Unity editor; run validators from menu  
**Effort:** ~15 min (human task)  
**Priority:** High (validates infrastructure deployed in SPEC_04-11)  
**Target:** Manual validation checkpoint before Play Mode

### 1.3 Play Mode Validation Not Executed

**Item:** Full interactive gameplay checklist  
**Status:** NOT RUN (requires active editor + Play Mode)  
**Checklist items:**
- [ ] Open FarmScene
- [ ] Move player (WASD)
- [ ] Interact with tree (E)
- [ ] Interact with plot (E)
- [ ] Interact with lake/fishing (E)
- [ ] Open hotbar (number keys 1-6)
- [ ] Open inventory (I)
- [ ] Open equipment (C)
- [ ] Equip bow + arrow from inventory
- [ ] Fire arrow (Q)
- [ ] Verify arrow fires from arrow hand, not bow hand
- [ ] Equip fireball (or spell from hotbar)
- [ ] Fire fireball (Q)
- [ ] Confirm burn/DOT if enemy available
- [ ] Save game (Ctrl+S or menu)
- [ ] Load game (Ctrl+L or menu)
- [ ] Verify hotbar/equipment/inventory preserved after load
- [ ] Transition Farm → Town (portal)
- [ ] Transition Town → Farm (portal)
- [ ] Transition to Cave (if available)
- [ ] Verify no new critical errors in console log

**What needs validation:**
- Gameplay behavior unchanged (movement, hotbar, inventory, equip, Q/E, save/load)
- Regression testing on core MVP features
- No new exception/error logs during normal play
- Save/load round-trip preserves state

**Fix:** Manual Play Mode validation in interactive editor  
**Effort:** ~30 min (human task)  
**Priority:** Critical (final validation before production)  
**Target:** Human acceptance test

---

## Categoria 2: Asset/Wiring Dependencies

### 2.1 StatusEffectDatabase Asset Not Wired in Scenes

**Item:** StatusEffectDatabaseSO created in code (SPEC_09); asset and Inspector wiring pending  
**Status:** INCOMPLETE  
**Details:**
- Code: StatusEffectDatabaseSO.cs created (✓)
- Asset: Assets/_Game/Data/Combat/StatusEffectDatabase.asset — **not confirmed created**
- Wiring: GameBootstrap._statusEffectDatabase field — **not assigned in scenes**

**What happens if not wired:**
- EnemyStatusRuntimeTicker.Start() fallback to Resources.Load("status_burn_test")
- SpellCastService fallback to Resources.Load(statusEffectId)
- No error; fallback preserves behavior

**Fix:**
1. Create StatusEffectDatabase.asset in Unity editor (Assets/_Game/Data/Combat/)
2. Add status effect SOs to database (e.g., status_burn_test)
3. Assign asset to GameBootstrap._statusEffectDatabase in FarmScene, TownScene, CaveScene
4. Run editor validator to confirm

**Effort:** ~20 min (asset creation + wiring)  
**Priority:** Medium (fallback works; best-practice wiring preferred)  
**Target:** Editor asset wiring checkpoint

---

## Categoria 3: Architecture Scaling

### 3.1 Save Providers Pattern Not Scaled Beyond Hotbar

**Item:** ISaveSectionProvider pattern created; only HotbarSectionProvider implemented  
**Status:** PILOT PHASE  
**Details:**
- Interface: ISaveSectionProvider (✓)
- Pilot: HotbarSectionProvider delegating to HotbarState (✓)
- SaveManager integration: _hotbarProvider with capture/restore fallback (✓)
- Scalability: Pattern proven; ready for additional domains

**Domains awaiting provider extraction (in order of priority):**
1. Inventory (InventoryManager.CaptureSaveData / RestoreSaveData)
2. Equipment (EquipmentManager, equipped items state)
3. Player (PlayerManager vitals, position, level)
4. Progression (PlayerProgressionManager)
5. GameTime (GameTimeManager)
6. Farm (FarmPlot states, Trees)
7. World (global state)
8. Cave (CaveRunState snapshot management — already complex)
9. Economy (EconomyManager)
10. Crafting (CraftingManager)
11. Stamina (StaminaManager)
12. StatusEffects (PlayerStatusEffectsSaveData)
13. Bestiary (BestiaryManager)
14. SkillTree (SkillTreeManager)

**Fix:** Implement additional providers following HotbarSectionProvider pattern  
**Effort:** ~30 min per domain (straightforward delegation)  
**Priority:** Low (fallback inline SaveManager works; refactor scales architecture)  
**Target:** Future specs (SPEC_13+) incremental provider extraction

**Benefits of scaling:**
- Decouples SaveManager from individual domain details
- Enables parallel save/load operations per domain
- Simplifies testing (provider can be mocked)
- Clearer separation of concerns

### 3.2 CombatRuntimeInstaller Pattern Not Scaled Beyond Combat

**Item:** CombatRuntimeInstaller created as validation-only pilot; not scaled to other domains  
**Status:** PILOT PHASE  
**Details:**
- CombatRuntimeInstallContext + Installer (✓)
- Validation: ItemDatabase, WeaponDatabase, SpellDatabase, EquipmentManager (✓)
- Integration: GameBootstrap.Install() called in InitializeManagers() (✓)
- Extensibility: Pattern proven; ready for additional domains

**Domains awaiting installer pattern (optional; lowest priority):**
1. InventoryInstaller (validate InventoryManager, ItemDatabase)
2. SaveInstaller (validate SaveManager wiring)
3. TimeInstaller (validate TimeManager, GameTimeManager)
4. EconomyInstaller (validate EconomyManager)
5. CraftingInstaller (validate CraftingManager, recipe databases)
6. NPCInstaller (validate NpcManager, NpcDataSOs)

**Fix:** Create installers following CombatRuntimeInstaller pattern  
**Effort:** ~15 min per installer (validation checks only, no logic changes)  
**Priority:** Low (code works without explicit installer validation; best-practice improvement)  
**Target:** Future specs if desired for full bootstrap auditing

**Note:** Installers are validation/diagnostics only; they do NOT inject dependencies or alter initialization logic. This is a read-only pattern.

### 3.3 GameBootstrap Still Monolithic

**Item:** GameBootstrap holds ~50 fields, 30+ properties; installers validate but do not redistribute  
**Status:** KNOWN LIMITATION  
**Details:**
- Current: Single composition root with all managers and databases
- Pattern added: Installers validate subsets (combat pilot)
- Limitation: No structural changes; still one big class

**Future considerations (out of scope for reorg):**
1. CombatBootstrap (separate composition root for combat systems)
2. SaveBootstrap (separate composition root for save/load)
3. TimeBootstrap (separate composition root for time/day systems)
4. NPCBootstrap (separate composition root for NPC/dialogue systems)

**Fix:** Structural refactor to domain-specific bootstraps (larger effort, future phase)  
**Effort:** ~2-3 days (significant refactor; high risk if done carelessly)  
**Priority:** Very Low (current structure is stable and works; optimization only)  
**Target:** Future architecture evolution phase (post-MVP)

---

## Categoria 4: Validator Coverage Gaps

### 4.1 No Scene Validators for SPEC_05-08 Services

**Item:** MvpSceneValidator covers SPEC_09/SPEC_11 database checks; no validators for service wiring (SPEC_05-08)  
**Status:** GAP IDENTIFIED  
**Details:**
- SPEC_04: Quarantine (no validators needed)
- SPEC_05: CombatActionContext, EquippedItemResolver, CooldownHelper — **no validator**
- SPEC_06: ProjectileSpawnService — **no validator**
- SPEC_07: BowArrowAttackService, SpellCastService — **no validator**
- SPEC_08: ItemUseKind, ItemUseContractResolver — **no validator**
- SPEC_09: StatusEffectDatabaseSO — **validator added** ✓
- SPEC_10: ISaveSectionProvider, HotbarSectionProvider — **no validator needed** (not scene-dependent)
- SPEC_11: CombatRuntimeInstaller — **validator added** ✓

**What should be validated:**
- PlayerAttackController references to services (CombatActionContext resolver, EquippedItemResolver, etc.)
- FarmScene/TownScene/CaveScene have PlayerAttackController wired and services available
- ProjectileSpawnService accessible from SpellCastService
- Bow/arrow/fireball ItemUseKind contracts resolved

**Fix:** Add editor validators for service/contract wiring in scene  
**Effort:** ~1-2 hours (analyze dependencies, write validator methods)  
**Priority:** Low (services are wired via existing managers; no new breakage risk)  
**Target:** Future validator expansion phase

---

## Categoria 5: Documentation Gaps

### 5.1 No Architecture Diagram Post-Reorg

**Item:** No visual architecture diagram showing domain boundaries, contracts, and data flow after SPEC_04-11  
**Status:** MISSING  
**Details:** Existing docs describe individual specs but lack consolidated architecture visualization  
**Fix:** Create architecture diagram (Markdown ASCII or external tool)  
**Effort:** ~1-2 hours (draw and document)  
**Priority:** Low (documentation only; no code impact)  
**Target:** Knowledge base improvement

### 5.2 No Migration Guide for Future Spec Implementers

**Item:** How to add a new domain (installer, provider, validator, service)  
**Status:** IMPLIED (pattern exists but not documented)  
**Details:** SPEC_04-11 established pattern; new implementers should follow recipe  
**Fix:** Write guide: "Adding a New Domain to the Architecture"  
**Effort:** ~1 hour (document existing pattern)  
**Priority:** Low (nice-to-have for scalability)  
**Target:** Developer onboarding docs

---

## Summary by Priority

| Priority | Item | Effort | Target |
|----------|------|--------|--------|
| **Critical** | Play Mode validation | ~30 min | Human acceptance test |
| **High** | Editor validators (prefab, database, wiring) | ~15 min | Validation checkpoint |
| **Medium** | StatusEffectDatabase asset + wiring | ~20 min | Asset dependency fix |
| **Medium** | Unity licensing wrapper (batchmode) | N/A | CI/CD improvement |
| **Low** | Save provider pattern scaling | ~4.5 hours | Future specs |
| **Low** | Installer pattern scaling | ~1.5 hours | Future specs |
| **Very Low** | GameBootstrap monolithic structure | 2-3 days | Post-MVP evolution |
| **Low** | Service/contract validators (SPEC_05-08) | 1-2 hours | Validator expansion |
| **Low** | Architecture diagram | 1-2 hours | Knowledge base |
| **Low** | Migration guide | ~1 hour | Developer docs |

---

## Rollout Recommendation

### Immediate (within 1 sprint)
1. ✓ Play Mode validation (critical path to MVP approval)
2. ✓ Editor validators (validates deployed infrastructure)
3. StatusEffectDatabase asset + wiring (completes SPEC_09)

### Next Sprint
1. Save provider pattern scaling (architectural improvement)
2. Service/contract validators (closes coverage gap)

### Future Phases
1. Installer pattern scaling (optional; validation-only)
2. Architecture diagram + guide (documentation)
3. GameBootstrap refactor (large, lower priority)

---

## Conclusion

SPEC_04-11 reorganizacao arquitetural esta **code-complete**, **compila clean**, e **pronta para Play Mode validation**. Nenhum critical blocker. Residual backlog e claro, actionable, e bem-priorizado. Pattern para providers/installers esta proven e ready para scaling em future specs.

**Next mandatory step:** Play Mode human validation (30 min).  
**Next recommended step:** Editor validator audit (15 min).  
**Next architectural step:** StatusEffectDatabase wiring + save provider scaling (SPEC_13+).
