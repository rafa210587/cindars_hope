# SPEC: Fishing Combat Integration & Equipment Final (PENDING)

**Status**: Pendente (Tool system basic, combat integration pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Full combat integration with weapons, equipment visuals, and fishing/farm tool optimization.

## Pending Work

- [ ] Weapon system (swords, staves, bows)
- [ ] Weapon tier progression
- [ ] Attack animations
- [ ] Damage application in melee combat
- [ ] Equipment visual feedback (on-player rendering)
- [ ] Tool durability display
- [ ] Fishing rod mechanics (reel mini-game)
- [ ] Axe chop animation
- [ ] Weapon/tool switching animation
- [ ] Equipment preset saving

## Key Files to Create

- `Assets/_Game/Scripts/Combat/WeaponController.cs` (new)
- `Assets/_Game/Scripts/Equipment/EquipmentVisualizer.cs` (new)
- `Assets/_Game/Scripts/Fishing/FishingMiniGame.cs` (new)

## Acceptance Criteria

- [ ] Weapon tiers implemented (Basic/Standard/Fine)
- [ ] Attack applies weapon damage
- [ ] Animations play on action
- [ ] Tool switching smooth (no stutter)
- [ ] Equipment visible on player
- [ ] Fishing mini-game functional
- [ ] Tool durability visible

## Dependencies

- Equipment system (implemented in PR-101-130)
- Damage formula (implemented in PR-101-130)
- Combat base (basic combat implemented)
- Animation system (core Unity)

## Next Steps

Implement after FASE9E validation.
