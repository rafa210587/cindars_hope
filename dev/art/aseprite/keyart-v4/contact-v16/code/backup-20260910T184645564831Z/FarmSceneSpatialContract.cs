using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    public enum FarmSpatialUse { Walkable, Solid, Water, Building, CropField, TriggerOnly, Spawn }

    /// <summary>Pure value description of one gameplay footprint in the FarmScene.</summary>
    public readonly struct FarmSceneFootprint
    {
        public FarmSceneFootprint(string id, FarmSpatialUse use, bool blocksMovement, bool blocksTilling,
            IReadOnlyList<Vector2> polygon)
        {
            Id = id;
            Use = use;
            BlocksMovement = blocksMovement;
            BlocksTilling = blocksTilling;
            Polygon = polygon;
        }

        public string Id { get; }
        public FarmSpatialUse Use { get; }
        public bool BlocksMovement { get; }
        public bool BlocksTilling { get; }
        public IReadOnlyList<Vector2> Polygon { get; }

        public Bounds Bounds
        {
            get
            {
                var min = Polygon[0];
                var max = min;
                for (var i = 1; i < Polygon.Count; i++)
                {
                    min = Vector2.Min(min, Polygon[i]);
                    max = Vector2.Max(max, Polygon[i]);
                }

                return new Bounds((min + max) * 0.5f, max - min);
            }
        }
    }

    /// <summary>Single source of truth for FarmScene gameplay footprints.</summary>
    public static class FarmSceneSpatialContract
    {
        public const string Lake = "farm_spatial_lake";
        public const string River = "farm_spatial_river";
        public const string Mountain = "farm_spatial_mountain";
        public const string Bridge = "farm_spatial_bridge";
        public const string House = "farm_spatial_house";
        public const string Greenhouse = "farm_spatial_greenhouse";
        public const string AnimalBuildings = "farm_spatial_animal_buildings";
        public const string CraftingYard = "farm_spatial_crafting_yard";
        public const string CaveMouth = "farm_spatial_cave_mouth";
        public const string TownExit = "farm_spatial_town_exit";
        public const string CropField = "farm_spatial_crop_field";

        private static readonly FarmSceneFootprint[] Footprints =
        {
            new FarmSceneFootprint(Mountain, FarmSpatialUse.Solid, true, true, NorthMountainPolygon()),
            // spec_farm_scene_keyart_visual_corrections_v1: lago puxado para DENTRO nas bordas leste
            // (x max 31->28) e sul (y min -20->-18), abrindo um anel andavel entre o lago e a parede
            // do mapa (x32/y-22) para o jogador "dar a volta no lago". Borda N/NO (foz do rio em
            // ~(21,-6)/(12,-7)) preservada para o rio continuar desaguando no lago.
            Polygon(Lake, FarmSpatialUse.Water, true, true,
                new Vector2(12.25f, -14f), new Vector2(11.56f, -8.75f), new Vector2(15.56f, -5.75f),
                new Vector2(23f, -3.94f), new Vector2(31f, -6f), new Vector2(34f, -10f),
                new Vector2(32f, -14.5f), new Vector2(26f, -16.2f), new Vector2(17f, -16.2f)),            Polygon(River, FarmSpatialUse.Water, true, true,
                new Vector2(27f, 22f), new Vector2(31f, 22f), new Vector2(31.5f, 17f),
                new Vector2(30.5f, 12f), new Vector2(30.7f, 8f), new Vector2(29f, 4f),
                new Vector2(29f, 2f), new Vector2(30.05f, 0.25f), new Vector2(31.15f, -1.75f),
                new Vector2(31.25f, -3.5f), new Vector2(30.85f, -5.5f), new Vector2(30.5f, -7f),
                new Vector2(27.1f, -7f), new Vector2(27.15f, -5.25f), new Vector2(27.45f, -3.6f),
                new Vector2(27.05f, -1.9f), new Vector2(26.1f, 0.2f), new Vector2(25f, 2f),
                new Vector2(25f, 4f), new Vector2(26.7f, 8f), new Vector2(26.5f, 12f),
                new Vector2(27.5f, 17f)),
            Rect(Bridge, FarmSpatialUse.Walkable, false, false, FarmLevel1LayoutContract.BridgeMinX, FarmLevel1LayoutContract.BridgeMinY, FarmLevel1LayoutContract.BridgeWidth, FarmLevel1LayoutContract.BridgeHeight),
            Rect(House, FarmSpatialUse.Building, true, true, FarmLevel1LayoutContract.HouseMinX, FarmLevel1LayoutContract.HouseMinY, FarmLevel1LayoutContract.HouseWidth, FarmLevel1LayoutContract.HouseHeight),
            // spec_farm_scene_keyart_visual_corrections_v1: estufa movida para o flanco OESTE da casa,
            // na faixa limpa entre o rio (~x17.5) e a casa (x24.5). Footprint x[19,24] y[5.75,10.25]
            // (literal, fora de FarmLevel1LayoutContract — arquivo fora do repo lock scope; os antigos
            // GreenhouseMinX/Y/Width/Height la ficam sem uso).
            Rect(Greenhouse, FarmSpatialUse.Building, true, true, FarmLevel1LayoutContract.GreenhouseMinX, FarmLevel1LayoutContract.GreenhouseMinY, FarmLevel1LayoutContract.GreenhouseWidth, FarmLevel1LayoutContract.GreenhouseHeight),
            Rect(AnimalBuildings, FarmSpatialUse.Building, true, true, FarmLevel1LayoutContract.AnimalBuildingsMinX, FarmLevel1LayoutContract.AnimalBuildingsMinY, FarmLevel1LayoutContract.AnimalBuildingsWidth, FarmLevel1LayoutContract.AnimalBuildingsHeight),
            Rect(CraftingYard, FarmSpatialUse.Building, true, true, FarmLevel1LayoutContract.CraftingYardMinX, FarmLevel1LayoutContract.CraftingYardMinY, FarmLevel1LayoutContract.CraftingYardWidth, FarmLevel1LayoutContract.CraftingYardHeight),
            Rect(CaveMouth, FarmSpatialUse.TriggerOnly, false, true, FarmLevel1LayoutContract.CaveMouthMinX, FarmLevel1LayoutContract.CaveMouthMinY, FarmLevel1LayoutContract.CaveMouthWidth, FarmLevel1LayoutContract.CaveMouthHeight),
            Rect(TownExit, FarmSpatialUse.TriggerOnly, false, false, FarmLevel1LayoutContract.TownExitMinX, FarmLevel1LayoutContract.TownExitMinY, FarmLevel1LayoutContract.TownExitWidth, FarmLevel1LayoutContract.TownExitHeight),
            Rect(CropField, FarmSpatialUse.CropField, false, false, FarmLevel1LayoutContract.CropFieldMinX, FarmLevel1LayoutContract.CropFieldMinY, FarmLevel1LayoutContract.CropFieldWidth, FarmLevel1LayoutContract.CropFieldHeight)
        };

        public static IReadOnlyList<FarmSceneFootprint> All => Footprints;

        // The bridge splits the river into two physical paths. Both paths are canonical scene
        // geometry, rather than a bounding-box approximation that could open water beside it.
        public static IReadOnlyList<Vector2> RiverAboveBridgeCollisionPath { get; } = new[]
        {
            new Vector2(27f, 22f), new Vector2(31f, 22f), new Vector2(31.5f, 17f),
            new Vector2(30.5f, 12f), new Vector2(30.7f, 8f), new Vector2(29f, 4f),
            new Vector2(25f, 4f), new Vector2(26.7f, 8f), new Vector2(26.5f, 12f), new Vector2(27.5f, 17f)
        };

        public static IReadOnlyList<Vector2> RiverBelowBridgeCollisionPath { get; } = new[]
        {
            // Keyart bend east of the bridge; the terminal cap overlaps the unchanged lake.
            new Vector2(25f, 2f), new Vector2(29f, 2f), new Vector2(30.05f, 0.25f),
            new Vector2(31.15f, -1.75f), new Vector2(31.25f, -3.5f), new Vector2(30.85f, -5.5f),
            new Vector2(30.5f, -7f), new Vector2(27.1f, -7f), new Vector2(27.15f, -5.25f),
            new Vector2(27.45f, -3.6f), new Vector2(27.05f, -1.9f), new Vector2(26.1f, 0.2f)
        };

        // The narrow notch opens only the deck and its north gangway, not the boat or adjacent water.
        public static IReadOnlyList<Vector2> LakeCollisionPath { get; } = new[]
        {
            new Vector2(12.25f, -14f), new Vector2(11.56f, -8.75f),
            // Intersect the deck's west side with the sloped shore before entering the notch.
            // Extending the former horizontal top would cross the lake boundary twice.
            new Vector2(11.91f, -8.4875f), new Vector2(11.91f, -10.798f),
            new Vector2(15.21f, -10.798f), new Vector2(15.21f, -7.355f),
            new Vector2(14.055f, -7.355f), new Vector2(14.055f, -6.87875f),
            new Vector2(15.56f, -5.75f), new Vector2(23f, -3.94f),
            new Vector2(31f, -6f), new Vector2(34f, -10f), new Vector2(32f, -14.5f),
            new Vector2(26f, -16.2f), new Vector2(17f, -16.2f)
        };
        public static bool TryGet(string id, out FarmSceneFootprint footprint)
        {
            for (var i = 0; i < Footprints.Length; i++)
            {
                if (string.Equals(Footprints[i].Id, id, StringComparison.Ordinal))
                {
                    footprint = Footprints[i];
                    return true;
                }
            }

            footprint = default;
            return false;
        }

        private static Vector2[] NorthMountainPolygon()
        {
            // The same measured escarpment profile drives painting, the solid boundary and masks.
            var boundary = FarmEnclosedValleyBoundaryContract.InnerBoundary;
            var points = new Vector2[13];
            for (var i = 0; i <= 10; i++) points[i] = boundary[14 + i];
            points[11] = new Vector2(36f, FarmLevel1LayoutContract.MaxY);
            points[12] = new Vector2(-36f, FarmLevel1LayoutContract.MaxY);
            return points;
        }

        private static FarmSceneFootprint Rect(string id, FarmSpatialUse use, bool blocksMovement, bool blocksTilling,
            float minX, float minY, float width, float height)
        {
            return new FarmSceneFootprint(id, use, blocksMovement, blocksTilling, new[]
            {
                new Vector2(minX, minY), new Vector2(minX, minY + height),
                new Vector2(minX + width, minY + height), new Vector2(minX + width, minY)
            });
        }

        private static FarmSceneFootprint Polygon(string id, FarmSpatialUse use, bool blocksMovement,
            bool blocksTilling, params Vector2[] points)
        {
            return new FarmSceneFootprint(id, use, blocksMovement, blocksTilling, points);
        }
    }
}
