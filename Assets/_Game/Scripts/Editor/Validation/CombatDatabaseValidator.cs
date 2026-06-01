#if UNITY_EDITOR

using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Validates cross-references in combat databases (items, weapons, spells, status effects).
    /// </summary>
    public class CombatDatabaseValidator : IProjectValidator
    {
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";

        // Hardcoded hotbar defaults from SaveManager.Initialize()
        private readonly string[] _hotbarDefaults = new[]
        {
            "item_seed_wheat",
            "item_seed_carrot",
            "item_tool_fishing_rod_basic",
            "item_weapon_bow_basic",
            "item_ammo_arrow_basic",
            "item_spell_fireball_test"
        };

        public string ValidatorId => "combat_database_validator";
        public string DisplayName => "Combat Database Validator";

        public ValidationReport Run()
        {
            var report = new ValidationReport();

            // Load databases
            var itemDb = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            var weaponDb = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            var spellDb = AssetDatabase.LoadAssetAtPath<SpellDatabaseSO>(SpellDatabasePath);
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);

            if (itemDb == null)
            {
                report.AddIssue("Database", "ITEM_DB_MISSING", ValidationSeverity.Error,
                    "ItemDatabase not found.", ItemDatabasePath, "ItemDatabase", "Load ItemDatabase.asset");
                return report; // Cannot continue without ItemDatabase
            }

            // Validate ItemDataSO cross-references
            ValidateItemData(itemDb, weaponDb, spellDb, report);

            // Validate Weapon and Spell specifics
            if (weaponDb != null)
                ValidateWeaponData(weaponDb, report);

            if (spellDb != null)
                ValidateSpellData(spellDb, report);

            // Validate PlayerData starting items
            if (playerData != null)
                ValidatePlayerData(playerData, itemDb, report);

            // Validate hotbar defaults
            ValidateHotbarDefaults(itemDb, report);

            return report;
        }

        private void ValidateItemData(ItemDatabaseSO itemDb, WeaponDatabaseSO weaponDb, SpellDatabaseSO spellDb, ValidationReport report)
        {
            var items = itemDb.All;
            if (items == null || items.Count == 0)
                return;

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                // FR-001: Weapon items must have valid WeaponId
                if (item.Category == ItemCategory.Weapon && item.IsEquippable)
                {
                    if (string.IsNullOrEmpty(item.WeaponId))
                    {
                        var assetPath = AssetDatabase.GetAssetPath(item);
                        report.AddIssue("ItemData", "WEAPON_ITEM_NO_WEAPON_ID", ValidationSeverity.Error,
                            $"Weapon item '{item.DisplayName}' (ID: {item.Id}) is equippable but has no WeaponId.",
                            assetPath, item.DisplayName, "Set WeaponId in ItemDataSO.");
                    }
                    else if (weaponDb != null && !weaponDb.TryGetById(item.WeaponId, out _))
                    {
                        var assetPath = AssetDatabase.GetAssetPath(item);
                        report.AddIssue("ItemData", "WEAPON_ID_NOT_IN_DB", ValidationSeverity.Error,
                            $"Weapon item '{item.DisplayName}' references WeaponId '{item.WeaponId}' not in WeaponDatabase.",
                            assetPath, item.DisplayName, "Add weapon to WeaponDatabase or fix WeaponId.");
                    }
                }

                // FR-002: Magic items must have valid SpellId
                if (item.Category == ItemCategory.Magic && item.IsEquippable)
                {
                    if (string.IsNullOrEmpty(item.SpellId))
                    {
                        var assetPath = AssetDatabase.GetAssetPath(item);
                        report.AddIssue("ItemData", "MAGIC_ITEM_NO_SPELL_ID", ValidationSeverity.Error,
                            $"Magic item '{item.DisplayName}' (ID: {item.Id}) is equippable but has no SpellId.",
                            assetPath, item.DisplayName, "Set SpellId in ItemDataSO.");
                    }
                    else if (spellDb != null && !spellDb.TryGetById(item.SpellId, out _))
                    {
                        var assetPath = AssetDatabase.GetAssetPath(item);
                        report.AddIssue("ItemData", "SPELL_ID_NOT_IN_DB", ValidationSeverity.Error,
                            $"Magic item '{item.DisplayName}' references SpellId '{item.SpellId}' not in SpellDatabase.",
                            assetPath, item.DisplayName, "Add spell to SpellDatabase or fix SpellId.");
                    }
                }

                // FR-003: Ammo items should be stackable and equippable
                if (item.Category == ItemCategory.Ammo)
                {
                    if (item.MaxStack <= 1)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(item);
                        report.AddIssue("ItemData", "AMMO_LOW_STACK", ValidationSeverity.Warning,
                            $"Ammo item '{item.DisplayName}' has MaxStack {item.MaxStack} (should be > 1).",
                            assetPath, item.DisplayName, "Set MaxStack > 1 for stackable ammo.");
                    }

                    if (!item.IsEquippable)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(item);
                        report.AddIssue("ItemData", "AMMO_NOT_EQUIPPABLE", ValidationSeverity.Warning,
                            $"Ammo item '{item.DisplayName}' is not equippable.",
                            assetPath, item.DisplayName, "Set IsEquippable = true if used in hands.");
                    }
                }
            }
        }

        private void ValidateWeaponData(WeaponDatabaseSO weaponDb, ValidationReport report)
        {
            var weapons = weaponDb.All;
            if (weapons == null || weapons.Count == 0)
                return;

            foreach (var weapon in weapons)
            {
                if (weapon == null || weapon.Type != WeaponType.Bow)
                    continue;

                // FR-004: Bow must have valid projectile config
                var assetPath = AssetDatabase.GetAssetPath(weapon);

                if (weapon.Range <= 0)
                {
                    report.AddIssue("Weapon", "BOW_INVALID_RANGE", ValidationSeverity.Error,
                        $"Bow '{weapon.DisplayName}' has invalid Range: {weapon.Range}",
                        assetPath, weapon.DisplayName, "Set Range > 0");
                }

                if (weapon.ProjectileSpeed <= 0)
                {
                    report.AddIssue("Weapon", "BOW_INVALID_SPEED", ValidationSeverity.Error,
                        $"Bow '{weapon.DisplayName}' has invalid ProjectileSpeed: {weapon.ProjectileSpeed}",
                        assetPath, weapon.DisplayName, "Set ProjectileSpeed > 0");
                }

                if (weapon.ProjectilePrefab == null)
                {
                    report.AddIssue("Weapon", "BOW_NO_PROJECTILE", ValidationSeverity.Error,
                        $"Bow '{weapon.DisplayName}' has no ProjectilePrefab.",
                        assetPath, weapon.DisplayName, "Assign ProjectilePrefab in WeaponDataSO.");
                }
            }
        }

        private void ValidateSpellData(SpellDatabaseSO spellDb, ValidationReport report)
        {
            var spells = spellDb.All;
            if (spells == null || spells.Count == 0)
                return;

            foreach (var spell in spells)
            {
                if (spell == null || spell.Type != SpellType.Fireball)
                    continue;

                // FR-005: Fireball must have valid projectile config
                var assetPath = AssetDatabase.GetAssetPath(spell);

                if (spell.Range <= 0)
                {
                    report.AddIssue("Spell", "FIREBALL_INVALID_RANGE", ValidationSeverity.Error,
                        $"Fireball '{spell.SpellName}' has invalid Range: {spell.Range}",
                        assetPath, spell.SpellName, "Set Range > 0");
                }

                if (spell.ProjectileSpeed <= 0)
                {
                    report.AddIssue("Spell", "FIREBALL_INVALID_SPEED", ValidationSeverity.Error,
                        $"Fireball '{spell.SpellName}' has invalid ProjectileSpeed: {spell.ProjectileSpeed}",
                        assetPath, spell.SpellName, "Set ProjectileSpeed > 0");
                }

                if (spell.ProjectilePrefab == null)
                {
                    report.AddIssue("Spell", "FIREBALL_NO_PROJECTILE", ValidationSeverity.Error,
                        $"Fireball '{spell.SpellName}' has no ProjectilePrefab.",
                        assetPath, spell.SpellName, "Assign ProjectilePrefab in SpellDataSO.");
                }

                // FR-006: StatusEffect validation (optional in MVP, log as warning if missing)
                if (!string.IsNullOrEmpty(spell.StatusEffectId))
                {
                    // Try to load StatusEffect via Resources or database
                    var statusEffect = Resources.Load($"status_effects/{spell.StatusEffectId}");
                    if (statusEffect == null)
                    {
                        // Could be in a database; for MVP we log warning
                        report.AddIssue("Spell", "STATUSEFFECT_NOT_FOUND", ValidationSeverity.Warning,
                            $"Fireball '{spell.SpellName}' references StatusEffectId '{spell.StatusEffectId}' not found.",
                            assetPath, spell.SpellName, "Ensure StatusEffect asset exists in Resources or database.");
                    }
                }
            }
        }

        private void ValidatePlayerData(PlayerDataSO playerData, ItemDatabaseSO itemDb, ValidationReport report)
        {
            if (playerData.StartingItems == null || playerData.StartingItems.Length == 0)
                return;

            // FR-007: Starting items validation
            for (int i = 0; i < playerData.StartingItems.Length; i++)
            {
                var startingItem = playerData.StartingItems[i];
                var assetPath = AssetDatabase.GetAssetPath(playerData);

                if (startingItem.Item == null)
                {
                    report.AddIssue("PlayerData", "STARTING_ITEM_NULL", ValidationSeverity.Error,
                        $"StartingItems[{i}] has null Item reference.",
                        assetPath, "StartingItems", "Set Item in PlayerDataSO StartingItems list.");
                    continue;
                }

                if (startingItem.Amount <= 0)
                {
                    report.AddIssue("PlayerData", "STARTING_ITEM_ZERO_AMOUNT", ValidationSeverity.Warning,
                        $"StartingItems[{i}] ({startingItem.Item.DisplayName}) has Amount {startingItem.Amount}.",
                        assetPath, "StartingItems", "Set Amount > 0");
                }

                if (!itemDb.TryGetById(startingItem.Item.Id, out _))
                {
                    report.AddIssue("PlayerData", "STARTING_ITEM_NOT_IN_DB", ValidationSeverity.Error,
                        $"StartingItems[{i}] ({startingItem.Item.DisplayName}) not in ItemDatabase.",
                        assetPath, "StartingItems", "Add item to ItemDatabase.");
                }
            }
        }

        private void ValidateHotbarDefaults(ItemDatabaseSO itemDb, ValidationReport report)
        {
            // FR-008: Hotbar default validation
            var assetPath = "Assets/_Game/Scripts/Save/SaveManager.cs";

            for (int i = 0; i < _hotbarDefaults.Length; i++)
            {
                var itemId = _hotbarDefaults[i];

                if (string.IsNullOrEmpty(itemId))
                    continue;

                if (!itemDb.TryGetById(itemId, out _))
                {
                    report.AddIssue("Hotbar", "HOTBAR_ITEM_NOT_IN_DB", ValidationSeverity.Error,
                        $"Hotbar slot {i} default ID '{itemId}' not in ItemDatabase.",
                        assetPath, "SaveManager", "Add item to ItemDatabase or update hotbar defaults in SaveManager.Initialize().");
                }
            }
        }
    }
}

#endif
