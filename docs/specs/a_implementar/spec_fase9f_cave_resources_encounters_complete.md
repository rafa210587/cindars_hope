# SPEC FUTURA — FASE9F CAVE RESOURCES ENCOUNTERS COMPLETE

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: A implementar
> Tipo: Spec preparada / futura

---

# SPEC: FASE9F Cave Resources, Encounters & Scaling (PENDING)

**Status**: Pendente (MVP contracts and generator implemented, full system pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Complete cave resource node system with loot tables, XP rewards, encounter generation per biome, and level scaling.

## Pending Work

- [ ] Loot table system (chance-based drops)
- [ ] XP reward scaling by level
- [ ] Enemy encounter selection per biome
- [ ] Resource respawn on new day
- [ ] Biome-specific enemy families
- [ ] Boss minion spawn
- [ ] Item rarity (common, uncommon, rare, epic)
- [ ] Resource node visual variety
- [ ] Encounter difficulty adjustment
- [ ] Encounter composition balance

## Key Files to Extend

- `Assets/_Game/Scripts/Cave/Generation/CaveGenerator.cs`
- `Assets/_Game/Scripts/Cave/Runtime/ResourceNode.cs`
- `Assets/_Game/Scripts/Cave/Data/LootTableSO.cs` (new)
- `Assets/_Game/Scripts/Cave/Data/EncounterConfigSO.cs` (new)
- `Assets/_Game/Scripts/Cave/Generation/EncounterGenerator.cs` (new)

## Loot Table System

| Node Type | Common | Uncommon | Rare | Epic |
|-----------|--------|----------|------|------|
| Stone | 70% | 20% | 9% | 1% |
| Copper | 50% | 35% | 13% | 2% |
| Gold | 30% | 40% | 25% | 5% |

## Acceptance Criteria

- [ ] Loot tables define drop chances
- [ ] Resources drop based on table
- [ ] XP awarded on harvest
- [ ] XP scales with level
- [ ] Enemy encounters per biome
- [ ] Balanced encounter composition
- [ ] Resources respawn on new day
- [ ] Rarity visual distinction

## Dependencies

- Cave procedural runtime (implemented in PR-141-153)
- Cave stable run (implemented in PR-170-192)
- Boss gates (implemented in PR-193-202)

## Next Steps

Implement after cave boss gates and checkpoints (PR-193-202).



