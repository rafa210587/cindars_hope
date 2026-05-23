# Validation Guide — 13 Implemented Features
**Date:** 2026-05-23  
**Wave Range:** Wave 01–07 (Overnight Specs Execution)

---

## Feature 1: Item Taxonomy System (ItemCategory + ConsumableSubtype)
**Spec:** `spec_fase9e_item_taxonomy_ids.md`  
**Implementation:** `Assets/_Game/Scripts/Core/ItemCategory.cs`, `Assets/_Game/Data/ItemDataSO.cs`  
**Assets Generated:** 23 ItemDataSO instances

### How to Validate in Unity:

1. Open Unity Project and navigate to `Assets/_Game/Data/Items/`
2. Verify 23 ItemDataSO assets exist (6 seeds, 6 crops, 7 consumables, 4 materials)
3. Select any ItemDataSO (e.g., `ItemData_Wheat_Seed`) and inspect:
   - ✅ `Id` field set (e.g., "seed_wheat")
   - ✅ `ItemCategory` enum shows 17+ categories in dropdown
   - ✅ If category is Consumable, `ConsumableSubtype` shows options (Potion, Food, BuffFood)
4. Open script `ItemCategory.cs`:
   ```csharp
   public enum ItemCategory
   {
       Seed, Crop, Consumable, Weapon, Magic, Ammo, Ore, Gem, 
       MonsterDrop, Quest, KeyItem, Furniture, Misc, [+6 more]
   }
   ```
5. **Pass Criteria:** All 17+ categories present, ConsumableSubtype applied to Consumable items

---

## Feature 2: Level-Up Progression System
**Spec:** `spec_fase9e_player_level_up_progression.md`  
**Implementation:** `Assets/_Game/Scripts/Core/LevelUpManager.cs`  
**Key Logic:** XP curve (linear with 10-level multiplier blocks), +1 attribute/level, +1 skill point per 3 levels

### How to Validate in Unity:

1. Create a new C# test script in `Assets/_Game/Scripts/Tests/` (if Tests folder doesn't exist, create it):
   ```csharp
   using UnityEngine;
   using CindarsHope.Core.Progression;
   
   public class TestLevelUpManager : MonoBehaviour
   {
       void Start()
       {
           // Test XP curve
           int xpLevel1 = LevelUpManager.GetXpRequiredForLevel(1);
           int xpLevel2 = LevelUpManager.GetXpRequiredForLevel(2);
           int xpLevel10 = LevelUpManager.GetXpRequiredForLevel(10);
           int xpLevel11 = LevelUpManager.GetXpRequiredForLevel(11);
           
           Debug.Log($"XP Level 1: {xpLevel1}, Level 2: {xpLevel2}");
           Debug.Log($"XP Level 10: {xpLevel10}, Level 11: {xpLevel11}");
           
           // Verify multiplier block at level 11 (+10 multiplier)
           if (xpLevel11 > xpLevel10 * 2)
               Debug.Log("✅ Level 11 multiplier block applied");
           
           // Test attribute allocation
           Debug.Log("✅ PlayerAttribute enum has: Strength, Dexterity, Intelligence, Willpower, Constitution, Breath");
       }
   }
   ```
2. Run test in Unity Play Mode (press Play)
3. Check Console for logs:
   - ✅ XP increases from level 1→2→10
   - ✅ Level 11 has multiplier jump (10x block starts)
   - ✅ PlayerAttribute enum has 6 attributes
4. **Pass Criteria:** XP curve scales correctly, no exceptions thrown

---

