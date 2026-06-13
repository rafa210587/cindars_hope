# REF — CROSS SCENE HARDENING PR065

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: `.specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md`

---

# SPEC: Cross-Scene Hardening (PR-065+)

**Status**: Implementado Parcial  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-065+

---

## Summary

Hardening of cross-scene state management, bootstrap caching, and component rebinding to ensure stable multi-scene gameplay.

## Scope

- ✅ Bootstrap state caching (scene, position, cave run)
- ✅ Component rebinding on scene load
- ✅ Event-driven state restoration
- ✅ Null-safe reference injection
- ✅ No singleton abuse (prefer events + caching)
- ⚠️ Comprehensive smoke test (partial)

## Architecture

### Bootstrap Lifecycle
1. **Initialization**: GameBootstrap.Instance created at startup
2. **Caching**: Before scene transition, systems cache state to bootstrap
3. **Load**: New scene loads, bootstrap persists across scenes
4. **Rebinding**: Scene installer queries bootstrap, injects cached state
5. **Cleanup**: Old scene refs cleared, new scene components set up

### Installers Pattern
- **SceneInstaller**: Each scene has installer (FarmSceneInstaller, TownSceneInstaller, etc.)
- **OnEnable()**: Subscribe to bootstrap events
- **Rebinding**: Retrieve cached state, configure local components
- **No Hardcoding**: All state comes from bootstrap or serialized fields

### State Transitions
- **Farm → Town**: Cache player position, day, inventory
- **Town → Farm**: Restore from cache, update scene
- **Farm → Cave**: Cache cave run seed, checkpoint
- **Cave → Farm**: Restore checkpoint, update cave state

## Key Files

- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` — Central cache
- `Assets/_Game/Scripts/SceneManagement/SceneManager.cs` — Transition handler
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` — Installer
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` — Installer
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs` — Installer

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Bootstrap survives scene loads | ✅ |
| 2 | State cached before transition | ✅ |
| 3 | Components rebind after load | ✅ |
| 4 | Player position restored | ✅ |
| 5 | Inventory restored | ✅ |
| 6 | Day counter persists | ✅ |
| 7 | No null reference exceptions | ✅ |
| 8 | Farm → Town → Farm → Cave works | ⚠️ (code OK, Play Mode pending) |

## Pending

- Full smoke test (Farm → Town → Farm → Cave → Farm)
- Verify no state leaks between scenes
- Verify HUD doesn't duplicate

## Next Steps

Continue to Tools/Equipment/Hotbar/Progression/Damage (PR-101 to PR-130).



