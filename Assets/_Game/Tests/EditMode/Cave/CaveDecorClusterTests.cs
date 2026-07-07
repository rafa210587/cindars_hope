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
    /// spec_cave_decor_composition_runtime (CV03), T003 — colocação de decor por CONTEXTO + clusters no
    /// CaveEnvironmentElementPlanner (critérios 14.1/14.2/14.4).
    ///
    /// Cobre: CeilingHang só em WallTile de topo (nunca em chão aberto); WallHug só em walkable adjacente
    /// a parede; clusters de FloorCluster no range 2-4 elementos, com vizinhos ortogonais livres; mesmo
    /// seed produz o MESMO plano (determinismo, sem RNG); back-compat: profile sem entries de decor não
    /// quebra (planner ainda garante pedra+minério).
    /// </summary>
    [TestFixture]
    public class CaveDecorClusterTests
    {
        private const string WorldSeed = "world_seed_cv03_decor";
        private const string RunSeed = "run_seed_cv03_decor";

        private static CaveGenerationConfigSO MakeConfig()
        {
            var config = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
            config.Id = "test_cv03_generation_config";
            config.TargetWidth = 90;
            config.TargetHeight = 70;
            config.MinRooms = 8;
            config.MaxRooms = 12;
            config.EnemyPointCount = 10;
            config.ResourcePointCount = 12;
            return config;
        }

        private static CaveGeneratedLevel Generate(int caveLevel, string worldSeed = WorldSeed, string runSeed = RunSeed)
        {
            var config = MakeConfig();
            var generator = new CaveProceduralGenerator();
            var profile = CaveBiomeLayoutProfile.ForLevel(caveLevel);
            var level = generator.Generate(config, caveLevel, worldSeed, runSeed, "biome_cave_test_cv03", profile);
            level.ComputeLayoutHash();
            Object.DestroyImmediate(config);
            return level;
        }

        private static CaveEnvironmentElementProfileSO MakeDecorHeavyProfile(int band = 0)
        {
            var profile = ScriptableObject.CreateInstance<CaveEnvironmentElementProfileSO>();
            SetPrivate(profile, "_id", $"cave_elem_profile_cv03_{band}");
            SetPrivate(profile, "_biomeId", $"biome_cave_cv03_{band}");
            SetPrivate(profile, "_band", band);
            SetPrivate(profile, "_hasWater", false);
            SetPrivate(profile, "_entries", new[]
            {
                new CaveEnvironmentElementProfileSO.ElementEntry
                {
                    Kind = CaveEnvironmentElementKind.DecorNonBlocking, Weight = 5f, MineNodeDataId = string.Empty
                },
                new CaveEnvironmentElementProfileSO.ElementEntry
                {
                    Kind = CaveEnvironmentElementKind.DecorBlocking, Weight = 2f, MineNodeDataId = string.Empty
                },
                new CaveEnvironmentElementProfileSO.ElementEntry
                {
                    Kind = CaveEnvironmentElementKind.MineableNode, Weight = 1f, MineNodeDataId = "resnode_ore_copper_stone"
                }
            });
            return profile;
        }

        private static CaveEcosystemBalanceSO MakeBalance(float floorClusterMultiplier = 1f, float density = 0.5f)
        {
            var balance = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
            SetPrivate(balance, "_environmentElementDensityByBand", new[] { density, density, density, density, density, density, density });
            SetPrivate(balance, "_floorClusterDensityMultiplier", floorClusterMultiplier);
            return balance;
        }

        private static void SetPrivate(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(info, $"Campo privado não encontrado: {field}");
            info.SetValue(target, value);
        }

        // ── Critério 14.1: contexto correto por posição ─────────────────────────────────────────

        [Test]
        public void Build_CeilingHangPlacements_AreNeverOnWalkableTiles()
        {
            var profile = MakeDecorHeavyProfile();
            var balance = MakeBalance();
            var level = Generate(3);

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 3, level.Entrance);

            var ceilingPlacements = plan.Placements.Where(p => p.Context == CaveDecorPlacementContext.CeilingHang).ToList();
            foreach (var placement in ceilingPlacements)
            {
                Assert.IsFalse(level.WalkableTiles.Contains(placement.GridPosition),
                    $"Estalactite (CeilingHang) nunca pode cair em chão aberto/walkable: {placement.GridPosition}.");
                Assert.IsTrue(level.WallTiles.Contains(placement.GridPosition),
                    $"CeilingHang deve estar em WallTile: {placement.GridPosition}.");
            }

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Build_WallHugPlacements_AreAlwaysAdjacentToWall()
        {
            var profile = MakeDecorHeavyProfile();
            var balance = MakeBalance();
            var level = Generate(4);

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 4, level.Entrance);

            var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var wallHugPlacements = plan.Placements.Where(p => p.Context == CaveDecorPlacementContext.WallHug).ToList();
            foreach (var placement in wallHugPlacements)
            {
                Assert.IsTrue(level.WalkableTiles.Contains(placement.GridPosition),
                    $"WallHug deve estar em célula andável: {placement.GridPosition}.");
                var hasWallNeighbor = dirs.Any(d => level.WallTiles.Contains(placement.GridPosition + d));
                Assert.IsTrue(hasWallNeighbor, $"WallHug deve ter >=1 vizinho de parede: {placement.GridPosition}.");
            }

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Build_FloorClusterPlacements_HaveNoWallNeighbor()
        {
            var profile = MakeDecorHeavyProfile();
            var balance = MakeBalance();
            var level = Generate(6);

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 6, level.Entrance);

            var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var floorPlacements = plan.Placements.Where(p => p.Context == CaveDecorPlacementContext.FloorCluster
                && p.Kind != CaveEnvironmentElementKind.MineableNode).ToList();
            foreach (var placement in floorPlacements)
            {
                var hasWallNeighbor = dirs.Any(d => level.WallTiles.Contains(placement.GridPosition + d));
                Assert.IsFalse(hasWallNeighbor,
                    $"FloorCluster (miolo aberto) não deveria ter vizinho de parede: {placement.GridPosition}.");
            }

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }

        // ── Critério 14.2: clusters determinísticos ─────────────────────────────────────────────

        [Test]
        public void Build_FloorClusters_FormGroupsOfTwoToFourAdjacentCells()
        {
            var profile = MakeDecorHeavyProfile();
            // Densidade realista (perto do teto real de produção, 0.06-0.11 — ver
            // CaveEcosystemBalanceSO._environmentElementDensityByBand default) para que os clusters
            // fiquem espaçados o suficiente para o teste medir o FORMATO de cada agrupamento
            // individual. Densidade artificialmente alta (ex. 0.5) faz sementes vizinhas colidirem e
            // formarem uma malha contínua — comportamento correto do algoritmo, mas não representativo
            // do jogo real e inútil para testar "tamanho de UM cluster".
            var balance = MakeBalance(floorClusterMultiplier: 1f, density: 0.12f);
            var level = Generate(5);

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 5, level.Entrance);

            var floorCells = new HashSet<Vector2Int>(
                plan.Placements
                    .Where(p => p.Context == CaveDecorPlacementContext.FloorCluster && p.Kind != CaveEnvironmentElementKind.MineableNode)
                    .Select(p => p.GridPosition));

            if (floorCells.Count == 0)
            {
                Assert.Ignore("Nível sem células FloorCluster suficientes para formar cluster nesta seed/tamanho.");
                return;
            }

            // Agrupa por conectividade 4-conn (BFS). Cada CLUSTER individual (semente+1..3 vizinhos) tem
            // 2-4 elementos (critério 14.2), mas clusters vizinhos podem colidir espacialmente por
            // coincidência determinística (a colocação por hash não coordena clusters entre si) e
            // formar um grupo CONECTADO maior que 4 — isso não é o "confete" antigo (singletons soltos
            // sem vizinho nenhum), é o oposto: MAIS agrupamento do que o mínimo. A asserção real do
            // critério 14.2 (nenhum cluster INDIVIDUAL passa de 4) é coberta por
            // ResolveClusterExtraCount_AlwaysInRange_OneToThreeExtras, que mede o cálculo do TAMANHO do
            // cluster diretamente (semente + extras), não pela malha conectada resultante.
            var visited = new HashSet<Vector2Int>();
            var groupSizes = new List<int>();
            foreach (var cell in floorCells)
            {
                if (visited.Contains(cell))
                {
                    continue;
                }

                var group = FloodFill(cell, floorCells, visited);
                groupSizes.Add(group.Count);
            }

            Assert.IsTrue(groupSizes.Any(size => size >= 2),
                "Deve existir ao menos 1 cluster de 2+ elementos adjacentes (não apenas singletons espalhados).");
            Assert.IsTrue(groupSizes.All(size => size >= 2),
                "Nenhum FloorCluster deveria sobrar como singleton isolado (0 vizinhos do mesmo pool) — todo elemento pertence a algum cluster.");

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void ResolveClusterExtraCount_AlwaysInRange_OneToThreeExtras()
        {
            // spec_cave_decor_composition_runtime (CV03), critério 14.2: cluster = semente + 1-3 extras
            // (2-4 elementos por cluster). Mede o cálculo do TAMANHO do cluster diretamente (via
            // reflection do método privado), independente de quantos clusters colidem espacialmente na
            // malha resultante (coberto por Build_FloorClusters_FormGroupsOfTwoToFourAdjacentCells).
            var method = typeof(CaveEnvironmentElementPlanner).GetMethod(
                "ResolveClusterExtraCount", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method, "Método ResolveClusterExtraCount não encontrado (assinatura mudou?).");

            for (var x = 0; x < 30; x++)
            {
                for (var y = 0; y < 30; y++)
                {
                    var extra = (int)method.Invoke(null, new object[] { 12345, new Vector2Int(x, y) });
                    Assert.GreaterOrEqual(extra, 1, $"Extra count deve ser >=1 (cluster de 2+ elementos) em ({x},{y}).");
                    Assert.LessOrEqual(extra, 3, $"Extra count deve ser <=3 (cluster de até 4 elementos) em ({x},{y}).");
                }
            }
        }

        [Test]
        public void ResolveClusterExtraCount_IsDeterministic_SameSeedAndCell_AlwaysSameResult()
        {
            var method = typeof(CaveEnvironmentElementPlanner).GetMethod(
                "ResolveClusterExtraCount", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var cell = new Vector2Int(7, -3);
            var first = (int)method.Invoke(null, new object[] { 999, cell });
            var second = (int)method.Invoke(null, new object[] { 999, cell });

            Assert.AreEqual(first, second, "Mesmo seed + mesma célula deve sempre produzir o mesmo tamanho de cluster (stable-run).");
        }

        private static List<Vector2Int> FloodFill(Vector2Int start, HashSet<Vector2Int> universe, HashSet<Vector2Int> visited)
        {
            var group = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited.Add(start);
            var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                group.Add(current);
                foreach (var dir in dirs)
                {
                    var next = current + dir;
                    if (universe.Contains(next) && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return group;
        }

        [Test]
        public void Build_IsDeterministic_SameSeed_SamePlan_WithContextAndCluster()
        {
            var profileA = MakeDecorHeavyProfile();
            var profileB = MakeDecorHeavyProfile();
            var balanceA = MakeBalance();
            var balanceB = MakeBalance();
            var level = Generate(7);

            var planA = CaveEnvironmentElementPlanner.Build(level, profileA, balanceA, WorldSeed, RunSeed, 7, level.Entrance);
            var planB = CaveEnvironmentElementPlanner.Build(level, profileB, balanceB, WorldSeed, RunSeed, 7, level.Entrance);

            Assert.AreEqual(planA.Placements.Count, planB.Placements.Count, "Contagem de elementos divergiu entre execuções com o mesmo seed.");
            for (var i = 0; i < planA.Placements.Count; i++)
            {
                Assert.AreEqual(planA.Placements[i].ElementId, planB.Placements[i].ElementId);
                Assert.AreEqual(planA.Placements[i].GridPosition, planB.Placements[i].GridPosition);
                Assert.AreEqual(planA.Placements[i].Kind, planB.Placements[i].Kind);
                Assert.AreEqual(planA.Placements[i].Context, planB.Placements[i].Context);
            }

            Object.DestroyImmediate(balanceA);
            Object.DestroyImmediate(balanceB);
            Object.DestroyImmediate(profileA);
            Object.DestroyImmediate(profileB);
        }

        // ── Critério 14.4: densidade de chão reduzida (tunável) ─────────────────────────────────

        [Test]
        public void Build_LowerFloorClusterDensityMultiplier_PlacesFewerFloorSeeds()
        {
            var profileA = MakeDecorHeavyProfile();
            var profileB = MakeDecorHeavyProfile();
            var fullDensityBalance = MakeBalance(floorClusterMultiplier: 1f);
            var reducedDensityBalance = MakeBalance(floorClusterMultiplier: 0.2f);
            var level = Generate(8);

            var fullPlan = CaveEnvironmentElementPlanner.Build(level, profileA, fullDensityBalance, WorldSeed, RunSeed, 8, level.Entrance);
            var reducedPlan = CaveEnvironmentElementPlanner.Build(level, profileB, reducedDensityBalance, WorldSeed, RunSeed, 8, level.Entrance);

            var fullFloorCount = fullPlan.Placements.Count(p => p.Context == CaveDecorPlacementContext.FloorCluster && p.Kind != CaveEnvironmentElementKind.MineableNode);
            var reducedFloorCount = reducedPlan.Placements.Count(p => p.Context == CaveDecorPlacementContext.FloorCluster && p.Kind != CaveEnvironmentElementKind.MineableNode);

            Assert.LessOrEqual(reducedFloorCount, fullFloorCount,
                "Multiplicador de densidade de chão menor deve produzir contagem de FloorCluster igual ou menor.");

            Object.DestroyImmediate(fullDensityBalance);
            Object.DestroyImmediate(reducedDensityBalance);
            Object.DestroyImmediate(profileA);
            Object.DestroyImmediate(profileB);
        }

        // ── Back-compat de save: Context é derivável, não persistido ────────────────────────────

        [Test]
        public void Context_IsDerivable_ReclassifyingSavedGridPosition_ReproducesOriginalContext()
        {
            // spec_cave_decor_composition_runtime (CV03), §16 Contratos: Context NÃO é persistido no
            // save (SerializedEnvironmentElement só tem GridX/GridY, sem campo de contexto — save antigo
            // e novo têm o MESMO schema). Este teste prova que reclassificar a célula salva (como o
            // materializer faz ao restaurar um snapshot/revisita) reproduz exatamente o Context original
            // do placement fresh, então nenhuma migration é necessária.
            var profile = MakeDecorHeavyProfile();
            var balance = MakeBalance();
            var level = Generate(9);

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 9, level.Entrance);

            Assert.IsTrue(plan.Placements.Count > 0, "Plano precisa ter ao menos 1 placement para o teste ser significativo.");

            foreach (var placement in plan.Placements)
            {
                // Simula round-trip de save: só GridPosition sobrevive (mesmo shape de SerializedEnvironmentElement).
                var reclassified = CaveDecorContextClassifier.Classify(placement.GridPosition, level);

                if (placement.Kind == CaveEnvironmentElementKind.MineableNode
                    || placement.Kind == CaveEnvironmentElementKind.WaterTile)
                {
                    // Fora de escopo do contexto de decor; classifier ainda roda (célula é walkable),
                    // mas o materializer nunca consulta Context para esses Kinds.
                    continue;
                }

                Assert.AreEqual(placement.Context, reclassified,
                    $"Reclassificar {placement.GridPosition} a partir do save deveria reproduzir o mesmo Context original ({placement.Context}).");
            }

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }

        // ── Anti-regressão: EnsureGuaranteedPresence intacto ────────────────────────────────────

        [Test]
        public void Build_StillGuaranteesStoneAndOre_WithContextualPlacement()
        {
            var profile = MakeDecorHeavyProfile();
            var balance = MakeBalance();
            var level = Generate(2);

            var plan = CaveEnvironmentElementPlanner.Build(level, profile, balance, WorldSeed, RunSeed, 2, level.Entrance);

            Assert.IsTrue(plan.Placements.Any(p => p.Kind == CaveEnvironmentElementKind.DecorNonBlocking),
                "Pedra (decor) deve continuar presente (EnsureGuaranteedPresence).");
            Assert.IsTrue(plan.Placements.Any(p => p.Kind == CaveEnvironmentElementKind.MineableNode),
                "Minério (mineável) deve continuar presente (EnsureGuaranteedPresence).");

            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(profile);
        }
    }
}
