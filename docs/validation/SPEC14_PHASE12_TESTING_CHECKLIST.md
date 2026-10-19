# SPEC 14 Phase 12 - Final Testing & Integration Checklist

**Status:** Documentation Only (requires human play-testing)
**Date:** 2026-05-25

## Play Mode Testing Checklist

### Biome Transitions
- [ ] Test entering cave level 1 (Stone Cavern biome 1)
- [ ] Test progression through biome 1 (levels 1-10)
- [ ] Verify biome visual/audio changes at level 11 (Forest)
- [ ] Test all 8 biome transitions (Stone → Forest → Ice → Fire → Ruins → Abyss → Core → Final)
- [ ] Verify biome resolver returns correct biome for each level range

### Boss Gate Blocking
- [ ] Test boss gate at level 15 blocks progression to level 16
- [ ] Test boss gate at level 30 blocks progression to level 31
- [ ] Test boss gate at level 45 blocks progression to level 46
- [ ] Test boss gate at level 60 blocks progression to level 61
- [ ] Test boss gate at level 75 blocks progression to level 76
- [ ] Test boss gate at level 90 blocks progression to level 91
- [ ] Verify all gates follow FASE9F amendment rules (CanAdvanceToLevel checks)

### Checkpoint Unlock Mechanics
- [ ] Defeat boss at level 15, verify checkpoint 15 unlocks
- [ ] Defeat boss at level 30, verify checkpoint 30 unlocks
- [ ] Verify checkpoint 1 always unlocked (default)
- [ ] Verify unlocked checkpoints persist across level transitions within same run

### Checkpoint Teleport (Same Run)
- [ ] Open checkpoint portal at any level
- [ ] Select unlocked checkpoint from menu
- [ ] Verify player teleports to entrance of destination level
- [ ] Verify level layout matches snapshot (no regeneration)
- [ ] Verify enemy spawn plan is correct
- [ ] Test multiple teleports within same run

### Enemy Respawn After 2 In-Game Days
- [ ] Defeat enemies on level 20
- [ ] Advance 1 game day via time manager
- [ ] Return to level 20, verify enemies NOT respawned
- [ ] Advance 1 more game day (2 days total)
- [ ] Return to level 20, verify enemies respawned
- [ ] Verify respawn plan matches original spawn anchor positions

### Layout Hash Consistency on Revisit
- [ ] Enter level 25, capture layout hash
- [ ] Exit to another level
- [ ] Return to level 25
- [ ] Verify layout hash identical (snapshot loaded, not regenerated)
- [ ] Check in debug: "Layout hash validated" message appears

### Confinement Validation (No Spawns in Walls)
- [ ] Enter any level
- [ ] Observe enemy positions
- [ ] Verify no enemies spawned in/on walls
- [ ] Verify all enemies on walkable tiles
- [ ] Check debug logs for confinement warnings (should be none)

### Save/Load State Restoration
- [ ] Play cave for multiple levels
- [ ] Save game via SaveManager
- [ ] Load game
- [ ] Verify current cave level restored
- [ ] Verify deepest layer reached restored
- [ ] Verify all unlocked checkpoints restored
- [ ] Verify run seed unchanged
- [ ] Verify snapshots loaded correctly
- [ ] Verify boss defeat states restored

### Snapshot Cache Behavior (Same Run)
- [ ] Enter level 20 (generates fresh)
- [ ] Verify "Cached snapshot for level 20" does NOT appear (first visit)
- [ ] Exit to level 21
- [ ] Return to level 20
- [ ] Verify "Using cached snapshot for level 20" in debug logs
- [ ] Verify no regeneration happened (same layout)
- [ ] Regenerate run (new run seed)
- [ ] Return to level 20 - should regenerate (new run)

## Non-Regression Testing

### Run Tools
```bash
tools/docs/validate_docs.ps1
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1
```

### Verify No Regressions in SPEC 05-13 Systems
- [ ] Farm gameplay works (planting, harvesting)
- [ ] Fishing system functional
- [ ] Crafting queue works
- [ ] NPC interactions unchanged
- [ ] Inventory management operational
- [ ] Equipment durability system working
- [ ] Stamina/hunger/mana systems working
- [ ] Status effects applying correctly
- [ ] Combat damage calculations correct
- [ ] Enemy AI behavior unchanged
- [ ] Bestiary recording kills

### Compiler & Logs
- [ ] No error CS* in compile logs
- [ ] Only expected warnings (deprecations)
- [ ] No Unity exceptions in play
- [ ] No null reference exceptions

## Test Results Template

```
Date: YYYY-MM-DD
Tester: [Name]
Unity Version: 6000.4.7f1

### Passing Tests
- [Test name]
- [Test name]

### Failing Tests
- [Test name]: [Reason]
- [Test name]: [Reason]

### Notes
[Any observations or issues]
```

## Known Limitations

- Fishing spot snapshot serialization: Not implemented (feature not yet in SPEC 14)
- Boss defeat unique rewards: Stored in snapshot but reward distribution tested separately
- Checkpoint portal visual clarity: Uses simple OnGUI, not polished

## Success Criteria for Phase 12

✅ All play mode tests pass
✅ No regressions in SPEC 05-13 systems
✅ Compiler validation passes
✅ All edge cases handle gracefully
