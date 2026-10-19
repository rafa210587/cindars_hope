using System;
using System.Collections.Generic;
using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using CindarsHope.World.Scale;
using UnityEngine;

namespace CindarsHope.Editor.Art
{
    public enum FarmDecorationBiome { Meadow, Forest, WaterEdge, CliffBase, Homestead, AnimalPen }

    public readonly struct FarmDecorationPlacement
    {
        public FarmDecorationPlacement(string spriteId, Vector2 position, FarmDecorationBiome biome)
        {
            SpriteId = spriteId;
            Position = position;
            Biome = biome;
        }

        public string SpriteId { get; }
        public Vector2 Position { get; }
        public FarmDecorationBiome Biome { get; }
    }

    /// <summary>Pure, editor-only visual clutter plan for the generated FarmScene.</summary>
    public static class FarmDecorationPlanner
    {
        public const int Seed = 20260814;
        // spec_farm_scene_keyart_visual_corrections_v1 Fase B: buffer em torno do polígono REAL
        // de água (Lake/River) — nenhuma árvore/decoração pode nascer a menos de 1u da margem.
        public const float WaterDecorationBuffer = 1f;
        public const int MeadowDensityPerThousand = 120;
        // spec_farm_scene_keyart_visual_corrections_v1: 260->120. O bosque ja tem 40 arvores FIXAS
        // (CreateTrees); a decoracao densa por cima virava um "cemiterio de tocos". Agora e acento esparso.
        public const int ForestDensityPerThousand = 120;
        // spec ad-hoc (FarmRichness wave 2026-08-17): 240->384 (~1.6x) para adensar a margem do
        // lago (juncos/vegetacao substitutos ate termos arte dedicada de junco).
        public const int WaterEdgeDensityPerThousand = 160;
        public const int CliffBaseDensityPerThousand = 110;
        public const int HomesteadDensityPerThousand = 180;
        public const int AnimalPenDensityPerThousand = 300;

        public const int MeadowMinimumCount = 12;
        public const int MeadowMaximumCount = 48;
        public const int ForestMinimumCount = 12;
        public const int ForestMaximumCount = 64;
        public const int WaterEdgeMinimumCount = 8;
        public const int WaterEdgeMaximumCount = 64;
        public const int CliffBaseMinimumCount = 3;
        public const int CliffBaseMaximumCount = 32;
        public const int HomesteadMinimumCount = 5;
        public const int HomesteadMaximumCount = 48;
        public const int AnimalPenMinimumCount = 2;
        public const int AnimalPenMaximumCount = 48;

