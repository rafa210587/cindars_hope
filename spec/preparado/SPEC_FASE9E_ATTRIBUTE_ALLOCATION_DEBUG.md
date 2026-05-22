# SPEC: FASE9E Attribute Allocation Debug UI (PENDING)

**Status**: Pendente (Contracts implemented, UI debug pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Debug UI for allocating stat points to attributes (Strength, Vitality, Dexterity, Intelligence).

## Pending Work

- [ ] OnGUI debug attribute allocation panel
- [ ] Input (arrow keys or +/- keys) to adjust points
- [ ] Real-time stat preview
- [ ] Save allocation on confirm
- [ ] Reset allocation
- [ ] Display current points available
- [ ] Show attribute bonuses

## Key Files to Create

- `Assets/_Game/Scripts/UI/AttributeAllocationDebugPanel.cs` (new)
- Extend `Assets/_Game/Scripts/UI/DebugHud.cs`

## Acceptance Criteria

- [ ] Debug UI shows 4 attributes
- [ ] Points increase/decrease with input
- [ ] Bonus damage/vitality preview updates
- [ ] Allocation persists save/load
- [ ] No points overspent
- [ ] Visual feedback on input

## Dependencies

- Player progression system (implemented in PR-101-130)
- Damage calculator (implemented in PR-101-130)

## Next Steps

Implement after FASE9E HUD validation (PR-132-FIX).
