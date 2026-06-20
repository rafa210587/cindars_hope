using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_09 — variedade de layout por bioma, hazards e sala de tesouro.
    ///
    /// Cobre os 4 critérios de aceite de forma DETERMINÍSTICA (ADR-0005 / cave-stable-run):
    /// CA-1 identidade por banda; CA-2 hazards determinísticos e justos; CA-3 sala de tesouro
    /// estável + round-trip do snapshot; CA-4 replay/LayoutHash intactos.
    /// </summary>
    [TestFixture]
    public class CaveBiomeLayoutTests
    {
        private const string WorldSeed = "world_seed_fable09";
        private const string RunSeed = "run_seed_fable09";

        private static CaveGenerationConfigSO MakeConfig()
        {
            // Config "base" — o perfil de banda sobrepõe dimensões/salas/corredor de forma determinística.
            var config = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
            config.Id = "test_cave_generation_config";
            config.TargetWidth = 80;
            config.TargetHeight = 60;
            config.MinRooms = 6;
            config.MaxRooms = 10;
            config.EnemyPointCount = 10;
            config.ResourcePointCount = 12;
            return config;
        }

        private static CaveGeneratedLevel Generate(int caveLevel, string worldSeed = WorldSeed, string runSeed = RunSeed)
        {
            var config = MakeConfig();
            var generator = new CaveProceduralGenerator();
            var profile = CaveBiomeLayoutProfile.ForLevel(caveLevel);
            var level = generator.Generate(config, caveLevel, worldSeed, runSeed, "biome_cave_earth", profile);
            level.ComputeLayoutHash();
            Object.DestroyImmediate(config);
            return level;
        }

        // ---- Banda / perfil ----------------------------------------------------------------------

        [Test]
        public void ForLevel_MapsCanonicalBands()
        {
            Assert.AreEqual("stone", CaveBiomeLayoutProfile.ForLevel(1).BandId);
            Assert.AreEqual("stone", CaveBiomeLayoutProfile.ForLevel(10).BandId);
            Assert.AreEqual("fungal", CaveBiomeLayoutProfile.ForLevel(11).BandId);
            Assert.AreEqual("fungal", CaveBiomeLayoutProfile.ForLevel(25).BandId);
            Assert.AreEqual("ice", CaveBiomeLayoutProfile.ForLevel(26).BandId);
            Assert.AreEqual("ice", CaveBiomeLayoutProfile.ForLevel(40).BandId);
            Assert.AreEqual("fire", CaveBiomeLayoutProfile.ForLevel(41).BandId);
            Assert.AreEqual("fire", CaveBiomeLayoutProfile.ForLevel(55).BandId);
            Assert.AreEqual("ruins", CaveBiomeLayoutProfile.ForLevel(56).BandId);
            Assert.AreEqual("ruins", CaveBiomeLayoutProfile.ForLevel(70).BandId);
            Assert.AreEqual("deep", CaveBiomeLayoutProfile.ForLevel(71).BandId);
            Assert.AreEqual("deep", CaveBiomeLayoutProfile.ForLevel(85).BandId);
            Assert.AreEqual("void", CaveBiomeLayoutProfile.ForLevel(86).BandId);
            Assert.AreEqual("void", CaveBiomeLayoutProfile.ForLevel(101).BandId);
            Assert.AreEqual("void", CaveBiomeLayoutProfile.ForLevel(150).BandId);
        }

        [Test]
        public void ForLevel_NeverReturnsNull()
        {
            for (var level = 1; level <= 200; level++)
            {
                Assert.IsNotNull(CaveBiomeLayoutProfile.ForLevel(level), $"Level {level} returned null profile.");
            }
        }

        [Test]
        public void MapSize_IsDeterministicAndWithinAllowedSet()
        {
            var allowed = new[]
            {
                CaveBiomeLayoutProfile.SmallMapSize,
                CaveBiomeLayoutProfile.BaseMapSize,
                CaveBiomeLayoutProfile.LargeMapSize
            };

            for (var level = 1; level <= 101; level++)
            {
                var first = CaveBiomeLayoutProfile.ResolveMapSize(WorldSeed, RunSeed, level);
                var second = CaveBiomeLayoutProfile.ResolveMapSize(WorldSeed, RunSeed, level);
                Assert.AreEqual(first, second, $"Level {level} size not deterministic.");
                CollectionAssert.Contains(allowed, first, $"Level {level} size {first} outside allowed set.");
            }
        }

        [Test]
        public void MapSize_OscillationDistribution_RoughlyMatchesEmenda()
        {
            int small = 0, large = 0, baseCount = 0;
            for (var level = 1; level <= 400; level++)
            {
                var size = CaveBiomeLayoutProfile.ResolveMapSize(WorldSeed, RunSeed, level);
                if (size == CaveBiomeLayoutProfile.SmallMapSize) small++;
                else if (size == CaveBiomeLayoutProfile.LargeMapSize) large++;
                else baseCount++;
            }

            // EMENDA Q12.1: ~20% pequeno, ~20% grande, ~60% base. Faixa tolerante.
            Assert.That(small / 400f, Is.InRange(0.12f, 0.30f), $"small share {small / 400f:P0}");
            Assert.That(large / 400f, Is.InRange(0.12f, 0.30f), $"large share {large / 400f:P0}");
            Assert.That(baseCount / 400f, Is.InRange(0.45f, 0.72f), $"base share {baseCount / 400f:P0}");
        }

        // ---- CA-1 Identidade por banda ------------------------------------------------------------

        [Test]
        public void CA1_DifferentBands_ProduceDistinctRoomMetrics()
        {
            var stone = Generate(5);
            var ice = Generate(30);
            var ruins = Generate(60);

            Assert.AreEqual("stone", stone.LayoutProfileBandId);
            Assert.AreEqual("ice", ice.LayoutProfileBandId);
            Assert.AreEqual("ruins", ruins.LayoutProfileBandId);

            // Cada banda gera ao menos uma sala e tem walkable.
            foreach (var level in new[] { stone, ice, ruins })
            {
                Assert.Greater(level.Rooms.Count, 0, $"Band {level.LayoutProfileBandId} produced no rooms.");
                Assert.Greater(level.WalkableTiles.Count, 0, $"Band {level.LayoutProfileBandId} produced no walkable tiles.");
            }

            // Ice (salas grandes, poucas) vs. ruins (galerias grandes) vs. stone — métricas distintas.
            var stoneAvgRoom = stone.Rooms.Average(r => r.Width * r.Height);
            var iceAvgRoom = ice.Rooms.Average(r => r.Width * r.Height);

            // Ice tem salas em média maiores que stone (perfil: ice 14-26 vs stone 10-20).
            Assert.Greater(iceAvgRoom, stoneAvgRoom,
                $"Ice avg room area ({iceAvgRoom:F0}) should exceed stone ({stoneAvgRoom:F0}).");
        }

        [Test]
        public void CA1_RoomCount_RespectsScaledRangePerBand()
        {
            // Para uma amostra de níveis, a contagem de salas fica dentro do range re-escalado por área.
            foreach (var level in new[] { 5, 18, 30, 48, 62, 78, 95 })
            {
                var generated = Generate(level);
                var profile = CaveBiomeLayoutProfile.ForLevel(level);
                profile.ResolveScaledRoomCount(generated.Width, generated.Height, out var minRooms, out var maxRooms);

                Assert.GreaterOrEqual(generated.Rooms.Count, 1,
                    $"Level {level} produced no rooms.");
                Assert.LessOrEqual(generated.Rooms.Count, maxRooms,
                    $"Level {level} produced {generated.Rooms.Count} rooms, above scaled max {maxRooms}.");
            }
        }

        // ---- Determinismo de geração (base do cave-stable-run) -----------------------------------

        [Test]
        public void Generation_SameSeed_ProducesIdenticalLayoutHash()
        {
            for (var level = 1; level <= 101; level += 10)
            {
                var a = Generate(level);
                var b = Generate(level);
                Assert.AreEqual(a.LayoutHash, b.LayoutHash, $"Level {level} layout hash diverged for same seed.");
                Assert.AreEqual(a.Width, b.Width);
                Assert.AreEqual(a.Height, b.Height);
                CollectionAssert.AreEquivalent(a.WalkableTiles, b.WalkableTiles, $"Level {level} walkable set diverged.");
                Assert.AreEqual(a.Entrance, b.Entrance);
                Assert.AreEqual(a.Exit, b.Exit);
            }
        }

        [Test]
        public void Generation_DifferentRunSeed_DivergesLayout()
        {
            var runA = Generate(30, WorldSeed, "run_A");
            var runB = Generate(30, WorldSeed, "run_B");
            Assert.AreNotEqual(runA.LayoutHash, runB.LayoutHash,
                "Different run seeds should (very likely) produce different layouts.");
        }

        // ---- CA-2 Hazards determinísticos e justos -----------------------------------------------

        [Test]
        public void CA2_HazardPlan_IsDeterministicForSameSeed()
        {
            for (var level = 1; level <= 101; level += 7)
            {
                var generated = Generate(level);
                var spawn = generated.Entrance;
                var planA = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, spawn);
                var planB = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, spawn);

                Assert.AreEqual(planA.Hazards.Count, planB.Hazards.Count, $"Level {level} hazard count diverged.");
                for (var i = 0; i < planA.Hazards.Count; i++)
                {
                    Assert.AreEqual(planA.Hazards[i].HazardId, planB.Hazards[i].HazardId);
                    Assert.AreEqual(planA.Hazards[i].GridPosition, planB.Hazards[i].GridPosition);
                    Assert.AreEqual(planA.Hazards[i].Kind, planB.Hazards[i].Kind);
                }
            }
        }

        [Test]
        public void CA2_Hazards_NeverOnEntranceExitOrPath_AndAwayFromSpawn()
        {
            for (var level = 1; level <= 101; level += 3)
            {
                var generated = Generate(level);
                var spawn = generated.Entrance;
                var pathTiles = CaveHazardPlanner.ComputeEntranceExitPathWithBuffer(generated);
                var plan = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, spawn);

                foreach (var hazard in plan.Hazards)
                {
                    Assert.AreNotEqual(generated.Entrance, hazard.GridPosition, $"Level {level}: hazard on entrance.");
                    Assert.AreNotEqual(generated.Exit, hazard.GridPosition, $"Level {level}: hazard on exit.");
                    Assert.IsFalse(pathTiles.Contains(hazard.GridPosition),
                        $"Level {level}: hazard {hazard.HazardId} sits on the mandatory entrance-exit path.");
                    Assert.IsTrue(generated.WalkableTiles.Contains(hazard.GridPosition),
                        $"Level {level}: hazard {hazard.HazardId} not on a walkable tile.");

                    var distSpawn = Mathf.Abs(hazard.GridPosition.x - spawn.x) + Mathf.Abs(hazard.GridPosition.y - spawn.y);
                    Assert.GreaterOrEqual(distSpawn, CaveHazardPlanner.MinDistanceFromPlayerSpawn,
                        $"Level {level}: hazard too close to player spawn.");
                }
            }
        }

        [Test]
        public void CA2_HazardCount_RespectsBandMaximum()
        {
            for (var level = 1; level <= 101; level++)
            {
                var generated = Generate(level);
                var profile = CaveBiomeLayoutProfile.ForLevel(level);
                var plan = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                Assert.LessOrEqual(plan.Hazards.Count, profile.MaxHazards,
                    $"Level {level} produced {plan.Hazards.Count} hazards, above band max {profile.MaxHazards}.");
            }
        }

        [Test]
        public void CA2_HazardKinds_AreAllowedByBand()
        {
            for (var level = 1; level <= 101; level += 2)
            {
                var generated = Generate(level);
                var profile = CaveBiomeLayoutProfile.ForLevel(level);
                var allowed = new HashSet<CaveHazardKind>(profile.AllowedHazards);
                var plan = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                foreach (var hazard in plan.Hazards)
                {
                    Assert.IsTrue(allowed.Contains(hazard.Kind),
                        $"Level {level}: hazard kind {hazard.Kind} not allowed in band {profile.BandId}.");
                }
            }
        }

        [Test]
        public void CA2_TelegraphColor_DistinctPerKind()
        {
            var toxic = CaveHazardTile.ResolveTelegraphColor(CaveHazardKind.ToxicPool);
            var ice = CaveHazardTile.ResolveTelegraphColor(CaveHazardKind.IceSlick);
            var rock = CaveHazardTile.ResolveTelegraphColor(CaveHazardKind.FallingRock);
            Assert.AreNotEqual(toxic, ice);
            Assert.AreNotEqual(ice, rock);
            Assert.AreNotEqual(toxic, rock);
        }

        // ---- CA-3 Sala de tesouro estável --------------------------------------------------------

        [Test]
        public void CA3_TreasureRoom_IsDeterministicForSameSeed()
        {
            for (var level = 1; level <= 101; level += 5)
            {
                var generated = Generate(level);
                var planA = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                var planB = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);

                Assert.AreEqual(planA.HasTreasureRoom, planB.HasTreasureRoom, $"Level {level}: treasure presence diverged.");
                if (planA.HasTreasureRoom)
                {
                    Assert.AreEqual(planA.TreasureRoom.ChestId, planB.TreasureRoom.ChestId);
                    Assert.AreEqual(planA.TreasureRoom.ChestGridPosition, planB.TreasureRoom.ChestGridPosition);
                    Assert.AreEqual(planA.TreasureRoom.LootSeed, planB.TreasureRoom.LootSeed);
                }
            }
        }

        [Test]
        public void CA3_TreasureRoomRate_IsWithinTenToTwentyPercent_Over200Levels()
        {
            // Mede em 200 níveis sintéticos (mesma run). EMENDA/CA-3: 10-20%.
            var treasureCount = 0;
            const int sampleSize = 200;
            for (var level = 1; level <= sampleSize; level++)
            {
                var generated = Generate(level);
                var plan = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                if (plan.HasTreasureRoom)
                {
                    treasureCount++;
                }
            }

            var rate = treasureCount / (float)sampleSize;
            Assert.That(rate, Is.InRange(0.10f, 0.20f),
                $"Treasure room rate {rate:P1} ({treasureCount}/{sampleSize}) outside the 10-20% window.");
        }

        [Test]
        public void CA3_TreasureChest_NotInEntranceRoom_AndOnWalkable()
        {
            var found = 0;
            for (var level = 1; level <= 200 && found < 25; level++)
            {
                var generated = Generate(level);
                var plan = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                if (!plan.HasTreasureRoom)
                {
                    continue;
                }

                found++;
                var chestPos = plan.TreasureRoom.ChestGridPosition;
                Assert.IsTrue(generated.WalkableTiles.Contains(chestPos),
                    $"Level {level}: chest not on walkable tile.");
                Assert.AreNotEqual(generated.Entrance, chestPos, $"Level {level}: chest on entrance.");
            }

            Assert.Greater(found, 0, "No treasure rooms found across 200 levels — unexpected.");
        }

        [Test]
        public void CA3_OpenedChest_RoundTripsThroughSnapshot()
        {
            // Acha um nível com tesouro e prova o round-trip do OpenedChestIds via CaveSnapshotService.
            var service = new CaveSnapshotService();
            for (var level = 1; level <= 200; level++)
            {
                var generated = Generate(level);
                var plan = CaveHazardPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                if (!plan.HasTreasureRoom)
                {
                    continue;
                }

                var chestId = plan.TreasureRoom.ChestId;

                // Captura inicial (baú fechado) → sem aberto.
                var initial = service.CaptureSnapshot(generated, WorldSeed, RunSeed, null, null, null, null, null, null);
                Assert.IsFalse(initial.IsChestOpened(chestId), "Chest should start closed.");

                // Captura com o baú aberto → round-trip preserva.
                var withOpened = service.CaptureSnapshot(
                    generated, WorldSeed, RunSeed, null, null, null, null, null,
                    new List<string> { chestId });
                Assert.IsTrue(withOpened.IsChestOpened(chestId), "Opened chest id must survive capture.");
                CollectionAssert.Contains(withOpened.OpenedChestIds, chestId);

                // O LayoutHash NÃO muda por causa do baú aberto (estado mutável fora do hash) → replay intacto.
                Assert.AreEqual(initial.LayoutHash, withOpened.LayoutHash,
                    "Opened chest state must NOT change the LayoutHash (replay invariant).");
                return;
            }

            Assert.Fail("No treasure room found across 200 levels to exercise the round-trip.");
        }

        // ---- CA-4 Replay intacto -----------------------------------------------------------------

        [Test]
        public void CA4_SnapshotRoundTrip_PreservesLayoutHash()
        {
            var service = new CaveSnapshotService();
            for (var level = 1; level <= 101; level += 9)
            {
                var generated = Generate(level);
                var snapshot = service.CaptureSnapshot(generated, WorldSeed, RunSeed, null, null, null, null, null, null);

                Assert.IsTrue(snapshot.IsValid(), $"Level {level}: captured snapshot invalid.");
                Assert.IsTrue(service.ValidateReplayHash(snapshot),
                    $"Level {level}: replay hash mismatch after capture.");

                // Restaura e recomputa: o hash do snapshot bate com o recomputado (replay PASS).
                var restored = service.RestoreGeneratedLevel(snapshot);
                Assert.IsNotNull(restored, $"Level {level}: restore returned null.");
                Assert.AreEqual(snapshot.LayoutHash, service.CalculateLayoutHash(snapshot),
                    $"Level {level}: recomputed layout hash diverged.");
            }
        }

        [Test]
        public void CA4_Hazards_DoNotAffectLayoutHash()
        {
            // O LayoutHash é estrutural; hazards são derivados à parte e não entram nele.
            for (var level = 1; level <= 101; level += 11)
            {
                var withProfileA = Generate(level);
                var withProfileB = Generate(level);
                Assert.AreEqual(withProfileA.LayoutHash, withProfileB.LayoutHash,
                    $"Level {level}: layout hash should be stable regardless of hazard derivation.");
            }
        }
    }
}
