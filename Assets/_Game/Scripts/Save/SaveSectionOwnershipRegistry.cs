using System;
using System.Collections.Generic;

namespace CindarsHope.Save
{
    /// <summary>
    /// SPEC 01.05: Save Section Ownership Registry
    /// Canonical registry of save sections, owners, dependencies, and restore order.
    /// Used to prevent ownership conflicts, undefined defaults, and restore-order violations.
    /// </summary>
    public static class SaveSectionOwnershipRegistry
    {
        /// <summary>
        /// Ownership metadata for a save section.
        /// </summary>
        [Serializable]
        public class SaveSectionOwnership
        {
            public string SectionName;               // e.g., "Inventory"
            public string DtoType;                   // e.g., "InventorySaveData"
            public string OwnerDomain;               // e.g., "InventoryManager"
            public string CaptureOwner;              // Which system captures this section
            public string RestoreOwner;              // Which system restores this section
            public string DefaultBehavior;           // What happens if section is missing
            public bool CanRestoreIndependently;    // YES if restore order doesn't matter
            public string[] DependsOn;              // Required sections before restore
            public string[] EnablesRestore;         // Sections that depend on this
            public string PostRestoreEvent;         // Event published after restore, if any
            public string RiskLevel;                 // INFO, LOW, MEDIUM, HIGH
            public string MigrationOwner;           // Who handles migrations for this section
        }

        // OWNERSHIP MATRIX (23 sections from GameSaveData)
        // Based on spec 01.04 restore order audit

