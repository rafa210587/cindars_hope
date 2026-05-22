# REF — TOWN PR053 063

> Origem: $src`n> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md`n
---

# SPEC: Town (PR-053 to PR-063)

**Status**: Implementado MVP  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-053 to PR-063

---

## Summary

Town scene with portals, NPC interactions, and shop points. First multi-scene experience.

## Scope

- ✅ TownScene generation (CreateMvpTownScene)
- ✅ Farm ↔ Town portals
- ✅ NPC Pip placeholder
- ✅ Shop points in town
- ✅ Crafting point in town
- ✅ Scene transitions with state caching
- ⚠️ NPC dialogue system (not implemented)
- ⚠️ Town visual layout (not finalized)

## Architecture

### Portals
- **Farm → Town**: Portal at farm exit
- **Town → Farm**: Portal at town entrance
- **Scene Loading**: Via SceneManager with bootstrap state caching
- **Player Spawn**: At portal destination

### Town Layout
- **Entry Point**: Farm portal
- **NPC Pip**: Placeholder character
- **Shop Points**: Buy/sell points
- **Crafting Point**: Carpentry station

### Cross-Scene Persistence
- **Bootstrap**: Caches scene name, player position
- **Load Flow**: Scene loads, installer rebinds components
- **State Restoration**: Components query bootstrap for cached state

## Key Files

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` — Scene generation
- `Assets/_Game/Scripts/SceneManagement/ScenePortal.cs` — Portal interaction
- `Assets/_Game/Scripts/NPC/NPCCharacter.cs` — NPC base (Pip placeholder)
- `Assets/_Game/Scripts/Town/TownSceneInstaller.cs` — Runtime setup

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Farm → Town portal works | ✅ |
| 2 | Town → Farm portal works | ✅ |
| 3 | Player spawns at destination | ✅ |
| 4 | NPC Pip visible in town | ✅ |
| 5 | Shop points in town | ✅ |
| 6 | Crafting point in town | ✅ |
| 7 | State persists across scenes | ✅ |
| 8 | HUD doesn't duplicate after transitions | ✅ |

## Pending

- NPC dialogue system
- Town visual layout and sprites
- Additional NPCs
- Shops with distinct inventories
- Quest system

## Next Steps

Continue to Cross-Scene Hardening (PR-065+).


