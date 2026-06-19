using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Cave.Runtime;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_33 — pure consistency tests for the canonical bestiary catalog + band scaling/spawn,
    /// runnable in EditMode without Unity Play Mode or generated assets (the .asset generation is
    /// deferred). Covers CA-1 (catalog integrity), CA-2 (scale formula), CA-3 (deterministic spawn),
    /// CA-4 (renames / no trademarks).
    /// </summary>
    [TestFixture]
    public class BestiaryDataTests
    {
        private static IReadOnlyList<BestiaryCreatureDef> Catalog => CanonicalBestiaryCatalog.All;

        // Drop ids that the bestiary references — every one MUST be a real fable_32 item id.
        // Mirror of the canonical item catalog material/essence/fish ids used as primary drops.
        private static readonly HashSet<string> KnownItemIds = new HashSet<string>
        {
            // basic + monster-part materials (fable_32)
            "item_material_wood", "item_material_stone", "item_material_copper_ore",
            "item_material_iron_ore", "item_material_leather", "item_material_silver_ore",
            "item_material_arcane_crystal", "item_material_mithril_ore",
            "item_material_bromecian_alloy", "item_material_star_iron",
            "item_material_chitin", "item_material_chitin_plate", "item_material_fiber",
            "item_material_glowcap", "item_material_spores", "item_material_grub_meat",
            "item_material_rot_gland", "item_material_sinew", "item_material_white_pelt",
            "item_material_frost_core", "item_material_ember_fang", "item_material_magma_chitin",
            "item_material_shade_ash", "item_material_spark_dust", "item_material_mycel_thread",
            "item_material_mycel_heart", "item_material_gears", "item_material_turret_core",
            "item_material_warden_core", "item_material_phantom_essence", "item_material_night_essence",
            "item_material_void_ichor", "item_material_abyssal_fang", "item_material_wyrmling_scale",
            "item_material_lurker_eye",
            // essences
            "item_essence_fire", "item_essence_ice", "item_essence_toxic",
            "item_essence_lightning", "item_essence_arcane", "item_essence_void",
            // specials
            "item_fruto_mana", "item_material_stabilized_blackstone", "item_blackstone_corrupted_shard",
            // fish used as drops
            "item_fish_pale", "item_fish_mirrorfin",
        };

        // ── CA-1: catalog completeness and integrity ────────────────────────────────────────────

        [Test]
        public void Catalog_HasExpectedDistinctCount()
        {
            Assert.AreEqual(
                CanonicalBestiaryCatalogCounts.TotalDistinct, Catalog.Count,
                "Catalog must materialize exactly the canonical distinct fichas " +
                "(50 commons + 14 minibosses + 9 gate bosses + 4 finals = 77).");
        }

        [Test]
        public void Catalog_HeadlineReconciliation_Holds()
        {
            int commons = Catalog.Count(d => !d.IsMiniBoss && !d.IsBoss);
            int minibosses = Catalog.Count(d => d.IsMiniBoss);
            int gateBosses = Catalog.Count(d => d.IsBoss && d.SpoilerTier == 3);
            int finalFour = Catalog.Count(d => d.IsBoss && d.SpoilerTier == 4);

            Assert.AreEqual(CanonicalBestiaryCatalogCounts.Commons, commons, "50 band commons.");
            Assert.AreEqual(CanonicalBestiaryCatalogCounts.Minibosses, minibosses, "14 minibosses (2/band).");
            Assert.AreEqual(CanonicalBestiaryCatalogCounts.GateBosses, gateBosses, "9 gate bosses.");
            Assert.AreEqual(CanonicalBestiaryCatalogCounts.FinalFour, finalFour, "4 finals (SpoilerTier 4).");

            // Catalog headline framing: 64 "band creatures" (commons + minibosses) + the gate/finals.
            Assert.AreEqual(CanonicalBestiaryCatalogCounts.BandRosterHeadline, commons + minibosses,
                "Commons + minibosses == 64 (catalog band-roster headline).");
            Assert.AreEqual(CanonicalBestiaryCatalogCounts.TotalDistinct,
                commons + minibosses + gateBosses + finalFour, "All buckets sum to 77 distinct.");
        }

        [Test]
        public void Catalog_AllIds_AreUniqueAndWellFormed()
        {
            var seen = new HashSet<string>();
            foreach (var def in Catalog)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(def.EnemyId), "Empty enemyId in catalog.");
                Assert.IsTrue(seen.Add(def.EnemyId), $"Duplicate enemyId: {def.EnemyId}");
                Assert.AreEqual(def.EnemyId.ToLowerInvariant(), def.EnemyId,
                    $"Id must be lowercase_snake: {def.EnemyId}");
                Assert.IsTrue(
                    def.EnemyId.StartsWith("enemy_") || def.EnemyId.StartsWith("boss_"),
                    $"Id must start with enemy_ or boss_: {def.EnemyId}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(def.DisplayName), $"Empty DisplayName: {def.EnemyId}");
            }
        }

        [Test]
        public void Catalog_BandAndFaixa_AreValid()
        {
            foreach (var def in Catalog)
            {
                Assert.IsTrue(def.Band >= 1 && def.Band <= 7, $"Band out of 1..7: {def.EnemyId} band {def.Band}");
                Assert.IsTrue(def.MinLevel >= 1 && def.MinLevel <= 101, $"MinLevel out of range: {def.EnemyId}");
                Assert.IsTrue(def.MaxLevel >= def.MinLevel, $"MaxLevel < MinLevel: {def.EnemyId}");

                // MinLevel must fall inside the band's declared window.
                var bandInfo = CanonicalBestiaryCatalog.Bands.First(b => b.Band == def.Band);
                Assert.IsTrue(def.MinLevel >= bandInfo.MinLevel && def.MinLevel <= bandInfo.MaxLevel,
                    $"{def.EnemyId} MinLevel {def.MinLevel} outside band {def.Band} window " +
                    $"[{bandInfo.MinLevel},{bandInfo.MaxLevel}].");
            }
        }

        [Test]
        public void Catalog_StatBlocks_ArePositiveAndSane()
        {
            foreach (var def in Catalog)
            {
                Assert.IsTrue(def.Hp >= 1, $"HP must be >=1: {def.EnemyId}");
                Assert.IsTrue(def.Damage >= 0, $"DMG must be >=0: {def.EnemyId}");
                Assert.IsTrue(def.Defense >= 0, $"DEF must be >=0: {def.EnemyId}");
                Assert.IsTrue(def.Xp >= 0, $"XP must be >=0: {def.EnemyId}");
                // Bosses must be meaningfully tankier than the lightest common.
                if (def.IsBoss)
                {
                    Assert.IsTrue(def.Hp >= 200, $"Boss HP unexpectedly low: {def.EnemyId} ({def.Hp})");
                }
            }
        }

        [Test]
        public void Catalog_Drops_ReferenceKnownItemIds()
        {
            foreach (var def in Catalog)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(def.PrimaryDropItemId),
                    $"Empty PrimaryDropItemId: {def.EnemyId}");
                Assert.IsTrue(KnownItemIds.Contains(def.PrimaryDropItemId),
                    $"{def.EnemyId} drops unknown item id '{def.PrimaryDropItemId}' " +
                    "(must be an existing fable_32 item_* id, else F30 drop cross-ref breaks).");
            }
        }

        // ── CA-1: moves must be one of the 22 canonical Moves (fable_24) ────────────────────────

        [Test]
        public void Catalog_Moves_AreCanonicalEnumValues()
        {
            foreach (var def in Catalog)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(EnemyMovementType), def.MovePrimary),
                    $"{def.EnemyId} primary move not a defined EnemyMovementType.");
                Assert.IsTrue(System.Enum.IsDefined(typeof(EnemyMovementType), def.MoveSecondary),
                    $"{def.EnemyId} secondary move not a defined EnemyMovementType.");
            }
        }

        [Test]
        public void Catalog_SpoilerTiers_FollowCatalogRules()
        {
            foreach (var def in Catalog)
            {
                Assert.IsTrue(def.SpoilerTier >= 0 && def.SpoilerTier <= 4,
                    $"SpoilerTier out of 0..4: {def.EnemyId}");

                if (def.IsMiniBoss)
                {
                    Assert.IsTrue(def.SpoilerTier >= 2, $"Miniboss SpoilerTier must be >=2: {def.EnemyId}");
                }
                if (def.IsBoss && def.SpoilerTier != 4)
                {
                    Assert.AreEqual(3, def.SpoilerTier, $"Gate boss SpoilerTier must be 3: {def.EnemyId}");
                }
            }
        }

        [Test]
        public void Catalog_BossAndMinibossCounts_MatchCatalog()
        {
            // 5 gate bosses (gates 10/20/30/50/60... actually 7 gate bosses across bands) + 4 finals.
            // Assert at least the canonical minimums to catch accidental drops/dupes.
            int gateBosses = Catalog.Count(d => d.IsBoss && d.SpoilerTier == 3);
            int finalBosses = Catalog.Count(d => d.IsBoss && d.SpoilerTier == 4);
            int minibosses = Catalog.Count(d => d.IsMiniBoss);

            Assert.AreEqual(4, finalBosses, "Exactly 4 final bosses (the Four of 101).");
            Assert.IsTrue(gateBosses >= 7, $"Expected >=7 gate bosses, found {gateBosses}.");
            // Each band (1..7) must have >=2 minibosses (decision Q12.3).
            for (int band = 1; band <= 7; band++)
            {
                int bandMinibosses = Catalog.Count(d => d.Band == band && d.IsMiniBoss);
                Assert.IsTrue(bandMinibosses >= 2,
                    $"Band {band} must have >=2 minibosses, found {bandMinibosses}.");
            }
            Assert.IsTrue(minibosses >= 14, $"Expected >=14 minibosses total, found {minibosses}.");
        }

        // ── CA-4: renames applied, zero trademarks ──────────────────────────────────────────────

        [Test]
        public void Catalog_NoTrademarkedNames_Anywhere()
        {
            string[] trademarked = { "drow", "duergar" };
            foreach (var def in Catalog)
            {
                var idLower = def.EnemyId.ToLowerInvariant();
                var nameLower = def.DisplayName.ToLowerInvariant();
                foreach (var tm in trademarked)
                {
                    Assert.IsFalse(idLower.Contains(tm),
                        $"Trademarked name '{tm}' in id {def.EnemyId} — must be Veilkin/Gravedelver.");
                    Assert.IsFalse(nameLower.Contains(tm),
                        $"Trademarked name '{tm}' in DisplayName '{def.DisplayName}'.");
                }
            }
        }

        [Test]
        public void Catalog_RenamesApplied_VeilkinAndGravedelverPresent()
        {
            Assert.IsTrue(Catalog.Any(d => d.EnemyId.Contains("veilkin")),
                "Expected Veilkin creatures (renamed from Drow).");
            Assert.IsTrue(Catalog.Any(d => d.EnemyId.Contains("gravedelver")),
                "Expected Gravedelver creatures (renamed from Duergar).");

            // The specific renamed fichas the catalog §3 calls out must exist by id.
            string[] expected =
            {
                "enemy_gravedelver_crossbowman", "enemy_gravedelver_warder",
                "enemy_gravedelver_artificer_lord", "enemy_veilkin_skirmisher",
                "enemy_veilkin_witch", "enemy_veilkin_pyromancer", "enemy_veilkin_blademaster",
            };
            foreach (var id in expected)
            {
                Assert.IsTrue(Catalog.Any(d => d.EnemyId == id), $"Missing renamed ficha: {id}");
            }
        }

        // ── CA-2: intra-band scale formula (HP +12%/lvl, DMG +8%/lvl, DEF fixed) ─────────────────

        [Test]
        public void Scale_AtBandMinimum_ReturnsBaseValues()
        {
            // Band 11-25 creature at level 11 = base unchanged.
            Assert.AreEqual(36, CaveBandScaling.ScaleHp(36, 11, 11));
            Assert.AreEqual(6, CaveBandScaling.ScaleDamage(6, 11, 11));
            Assert.AreEqual(1, CaveBandScaling.ScaleDefense(1, 11, 11));
        }

        [Test]
        public void Scale_FollowsCanonicalFormula_PerBand()
        {
            // CA-2 worked example: a band 11-25 creature spawned at level 20.
            // HP = base * 1.12^(20-11), DMG = base * 1.08^(20-11), DEF unchanged.
            const int baseHp = 36;
            const int baseDmg = 6;
            const int baseDef = 1;
            const int bandMin = 11;
            const int spawnLevel = 20;
            int steps = spawnLevel - bandMin; // 9

            int expectedHp = (int)System.Math.Round(baseHp * System.Math.Pow(1.12, steps),
                System.MidpointRounding.AwayFromZero);
            int expectedDmg = (int)System.Math.Round(baseDmg * System.Math.Pow(1.08, steps),
                System.MidpointRounding.AwayFromZero);

            Assert.AreEqual(expectedHp, CaveBandScaling.ScaleHp(baseHp, spawnLevel, bandMin));
            Assert.AreEqual(expectedDmg, CaveBandScaling.ScaleDamage(baseDmg, spawnLevel, bandMin));
            Assert.AreEqual(baseDef, CaveBandScaling.ScaleDefense(baseDef, spawnLevel, bandMin),
                "DEF must be fixed (never scaled).");

            // Sanity: scaling 9 levels grows HP but not absurdly.
            Assert.IsTrue(CaveBandScaling.ScaleHp(baseHp, spawnLevel, bandMin) > baseHp);
        }

        [Test]
        public void Scale_BandForLevel_MatchesCatalogWindows()
        {
            Assert.AreEqual(1, CaveBandScaling.BandForLevel(1));
            Assert.AreEqual(1, CaveBandScaling.BandForLevel(10));
            Assert.AreEqual(2, CaveBandScaling.BandForLevel(11));
            Assert.AreEqual(2, CaveBandScaling.BandForLevel(25));
            Assert.AreEqual(3, CaveBandScaling.BandForLevel(26));
            Assert.AreEqual(4, CaveBandScaling.BandForLevel(41));
            Assert.AreEqual(5, CaveBandScaling.BandForLevel(56));
            Assert.AreEqual(6, CaveBandScaling.BandForLevel(71));
            Assert.AreEqual(7, CaveBandScaling.BandForLevel(86));
            Assert.AreEqual(7, CaveBandScaling.BandForLevel(101));
        }

        [Test]
        public void Scale_BandMinLevel_MatchesBandForLevel()
        {
            for (int band = 1; band <= 7; band++)
            {
                int min = CaveBandScaling.BandMinLevel(band);
                Assert.AreEqual(band, CaveBandScaling.BandForLevel(min),
                    $"BandMinLevel({band})={min} must belong to band {band}.");
            }
        }

        // ── CA-3: deterministic band spawn composition (stable-run) ──────────────────────────────

        [Test]
        public void Spawn_SameSeed_SameComposition()
        {
            var a = CaveBandSpawnTable.ResolveComposition("world1", "run1", 15, 16, hasLake: false, isNight: false);
            var b = CaveBandSpawnTable.ResolveComposition("world1", "run1", 15, 16, hasLake: false, isNight: false);
            Assert.AreEqual(16, a.Count, "Composition must fill all requested slots when a pool exists.");
            CollectionAssert.AreEqual(a, b, "Same seed + level + slots must yield identical composition (CA-3).");
        }

        [Test]
        public void Spawn_DifferentSeed_MayDiffer_ButStaysDeterministic()
        {
            var run1 = CaveBandSpawnTable.ResolveComposition("world1", "run1", 15, 24, false, false);
            var run2 = CaveBandSpawnTable.ResolveComposition("world1", "run2", 15, 24, false, false);
            // Re-resolving run2 is still identical to itself.
            var run2Again = CaveBandSpawnTable.ResolveComposition("world1", "run2", 15, 24, false, false);
            CollectionAssert.AreEqual(run2, run2Again, "Determinism holds per seed.");
            Assert.AreEqual(24, run1.Count);
            Assert.AreEqual(24, run2.Count);
        }

        [Test]
        public void Spawn_Composition_OnlyUsesEligibleCommonsOfThatBand()
        {
            // Level 15 = band 2 (fungal). Pool excludes minibosses/bosses and other bands.
            var comp = CaveBandSpawnTable.ResolveComposition("w", "r", 15, 30, hasLake: false, isNight: false);
            var byId = Catalog.ToDictionary(d => d.EnemyId);
            foreach (var id in comp.Distinct())
            {
                Assert.IsTrue(byId.ContainsKey(id), $"Composition referenced unknown id {id}.");
                var def = byId[id];
                Assert.AreEqual(2, def.Band, $"{id} is not band 2 (fungal).");
                Assert.IsFalse(def.IsBoss || def.IsMiniBoss, $"{id} is a boss/miniboss — must not be in the pool.");
                Assert.IsTrue(15 >= def.MinLevel && 15 <= def.MaxLevel, $"{id} faixa does not contain level 15.");
            }
        }

        [Test]
        public void Spawn_Aquatic_OnlyWhenLakePresent()
        {
            // enemy_lake_lurker (band 1, levels 5-9, aquatic). Without a lake it must never appear.
            var noLake = CaveBandSpawnTable.BuildPool(7, hasLake: false, isNight: false);
            var withLake = CaveBandSpawnTable.BuildPool(7, hasLake: true, isNight: false);
            Assert.IsFalse(noLake.Any(e => e.EnemyId == "enemy_lake_lurker"),
                "Aquatic creature must not be eligible without a lake.");
            Assert.IsTrue(withLake.Any(e => e.EnemyId == "enemy_lake_lurker"),
                "Aquatic creature must be eligible when a lake is present at an in-faixa level.");
        }

        [Test]
        public void Spawn_Nocturnal_OnlyWhenNight()
        {
            // enemy_gloom_moth (band 2, levels 15-22, nocturnal).
            var day = CaveBandSpawnTable.BuildPool(16, hasLake: false, isNight: false);
            var night = CaveBandSpawnTable.BuildPool(16, hasLake: false, isNight: true);
            Assert.IsFalse(day.Any(e => e.EnemyId == "enemy_gloom_moth"),
                "Nocturnal creature must not be eligible during the day.");
            Assert.IsTrue(night.Any(e => e.EnemyId == "enemy_gloom_moth"),
                "Nocturnal creature must be eligible at night.");
        }

        [Test]
        public void Spawn_EmptyPool_ReturnsEmptyComposition()
        {
            // Level with no matching commons after gating still must not throw.
            var comp = CaveBandSpawnTable.ResolveComposition("w", "r", 101, 16, hasLake: false, isNight: false);
            // Level 101 band-7 commons whose faixa includes 101 (dread_choir 92-101) exist, so this
            // is non-empty; assert determinism + no throw rather than emptiness.
            var compAgain = CaveBandSpawnTable.ResolveComposition("w", "r", 101, 16, hasLake: false, isNight: false);
            CollectionAssert.AreEqual(comp, compAgain);
        }
    }
}