        private static readonly Vector2[] PathApproaches =
        {
            new Vector2(FarmLevel1LayoutContract.WellX, FarmLevel1LayoutContract.WellY - 1f),
            new Vector2(FarmLevel1LayoutContract.SpawnFromCaveX, FarmLevel1LayoutContract.SpawnFromCaveY),
            new Vector2(FarmLevel1LayoutContract.DockEntranceX, FarmLevel1LayoutContract.DockEntranceY),
            new Vector2(-28f, 3.1f), new Vector2(15.3f, 9f), new Vector2(23.2f, 8.8f)
        };
        // These are the same polygons passed to CreateMvpFarmScene.PaintPath.  Keeping the
        // explicit mask here makes planning and validation independent of materialized Tilemaps.
        // Shared by terrain painting and decoration rejection; coordinates are world units.
        public static IReadOnlyList<Vector2[]> AuthoredPaths { get; } = new[]
        {
            Ribbon(FarmSceneCompositionContract.PrimaryPathWidth,
                new[] { 0.95f, 1.1f, 0.85f, 1f, 1.05f, 0.9f, 0.8f, 0.9f, 1.05f, 0.8f, 0.9f, 1f, 0.85f, 0.9f, 0.8f },
                new Vector2(-22.75f,20.1f), new Vector2(-20f,18.2f), new Vector2(-16f,17.1f),
                new Vector2(-12.8f,15.3f), new Vector2(-11.5f,11.7f), new Vector2(-8.4f,10.2f),
                new Vector2(-3f,10.35f), new Vector2(1.4f,10.55f), new Vector2(4.7f,10.35f),
                new Vector2(6.6f,9.2f), new Vector2(8.5f,7.8f), new Vector2(11.4f,8.5f),
                new Vector2(15.3f,9f), new Vector2(19.2f,8.6f), new Vector2(23f,8.8f)),
            Ribbon(FarmSceneCompositionContract.PathSpurWidth,
                new Vector2(-12.8f,15.3f), new Vector2(-9.2f,16.1f), new Vector2(-4.2f,15.5f),
                new Vector2(-0.2f,13.8f), new Vector2(1.5f,10.6f)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new Vector2(-8f,12f), new Vector2(-8.8f,8f), new Vector2(-9.2f,3f),
                new Vector2(-9.8f,-1f), new Vector2(-11.5f,-4.7f), new Vector2(-14f,-7.5f),
                new Vector2(-15.7f,-10.2f), new Vector2(-16.05f,-12.5f), new Vector2(-14.9f,-14.6f)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new[] { 0.85f, 0.8f, 1f, 0.85f, 0.95f, 1.05f, 0.8f, 0.95f, 0.8f, 1.1f },
                new Vector2(6.6f,9.2f), new Vector2(7.15f,7.05f), new Vector2(6.85f,4.4f),
                new Vector2(7.6f,2.15f), new Vector2(7.2f,-1f), new Vector2(6.9f,-4.35f),
                new Vector2(3.6f,-5.45f), new Vector2(-0.9f,-5.05f), new Vector2(-5.2f,-5.75f), new Vector2(-9.5f,-5.3f)),
            Ribbon(FarmSceneCompositionContract.PathSpurWidth,
                new Vector2(-9.3f,2.7f), new Vector2(-4f,2.45f), new Vector2(1.5f,2.55f), new Vector2(7.6f,2.15f)),
            Ribbon(FarmSceneCompositionContract.TownPathWidth,
                new[] { 1f, 0.85f, 0.8f, 0.95f, 0.85f, 1f, 1f, 1f },
                new Vector2(7.4f,3f), new Vector2(9.8f,3.5f), new Vector2(14.3f,3.75f),
                new Vector2(19f,3.55f), new Vector2(23f,3.4f), new Vector2(27f,3f),
                new Vector2(32f,3.1f), new Vector2(36f,3f)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new Vector2(-20f,18.2f), new Vector2(-24.5f,15.8f), new Vector2(-27.2f,12.1f),
                new Vector2(-28.9f,8.8f), new Vector2(-28f,3.1f)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new Vector2(-28f,1f), new Vector2(-27f,-1.2f), new Vector2(-24f,-2.8f),
                new Vector2(-20f,-3f), new Vector2(-16f,-4.7f), new Vector2(-11f,-6f)),
            Ribbon(FarmSceneCompositionContract.PathSpurWidth,
                new Vector2(-18f,-4f), new Vector2(-17.4f,-3f), new Vector2(-16.625f,-1.95f)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new Vector2(-18.5f,-4f), new Vector2(-17.5f,-6f), new Vector2(-18f,-8f),
                new Vector2(-19.25f,-8.625f)),
            Ribbon(FarmSceneCompositionContract.SouthFrontPathWidth,
                new[] { 1f, 0.85f, 0.75f, 1.05f, 0.85f, 0.95f, 1.1f, 0.8f, 0.85f, 0.875f, 0.9f },
                new Vector2(-14.9f,-14.6f), new Vector2(-12.3f,-14.75f), new Vector2(-9.5f,-15.05f),
                new Vector2(-5.8f,-15.65f), new Vector2(-1.8f,-15.45f), new Vector2(2.4f,-15.15f),
                new Vector2(5.6f,-14.45f), new Vector2(7.3f,-12.8f), new Vector2(9.1f,-8f),
                new Vector2(8.15f,-5.9f), new Vector2(6.9f,-4.35f)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new Vector2(9.1f,-8f), new Vector2(10.8f,-8.45f), new Vector2(12.2f,-7.8f),
                new Vector2(FarmLevel1LayoutContract.DockEntranceX,FarmLevel1LayoutContract.DockEntranceY)),
            Ribbon(FarmSceneCompositionContract.SecondaryPathWidth,
                new Vector2(14f,3f), new Vector2(14.1f,1f), new Vector2(16f,0.1f), new Vector2(21f,0f))
        };

