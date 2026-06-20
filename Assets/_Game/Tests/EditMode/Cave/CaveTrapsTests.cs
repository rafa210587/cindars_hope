using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using CindarsHope.Cave.Traps;
using CindarsHope.Tools;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_60 — armadilhas da caverna por bioma/tier (subset v1 determinístico).
    ///
    /// Cobre os 6 critérios de aceite de forma DETERMINÍSTICA (ADR-0005 / cave-stable-run), reusando o
    /// gerador/perfil de bioma do fable_09 e os tipos canônicos do catálogo em código:
    /// CA-1 geração determinística por bioma/tier; CA-2 telegraph + efeito com counterplay;
    /// CA-3 desarme por ferramenta; CA-4 baú falso → Hoardmaw 1×; CA-5 revisita estável (snapshot);
    /// CA-6 detecção do amuleto de Nyx (flag sintética ON/OFF).
    /// </summary>
    [TestFixture]
    public class CaveTrapsTests
    {
        private const string WorldSeed = "world_seed_fable60";
        private const string RunSeed = "run_seed_fable60";

        private static CaveGenerationConfigSO MakeConfig()
        {
            var config = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
            config.Id = "test_trap_generation_config";
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
            var level = generator.Generate(config, caveLevel, worldSeed, runSeed, "biome_cave_test", profile);
            level.ComputeLayoutHash();
            Object.DestroyImmediate(config);
            return level;
        }

        // ---- Catálogo (CA-2: todo tipo tem telegraph > 0) ----------------------------------------

        [Test]
        public void Catalog_HasTenCanonicalTypes()
        {
            Assert.AreEqual(10, TrapDefinition.All.Count, "Subset v1 deve ter exatamente 10 armadilhas.");
        }

        [Test]
        public void Catalog_EveryTrapHasTelegraphGreaterThanZero()
        {
            foreach (var def in TrapDefinition.All)
            {
                Assert.Greater(def.TelegraphSeconds, 0f,
                    $"{def.TrapKey} precisa de telegraph > 0 (counterplay obrigatório §27/CA-2).");
            }
        }

        [Test]
        public void Catalog_FalseChestIsNotDisarmableButSpawnsHoardmaw()
        {
            var falseChest = TrapDefinition.Get(TrapId.FalseChest);
            Assert.IsNotNull(falseChest);
            Assert.IsFalse(falseChest.Disarmable, "Baú falso nunca é desarmável (só detectável).");
            Assert.AreEqual(TrapEffectCategory.SpawnEnemy, falseChest.Category);
            Assert.AreEqual("enemy_hoardmaw", falseChest.SpawnEnemyId);
        }

        [Test]
        public void Catalog_EveryBiomeBandHasAtLeastOneType()
        {
            foreach (var band in new[]
                     {
                         TrapDefinition.BiomeStone, TrapDefinition.BiomeFungal, TrapDefinition.BiomeIce,
                         TrapDefinition.BiomeFire, TrapDefinition.BiomeRuins, TrapDefinition.BiomeDeep,
                         TrapDefinition.BiomeVoid
                     })
            {
                Assert.Greater(TrapDefinition.PoolForBiome(band).Count, 0,
                    $"Bioma '{band}' não tem nenhum tipo no subset v1 (cobertura §26).");
            }
        }

        [Test]
        public void Catalog_StatusTrapsMapToCanonicalF01Ids()
        {
            Assert.AreEqual("status_poison", TrapDefinition.Get(TrapId.SporePod).StatusId);
            Assert.AreEqual("status_slow", TrapDefinition.Get(TrapId.SporePod).SecondaryStatusId);
            Assert.AreEqual("status_chill", TrapDefinition.Get(TrapId.IcePlate).StatusId);
            Assert.AreEqual("status_burn", TrapDefinition.Get(TrapId.EmberVent).StatusId);
            Assert.AreEqual("status_stun", TrapDefinition.Get(TrapId.RuneLockPulse).StatusId);
            Assert.AreEqual("status_root", TrapDefinition.Get(TrapId.RootSnare).StatusId);
            Assert.AreEqual("status_chill", TrapDefinition.Get(TrapId.FrostBurstRune).StatusId);
        }

        // ---- CA-1 Geração determinística por bioma/tier ------------------------------------------

        [Test]
        public void CA1_TrapPlan_IsDeterministicForSameSeed()
        {
            for (var level = 1; level <= 101; level += 7)
            {
                var generated = Generate(level);
                var spawn = generated.Entrance;
                var planA = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, spawn);
                var planB = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, spawn);

                Assert.AreEqual(planA.Traps.Count, planB.Traps.Count, $"Level {level}: contagem divergiu.");
                for (var i = 0; i < planA.Traps.Count; i++)
                {
                    Assert.AreEqual(planA.Traps[i].TrapInstanceId, planB.Traps[i].TrapInstanceId);
                    Assert.AreEqual(planA.Traps[i].Cell, planB.Traps[i].Cell);
                    Assert.AreEqual(planA.Traps[i].TrapId, planB.Traps[i].TrapId);
                }
            }
        }

        [Test]
        public void CA1_TrapTypes_AlwaysFromBiomePool()
        {
            for (var level = 1; level <= 101; level += 3)
            {
                var generated = Generate(level);
                var band = CaveBiomeLayoutProfile.ForLevel(level).BandId;
                var poolIds = new HashSet<TrapId>(TrapDefinition.PoolForBiome(band).Select(d => d.Id));

                var plan = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                foreach (var trap in plan.Traps)
                {
                    Assert.Contains(trap.TrapId, poolIds.ToList(),
                        $"Level {level} ({band}): {trap.TrapId} fora do pool do bioma (§26).");
                }
            }
        }

        [Test]
        public void CA1_TrapPositions_NeverOnCriticalPathOrAnchors()
        {
            for (var level = 1; level <= 101; level += 5)
            {
                var generated = Generate(level);
                var pathTiles = CaveHazardPlanner.ComputeEntranceExitPathWithBuffer(generated);
                var plan = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);

                foreach (var trap in plan.Traps)
                {
                    Assert.AreNotEqual(generated.Entrance, trap.Cell, $"Level {level}: armadilha na entrada.");
                    Assert.AreNotEqual(generated.Exit, trap.Cell, $"Level {level}: armadilha na saída.");
                    Assert.IsFalse(pathTiles.Contains(trap.Cell),
                        $"Level {level}: armadilha {trap.TrapId} no caminho crítico {trap.Cell} (§22).");
                    Assert.IsTrue(generated.WalkableTiles.Contains(trap.Cell),
                        $"Level {level}: armadilha fora de célula walkable.");
                    Assert.IsFalse(generated.WallTiles.Contains(trap.Cell),
                        $"Level {level}: armadilha sobre parede.");
                }
            }
        }

        [Test]
        public void CA1_TrapCount_WithinSizeAndTierRanges()
        {
            for (var level = 1; level <= 101; level += 4)
            {
                var generated = Generate(level);
                var band = CaveTrapPlanner.ResolveBand(level);

                CaveTrapPlanner.ResolveSizeRange(generated.Width * generated.Height, out var sizeMin, out var sizeMax);
                CaveTrapPlanner.ResolveTierRange(band, out var tierMin, out var tierMax);
                var max = Mathf.Min(Mathf.Min(sizeMax, tierMax), 5);

                var plan = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                Assert.LessOrEqual(plan.Traps.Count, max,
                    $"Level {level}: {plan.Traps.Count} armadilhas acima do teto {max} (§24∩§25).");
                Assert.GreaterOrEqual(plan.Traps.Count, 0);
            }
        }

        [Test]
        public void CA1_DifferentLevels_ProduceDistinctPlans()
        {
            // Em uma amostra, ao menos dois níveis distintos devem ter planos distintos (anti-clone).
            var signatures = new HashSet<string>();
            for (var level = 11; level <= 90; level += 9)
            {
                var generated = Generate(level);
                var plan = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);
                signatures.Add(string.Join("|", plan.Traps.Select(t => $"{t.TrapId}:{t.Cell.x},{t.Cell.y}")));
            }

            Assert.Greater(signatures.Count, 1, "Planos de níveis distintos não deveriam ser todos idênticos.");
        }

        [Test]
        public void CA1_TrapInstanceIds_AreStableAndContainNoGuid()
        {
            var generated = Generate(60);
            var plan = CaveTrapPlanner.BuildPlan(generated, WorldSeed, RunSeed, generated.Entrance);

            foreach (var trap in plan.Traps)
            {
                Assert.IsNotEmpty(trap.TrapInstanceId);
                // Ids estáveis = hash posicional; um GUID tem 32 hex + hifens. Garante formato curto.
                Assert.IsFalse(System.Guid.TryParse(trap.TrapInstanceId, out _),
                    "TrapInstanceId nunca deve ser um GUID (cave-stable-run).");
                Assert.IsTrue(trap.TrapInstanceId.StartsWith("trap_"), "Id deve seguir o formato estável.");

                // Re-derivação produz o mesmo id (idempotente).
                var reId = CaveTrapPlanner.BuildTrapInstanceId(RunSeed, generated.CaveLevel, trap.Cell, trap.TrapId);
                Assert.AreEqual(trap.TrapInstanceId, reId);
            }
        }

        [Test]
        public void CA1_DifferentRunSeed_DivergesPlanOnAverage()
        {
            var matches = 0;
            var total = 0;
            for (var level = 11; level <= 80; level += 10)
            {
                var genA = Generate(level, WorldSeed, "trap_run_A");
                var genB = Generate(level, WorldSeed, "trap_run_B");
                var planA = CaveTrapPlanner.BuildPlan(genA, WorldSeed, "trap_run_A", genA.Entrance);
                var planB = CaveTrapPlanner.BuildPlan(genB, WorldSeed, "trap_run_B", genB.Entrance);

                var sigA = string.Join("|", planA.Traps.Select(t => $"{t.TrapId}:{t.Cell.x},{t.Cell.y}"));
                var sigB = string.Join("|", planB.Traps.Select(t => $"{t.TrapId}:{t.Cell.x},{t.Cell.y}"));
                if (sigA == sigB) matches++;
                total++;
            }

            Assert.Less(matches, total, "Runs diferentes deveriam (quase sempre) divergir nos planos.");
        }

        // ---- CA-2 Telegraph e dano por tier ------------------------------------------------------

        [Test]
        public void CA2_DamageScalesWithBand_AndIsZeroForPureStatus()
        {
            var spike = TrapDefinition.Get(TrapId.SpikeFloor);
            Assert.Greater(spike.ResolveDamage(7), spike.ResolveDamage(1),
                "Dano da armadilha de dano direto deve crescer com a banda.");

            var sporePod = TrapDefinition.Get(TrapId.SporePod);
            Assert.AreEqual(0, sporePod.ResolveDamage(5), "Armadilha de status puro não causa dano direto.");

            var falseChest = TrapDefinition.Get(TrapId.FalseChest);
            Assert.AreEqual(0, falseChest.ResolveDamage(7), "Baú falso não causa dano direto.");
        }

        // ---- CA-3 Desarme por ferramenta ---------------------------------------------------------

        [Test]
        public void CA3_DisarmChance_IncreasesWithToolTier()
        {
            Assert.AreEqual(0f, TrapDisarmResolver.ChanceForTier(ToolTier.None), 0.0001f);
            Assert.AreEqual(0.50f, TrapDisarmResolver.ChanceForTier(ToolTier.Basic), 0.0001f);
            Assert.AreEqual(0.65f, TrapDisarmResolver.ChanceForTier(ToolTier.Copper), 0.0001f);
            Assert.AreEqual(0.80f, TrapDisarmResolver.ChanceForTier(ToolTier.Iron), 0.0001f);
            // Gold/Diamond batem no teto 0.95 (nunca 100% garantido — §22).
            Assert.AreEqual(0.95f, TrapDisarmResolver.ChanceForTier(ToolTier.Gold), 0.0001f);
            Assert.AreEqual(0.95f, TrapDisarmResolver.ChanceForTier(ToolTier.Diamond), 0.0001f);
        }

        [Test]
        public void CA3_Disarm_IsDeterministicPerAttemptSeed()
        {
            const string trapId = "trap_5_10_12_trap_spike_floor_abcd1234";
            var a = TrapDisarmResolver.TryDisarm(ToolTier.Iron, RunSeed, 5, trapId);
            var b = TrapDisarmResolver.TryDisarm(ToolTier.Iron, RunSeed, 5, trapId);
            Assert.AreEqual(a, b, "Mesma tentativa (seed) deve dar o mesmo desfecho.");
        }

        [Test]
        public void CA3_Disarm_NoneTierNeverSucceeds()
        {
            for (var level = 1; level <= 20; level++)
            {
                Assert.IsFalse(TrapDisarmResolver.TryDisarm(ToolTier.None, RunSeed, level, $"trap_{level}"),
                    "Sem ferramenta (None) o desarme nunca tem sucesso.");
            }
        }

        [Test]
        public void CA3_Disarm_RollBelowChanceSucceeds_RollAtOrAboveFails()
        {
            // Borda: o desfecho deve ser exatamente roll < chance.
            const string trapId = "trap_30_5_5_trap_ice_plate_ff00ff00";
            const int level = 30;
            foreach (var tier in new[] { ToolTier.Basic, ToolTier.Copper, ToolTier.Iron, ToolTier.Gold })
            {
                var chance = TrapDisarmResolver.ChanceForTier(tier);
                var roll = TrapDisarmResolver.Roll01(tier, RunSeed, level, trapId);
                var expected = roll < chance;
                Assert.AreEqual(expected, TrapDisarmResolver.TryDisarm(tier, RunSeed, level, trapId),
                    $"Tier {tier}: desfecho deve ser (roll {roll:F3} < chance {chance:F3}).");
            }
        }

        // ---- CA-5 Revisita estável (snapshot round-trip) -----------------------------------------

        [Test]
        public void CA5_SnapshotTrapState_RoundTripsAndOnlyAdvances()
        {
            var snapshot = new CaveLevelSnapshot(5, "biome_cave_test", "hash", WorldSeed, RunSeed);
            snapshot.SetTrapState("trap_a", "trap_spike_floor", new Vector2Int(3, 4), (int)TrapState.Armed);
            snapshot.SetTrapState("trap_b", "trap_ice_plate", new Vector2Int(7, 8), (int)TrapState.Triggered);

            Assert.AreEqual((int)TrapState.Armed, snapshot.GetTrapState("trap_a"));
            Assert.AreEqual((int)TrapState.Triggered, snapshot.GetTrapState("trap_b"));

            // Avança trap_a para Disarmed; trap_b NÃO regride (já Triggered) se vier Armed.
            snapshot.SetTrapState("trap_a", "trap_spike_floor", new Vector2Int(3, 4), (int)TrapState.Disarmed);
            snapshot.SetTrapState("trap_b", "trap_ice_plate", new Vector2Int(7, 8), (int)TrapState.Armed);

            Assert.AreEqual((int)TrapState.Disarmed, snapshot.GetTrapState("trap_a"));
            Assert.AreEqual((int)TrapState.Triggered, snapshot.GetTrapState("trap_b"),
                "Estado terminal não pode regredir na revisita (cave-stable-run).");

            // Apenas 2 entradas (idempotente por instanceId).
            Assert.AreEqual(2, snapshot.TrapStates.Count);
        }

        [Test]
        public void CA5_RevisitPlan_IsIdenticalForSameRun()
        {
            // O plano re-derivado na revisita (mesmo run/level) é idêntico — composição estável.
            var first = Generate(45);
            var planFirst = CaveTrapPlanner.BuildPlan(first, WorldSeed, RunSeed, first.Entrance);

            var revisit = Generate(45); // mesmo seed/level → mesma geração
            var planRevisit = CaveTrapPlanner.BuildPlan(revisit, WorldSeed, RunSeed, revisit.Entrance);

            Assert.AreEqual(planFirst.Traps.Count, planRevisit.Traps.Count);
            for (var i = 0; i < planFirst.Traps.Count; i++)
            {
                Assert.AreEqual(planFirst.Traps[i].TrapInstanceId, planRevisit.Traps[i].TrapInstanceId);
                Assert.AreEqual(planFirst.Traps[i].Cell, planRevisit.Traps[i].Cell);
                Assert.AreEqual(planFirst.Traps[i].TrapId, planRevisit.Traps[i].TrapId);
            }
        }

        [Test]
        public void CA5_LegacySnapshotWithoutTraps_DefaultsToArmed()
        {
            var snapshot = new CaveLevelSnapshot(5, "biome_cave_test", "hash", WorldSeed, RunSeed);
            // Snapshot legado: nenhuma entrada de armadilha. Consulta retorna Armed (re-derivação).
            Assert.AreEqual((int)TrapState.Armed, snapshot.GetTrapState("trap_unknown"));
            Assert.AreEqual(0, snapshot.TrapStates.Count);
        }

        // ---- CA-4 Baú falso → Hoardmaw (1×) ------------------------------------------------------

        [Test]
        public void CA4_FalseChest_SpawnsHoardmawExactlyOnce()
        {
            var go = new GameObject("false_chest_test");
            try
            {
                var chest = go.AddComponent<FalseChestTrap>();
                var placement = new CaveTrapPlacement
                {
                    TrapInstanceId = "trap_false_chest_test",
                    TrapId = TrapId.FalseChest,
                    Cell = new Vector2Int(5, 5),
                    Band = 5
                };

                var spawnCount = 0;
                var capturedId = string.Empty;
                System.Func<string, bool> spawn = id =>
                {
                    spawnCount++;
                    capturedId = id;
                    return true;
                };

                var stateChanges = new List<TrapState>();
                chest.Configure(placement, 60, TrapState.Armed, null, spawn, (_, s) => stateChanges.Add(s));

                Assert.IsTrue(chest.CanInteract(null));
                chest.Interact(null); // abrir → spawn

                Assert.AreEqual(1, spawnCount, "Hoardmaw deve spawnar exatamente 1×.");
                Assert.AreEqual("enemy_hoardmaw", capturedId);
                Assert.AreEqual(TrapState.Triggered, chest.State);
                Assert.Contains(TrapState.Triggered, stateChanges);

                // Reentrada não duplica (estado já Triggered).
                chest.Interact(null);
                Assert.AreEqual(1, spawnCount, "Reentrada não pode duplicar o spawn.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---- CA-6 Detecção do amuleto de Nyx (flag sintética) ------------------------------------

        [Test]
        public void CA6_Detection_OnlyRevealsWithinRadius_WhenFlagOn()
        {
            var plan = new CaveTrapPlan();
            plan.Traps.Add(new CaveTrapPlacement
            {
                TrapInstanceId = "near", TrapId = TrapId.SpikeFloor, Cell = new Vector2Int(5, 5), Band = 1
            });
            plan.Traps.Add(new CaveTrapPlacement
            {
                TrapInstanceId = "far", TrapId = TrapId.SpikeFloor, Cell = new Vector2Int(40, 40), Band = 1
            });

            var player = new Vector2Int(6, 6);

            // Flag OFF (raio 0): nada é detectado.
            var off = TrapDetectionService.ResolveDetectedTraps(plan, player, 0f);
            Assert.AreEqual(0, off.Count, "Sem o efeito F23, nada deve ser detectado (flag dormente).");

            // Flag ON (raio 5): só a armadilha próxima.
            var on = TrapDetectionService.ResolveDetectedTraps(plan, player, 5f);
            Assert.AreEqual(1, on.Count, "Com o efeito F23, só a armadilha no raio deve ser detectada.");
            Assert.AreEqual("near", on[0].TrapInstanceId);
        }

        [Test]
        public void CA6_Detection_SkipsAlreadyResolvedTraps()
        {
            var plan = new CaveTrapPlan();
            plan.Traps.Add(new CaveTrapPlacement
            {
                TrapInstanceId = "armed", TrapId = TrapId.SpikeFloor, Cell = new Vector2Int(5, 5), Band = 1
            });
            plan.Traps.Add(new CaveTrapPlacement
            {
                TrapInstanceId = "disarmed", TrapId = TrapId.SpikeFloor, Cell = new Vector2Int(5, 6), Band = 1
            });

            var states = new Dictionary<string, TrapState>
            {
                { "armed", TrapState.Armed },
                { "disarmed", TrapState.Disarmed }
            };

            var detected = TrapDetectionService.ResolveDetectedTraps(plan, new Vector2Int(5, 5), 5f, states);
            Assert.AreEqual(1, detected.Count, "Armadilha já desarmada não deve gerar aviso de detecção.");
            Assert.AreEqual("armed", detected[0].TrapInstanceId);
        }

        [Test]
        public void CA6_DetectionService_IsInactiveByDefault()
        {
            // Sem router ativo (teste puro), o efeito é dormente: raio 0, detecção inativa.
            var previous = CindarsHope.Equipment.AccessoryEffectRouter.Active;
            CindarsHope.Equipment.AccessoryEffectRouter.Active = null;
            try
            {
                Assert.AreEqual(0f, TrapDetectionService.ActiveDetectionRadius(), 0.0001f);
                Assert.IsFalse(TrapDetectionService.IsDetectionActive());
            }
            finally
            {
                CindarsHope.Equipment.AccessoryEffectRouter.Active = previous;
            }
        }
    }
}
