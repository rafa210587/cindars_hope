# SpecKit Documentation — Cindar's Hope

## Overview

Complete SpecKit documentation for all implemented features and pending work. Organized into two categories:

- **`implementado/`** — Fully implemented and code-complete features
- **`preparado/`** — Pending features and future work

---

## Implementado (Implemented Specs)

### Foundation & Core Systems

1. **[SPEC_CORE_FOUNDATION_PR001_012.md](implementado/SPEC_CORE_FOUNDATION_PR001_012.md)**
   - GameEventBus, bootstrap, scene management, save/load contracts, registries, interactions

2. **[SPEC_SAVE_LOAD_PR025_030.md](implementado/SPEC_SAVE_LOAD_PR025_030.md)**
   - JSON persistence, cross-scene state caching, player state restoration

3. **[SPEC_CROSS_SCENE_HARDENING_PR065.md](implementado/SPEC_CROSS_SCENE_HARDENING_PR065.md)**
   - Bootstrap caching, component rebinding, multi-scene stability

### Farm, Economy & World

4. **[SPEC_FARM_LOOP_PR013_017.md](implementado/SPEC_FARM_LOOP_PR013_017.md)**
   - Plant seeds, advance day, crop growth, harvest, item collection

5. **[SPEC_ECONOMY_HUNGER_HUD_PR018_024.md](implementado/SPEC_ECONOMY_HUNGER_HUD_PR018_024.md)**
   - Gold economy, hunger system, buy/sell points, debug HUD

6. **[SPEC_WORLD_SHOP_HARDENING_PR031_045.md](implementado/SPEC_WORLD_SHOP_HARDENING_PR031_045.md)**
   - Tree harvesting, fishing, seed shop, persistent item pickups

### Crafting & Town

7. **[SPEC_CRAFTING_PR046_052.md](implementado/SPEC_CRAFTING_PR046_052.md)**
   - Recipe system, crafting manager, crafting points, ingredient validation

8. **[SPEC_TOWN_PR053_063.md](implementado/SPEC_TOWN_PR053_063.md)**
   - Town scene, portals (Farm ↔ Town), NPC placeholder, shop integration

### Systems & Combat

9. **[SPEC_TOOLS_EQUIPMENT_HOTBAR_PROGRESSION_DAMAGE_PR101_130.md](implementado/SPEC_TOOLS_EQUIPMENT_HOTBAR_PROGRESSION_DAMAGE_PR101_130.md)**
   - Tool tiers, equipment manager, hotbar (6 slots), item taxonomy, XP/level system, damage formula

### Cave System

10. **[SPEC_CAVE_PROCEDURAL_RUNTIME_PR141_153.md](implementado/SPEC_CAVE_PROCEDURAL_RUNTIME_PR141_153.md)**
    - BSP generation, runtime materialization, resource nodes, exits, level snapshots

11. **[SPEC_CAVE_STABLE_RUN_REPLAY_PR170_192.md](implementado/SPEC_CAVE_STABLE_RUN_REPLAY_PR170_192.md)**
    - Deterministic generation, snapshot replay, KO flow, daily refresh, seed lifecycle

12. **[SPEC_CAVE_BOSS_GATES_CHECKPOINTS_CONFINEMENT_PR193_202.md](implementado/SPEC_CAVE_BOSS_GATES_CHECKPOINTS_CONFINEMENT_PR193_202.md)**
    - Boss gate blocking, boss spawner, checkpoint selection, path confinement, death detection

13. **[SPEC_CAVE_VISUAL_FIXES.md](implementado/SPEC_CAVE_VISUAL_FIXES.md)**
    - Procedural materialization, camera smooth follow, enemy visuals, fallback rendering

---

## Preparado (Pending Specs)

### FASE 9E (UI & Progression Completion)

1. **[SPEC_FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_FINAL.md](preparado/SPEC_FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_FINAL.md)**
   - Final hotbar UI, inventory drag-drop, equipment comparison, visual feedback

2. **[SPEC_FASE9E_ATTRIBUTE_ALLOCATION_DEBUG.md](preparado/SPEC_FASE9E_ATTRIBUTE_ALLOCATION_DEBUG.md)**
   - Debug UI for stat point allocation to attributes (Str, Vit, Dex, Int)

3. **[SPEC_FASE9E_DAMAGE_STATUS_ELEMENTS_COMPLETE.md](preparado/SPEC_FASE9E_DAMAGE_STATUS_ELEMENTS_COMPLETE.md)**
   - Status effects (poison, burn, bleed), elemental interactions, resistances

### FASE 9F (Cave Resources & Encounters)

4. **[SPEC_FASE9F_CAVE_RESOURCES_ENCOUNTERS_COMPLETE.md](preparado/SPEC_FASE9F_CAVE_RESOURCES_ENCOUNTERS_COMPLETE.md)**
   - Loot tables, XP scaling, encounter generation per biome, respawn mechanics

### FASE 9G (Cave Bestiary & Ecology)

5. **[SPEC_FASE9G_CAVE_BESTIARY_FACTION_LOCKS.md](preparado/SPEC_FASE9G_CAVE_BESTIARY_FACTION_LOCKS.md)**
   - Enemy factions, biome progression, boss selection, portal ecology, bestiary UI

### Polish & Integration

6. **[SPEC_FISHING_COMBAT_INTEGRATION_FINAL.md](preparado/SPEC_FISHING_COMBAT_INTEGRATION_FINAL.md)**
   - Weapons, equipment visuals, fishing mechanics, combat animation

7. **[SPEC_UI_MENU_SYSTEMS_FINAL.md](preparado/SPEC_UI_MENU_SYSTEMS_FINAL.md)**
   - Main menu, pause menu, settings, inventory screen, character sheet

---

## Quick Stats

| Category | Count | Status |
|----------|-------|--------|
| Implemented Specs | 13 | ✅ Code-complete |
| Pending Specs | 7 | 📋 Ready to implement |
| **Total** | **20** | — |

---

## Usage

- **For implemented features**: Check `implementado/` for architectural details, key files, and test status
- **For pending features**: Check `preparado/` for scope, dependencies, and acceptance criteria
- **For full architectural context**: See `docs/ARCH_fase4_v2.2.md` and `docs/GDD_v2.6.md`

## Contributing

When implementing a spec:

1. Create a PR with the implementation
2. Update the corresponding spec file with test status
3. Move complete specs to `implementado/` if not already there
4. Update `PROJECT_LOG.md` and `docs/IMPLEMENTATION_STATUS.md`

---

## Related Documentation

- [PROJECT_LOG.md](../../PROJECT_LOG.md) — Operational history
- [IMPLEMENTATION_STATUS.md](../../docs/IMPLEMENTATION_STATUS.md) — Current status summary
- [SPEC_EVOLUTION_POLICY_v1.0.md](../../docs/SPEC_EVOLUTION_POLICY_v1.0.md) — Spec amendment rules
- [GDD_v2.6.md](../../docs/GDD_v2.6.md) — Complete game design
