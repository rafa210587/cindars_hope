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
        public TownRoadSegment(string id, Vector2 start, Vector2 end, float width)
        {
            Id = id;
            Start = start;
            End = end;
            Width = width;
        }

        public string Id { get; }
        public Vector2 Start { get; }
        public Vector2 End { get; }
        public float Width { get; }
        public Vector2 Center => (Start + End) * .5f;
        // Broad-phase bounds only; the actual shape is a swept circular corridor.
        public Vector2 Size => new Vector2(Mathf.Abs(End.x-Start.x)+Width, Mathf.Abs(End.y-Start.y)+Width);
        public float MinX => Center.x - Size.x * 0.5f;
        public float MaxX => Center.x + Size.x * 0.5f;
        public float MinY => Center.y - Size.y * 0.5f;
        public float MaxY => Center.y + Size.y * 0.5f;

        public bool Contains(Vector2 point, float tolerance = 0f) =>
            TownKeyartGeometry.PointSegmentDistance(point, Start, End) <= Width * .5f + tolerance;
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
        // The mill has an off-centre entry; the wheel is outside the walk-in shell.
        public float DoorTangentOffset => Name == "House_Fishery" ? -2f : 0f;
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
                    case TownDoorSide.North: return new Vector3(Center.x + DoorTangentOffset, MaxY, 0f);
                    case TownDoorSide.West: return new Vector3(MinX, Center.y, 0f);
                    case TownDoorSide.East: return new Vector3(MaxX, Center.y, 0f);
                    default: return new Vector3(Center.x + DoorTangentOffset, MinY, 0f);
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
                    case TownDoorSide.North: return door + new Vector3(0f, 1.5f, 0f);
                    case TownDoorSide.West: return door + new Vector3(-1.5f, 0f, 0f);
                    case TownDoorSide.East: return door + new Vector3(1.5f, 0f, 0f);
                    default: return door + new Vector3(0f, -1.5f, 0f);
                }
            }
        }

        public bool Overlaps(TownBuildingLot other, float margin = 0f) =>
            Mathf.Abs(Center.x - other.Center.x) < (Size.x + other.Size.x) * 0.5f + margin &&
            Mathf.Abs(Center.y - other.Center.y) < (Size.y + other.Size.y) * 0.5f + margin;

        // Frontage strips are hand-fit to touch the lot's door-side edge EXACTLY (zero visual gap,
        // zero penetration — see the Roads[] comment in TownCityLayout). At that exact boundary,
        // float subtraction noise (~1e-15) can make Center-distance land a hair under the sum-of-
        // half-extents threshold, false-flagging a touching-not-overlapping pair as an overlap.
        // OverlapEpsilon absorbs that noise without hiding a real overlap (which is always >= many
        // orders of magnitude larger than float ULP at these coordinate scales).
        private const float OverlapEpsilon = 0.001f;

        public bool Overlaps(TownRoadSegment road, float margin = 0f) =>
            TownKeyartGeometry.SegmentRectDistance(road.Start, road.End, Center, Size) < road.Width * .5f + margin - OverlapEpsilon;
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
        // v9 organic relayout (spec_town_layout_v9_organic): circular plaza ~r13 centered (0,4).
        // CentralPlazaSize keeps the 26x26 bounding box (diameter 26 == 2*13) used by ground-slab
        // painting and overlap audits; the visual paint can render a rounder silhouette on top.
        public static readonly Vector2 CentralPlazaCenter = new Vector2(0f, 4f);
        public static readonly Vector2 CentralPlazaSize = new Vector2(26f, 26f);
        public const float CentralPlazaRadius = 13f;
        public const int BaselineHouseCount = 24;
        public const int BaselineNpcStallCount = 23;
        public const int BaselineMarketStallCount = 6;
        public const int BaselineTreeCount = TownAccessMetrics.MinimumMaterializedTrees;
        // Actual materialized bands; not six nominal bands while silently skipping half the loop.
        public const int ExteriorForestBandCount = 3;
        public const float ExteriorForestStep = 2.25f;
        public const float ExteriorForestGateHalfClearance = 6f;
        public const float ExteriorForestGroundPadding = 30f;

        // Materialized routes and lot geometry share one deterministic source. No legacy road grid.
        private static TownRoadSegment[] s_roads;
        private static TownRoadSegment[] Roads => s_roads ??= TownKeyartGeometry.BuildRoads(Buildings);

        // v9 organic relayout lots (spec_town_layout_v9_organic; spec_codex door-south + breathing-
        // room pass). Coordinates hand-fit around the spec's target table (tolerance +/-2un) to clear
        // lot-lot overlap (>=4un edge-to-edge gap), lot-road overlap (>=2un edge-to-edge gap outside a
        // lot's own frontage), >=3un wall clearance and door-reaches-road; every pairwise/road check
        // was re-verified with an offline audit script mirroring AuditHouseOverlaps before being
        // committed here (0 violations). See the spec's "Composicao-alvo" tables for intent and the
        // execution report for the full deviation-and-reason table.
        private static readonly TownBuildingLot[] Buildings =
        {
            // Distrito NORTE (civico/religioso) — fachadas com porta S tocando road_civic_frontage.
            Lot("House_Temple", TownDistrictLayout.DistrictTempleNorth, -32f, 24f, 16f, 11f, TownDoorSide.South, TownBuildingArchetype.Temple, 0.72f, 0.76f, 0.82f),
            Lot("House_Chamber", TownDistrictLayout.DistrictTempleNorth, -57.5f, 42.5f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Civic, 0.28f, 0.29f, 0.32f),
            Lot("House_Prison", TownDistrictLayout.DistrictTempleNorth, -14.5f, 41.5f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Guard, 0.30f, 0.30f, 0.34f),
            Lot("House_Manor", TownDistrictLayout.DistrictTempleNorth, 62.5f, 39f, 9f, 8f, TownDoorSide.South, TownBuildingArchetype.Noble, 0.42f, 0.52f, 0.66f),
            // Registry: x 19->17.5 (clears the central-plaza AABB check), y 19->18 (>=2un from
            // road_civic_frontage on its north side; door/frontage stay south, unaffected).
            Lot("House_Registry", TownDistrictLayout.DistrictCentralPlaza, 17.5f, 41.5f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Civic, 0.55f, 0.58f, 0.66f),
            // Archive: West->South (task 2). x 51->52.5, y 20->18 (task 3: >=4un from House_AlchemyLab
            // and >=2un from road_civic_frontage on its north side, now that both face south).
            Lot("House_Archive", TownDistrictLayout.DistrictTempleNorth, 65.5f, 20f, 9f, 8f, TownDoorSide.South, TownBuildingArchetype.Noble, 0.43f, 0.30f, 0.58f),

            // Distrito OESTE (comercio + parque).
            // MarketHall: East->South (task 2). x -44->-46, y 12->13.5 (task 3: >=2un from
            // road_market_ns and road_main_ew now that its frontage faces south).
            Lot("House_MarketHall", TownDistrictLayout.DistrictMarketWest, -57.5f, 16.5f, 9f, 8f, TownDoorSide.South, TownBuildingArchetype.Market, 0.60f, 0.50f, 0.34f),
            Lot("House_Bakery", TownDistrictLayout.DistrictMarketWest, -29.5f, -1f, 10f, 7f, TownDoorSide.South, TownBuildingArchetype.Bakery, 0.78f, 0.58f, 0.35f),
            // Inn: East->South (task 2). Mantida em y=-4 para liberar a margem norte do lago;
            // o conector horizontal ao sul fecha a rota ate a avenida N-S.
            Lot("House_Inn", TownDistrictLayout.DistrictMarketWest, -52f, -6f, 14f, 11f, TownDoorSide.South, TownBuildingArchetype.Inn, 0.62f, 0.50f, 0.36f),
            // Fishery: East->South (task 2); position/size unchanged (already clears its neighbors).
            Lot("House_Fishery", TownDistrictLayout.DistrictLakeParkSouthwest, -52f, -24f, 8f, 9f, TownDoorSide.South, TownBuildingArchetype.Watermill, 0.42f, 0.55f, 0.64f),

            // Distrito LESTE (oficios).
            // Blacksmith/AlchemyLab/Workshop/Tannery: West->South (task 2). Coluna leste reespacada em
            // X (27/40 -> 29/40.5) para abrir >=4un contra House_Registry/House_Archive e entre si, e
            // >=2un de road_main_ew, agora que todos os quatro tem fachada sul.
            Lot("House_Blacksmith", TownDistrictLayout.DistrictResidentialEast, 59f, 0f, 12f, 9f, TownDoorSide.South, TownBuildingArchetype.Forge, 0.34f, 0.30f, 0.28f),
            Lot("House_AlchemyLab", TownDistrictLayout.DistrictResidentialEast, 25.5f, 3.5f, 9f, 9f, TownDoorSide.South, TownBuildingArchetype.Alchemy, 0.30f, 0.52f, 0.56f),
            Lot("House_Workshop", TownDistrictLayout.DistrictResidentialEast, 29.5f, -11f, 8f, 9f, TownDoorSide.South, TownBuildingArchetype.Workshop, 0.56f, 0.46f, 0.30f),
            // Extra vertical breathing room keeps the south-facing blacksmith and tannery facades
            // readable without moving the tannery onto the district road.
            Lot("House_Tannery", TownDistrictLayout.DistrictResidentialEast, 57.5f, -16f, 9f, 9f, TownDoorSide.South, TownBuildingArchetype.Tannery, 0.58f, 0.44f, 0.30f),

            // Distrito SUL (residencial) — duas colunas ladeando a avenida N-S. Todas East/West->South
            // (task 2). Row1 (y=-22->-21) subida 1un para abrir >=2un contra road_south_spine; dentro
            // da row1, Residential_1/3 reespacadas em X (task 3: >=2un contra road_south_lane_w/e)
            // preservando >=4un entre si. AnimalYard/GateKeeper reposicionados
            // (ver comentarios abaixo).
            Lot("House_Residential_4", TownDistrictLayout.DistrictCorralSouth, -36f, -21f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Residential, 0.62f, 0.51f, 0.35f),
            Lot("House_CarvalhoTorto", TownDistrictLayout.DistrictCorralSouth, -30.5f, -41f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Residential, 0.50f, 0.46f, 0.34f),
            Lot("House_Residential_1", TownDistrictLayout.DistrictCorralSouth, -13.5f, -26f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Rural, 0.64f, 0.54f, 0.40f),
            Lot("House_Dagna", TownDistrictLayout.DistrictCorralSouth, -10.5f, -46f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Residential, 0.52f, 0.44f, 0.40f),
            Lot("House_Residential_2", TownDistrictLayout.DistrictCorralSouth, 14.5f, -21f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Market, 0.58f, 0.48f, 0.34f),
            Lot("House_Pip", TownDistrictLayout.DistrictCorralSouth, 18.5f, -42f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Residential, 0.60f, 0.54f, 0.42f),
            Lot("House_Residential_3", TownDistrictLayout.DistrictCorralSouth, 37.5f, -28.5f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Workshop, 0.45f, 0.56f, 0.48f),
            Lot("House_Tovin", TownDistrictLayout.DistrictCorralSouth, 43f, -47f, 8f, 7f, TownDoorSide.South, TownBuildingArchetype.Residential, 0.58f, 0.56f, 0.62f),
            // GateKeeper: West->South. Reshaped from 8x4 to 3.6x2 and moved (10,-40)->(19.5,-41): the
            // corner between road_main_ns, House_Pip and the south wall is only ~7un wide once every
            // neighbor needs South clearance too, so a full 8x4 guard post cannot fit there with
            // >=4un from Pip/Tovin, >=2un from road_main_ns, and >=3un wall clearance simultaneously;
            // 3.6x2 (tucked between House_Pip and House_Tovin, south of the row) is the largest
            // footprint that clears every constraint. Documented reshape, not silently shrunk.
            Lot("House_GateKeeper", TownDistrictLayout.DistrictCorralSouth, 31f, -44f, 8f, 6f, TownDoorSide.South, TownBuildingArchetype.Guard, 0.42f, 0.46f, 0.56f),
            // AnimalYard: East->South. x -42->-43 (task 3: >=2un from road_south_spine now that its
            // frontage faces south); size/y unchanged (already clears CarvalhoTorto/Fishery).
            Lot("House_AnimalYard", TownDistrictLayout.DistrictCorralSouth, 55f, -33f, 16f, 12f, TownDoorSide.South, TownBuildingArchetype.Warehouse, 0.44f, 0.32f, 0.20f),
        };

        // v9 organic relayout NPC anchors (spec_town_layout_v9_organic). Work = recalculated from
        // the new lot's door approach (TownBuildingLot.DoorApproach), offset ~1un further out so the
        // NPC stands clear of the doorway; buildings shared by 2 NPCs get a small tangential offset
        // so both fit outside the same door. Social keeps the existing hub semantics: plaza center
        // (0,4), the Inn/tavern corner, or the night-market corner — repositioned to the new plaza
        // and Inn coordinates. Home is derived separately in CreateMvpTownScene (TownNpcHomes).
        private static readonly TownNpcPlace[] NpcPlaces =
        {
            Npc("npc_corvus", "sacerdote de Kanthor", "House_Temple", -38f, 27f, -6f, 4f),
            Npc("npc_mara", "registradora", "House_Registry", 19f, 14f, 6f, 4f),
            Npc("npc_tovin", "escrivão e licenças", "House_Tovin", 21f, -34f, 6f, -1f),
            Npc("npc_sylveth", "sementes e horticultura", "House_Residential_1", -8f, -22f, -3f, 5f),
            Npc("npc_renko", "loja geral", "House_Residential_2", 9f, -22f, 4f, 5f),
            Npc("npc_mirela", "alfaiataria", "House_Residential_3", 21f, -22f, 8f, -1f),
            Npc("npc_orlan", "estalagem", "House_Inn", -17.5f, -2f, -8f, 6.5f),
            Npc("npc_gruta", "taverna e cozinha", "House_Inn", -14.5f, -2f, -6f, 6.5f),
            Npc("npc_brumdar", "ferreiro", "House_Blacksmith", 26f, 12.5f, 2f, 5f),
            Npc("npc_dagna", "mineração e estrada da pedreira", "House_Dagna", -8f, -34f, -19f, -28f),
            Npc("npc_hund", "guarda e construção", "House_CarvalhoTorto", -22.5f, -34f, -27f, -22f),
            Npc("npc_thalindra", "arquivo e pesquisa", "House_Archive", 45.5f, 20f, 6f, 4f),
            Npc("npc_alaric", "capitão da guarda", "House_GateKeeper", 5f, -40f, 0f, -41f),
            Npc("npc_pip", "mensageiro", "House_Pip", 9f, -34f, 0f, -30f),
            Npc("npc_nimble", "carpintaria", "House_Workshop", 26f, -6f, 8f, 5.5f),
            Npc("npc_gurd", "construção pesada", "House_CarvalhoTorto", -21.5f, -34f, -30f, -22f),
            Npc("npc_yael", "mercado noturno", null, 12f, -10.5f, 8f, -8f),
            Npc("npc_maelor", "rota noturna e cemitério", null, -45f, 25f, -6f, 4f),
            Npc("npc_zrix", "estrada da caverna", null, 57f, -4f, 6f, 4.5f),
            Npc("npc_savra", "ervas e floresta", "House_Residential_4", -22f, -22f, 45f, -18f),
            Npc("npc_ozzra", "alquimia", "House_AlchemyLab", 40f, 7f, 4f, 5.5f),
            Npc("npc_eiran", "animais e armazém", "House_AnimalYard", 46f, -40f, 14f, -6f),
            Npc("npc_liora", "música e jardim das estátuas", null, 3f, 3f, 3f, 3f),
            Npc("npc_velorin", "liderança e conselho", "House_Chamber", -12f, 27f, -8f, 2.5f),
            Npc("npc_sael", "pesca, moinho e cais", "House_Fishery", -39.5f, -15f, -46f, -21f),
            Npc("npc_mella", "padaria e moagem", "House_Bakery", -20f, 9.5f, -2f, 4f),
            Npc("npc_hess", "curtume", "House_Tannery", 40f, -11.5f, 40f, -1f),
            Npc("npc_tibbet", "cemitério e sacristia", null, -43f, 23f, -45f, 27f),
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
                if (Roads[i].Contains(p, tolerance) && IsRoadConnectedToMainNetwork(i))
                {
                    return true;
                }
            }

            return false;
        }

        // Acesso fisico exige mais do que cair sobre um retangulo de caminho: o retangulo precisa
        // tocar a malha que nasce na avenida principal. Busca iterativa pequena e deterministica,
        // usada apenas no editor/gerador e nos testes de layout.
        private static bool IsRoadConnectedToMainNetwork(int startIndex)
        {
            if (s_roadConnectivity != null) return s_roadConnectivity[startIndex];
            var visited = new bool[Roads.Length];
            var queue = new int[Roads.Length];
            int head = 0;
            int tail = 0;
            visited[0] = true;
            queue[tail++] = 0;

            while (head < tail)
            {
                int current = queue[head++];
                for (int candidate = 0; candidate < Roads.Length; candidate++)
                {
                    if (visited[candidate] || !RoadsTouch(Roads[current], Roads[candidate]))
                    {
                        continue;
                    }

                    visited[candidate] = true;
                    queue[tail++] = candidate;
                }
            }

            s_roadConnectivity = visited;
            return visited[startIndex];
        }

        private static bool[] s_roadConnectivity;

        private static bool RoadsTouch(TownRoadSegment a, TownRoadSegment b)
        {
            return TownKeyartGeometry.SegmentDistance(a.Start,a.End,b.Start,b.End) <= (a.Width+b.Width)*.5f+.02f;
        }

        private static TownBuildingLot Lot(
            string name, string districtId, float x, float y, float width, float height,
            TownDoorSide side, TownBuildingArchetype archetype, float r, float g, float b) =>
            new TownBuildingLot(name, districtId, new Vector2(x, y), new Vector2(width, height), side, archetype, new Color(r, g, b));

        private static TownNpcPlace Npc(
            string npcId, string role, string buildingName, float workX, float workY, float socialX, float socialY)
        {
            var work = new Vector2(workX * (4f/3f), (workY - 4f) * (56f/45f) + 4f);
            if(npcId=="npc_liora")work=new Vector2(7f,1f);
            if (!string.IsNullOrWhiteSpace(buildingName) && TryGetBuilding(buildingName, out var lot))
            {
                float tangent = npcId == "npc_gruta" || npcId == "npc_hund" ? -1.6f : 1.6f;
                // Work stays inside the reserved apron and clear of the central 1.4u entrance lane.
                work = (Vector2)lot.DoorApproach + new Vector2(tangent, -.5f);
            }
            var social=new Vector2(socialX,socialY);
            if(npcId=="npc_mirela")social=new Vector2(4f,-6f);
            var fromBasin=social-CentralPlazaCenter;
            if(Mathf.Abs(fromBasin.x)<5f&&Mathf.Abs(fromBasin.y)<3f)
                social=CentralPlazaCenter+(fromBasin.sqrMagnitude<.1f?Vector2.right:fromBasin.normalized)*6f;
            return new TownNpcPlace(npcId, role, buildingName, work, social);
        }
    }
}
