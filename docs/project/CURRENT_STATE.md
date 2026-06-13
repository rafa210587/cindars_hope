# Current State — Cindar's Hope

> **Primary execution context for agents.** Read this + AGENTS.md + active spec.  
> **Do NOT read:** PROJECT_LOG.md, ROADMAP.md, GDD, old refinements (unless spec requires).

---

## Project Status

| Item | Status |
|------|--------|
| Branch (working) | `dev` |
| MVP code-complete | ✓ YES |
| MVP build-validated (C#) | ✓ YES — 0E/0W runtime + editor |
| Legacy docs validation | ✓ YES — 25+ checks (SPEC_DOCS_37) |
| Generated specs validation | ⚠ RUN_WITH_ISSUES — naming/header mismatch (blocks WAVE 02+) |
| Legacy spec cleanup | ✓ YES — 7 specs absorbed (2026-06-07) |
| MVP Phase 2-3 (Unity/Play Mode) | ✗ DEFERRED TO FINAL ACCEPTANCE | Not required for WAVE 02 implementation start |
| MVP final accepted | ✗ NOT YET — pending Phase 2-3 execution |

---

## What to Read for Implementation Tasks

```
1. AGENTS.md
2. docs/project/CURRENT_STATE.md  (this file)
3. Active spec file
4. Source files referenced by the spec
5. Immediately prior validation report ONLY if listed as a dependency
```

## What NOT to Read by Default

```
- PROJECT_LOG.md  (use only for audit/reconciliation/regression)
- docs/project/ROADMAP.md  (planning only)
- docs/refinements/  (read only if spec is ambiguous)
- docs/specs/a_implementar/reorg/  (CLOSED — do not execute)
- docs/validation/spec_mvp_closeout_*  (evidence; read only if explicitly listed)
- docs/IMPLEMENTATION_STATUS.md  (broad status; read only for audit)
```

---

## Claude Code Harness

| Item | Status |
|------|--------|
| CLAUDE.md | Short router (~80 lines) — SPEC_CLAUDE_31 |
| AGENTS.md | Multi-agent rules — SPEC_CLAUDE_31 |
| `.claude/rules/` | 16 rules active — SPEC_DOCS_38/39 (incl. decision-and-game-rule-policy, legacy-doc-paths-forbidden) |
| `.claude/settings.json` | Updated with canonical paths and new hooks — SPEC_DOCS_39D |
| `.claude/commands/` | 11 commands (8 updated, 3 created) — SPEC_CLAUDE_31 |
| `.claude/skills/` | 15 skills (incl. decision-rule-extraction) — SPEC_DOCS_39B/39E |
| `.claude/agents/` | 7 agents (2 updated, 2 created) — SPEC_CLAUDE_31 |
| `.claude/hooks/` | 14 hooks (incl. decision-rule-reference-guard) — SPEC_DOCS_39B/39D |
| Decision Records | 9 ADRs (ADR-0001 to ADR-0009) — SPEC_DOCS_38 |
| Game Rules | 12 game_rules documents — SPEC_DOCS_38 |
| Docs validation | 25+ checks (validate_docs.ps1) — SPEC_DOCS_37/39C |
| Default context | `CURRENT_STATE.md` (not PROJECT_LOG.md) |
| Spec promotion | Phase-gated via `/finish-spec` |

---

## Active Spec Queue

| Spec | Status | Notes |
|------|--------|-------|
| SPEC_DOCS_31 | COMPLETE | Safe Batch 1 deletion (20 files); commit 956e4d1 |
| SPEC_DOCS_32 | PHASE 1 COMPLETE | Phase 0-1 complete: 6 obsolete files deleted; AGENTS.md updated; commit af8bb1b |
| SPEC_DOCS_33 | COMPLETE | Agent_prompts + agent_packages cleanup (39 files); commit 5c5ecae |
| SPEC_DOCS_34 | COMPLETE | Final legacy cleanup: 65 files (ARCH delta + orquestrador/); commit 3c6dda9 |
| SPEC_DOCS_35 | COMPLETE | Canonical folder consolidation: 5 numbered folders → canonical (14 files moved); commit [migração] |
| SPEC_DOCS_36 | COMPLETE | Final root/legacy cleanup: 159 files deleted (5 root + 86 docs_old + 68 prompts/templates/orquestrador); commit 9c4ffa5 |
| SPEC_DOCS_37 | COMPLETE | Refinements/specs/validation sweep: 12 references fixed, 14 refinements archived, 4 deleted, validation enhanced (25+ checks); commits b9ae4d5, 45e299f |
| SPEC_18-28 Phase 2-3 | PENDING HUMAN | Play Mode validation; requires local Unity Editor |
| SPEC_29 Phase 2-3 | PENDING HUMAN | Final acceptance; blocked on Phase 2-3 above |
| WAVE 00.04 Existing Implementation Audit | COMPLETE / BUILD_VALIDATED | Governance audit only; no code/gameplay changes; commit ee1c0fb+ |
| WAVE 01 Hardening & Quality Gate | CODE_COMPLETE | 9 specs executed (00.04, 01.01-01.07, 01Q); BUILD_VALIDATED; 36 EditMode tests compile successfully (0E/0W) |
| WAVE 02 Time/Calendar/Lunar/Weather/Rain/Save/Festivals | COMPLETED_WITH_DEFERRED_UI | 7 of 8 specs BUILD_VALIDATED; 1 deferred (Spec 3 Calendar UI); all runtime systems complete |
| WAVE 03 Quests/Objectives/Events | COMPLETED_WITH_DEFERRED_UI | 8 of 8 runtime specs BUILD_VALIDATED; 4 future specs blocked; quest system foundation complete; readiness check PASS |
| WAVE 04 UI Foundation Phase 1 | PHASE1_COMPLETED_WITH_CONTRACT_ONLY_CORE | 14 of 14 specs have execution reports; 3 BUILD_VALIDATED (SPECS 1-2, 8), 11 CONTRACT_ONLY (SPECS 3-7,9,11-16); SPEC 8 fully reworked (10 focus states, modal stack, 47 tests); Assembly builds PASS; quality check PASS; docs validation PASS (legacy errors only) |
| WAVE 05 Farm Gameplay Core | COMPLETED_WITH_KNOWN_LEGACY_GATES | 20/20 specs executed; all BUILD_VALIDATED with RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES; ~123 EditMode tests; Assembly-CSharp PASS; closeout: docs/validation/WAVE_05_CLOSEOUT_REPORT.md |
| WAVE 06 Economy/Loot/Crafting/Shop/Cave Foundation | COMPLETED_WITH_KNOWN_LEGACY_GATES | 8/8 specs executed; ~75 EditMode tests; Assembly-CSharp PASS; closeout: docs/validation/WAVE_06_CLOSEOUT_REPORT.md |
| WAVE 07 Playable Scene Integration | WAVE_INTEGRATION_10_BUILD_VALIDATED_WITH_UI_DEBT | Scene architecture documented; manager audit complete; FarmScene restored to WAVE03 baseline; resource interactables added; debug loadout provisioner (06A); economy loop wired via SellPoint (07); HUD feedback events wired (08); inventory/tooltip/equipment runtime binding (09): InventoryPanelController + CharacterEquipmentPanelController reused (RuntimeInitializeOnLoad), GameplayInputRouter added to CreateMvpFarmScene; human must run CreateMvpFarmScene generator and execute 06A + 07 + 08 + 09 Play Mode checklists |
| WAVE 08 City/NPC/Dialogue/Services | COMPLETED_WITH_KNOWN_LEGACY_GATES | 4/4 specs BUILD_VALIDATED; ~61 EditMode tests; Assembly-CSharp PASS; closeout: docs/validation/WAVE_08_CLOSEOUT_REPORT.md |
| WAVE 09 Quest System | COMPLETED_WITH_KNOWN_LEGACY_GATES | 8/8 specs BUILD_VALIDATED; ~107 EditMode tests; Assembly-CSharp PASS; closeout: docs/validation/WAVE_09_CLOSEOUT_REPORT.md |
| WAVE 10 Main Progression / Fonte / Endgame | COMPLETED_WITH_KNOWN_LEGACY_GATES | 4/4 specs BUILD_VALIDATED; ~72 EditMode tests; Assembly-CSharp PASS; closeout: docs/validation/WAVE_10_CLOSEOUT_REPORT.md |
| WAVE 11 UI / HUD / Inventory / Shop projections | COMPLETED_WITH_KNOWN_LEGACY_GATES | 4/4 specs BUILD_VALIDATED; ~96 new EditMode tests; closeout: docs/validation/WAVE_11_CLOSEOUT_REPORT.md |
| WAVE 12 Final Validation Docs / Reconciliation | COMPLETED_WITH_KNOWN_LEGACY_GATES | 2/2 docs specs BUILD_VALIDATED; closeout: docs/validation/WAVE_12_CLOSEOUT_REPORT.md |
| WAVE 13 Bestiary | BLOCKED_BY_FUTURE_SCOPE_MOVED_TO_FEATURES_FUTURAS | 4 specs moved to `docs/specs/a_implementar/features_futuras/`; do not execute without explicit human decision |
| WAVE 17-24 Future | BLOCKED INTENTIONALLY / MOVED_TO_FEATURES_FUTURAS | Future/expansion specs moved to `docs/specs/a_implementar/features_futuras/`; pets (WAVE 23) blocked as HOLD/BLOCKED_SCOPE |

---

## Future Specs Automation Rule

Specs futuras foram movidas para `docs/specs/a_implementar/features_futuras/` e nao sao executaveis por loop automatico.

Automation must ignore `docs/specs/a_implementar/features_futuras/` unless a human explicitly moves a spec back to `docs/specs/a_implementar/` and updates the registry.

---

## WAVE 07 - Playable Scene Integration

Status: WAVE_INTEGRATION_08_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

Latest validation:
- Assembly-CSharp: PASS (exit code 0, 0 warnings, 0 errors)
- Assembly-CSharp-Editor: PASS (exit code 0, 3 pre-existing warnings, 0 errors)
- Docs validation: EXPECTED_FAIL_LEGACY_ONLY (exit code 1, pre-existing governance/doc errors only)
- Scene inventory: CREATED (`docs/validation/WAVE_07_SCENE_INVENTORY.md`, 34 scenes found)
- Scene architecture decision: DOCUMENTATION_ONLY_HUMAN_UNITY_ACTION_REQUIRED
- Manager audit: VALIDATED (`docs/validation/WAVE_INTEGRATION_02_MANAGER_AUDIT.md`)
- Player/camera/movement baseline: VALIDATED_STRUCTURAL (`docs/validation/WAVE_INTEGRATION_03_PLAYER_CAMERA_MOVEMENT_REPORT.md`)
- FarmScene foundation zones: SCENE_REVERTED_TO_WAVE03 — code updated with corrected layout (`docs/validation/WAVE_INTEGRATION_04_FARMSCENE_REBUILD_REPORT.md` is historical; new layout in zone map v2)
- FarmScene crop interactable slice: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED (`docs/validation/WAVE_INTEGRATION_05_CROP_INTERACTABLE_REPORT.md`)
- FarmScene spatial reconciliation: CODE_READY (`docs/validation/WAVE_INTEGRATION_04_05_SPATIAL_RECONCILIATION_REPORT.md`)
- FarmScene resource interactables: CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED — P1 hotfix applied (`docs/validation/WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_REPORT.md`)
- Tree/Rock/Forage interactables: CODE_READY — depletion now only on AddItem success; ClampedAmount guard added; validator expanded
- LakeFishing interactable: EXISTING (FishingSpot.cs at (7.8, -2.8) fully implements IInteractable; no additional adapter needed)
- Reward strategy: INVENTORY_REWARD via InventoryManager.AddItem (same path as FarmPlot harvest)
- CreateMvpFarmScene.cs: UPDATED with resource interactable creation (TreeResource_01, RockResource_01, ForageResource_01)
- WAVE_INTEGRATION_06 decision: CREATED (`docs/validation/WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_DECISION.md`)
- Debug loadout provisioner (WAVE_INTEGRATION_06A): CODE_READY — Editor menus created; item use audit documented; 5 tool IDs unconfirmed in ItemDatabase (see audit)
- Debug loadout: CindarsHope/Integration/Debug/Provision Farm Smoke Loadout (Play Mode only; TODO_INTEGRATION_NOT_FINAL)
- Item use audit: STATIC_AUDIT_COMPLETE — see `docs/validation/WAVE_INTEGRATION_06A_DEBUG_LOADOUT_ITEM_USE_AUDIT.md`; WAVE_INTEGRATION_06B may be needed if tool IDs are absent
- Human Play Mode checklist (06A): `docs/validation/WAVE_INTEGRATION_06A_HUMAN_PLAYMODE_CHECKLIST.md`
- Human wiring instructions: CREATED (`docs/validation/WAVE_INTEGRATION_06_HUMAN_UNITY_RESOURCE_WIRING_INSTRUCTIONS.md`)
- Economy loop (WAVE_INTEGRATION_07): BUILD_VALIDATED_CODE_READY — REUSE_EXISTING_SHIPPING_RUNTIME strategy; SellPoint wired at Zone_ShippingSellpoint (3.5, 7.5); CreateSellPoint() added to CreateScene(); SellableItemPolicy confirmed: crops/materials/fish sellable; hoe/watering/pickaxe protected
- WAVE_INTEGRATION_07 decision: CREATED (`docs/validation/WAVE_INTEGRATION_07_SHIPPING_ECONOMY_DECISION.md`)
- WAVE_INTEGRATION_07 report: CREATED (`docs/validation/WAVE_INTEGRATION_07_SHIPPING_ECONOMY_REPORT.md`)
- Human Play Mode checklist (07): `docs/validation/WAVE_INTEGRATION_07_HUMAN_PLAYMODE_CHECKLIST.md`
- Human wiring instructions (07): `docs/validation/WAVE_INTEGRATION_07_HUMAN_UNITY_SHIPPING_WIRING_INSTRUCTIONS.md`
- HUD binding (WAVE_INTEGRATION_08): BUILD_VALIDATED_WITH_UI_DEBT — REUSE_EXISTING_DEBUG_HUD strategy; DebugHud already covers prompt/gold/feedback/inventory/stamina/hotbar via IMGUI; SellPoint now publishes PlayerActionFeedbackEvent ("Vendido! +X ouro.") + EconomyTransactionCompletedEvent for HUD feedback; active skill slots deferred (ACTIVE_SKILL_RUNTIME_DEFERRED_TO_WAVE_INTEGRATION_10_11)
- WAVE_INTEGRATION_08 decision: CREATED (`docs/validation/WAVE_INTEGRATION_08_HUD_RUNTIME_BINDING_DECISION.md`)
- WAVE_INTEGRATION_08 report: CREATED (`docs/validation/WAVE_INTEGRATION_08_HUD_RUNTIME_BINDING_REPORT.md`)
- Human Play Mode checklist (08): `docs/validation/WAVE_INTEGRATION_08_HUMAN_PLAYMODE_CHECKLIST.md`
- Inventory/tooltip/equipment binding (WAVE_INTEGRATION_09): BUILD_VALIDATED_WITH_UI_DEBT — REUSE_EXISTING_CONTROLLERS strategy; InventoryPanelController (IMGUI, I key, ModalManager push, shows real slots) and CharacterEquipmentPanelController (K/L keys, equipment/attributes) already fully implemented via RuntimeInitializeOnLoadMethod; GameplayInputRouter added to CreateMvpFarmScene.cs; tooltip is minimal (itemId+amount); equipment panel does not push ModalManager (UI debt); ValidateInventoryRuntimeBinding.cs created
- WAVE_INTEGRATION_09 decision: CREATED (`docs/validation/WAVE_INTEGRATION_09_INVENTORY_TOOLTIP_EQUIPMENT_DECISION.md`)
- WAVE_INTEGRATION_09 report: CREATED (`docs/validation/WAVE_INTEGRATION_09_INVENTORY_TOOLTIP_EQUIPMENT_REPORT.md`)
- Human Play Mode checklist (09): `docs/validation/WAVE_INTEGRATION_09_HUMAN_PLAYMODE_CHECKLIST.md`
- Skill tree + active slot equip (WAVE_INTEGRATION_10): BUILD_VALIDATED_WITH_UI_DEBT — REUSE_EXISTING_CONTROLLERS strategy; SkillTreeGameplayPanelController (IMGUI singleton, RuntimeInitializeOnLoadMethod) already fully implemented (U key, open/close, 55 nodes/5 trees via DefaultSkillCatalog, purchase, active slot R/T/Y/G); fixed: GameplayInputRouter U→SkillTreeOpenedEvent now subscribed by panel (was gap); DebugHud now shows active skill slots (DrawActiveSkillSlots added); ValidateSkillTreeRuntimeBinding.cs created; Canvas SkillTreePanel/SkillTreeInputHandler deferred (prefab not wired); HUDGameplayViewModel.ActiveSkillSlots wiring deferred to WAVE_INTEGRATION_11
- WAVE_INTEGRATION_10 decision: CREATED (`docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_DECISION.md`)
- WAVE_INTEGRATION_10 report: CREATED (`docs/validation/WAVE_INTEGRATION_10_SKILL_TREE_ACTIVE_EQUIP_REPORT.md`)
- Human Play Mode checklist (10): `docs/validation/WAVE_INTEGRATION_10_HUMAN_PLAYMODE_CHECKLIST.md`
- Can start WAVE_INTEGRATION_11: COMPLETE (BUILD_VALIDATED 2026-06-08); see WAVE_INTEGRATION_11_skill_effects_gameplay_bridge_execution_report.md
- WAVE_INTEGRATION_11: BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE (2026-06-09) — Skill effects pipeline wired to numeric 1-4 runtime use; active slot storage accepts SkillActionId or SkillNodeId and revalidates purchased/equippable node; Dash/Dodge/Block are non-slot movement actions attached to real `PlayerController` at runtime via `PlayerMovementActionRuntimeBootstrap` (second fix 2026-06-09: bootstrap now resolves PlayerController.gameObject, not PlayerManager.gameObject; WaitForFixedUpdate sync; ShouldIgnoreHit self-collision filter; arrow mapping corrected; input helper added); Dash = Space+direction/facing fallback, 4.0 units, 0.22s, 40 stamina; Dodge = double-tap direction, 1.8 units, 0.32s, 40 stamina; Block = Left Shift 0.45x slow, 18 stamina/s drain; report: `docs/validation/WAVE_INTEGRATION_11B_MOVEMENT_ACTIONS_SECOND_FIX_REPORT.md`; human must execute 06A + 07 + 08 + 09 + 10 + 11 Play Mode checklists before ACCEPTED/WAVE12/WAVE13 continuation
- WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH: BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT (2026-06-08) — +14 nodes no DefaultSkillCatalog (3 Melee, 2 Magic, 5 Survival, 4 Crafting); FeedbackOnlySkillEffectExecutor para 14 novos EffectIds; 4 docs novos (BALANCE_ADDENDUM, SLOT_MAPPING, PASSIVE_MODIFIER_MAPPING, MOVEMENT_ACTIONS_MAPPING); catálogo total: 69 nodes / 34 active slot skills; Assembly-CSharp PASS; Assembly-CSharp-Editor PASS; superseded by 2026-06-09 runtime input fix for Play Mode wiring
- WAVE_INTEGRATION_12: BUILD_VALIDATED_WITH_NPC_DIALOGUE_SHOP_DEBT_PENDING_HUMAN_PLAYMODE (2026-06-09 corrective pass) — TownScene now contains 7 MVP/playable-slice NPCs placed and wired through existing `NpcDataSO`, `NpcController`, `NpcShopController`, `DialogueModal`, `ShopMenuModal`, `BuyPanel`, `SellPanel`, `ShopManager`, `InventoryManager`, and `PlayerManager`; Pip, Sylveth, Brumdar, Renko, Thalindra, Zrix, and Nimble have canonical scene placement markers and dialogue coverage with at least 10 nodes each; Sylveth, Brumdar, Renko, and Zrix are wired to real shop/service assets; no final quest/social/reputation/romance/companion/pet systems were created; no class-per-NPC implementation was added; human must execute WAVE12 Play Mode checklist before ACCEPTED/WAVE13 continuation
- WAVE_INTEGRATION_12C: BUILD_VALIDATED_WITH_REFINED_NPC_DEBT_PENDING_HUMAN_PLAYMODE (2026-06-09) — TownScene now contains all 23 refined canonical city NPCs from `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` with `NpcDataSO`, `DialogueTreeSO` >=10 nodes, `NpcScenePlacementMarker`, purpose/zone/movement profile docs, and 20 shop/service definitions; 20 NPCs are scene-wired through existing `NpcShopController`; Pip, Nimble, and Thalindra use basic shop runtime with advanced service UI debt; Alaric, Liora, and Maelor are dialogue-only per initial no-shop mapping; no WAVE13/14/15, final quest, social, romance, reputation, companion, pet, or class-per-NPC implementation was created; Unity batchmode generation blocked by already-open editor, so human Play Mode remains required before ACCEPTED/WAVE13 continuation
- WAVE_INTEGRATION_12C_SHOP_PRICE_FIX: BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE (2026-06-09) — shop price overrides applied for `item_material_stone` and `item_material_copper_ore`; no ShopManager code relaxation; Pip, Nimble, and Thalindra are reconciled as `SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT`; human must re-run TownScene Play Mode checklist before ACCEPTED/WAVE13 continuation
- WAVE_INTEGRATION_13: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (2026-06-09) — SceneId/SceneTransitionRequest/SceneTransitionResult/SceneTransitionGate/SceneSpawnAnchor/SceneTransitionRouter/PlayerSpawnResolver created; ValidateSceneTransitions editor validator created; 11 required docs + 2 optional human-instruction docs created; FarmScene/TownScene/CaveScene require human wiring (gates + anchors + PlayerSpawnResolver + Build Settings via Unity Editor); existing SceneTransitionStartedEvent/SceneTransitionCompletedEvent/SceneTransitionState/ScenePortal/SceneSpawnInstaller/SceneSpawnPoint reutilized; state preservation debt documented; Play Mode blocked until human wiring applied; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/0W)
- WAVE_INTEGRATION_14: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (2026-06-10) — USE_EXISTING_CRAFTING_RUNTIME strategy: CraftingPoint/CraftingRuntime/CraftingStation/CraftingJob/CraftingModal/RecipeDataSO/RecipeDatabaseSO/WorkshopType/CraftingEvents all reutilized; new: CraftingStationRuntimeBootstrap (wires CraftingRuntime to InventoryManager+StaminaManager at AfterSceneLoad) + ValidateCraftingProcessingRuntimeBinding (editor validator); 6 required docs + 2 optional human-instruction docs created; FarmScene requires human wiring (CraftingPoint GameObjects + CraftingRuntime manager + CraftingModal Canvas + RecipeDatabaseSO wiring); processing job save/load FULLY IMPLEMENTED (no debt); smoke test recipes TEMPORARY_CRAFTING_TEST_RECIPE (confirmed existing: recipe_workbench_processed_wood, recipe_forge_iron_sword); Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/0W); Play Mode blocked until human wiring applied
- WAVE_INTEGRATION_15: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (2026-06-10) — USE_EXISTING_WAVE09_QUEST_FOUNDATION strategy: QuestDefinition/QuestState/QuestStateRecord/QuestStateSection/QuestRewardApplicator/QuestFlagService all reutilized from WAVE09; new: QuestRegistry (in-memory catalog + smoke test quest_first_supplies_for_cindar), QuestService (accept/progress/turn-in with idempotent rewards via GrantedRewardIds), QuestInventoryAdapter, QuestGoldAdapter, QuestProgressEventBridge (InventoryChangedEvent+ItemCraftedEvent), QuestGiverInteractable (IInteractable, npc_thalindra), QuestBoardInteractable, QuestOfferPanelController+QuestLogPanelController+QuestLogRuntimeBinder (IMGUI headless, J key), QuestRuntimeBootstrap (RuntimeInitializeOnLoadMethod singleton), QuestRuntimeEvents (5 typed events); 7 docs created; quest_first_supplies_for_cindar registered (CollectItem wood x2 + stone x2 → Gold 50 + flag); reward idempotency via GrantedRewardIds; QUEST_SAVE_LOAD_IN_MEMORY (SaveManager integration deferred); npc_thalindra needs QuestGiverInteractable wiring in TownScene; Assembly-CSharp PASS (0E/3W pre-existing); Assembly-CSharp-Editor PASS (0E/3W pre-existing); Play Mode blocked until human wiring applied
- WAVE_INTEGRATION_16: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (2026-06-10) — USE_EXISTING_CAVE_RUNTIME strategy: CaveRunManager/CaveLevelRuntimeController/CaveExitPortal/CaveRuntimeState/CaveSpawnAnchor all reutilized from cave_001-008 specs; new: CaveEntranceInteractable (IInteractable, uses SceneTransitionRouter WAVE13), CaveRuntimeBridge (RuntimeInitializeOnLoadMethod singleton, publishes CaveExitedEvent, validates cave entry), CaveRunStartedEvent, CaveExitedEvent, ValidateWave16CaveEntranceRuntimeBridge (editor validator); 8 docs created (decision, report, scene audit, bridge report, route map, state preservation matrix, human checklist, human wiring instructions); FarmScene/CaveScene require human wiring (CaveEntranceInteractable on Zone_CaveEntrance, spawn_farm_from_cave anchor, CaveEntryController + CaveExitPortal in CaveScene); state preservation: Inventory/Gold/Quest/Health LIKELY_YES (DontDestroyOnLoad bootstrap); cave run state cached via GameBootstrap.SetCachedCaveRunState (stable run ADR-0005 compliant); CAVE_RUN_SAVE_LOAD_DEBT (SaveManager integration deferred WAVE18); combat/loot: deferred WAVE17; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/0W); Play Mode blocked until human wiring applied
- WAVE_INTEGRATION_17: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED (2026-06-10) — Cave First Combat + Loot Extraction Loop; USE_EXISTING strategy for all major systems: PlayerAttackController (Q/E melee, stamina guard, modal guard), EnemyHealth (TakeDamage→Die→EnemyKilledEvent), EnemyChaseController (idle→detect→chase), EnemyContactDamage (trigger-based), EnemyDropSpawner (EnemyKilledEvent→InventoryManager.AddItem), InventoryManager.AddItem; new: CaveSmokeTestSpawnerBridge.cs (thin MonoBehaviour spawns 1 smoke enemy using existing components in CaveScene), ValidateWave17CaveCombatLootLoop.cs (editor validator); NO parallel combat/loot/inventory systems created; loot item: item_material_stone via enemy_slime_basic.dropItemId; drop idempotency: EnemyHealth.Die() called once, enemy SetActive(false); extraction preservation: InventoryManager is DontDestroyOnLoad; PICKUP_SAVE_LOAD_DEBT deferred WAVE18; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/0W); human must: place CaveSmokeTestSpawnerBridge in CaveScene, assign enemy_slime_basic asset, place EnemyDropSpawner in CaveScene, verify dropItemId; report: docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md; checklist: docs/validation/WAVE_INTEGRATION_17_HUMAN_PLAYMODE_CHECKLIST.md
- WAVE_INTEGRATION_18: BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED_PENDING_HUMAN_PLAYMODE (2026-06-10) — GAP_CLOSURE spec; SaveManager NOT rewritten; SchemaVersion=5 unchanged; no new migration; MAIN GAP CLOSED: QuestStateSection now persisted via new [Serializable] QuestStateSectionSaveData/QuestStateSaveData/QuestObjectiveStateSaveData DTOs; GameSaveData.Quests field added; SaveManager.CaptureQuestSaveData + RestoreQuestSaveData wired in SaveGame/ApplySaveData; QuestRuntimeBootstrap.CaptureSaveData/RestoreFromSaveData/SetPendingSaveData static APIs added (with _pendingSaveData for pre-initialization restore); QuestService.RestoreFromSaveData added (preserves GrantedRewardIds → reward idempotency after load); NPC save: ALREADY_IMPLEMENTED; CaveRun save: ALREADY_IMPLEMENTED; enemy/loot: DROP_DIRECT_TO_INVENTORY (WAVE17) → loot in inventory (persisted), enemy HP save = CAVE_ENEMY_HP_SAVE_DEBT; scene-bound preservation: VALIDATED (all section captures fall back to existingSaveData); editor validator ValidateWave18SaveLoadGapClosure.cs created; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/3W pre-existing); human must execute HUMAN_PLAYMODE_CHECKLIST to verify quest round-trip in Unity Play Mode; report: docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md; checklist: docs/validation/WAVE_INTEGRATION_18_HUMAN_PLAYMODE_CHECKLIST.md
- WAVE_INTEGRATION_19: BUILD_VALIDATED_HUD_UX_ACCEPTANCE_READY_PENDING_HUMAN_PLAYMODE (2026-06-10) — HUD/UX Polish + Playable Slice Acceptance Gate; GAP_CLOSURE spec; HUD/UI NOT recreated; ALREADY_IMPLEMENTED: ModalManager stack, Dash/Dodge/Block/DoubleTap modal guards, InventoryPanel (ModalType.Inventory), EquipmentPanel (ModalType.CharacterEquipment), SkillTreePanel (ModalType.SkillTree), QuestOfferPanel (ModalType.QuestOffer), DialogueModal, ShopMenuModal, Thalindra quest option in NpcShopController; DELTA: GameLoadedEvent criado; ModalType.QuestLog adicionado ao enum; QuestLogPanelController.Open/Close com PushModal/TryPopModal(ModalType.QuestLog); DebugHud com 10 novas subscriptions (GameSavedEvent/GameLoadedEvent/5 quest events/CaveLevelEnteredEvent/CaveExitedEvent/EnemyKilledEvent → timed feedback messages); ValidateWave19HudUxAcceptanceGate.cs (22 checks); 7 docs criados; Debts explícitos: WAVE16 scene wiring (cave entrance/exit), WAVE17 combat wiring (CaveSmokeTestSpawnerBridge), WAVE18 PlayMode pendente, CAVE_ENEMY_HP_SAVE_DEBT; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/3W pre-existing); Can continue WAVE20: YES; report: docs/validation/WAVE_INTEGRATION_19_HUD_UX_ACCEPTANCE_REPORT.md; checklist: docs/validation/WAVE_INTEGRATION_19_HUMAN_PLAYMODE_CHECKLIST.md (40 steps)
- WAVE_INTEGRATION_26: BUILD_VALIDATED_QUESTLINE_OBJECTIVE_VARIETY_READY_PENDING_HUMAN_PLAYMODE (2026-06-11) — MVP+ 04: Questline Expansion + Objective Variety; full audit of existing quest system: QuestService/QuestRegistry/QuestRuntimeBootstrap/QuestProgressEventBridge/QuestGiverInteractable/QuestOfferPanel/QuestLogPanel/QuestStateSection/WAVE18 save-load all confirmed EXISTING; quest_first_supplies_for_cindar (Q1) ALREADY_IMPLEMENTED; delta: QuestRegistry +2 quests (quest_tools_for_the_town/SellItem/npc_pip + quest_echo_from_the_cave/ReachCaveDepth/npc_maelor), QuestService +6 handlers (OnCropHarvested/OnItemSold/OnNpcTalkedTo/OnCaveLevelEntered/OnEnemyKilled/ProgressObjectiveCount), QuestProgressEventBridge +5 subscriptions (CropHarvestedEvent/EconomyTransactionCompletedEvent/NpcInteractionStartedEvent/CaveLevelEnteredEvent/EnemyKilledEvent), ValidateWave26 (42 checks), 8 docs (decision + 6 matrices + report + checklist); SCENE_WIRING_DEBT (npc_pip and npc_maelor need QuestGiverInteractable in TownScene); PREREQUISITE_UI_DEBT (QuestGiverInteractable does not enforce PrerequisiteQuestIds); reward idempotency: PASS (GrantedRewardIds dual protection); save/load: PASS (WAVE18); 7 objective types handled (CollectItem/SellItem/ReachCaveDepth + HarvestCrop/TalkToNpc/DefeatEnemy/CraftItem wired for future); Assembly-CSharp PASS 0E/0W; Assembly-CSharp-Editor PASS 0E/3W pre-existing; human Play Mode checklist: docs/validation/WAVE_INTEGRATION_26_HUMAN_PLAYMODE_CHECKLIST.md; report: docs/validation/WAVE_INTEGRATION_26_QUESTLINE_OBJECTIVE_VARIETY_REPORT.md
- WAVE_INTEGRATION_25: BUILD_VALIDATED_WITH_SCENE_WIRING_DEBT_AND_TIME_BLOCK_DEBT_PENDING_HUMAN_PLAYMODE (2026-06-11) — MVP+ 03: Town NPC Schedules + Dialogue Expansion; full audit of existing NPC system: NpcManager/NpcController/NpcShopController/NpcWanderer/NpcScenePlacementMarker/City.Schedule all confirmed EXISTING; 23 canonical NPCs from WAVE12C with NpcDataSO/DialogueTreeSO(>=10 nodes)/placement all confirmed; Thalindra quest+buy+sell+adeus ALREADY_IMPLEMENTED; save/load (HasMet+position) ALREADY_IMPLEMENTED; modal/input guard ALREADY_IMPLEMENTED; delta: 6 new Schedule files (NpcScheduleProfile/Block/Anchor/RuntimeState/Service/RuntimeBootstrap), NpcTownRosterRegistry, NpcDialogueSetRegistry, NpcDialogueExpansionBootstrap, ValidateWave25 (42 checks); TIME_BLOCK_DEBT (schedule per-day only); SCENE_WIRING_DEBT (NpcScheduleAnchor not placed in TownScene); Assembly-CSharp PASS 0E/0W; Assembly-CSharp-Editor PASS 0E/3W pre-existing; 9 matrix docs + 2 closeout docs created; human Play Mode checklist: docs/validation/WAVE_INTEGRATION_25_HUMAN_PLAYMODE_CHECKLIST.md; report: docs/validation/WAVE_INTEGRATION_25_TOWN_NPC_SCHEDULES_DIALOGUE_REPORT.md
- WAVE_INTEGRATION_24: BUILD_VALIDATED_FARM_LOOP_DEPTH_READY_PENDING_HUMAN_PLAYMODE (2026-06-11) — MVP+ 02: Farm Loop Depth + Daily Goals; full farm audit performed — FarmPlot/FarmPlotRegistry/InventoryManager/EconomyManager/SaveManager/GameplayFeedbackService all reused; delta: 3 new event types (DailyGoalProgressedEvent, DailyGoalCompletedEvent, CropWateredEvent), 7 new Farm/Runtime scripts (FarmDailyGoalService, FarmDailyGoalDefinition, FarmDailyGoalState, FarmDailyGoalsSaveData, FarmDailyGoalRuntimeBootstrap, FarmLoopFeedbackBridge, ShippingSummaryService), 1 editor validator (ValidateWave24FarmLoopDepth), FarmDailyGoalsSaveData field added to GameSaveData; 2 daily goals (daily_goal_first_harvest + daily_goal_sell_first_crop) tracked via GameEventBus (CropHarvestedEvent + EconomyTransactionCompletedEvent), reset on DayStartedEvent; DontDestroyOnLoad singleton; SAVE_LOAD_DAILY_GOAL_DEBT (SaveManager not yet wired to CaptureSaveData/RestoreFromSaveData — P2 debt); no scene edits; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/3W pre-existing); report: docs/validation/WAVE_INTEGRATION_24_FARM_LOOP_DEPTH_REPORT.md; checklist: docs/validation/WAVE_INTEGRATION_24_HUMAN_PLAYMODE_CHECKLIST.md
- WAVE_INTEGRATION_23: BUILD_VALIDATED_HUD_CANVAS_READY_PENDING_HUMAN_PLAYMODE (2026-06-10) — MVP+ 01: UI/HUD Canvas Finalization; RuntimeInitializeOnLoadMethod bootstrap (GameplayHudBootstrap) cria GameplayHudCanvas DontDestroyOnLoad; GameplayHudRuntimeBinder subscreve HPChangedEvent/StaminaChangedEvent/ManaChangedEvent/HungerChangedEvent/InteractionPromptChangedEvent/QuestEvents → atualiza GameplayHudViewModel; GameplayFeedbackService gerencia fila priorizada de toasts (PlayerActionFeedbackEvent/GameSavedEvent/QuestEvents/EnemyKilledEvent/CaveEvents/NotificationToastRequestedEvent); HudVisibilityController polling ModalManager.HasActiveModal → publica HudVisibilityChangedEvent; 6 views headless criadas (StatusBarsHudView/QuestTrackerHudView/ActiveSkillSlotsHudView/InteractionPromptHudView/FeedbackToastHudView/ModalBlockerHudView) — visuais Canvas deferidos para wiring humano no Unity Editor; FinalHudGuardValidator usado em ActiveSkillSlotsHudView; DebugHud IMGUI mantido sem alterações; ValidateWave23UiHudCanvasFinalization.cs (27 checks); 7 docs criados; Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/4W pre-existing); Human Play Mode: executar WAVE_INTEGRATION_23_HUMAN_PLAYMODE_CHECKLIST.md; next: humano executa Play Mode sub-slice + wiring Canvas visual → MVP_PLUS_02 (farm crafting) ou MVP_PLUS_04 (equipment modal guard fix trivial); report: docs/validation/WAVE_INTEGRATION_23_UI_HUD_CANVAS_REPORT.md; checklist: docs/validation/WAVE_INTEGRATION_23_HUMAN_PLAYMODE_CHECKLIST.md
- WAVE_INTEGRATION_22: DEBT_BACKLOG_CONSOLIDATED_NEXT_ROADMAP_READY (2026-06-10) — Post-MVP P2/P3 Debt Backlog + Next Roadmap Handoff; DOC_ONLY; P0=0; P1 código=0; P1 WIRING=4 (DEBT-SCENE-001-004); 21 debts canônicos (13 P2 + 4 P3 + 1 ACCEPTED + 3 FIX/NEEDS_DECISION); 11 duplicidades resolvidas; 6 debts obsoletos removidos; 10 spec candidatas (MVP_PLUS_00-10); release candidate notes + handoff criados; ValidateWave22DebtBacklog.cs (18 checks); Assembly-CSharp-Editor PASS 0E; Can start MVP+: YES (após Play Mode sub-slice humano); next recommended: MVP_PLUS_00 (wiring humano) → MVP_PLUS_04 (modal fix trivial); report: docs/validation/WAVE_INTEGRATION_22_DEBT_BACKLOG_REPORT.md
- WAVE_INTEGRATION_21: NO_OP_NO_P0_P1_FOUND (2026-06-10) — Post-Acceptance P0/P1 Bugfix; P0=0; P1 código=0; P1 HUMAN_WIRING_REQUIRED=4 (B001-B004; B005 reclassificado P2); nenhum código alterado; 6 docs + 1 validator criados; builds passam; playable slice permanece BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE; next: humano executa sub-slice WAVE20 checklist → ACCEPTED_WITH_DEBT; report: docs/validation/WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md
- WAVE_INTEGRATION_20: BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE (2026-06-10) — Playable Slice Acceptance Checklist + Closeout; WAVE20 é closeout spec — sem feature nova; WAVE18+19 gates satisfeitos; WAVE13-19 reports todos auditados e existem; P0=0; P1=5 (TODOS HUMAN_WIRING_REQUIRED, não code bugs: scene gates/anchors WAVE13, FarmScene layout WAVE05/07, CaveEntranceInteractable WAVE16, CaveSmokeTestSpawnerBridge WAVE17, CraftingStation WAVE14); P2=8 (UI visual, save debts, event proxies); P3=3 (tooltip, modal guard parcial, art); sub-slice sem wiring manual: boot+player+inventory+skill tree+dash/dodge/block+save/load+modal guards (testável imediatamente); 7 docs obrigatórios criados; ValidateWave20PlayableSliceAcceptance.cs (35 checks); Assembly-CSharp PASS (0E/0W); Assembly-CSharp-Editor PASS (0E/3W pre-existing); MVP playable slice: BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE; Human Play Mode: executar FINAL_HUMAN_PLAYMODE_CHECKLIST.md (56 steps); Status após Play Mode sub-slice esperado: ACCEPTED_WITH_DEBT; report: docs/validation/WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md; checklist: docs/validation/WAVE_INTEGRATION_20_FINAL_HUMAN_PLAYMODE_CHECKLIST.md

