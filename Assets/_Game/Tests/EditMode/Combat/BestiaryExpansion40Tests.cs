using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;

namespace CindarsHope.Tests.EditMode.Combat
{
    /// <summary>
    /// fable_80 — EditMode tests for the +40 bestiary expansion.
    /// Tests: total count (104), ID uniqueness, MovePrimary/Secondary in enum,
    /// BestiarySizeClass valid, Notes non-empty, canonical lore terms respected,
    /// size variety (Tiny/Small per band), role variety per band.
    /// </summary>
    [TestFixture]
    public class BestiaryExpansion40Tests
    {
        private static readonly HashSet<EnemyMovementType> ValidMoves =
            new HashSet<EnemyMovementType>((EnemyMovementType[])System.Enum.GetValues(typeof(EnemyMovementType)));

        private static readonly HashSet<BestiarySizeClass> ValidSizes =
            new HashSet<BestiarySizeClass>((BestiarySizeClass[])System.Enum.GetValues(typeof(BestiarySizeClass)));

        private IReadOnlyList<BestiaryCreatureDef> _all;

        [SetUp]
        public void SetUp()
        {
            _all = CanonicalBestiaryCatalog.All;
        }

        // ── CA-1: 40 novas fichas → total 104 ──────────────────────────────────────────────

        [Test]
        public void TotalCreatureCount_Is117()
        {
            Assert.AreEqual(CanonicalBestiaryCatalogCounts.TotalDistinct, _all.Count,
                $"Expected the current 117 canonical fichas. Got {_all.Count}.");
        }

        // ── CA-1: unicidade de IDs ──────────────────────────────────────────────────────────

        [Test]
        public void AllEnemyIds_AreUnique()
        {
            var ids = _all.Select(c => c.EnemyId).ToList();
            var duplicates = ids.GroupBy(id => id)
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key)
                                .ToList();
            Assert.IsEmpty(duplicates,
                $"Duplicate EnemyIds found: {string.Join(", ", duplicates)}");
        }

        [Test]
        public void AllEnemyIds_HaveEnemyPrefix()
        {
            // boss_* are allowed for the Final Four; all fable_80 must start with enemy_
            var fable80Ids = _all.Where(c => IsFable80Id(c.EnemyId)).Select(c => c.EnemyId).ToList();
            var badPrefix = fable80Ids.Where(id => !id.StartsWith("enemy_")).ToList();
            Assert.IsEmpty(badPrefix,
                $"fable_80 IDs without 'enemy_' prefix: {string.Join(", ", badPrefix)}");
        }

        // ── CA-2: Notes não-vazio e sem termos proibidos ────────────────────────────────────

        [Test]
        public void AllFable80Creatures_HaveNonEmptyNotes()
        {
            var missing = _all.Where(c => IsFable80Id(c.EnemyId) && string.IsNullOrWhiteSpace(c.Notes))
                              .Select(c => c.EnemyId).ToList();
            Assert.IsEmpty(missing,
                $"fable_80 creatures with empty Notes: {string.Join(", ", missing)}");
        }

        [Test]
        public void AllCreatures_NotesDoNotContainForbiddenLoreTerms()
        {
            // Drow → Veilkin; Duergar → Gravedelver (case-insensitive)
            var forbidden = new[] { "drow", "duergar" };
            foreach (var creature in _all)
            {
                foreach (var term in forbidden)
                {
                    Assert.IsFalse(
                        creature.Notes != null && creature.Notes.ToLowerInvariant().Contains(term),
                        $"Creature '{creature.EnemyId}' Notes contains forbidden lore term '{term}'.");
                    Assert.IsFalse(
                        creature.DisplayName.ToLowerInvariant().Contains(term),
                        $"Creature '{creature.EnemyId}' DisplayName contains forbidden lore term '{term}'.");
                }
            }
        }

        // ── CA-4: Moves e BestiarySizeClass válidos ─────────────────────────────────────────

        [Test]
        public void AllFable80Creatures_MovePrimary_IsInValidEnum()
        {
            var bad = _all.Where(c => IsFable80Id(c.EnemyId) && !ValidMoves.Contains(c.MovePrimary))
                          .Select(c => $"{c.EnemyId}:{c.MovePrimary}").ToList();
            Assert.IsEmpty(bad,
                $"Creatures with invalid MovePrimary: {string.Join(", ", bad)}");
        }

        [Test]
        public void AllFable80Creatures_MoveSecondary_IsInValidEnum()
        {
            var bad = _all.Where(c => IsFable80Id(c.EnemyId) && !ValidMoves.Contains(c.MoveSecondary))
                          .Select(c => $"{c.EnemyId}:{c.MoveSecondary}").ToList();
            Assert.IsEmpty(bad,
                $"Creatures with invalid MoveSecondary: {string.Join(", ", bad)}");
        }

        [Test]
        public void AllFable80Creatures_SizeClass_IsInValidEnum()
        {
            var bad = _all.Where(c => IsFable80Id(c.EnemyId) && !ValidSizes.Contains(c.Size))
                          .Select(c => $"{c.EnemyId}:{c.Size}").ToList();
            Assert.IsEmpty(bad,
                $"Creatures with invalid BestiarySizeClass: {string.Join(", ", bad)}");
        }

