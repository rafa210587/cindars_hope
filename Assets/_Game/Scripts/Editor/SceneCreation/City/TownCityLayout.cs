using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    public enum TownDoorSide
    {
        South,
        North,
        West,
        East
    }

    public enum TownBuildingArchetype
    {
        Civic,
        Temple,
        Market,
        Inn,
        Workshop,
        Bakery,
        Forge,
        Alchemy,
        Tannery,
        Watermill,
        Warehouse,
        Noble,
        Rural,
        Residential,
        Guard
    }

    public readonly struct TownRoadSegment
    {
        public TownRoadSegment(string id, Vector2 center, Vector2 size)
        {
            Id = id;
            Center = center;
            Size = size;
        }

        public string Id { get; }
        public Vector2 Center { get; }
        public Vector2 Size { get; }
        public float MinX => Center.x - Size.x * 0.5f;
        public float MaxX => Center.x + Size.x * 0.5f;
        public float MinY => Center.y - Size.y * 0.5f;
        public float MaxY => Center.y + Size.y * 0.5f;

        public bool Contains(Vector2 point, float tolerance = 0f) =>
            point.x >= MinX - tolerance && point.x <= MaxX + tolerance &&
            point.y >= MinY - tolerance && point.y <= MaxY + tolerance;
    }

    public readonly struct TownBuildingLot
    {
        public TownBuildingLot(
            string name,
            string districtId,
            Vector2 center,
            Vector2 size,
            TownDoorSide doorSide,
            TownBuildingArchetype archetype,
            Color color)
        {
            Name = name;
            DistrictId = districtId;
            Center = center;
            Size = size;
            DoorSide = doorSide;
            Archetype = archetype;
            Color = color;
        }

        public string Name { get; }
        public string DistrictId { get; }
        public Vector2 Center { get; }
        public Vector2 Size { get; }
        public TownDoorSide DoorSide { get; }
        public TownBuildingArchetype Archetype { get; }
        public Color Color { get; }
        public float MinX => Center.x - Size.x * 0.5f;
        public float MaxX => Center.x + Size.x * 0.5f;
        public float MinY => Center.y - Size.y * 0.5f;
        public float MaxY => Center.y + Size.y * 0.5f;

        public Vector3 DoorPosition
        {
            get
            {
                switch (DoorSide)
                {
                    case TownDoorSide.North: return new Vector3(Center.x, MaxY, 0f);
                    case TownDoorSide.West: return new Vector3(MinX, Center.y, 0f);
                    case TownDoorSide.East: return new Vector3(MaxX, Center.y, 0f);
                    default: return new Vector3(Center.x, MinY, 0f);
                }
            }
        }

        public Vector3 DoorApproach
        {
            get
            {
                var door = DoorPosition;
                switch (DoorSide)
                {
                    case TownDoorSide.North: return door + new Vector3(0f, 1.3f, 0f);
                    case TownDoorSide.West: return door + new Vector3(-1.3f, 0f, 0f);
                    case TownDoorSide.East: return door + new Vector3(1.3f, 0f, 0f);
                    default: return door + new Vector3(0f, -1.3f, 0f);
                }
            }
        }

        public bool Overlaps(TownBuildingLot other, float margin = 0f) =>
            Mathf.Abs(Center.x - other.Center.x) < (Size.x + other.Size.x) * 0.5f + margin &&
            Mathf.Abs(Center.y - other.Center.y) < (Size.y + other.Size.y) * 0.5f + margin;

        public bool Overlaps(TownRoadSegment road, float margin = 0f) =>
            Mathf.Abs(Center.x - road.Center.x) < (Size.x + road.Size.x) * 0.5f + margin &&
            Mathf.Abs(Center.y - road.Center.y) < (Size.y + road.Size.y) * 0.5f + margin;
    }

    public readonly struct TownNpcPlace
    {
        public TownNpcPlace(string npcId, string role, string buildingName, Vector2 work, Vector2 social)
        {
            NpcId = npcId;
            Role = role;
            BuildingName = buildingName;
            Work = work;
            Social = social;
        }

        public string NpcId { get; }
        public string Role { get; }
        public string BuildingName { get; }
        public Vector2 Work { get; }
        public Vector2 Social { get; }
    }

    /// <summary>
    /// Preservation-first source of truth for TownScene lots, roads and NPC destinations.
    /// Stable object/NPC IDs are intentionally kept identical to the materialized baseline.
    /// </summary>
    public static class TownCityLayout
    {
        public static readonly Vector2 StandardHouseExteriorSize = new Vector2(8f, 7f);
        public static readonly Vector2 StandardHouseInteriorSize = new Vector2(6f, 5f);
        public static readonly Vector2 CentralPlazaCenter = new Vector2(0f, 4f);
        public static readonly Vector2 CentralPlazaSize = new Vector2(26f, 26f);
        public const int BaselineHouseCount = 24;
        public const int BaselineNpcStallCount = 23;
        public const int BaselineMarketStallCount = 6;
        public const int BaselineTreeCount = 497;

        private static readonly TownRoadSegment[] Roads =
        {
            new TownRoadSegment("road_main_ew", new Vector2(0f, 5.5f), new Vector2(112f, 7f)),
            new TownRoadSegment("road_main_ns", new Vector2(0f, -7f), new Vector2(7f, 72f)),
            new TownRoadSegment("road_civic_frontage", new Vector2(0f, 27f), new Vector2(112f, 3f)),
            new TownRoadSegment("road_registry_frontage", new Vector2(-21f, 18.25f), new Vector2(4f, 13.5f)),
            new TownRoadSegment("road_east_lower", new Vector2(42f, -10.4f), new Vector2(28f, 4f)),
            new TownRoadSegment("road_lake_mill", new Vector2(-18f, -17.4f), new Vector2(36f, 4f)),
            new TownRoadSegment("road_plaza_southwest", new Vector2(-8.5f, -4.2f), new Vector2(17f, 4f)),
            new TownRoadSegment("road_south_residences", new Vector2(0f, -30.5f), new Vector2(82f, 4f)),
            new TownRoadSegment("road_warehouse_gate", new Vector2(-40.25f, -36f), new Vector2(4f, 13f)),
        };

        private static readonly TownBuildingLot[] Buildings =
        {
            Lot("House_Temple", TownDistrictLayout.DistrictTempleNorth, -50f, 35f, 16f, 13f, TownDoorSide.South, TownBuildingArchetype.Temple, 0.72f, 0.76f, 0.82f),
            Lot("House_Manor", TownDistrictLayout.DistrictTempleNorth, -22f, 35f, 16f, 13f, TownDoorSide.South, TownBuildingArchetype.Noble, 0.42f, 0.52f, 0.66f),
            Lot("House_Chamber", TownDistrictLayout.DistrictTempleNorth, 12f, 34f, 16f, 11f, TownDoorSide.South, TownBuildingArchetype.Civic, 0.28f, 0.29f, 0.32f),
            Lot("House_Archive", TownDistrictLayout.DistrictTempleNorth, 42f, 35f, 17f, 13f, TownDoorSide.South, TownBuildingArchetype.Noble, 0.43f, 0.30f, 0.58f),
            Lot("House_Registry", TownDistrictLayout.DistrictCentralPlaza, -13f, 21f, 12f, 9f, TownDoorSide.West, TownBuildingArchetype.Civic, 0.55f, 0.58f, 0.66f),
            Lot("House_Prison", TownDistrictLayout.DistrictResidentialEast, 55f, 32f, 8f, 6f, TownDoorSide.South, TownBuildingArchetype.Guard, 0.30f, 0.30f, 0.34f),
            Lot("House_MarketHall", TownDistrictLayout.DistrictMarketWest, -48f, 15f, 18f, 12f, TownDoorSide.South, TownBuildingArchetype.Market, 0.60f, 0.50f, 0.34f),
            Lot("House_Bakery", TownDistrictLayout.DistrictMarketWest, -29f, 15f, 12f, 10f, TownDoorSide.South, TownBuildingArchetype.Bakery, 0.78f, 0.58f, 0.35f),
            Lot("House_Inn", TownDistrictLayout.DistrictMarketWest, -48f, -3.5f, 16f, 11f, TownDoorSide.North, TownBuildingArchetype.Inn, 0.62f, 0.50f, 0.36f),
            Lot("House_Residential_1", TownDistrictLayout.DistrictMarketWest, -29f, -2.5f, 12f, 9f, TownDoorSide.North, TownBuildingArchetype.Rural, 0.64f, 0.54f, 0.40f),
            Lot("House_Residential_2", TownDistrictLayout.DistrictMarketWest, -17f, -10.8f, 10f, 9f, TownDoorSide.North, TownBuildingArchetype.Market, 0.58f, 0.48f, 0.34f),
            Lot("House_Blacksmith", TownDistrictLayout.DistrictResidentialEast, 27f, 15f, 13f, 10f, TownDoorSide.South, TownBuildingArchetype.Forge, 0.34f, 0.30f, 0.28f),
            Lot("House_AlchemyLab", TownDistrictLayout.DistrictResidentialEast, 47f, 15f, 13f, 10f, TownDoorSide.South, TownBuildingArchetype.Alchemy, 0.30f, 0.52f, 0.56f),
            Lot("House_Workshop", TownDistrictLayout.DistrictResidentialEast, 27f, -3f, 14f, 10f, TownDoorSide.North, TownBuildingArchetype.Workshop, 0.56f, 0.46f, 0.30f),
            Lot("House_Tannery", TownDistrictLayout.DistrictResidentialEast, 47f, -3f, 14f, 10f, TownDoorSide.North, TownBuildingArchetype.Tannery, 0.58f, 0.44f, 0.30f),
            Lot("House_Residential_3", TownDistrictLayout.DistrictResidentialEast, 37f, -17f, 12f, 9f, TownDoorSide.North, TownBuildingArchetype.Workshop, 0.45f, 0.56f, 0.48f),
            Lot("House_Fishery", TownDistrictLayout.DistrictLakeParkSouthwest, -30f, -24f, 12f, 9f, TownDoorSide.North, TownBuildingArchetype.Watermill, 0.42f, 0.55f, 0.64f),
            Lot("House_AnimalYard", TownDistrictLayout.DistrictCorralSouth, -51f, -36f, 17f, 12f, TownDoorSide.East, TownBuildingArchetype.Warehouse, 0.44f, 0.32f, 0.20f),
            Lot("House_CarvalhoTorto", TownDistrictLayout.DistrictCorralSouth, -34f, -36f, 8f, 7f, TownDoorSide.North, TownBuildingArchetype.Residential, 0.50f, 0.46f, 0.34f),
            Lot("House_Dagna", TownDistrictLayout.DistrictCorralSouth, -21f, -36f, 8f, 7f, TownDoorSide.North, TownBuildingArchetype.Residential, 0.52f, 0.44f, 0.40f),
            Lot("House_GateKeeper", TownDistrictLayout.DistrictCorralSouth, -10f, -36f, 8f, 7f, TownDoorSide.North, TownBuildingArchetype.Guard, 0.42f, 0.46f, 0.56f),
            Lot("House_Pip", TownDistrictLayout.DistrictCorralSouth, 10f, -36f, 8f, 7f, TownDoorSide.North, TownBuildingArchetype.Residential, 0.60f, 0.54f, 0.42f),
            Lot("House_Residential_4", TownDistrictLayout.DistrictCorralSouth, 22f, -36f, 8f, 7f, TownDoorSide.North, TownBuildingArchetype.Residential, 0.62f, 0.51f, 0.35f),
            Lot("House_Tovin", TownDistrictLayout.DistrictCorralSouth, 34f, -36f, 8f, 7f, TownDoorSide.North, TownBuildingArchetype.Residential, 0.58f, 0.56f, 0.62f),
        };

        private static readonly TownNpcPlace[] NpcPlaces =
        {
            Npc("npc_corvus", "sacerdote de Kanthor", "House_Temple", -50f, 27.7f, -46f, 27f),
            Npc("npc_mara", "registradora", "House_Registry", -20.3f, 21f, -5f, 8f),
            Npc("npc_tovin", "escrivão e licenças", "House_Tovin", 34f, -30.2f, 5f, 8f),
            Npc("npc_sylveth", "sementes e horticultura", "House_Residential_1", -29f, 3.8f, -20f, 6.5f),
            Npc("npc_renko", "loja geral", "House_Residential_2", -17f, -5f, -12f, 6.5f),
            Npc("npc_mirela", "alfaiataria", "House_Residential_3", 37f, -11.2f, 28f, -9.2f),
            Npc("npc_orlan", "estalagem", "House_Inn", -50f, 3.3f, -12f, 4f),
            Npc("npc_gruta", "taverna e cozinha", "House_Inn", -46f, 3.3f, -10f, 4f),
            Npc("npc_brumdar", "ferreiro", "House_Blacksmith", 27f, 8.7f, 20f, 6.5f),
            Npc("npc_dagna", "mineração e estrada da pedreira", "House_Dagna", -21f, -30.2f, -12f, -28f),
            Npc("npc_hund", "guarda e construção", "House_CarvalhoTorto", -34f, -30.2f, -6f, 5f),
            Npc("npc_thalindra", "arquivo e pesquisa", "House_Archive", 42f, 28.2f, 32f, 27f),
            Npc("npc_alaric", "capitão da guarda", "House_GateKeeper", -10f, -30.2f, 0f, -40f),
            Npc("npc_pip", "mensageiro", "House_Pip", 10f, -30.2f, 3f, 2f),
            Npc("npc_nimble", "carpintaria", "House_Workshop", 27f, 3.3f, 20f, 4f),
            Npc("npc_gurd", "construção pesada", "House_CarvalhoTorto", -36f, -31.2f, -28f, -28f),
            Npc("npc_yael", "mercado noturno", null, 54f, -16f, 50f, -13f),
            Npc("npc_maelor", "rota noturna e cemitério", null, -36f, 32f, -8f, 4f),
            Npc("npc_zrix", "estrada da caverna", null, 56f, -4f, 48f, 6.5f),
            Npc("npc_savra", "ervas e floresta", "House_Residential_4", 22f, -30.2f, 45f, -18f),
            Npc("npc_ozzra", "alquimia", "House_AlchemyLab", 47f, 8.7f, 38f, 6.5f),
            Npc("npc_eiran", "animais e armazém", "House_AnimalYard", -41.2f, -36f, -34f, -28f),
            Npc("npc_liora", "música e jardim das estátuas", null, 3f, 3f, 3f, 3f),
            Npc("npc_velorin", "liderança e conselho", "House_Chamber", 12f, 27.2f, 5f, 8f),
            Npc("npc_sael", "pesca, moinho e cais", "House_Fishery", -30f, -18.2f, -43f, -21f),
            Npc("npc_mella", "padaria e moagem", "House_Bakery", -29f, 8.7f, -22f, 6.5f),
            Npc("npc_hess", "curtume", "House_Tannery", 47f, 3.3f, 39f, 4f),
            Npc("npc_tibbet", "cemitério e sacristia", null, -33f, 32f, -46f, 27f),
        };

        public static IReadOnlyList<TownRoadSegment> AllRoads => Roads;
        public static IReadOnlyList<TownBuildingLot> AllBuildings => Buildings;
        public static IReadOnlyList<TownNpcPlace> AllNpcPlaces => NpcPlaces;

        public static bool TryGetBuilding(string name, out TownBuildingLot building)
        {
            for (var i = 0; i < Buildings.Length; i++)
            {
                if (Buildings[i].Name == name)
                {
                    building = Buildings[i];
                    return true;
                }
            }

            building = default;
            return false;
        }

        public static bool TryGetNpcPlace(string npcId, out TownNpcPlace place)
        {
            for (var i = 0; i < NpcPlaces.Length; i++)
            {
                if (NpcPlaces[i].NpcId == npcId)
                {
                    place = NpcPlaces[i];
                    return true;
                }
            }

            place = default;
            return false;
        }

        public static Vector3 ResolveNpcWorkPosition(string npcId, Vector3 fallback)
        {
            return TryGetNpcPlace(npcId, out var place)
                ? new Vector3(place.Work.x, place.Work.y, fallback.z)
                : fallback;
        }

        public static Vector3 ResolveNpcSocialPosition(string npcId, Vector3 fallback)
        {
            return TryGetNpcPlace(npcId, out var place)
                ? new Vector3(place.Social.x, place.Social.y, fallback.z)
                : fallback;
        }

        public static Vector3 ResolveNpcStallPosition(string npcId, Vector3 fallback)
        {
            if (!TryGetNpcPlace(npcId, out var place))
            {
                return fallback;
            }

            if (string.IsNullOrWhiteSpace(place.BuildingName) || !TryGetBuilding(place.BuildingName, out var building))
            {
                return new Vector3(place.Work.x, place.Work.y + 1.15f, fallback.z);
            }

            float direction = 1f;
            if (npcId == "npc_gruta" || npcId == "npc_hund")
            {
                direction = -1f;
            }

            var tangent = building.DoorSide == TownDoorSide.North || building.DoorSide == TownDoorSide.South
                ? new Vector2(direction * 4f, 0f)
                : new Vector2(0f, direction * 4f);
            return building.DoorApproach + new Vector3(tangent.x, tangent.y, fallback.z);
        }

        public static bool IsPointOnRoad(Vector3 point, float tolerance = 0.35f)
        {
            var p = new Vector2(point.x, point.y);
            for (var i = 0; i < Roads.Length; i++)
            {
                if (Roads[i].Contains(p, tolerance))
                {
                    return true;
                }
            }

            return false;
        }

        private static TownBuildingLot Lot(
            string name, string districtId, float x, float y, float width, float height,
            TownDoorSide side, TownBuildingArchetype archetype, float r, float g, float b) =>
            new TownBuildingLot(name, districtId, new Vector2(x, y), new Vector2(width, height), side, archetype, new Color(r, g, b));

        private static TownNpcPlace Npc(
            string npcId, string role, string buildingName, float workX, float workY, float socialX, float socialY) =>
            new TownNpcPlace(npcId, role, buildingName, new Vector2(workX, workY), new Vector2(socialX, socialY));
    }
}