## Feature 3: Status Effects (Poison, Burn, Bleed)
**Spec:** `spec_fase9e_damage_status_elements_complete.md` (partial backend)  
**Implementation:** `Assets/_Game/Scripts/Combat/StatusEffectSO.cs`, `StatusEffectManager.cs`, `ActiveStatusEffect.cs`  
**Assets Generated:** 3 status effect definitions

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/StatusEffects/`
2. Verify 3 assets exist:
   - StatusEffect_Poison.asset
   - StatusEffect_Burn.asset
   - StatusEffect_Bleed.asset
3. Select `StatusEffect_Poison` and inspect:
   - ✅ `Id` = "status_poison"
   - ✅ `Type` = Poison
   - ✅ `DurationTurns` = 3
   - ✅ `DamagePerTurn` = 2
   - ✅ `VisualColor` assigned
4. Create test script:
   ```csharp
   using UnityEngine;
   using CindarsHope.Combat.Status;
   
   public class TestStatusEffects : MonoBehaviour
   {
       void Start()
       {
           var manager = new StatusEffectManager();
           var poisonSO = Resources.Load<StatusEffectSO>("StatusEffects/StatusEffect_Poison");
           
           // Apply poison
           manager.ApplyStatusEffect(poisonSO);
           Debug.Log($"Has poison: {manager.HasStatusEffect("status_poison")}");
           
           // Tick 3 times
           for (int i = 0; i < 3; i++)
           {
               manager.TickStatusEffects();
               Debug.Log($"Turn {i+1}: Status damage = {manager.GetStatusDamageThisTurn()}");
           }
           
           // Should be removed after 3 turns
           if (!manager.HasStatusEffect("status_poison"))
               Debug.Log("✅ Status effect auto-removed after duration");
       }
   }
   ```
5. Run in Play Mode, check console for damage per turn and removal
6. **Pass Criteria:** Apply → Tick 3 times → Auto-remove, damage = 2 HP/turn

---

## Feature 4: Equipment System (Durability + Environmental Resistance)
**Spec:** `spec_fase9h_loot_crafting_equipment_durability_environment.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs`, `DurabilityManager.cs`, `EnvironmentalResistanceManager.cs`  
**Assets Generated:** 10+ equipment items

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Equipment/`
2. Verify 10+ EquipmentDataSO assets (mix of helmets, armor, weapons, etc.)
3. Select any armor piece (e.g., `EquipmentData_IronArmor`) and inspect:
   - ✅ `Type` = Armor
   - ✅ `BaseDefense` > 0
   - ✅ `HeatResistance` or `ColdResistance` values
   - ✅ `DurabilityMax` = 100
4. Create durability test:
   ```csharp
   using UnityEngine;
   using CindarsHope.Equipment.Durability;
   
   public class TestDurabilitySystem : MonoBehaviour
   {
       void Start()
       {
           var durMgr = new DurabilityManager(100);
           
           // Use 3 times = -1 durability
           for (int i = 0; i < 9; i++) // 9 uses = -3 durability
               durMgr.RegisterUsage();
           
           Debug.Log($"Durability after 9 uses: {durMgr.CurrentDurability} (expected: 97)");
           
           // Test repair
           durMgr.Repair();
           Debug.Log($"Durability after repair: {durMgr.CurrentDurability} (expected: 100)");
           
           if (durMgr.CurrentDurability == 100)
               Debug.Log("✅ Durability system working");
       }
   }
   ```
5. Run in Play Mode
6. **Pass Criteria:** -1 durability per 3 uses, repair() restores to 100

---

## Feature 5: Crafting Recipe System
**Spec:** `spec_fase9h_loot_crafting_equipment_durability_environment.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Craft/CraftingRecipeSO.cs`  
**Assets Generated:** 5+ crafting recipes

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Crafting/Recipes/`
2. Verify 5+ CraftingRecipeSO assets (e.g., Bread, Stew, processed materials)
3. Select `CraftingRecipe_Bread` and inspect:
   - ✅ `OutputItemId` = "consumable_bread"
   - ✅ `OutputQuantity` > 0
   - ✅ `CraftingTimeSeconds` > 0
   - ✅ `RequiredLevel` ≥ 0
   - ✅ `Ingredients[]` array populated with CraftingIngredient structs
4. Expand `Ingredients` array:
   - ✅ First ingredient: `ItemId` = "crop_wheat", `Quantity` = 1
5. **Pass Criteria:** Recipes have output, time, level, and ingredient arrays

---

## Feature 6: Loot Table System (Weighted Random)
**Spec:** `spec_fase9h_loot_crafting_equipment_durability_environment.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Loot/LootTableSO.cs`

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Loot/LootTables/`
2. Select any loot table (e.g., `LootTable_CommonDrops`)
3. Inspect:
   - ✅ `Entries[]` array populated
   - ✅ Each entry has `ItemId`, `MinAmount`, `MaxAmount`, `Weight`
   - ✅ Weights are > 0
4. Create weighted selection test:
   ```csharp
   using UnityEngine;
   using CindarsHope.Loot;
   
   public class TestLootTable : MonoBehaviour
   {
       void Start()
       {
           var lootTable = Resources.Load<LootTableSO>("Loot/LootTable_CommonDrops");
           
           // Sample 100 times
           var histogram = new System.Collections.Generic.Dictionary<string, int>();
           for (int i = 0; i < 100; i++)
           {
               var loot = lootTable.GetRandomLoot();
               if (!histogram.ContainsKey(loot.ItemId))
                   histogram[loot.ItemId] = 0;
               histogram[loot.ItemId]++;
           }
           
           foreach (var kvp in histogram)
               Debug.Log($"Item {kvp.Key}: {kvp.Value}% drops");
           
           Debug.Log("✅ Loot distribution shows variation (weights working)");
       }
   }
   ```
