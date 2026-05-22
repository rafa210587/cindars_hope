# SPEC: World, Shop & Hardening (PR-031 to PR-045)

**Status**: Implementado Parcial  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-031 to PR-045

---

## Summary

World interactions (tree harvesting, fishing, seed shop), item pickups persistence, and system hardening for production readiness.

## Scope

- ✅ TreeNode (harvest wood with axe)
- ✅ FishingSpot (fish for common fish)
- ✅ SeedShopPoint (buy seeds)
- ✅ ItemPickup (collectible items in world)
- ✅ ItemPickupRegistry (persistence)
- ✅ System hardening (null checks, validation)

## Architecture

### World Interactions
- **TreeNode**: Requires axe tool, yields wood on harvest
- **FishingSpot**: Requires fishing rod, yields fish
- **SeedShopPoint**: UI to buy seeds (overlaps with BuyPoint)
- **Common Pattern**: Interaction via E key, feedback event

### Item Pickups
- **ItemPickup**: GameObject with item data, collectable
- **ItemPickupRegistry**: Global registry of all pickups
- **Persistence**: Save which pickups collected, don't respawn
- **Restoration**: Load registry state, skip collected items

### Hardening
- **Null Checks**: All references validated
- **Bounds Validation**: Positions, inventory counts
- **Event Subscriptions**: Always unsubscribe in OnDisable
- **Asset Loading**: Fallback data if registry empty

## Key Files

- `Assets/_Game/Scripts/World/TreeNode.cs` — Wood harvesting
- `Assets/_Game/Scripts/World/FishingSpot.cs` — Fishing
- `Assets/_Game/Scripts/Economy/SeedShopPoint.cs` — Seed purchase
- `Assets/_Game/Scripts/World/ItemPickup.cs` — Collectible items
- `Assets/_Game/Scripts/World/ItemPickupRegistry.cs` — Global pickup registry
- `Assets/_Game/Scripts/Save/ItemPickupSaveData.cs` — Pickup persistence

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | TreeNode yields wood | ✅ |
| 2 | FishingSpot yields fish | ✅ |
| 3 | SeedShopPoint sells seeds | ✅ |
| 4 | ItemPickup collectible | ✅ |
| 5 | Collected pickups don't respawn | ✅ |
| 6 | Pickup state persists save/load | ✅ |
| 7 | All interactions require proper tools/context | ✅ |
| 8 | No null reference exceptions | ✅ |

## Pending

- Visual polish (sprites, animations)
- Tree/fishing rod tool gating (partially done in PR-132-FIX)
- Balancing (yield amounts, respawn rates)

## Next Steps

Continue to Crafting (PR-046 to PR-052).
