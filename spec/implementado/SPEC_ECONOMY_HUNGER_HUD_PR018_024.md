# SPEC: Economy, Hunger & HUD (PR-018 to PR-024)

**Status**: Implementado Parcial  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-018 to PR-024

---

## Summary

Player economy (gold), hunger system with consumption/loss, sell/buy points, and debug HUD for state visibility.

## Scope

- ✅ Gold system (add/remove)
- ✅ Hunger system (lose by movement/day, consume food)
- ✅ SellPoint (sell items for gold)
- ✅ BuyPoint (buy items for gold)
- ✅ DebugHud (OnGUI state display)
- ⚠️ Food consumer (partial)

## Architecture

### Gold Economy
- **PlayerState**: Stores current gold
- **SellPoint**: Queries sellable items from inventory, exchanges for gold
- **BuyPoint**: Offers items for gold, consumes gold on purchase

### Hunger System
- **HungerManager**: Tracks hunger value (0-100)
- **Hunger Loss**: Per movement step, per day advance
- **Food Consumption**: Restore hunger via FoodConsumer interaction
- **KO on Starvation**: Player defeated if hunger ≤ 0

### HUD
- **DebugHud**: OnGUI rendering of player state (gold, hunger, health, inventory)
- **Layout**: Two-column debug info panel
- **Real-time Updates**: No caching, reads current state each frame

## Key Files

- `Assets/_Game/Scripts/Player/PlayerState.cs` — Gold & stats
- `Assets/_Game/Scripts/Economy/SellPoint.cs` — Item → Gold
- `Assets/_Game/Scripts/Economy/BuyPoint.cs` — Gold → Item
- `Assets/_Game/Scripts/Hunger/HungerManager.cs` — Hunger tracking
- `Assets/_Game/Scripts/Hunger/FoodConsumer.cs` — Food interaction
- `Assets/_Game/Scripts/UI/DebugHud.cs` — State display

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Add/remove gold from player state | ✅ |
| 2 | SellPoint sells inventory items | ✅ |
| 3 | BuyPoint buys items | ✅ |
| 4 | Hunger decreases per step | ✅ |
| 5 | Hunger decreases per day | ✅ |
| 6 | Food consumption restores hunger | ✅ |
| 7 | DebugHud displays gold/hunger/health | ✅ |
| 8 | Starvation triggers KO | ⚠️ (implemented but not validated) |

## Pending

- Hunger KO validation in Play Mode
- Final UI design (not OnGUI)
- Balancing (loss rates, food restores)

## Next Steps

Continue to Save/Load (PR-025 to PR-030).
