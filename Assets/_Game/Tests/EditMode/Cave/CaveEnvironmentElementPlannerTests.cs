using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_78 (SLICE 1) — planejador de elementos ambientais determinístico (critério 14.2).
    ///
    /// Cobre: determinismo (mesmo seed → mesmo plano), não-bloqueio do caminho entrance↔exit (a saída
    /// continua alcançável após colocar elementos), presença obrigatória de pedra/minério, e respeito
    /// ao perfil (água propagada; só tipos do perfil são colocados). Determinístico (FNV-1a) — sem RNG.
    /// </summary>
    [TestFixture]
    public class CaveEnvironmentElementPlannerTests
    {
        private const string WorldSeed = "world_seed_fable78_env";
        private const string RunSeed = "run_seed_fable78_env";

        private static CaveGenerationConfigSO MakeConfig()
        {
            var config = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
            config.Id = "test_env_generation_config";
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

        private static CaveEnvironmentElementProfileSO MakeProfile(int band = 0, bool hasWater = true)
        {
            var profile = ScriptableObject.CreateInstance<CaveEnvironmentElementProfileSO>();
            SetPrivate(profile, "_id", $"cave_elem_profile_test_{band}");
            SetPrivate(profile, "_biomeId", $"biome_cave_test_{band}");
            SetPrivate(profile, "_band", band);
            SetPrivate(profile, "_hasWater", hasWater);
            SetPrivate(profile, "_entries", new[]
            {
                new CaveEnvironmentElementProfileSO.ElementEntry
                {
                    Kind = CaveEnvironmentElementKind.DecorNonBlocking, Weight = 4f, MineNodeDataId = string.Empty
                },
                new CaveEnvironmentElementProfileSO.ElementEntry
                {
                    Kind = CaveEnvironmentElementKind.DecorBlocking, Weight = 1f, MineNodeDataId = string.Empty
                },
                new CaveEnvironmentElementProfileSO.ElementEntry
                {
                    Kind = CaveEnvironmentElementKind.MineableNode, Weight = 2f, MineNodeDataId = "resnode_ore_copper_stone"
                }
            });
            return profile;
        }

        private static void SetPrivate(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(info, $"Campo privado não encontrado: {field}");
            info.SetValue(target, value);
        }

        [Test]
        public void Build_IsDeterministic_SameSeedSamePlan()
        {
            var profile = MakeProfile();
            var level = Generate(3);
            var spawn = level.Entrance;

            var planA = CaveEnvironmentElementPlanner.Build(level, profile, WorldSeed, RunSeed, 3, spawn);
            var planB = CaveEnvironmentElementPlanner.Build(level, profile, WorldSeed, RunSeed, 3, spawn);

            Assert.AreEqual(planA.Placements.Count, planB.Placements.Count, "Contagem de elementos divergiu.");
            for (var i = 0; i < planA.Placements.Count; i++)
            {
                Assert.AreEqual(planA.Placements[i].ElementId, planB.Placements[i].ElementId);
                Assert.AreEqual(planA.Placements[i].GridPosition, planB.Placements[i].GridPosition);
                Assert.AreEqual(planA.Placements[i].Kind, planB.Placements[i].Kind);
            }

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Build_DoesNotBlock_EntranceToExitPath()
        {
            var profile = MakeProfile();
            var level = Generate(7);
            var spawn = level.Entrance;

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, WorldSeed, RunSeed, 7, spawn);

            // Remove os tiles de elementos BLOQUEANTES e mineráveis dos walkable e confirma que a saída
            // ainda é alcançável a partir da entrada (4-conn).
            var blocked = new HashSet<Vector2Int>(
                plan.Placements
                    .Where(p => p.Kind == CaveEnvironmentElementKind.DecorBlocking
                                || p.Kind == CaveEnvironmentElementKind.MineableNode)
                    .Select(p => p.GridPosition));

            var walkable = new HashSet<Vector2Int>(level.WalkableTiles);
            walkable.ExceptWith(blocked);

            Assert.IsTrue(PathExists(walkable, level.Entrance, level.Exit),
                "Elementos bloquearam o caminho entrance↔exit (softlock).");

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Build_AlwaysPlacesStoneAndOre()
        {
            var profile = MakeProfile();
            var level = Generate(2);
            var spawn = level.Entrance;

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, WorldSeed, RunSeed, 2, spawn);

            Assert.IsTrue(plan.Placements.Any(p => p.Kind == CaveEnvironmentElementKind.DecorNonBlocking),
                "Pedra (decor) deve estar presente em todas as bandas.");
            Assert.IsTrue(plan.Placements.Any(p => p.Kind == CaveEnvironmentElementKind.MineableNode),
                "Minério (mineável) deve estar presente em todas as bandas.");

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Build_PropagatesWaterFlag_FromProfile()
        {
            var watery = MakeProfile(band: 2, hasWater: true);
            var dry = MakeProfile(band: 0, hasWater: false);
            var level = Generate(4);
            var spawn = level.Entrance;

            var wateryPlan = CaveEnvironmentElementPlanner.Build(level, watery, WorldSeed, RunSeed, 4, spawn);
            var dryPlan = CaveEnvironmentElementPlanner.Build(level, dry, WorldSeed, RunSeed, 4, spawn);

            Assert.IsTrue(wateryPlan.HasWater);
            Assert.IsFalse(dryPlan.HasWater);

            Object.DestroyImmediate(watery);
            Object.DestroyImmediate(dry);
        }

        [Test]
        public void Build_OnlyPlacesElementsOnWalkableTiles()
        {
            var profile = MakeProfile();
            var level = Generate(9);
            var spawn = level.Entrance;

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, WorldSeed, RunSeed, 9, spawn);

            foreach (var placement in plan.Placements)
            {
                Assert.IsTrue(level.WalkableTiles.Contains(placement.GridPosition),
                    $"Elemento colocado em tile não-walkable: {placement.GridPosition}.");
                Assert.AreNotEqual(level.Entrance, placement.GridPosition, "Elemento na entrada.");
                Assert.AreNotEqual(level.Exit, placement.GridPosition, "Elemento na saída.");
            }

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Build_RespectsProfileDensity_ViaBalanceOverload()
        {
            var profile = MakeProfile();
            var balance = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
            var level = Generate(5);
            var spawn = level.Entrance;

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 5, spawn);

            // Sanidade: o plano não estoura o número de walkable tiles e tem ao menos os garantidos.
            Assert.LessOrEqual(plan.Placements.Count, level.WalkableTiles.Count);
            Assert.GreaterOrEqual(plan.Placements.Count, 2, "Ao menos pedra + minério garantidos.");

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }

        private static bool PathExists(HashSet<Vector2Int> walkable, Vector2Int start, Vector2Int goal)
        {
            if (!walkable.Contains(start) || !walkable.Contains(goal))
            {
                return false;
            }

            var visited = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == goal)
                {
                    return true;
                }

                foreach (var dir in dirs)
                {
                    var next = current + dir;
                    if (!visited.Contains(next) && walkable.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return false;
        }
    }
}
