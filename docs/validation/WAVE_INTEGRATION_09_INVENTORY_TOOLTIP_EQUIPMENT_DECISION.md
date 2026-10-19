# WAVE_INTEGRATION_09 — Inventory Tooltip Equipment Decision

**Wave:** WAVE_INTEGRATION_09  
**Date:** 2026-06-08  
**Status:** DECIDED

---

## Decision Summary

**Strategy:** REUSE_EXISTING_CONTROLLERS

All inventory, tooltip, and equipment runtime UI already exists via fully implemented IMGUI-based singleton controllers. No new code is required for the core functionality.

---

## Audit Findings

### Inventory System

| Component | Status | Notes |
|-----------|--------|-------|
| InventoryManager | EXISTING | `Assets/_Game/Scripts/Inventory/InventoryManager.cs` — full slot/item API, publishes InventoryChangedEvent |
| InventoryPanelController | EXISTING_FULLY_FUNCTIONAL | `Assets/_Game/Scripts/UI/InventoryPanelController.cs` — IMGUI panel, I key open/close, Esc support, slot grid, action menu (Use/Equip/Drop/Destroy/Split/Cancel), ModalManager push/pop |
| InventoryItemViewModel | EXISTING | `Assets/_Game/Scripts/UI/Inventory/InventoryItemViewModel.cs` — full projection model |
| InventoryTooltipViewModel | EXISTING | `Assets/_Game/Scripts/UI/Inventory/InventoryTooltipViewModel.cs` — tooltip + list VMs |

### Tooltip System

| Component | Status | Notes |
|-----------|--------|-------|
| InventoryPanelController.DrawSelectedDetails() | EXISTING | Shows "Selected: itemId x amount [equipment-binding]" — qualifies as minimal tooltip per spec scope |
| ItemTooltipViewModel | EXISTING | `Assets/_Game/Scripts/UI/Tooltips/ItemTooltipViewModel.cs` — full tooltip model (not wired to panel yet) |
| InventoryTooltipViewModel.InventoryItemTooltip | EXISTING | Has ItemId, ItemName, Rarity, Quantity, Description, Properties, Value |

### Equipment System

| Component | Status | Notes |
|-----------|--------|-------|
| CharacterEquipmentPanelController | EXISTING_FULLY_FUNCTIONAL | `Assets/_Game/Scripts/UI/Character/CharacterEquipmentPanelController.cs` — K=attributes, L=equipment, Esc close, IMGUI panel |
| EquipmentSlotViewModel | EXISTING | `Assets/_Game/Scripts/UI/Equipment/EquipmentSlotViewModel.cs` |
| EquipmentCompareViewModel | EXISTING | `Assets/_Game/Scripts/UI/Equipment/EquipmentCompareViewModel.cs` |

### Input/Focus/Modal

| Component | Status | Notes |
|-----------|--------|-------|
| ModalManager | EXISTING | On _Bootstrap; PushModal/PopModal; HasActiveModal read by PlayerController to block movement |
| GameplayInputRouter | EXISTING_NOT_IN_SCENE | `Assets/_Game/Scripts/UI/Input/GameplayInputRouter.cs` — publishes InventoryPanelOpenedEvent/EquipmentPanelOpenedEvent, blocks shortcuts during modal |
| PlayerController.ReadMoveInput() | EXISTING | Returns Vector2.zero when ModalManager.HasActiveModal = true |

---

## Decision

### UI Technology: EXTEND_DEBUGHUD_IMGUI (already implemented)

Both `InventoryPanelController` and `CharacterEquipmentPanelController` are IMGUI-based, matching the DebugHud technology already in use. No Canvas-based UI is required for this wave.

### Auto-wiring: RuntimeInitializeOnLoadMethod

Both controllers use `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` to auto-create themselves as DontDestroyOnLoad singletons. They do NOT need to be placed in FarmScene.unity.

### GameplayInputRouter: ADD to CreateMvpFarmScene.cs

`GameplayInputRouter` does NOT use RuntimeInitializeOnLoad. It was missing from the scene. Action: add `CreateGameplayInputRouter()` call in `CreateScene()` and a new method that creates the GameObject with the component.

### Open/Close Strategy

- Inventory: `I` key → `InventoryPanelController.Toggle()` → `ModalManager.PushModal(ModalType.Inventory)`
- Equipment: `L` key → `CharacterEquipmentPanelController.Toggle(PanelMode.Equipment)` → IMGUI panel
- Attributes: `K` key → `CharacterEquipmentPanelController.Toggle(PanelMode.Attributes)` → IMGUI panel
- Close: `Esc` or "Fechar" button → `ModalManager.TryPopModal` → gameplay input restored

### Input Blocking

`PlayerController.ReadMoveInput()` returns Vector2.zero when `ModalManager.HasActiveModal == true`. This covers the inventory modal. Equipment panel does NOT push to ModalManager — this is a known UI debt (player can still move while equipment panel is open).

---

## Files Changed

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` — added `CreateGameplayInputRouter()` method + call in `CreateScene()`
- `Assets/_Game/Scripts/Editor/Validation/ValidateInventoryRuntimeBinding.cs` — created (new)

## Files NOT Changed (reused)

- `Assets/_Game/Scripts/UI/InventoryPanelController.cs` — REUSE_AS_IS
- `Assets/_Game/Scripts/UI/Character/CharacterEquipmentPanelController.cs` — REUSE_AS_IS
- `Assets/_Game/Scripts/UI/Input/GameplayInputRouter.cs` — REUSE_AS_IS

---

## Known UI Debt

| Debt | Description | Impact |
|------|-------------|--------|
| Equipment panel no modal push | CharacterEquipmentPanelController does not push ModalType; player can move while open | AC-07 partial for equipment |
| Tooltip is itemId+amount only | DrawSelectedDetails shows raw ID; ItemTooltipViewModel not wired | AC-05 partial |
| InventoryPanelController doesn't subscribe to InventoryPanelOpenedEvent | Handles its own I key directly; event is published but not consumed | Potential double-open if GameplayInputRouter and controller both active |

---

*Created: 2026-06-08 (WAVE_INTEGRATION_09)*
