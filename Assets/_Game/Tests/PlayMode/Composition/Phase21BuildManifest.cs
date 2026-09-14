using System.Collections.Generic;
using CindarsHope.Combat.Telemetry;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public static class Phase21BuildManifest
    {
        public const int Level20 = 20, Level50 = 50, Level54 = 54, Rank1 = 1, Rank3 = 3;
        public static readonly IReadOnlyList<int> Levels = new[] { Level20, Level50, Level54 };
        public static readonly IReadOnlyList<int> Ranks = new[] { Rank1, Rank3 };

        // Ranged is intentionally one L20 baseline; it is not three active lunar variants.
        public static readonly IReadOnlyList<Phase21ActionDefinition> Level20Actions = new[]
        {
            new Phase21ActionDefinition("melee_pre_capstone", "skill_melee_whirl_cut", "item_weapon_sword_iron"),
            new Phase21ActionDefinition("ranged_pre_capstone", "skill_ranged_charged_shot", "item_weapon_bow_wood"),
            new Phase21ActionDefinition("magic_anya_pre_capstone", "skill_magic_ice_bind", "item_weapon_staff_oak"),
            new Phase21ActionDefinition("magic_senya_pre_capstone", "skill_magic_chama_breve", "item_weapon_staff_oak"),
        };

        public static readonly IReadOnlyList<Phase21BuildDefinition> Builds = new[]
        {
            Build("melee_kanthor", "melee", "kanthor", "melee_capstone_battle_rhythm", "skill_melee_whirl_cut", "item_weapon_sword_iron", "item_weapon_sword_steel"),
            Build("melee_kaand", "melee", "kaand", "melee_capstone_battle_rhythm", "skill_melee_whirl_cut", "item_weapon_sword_iron", "item_weapon_sword_steel"),
            Build("ranged_alihana", "ranged", "alihana", "ranged_capstone_eagle_focus", "skill_ranged_charged_shot", "item_weapon_bow_wood", "item_weapon_bow_steel"),
            Build("ranged_senya", "ranged", "senya", "ranged_capstone_eagle_focus", "skill_ranged_charged_shot", "item_weapon_bow_wood", "item_weapon_bow_steel"),
            Build("ranged_nyx", "ranged", "nyx", "ranged_capstone_eagle_focus", "skill_ranged_charged_shot", "item_weapon_bow_wood", "item_weapon_bow_steel"),
            Build("magic_anya", "magic", "anya", "magic_capstone_elemental_confluence", "skill_magic_ice_bind", "item_weapon_staff_oak", "item_weapon_staff_meteoric"),
            Build("magic_senya", "magic", "senya", "magic_capstone_elemental_confluence", "skill_magic_chama_breve", "item_weapon_staff_oak", "item_weapon_staff_meteoric"),
            Build("survival_telisandra", "survival", null, "survival_capstone_caveborn", null, null, null),
            Build("crafting_thoren", "crafting", null, "crafting_capstone_master_artisan", null, null, null),
        };

        public static readonly IReadOnlyList<Phase21ScenarioDefinition> Scenarios = new[]
        {
            new Phase21ScenarioDefinition("phase21_l20", Level20, "phase21_seed_l20", new[]
            { Target("enemy_spore_imp", TelemetryCreatureClass.Common), Target("enemy_spore_imp", TelemetryCreatureClass.Elite), TargetCatalogOnly("enemy_packlord_ruvash", TelemetryCreatureClass.Miniboss, "Catalog entry absent from the fixed L20 Cave spawn plan."), TargetCatalogOnly("enemy_fungal_patriarch", TelemetryCreatureClass.Boss, "No cave boss gate at L20.") }),
            new Phase21ScenarioDefinition("phase21_l50", Level50, "phase21_seed_l50", new[]
            { Target("enemy_ash_crawler", TelemetryCreatureClass.Common), Target("enemy_furnace_warden", TelemetryCreatureClass.Elite), TargetCatalogOnly("enemy_ashwing_matriarch", TelemetryCreatureClass.Miniboss, "Catalog entry absent from the fixed L50 Cave spawn plan."), TargetCatalogOnly("enemy_cindershard_wyrm", TelemetryCreatureClass.Boss, "No cave boss gate at L50.") }),
            // L54 stays in the authored fourth cave band; this is a separate rank-3 budget row,
            // not a claim that its catalog-only Boss has a playable gate.
            new Phase21ScenarioDefinition("phase21_l54", Level54, "phase21_seed_l54", new[]
            { Target("enemy_ash_crawler", TelemetryCreatureClass.Common), Target("enemy_furnace_warden", TelemetryCreatureClass.Elite), TargetCatalogOnly("enemy_ashwing_matriarch", TelemetryCreatureClass.Miniboss, "Catalog entry absent from the fixed L54 Cave spawn plan."), TargetCatalogOnly("enemy_cindershard_wyrm", TelemetryCreatureClass.Boss, "No cave boss gate at L54.") }),
        };

        private static Phase21BuildDefinition Build(string id, string discipline, string variant, string capstone, string action, string l20, string l50)
            => new Phase21BuildDefinition(id, discipline, variant, capstone, action, l20 == null ? null : new Phase21EquipmentFixture(l20, Level20), l50 == null ? null : new Phase21EquipmentFixture(l50, Level50));
        private static Phase21TargetDefinition Target(string id, TelemetryCreatureClass type) => new Phase21TargetDefinition(id, type, Phase21TargetAvailability.Available);
        private static Phase21TargetDefinition TargetUnavailable(TelemetryCreatureClass type, string reason) => new Phase21TargetDefinition(null, type, Phase21TargetAvailability.Unavailable, reason);
        private static Phase21TargetDefinition TargetCatalogOnly(string id, TelemetryCreatureClass type, string reason) => new Phase21TargetDefinition(id, type, Phase21TargetAvailability.CatalogOnly, reason);
    }

    public sealed class Phase21ActionDefinition
    {
        public Phase21ActionDefinition(string id, string actionId, string equipmentItemId)
        { Id = id; ActionId = actionId; EquipmentItemId = equipmentItemId; }
        public string Id { get; }
        public string ActionId { get; }
        public string EquipmentItemId { get; }
    }
    public sealed class Phase21BuildDefinition
    {
        public Phase21BuildDefinition(string id, string discipline, string variant, string capstone, string action, Phase21EquipmentFixture l20, Phase21EquipmentFixture l50)
        { BuildId = id; Discipline = discipline; VariantId = variant; CapstoneNodeId = capstone; OffensiveActionId = action; Level20Equipment = l20; Level50Equipment = l50; ManifestFields = l20 == null || l50 == null ? Phase21ManifestFields.Missing : Phase21ManifestFields.Specified; }
        public string BuildId { get; }
        public string Discipline { get; }
        public string VariantId { get; }
        public string CapstoneNodeId { get; }
        public string OffensiveActionId { get; }
        public Phase21EquipmentFixture Level20Equipment { get; }
        public Phase21EquipmentFixture Level50Equipment { get; }
        public Phase21ManifestFields ManifestFields { get; }
    }
    public sealed class Phase21EquipmentFixture
    {
        public Phase21EquipmentFixture(string itemId, int level) { ItemId = itemId; Level = level; }
        public string ItemId { get; }
        public int Level { get; }
        public Phase21EquipmentSelection Selection { get; } = Phase21EquipmentSelection.Fixture;
    }
    public sealed class Phase21ScenarioDefinition
    {
        public Phase21ScenarioDefinition(string id, int level, string seed, IReadOnlyList<Phase21TargetDefinition> targets) { ScenarioId = id; Level = level; Seed = seed; Targets = targets; }
        public string ScenarioId { get; }
        public int Level { get; }
        public string Seed { get; }
        public IReadOnlyList<Phase21TargetDefinition> Targets { get; }
    }
    public sealed class Phase21TargetDefinition
    {
        public Phase21TargetDefinition(string id, TelemetryCreatureClass type, Phase21TargetAvailability availability, string reason = null) { EnemyId = id; CreatureClass = type; Availability = availability; UnavailableReason = reason; }
        public string EnemyId { get; }
        public TelemetryCreatureClass CreatureClass { get; }
        public Phase21TargetAvailability Availability { get; }
        public string UnavailableReason { get; }
    }
    public enum Phase21EquipmentSelection { Fixture }
    public enum Phase21ManifestFields { Specified, Missing }
    public enum Phase21TargetAvailability { Available, CatalogOnly, Unavailable }
}
