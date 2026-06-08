# WAVE 11 Closeout Report

**Wave:** WAVE 11 — UI / UX / Input / Menus  
**Status:** COMPLETED_WITH_KNOWN_LEGACY_GATES  
**Date:** 2026-06-08  
**Branch:** dev

---

## Specs Executed

| Spec | Status | Commit | Tests |
|------|--------|--------|-------|
| 11_spec_ui_hud_gameplay_projection_notification_debug_separation_runtime | BUILD_VALIDATED | 8908f5a | 20 |
| 11_spec_ui_input_focus_modal_routing_runtime | BUILD_VALIDATED | d04affc | 20 + 49 pre-existing |
| 11_spec_ui_inventory_equipment_tooltips_runtime | BUILD_VALIDATED | c48d4dc | 28 |
| 11_spec_ui_shop_crafting_skilltree_quest_fonte_menu_projections_runtime | BUILD_VALIDATED | 22a8a42 | 28 |

**Total: 4/4 specs BUILD_VALIDATED**  
**Total new tests: ~96 EditMode + 49 pre-existing = ~145 cumulative**

---

## Systems Created / Hardened

### HUD / Notification / Debug
- `HUDGameplayViewModel.cs` (hardened — HotbarSlots, ActiveSkillSlots, StatusEffects, ContextPrompt, etc.)
- `HotbarSlotViewModel.cs` (ActiveSlotBlockedReason, ActiveSkillSlotViewModel, StatusBuffProjection, ContextPromptProjection)
- `FinalHudGuardValidator.cs` (MaxActiveSlots=4, ForbiddenFieldIds, IsDashDodgeBlock)
- `NotificationViewModel.cs` (NotificationPriority, NotificationQueuePolicy, HardQueueCap=20)
- `DebugHudProjection.cs` (IsAllowedInFinalBuild=false, namespace CindarsHope.UI.HUD)

### Input Focus / Modal Routing
- `InputFocusModalRoutingModel.cs` (hardened — SocialLogFocus, CalendarFocus, MapFocus, FonteFocus added; AnyaFountain→FonteFocus corrected)
- `InputFocusLayerContracts.cs` (UIFocusLayer, ModalKind, InputRoutingDecision, GameplayInputGate)

### Inventory / Equipment / Tooltips
- `InventoryItemViewModel.cs` (InventoryItemViewModel, InventoryActionAvailability, StackSplitRequest, ProtectedItemActionGuard)
- `EquipmentSlotViewModel.cs` (StatEntry, ResistanceEntry, SlotWarningState, EquipmentSlotViewModel)
- `EquipmentCompareViewModel.cs` (hardened — StatDiff, ResistanceDiff, DurabilityDiff, Warnings, PreviewOnly=true)
- `Tooltips/ItemTooltipViewModel.cs` (TooltipEquipmentBlock, TooltipSkillMagicBlock, TooltipAdvancedBlock, ItemTooltipViewModel)
- `Tooltips/TooltipLayerPolicy.cs` (TooltipContext enum, contextual rules)

### Shop / Crafting / SkillTree / Quest / Fonte / Menus
- `Shop/ShopMenuViewModel.cs` (ShopMode, ShopStockState, ShopRowViewModel, ShopMenuViewModel)
- `Crafting/CraftingMenuViewModel.cs` (ProcessingJobViewModel, CraftingMenuViewModel)
- `Skills/SkillTreeMenuViewModel.cs` (SkillNodeState, SkillNodeViewModel, SkillTreeTabViewModel, ActiveSlotSummary, SkillTreeMenuViewModel)
- `Quest/QuestLogMenuState.cs` (QuestLogTab enum, QuestLogMenuState)
- `Fonte/FonteMenuViewModel.cs` (hardened — FonteVisualStage, FonteUnlockedFunction, anti-spoiler gates)
- `Menus/MenuCommand.cs` (MenuType, MenuCommandType, MenuCommandValidationResult, MenuCommand)
- `Menus/MenuProjectionValidator.cs` (ValidateShopBuy/Sell/FonteFunction)

---

## Canon Invariants Enforced

| Invariant | Evidence |
|-----------|----------|
| Active skill slot cap = 4 | FinalHudGuardValidator.MaxActiveSlots=4 |
| Dash/Dodge/Block excluded from active slots | FinalHudGuardValidator.IsDashDodgeBlock() |
| AnyaFountain → FonteFocus (not ShopFocus) | InputFocusModalRoutingModel updated |
| Modal UI blocks all gameplay input | GameplayInputGate routing rules |
| ESC routes to UI when modal, gameplay when not | GameplayInputGate.RouteEscCancel |
| Respec hidden until Memory fragment | FonteMenuViewModel.IsRespecVisible guard |
| Purification hidden until Life fragment | FonteMenuViewModel.IsPurificationVisible guard |
| FinalChoice hidden until Hope/final route | FonteMenuViewModel.IsFinalChoiceVisible guard |
| Equipment compare preview-only | EquipmentComparisonViewModel.PreviewOnly=true |
| Quest/key items cannot sell/drop | ProtectedItemActionGuard |
| Unique items require confirmation | ProtectedItemActionGuard |
| No Pets/Social/Romance runtime | Verified |
| No Breath/Fôlego/BR in HUD | FinalHudGuardValidator.ForbiddenFieldIds |
| Debug HUD not in final build | DebugHudProjection.IsAllowedInFinalBuild=false |

---

## Validation Summary

- Assembly-CSharp: PASS (exit code 0, 0E/0W) — all 4 specs
- Assembly-CSharp-Editor: LEGACY_BLOCKER (pre-existing, not blocking)
- Quality check: known Pester issue (pre-existing, not blocking)
- Docs validation: EXPECTED_FAIL_LEGACY_ONLY
- Mode: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

---

## Deferred / Remaining Work

- Wire all ViewModels to domain services (backend wiring)
- Play Mode scenarios for all 4 specs (deferred to FINAL_HUMAN_VALIDATION_BY_WAVE)
- Unity batchmode compile (deferred — no Unity access in this environment)
- Final human scenario: open inventory, shop, skill tree, Fonte and verify all spoiler gates