---

## Deferred Final Acceptance Gates

These do NOT block WAVE 05 execution but are required for final project acceptance:

- **PlayMode validation (Phase 2-3):** Required for final MVP acceptance; deferred to post-integration gate
- **Human validation:** Required for final MVP acceptance; deferred to post-integration gate
- **Spec promotion to implementados/:** Only after Phase 2-3 evidence collected

**Current status:** WAVE 04 Phase 1 BUILD_VALIDATED; Phase 2-3 intentionally deferred to integration closeout.

---

## Key File Locations

| What | Where |
|------|-------|
| Agent rules | `CLAUDE.md` (router), `AGENTS.md` (rules), `.claude/rules/RULES.md` |
| Active specs | `docs/specs/a_implementar/` (147 wave-based) |
| Absorbed legacy specs | `docs/specs/absorvidas/legacy_pre_wave_reconciliation/` (7 specs) |
| Commands | `.claude/commands/` (11 commands) |
| Skills | `.claude/skills/` (14 skills) |
| Validation evidence | `docs/validation/spec_mvp_closeout_*.md` |
| MVP acceptance | `docs/release/MVP_ACCEPTANCE_REPORT.md` |
| Post-MVP backlog | `docs/backlog/post_mvp_backlog.md` |
| Last validation status | `docs/validation/current/LAST_VALIDATION_STATUS.md` |
| Spec template | `docs/specs/_templates/SPEC_TEMPLATE.md` |
| Document governance | `docs/project/DOCUMENT_GOVERNANCE.md` |
| Decision log | `docs/project/DECISION_LOG.md` |
| ADRs | `docs/decisions/` (9 ADRs, ADR-0001 to ADR-0009) |
| Game rules | `docs/game_rules/GAME_RULES_INDEX.md` |

