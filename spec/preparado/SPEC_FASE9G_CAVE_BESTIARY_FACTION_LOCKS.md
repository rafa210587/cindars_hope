# SPEC: FASE9G Cave Bestiary, Faction Locks & Portal Ecology (PENDING)

**Status**: Pendente (Spec document drafted, implementation pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Enemy biome progression with faction locks (bonded enemy families), portal ecology (restricted enemy appearances), and procedural bestiary.

## Pending Work

- [ ] Faction lock system (enemies bonded in families)
- [ ] Biome progression gating
- [ ] Enemy family definitions per faction
- [ ] Portal ecology incompatibility matrix
- [ ] Boss candidates selection (3 per depth range)
- [ ] Miniboss encounters
- [ ] Bestiary UI (discovered enemies)
- [ ] Enemy drop pools per faction
- [ ] Encounter weight balancing
- [ ] Faction visual themes

## Key Files to Create

- `Assets/_Game/Scripts/Cave/Data/FactionDataSO.cs` (new)
- `Assets/_Game/Scripts/Cave/Data/EnemyFamilyDataSO.cs` (new)
- `Assets/_Game/Scripts/Cave/Data/BossCandidateDataSO.cs` (new)
- `Assets/_Game/Scripts/Cave/Generation/EncounterEcologyValidator.cs` (new)
- `Assets/_Game/Scripts/UI/BestiaryUI.cs` (new)

## Faction Example

| Faction | Levels | Enemies | Boss |
|---------|--------|---------|------|
| Ooze | 1-15 | Slime, Ooze, Blob | Meteor Ooze King |
| Crystalline | 16-30 | Crystal Sprite, Geode | Crystal Golem |
| Abyssal | 31-45 | Shadow, Void, Wraith | Void Terror |

## Acceptance Criteria

- [ ] Factions define bonded enemies
- [ ] Enemies appear only in faction range
- [ ] Incompatible enemies never spawn together
- [ ] Boss selected from faction candidates
- [ ] Bestiary tracks discovered enemies
- [ ] Faction progression unlocks biomes
- [ ] No softlock (always escape route)

## Dependencies

- Cave resources complete (depends on FASE9F complete)
- Enemy spawn system (basic combat implemented)

## Specification

Full specification: `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`

## Next Steps

Implement after FASE9F complete (cave resources, encounters, scaling).