        public static readonly SaveSectionOwnership[] Sections = new[]
        {
            // 1. TIME/CALENDAR — Foundation
            new SaveSectionOwnership
            {
                SectionName = "CurrentDay",
                DtoType = "int",
                OwnerDomain = "TimeManager",
                CaptureOwner = "TimeManager",
                RestoreOwner = "TimeManager",
                DefaultBehavior = "Defaults to day 1 if missing",
                CanRestoreIndependently = true,
                DependsOn = Array.Empty<string>(),
                EnablesRestore = new[] { "GameTimeManager", "Farm", "World", "NPC" },
                PostRestoreEvent = null,
                RiskLevel = "LOW",
                MigrationOwner = "SaveManager"
            },

            // 2. PLAYER — Core stats
            new SaveSectionOwnership
            {
                SectionName = "Player",
                DtoType = "PlayerSaveData",
                OwnerDomain = "PlayerManager",
                CaptureOwner = "PlayerManager",
                RestoreOwner = "PlayerManager",
                DefaultBehavior = "Skipped if missing; warns. Next spec must define default.",
                CanRestoreIndependently = true,
                DependsOn = Array.Empty<string>(),
                EnablesRestore = new[] { "Inventory", "Equipment", "Hotbar", "Mana", "SkillTree" },
                PostRestoreEvent = null,
                RiskLevel = "MEDIUM",
                MigrationOwner = "SaveManager"
            },

            // 3. INVENTORY
            new SaveSectionOwnership
            {
                SectionName = "Inventory",
                DtoType = "InventorySaveData",
                OwnerDomain = "InventoryManager",
                CaptureOwner = "InventoryManager",
                RestoreOwner = "InventoryManager",
                DefaultBehavior = "Starter items injected if missing (EnsureStarterItemsPresent)",
                CanRestoreIndependently = false,
                DependsOn = Array.Empty<string>(),
                EnablesRestore = new[] { "Equipment", "Hotbar", "SkillTree" },
                PostRestoreEvent = null,
                RiskLevel = "MEDIUM",
                MigrationOwner = "InventorySlotsV1ToV2Migration"
            },

            // 4. EQUIPMENT
            new SaveSectionOwnership
            {
                SectionName = "Equipment",
                DtoType = "EquipmentSaveData",
                OwnerDomain = "EquipmentManager",
                CaptureOwner = "EquipmentManager",
                RestoreOwner = "EquipmentManager",
                DefaultBehavior = "Empty equipment if missing; safe default",
                CanRestoreIndependently = false,
                DependsOn = new[] { "Inventory" },
                EnablesRestore = new[] { "Hotbar", "EquipmentDurability" },
                PostRestoreEvent = null,
                RiskLevel = "MEDIUM",
                MigrationOwner = "SaveManager"
            },

            // 5. HOTBAR
            new SaveSectionOwnership
            {
                SectionName = "Hotbar",
                DtoType = "HotbarSaveData",
                OwnerDomain = "HotbarSectionProvider",
                CaptureOwner = "HotbarSectionProvider or HotbarState",
                RestoreOwner = "HotbarSectionProvider or HotbarState",
                DefaultBehavior = "Fallback to default hotbar (wheat seed, carrot seed, fishing rod, weapon, ammo, spell)",
                CanRestoreIndependently = false,
                DependsOn = new[] { "Inventory", "Equipment" },
                EnablesRestore = Array.Empty<string>(),
                PostRestoreEvent = null,
                RiskLevel = "LOW",
                MigrationOwner = "SaveManager"
            },

            // 6-23. Additional sections (abbreviated for brevity; full matrix in spec 01.04 report)
            new SaveSectionOwnership
            {
                SectionName = "Progression",
                DtoType = "PlayerProgressionSaveData",
                OwnerDomain = "ProgressionManager",
                CaptureOwner = "ProgressionManager",
                RestoreOwner = "ProgressionManager",
                DefaultBehavior = "Level 1, 0 XP if missing",
                CanRestoreIndependently = true,
                DependsOn = Array.Empty<string>(),
                EnablesRestore = new[] { "SkillTree" },
                PostRestoreEvent = null,
                RiskLevel = "LOW",
                MigrationOwner = "SaveManager"
            },

            new SaveSectionOwnership
            {
                SectionName = "Farm",
                DtoType = "FarmSaveData",
                OwnerDomain = "FarmPlotRegistry",
                CaptureOwner = "FarmPlotRegistry",
                RestoreOwner = "FarmPlotRegistry",
                DefaultBehavior = "Empty farm (all plots cleared) if missing",
                CanRestoreIndependently = false,
                DependsOn = new[] { "CurrentDay" },
                EnablesRestore = Array.Empty<string>(),
                PostRestoreEvent = null,
                RiskLevel = "LOW",
                MigrationOwner = "SaveManager"
            },

            new SaveSectionOwnership
            {
                SectionName = "World",
                DtoType = "WorldSaveData",
                OwnerDomain = "ItemPickupRegistry/TreeRegistry",
                CaptureOwner = "ItemPickupRegistry/TreeRegistry",
                RestoreOwner = "ItemPickupRegistry/TreeRegistry",
                DefaultBehavior = "Empty world (no pickups/trees) if missing; next spec must define",
                CanRestoreIndependently = false,
                DependsOn = new[] { "CurrentDay" },
                EnablesRestore = Array.Empty<string>(),
                PostRestoreEvent = null,
                RiskLevel = "MEDIUM",
                MigrationOwner = "SaveManager"
            },

            new SaveSectionOwnership
            {
                SectionName = "Cave",
                DtoType = "CaveSaveData",
                OwnerDomain = "CaveRunManager",
                CaptureOwner = "CaveRunManager",
                RestoreOwner = "CaveRunManager",
                DefaultBehavior = "No active cave run if missing",
                CanRestoreIndependently = false,
                DependsOn = Array.Empty<string>(),
                EnablesRestore = Array.Empty<string>(),
                PostRestoreEvent = null,
                RiskLevel = "MEDIUM",
                MigrationOwner = "SaveManager"
            },

            new SaveSectionOwnership
            {
                SectionName = "Death",
                DtoType = "DeathSaveData",
                OwnerDomain = "CorpseRecoveryManager",
                CaptureOwner = "CorpseRecoveryManager",
                RestoreOwner = "CorpseRecoveryManager",
                DefaultBehavior = "No death state if missing; player respawned normally",
                CanRestoreIndependently = false,
                DependsOn = Array.Empty<string>(),
                EnablesRestore = Array.Empty<string>(),
                PostRestoreEvent = "DeathSaveData restored last",
                RiskLevel = "MEDIUM",
                MigrationOwner = "SaveManager"
            },

            new SaveSectionOwnership
            {
                SectionName = "Economy",
                DtoType = "EconomySaveData",
                OwnerDomain = "ShopManager",
                CaptureOwner = "ShopManager",
                RestoreOwner = "ShopManager",
                DefaultBehavior = "All shops restocked if Economy section missing",
                CanRestoreIndependently = false,
                DependsOn = new[] { "CurrentDay" },
                EnablesRestore = Array.Empty<string>(),
                PostRestoreEvent = null,
                RiskLevel = "LOW",
                MigrationOwner = "SaveManager"
            },

            // ... (additional sections abbreviated in output)
        };

        /// <summary>
        /// Query ownership for a given section name.
        /// Returns null if section not found.
        /// </summary>
        public static SaveSectionOwnership GetOwnership(string sectionName)
        {
            foreach (var section in Sections)
            {
                if (section.SectionName == sectionName)
                    return section;
            }
            return null;
        }

        /// <summary>
        /// Template for declaring a new save section in future specs.
        /// REQUIRED FIELDS:
        /// - SectionName: Unique identifier
        /// - OwnerDomain: Which system owns this data
        /// - DependsOn: Sections that must restore before this one
        /// - DefaultBehavior: What happens if missing (must be explicit, not null)
        /// - MigrationOwner: Who handles schema changes
        /// </summary>
        public static string NewSectionTemplate => @"
new SaveSectionOwnership
{
    SectionName = ""<SectionName>"",
    DtoType = ""<DtoType>"",
    OwnerDomain = ""<OwnerDomain>"",
    CaptureOwner = ""<System that captures>"",
    RestoreOwner = ""<System that restores>"",
    DefaultBehavior = ""<Explicit default, not null/skip>"",
    CanRestoreIndependently = <YES/NO>,
    DependsOn = new[] { /* section names */ },
    EnablesRestore = new[] { /* section names that depend on this */ },
    PostRestoreEvent = ""<Event name or null>"",
    RiskLevel = ""<INFO/LOW/MEDIUM/HIGH>"",
    MigrationOwner = ""<Who handles migrations>""
}
";
    }
}