---

## Reading Policy for ADRs and Game Rules

- **Agents do NOT read all ADRs or all game_rules by default.**
- **Agents read only ADRs/game_rules explicitly listed in the active spec's `required_adrs:` and `required_game_rules:` fields.**
- **If a spec conflicts with an ADR or game_rule, STOP and report the conflict. Do not implement until reconciled.**
- **See `DECISION_LOG.md` for index of all ADRs and reading policy details.**
- **See `docs/game_rules/GAME_RULES_INDEX.md` for index of all game rules.**

---

## Do NOT Execute

```
- docs/specs/absorvidas/legacy_pre_wave_reconciliation/** (legacy specs — moved 2026-06-07)
- docs/specs/a_implementar/reorg/SPEC_00-12  (CLOSED per README_STATUS.md)
- SPEC_18-29 again  (already executed; Phase 0-1 complete)
```

**Legacy specs moved (2026-06-07):** All 7 legacy specs from pre-wave era absorbed into new wave-based specs.  
See `docs/specs/absorvidas/legacy_pre_wave_reconciliation/LEGACY_SPECS_CROSSWALK.md`

---

## Conflict Resolution Rules

```
spec + roadmap conflict  → follow spec
spec + refinement conflict → follow spec
spec + CURRENT_STATE conflict → STOP and report inconsistency
PROJECT_LOG + CURRENT_STATE conflict → prefer CURRENT_STATE; report mismatch
```