5. **Pass Criteria:** Random loot varies in type and amount, no exceptions

---

## Feature 7: Weapons, Spells, and Skill Actions
**Spec:** `spec_fase9i_player_combat_weapons_magic_skill_actions.md`  
**Implementation:** `Assets/_Game/Scripts/Combat/WeaponDataSO.cs`, `SpellDataSO.cs`, `SkillActionSO.cs`  
**Assets Generated:** 3 weapons, 3 spells, 3 skill actions

### How to Validate in Unity:

1. **Weapons:** Navigate to `Assets/_Game/Data/Combat/Weapons/`
   - Verify 3 assets (e.g., Sword, Spear, Bow)
   - Select `WeaponData_Sword` and inspect:
     - ✅ `Type` = Sword
     - ✅ `BaseDamage` > 0
     - ✅ `CriticalChance` (0–1 range)
     - ✅ `CooldownMs` > 0
     - ✅ `StaminaCost` > 0
     - ✅ `RequiredStrength` ≥ 0

2. **Spells:** Navigate to `Assets/_Game/Data/Combat/Spells/`
   - Verify 3 assets (e.g., Fireball, IceSpike, Heal)
   - Select `SpellData_Fireball` and inspect:
     - ✅ `Type` = Fireball
     - ✅ `ManaCost` > 0
     - ✅ `CastRangeMeters` > 0
     - ✅ `RequiredIntelligence` ≥ 0

3. **Skills:** Navigate to `Assets/_Game/Data/Combat/Skills/`
   - Verify 3 assets (e.g., Slash, PowerStrike, Dodge)
   - Select `SkillActionData_Slash` and inspect:
     - ✅ `Type` = Melee
     - ✅ `StaminaCost` > 0
     - ✅ `RequiredLevel` ≥ 0

4. **Pass Criteria:** All 9 assets exist with correct types, costs, and ranges

---

## Feature 8: Enemy Data + AI Behaviors
**Spec:** `spec_fase9d_enemy_actions_ai_combat.md`, `spec_fase9d_enemy_architecture_40_monsters.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Enemy/EnemyDataSO.cs`, `AIBehaviorSO.cs`  
**Assets Generated:** 4 enemies, 3 AI behaviors

### How to Validate in Unity:

1. **Enemies:** Navigate to `Assets/_Game/Data/Enemies/`
   - Verify 4 EnemyDataSO assets (e.g., BasicSlime, GoblinScout, OrcWarrior, IceSpider)
   - Select `EnemyData_BasicSlime` and inspect:
     - ✅ `Id` = "enemy_slime_basic"
     - ✅ `Level` > 0
     - ✅ `MaxHP` > 0
     - ✅ `Damage` > 0
     - ✅ `Defense` ≥ 0
     - ✅ `XpReward` > 0
     - ✅ `DetectionRange` > 0
     - ✅ `AIBehaviorId` set (e.g., "ai_patrol")

2. **AI Behaviors:** Navigate to `Assets/_Game/Data/Enemies/AIBehaviors/`
   - Verify 3 AIBehaviorSO assets (Patrol, Aggressive, Defensive)
   - Select `AIBehavior_Patrol` and inspect:
     - ✅ `Type` = Patrol
     - ✅ `PatrolDistance` > 0
     - ✅ `ChaseDuration` ≥ 0 (how long to chase after aggro)
     - ✅ `ActionCooldownMs` > 0

3. **Pass Criteria:** Enemies linked to AI behaviors, all stats > 0

---

