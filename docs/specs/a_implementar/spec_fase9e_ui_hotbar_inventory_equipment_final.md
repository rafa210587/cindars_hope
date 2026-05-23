# SPEC FUTURA — FASE9E UI HOTBAR INVENTORY EQUIPMENT FINAL

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: A implementar
> Tipo: Spec preparada / futura

---

# SPEC: FASE9E UI - Hotbar, Inventory & Equipment Final (PENDING)

**Status**: Pendente (Contracts implemented, UI final pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Final UI implementation for hotbar, inventory, and equipment system with visual feedback and full interactivity.

## Pending Work

- [ ] Hotbar UI component (6 visible slots with icons)
- [ ] Drag-drop inventory management
- [ ] Equipment comparison tooltips
- [ ] Tool tier visual indicators
- [ ] Durability visualization
- [ ] Inventory search/sort
- [ ] Keyboard shortcuts (F1-F6 for hotbar, I for inventory)
- [ ] Consumable usage from hotbar
- [ ] Equipment swap visual feedback

## Key Contracts to Extend

- `Assets/_Game/Scripts/UI/Hotbar/HotbarManager.cs`
- `Assets/_Game/Scripts/UI/Hotbar/HotbarSlot.cs`
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`
- `Assets/_Game/Scripts/UI/InventoryUI.cs` (new)

## Acceptance Criteria

- [ ] Hotbar displays 6 slots with icons
- [ ] F1-F6 switches equipped tool visually
- [ ] Inventory opens with I key
- [ ] Drag-drop swaps items
- [ ] Tool comparison tooltip appears on hover
- [ ] No frame rate impact
- [ ] Persists across scenes
- [ ] Responsive to screen resize

## Dependencies

- Core UI framework (depends on core foundation)
- Equipment system (implemented in PR-101-130)
- Hotbar system (implemented in PR-101-130)

## Next Steps

Implement after FASE9E HUD validation (PR-132-FIX).



