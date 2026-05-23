# REF — FARM LOOP PR013 017

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: `docs/specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md`

---

# SPEC: Farm Loop (PR-013 to PR-017)

**Status**: Implementado MVP  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-013 to PR-017

---

## Summary

Core farm gameplay: plant seeds in plots, advance day, crops grow, harvest and collect items. Deterministic growth via DayManager.

## Scope

- ✅ FarmPlot component (planting, growing, harvesting)
- ✅ Seed data contracts (SeedDataSO)
- ✅ Growth stages and visuals
- ✅ Day advancement system
- ✅ Deterministic crop growth
- ✅ Item pickup collection
- ✅ Persistence of plot state

## Architecture

### FarmPlot State Machine
- **Empty**: No crop planted
- **Growing**: Crop in growth stage (day_planted to maturity)
- **Ready**: Mature, can harvest
- **Post-Harvest**: Cleanup, returns to Empty

### Day Advancement
- **DayManager**: Tracks current day
- **DayStartedEvent**: Published when day advances
- **All Plots Listen**: Increment growth stage on event

### Seed Data
- **SeedDataSO**: ID, display name, growth days, yield item
- **Registry**: Lookup by ID or name

## Key Files

- `Assets/_Game/Scripts/Farm/FarmPlot.cs` — Plot state and interaction
- `Assets/_Game/Scripts/Farm/FarmManager.cs` — Day advancement
- `Assets/_Game/Scripts/Core/Data/SeedDataSO.cs` — Seed contracts
- `Assets/_Game/Scripts/Core/Data/SeedDatabase.cs` — Registry
- `Assets/_Game/Scripts/Farm/GrowthVisualizer.cs` — Sprite/animation updates

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Plant seed in empty plot | ✅ |
| 2 | Crop grows each day | ✅ |
| 3 | Harvest at maturity | ✅ |
| 4 | Collect item after harvest | ✅ |
| 5 | Plot resets to empty | ✅ |
| 6 | Save/load preserves plot state | ✅ |
| 7 | Day counter increments | ✅ |
| 8 | Multiple plots work independently | ✅ |

## Pending

- Visual polish (sprites, animations)
- Seed selection UI
- Seed balancing (growth times)

## Next Steps

Continue to Economy/Hunger (PR-018 to PR-024).