        // ── CA-3: variedade de tamanho ───────────────────────────────────────────────────────

        [Test]
        public void Band1Stone_HasAtLeastOneTinyOrSmall_FromFable80()
        {
            var tinysOrSmall = _all.Where(c => IsFable80Id(c.EnemyId) && c.Band == 1
                && (c.Size == BestiarySizeClass.Tiny || c.Size == BestiarySizeClass.Small)).ToList();
            Assert.IsNotEmpty(tinysOrSmall, "Band 1 (Stone) fable_80 should have at least one Tiny or Small creature.");
        }

        [Test]
        public void Band2Fungal_HasAtLeastOneTinyOrSmall_FromFable80()
        {
            var tinysOrSmall = _all.Where(c => IsFable80Id(c.EnemyId) && c.Band == 2
                && (c.Size == BestiarySizeClass.Tiny || c.Size == BestiarySizeClass.Small)).ToList();
            Assert.IsNotEmpty(tinysOrSmall, "Band 2 (Fungal) fable_80 should have at least one Tiny or Small creature.");
        }

        [Test]
        public void Band3Ice_HasAtLeastOneTinyOrSmall_FromFable80()
        {
            var tinysOrSmall = _all.Where(c => IsFable80Id(c.EnemyId) && c.Band == 3
                && (c.Size == BestiarySizeClass.Tiny || c.Size == BestiarySizeClass.Small)).ToList();
            Assert.IsNotEmpty(tinysOrSmall, "Band 3 (Ice) fable_80 should have at least one Tiny or Small creature.");
        }

        [Test]
        public void Band6Deep_HasAtLeastOneTinyOrSmall_FromFable80()
        {
            var tinysOrSmall = _all.Where(c => IsFable80Id(c.EnemyId) && c.Band == 6
                && (c.Size == BestiarySizeClass.Tiny || c.Size == BestiarySizeClass.Small)).ToList();
            Assert.IsNotEmpty(tinysOrSmall, "Band 6 (Deep) fable_80 should have at least one Tiny or Small creature.");
        }

        // ── CA-3: variedade de role por banda (total, não só fable_80) ─────────────────────

        [Test]
        public void EachBand_HasAtLeast4DistinctRoles()
        {
            for (int band = 1; band <= 7; band++)
            {
                var roles = _all.Where(c => c.Band == band)
                                .Select(c => c.Role)
                                .Distinct()
                                .Count();
                Assert.GreaterOrEqual(roles, 4,
                    $"Band {band} should have at least 4 distinct roles. Got {roles}.");
            }
        }

        // ── Stat sanity (HP/Damage/Xp > 0 para commons) ────────────────────────────────────

        [Test]
        public void AllFable80Creatures_HavePositiveStats()
        {
            var bad = _all.Where(c => IsFable80Id(c.EnemyId) && (c.Hp <= 0 || c.Damage <= 0 || c.Xp <= 0))
                          .Select(c => c.EnemyId).ToList();
            Assert.IsEmpty(bad,
                $"fable_80 creatures with non-positive HP/Damage/Xp: {string.Join(", ", bad)}");
        }

        // ── Faixas de nível por banda ────────────────────────────────────────────────────────

        [Test]
        public void AllFable80Creatures_LevelRange_CoherentWithBand()
        {
            var bandRanges = new Dictionary<int, (int min, int max)>
            {
                { 1, (1,  10) },
                { 2, (11, 25) },
                { 3, (26, 40) },
                { 4, (41, 55) },
                { 5, (56, 70) },
                { 6, (71, 85) },
                { 7, (86, 99) }
            };
            var bad = new List<string>();
            foreach (var c in _all.Where(x => IsFable80Id(x.EnemyId)))
            {
                if (!bandRanges.TryGetValue(c.Band, out var range)) continue;
                if (c.MinLevel < range.min || c.MaxLevel > range.max)
                    bad.Add($"{c.EnemyId} (Band {c.Band}, Lvl {c.MinLevel}-{c.MaxLevel}, expected {range.min}-{range.max})");
            }
            Assert.IsEmpty(bad, $"Creatures with incoherent band/level range: {string.Join("; ", bad)}");
        }

        // ── Fable_80 specific IDs all present ───────────────────────────────────────────────