---

- FABLE_REDUNDANCY_SWEEP: COMPLETE (2026-06-12) — varredura profunda autorizada pelo humano; 16 arquivos DELETADOS via batch FABLE-2 do DOCUMENT_DELETE_CANDIDATES (build_logs.zip 8MB, CHECKLIST_PR001, validate_quick.py, BACKLOG.md raiz, compile log solto, FARM_DESIGN v1.0 superada, arquivo solto docs/validation/playmode, FIX_001 stale, 8 duplicatas 03_spec_quest_*); pasta playmode/ correta criada com o cenário humano dentro; MOJIBAKE reparado em 17 arquivos (12 da sessão + 5 históricos, incl. triplo-encoding no docs_33); SPEC_EXECUTION_ORDER reconciliado (fila histórica; ativa = fable/); CLAUDE.md tabelas completadas (+5 skills, +4 commands); paths mortos anotados em 2 commands + 1 rule; questionário marcado RESPONDIDO; hooks auditados (15/15 registrados, zero órfãos); docs validation exit 0
- FABLE_CONTENT_CATALOGS: CANONIZED (2026-06-12) — questionário de decisões respondido pelo humano (registro vinculante: docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md); 6 catálogos canônicos gerados e registrados no SPEC_SOURCE_MAP (PARTE FABLE): BESTIARY (60 criaturas + 4 chefes finais com stat blocks; renomeações Veilkin/Gravedelver; dragão ancestral Ithryndor no 101), ITEM_CATALOG (~118 itens c/ BaseValue), QUEST_CATALOG (~86 quests, 5 fontes incl. secretas da caverna, XP escalado, +1 skill point/ato), BALANCE_CURVES (cap 100, XP 60×N^1.5, multiplicadores por tipo, TTK), SKILL_ACTION_MOVEMENT_TABLE, HUD_LAYOUT_SCENES (minimapa v1, abas, Town 48×42, Cave 55×55±, lotes da farm); docs validation exit 0; PRÓXIMO: gerar specs fable_21+ a partir dos catálogos + emendas F09/F14 (tamanhos/abas/responsivo)
- FABLE_ADHERENCE_AUDIT_QUEUE_CLEANUP: COMPLETE (2026-06-12) — auditoria de código real (fable_00B) revelou sistemas das WAVES 02-11 ÓRFÃOS (DerivedStatsCalculator, RainIrrigation/Weather, FarmResourceRefresh, CropQuality/Fertilizer, FatigueSystem, Fonte sem corpo físico, City/Schedule duplicado de NPC/Schedule, CityServices); 6 specs corretivas geradas (fable_15-20, F15-17 = P0); fila limpa: 102 specs executadas movidas para a_implementar/executadas_build_validated/ (README + crosswalks; NÃO reexecutar); 02_spec_calendar_ui absorvida pela fable_20; docs validation ZERO ERROS exit 0 pela primeira vez (10 reports retro-preenchidos, 2 amendments marcados archived); run_strict_validation VALIDATION_PASS; delete candidates batch FABLE adicionado (playmode stray file, FIX_001 stale, duplicatas 03_quest) aguardando confirmação humana
- FABLE_GAP_CLOSURE_SPECS: GENERATED_AWAITING_HUMAN_PROMOTION (2026-06-12) — gap analysis completa de docs/design/** (42 directions) vs. código; 14 specs densas SpecKit em `docs/specs/a_implementar/fable/` (F01-F14: status effects canônicos, weapon actions+derived stats, equipment baselines, threat/pack, boss phases, loot tables+vulnerabilidades, magic learning, spell shapes, biomas da caverna, main quest Ato 1, interiores/schedules da cidade, animais de fazenda, save debts, UI Canvas) + índice `fable_00_index_gap_analysis.md`; registradas no SPEC_REGISTRY_TO_IMPLEMENT.md (seção Lote FABLE); directions cobertas por features_futuras NÃO duplicadas; docs validation sem erros novos; quality check PASS
- GAMEPLAY_EXPANSION_SLICE: BUILD_VALIDATED_WITH_HUMAN_UNITY_ACTION_REQUIRED (2026-06-12) — pedido humano direto; projéteis runtime com visual procedural para arco/magia (RuntimeProjectileFactory/ProjectileVisualAnimator, fallback no ProjectileSpawnService); EnemyBrain com comportamentos Leaper/PhaseShortBlink/BurrowAmbush/Retreat/GuardHold-return e projéteis inimigos reais (EnemyProjectileBehaviour); densidade da caverna 16-32 inimigos com escala por profundidade (cap 44, determinístico); CaveWanderingMerchant (22%/nível, estável por run, ADR-0005); 21 skill executors reais (melee/projétil/restauro) com cooldown por skill; CreateMvpTownScene v2 (23 NPCs em distritos ±16x±12, praça central com estátua do guerreiro espada bastarda+escudo, 12 casas, 24 árvores, barracas por vendedor); TownNpcDialogueLibrary (23 NPCs × 13 nós) + menu RebuildTownNpcDialogues + opção "Conversar" em NpcShopController; CreateMvpFarmScene com 24 canteiros e CaveEntranceInteractable (fecha DEBT-SCENE WAVE16); 18 testes EditMode novos; HARNESS FIX: check_spec_quality.ps1 corrigido (BOM UTF-8 + regex de isenção multiline) — exceção Pester de 2026-06-08 resolvida na raiz; run_strict_validation.ps1 exit 0 VALIDATION_PASS; humano deve regenerar TownScene/FarmScene + rodar menu de diálogos + checklist: docs/validation/gameplay_expansion_2026_06_12_human_playmode_scenario.md; report: docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md
- FIX-001 + FIX-001B: BUILD_VALIDATED (2026-06-10) — CS0618 eliminado via registro estático (CraftingRuntime.ActiveInstances substitui FindObjectsOfType em CraftingStationRuntimeBootstrap; CaveRunManager.Instance substitui FindAnyObjectByType em CaveRuntimeBridge); QuestOfferPanelController e QuestLogPanelController com guard de destruição de duplicata em Awake; ValidateTownShopCatalogIntegrity expandido para todos 25 ShopDataSO em Data/Economy (era 5 hardcoded); 3 docs criados; Assembly-CSharp PASS 0E/0W; Assembly-CSharp-Editor PASS 0E/0W; Play Mode checklist pendente: docs/validation/FIX_001B_HUMAN_PLAYMODE_CHECKLIST.md

*Last updated: 2026-06-13 (Refinamento v3 + dívida de game_rules fechada + cobertura de design (fable_72/73, companions densas) — 73 specs, fable_71, ADR-0010..0014, 2 game_rules reconciliados; Refinamento v2 respondido — 70 specs, F68-F70 geradas, 18 emendas; GAMEPLAY_EXPANSION_SLICE BUILD_VALIDATED_WITH_HUMAN_UNITY_ACTION_REQUIRED — projéteis, enemy behaviors, densidade, mercador errante, skills reais, town/farm scene v2, diálogos 23×13, harness quality check consertado)*
*Next update: after human Unity actions (scene regen + dialogue rebuild) and Play Mode checklist execution*

## 2026-06-12 — FABLE_MASTER_PLAN_F21_F42

- Plano mestre criado: `docs/specs/a_implementar/fable/fable_00C_master_execution_plan.md`
  (revisão F01-F20 ✅ todas válidas; 9 emendas vinculantes aplicadas em F01/F02/F03/F05/F06/F09/F12/F14/F20).
- 22 specs novas F21-F42 geradas (catálogos canônicos → runtime/data/content).
- Fila ativa = 42 specs em `fable/`; ordem e paralelismo no plano mestre (9 batches, checkpoints M1-M4).
- Próxima ação: execução sequencial começando por F15 (corretiva P0 wiring clima/farm).
## 2026-06-12 — FABLE_EXECUTION_BATCH_0_DONE

- F15 (clima/chuva/refresh/qualidade/fertilizante): BUILD_VALIDATED — 5 módulos órfãos ligados.
- F16 (fadiga/sono/colapso 02:00 + cama): BUILD_VALIDATED — Player/Conditions hospedado.
- F17 (Fonte de Anya física + respawn + Água Viva): BUILD_VALIDATED — wiring do _anyaFountain completa o respawn.
- Cada spec: run_strict_validation exit 0 + report individual em docs/validation/fable_1X_*.md.
- Pendência humana acumulada: regenerar FarmScene no Unity (RainIrrigation + cama + Fonte) e rodar Test Runner EditMode.
- Próxima: F13 (BATCH 1 — save schema, SOLO).
## 2026-06-12 — FABLE_EXECUTION_BATCH_1_DONE

- F13 (save debt closure: cave run + enemy HP + daily goals): BUILD_VALIDATED.
- F42 (progressão: curva canônica 60×N^1.5, cap 100, TotalXp fonte de verdade, migração, 4 fontes de XP): BUILD_VALIDATED.
- Acumulado: 5/42 specs executadas (F15 F16 F17 F13 F42), todas com strict validation exit 0 + report individual.
- Próxima: BATCH 2 — F01 (status effects) → F02 (combat actions) → F03 (equipment baselines) → F18 (derived vitals) → F27 (perfect block).
## 2026-06-12 — FABLE_EXECUTION_BATCH_2_PROGRESS

- F01 (13 status canônicos + semântica + gerador/validator + wiring skills/actions): BUILD_VALIDATED.
- F02 (light/heavy/charged canônicos + crítico canon + DerivedStats no dano/cooldown + posture/stagger): BUILD_VALIDATED.
- Acumulado: 7/42 (F15 F16 F17 F13 F42 F01 F02). Próximas no BATCH 2: F03 → F18 → F27.
- Pendência Unity acumulada: regenerar FarmScene; rodar Generate Canonical Status Effects + validator; Test Runner EditMode.
## 2026-06-12 — FABLE_EXECUTION_BATCH_2_DONE (CHECKPOINT M1)

- F03 (baselines canônicos por arma + armadura funcional via PlayerDamageReceiver): BUILD_VALIDATED.
- F18 (vitals derivados: MaxHP/Stamina/Mana proporção, regens, resistências, fome): BUILD_VALIDATED.
- F27 (perfect block 0.15s + anti-spam + mitigação 50% + reflexo de postura + guard break): BUILD_VALIDATED.
- ACUMULADO: 10/42 specs (F15 F16 F17 | F13 F42 | F01 F02 F03 F18 F27) — todas strict validation exit 0 + report individual.
- CHECKPOINT M1 (humano) — pendências Unity antes do BATCH 3:
  1. Regenerar FarmScene (RainIrrigation + cama + Fonte de Anya + wiring _anyaFountain).
  2. Rodar CindarsHope/Combat/Generate Canonical Status Effects + Validate Status Effect Database.
  3. Rodar CindarsHope/Combat/Apply Weapon Mechanical Baselines.
  4. Unity Test Runner EditMode (≈70 testes novos das 10 specs).
- Próxima (após M1): BATCH 3 — F04 (enemy actions) → F24 (12 Moves + elites) → F05 (boss phases).
## 2026-06-12 — FABLE_SPEC_GENERATION_COMPLETE (47 specs)

- Geração de specs CONCLUÍDA: F01-F47 todas em formato SpecKit denso em `a_implementar/fable/`.
  - F21-F26, F28-F41 expandidas de compactas para densas (~300 linhas cada, 4 subagents).
  - F43-F47 criadas (endgame, save multi-nível, bestiário codex, romance, follow-ups stats).
- Ordem de execução oficial: fable_00C PARTE E — enumeração E01-E47 com dependências,
  locks e grupos paralelos P1-P5. E01-E10 já executadas (BUILD_VALIDATED).
- EXECUÇÃO PAUSADA por ordem do humano até autorização (checkpoint M1 pendente: regenerar
  FarmScene, rodar 3 geradores de combat data, Test Runner EditMode).
- Docs validation: zero erros.
## 2026-06-12 — .SPECS_MODELO_OPERACIONAL

- Criada `.specs/` na raiz: espelho operacional de execução (fonte canônica segue docs/specs/).
  - `00_executadas/legado/` (94 specs históricas) + `00_executadas/fable/` (E01-E10).
  - `01_a_executar/` (E11-E47, arquivos prefixados Exx_ na ordem oficial do 00C PARTE E).
  - `README.md` = modelo operacional: executou (BUILD_VALIDATED) → MOVE Exx_*.md para
    00_executadas/fable/ e atualiza o placar (hoje: 10/47).
## 2026-06-12 — AUDITORIA_COMPLETUDE_F48_F60_RETRO

- 3 auditorias (reconstrutibilidade código↔specs + completude design↔specs ×2):
  slice 2026-06-12 e WI-17..26 não tinham spec; ~24 quests do catálogo sem dono;
  munição de arco, craft tier alto, pesca, armadilhas, áudio, telemetria, aba Sistema sem spec.
- Gerados: F48-F60 (13 funcionais), spec_retro_01..09 (reconstrutibilidade), emendas F21/F34.
- .specs/ agora REFAZ o projeto inteiro: fundacao/ (66) + legado/ (94) + retro/ (9) + fable.
- Placar oficial: 60 specs funcionais — 10 executadas (E01-E10), 50 a executar (E11-E60).
- Ordem/janelas: fable_00C PARTES E+F; espelho .specs/README.md.
## 2026-06-12 — AUDITORIA_MVP_SHIPPING_F61_F67_REFINAMENTO_V2

- 3 auditorias finais: build standalone INEXISTENTE (zero cenas no EditorBuildSettings),
  onboarding/intro/tela de morte sem spec, ~30 decisões humanas em aberto nos próprios docs.
- Gerados: F61-F67 (build, onboarding, intro, death screen, daily goals closeout, débitos,
  governança) + emendas F34-C/F20-C + docs/design/FABLE_REFINAMENTO_V2_PERGUNTAS.md.
- PLACAR: 67 specs funcionais (10 executadas, 57 a executar E11-E67) + 9 retro + fundação/legado.
- AGUARDANDO: respostas do Refinamento v2 (blocos 1-4 e 7 travam F36/F43/F46/F56/F63 e o
  corte oficial do MVP — proposta de caminho crítico na 00C PARTE G.3).
- E67 (governança docs-only) pode ser executada imediatamente, sem dependências.
## 2026-06-12 — REFINAMENTO_V2_RESPONDIDO_F68_F70

- Dono respondeu o Refinamento v2 INTEIRO → `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md`
  (VINCULANTE; inclui APÊNDICE LORE A.1-A.5: nymirianos, Cindar fundadora, Raiz Primeva de
  Mana, regra "USAR amplifica só o liberado", catálogo das 11 Marcas dos Deuses).
- Decisões-chave: MVP = LOTE INTEIRO (7.6 "Faça tudo"); 3 slots + backup rolling (7.4);
  Fonte começa Dormant (5.2-B); kit canônico + baú narrativo (2.1/2.2); criação de
  personagem nome+M/F/Neutro+tints (1.1-1.3); morte permanente de animais (5.1-A);
  estações Semeio/Brasa/Véu/Gelo (6.1-A); nível 101 = CaveScene especial (4.5-B);
  painel 9 abas com Social + busca no inventário (7.7); sprint em combate aprovado (6.5-A).
- 18 emendas EMENDA-D propagadas: F08/F11/F12/F14/F20/F21/F37/F41/F43/F45/F46/F49/F50/
  F56/F61/F63/F66 (+ cópias .specs).
- Gerados: F68 (Marcas dos Deuses), F69 (sprint em combate), F70 (2ª leva side quests,
  11 NPCs/~33 quests). Lore consolidada destrava textos de F36/F43/F63.
- PLACAR FINAL DA GERAÇÃO: 70 specs funcionais (10 executadas, 60 a executar E11-E70)
  + 9 retro + fundação/legado. Ordem/janelas: 00C PARTE H; espelho .specs/README.md.
- EXECUÇÃO DO LOTE: aguarda autorização humana. Checkpoint M1 no Unity segue PENDENTE.
## 2026-06-13 — REFINAMENTO_V3_RESPONDIDO + RE-AUDITORIA_DE_CÓDIGO

- Dono respondeu o Refinamento v3 INTEIRO (skills/itens/companions/boas práticas) →
  `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (VINCULANTE).
- Re-auditoria de código (`reaudit-code-v3`, 7 agentes) corrigiu 3 premissas: números de dano
  flutuantes JÁ existem e estão wired; inventário já é 30 slots; 21 executores de skill reais +
  catálogo cresceu p/ 69 nós. Achou 5 itens novos (2 sistemas de slot paralelos 1-4 vs R/T/Y/G;
  NpcGiftPreferences morta; save de companion sem provider; Romance/Spouse fora do enum;
  skill point 1/2 níveis). Confirmou 7 game_rules obsoletos (stop condition latente).
- GERADO (workflow gen-v3-amendments, 10 artefatos, validate_docs exit 0):
  ADR-0010 + reescrita de skill_tree_rules.md e inventory_equipment_rules.md;
  ADR-0011..0014 (arte 32px, localização P4, input v1, dificuldade única);
  SKILL_NUMERIC_ADDENDUM_v1.0 (dano/cd/custo + prereqs + capstones);
  NPC_GIFT_TASTE_MATRIX_v1.0 (gostos por NPC); COMPANION_ROLES_CATALOG_v1.0 (bônus+ações);
  fable_71 (combat feel pass); emendas EMENDA-V3 em F03/F14/F26/F29/F32/F42/F43/F56/F58 +
  COMPANIONS_DIRECTION + 4 specs 14_companion convertidas para spec-ready.
- PLACAR: 71 specs funcionais (10 executadas, 61 a executar E11-E71) + 9 retro; 14 ADRs.
- LATENTE (registrado, não bloqueante; validate_docs verde): economy_rules.md e outros
  game_rules citados por ~6 specs ainda não existem em docs/game_rules/ — decidir criar ou
  normalizar referências antes/durante a execução.
## 2026-06-13 — DÍVIDA DE GOVERNANÇA FECHADA + COBERTURA DE DESIGN v3

- 8 game_rules faltantes CRIADOS (completos/descritivos, ancorados em direction+código+decisões):
  economy/player/quest/npc/time/city/fonte/ui_rules. Agora docs/game_rules/ = 20 regras; a
  referência required_game_rules das specs resolve de fato (não só no formato). Índices
  (GAME_RULES_INDEX, DECISION_LOG) atualizados. validate_docs exit 0.
- COBERTURA DE DESIGN v3: o design novo de FEATURE que só tinha direction virou spec densa —
  fable_72 (presentes por NPC, reusa F26 + matriz de gostos) e fable_73 (localização id→string,
  ADR-0012) entram no lote v1; as 4 specs de companion (features_futuras/14_*) foram elevadas de
  esqueleto a SpecKit denso (combate/brain, recrutamento+save provider, farm jobs, UI/HUD)
  refletindo o COMPANION_ROLES_CATALOG e as decisões v3 — seguem WAVE 14 (gated). game_rules não
  viram spec (regras consumidas; domínios já cobertos).
- PLACAR lote v1: 73 specs funcionais (10 executadas, 63 a executar E11-E73) + 9 retro; 14 ADRs;
  20 game_rules; 4 specs companion densas WAVE 14. Ordem: fable_00C PARTE J.
- LATENTE adicional (economy): dois caminhos de economia vivos no código (SellAll por BaseValue
  cheio vs Economy/Pricing services) — registrado em economy_rules.md como Open Question para spec
  de convergência. Não bloqueia.