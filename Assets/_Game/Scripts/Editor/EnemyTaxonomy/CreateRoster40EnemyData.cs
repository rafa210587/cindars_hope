using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// SPEC 13B — Creates the official 40-enemy roster as EnemyDataSO assets.
    /// Run via: CindarsHope > SPEC 13 > Create Roster 40 Enemy Data
    /// Skips assets that already exist (by enemyId) to avoid overwriting manual edits.
    /// </summary>
    public static class CreateRoster40EnemyData
    {
        private const string RosterFolder = "Assets/_Game/Data/Enemies/Roster";

        private struct RosterEntry
        {
            public string EnemyId;
            public string DisplayName;
            public string LoreTagline;
            public string FactionId;
            public EnemyRole PrimaryRole;
            public EnemyRole[] SecondaryRoles;
            public string MovementProfileId;
            public string SizeProfileId;
            public string ActionSetId;
            public string VulnerabilityProfileId;
            public string PrimaryDamageTypeId;
            public string BestiaryEntryId;
            public int XpReward;
            public int CaveBand;
            public int MaxHp;
            public int ContactDamage;
            public bool IsElite;
            public bool IsMiniBoss;
            public bool IsBoss;
        }

        [MenuItem("CindarsHope/SPEC 13/Create Roster 40 Enemy Data")]
        public static void CreateRoster()
        {
            EnsureFolder(RosterFolder);

            int created = 0;
            int updated = 0;

            foreach (var entry in BuildCanonicalRoster())
            {
                string assetPath = $"{RosterFolder}/{entry.EnemyId}.asset";
                var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(assetPath);

                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<EnemyDataSO>();
                    AssetDatabase.CreateAsset(so, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }

                so.enemyId              = entry.EnemyId;
                so.DisplayName          = entry.DisplayName;
                so.LoreTagline          = entry.LoreTagline;
                so.FactionId            = entry.FactionId;
                so.PrimaryRole          = entry.PrimaryRole;
                so.SecondaryRoles       = entry.SecondaryRoles ?? new EnemyRole[0];
                so.MovementProfileId    = entry.MovementProfileId;
                so.SizeProfileId        = entry.SizeProfileId;
                so.ActionSetId          = entry.ActionSetId;
                so.VulnerabilityProfileId = entry.VulnerabilityProfileId;
                so.PrimaryDamageTypeId  = entry.PrimaryDamageTypeId;
                so.BestiaryEntryId      = entry.BestiaryEntryId;
                so.xpReward             = entry.XpReward;
                so.CaveBand             = entry.CaveBand;
                so.maxHp                = entry.MaxHp;
                so.contactDamage        = entry.ContactDamage;
                so.IsElite              = entry.IsElite;
                so.IsMiniBoss           = entry.IsMiniBoss;
                so.IsBoss               = entry.IsBoss;

                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SPEC 13B] Canonical roster creation complete. Created: {created}, Updated: {updated}.");
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static List<RosterEntry> BuildRoster()
        {
            return new List<RosterEntry>
            {
                // ── BAND 1 (CaveBand=1, faixas 1-10) ─────────────────────────────────

                new RosterEntry
                {
                    EnemyId = "enemy_verdant_mite",
                    DisplayName = "Verdant Mite",
                    LoreTagline = "Swarms from rotting bark, hungry for warmth.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Swarm,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_swarm_erratic",
                    SizeProfileId = "size_tiny",
                    VulnerabilityProfileId = "vuln_swarm_after_bite",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_verdant_mite",
                    XpReward = 8, CaveBand = 1, MaxHp = 6, ContactDamage = 2
                },
                new RosterEntry
                {
                    EnemyId = "enemy_spore_crawler",
                    DisplayName = "Spore Crawler",
                    LoreTagline = "Pulsating with bioluminescent spores that cloud the mind.",
                    FactionId = "faction_fungal",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_spore_crawler",
                    XpReward = 12, CaveBand = 1, MaxHp = 10, ContactDamage = 3
                },
                new RosterEntry
                {
                    EnemyId = "enemy_goblin_scrounger",
                    DisplayName = "Goblin Scrounger",
                    LoreTagline = "Furtive and quick, always one step ahead of trouble.",
                    FactionId = "faction_goblin",
                    PrimaryRole = EnemyRole.Ranged,
                    SecondaryRoles = new[] { EnemyRole.Chaser },
                    MovementProfileId = "movement_kite_ranged",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_ranged_after_volley",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_goblin_scrounger",
                    XpReward = 14, CaveBand = 1, MaxHp = 12, ContactDamage = 2
                },
                new RosterEntry
                {
                    EnemyId = "enemy_kobold_sentry",
                    DisplayName = "Kobold Sentry",
                    LoreTagline = "Watches over nest-tunnels with unwavering loyalty.",
                    FactionId = "faction_kobold",
                    PrimaryRole = EnemyRole.Guard,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_guard_stationary",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_kobold_sentry",
                    XpReward = 14, CaveBand = 1, MaxHp = 14, ContactDamage = 3
                },
                new RosterEntry
                {
                    EnemyId = "enemy_rot_beetle",
                    DisplayName = "Rot Beetle",
                    LoreTagline = "Shell hardened by cave minerals, mandibles dripping decay.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_rot_beetle",
                    XpReward = 10, CaveBand = 1, MaxHp = 8, ContactDamage = 2
                },
                new RosterEntry
                {
                    EnemyId = "enemy_pale_grub",
                    DisplayName = "Pale Grub",
                    LoreTagline = "Eyeless larvae that sense heat with uncanny precision.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Swarm,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_swarm_erratic",
                    SizeProfileId = "size_tiny",
                    VulnerabilityProfileId = "vuln_swarm_after_bite",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_pale_grub",
                    XpReward = 6, CaveBand = 1, MaxHp = 4, ContactDamage = 1
                },
                new RosterEntry
                {
                    EnemyId = "enemy_mushroom_puffball",
                    DisplayName = "Mushroom Puffball",
                    LoreTagline = "Detonates in a cloud of toxic spores when threatened.",
                    FactionId = "faction_fungal",
                    PrimaryRole = EnemyRole.Guard,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_guard_stationary",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_mushroom_puffball",
                    XpReward = 12, CaveBand = 1, MaxHp = 10, ContactDamage = 2
                },

                // ── BAND 2 (CaveBand=2, faixas 11-25) ────────────────────────────────

                new RosterEntry
                {
                    EnemyId = "enemy_goblin_shaman",
                    DisplayName = "Goblin Shaman",
                    LoreTagline = "Channels totemic spirits to curse intruders.",
                    FactionId = "faction_goblin",
                    PrimaryRole = EnemyRole.Caster,
                    SecondaryRoles = new[] { EnemyRole.Ranged },
                    MovementProfileId = "movement_caster_keep_away",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_caster_after_cast",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_goblin_shaman",
                    XpReward = 28, CaveBand = 2, MaxHp = 22, ContactDamage = 2
                },
                new RosterEntry
                {
                    EnemyId = "enemy_kobold_trapmaster",
                    DisplayName = "Kobold Trapmaster",
                    LoreTagline = "Lures prey into hidden pits with stolen trinkets.",
                    FactionId = "faction_kobold",
                    PrimaryRole = EnemyRole.Ranged,
                    SecondaryRoles = new[] { EnemyRole.Guard },
                    MovementProfileId = "movement_kite_ranged",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_ranged_after_volley",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_kobold_trapmaster",
                    XpReward = 24, CaveBand = 2, MaxHp = 18, ContactDamage = 3
                },
                new RosterEntry
                {
                    EnemyId = "enemy_orc_grunt",
                    DisplayName = "Orc Grunt",
                    LoreTagline = "Muscles like stone, patience like a battering ram.",
                    FactionId = "faction_orc",
                    PrimaryRole = EnemyRole.Tank,
                    SecondaryRoles = new[] { EnemyRole.Chaser },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_tank_recover",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_orc_grunt",
                    XpReward = 32, CaveBand = 2, MaxHp = 38, ContactDamage = 6
                },
                new RosterEntry
                {
                    EnemyId = "enemy_cave_leaper",
                    DisplayName = "Cave Leaper",
                    LoreTagline = "Springs from darkness with bone-snapping force.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_leaper",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_leaper_landing",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_cave_leaper",
                    XpReward = 30, CaveBand = 2, MaxHp = 28, ContactDamage = 5
                },
                new RosterEntry
                {
                    EnemyId = "enemy_duergar_crossbowman",
                    DisplayName = "Duergar Crossbowman",
                    LoreTagline = "Cold-blooded marksman who never wastes a bolt.",
                    FactionId = "faction_duergar",
                    PrimaryRole = EnemyRole.Ranged,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_kite_ranged",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_ranged_after_volley",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_duergar_crossbowman",
                    XpReward = 26, CaveBand = 2, MaxHp = 24, ContactDamage = 4
                },
                new RosterEntry
                {
                    EnemyId = "enemy_burrowing_maggot",
                    DisplayName = "Burrowing Maggot",
                    LoreTagline = "Erupts without warning from fetid earth.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Burrower,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_burrow_ambush",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_burrow_emerge",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_burrowing_maggot",
                    XpReward = 22, CaveBand = 2, MaxHp = 20, ContactDamage = 4
                },
                new RosterEntry
                {
                    EnemyId = "enemy_fungal_spreader",
                    DisplayName = "Fungal Spreader",
                    LoreTagline = "Creeps slowly, seeding doom with every step.",
                    FactionId = "faction_fungal",
                    PrimaryRole = EnemyRole.Tank,
                    SecondaryRoles = new[] { EnemyRole.Caster },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_tank_recover",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_fungal_spreader",
                    XpReward = 34, CaveBand = 2, MaxHp = 42, ContactDamage = 5
                },
                new RosterEntry
                {
                    EnemyId = "enemy_drow_skirmisher",
                    DisplayName = "Drow Skirmisher",
                    LoreTagline = "Strikes fast, retreats into shadow before reprisal.",
                    FactionId = "faction_drow",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new[] { EnemyRole.Ranged },
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_drow_skirmisher",
                    XpReward = 28, CaveBand = 2, MaxHp = 26, ContactDamage = 5
                },

                // ── BAND 3 (CaveBand=3, faixas 26-40) ────────────────────────────────

                new RosterEntry
                {
                    EnemyId = "enemy_orc_berserker",
                    DisplayName = "Orc Berserker",
                    LoreTagline = "Rage incarnate; wounds only fuel its fury.",
                    FactionId = "faction_orc",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_orc_berserker",
                    XpReward = 52, CaveBand = 3, MaxHp = 64, ContactDamage = 9
                },
                new RosterEntry
                {
                    EnemyId = "enemy_duergar_warder",
                    DisplayName = "Duergar Warder",
                    LoreTagline = "Stone-faced sentinel who answers only to his lord.",
                    FactionId = "faction_duergar",
                    PrimaryRole = EnemyRole.Guard,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_guard_stationary",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_duergar_warder",
                    XpReward = 48, CaveBand = 3, MaxHp = 72, ContactDamage = 8
                },
                new RosterEntry
                {
                    EnemyId = "enemy_undead_shambler",
                    DisplayName = "Undead Shambler",
                    LoreTagline = "Animated by necrotic will, it will not stop.",
                    FactionId = "faction_undead",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_undead_shambler",
                    XpReward = 42, CaveBand = 3, MaxHp = 50, ContactDamage = 7
                },
                new RosterEntry
                {
                    EnemyId = "enemy_cultist_zealot",
                    DisplayName = "Cultist Zealot",
                    LoreTagline = "Sacrifice is a gift; its pain belongs to the Void.",
                    FactionId = "faction_cultist",
                    PrimaryRole = EnemyRole.Caster,
                    SecondaryRoles = new[] { EnemyRole.Ranged },
                    MovementProfileId = "movement_caster_keep_away",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_caster_after_cast",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_cultist_zealot",
                    XpReward = 50, CaveBand = 3, MaxHp = 44, ContactDamage = 6
                },
                new RosterEntry
                {
                    EnemyId = "enemy_gnome_tinkerer",
                    DisplayName = "Gnome Tinkerer",
                    LoreTagline = "Deploys mechanical traps with gleeful efficiency.",
                    FactionId = "faction_gnome",
                    PrimaryRole = EnemyRole.Ranged,
                    SecondaryRoles = new[] { EnemyRole.Guard },
                    MovementProfileId = "movement_kite_ranged",
                    SizeProfileId = "size_small",
                    VulnerabilityProfileId = "vuln_ranged_after_volley",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_gnome_tinkerer",
                    XpReward = 44, CaveBand = 3, MaxHp = 36, ContactDamage = 5
                },
                new RosterEntry
                {
                    EnemyId = "enemy_phase_stalker",
                    DisplayName = "Phase Stalker",
                    LoreTagline = "Blinks through solid rock, hunting without a sound.",
                    FactionId = "faction_ninrorin",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_phase_short_blink",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_phase_arrival",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_phase_stalker",
                    XpReward = 56, CaveBand = 3, MaxHp = 52, ContactDamage = 8
                },
                new RosterEntry
                {
                    EnemyId = "enemy_earth_elemental_minor",
                    DisplayName = "Minor Earth Elemental",
                    LoreTagline = "A fragment of living stone, angry at the intrusion.",
                    FactionId = "faction_elemental",
                    PrimaryRole = EnemyRole.Tank,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_tank_recover",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_earth_elemental_minor",
                    XpReward = 58, CaveBand = 3, MaxHp = 80, ContactDamage = 10
                },
                new RosterEntry
                {
                    EnemyId = "enemy_cave_burrower_elite",
                    DisplayName = "Cave Burrower (Elite)",
                    LoreTagline = "Veteran of a thousand ambushes, it never surfaces twice in the same spot.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Burrower,
                    SecondaryRoles = new[] { EnemyRole.Elite },
                    MovementProfileId = "movement_burrow_ambush",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_burrow_emerge",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_cave_burrower_elite",
                    XpReward = 70, CaveBand = 3, MaxHp = 60, ContactDamage = 9,
                    IsElite = true
                },

                // ── BAND 4 (CaveBand=4, faixas 41-55) ────────────────────────────────

                new RosterEntry
                {
                    EnemyId = "enemy_drow_witch",
                    DisplayName = "Drow Witch",
                    LoreTagline = "Weaves webs of shadow and suffering with practiced ease.",
                    FactionId = "faction_drow",
                    PrimaryRole = EnemyRole.Caster,
                    SecondaryRoles = new[] { EnemyRole.Ranged },
                    MovementProfileId = "movement_caster_keep_away",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_caster_after_cast",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_drow_witch",
                    XpReward = 78, CaveBand = 4, MaxHp = 62, ContactDamage = 8
                },
                new RosterEntry
                {
                    EnemyId = "enemy_undead_knight",
                    DisplayName = "Undead Knight",
                    LoreTagline = "Honor bound beyond death; its oath predates the kingdom.",
                    FactionId = "faction_undead",
                    PrimaryRole = EnemyRole.Guard,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_guard_stationary",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_undead_knight",
                    XpReward = 82, CaveBand = 4, MaxHp = 90, ContactDamage = 12
                },
                new RosterEntry
                {
                    EnemyId = "enemy_construct_sentry",
                    DisplayName = "Construct Sentry",
                    LoreTagline = "Iron automaton from a forgotten laboratory, still loyal to dead orders.",
                    FactionId = "faction_construct",
                    PrimaryRole = EnemyRole.Guard,
                    SecondaryRoles = new[] { EnemyRole.Ranged },
                    MovementProfileId = "movement_ground_patrol",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_construct_sentry",
                    XpReward = 84, CaveBand = 4, MaxHp = 100, ContactDamage = 11
                },
                new RosterEntry
                {
                    EnemyId = "enemy_abyssal_hound",
                    DisplayName = "Abyssal Hound",
                    LoreTagline = "Teeth drip with void-fire; it hunts by the scent of courage.",
                    FactionId = "faction_abyssal",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "fire",
                    BestiaryEntryId = "bestiary_abyssal_hound",
                    XpReward = 88, CaveBand = 4, MaxHp = 78, ContactDamage = 13
                },
                new RosterEntry
                {
                    EnemyId = "enemy_corrupted_vine_horror",
                    DisplayName = "Corrupted Vine Horror",
                    LoreTagline = "Ancient roots twisted by Void seepage; it aches with malice.",
                    FactionId = "faction_corrupted",
                    PrimaryRole = EnemyRole.Tank,
                    SecondaryRoles = new[] { EnemyRole.Caster },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_huge",
                    VulnerabilityProfileId = "vuln_corrupted_enrage_pulse",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_corrupted_vine_horror",
                    XpReward = 96, CaveBand = 4, MaxHp = 140, ContactDamage = 14
                },
                new RosterEntry
                {
                    EnemyId = "enemy_ninrorin_phantom",
                    DisplayName = "Ninrorin Phantom",
                    LoreTagline = "A dissolving soul that never accepted its death.",
                    FactionId = "faction_ninrorin",
                    PrimaryRole = EnemyRole.Caster,
                    SecondaryRoles = new[] { EnemyRole.Chaser },
                    MovementProfileId = "movement_phase_short_blink",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_phase_arrival",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_ninrorin_phantom",
                    XpReward = 90, CaveBand = 4, MaxHp = 74, ContactDamage = 10
                },
                new RosterEntry
                {
                    EnemyId = "enemy_gnome_wargolem",
                    DisplayName = "Gnome Wargolem",
                    LoreTagline = "Experimental chassis powered by a captured fire mephit.",
                    FactionId = "faction_gnome",
                    PrimaryRole = EnemyRole.Tank,
                    SecondaryRoles = new[] { EnemyRole.Elite },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_tank_recover",
                    PrimaryDamageTypeId = "fire",
                    BestiaryEntryId = "bestiary_gnome_wargolem",
                    XpReward = 100, CaveBand = 4, MaxHp = 120, ContactDamage = 13,
                    IsElite = true
                },

                // ── BAND 5 (CaveBand=5, faixas 56-70) ────────────────────────────────

                new RosterEntry
                {
                    EnemyId = "enemy_draconic_wyrmling",
                    DisplayName = "Draconic Wyrmling",
                    LoreTagline = "Barely hatched, already lethal; the first breath singes stone.",
                    FactionId = "faction_draconic",
                    PrimaryRole = EnemyRole.Chaser,
                    SecondaryRoles = new[] { EnemyRole.Ranged },
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "fire",
                    BestiaryEntryId = "bestiary_draconic_wyrmling",
                    XpReward = 120, CaveBand = 5, MaxHp = 110, ContactDamage = 16
                },
                new RosterEntry
                {
                    EnemyId = "enemy_abyssal_lurker",
                    DisplayName = "Abyssal Lurker",
                    LoreTagline = "Patience stretched across centuries, hungry once more.",
                    FactionId = "faction_abyssal",
                    PrimaryRole = EnemyRole.Burrower,
                    SecondaryRoles = new[] { EnemyRole.Caster },
                    MovementProfileId = "movement_burrow_ambush",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_burrow_emerge",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_abyssal_lurker",
                    XpReward = 130, CaveBand = 5, MaxHp = 100, ContactDamage = 15
                },
                new RosterEntry
                {
                    EnemyId = "enemy_corrupted_orc_champion",
                    DisplayName = "Corrupted Orc Champion",
                    LoreTagline = "The void gave him strength; it took everything else.",
                    FactionId = "faction_corrupted",
                    PrimaryRole = EnemyRole.Elite,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_huge",
                    VulnerabilityProfileId = "vuln_corrupted_enrage_pulse",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_corrupted_orc_champion",
                    XpReward = 140, CaveBand = 5, MaxHp = 180, ContactDamage = 20,
                    IsElite = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_earth_elemental_greater",
                    DisplayName = "Greater Earth Elemental",
                    LoreTagline = "The mountain breathes, and it is not pleased.",
                    FactionId = "faction_elemental",
                    PrimaryRole = EnemyRole.Tank,
                    SecondaryRoles = new EnemyRole[0],
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_huge",
                    VulnerabilityProfileId = "vuln_tank_recover",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_earth_elemental_greater",
                    XpReward = 135, CaveBand = 5, MaxHp = 200, ContactDamage = 18
                },
                new RosterEntry
                {
                    EnemyId = "enemy_undead_lich_acolyte",
                    DisplayName = "Lich Acolyte",
                    LoreTagline = "An apprentice who chose eternity over mastery.",
                    FactionId = "faction_undead",
                    PrimaryRole = EnemyRole.Caster,
                    SecondaryRoles = new[] { EnemyRole.Elite },
                    MovementProfileId = "movement_caster_keep_away",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_caster_after_cast",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_undead_lich_acolyte",
                    XpReward = 128, CaveBand = 5, MaxHp = 96, ContactDamage = 14,
                    IsElite = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_draconic_guardian",
                    DisplayName = "Draconic Guardian",
                    LoreTagline = "Bound to a hoard long since stolen; it guards the memory.",
                    FactionId = "faction_draconic",
                    PrimaryRole = EnemyRole.Guard,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_guard_stationary",
                    SizeProfileId = "size_huge",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "fire",
                    BestiaryEntryId = "bestiary_draconic_guardian",
                    XpReward = 145, CaveBand = 5, MaxHp = 220, ContactDamage = 22
                },

                // ── MINIBOSSES (IsMiniBoss=true, distributed across bands) ────────────

                new RosterEntry
                {
                    EnemyId = "enemy_goblin_warchief",
                    DisplayName = "Goblin Warchief",
                    LoreTagline = "Every scar is a battle story told at knifepoint.",
                    FactionId = "faction_goblin",
                    PrimaryRole = EnemyRole.MiniBoss,
                    SecondaryRoles = new[] { EnemyRole.Chaser },
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_medium",
                    VulnerabilityProfileId = "vuln_chaser_charge",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_goblin_warchief",
                    XpReward = 90, CaveBand = 2, MaxHp = 80, ContactDamage = 12,
                    IsMiniBoss = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_orc_warlord",
                    DisplayName = "Orc Warlord",
                    LoreTagline = "He carved his throne from the bones of those who doubted him.",
                    FactionId = "faction_orc",
                    PrimaryRole = EnemyRole.MiniBoss,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_large",
                    VulnerabilityProfileId = "vuln_tank_recover",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_orc_warlord",
                    XpReward = 140, CaveBand = 3, MaxHp = 160, ContactDamage = 18,
                    IsMiniBoss = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_abyssal_gatekeeper",
                    DisplayName = "Abyssal Gatekeeper",
                    LoreTagline = "It does not know what it guards. It knows only to deny passage.",
                    FactionId = "faction_abyssal",
                    PrimaryRole = EnemyRole.MiniBoss,
                    SecondaryRoles = new[] { EnemyRole.Guard },
                    MovementProfileId = "movement_guard_stationary",
                    SizeProfileId = "size_huge",
                    VulnerabilityProfileId = "vuln_guard_shield_drop",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_abyssal_gatekeeper",
                    XpReward = 200, CaveBand = 4, MaxHp = 250, ContactDamage = 24,
                    IsMiniBoss = true
                },

                // ── BOSSES (IsBoss=true) ───────────────────────────────────────────────

                new RosterEntry
                {
                    EnemyId = "enemy_cave_mite_queen",
                    DisplayName = "Cave Mite Queen",
                    LoreTagline = "Mother of swarms; the hive mind given terrible, singular will.",
                    FactionId = "faction_beast",
                    PrimaryRole = EnemyRole.Boss,
                    SecondaryRoles = new[] { EnemyRole.Swarm },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_boss",
                    VulnerabilityProfileId = "vuln_corrupted_enrage_pulse",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_cave_mite_queen",
                    XpReward = 300, CaveBand = 1, MaxHp = 200, ContactDamage = 16,
                    IsBoss = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_fungal_patriarch",
                    DisplayName = "Fungal Patriarch",
                    LoreTagline = "Ancient mycelium given consciousness; it remembers when the caves were young.",
                    FactionId = "faction_fungal",
                    PrimaryRole = EnemyRole.Boss,
                    SecondaryRoles = new[] { EnemyRole.Caster },
                    MovementProfileId = "movement_tank_slow_push",
                    SizeProfileId = "size_boss",
                    VulnerabilityProfileId = "vuln_corrupted_enrage_pulse",
                    PrimaryDamageTypeId = "poison",
                    BestiaryEntryId = "bestiary_fungal_patriarch",
                    XpReward = 400, CaveBand = 2, MaxHp = 300, ContactDamage = 20,
                    IsBoss = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_duergar_artificer_lord",
                    DisplayName = "Duergar Artificer Lord",
                    LoreTagline = "Forged himself a heart of adamantite. It does not beat; it grinds.",
                    FactionId = "faction_duergar",
                    PrimaryRole = EnemyRole.Boss,
                    SecondaryRoles = new[] { EnemyRole.Caster, EnemyRole.Tank },
                    MovementProfileId = "movement_caster_keep_away",
                    SizeProfileId = "size_boss",
                    VulnerabilityProfileId = "vuln_caster_after_cast",
                    PrimaryDamageTypeId = "physical",
                    BestiaryEntryId = "bestiary_duergar_artificer_lord",
                    XpReward = 500, CaveBand = 3, MaxHp = 420, ContactDamage = 25,
                    IsBoss = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_void_herald",
                    DisplayName = "Void Herald",
                    LoreTagline = "Vanguard of an uncaring cosmos; its presence unmakes memory.",
                    FactionId = "faction_abyssal",
                    PrimaryRole = EnemyRole.Boss,
                    SecondaryRoles = new[] { EnemyRole.Caster, EnemyRole.Elite },
                    MovementProfileId = "movement_phase_short_blink",
                    SizeProfileId = "size_boss",
                    VulnerabilityProfileId = "vuln_phase_arrival",
                    PrimaryDamageTypeId = "arcane",
                    BestiaryEntryId = "bestiary_void_herald",
                    XpReward = 650, CaveBand = 4, MaxHp = 550, ContactDamage = 32,
                    IsBoss = true
                },
                new RosterEntry
                {
                    EnemyId = "enemy_draconic_elder",
                    DisplayName = "Draconic Elder",
                    LoreTagline = "The last of its kind, which makes it the most dangerous thing alive.",
                    FactionId = "faction_draconic",
                    PrimaryRole = EnemyRole.Boss,
                    SecondaryRoles = new[] { EnemyRole.Tank },
                    MovementProfileId = "movement_ground_chase",
                    SizeProfileId = "size_boss",
                    VulnerabilityProfileId = "vuln_corrupted_enrage_pulse",
                    PrimaryDamageTypeId = "fire",
                    BestiaryEntryId = "bestiary_draconic_elder",
                    XpReward = 800, CaveBand = 5, MaxHp = 700, ContactDamage = 40,
                    IsBoss = true
                },
            };
        }

        private static List<RosterEntry> BuildCanonicalRoster()
        {
            return CreateEnemySpawnEcologyData.BuildProfileDefinitions()
                .Select(BuildCanonicalEntry)
                .ToList();
        }

        private static RosterEntry BuildCanonicalEntry(CreateEnemySpawnEcologyData.ProfileDefinition profile)
        {
            var caveBand = profile.MinLevel switch
            {
                <= 10 => 1,
                <= 25 => 2,
                <= 40 => 3,
                <= 55 => 4,
                _ => 5
            };

            var role = GuessPrimaryRole(profile.EnemyId, profile.SizeClass);
            var displayName = ToTitle(profile.EnemyId.Replace("enemy_", string.Empty));
            return new RosterEntry
            {
                EnemyId = profile.EnemyId,
                DisplayName = displayName,
                LoreTagline = $"A criatura {displayName} foi catalogada nas rotas profundas de Vaalara.",
                FactionId = $"faction_{MapFaction(profile.FactionId)}",
                PrimaryRole = role,
                SecondaryRoles = BuildSecondaryRoles(role, profile.SizeClass),
                MovementProfileId = GuessMovementProfile(profile.EnemyId, role),
                SizeProfileId = $"size_{profile.SizeClass.ToString().ToLowerInvariant()}",
                ActionSetId = $"actionset_{profile.EnemyId}",
                VulnerabilityProfileId = GuessVulnerability(profile.EnemyId, role),
                PrimaryDamageTypeId = GuessDamageType(profile.EnemyId),
                BestiaryEntryId = $"bestiary_{profile.EnemyId.Replace("enemy_", string.Empty)}",
                XpReward = profile.MinLevel * 4 + profile.MaxCountPerRoom * 3,
                CaveBand = caveBand,
                MaxHp = 8 + profile.MinLevel + (int)profile.SizeClass * 8,
                ContactDamage = 2 + caveBand + (profile.SizeClass >= EnemySizeClass.Large ? 3 : 0),
                IsElite = profile.CanSpawnAsElite && profile.EnemyId.Contains("warden"),
                IsMiniBoss = false,
                IsBoss = profile.SizeClass == EnemySizeClass.Boss
            };
        }

        private static EnemyRole GuessPrimaryRole(string enemyId, EnemySizeClass size)
        {
            if (enemyId.Contains("archer") || enemyId.Contains("spitter") || enemyId.Contains("bat") || enemyId.Contains("moth")) return EnemyRole.Ranged;
            if (enemyId.Contains("acolyte") || enemyId.Contains("ashcaller") || enemyId.Contains("adept") || enemyId.Contains("tinker")) return EnemyRole.Caster;
            if (enemyId.Contains("sentinel") || enemyId.Contains("guard") || enemyId.Contains("knight") || enemyId.Contains("warden")) return EnemyRole.Guard;
            if (enemyId.Contains("rootsnare")) return EnemyRole.Burrower;
            if (enemyId.Contains("mite") || enemyId.Contains("tick") || enemyId.Contains("shard")) return EnemyRole.Swarm;
            if (size >= EnemySizeClass.Large) return EnemyRole.Tank;
            return EnemyRole.Chaser;
        }

        private static EnemyRole[] BuildSecondaryRoles(EnemyRole primary, EnemySizeClass size)
        {
            if (size >= EnemySizeClass.Large && primary != EnemyRole.Tank)
            {
                return new[] { EnemyRole.Tank };
            }

            return new EnemyRole[0];
        }

        private static string GuessMovementProfile(string enemyId, EnemyRole role)
        {
            if (enemyId.Contains("leaper")) return "movement_leaper";
            if (enemyId.Contains("shade") || enemyId.Contains("mirror")) return "movement_phase_short_blink";
            return role switch
            {
                EnemyRole.Swarm => "movement_swarm_erratic",
                EnemyRole.Ranged => "movement_kite_ranged",
                EnemyRole.Caster => "movement_caster_keep_away",
                EnemyRole.Guard => "movement_guard_stationary",
                EnemyRole.Burrower => "movement_burrow_ambush",
                EnemyRole.Tank => "movement_tank_slow_push",
                _ => "movement_ground_chase"
            };
        }

        private static string GuessVulnerability(string enemyId, EnemyRole role)
        {
            if (enemyId.Contains("leaper")) return "vuln_leaper_landing";
            if (enemyId.Contains("shade") || enemyId.Contains("mirror")) return "vuln_phase_arrival";
            return role switch
            {
                EnemyRole.Swarm => "vuln_swarm_after_bite",
                EnemyRole.Ranged => "vuln_ranged_after_volley",
                EnemyRole.Caster => "vuln_caster_after_cast",
                EnemyRole.Guard => "vuln_guard_shield_drop",
                EnemyRole.Burrower => "vuln_burrow_emerge",
                EnemyRole.Tank => "vuln_tank_recover",
                _ => "vuln_chaser_charge"
            };
        }

        private static string GuessDamageType(string enemyId)
        {
            if (enemyId.Contains("frost") || enemyId.Contains("ice") || enemyId.Contains("cold")) return "ice";
            if (enemyId.Contains("ember") || enemyId.Contains("ash") || enemyId.Contains("lava") || enemyId.Contains("furnace") || enemyId.Contains("scorched")) return "fire";
            if (enemyId.Contains("spore") || enemyId.Contains("moss") || enemyId.Contains("root") || enemyId.Contains("blackroot")) return "toxic";
            if (enemyId.Contains("rune") || enemyId.Contains("mirror") || enemyId.Contains("shade")) return "arcane";
            return "physical";
        }

        private static string MapFaction(string factionId)
        {
            return factionId switch
            {
                "undead_weak" or "undead_stronger" or "oathless_undead" => "undead",
                "orc_nyx" or "orc_kaand" => "orc",
                "ice_cult" => "cultist",
                "elemental_fire" => "elemental",
                "furnace_construct" or "stronger_construct" => "construct",
                "gnome_ruins" => "gnome",
                _ => factionId
            };
        }

        private static string ToTitle(string value)
        {
            return string.Join(" ", value.Split('_').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }
    }
}