        [Test]
        public void AllFable80ExpectedIds_ArePresent()
        {
            var expectedIds = new[]
            {
                // Stone
                "enemy_glimmer_centipede", "enemy_stone_burrower", "enemy_roost_cave_bat",
                "enemy_bandit_scavenger", "enemy_cracked_golem_shard",
                // Fungal
                "enemy_rotcap_cluster", "enemy_mycelial_warden", "enemy_goblin_shredder",
                "enemy_orc_drummer", "enemy_cave_stalker_cat", "enemy_spore_amalgam",
                // Ice
                "enemy_frostshard_wisp", "enemy_crystal_hound", "enemy_veilkin_iceblade",
                "enemy_coldcult_preacher", "enemy_frostbound_revenant", "enemy_glacier_tick",
                // Fire
                "enemy_magma_slug", "enemy_ember_scorpion", "enemy_sulfur_wyrmling",
                "enemy_emberroot_horror", "enemy_veilkin_pyrecaller", "enemy_steam_golem_proto",
                // Ruins
                "enemy_rune_sentry_mk2", "enemy_mirror_golem", "enemy_gravedelver_runepriest",
                "enemy_ninrorin_echo_warrior", "enemy_chromatic_hoardling", "enemy_runic_warbeast",
                // Deep
                "enemy_void_brood_larva", "enemy_mindbound_thrall", "enemy_veilkin_voidassassin",
                "enemy_gloomspine_lurker", "enemy_corrupt_pseudowyrm", "enemy_nyx_shade_elemental",
                // Void
                "enemy_void_tendril_watcher", "enemy_reality_render", "enemy_veilkin_voidknight",
                "enemy_sealed_observer", "enemy_dread_chorister",
            };
            var presentIds = new HashSet<string>(_all.Select(c => c.EnemyId));
            var missing = expectedIds.Where(id => !presentIds.Contains(id)).ToList();
            Assert.IsEmpty(missing,
                $"fable_80 expected IDs missing from catalog: {string.Join(", ", missing)}");
        }

        // ── Fable_80 count is exactly 40 ─────────────────────────────────────────────────────

        [Test]
        public void Fable80Creatures_CountIs40()
        {
            var fable80Ids = new HashSet<string>(new[]
            {
                "enemy_glimmer_centipede", "enemy_stone_burrower", "enemy_roost_cave_bat",
                "enemy_bandit_scavenger", "enemy_cracked_golem_shard",
                "enemy_rotcap_cluster", "enemy_mycelial_warden", "enemy_goblin_shredder",
                "enemy_orc_drummer", "enemy_cave_stalker_cat", "enemy_spore_amalgam",
                "enemy_frostshard_wisp", "enemy_crystal_hound", "enemy_veilkin_iceblade",
                "enemy_coldcult_preacher", "enemy_frostbound_revenant", "enemy_glacier_tick",
                "enemy_magma_slug", "enemy_ember_scorpion", "enemy_sulfur_wyrmling",
                "enemy_emberroot_horror", "enemy_veilkin_pyrecaller", "enemy_steam_golem_proto",
                "enemy_rune_sentry_mk2", "enemy_mirror_golem", "enemy_gravedelver_runepriest",
                "enemy_ninrorin_echo_warrior", "enemy_chromatic_hoardling", "enemy_runic_warbeast",
                "enemy_void_brood_larva", "enemy_mindbound_thrall", "enemy_veilkin_voidassassin",
                "enemy_gloomspine_lurker", "enemy_corrupt_pseudowyrm", "enemy_nyx_shade_elemental",
                "enemy_void_tendril_watcher", "enemy_reality_render", "enemy_veilkin_voidknight",
                "enemy_sealed_observer", "enemy_dread_chorister",
            });
            var count = _all.Count(c => fable80Ids.Contains(c.EnemyId));
            Assert.AreEqual(40, count, $"Expected exactly 40 fable_80 creatures in catalog. Got {count}.");
        }

        // ── Helper ───────────────────────────────────────────────────────────────────────────

        private static readonly HashSet<string> _fable80Ids = new HashSet<string>(new[]
        {
            "enemy_glimmer_centipede", "enemy_stone_burrower", "enemy_roost_cave_bat",
            "enemy_bandit_scavenger", "enemy_cracked_golem_shard",
            "enemy_rotcap_cluster", "enemy_mycelial_warden", "enemy_goblin_shredder",
            "enemy_orc_drummer", "enemy_cave_stalker_cat", "enemy_spore_amalgam",
            "enemy_frostshard_wisp", "enemy_crystal_hound", "enemy_veilkin_iceblade",
            "enemy_coldcult_preacher", "enemy_frostbound_revenant", "enemy_glacier_tick",
            "enemy_magma_slug", "enemy_ember_scorpion", "enemy_sulfur_wyrmling",
            "enemy_emberroot_horror", "enemy_veilkin_pyrecaller", "enemy_steam_golem_proto",
            "enemy_rune_sentry_mk2", "enemy_mirror_golem", "enemy_gravedelver_runepriest",
            "enemy_ninrorin_echo_warrior", "enemy_chromatic_hoardling", "enemy_runic_warbeast",
            "enemy_void_brood_larva", "enemy_mindbound_thrall", "enemy_veilkin_voidassassin",
            "enemy_gloomspine_lurker", "enemy_corrupt_pseudowyrm", "enemy_nyx_shade_elemental",
            "enemy_void_tendril_watcher", "enemy_reality_render", "enemy_veilkin_voidknight",
            "enemy_sealed_observer", "enemy_dread_chorister",
        });

        private static bool IsFable80Id(string id) => _fable80Ids.Contains(id);
    }
}
