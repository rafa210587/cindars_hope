# REF — CORE FOUNDATION PR001 012

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: `.specs/implementados/spec_core_001_event_bus_e_eventos_base.md`

---

# SPEC: Core Foundation (PR-001 to PR-012)

**Status**: Implementado  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-001 to PR-012

---

## Summary

Foundational architecture for Cindar's Hope: event-driven system with GameEventBus, bootstrap manager, scene management, save/load contracts, registries, and core game loop.

## Scope

- ✅ GameEventBus (publish/subscribe event system)
- ✅ GameBootstrap (singleton for cross-scene state caching)
- ✅ SceneManager (scene transitions with caching)
- ✅ SaveManager & SaveData (JSON persistence)
- ✅ DataRegistry<T> (generic ID-based lookups)
- ✅ Core events base classes
- ✅ InteractionPoint system (E key interaction)

## Architecture

### Event-Driven Design
- **GameEventBus**: Static subscriber/publisher for loose coupling
- **Event Pattern**: `[Action][Noun]Event` (e.g., `ItemPickedUpEvent`)
- **Subscription**: `OnEnable()` subscribe, `OnDisable()` unsubscribe

### Bootstrap & Caching
- **GameBootstrap**: Singleton survives scene loads
- **Cross-Scene State**: Caches current scene, player transform, cave run state
- **DontDestroyOnLoad**: Applied to bootstrap GameObject

### Save/Load Architecture
- **SaveData**: JSON-serializable DTO with minimal types (no Unity refs)
- **SaveManager**: Handles file I/O to `Application.persistentDataPath`
- **Scene Rebinding**: Components subscribe to bootstrap events, re-obtain references

### Registries
- **DataRegistry<T>**: Generic dictionary-based lookup by ID
- **Subclasses**: ItemDataSO registry, EnemyDataSO registry, etc.
- **Editor Support**: CreateAssetMenu for easy asset creation

## Key Files

- `Assets/_Game/Scripts/Core/GameEventBus.cs` — Event system
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` — Cross-scene state
- `Assets/_Game/Scripts/SceneManagement/SceneManager.cs` — Scene transitions
- `Assets/_Game/Scripts/Save/SaveManager.cs` — File I/O
- `Assets/_Game/Scripts/Save/SaveData.cs` — DTO contracts
- `Assets/_Game/Scripts/Core/Data/DataRegistry.cs` — Generic lookups
- `Assets/_Game/Scripts/Interaction/InteractionPoint.cs` — E key interaction

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | GameEventBus publishes and subscribers receive events | ✅ |
| 2 | GameBootstrap survives scene loads | ✅ |
| 3 | SaveData serializes to JSON | ✅ |
| 4 | SaveManager persists to persistentDataPath | ✅ |
| 5 | DataRegistry lookups by ID work | ✅ |
| 6 | InteractionPoint responds to E key | ✅ |
| 7 | Scene transitions preserve cached state | ✅ |
| 8 | No runtime GameObject.Find() or FindObjectOfType() calls | ✅ |

## Pending

- Full Play Mode validation across all scenes
- HUD display of core state

## Next Steps

Continue to Farm Loop (PR-013 to PR-017).