        // Connected narrow ribbons: terrain painting and rejection consume identical polygons.
        private static Vector2[] Ribbon(float width, params Vector2[] centers)
        {
            var factors = new float[centers.Length];
            for (var i = 0; i < factors.Length; i++) factors[i] = 1f;
            return Ribbon(width, factors, centers);
        }

        private static Vector2[] Ribbon(float width, float[] widthFactors, params Vector2[] centers)
        {
            if (centers.Length < 2 || widthFactors.Length != centers.Length)
                throw new ArgumentException("A farm trail needs a positive width at each authored control.");
            var samples = new List<Vector2>();
            var widths = new List<float>();
            for (var span = 0; span < centers.Length - 1; span++)
            {
                var a = centers[Mathf.Max(0, span - 1)];
                var b = centers[span];
                var c = centers[span + 1];
                var d = centers[Mathf.Min(centers.Length - 1, span + 2)];
                var count = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(b, c) / 0.5f));
                for (var step = 0; step < count; step++)
                {
                    var t = (float)step / count;
                    samples.Add(0.5f * ((2f * b) + (-a + c) * t +
                        (2f * a - 5f * b + 4f * c - d) * t * t +
                        (-a + 3f * b - 3f * c + d) * t * t * t));
                    // Width variation is authored by location and eased between controls, never random noise.
                    widths.Add(width * Mathf.Lerp(widthFactors[span], widthFactors[span + 1], t * t * (3f - 2f * t)));
                }
            }
            samples.Add(centers[centers.Length - 1]);
            widths.Add(width * widthFactors[widthFactors.Length - 1]);
            var polygon = new Vector2[samples.Count * 2];
            for (var i = 0; i < samples.Count; i++)
            {
                var direction = samples[Mathf.Min(i + 1, samples.Count - 1)] - samples[Mathf.Max(0, i - 1)];
                var normal = new Vector2(-direction.y, direction.x).normalized * (widths[i] * 0.5f);
                polygon[i] = samples[i] + normal;
                polygon[polygon.Length - 1 - i] = samples[i] - normal;
            }
            return polygon;
        }
        private static readonly string[] MeadowSprites = { "foliage/flower_patch", "foliage/bush_leafy", "foliage/bush_berry" };
        // spec_farm_scene_keyart_visual_corrections_v1: removidas as ARVORES inteiras (tree_oak/pine) —
        // a decoracao duplicava a floresta fixa e entupia tudo. Agora so acento de CHAO (arbusto/flor/
        // cogumelo) + toco/log ESPARSOS (1/6 cada), pra ler como sub-bosque, nao cemiterio de tocos.
        private static readonly string[] ForestSprites = { "foliage/bush_leafy", "foliage/bush_berry", "foliage/flower_patch", "foliage/mushroom_cluster", "foliage/tree_stump", "foliage/log_fallen" };
        private static readonly string[] WaterEdgeSprites = { "foliage/flower_patch", "foliage/bush_leafy", "foliage/mushroom_cluster" };
        // spec_farm_scene_keyart_visual_corrections_v1: removido tree_stump — os tocos na faixa da
        // montanha caiam colados na boca da caverna. So minerio/pedra na base do paredao.
        private static readonly string[] CliffBaseSprites = { "props/rock_ore_0", "props/rock_ore_1", "props/rock_ore_2" };
        private static readonly string[] HomesteadSprites = { "foliage/flower_patch", "foliage/bush_leafy", "foliage/bush_berry" };
        private static readonly string[] AnimalPenSprites = { "foliage/flower_patch", "foliage/bush_leafy", "foliage/mushroom_cluster" };

        // FarmSceneSpatialContract is the project's static catalog, so it is consulted directly
        // instead of being passed as an impossible instance parameter.
        private static readonly Vector2[] ForestClusterCenters =
        {
            // Harvestable pockets remain inside the permanent edge. Keep the north clearing open.
            new Vector2(-31f, 16f), new Vector2(-31f, 10f), new Vector2(-31f, 1f),
            new Vector2(-25f, 12.5f), new Vector2(-23f, -21.8f), new Vector2(-15f, -23.2f),
            new Vector2(-7f, -23.5f), new Vector2(1f, -23.2f), new Vector2(12f, -22.8f),
            new Vector2(21f, -21.5f)
        };
        public static bool IsTreeCanopyClearOfCave(Vector2 foot)
        {
            // Reserve the southern sightline as well as the trunk corridor: mature canopies extend north.
            if (InRect(foot, FarmLevel1LayoutContract.CaveEntranceX - 4.5f, 14f,
                FarmLevel1LayoutContract.CaveEntranceX + 4.5f, 23f)) return false;
            var canopyCenter = foot + new Vector2(0f, 2.5f);
            var boardCenter = new Vector2(FarmLevel1LayoutContract.CaveBoardX, FarmLevel1LayoutContract.CaveBoardY + 0.8f);
            if ((canopyCenter - boardCenter).sqrMagnitude < 3.3f * 3.3f) return false;
            return DistancePointToSegment(canopyCenter, new Vector2(-22f, 17f),
                new Vector2(-16f, 14f)) > 3.3f;
        }

        public static bool IsTreeCanopyClearOfFountain(Vector2 foot, Vector2 canopySize)
        {
            var center = new Vector2(FarmLevel1LayoutContract.FonteAnchorX, FarmLevel1LayoutContract.FonteAnchorY + 2f);
            var closest = new Vector2(Mathf.Clamp(center.x, foot.x - canopySize.x * 0.5f, foot.x + canopySize.x * 0.5f),
                Mathf.Clamp(center.y, foot.y, foot.y + canopySize.y));
            return (closest - center).sqrMagnitude >= 4f * 4f;
        }

        public static IReadOnlyList<Vector2> PlanForestTreePositions(int count)
        {
            var positions = new List<Vector2>(count);
            for (var index = 0; index < count; index++)
            {
                const int attemptsPerCluster = 160;
                var placed = false;
                for (var attempt = 0; attempt < attemptsPerCluster * ForestClusterCenters.Length; attempt++)
                {
                    // Preserve the preferred pocket first; a new path may require a neighboring pocket.
                    var cluster = (index + attempt / attemptsPerCluster) % ForestClusterCenters.Length;
                    var center = ForestClusterCenters[cluster];
                    var angle = (index * 137.5f + attempt * 97.3f) * Mathf.Deg2Rad;
                    var radius = 1f + ((index * 17 + attempt * 31) % 47) / 47f * 3f;
                    var candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if (candidate.x < -32f || candidate.x > 29f || candidate.y < -23.8f || candidate.y > 19.5f) continue;
                    if (candidate.y > 10f && candidate.x > -24f) continue;
                    // A mature crown projects north of its trunk: reserve the south frontage in image space too.
                    if (candidate.x > -26f && candidate.x < 10f && candidate.y < -17f && candidate.y > -22.8f) continue;
                    if (!FarmEnclosedValleyBoundaryContract.ContainsClearing(candidate + Vector2.left * 0.65f) ||
                        !FarmEnclosedValleyBoundaryContract.ContainsClearing(candidate + Vector2.right * 0.65f) ||
                        !FarmEnclosedValleyBoundaryContract.ContainsClearing(candidate + Vector2.down * 0.65f)) continue;
                    if (IsForbiddenCell(candidate) || !IsTreeCanopyClearOfCave(candidate) ||
                        !IsTreeCanopyClearOfFountain(candidate, new Vector2(5.5f, 7f))) continue;
                    var separated = true;
                    for (var other = 0; other < positions.Count; other++)
                        if ((positions[other] - candidate).sqrMagnitude < 1.35f * 1.35f) { separated = false; break; }
                    if (!separated) continue;
                    positions.Add(candidate);
                    placed = true;
                    break;
                }
                if (!placed) throw new InvalidOperationException("Farm forest cluster cannot place stable tree ID " + index);
            }
            return positions;
        }

        public static IReadOnlyList<Vector2> PlanFarmTreePositions()
        {
            return PlanForestTreePositions(68);
        }

        public static IReadOnlyList<FarmDecorationPlacement> Plan()
        {
            var placements = new List<FarmDecorationPlacement>();
            // Sparse cluster seeds produce distinct patches and intervening quiet ground.
            for (var cellY = (int)FarmLevel1LayoutContract.MinY; cellY < (int)FarmLevel1LayoutContract.MaxY; cellY++)
                for (var cellX = (int)FarmLevel1LayoutContract.MinX; cellX < (int)FarmLevel1LayoutContract.MaxX; cellX++)
                {
                    var center = new Vector2(cellX + 0.5f, cellY + 0.5f);
                    if (IsForbiddenCell(center) || !TryGetBiome(center, out var biome)) continue;
                    var hash = CellHash(biome, cellX, cellY);
                    if (hash % 1000u >= (uint)(GetDensityPerThousand(biome) / 4)) continue;
                    var sprites = GetSprites(biome);
                    var patchCount = 3 + (int)(hash % 3u);
                    for (var patch = 0; patch < patchCount; patch++)
                    {
                        var angle = (hash % 360u + patch * 137.5f) * Mathf.Deg2Rad;
                        var radius = 0.12f + patch * 0.15f;
                        var position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                        if (IsForbiddenCell(position)) continue;
                        placements.Add(new FarmDecorationPlacement(sprites[(int)((hash + (uint)patch) % (uint)sprites.Length)], position, biome));
                    }
                }
            // Deliberate garden patches frame the west facade and the upper greenhouse garden.
            // They keep the same clearance as the general plan, including every doorway and path.
            var gardenCenters = new[] { new Vector2(8.4f, 8.2f), new Vector2(8.4f, 11.5f),
                new Vector2(14f, 14.7f), new Vector2(25.5f, 15.1f) };
            var gardenOffsets = new[] { new Vector2(-0.4f, 0.1f), new Vector2(0.15f, -0.25f),
                new Vector2(0.35f, 0.3f), new Vector2(-0.15f, 0.6f) };
            for (var garden = 0; garden < gardenCenters.Length; garden++)
                for (var accent = 0; accent < gardenOffsets.Length; accent++)
                {
                    var position = gardenCenters[garden] + gardenOffsets[accent];
                    if (IsForbiddenCell(position)) continue;
                    placements.Add(new FarmDecorationPlacement(HomesteadSprites[(garden + accent) % HomesteadSprites.Length],
                        position, FarmDecorationBiome.Homestead));
                }
            // The old southwest meadow is now the reserved pasture. Frame the open northern
            // clearing with low flower/leaf patches instead; never restore the removed tree wall.
            var meadowCenters = new[] { new Vector2(-13f, 16f), new Vector2(-3f, 14.8f),
                new Vector2(3f, 15.2f), new Vector2(7f, 14.8f) };
            for (var garden = 0; garden < meadowCenters.Length; garden++)
            for (var accent = 0; accent < gardenOffsets.Length; accent++)
            {
                var position = meadowCenters[garden] + gardenOffsets[accent];
                if (IsForbiddenCell(position) || !TryGetBiome(position, out var biome) ||
                    biome != FarmDecorationBiome.Meadow) continue;
                placements.Add(new FarmDecorationPlacement(MeadowSprites[(garden + accent) % MeadowSprites.Length],
                    position, FarmDecorationBiome.Meadow));
            }
            return LimitBiomeBudgets(placements);
        }

        private static IReadOnlyList<FarmDecorationPlacement> LimitBiomeBudgets(List<FarmDecorationPlacement> placements)
        {
            var biomeCount = (int)FarmDecorationBiome.AnimalPen + 1;
            var counts = new int[biomeCount];
            var visited = new int[biomeCount];
            foreach (var placement in placements) counts[(int)placement.Biome]++;
            var bounded = new List<FarmDecorationPlacement>(placements.Count);
            foreach (var placement in placements)
            {
                var biome = (int)placement.Biome;
                var rank = visited[biome]++;
                GetDeclaredRange(placement.Biome, out _, out var maximum);
                // Evenly thin the deterministic sequence rather than retaining only its southern
                // prefix. Includes authored groups in the same budget; no density/range inflation.
                if (counts[biome] > maximum &&
                    (rank + 1) * maximum / counts[biome] == rank * maximum / counts[biome]) continue;
                bounded.Add(placement);
            }
            return bounded;
        }
        public static bool IsForbiddenCell(Vector2 position)
        {
            if (position.x < FarmLevel1LayoutContract.MinX || position.x >= FarmLevel1LayoutContract.MaxX ||
                position.y < FarmLevel1LayoutContract.MinY || position.y >= FarmLevel1LayoutContract.MaxY) return true;

            if (position.y >= 21f || !FarmEnclosedValleyBoundaryContract.ContainsClearing(position)) return true;
            if (InRect(position, FarmLevel1LayoutContract.OrchardMinX - 0.5f, FarmLevel1LayoutContract.OrchardMinY - 0.5f,
                FarmLevel1LayoutContract.OrchardMinX + FarmLevel1LayoutContract.OrchardWidth + 0.5f,
                FarmLevel1LayoutContract.OrchardMinY + FarmLevel1LayoutContract.OrchardHeight + 0.5f)) return true;
            if (InRect(position, FarmLevel1LayoutContract.PastureMinX - 0.5f, FarmLevel1LayoutContract.PastureMinY - 0.5f,
                FarmLevel1LayoutContract.PastureMinX + FarmLevel1LayoutContract.PastureWidth + 0.5f,
                FarmLevel1LayoutContract.PastureMinY + FarmLevel1LayoutContract.PastureHeight + 0.5f)) return true;

            // Água (Lake/River) usa o POLÍGONO real + buffer (não os bounds retangulares) — ver
            // IsOverWater. As demais Uses continuam checadas pelo raster de navegação existente.
            if (IsOverWater(position)) return true;
            foreach (var body in FarmSettlementPhysicsContract.ServiceBodies)
                if (body.Bounds.Contains(position)) return true;
            foreach (var fence in FarmSettlementPhysicsContract.AllFenceSegments)
                if (fence.Bounds.Contains(position)) return true;

            // spec_farm_scene_keyart_visual_corrections_v1: clareira da Fonte da Anya (-24,4) fica
            // LIMPA de decoracao (tocos/foliage) num raio de 4u — senao a mata volta a cobrir a
            // fonte que acabou de virar landmark visivel (ver CreateTrees / CreateFonteAnya).
            var dxFonteClearing = position.x - (-28f);
            var dyFonteClearing = position.y - 3.1f;
            if ((dxFonteClearing * dxFonteClearing) + (dyFonteClearing * dyFonteClearing) <= 3.2f * 3.2f) return true;

            for (var i = 0; i < FarmSceneSpatialContract.All.Count; i++)
            {
                var footprint = FarmSceneSpatialContract.All[i];
                if (footprint.Use == FarmSpatialUse.Water) continue;
                if (footprint.Use == FarmSpatialUse.Solid ||
                    footprint.Use == FarmSpatialUse.Building || footprint.Use == FarmSpatialUse.CropField ||
                    footprint.Use == FarmSpatialUse.TriggerOnly || footprint.Id == FarmSceneSpatialContract.Bridge)
                {
                    if (FarmSceneNavigationRaster.Contains(footprint, position)) return true;
                }
            }

            return IsInsideHomesteadCompositionClearance(position) ||
                   IsPathCell(position) || IsInteractionApproach(position);
        }

        // Usado tanto pela máscara de decoração quanto pelo placement manual de árvore
        // (CreateTree em CreateMvpFarmScene) para nunca nascer sobre o polígono real de água.
        public static bool IsOverWater(Vector2 position)
        {
            for (var i = 0; i < FarmSceneSpatialContract.All.Count; i++)
            {
                var footprint = FarmSceneSpatialContract.All[i];
                if (footprint.Use != FarmSpatialUse.Water) continue;
                if (IsNearOrInsidePolygon(footprint.Polygon, position, WaterDecorationBuffer)) return true;
            }

            return false;
        }

        private static bool IsNearOrInsidePolygon(IReadOnlyList<Vector2> polygon, Vector2 point, float buffer)
        {
            if (ContainsPolygon(polygon, point)) return true;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (DistancePointToSegment(point, polygon[j], polygon[i]) <= buffer) return true;
            }

            return false;
        }

        private static float DistancePointToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var sqrLen = ab.sqrMagnitude;
            var t = sqrLen > 0.0001f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / sqrLen) : 0f;
            var closest = a + t * ab;
            return Vector2.Distance(point, closest);
        }

        public static bool TryGetBiome(Vector2 position, out FarmDecorationBiome biome)
        {
            if (IsWaterEdge(position)) { biome = FarmDecorationBiome.WaterEdge; return true; }
            if (InRect(position, -20f, -22f, 9f, -12f)) { biome = FarmDecorationBiome.AnimalPen; return true; }
            if (InRect(position, -18f, 11.5f, 8f, 16.5f)) { biome = FarmDecorationBiome.Meadow; return true; }
            if (InRect(position, -32f, -11f, -14f, 17f)) { biome = FarmDecorationBiome.Forest; return true; }
            if (InRect(position, -32f, 17f, 32f, 21f)) { biome = FarmDecorationBiome.CliffBase; return true; }
            if (InRect(position, 8f, 1f, 26f, 16f)) { biome = FarmDecorationBiome.Homestead; return true; }
            if (InRect(position, -32f, -20f, -14f, 2f)) { biome = FarmDecorationBiome.Meadow; return true; }
            foreach (var path in AuthoredPaths)
                if (IsNearOrInsidePolygon(path, position, 0.9f)) { biome = FarmDecorationBiome.Meadow; return true; }

            biome = default;
            return false;
        }

        public static bool IsCountInDeclaredRange(FarmDecorationBiome biome, int count)
        {
            GetDeclaredRange(biome, out var minimum, out var maximum);
            return count >= minimum && count <= maximum;
        }

        public static void GetDeclaredRange(FarmDecorationBiome biome, out int minimum, out int maximum)
        {
            switch (biome)
            {
                case FarmDecorationBiome.Meadow: minimum = MeadowMinimumCount; maximum = MeadowMaximumCount; return;
                case FarmDecorationBiome.Forest: minimum = ForestMinimumCount; maximum = ForestMaximumCount; return;
                case FarmDecorationBiome.WaterEdge: minimum = WaterEdgeMinimumCount; maximum = WaterEdgeMaximumCount; return;
                case FarmDecorationBiome.CliffBase: minimum = CliffBaseMinimumCount; maximum = CliffBaseMaximumCount; return;
                case FarmDecorationBiome.Homestead: minimum = HomesteadMinimumCount; maximum = HomesteadMaximumCount; return;
                case FarmDecorationBiome.AnimalPen: minimum = AnimalPenMinimumCount; maximum = AnimalPenMaximumCount; return;
                default: throw new ArgumentOutOfRangeException(nameof(biome), biome, null);
            }
        }

        private static bool IsWaterEdge(Vector2 point)
        {
            foreach (var footprint in FarmSceneSpatialContract.All)
                if (footprint.Use == FarmSpatialUse.Water && IsNearOrInsidePolygon(footprint.Polygon, point, 2.5f)) return true;
            return false;
        }

        private static bool IsPathCell(Vector2 point)
        {
            for (var i = 0; i < AuthoredPaths.Count; i++)
            {
                if (ContainsPolygon(AuthoredPaths[i], point)) return true;
            }

            return false;
        }

        private static bool IsInteractionApproach(Vector2 point)
        {
            for (var i = 0; i < PathApproaches.Length; i++)
            {
                if (InRect(point, PathApproaches[i].x - 1f, PathApproaches[i].y - 1f, PathApproaches[i].x + 1f, PathApproaches[i].y + 1f)) return true;
            }

            return false;
        }

        private static bool IsInsideHomesteadCompositionClearance(Vector2 point)
        {
            var clearance = FarmSceneCompositionContract.HomesteadDecorationClearance;
            return IsExpandedFootprint(point, FarmSceneSpatialContract.House, clearance) ||
                   IsExpandedFootprint(point, FarmSceneSpatialContract.Greenhouse, clearance) ||
                   IsExpandedFootprint(point, FarmSceneSpatialContract.CraftingYard, clearance);
        }

        private static bool IsExpandedFootprint(Vector2 point, string footprintId, float clearance)
        {
            if (!FarmSceneSpatialContract.TryGet(footprintId, out var footprint)) return false;
            var bounds = footprint.Bounds;
            return InRect(point, bounds.min.x - clearance, bounds.min.y - clearance,
                bounds.max.x + clearance, bounds.max.y + clearance);
        }

        private static bool InRect(Vector2 point, float minX, float minY, float maxX, float maxY) =>
            point.x >= minX && point.x < maxX && point.y >= minY && point.y < maxY;

        private static bool ContainsPolygon(IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.y > point.y) == (b.y > point.y)) continue;
                var intersectionX = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                if (point.x < intersectionX) inside = !inside;
            }

            return inside;
        }

        private static int GetDensityPerThousand(FarmDecorationBiome biome)
        {
            return biome switch
            {
                FarmDecorationBiome.Meadow => MeadowDensityPerThousand,
                FarmDecorationBiome.Forest => ForestDensityPerThousand,
                FarmDecorationBiome.WaterEdge => WaterEdgeDensityPerThousand,
                FarmDecorationBiome.CliffBase => CliffBaseDensityPerThousand,
                FarmDecorationBiome.Homestead => HomesteadDensityPerThousand,
                FarmDecorationBiome.AnimalPen => AnimalPenDensityPerThousand,
                _ => throw new ArgumentOutOfRangeException(nameof(biome), biome, null)
            };
        }

        private static string[] GetSprites(FarmDecorationBiome biome)
        {
            return biome switch
            {
                FarmDecorationBiome.Meadow => MeadowSprites,
                FarmDecorationBiome.Forest => ForestSprites,
                FarmDecorationBiome.WaterEdge => WaterEdgeSprites,
                FarmDecorationBiome.CliffBase => CliffBaseSprites,
                FarmDecorationBiome.Homestead => HomesteadSprites,
                FarmDecorationBiome.AnimalPen => AnimalPenSprites,
                _ => throw new ArgumentOutOfRangeException(nameof(biome), biome, null)
            };
        }

        private static uint CellHash(FarmDecorationBiome biome, int cellX, int cellY)
        {
            var hash = 2166136261u;
            hash = Mix(hash, (uint)Seed);
            hash = Mix(hash, (uint)biome);
            hash = Mix(hash, unchecked((uint)cellX));
            return Mix(hash, unchecked((uint)cellY));
        }

        private static uint Mix(uint hash, uint value)
        {
            hash ^= value;
            return hash * 16777619u;
        }
    }
}