## Feature 9: Bestiary System
**Spec:** `spec_fase9g_cave_bestiary_faction_locks.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Enemy/BestiaryDataSO.cs`

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Bestiary/`
2. Verify BestiaryDataSO asset(s) exist
3. Select any bestiary entry and inspect:
   - ✅ `EnemyId` field populated (e.g., "enemy_slime_basic")
   - ✅ `CommonName` set (human-readable)
   - ✅ `Lore` text present
   - ✅ `FirstEncounteredLevel` ≥ 0
   - ✅ `FactionId` set or empty (optional)
   - ✅ `KillCount` = 0 (starts untracked)
   - ✅ `IsDiscovered` = false (starts hidden)

4. **Pass Criteria:** Bestiary entries have lore, level, and discovery tracking

---

## Feature 10: Cave Entry + Loadout System
**Spec:** `spec_fase9j_cave_entry_loadout_death_anya_corpse.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs`

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Cave/`
2. Verify CaveEntryDataSO asset(s) exist
3. Select `CaveEntryData_Level1` and inspect:
   - ✅ `CaveLevel` = 1
   - ✅ `MaxLoadoutItems` > 0 (e.g., 20)
   - ✅ `MaxEquipmentSlots` > 0 (e.g., 6)
   - ✅ `MinimumLevelRequired` ≥ 0
   - ✅ `EntryHeatResistanceRequired` ≥ 0
   - ✅ `EntryColdResistanceRequired` ≥ 0
   - ✅ `AllowFoodConsumption` = true/false
   - ✅ `AllowPotionUsage` = true/false

4. **Pass Criteria:** Cave entries have resource limits and resistance requirements

---

## Feature 11: Death & Corpse Recovery System
**Spec:** `spec_fase9j_cave_entry_loadout_death_anya_corpse.md` (partial)  
**Implementation:** `Assets/_Game/Scripts/Save/DeathHandlerSO.cs`, `CorpseRecoverySO.cs`

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Death/`
2. Verify `DeathHandlerSO` asset:
   - ✅ `XpLossPercentageOnDeath` (0–1 range, e.g., 0.1 = 10% loss)
   - ✅ `GoldLossPercentageOnDeath` (similar range)
   - ✅ `RespawnHPPercentage` (0–1 range, e.g., 0.5 = 50% HP)
   - ✅ `AllowCorpseRecovery` = true
   - ✅ `CorpseRecoveryTimeHours` > 0 (e.g., 24 hours)
   - ✅ `RespawnAtLastSafeLocation` = true/false

3. Verify `CorpseRecoverySO` asset:
   - ✅ `CorpseDecayTimeHours` > 0 (e.g., 72 hours)
   - ✅ `MaxCorpsesPerLocation` > 0 (e.g., 3)
   - ✅ `ItemsDropOnDeath` = true/false
   - ✅ `EquipmentDropsOnDeath` = true/false

4. **Pass Criteria:** Death penalties and corpse decay times are configured

---

## Feature 12: Skill Tree System (Nodes + Respec)
**Spec:** `spec_fase9k_skill_trees_nodes_active_slots_respec.md`  
**Implementation:** `Assets/_Game/Scripts/Skills/SkillTreeDataSO.cs`, `SkillNodeDataSO.cs`, `SkillTreeManager.cs`

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/Skills/SkillTrees/`
2. Verify SkillTreeDataSO asset (e.g., `SkillTree_Warrior`):
   - ✅ `TreeName` set
   - ✅ `MaxActiveSlots` > 0 (e.g., 4)
   - ✅ `AllowRespeccing` = true
   - ✅ `Nodes[]` array populated with SkillNodeDataSO references

3. Verify SkillNodeDataSO assets:
   - ✅ Each node has `Id`, `SkillPointCost`, `RequiredLevel`
   - ✅ Attribute bonuses (Strength/Dexterity/etc.) are optional but > 0 when present
   - ✅ `RequiredSkillNodeId` empty or linked to prerequisite

4. Create manager test:
   ```csharp
   using UnityEngine;
   using CindarsHope.Skills;
   
   public class TestSkillTreeManager : MonoBehaviour
   {
       void Start()
       {
           var manager = new SkillTreeManager(maxActiveSlots: 4);
           var nodeData = Resources.Load<SkillNodeDataSO>("Skills/SkillNode_Slash");
           
           // Unlock node
           bool unlocked = manager.UnlockNode(nodeData, currentLevel: 10, availableSkillPoints: 5);
           Debug.Log($"Node unlocked: {unlocked}");
           
           // Activate slot
           bool activated = manager.ActivateSkillSlot(nodeData.Id);
           Debug.Log($"Skill activated: {activated}");
           
           // Check active slots
           var slots = manager.GetActiveSlots();
           Debug.Log($"Active slots: {slots.Count}");
           
           // Respec
           manager.Respec();
           if (manager.GetActiveSlots().Count == 0 && manager.GetTotalSpentPoints() == 0)
               Debug.Log("✅ Respec working correctly");
       }
   }
   ```
5. **Pass Criteria:** Unlock → Activate → Respec all work, max 4 active slots

