# SPEC FUTURA — FASE9E DAMAGE STATUS ELEMENTS COMPLETE

> Origem: $src`n> Status: A implementar
> Tipo: Spec preparada / futura

---

# SPEC: FASE9E Damage, Status & Elements Complete (PENDING)

**Status**: Pendente (MVP formula implemented, full system pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Complete damage formula with status effects (poison, burn, bleed) and elemental interactions.

## Pending Work

- [ ] Status effect application logic
- [ ] Poison damage over time
- [ ] Burn effect (fire damage modifier)
- [ ] Bleed effect (sustained health loss)
- [ ] Status duration tracking
- [ ] Immunity/resistance system
- [ ] Elemental weakness matrix
- [ ] Visual feedback for status (color overlay, particle FX)
- [ ] Status removal (cure potions, etc.)

## Key Files to Extend

- `Assets/_Game/Scripts/Combat/DamageCalculator.cs`
- `Assets/_Game/Scripts/Combat/DamageResult.cs`
- `Assets/_Game/Scripts/Combat/StatusEffect.cs` (new)
- `Assets/_Game/Scripts/Combat/ElementalInteraction.cs` (new)

## Status Effects

| Effect | Duration | Damage | Removal |
|--------|----------|--------|---------|
| Poison | 3 turns | 2 HP/turn | Cure potion |
| Burn | 2 turns | 3 HP/turn + fire immunity | Time/cure |
| Bleed | 4 turns | 1 HP/turn | Bandage |

## Acceptance Criteria

- [ ] Status effect applies to enemy
- [ ] Duration counts down each turn
- [ ] Damage applied per tick
- [ ] Status removed at zero duration
- [ ] Visual feedback on status
- [ ] Enemy takes modified damage
- [ ] Can cure status with item

## Dependencies

- Damage formula MVP (implemented in PR-101-130)
- Combat system (basic combat implemented)

## Next Steps

Implement after cave procedural foundation (PR-141-153).

