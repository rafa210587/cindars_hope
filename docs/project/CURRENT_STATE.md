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

*Last updated: 2026-06-09 (WAVE_INTEGRATION_13: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED — SceneId, SceneTransitionRequest/Result, SceneTransitionGate, SceneSpawnAnchor, SceneTransitionRouter, PlayerSpawnResolver created; 11 docs + 2 human instruction docs; Assembly-CSharp PASS 0E/0W)*
*Next update: after WAVE13 human Unity wiring + Play Mode checklist pass*