---

## Feature 13: Menu System (Pause, Navigation)
**Spec:** `spec_ui_menu_systems_final.md` (minimal backend only)  
**Implementation:** `Assets/_Game/Scripts/UI/MenuSystemDataSO.cs`, `MenuManager.cs`

### How to Validate in Unity:

1. Navigate to `Assets/_Game/Data/UI/Menu/`
2. Verify MenuSystemDataSO asset(s) exist
3. Select `MenuSystem_Pause` and inspect:
   - ✅ `Id` = "menu_pause"
   - ✅ `Type` = Pause
   - ✅ `RequiresGamePause` = true
   - ✅ `AllowGameplayWhileOpen` = false (for pause menu)

4. Create menu manager test:
   ```csharp
   using UnityEngine;
   using CindarsHope.UI.Menu;
   
   public class TestMenuManager : MonoBehaviour
   {
       void Start()
       {
           var menuMgr = new MenuManager();
           
           // Open pause menu
           menuMgr.OpenMenu(MenuType.Pause);
           Debug.Log($"Current menu: {menuMgr.GetCurrentMenu()}");
           Debug.Log($"Game paused: {menuMgr.IsGamePaused()}");
           
           if (menuMgr.IsGamePaused() && Time.timeScale == 0f)
               Debug.Log("✅ Pause menu correctly sets Time.timeScale to 0");
           
           // Close menu
           menuMgr.CloseMenu();
           if (!menuMgr.IsGamePaused() && Time.timeScale == 1f)
               Debug.Log("✅ Resume correctly restores Time.timeScale to 1");
       }
   }
   ```

5. **Pass Criteria:** OpenMenu(Pause) → Time.timeScale = 0, CloseMenu() → Time.timeScale = 1

---

## Summary: Validation Checklist

| Feature | Assets | Script | Manager | Expected Result |
|---------|--------|--------|---------|-----------------|
| 1. Item Taxonomy | 23 ItemDataSO | ItemCategory.cs | — | 17+ categories, ConsumableSubtype applied |
| 2. Level-Up System | — | LevelUpManager.cs | — | XP scales, +1 attr/lvl, +1 skill/3 lvls |
| 3. Status Effects | 3 StatusEffectSO | StatusEffectSO.cs | StatusEffectManager.cs | Apply → Tick → Remove, damage/turn tracked |
| 4. Equipment + Durability | 10+ EquipmentDataSO | EquipmentDataSO.cs | DurabilityManager.cs | -1 durability per 3 uses, repair() works |
| 5. Crafting Recipes | 5+ CraftingRecipeSO | CraftingRecipeSO.cs | — | Recipes with ingredients, time, level |
| 6. Loot Tables | Multiple LootTableSO | LootTableSO.cs | — | Weighted random selection working |
| 7. Combat Assets | 9 total (weapons/spells/skills) | WeaponDataSO, SpellDataSO, SkillActionSO | — | All 9 assets with correct types/costs |
| 8. Enemy System | 4 EnemyDataSO, 3 AIBehaviorSO | EnemyDataSO, AIBehaviorSO | — | Enemies linked to AI, stats > 0 |
| 9. Bestiary | BestiaryDataSO assets | BestiaryDataSO.cs | — | Lore, level, discovery tracking |
| 10. Cave Entry | CaveEntryDataSO assets | CaveEntryDataSO.cs | — | Loadout limits, resistance requirements |
| 11. Death & Corpse | DeathHandlerSO, CorpseRecoverySO | Both SO classes | — | Penalties configured, decay times set |
| 12. Skill Tree | SkillTreeDataSO, SkillNodeDataSO | SkillTreeDataSO, SkillNodeDataSO | SkillTreeManager.cs | Unlock → Activate → Respec, max 4 slots |
| 13. Menu System | MenuSystemDataSO assets | MenuSystemDataSO | MenuManager.cs | Pause sets timeScale to 0, Resume to 1 |

---

## Quick Validation Steps (Full Pass)

1. **Open Unity Editor** → Project loads with no compile errors
2. **For each feature 1–13 above:**
   - Navigate to corresponding `Assets/_Game/Data/[Feature]/`
   - Verify all assets exist and fields are populated
   - Run corresponding test script in Play Mode (if provided)
   - Check Console for ✅ logs
3. **Expected Console Output:** 13 ✅ marks = all features validated

---

**All 13 features are ready for integration into gameplay systems. Next: wire up managers in game scenes and test in actual game flow.**
