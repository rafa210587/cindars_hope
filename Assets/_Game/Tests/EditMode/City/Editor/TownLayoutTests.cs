using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC.Schedule;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// Pure validation of the preservation-first 120x90 town layout (no scene side effects).
    /// Covers CA-1 (footprint + element coverage), CA-2/CA-4 (stable schedule-anchor & spawn IDs),
    /// CA-3 (lake/park SW + town hall NE + mural exist inside their districts). The scene
    /// regeneration itself is DEFERRED (generator ready; Unity not run) — these tests guard the
    /// deterministic layout math the generator consumes.
    /// </summary>
    [TestFixture]
    public class TownLayoutTests
    {
        private const float Eps = 0.0001f;

        // ── CA-1: footprint canônico (ampliado p/ casas percorríveis — spec_city_real_walkin_houses) ──
        [Test]
        public void Footprint_Is120x90()
        {
            Assert.AreEqual(120f, TownDistrictLayout.WidthTiles, Eps);
            Assert.AreEqual(90f, TownDistrictLayout.HeightTiles, Eps);
            Assert.AreEqual(60f, TownDistrictLayout.HalfWidth, Eps);
            Assert.AreEqual(45f, TownDistrictLayout.HalfHeight, Eps);
        }

        // ── CA-1 / data contract: seven canonical districts, all inside bounds ────────
        [Test]
        public void DistrictMap_HasSevenDistricts()
        {
            Assert.AreEqual(7, TownDistrictLayout.AllDistricts.Count,
                "HUD_LAYOUT §3 / city_rules Rule 1 require exactly 7 districts.");
        }

        [Test]
        public void DistrictMap_AllRectanglesWithinBounds()
        {
            foreach (var d in TownDistrictLayout.AllDistricts)
            {
                Assert.IsTrue(d.WithinBounds(),
                    $"District '{d.Id}' rectangle escapes the 120x90 bounds " +
                    $"(x[{d.MinX},{d.MaxX}] y[{d.MinY},{d.MaxY}]).");
            }
        }

        [Test]
        public void DistrictMap_ContainsAllSevenCanonicalIds()
        {
            string[] ids =
            {
                TownDistrictLayout.DistrictCentralPlaza,
                TownDistrictLayout.DistrictMarketWest,
                TownDistrictLayout.DistrictResidentialEast,
                TownDistrictLayout.DistrictTempleNorth,
                TownDistrictLayout.DistrictCorralSouth,
                TownDistrictLayout.DistrictTownHallNortheast,
                TownDistrictLayout.DistrictLakeParkSouthwest,
            };
            foreach (var id in ids)
            {
                Assert.IsTrue(TownDistrictLayout.TryGetDistrict(id, out _),
                    $"Canonical district '{id}' is missing from the layout map.");
            }
        }

        // ── No-overlap: district CENTERS are not contained by another district ────────
        // (Districts may share an edge with the ring road, but no core sits inside another core.)
        [Test]
        public void DistrictMap_CentersDoNotOverlapOtherDistricts()
        {
            var all = TownDistrictLayout.AllDistricts;
            for (int i = 0; i < all.Count; i++)
            {
                for (int j = 0; j < all.Count; j++)
                {
                    if (i == j) continue;
                    Assert.IsFalse(all[j].Contains(all[i].Center),
                        $"District '{all[i].Id}' center lies inside district '{all[j].Id}' — cores overlap.");
                }
            }
        }

        // ── CA-3: new districts exist with landmarks inside their rectangle + bounds ──
        [Test]
        public void LakeParkDistrict_LandmarksInsideDistrictAndBounds()
        {
            Assert.IsTrue(TownDistrictLayout.TryGetDistrict(TownDistrictLayout.DistrictLakeParkSouthwest, out var d));
            AssertInside(d, TownDistrictLayout.LakeCenter, "LakeWater");
            AssertInside(d, TownDistrictLayout.LakeBenchWest, "ParkBench_W");
            AssertInside(d, TownDistrictLayout.LakeBenchEast, "ParkBench_E");
            // SW corner sanity: the lake sits in the south-west quadrant.
            Assert.Less(TownDistrictLayout.LakeCenter.x, 0f, "Lake must be west.");
            Assert.Less(TownDistrictLayout.LakeCenter.y, 0f, "Lake must be south.");
        }

        [Test]
        public void TownHallDistrict_BuildingAndMuralInsideDistrictAndBounds()
        {
            Assert.IsTrue(TownDistrictLayout.TryGetDistrict(TownDistrictLayout.DistrictTownHallNortheast, out var d));
            AssertInside(d, TownDistrictLayout.TownHallCenter, "TownHallBuilding");
            AssertInside(d, TownDistrictLayout.TownHallMural, "TownHallMural");
            // NE corner sanity: the town hall sits in the north-east quadrant.
            Assert.Greater(TownDistrictLayout.TownHallCenter.x, 0f, "Town hall must be east.");
            Assert.Greater(TownDistrictLayout.TownHallCenter.y, 0f, "Town hall must be north.");
        }

        // ── CA-1: element coverage — every repositioned legacy point stays inside the playfield ──
        // (No element is pushed out of the footprint by the relayout = nothing lost off-map.)
        [Test]
        public void Reposition_KeepsExtremeLegacyCornersInsideBounds()
        {
            // Furthest legacy authoring points (perimeter trees / corner houses).
            Vector3[] legacyExtremes =
            {
                new Vector3(-16.5f, 11f, 0f), new Vector3(16.5f, 11.5f, 0f),
                new Vector3(-16.5f, -11f, 0f), new Vector3(16.5f, -11f, 0f),
                new Vector3(14.5f, 10.5f, 0f), new Vector3(-14.5f, -3.5f, 0f),
                new Vector3(0f, -13.5f, 0f),
            };
            foreach (var legacy in legacyExtremes)
            {
                var p = TownDistrictLayout.Reposition(legacy);
                Assert.IsTrue(Mathf.Abs(p.x) <= TownDistrictLayout.HalfWidth - 1f + Eps,
                    $"Repositioned x {p.x} for legacy {legacy} escapes the playfield interior.");
                Assert.IsTrue(Mathf.Abs(p.y) <= TownDistrictLayout.HalfHeight - 1f + Eps,
                    $"Repositioned y {p.y} for legacy {legacy} escapes the playfield interior.");
            }
        }

        [Test]
        public void Reposition_IsDeterministicAndPreservesZ()
        {
            var legacy = new Vector3(3.5f, 7f, 0.25f);
            var a = TownDistrictLayout.Reposition(legacy);
            var b = TownDistrictLayout.Reposition(legacy);
            Assert.AreEqual(a, b, "Reposition must be deterministic.");
            Assert.AreEqual(0.25f, a.z, Eps, "Reposition must preserve z.");
        }

        [Test]
        public void Reposition_ScalesAwayFromCenter()
        {
            // A point off-center moves further from the origin (footprint grows ~1.3x).
            var legacy = new Vector3(6f, -4f, 0f);
            var p = TownDistrictLayout.Reposition(legacy);
            Assert.Greater(Mathf.Abs(p.x), Mathf.Abs(legacy.x), "Relayout should spread x outward.");
            Assert.Greater(Mathf.Abs(p.y), Mathf.Abs(legacy.y), "Relayout should spread y outward.");
            // Origin is a fixed point.
            var origin = TownDistrictLayout.Reposition(Vector3.zero);
            Assert.AreEqual(Vector3.zero, origin, "Town center must remain at the origin.");
        }

        // ── CA-2: schedule-anchor suffix IDs are FROZEN (match the runtime resolver constants) ──
        [Test]
        public void ScheduleAnchorSuffixes_MatchRuntimeResolverConstants()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    NpcScheduleBlockResolver.WorkAnchorSuffix,
                    NpcScheduleBlockResolver.SocialAnchorSuffix,
                    NpcScheduleBlockResolver.HomeAnchorSuffix,
                },
                TownDistrictLayout.StableScheduleAnchorSuffixes,
                "Schedule-anchor suffixes must stay identical to the NPC resolver (IDs frozen).");
        }

        [Test]
        public void ScheduleAnchorSuffixes_AreExactlyWorkSocialHome()
        {
            Assert.AreEqual("work", NpcScheduleBlockResolver.WorkAnchorSuffix);
            Assert.AreEqual("social", NpcScheduleBlockResolver.SocialAnchorSuffix);
            Assert.AreEqual("home", NpcScheduleBlockResolver.HomeAnchorSuffix);
        }

        [Test]
        public void StationaryCivicRoles_UseBusinessHoursInsteadOfWandererHours()
        {
            var chamber = NpcScheduleBlockResolver.ArchetypeFromMovementProfile("Stationary/ChamberDesk", false);
            var cemetery = NpcScheduleBlockResolver.ArchetypeFromMovementProfile("Stationary/Cemetery", false);
            Assert.AreEqual(NpcScheduleArchetype.Shopkeeper, chamber);
            Assert.AreEqual(NpcScheduleArchetype.Shopkeeper, cemetery);
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(chamber, 10));
            Assert.AreEqual(NpcRuntimeBlock.Social, NpcScheduleBlockResolver.ResolveBlock(chamber, 19));
            Assert.AreEqual(NpcRuntimeBlock.Home, NpcScheduleBlockResolver.ResolveBlock(chamber, 23));
        }

        // ── CA-4: spawn IDs are FROZEN (saves transition without error) ───────────────
        [Test]
        public void SpawnIds_AreStableCanonicalSet()
        {
            CollectionAssert.AreEquivalent(
                new[] { "town_default", "town_from_farm" },
                TownDistrictLayout.StableSpawnIds,
                "Spawn IDs must remain town_default / town_from_farm across the relayout (CA-4).");
        }

        [Test]
        public void PreservationBaseline_ContainsExpectedMinimumCounts()
        {
            Assert.AreEqual(24, TownCityLayout.BaselineHouseCount);
            Assert.AreEqual(23, TownCityLayout.BaselineNpcStallCount);
            Assert.AreEqual(6, TownCityLayout.BaselineMarketStallCount);
            Assert.AreEqual(497, TownCityLayout.BaselineTreeCount);
            Assert.AreEqual(TownCityLayout.BaselineHouseCount, TownCityLayout.AllBuildings.Count);
        }

        [Test]
        public void BuildingLots_HaveUniqueStableNamesAndDoNotOverlap()
        {
            var names = new HashSet<string>();
            var lots = TownCityLayout.AllBuildings;
            for (var i = 0; i < lots.Count; i++)
            {
                Assert.IsTrue(names.Add(lots[i].Name), $"Duplicate stable building name: {lots[i].Name}");
                Assert.IsTrue(TownDistrictLayout.TryGetDistrict(lots[i].DistrictId, out _),
                    $"Building {lots[i].Name} references unknown district {lots[i].DistrictId}.");
                Assert.GreaterOrEqual(lots[i].MinX, -TownDistrictLayout.HalfWidth);
                Assert.LessOrEqual(lots[i].MaxX, TownDistrictLayout.HalfWidth);
                Assert.GreaterOrEqual(lots[i].MinY, -TownDistrictLayout.HalfHeight);
                Assert.LessOrEqual(lots[i].MaxY, TownDistrictLayout.HalfHeight);

                for (var j = i + 1; j < lots.Count; j++)
                {
                    Assert.IsFalse(lots[i].Overlaps(lots[j], 0.3f),
                        $"Building lots overlap: {lots[i].Name} × {lots[j].Name}.");
                }
            }
        }

        [Test]
        public void BuildingLots_DoNotOverlapRoads_AndEveryDoorReachesRoad()
        {
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                foreach (var road in TownCityLayout.AllRoads)
                {
                    Assert.IsFalse(lot.Overlaps(road), $"{lot.Name} overlaps road {road.Id}.");
                }

                Assert.IsTrue(TownCityLayout.IsPointOnRoad(lot.DoorApproach, 0.4f),
                    $"Door approach for {lot.Name} is disconnected from the road graph.");
            }
        }

        [Test]
        public void DoorFrontages_SupportAllFourDirections()
        {
            var sides = new HashSet<TownDoorSide>();
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                sides.Add(lot.DoorSide);
            }

            CollectionAssert.AreEquivalent(
                new[] { TownDoorSide.North, TownDoorSide.South, TownDoorSide.West, TownDoorSide.East },
                sides);
        }

        [Test]
        public void NpcPlaces_PreserveAllRoles_AndStayNearAssociatedBuildings()
        {
            Assert.AreEqual(28, TownCityLayout.AllNpcPlaces.Count, "All materialized canonical NPCs need a placement contract.");
            var ids = new HashSet<string>();
            foreach (var place in TownCityLayout.AllNpcPlaces)
            {
                Assert.IsTrue(ids.Add(place.NpcId), $"Duplicate NPC placement: {place.NpcId}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(place.Role), $"NPC {place.NpcId} has no role description.");

                if (string.IsNullOrWhiteSpace(place.BuildingName))
                {
                    continue;
                }

                Assert.IsTrue(TownCityLayout.TryGetBuilding(place.BuildingName, out var building),
                    $"NPC {place.NpcId} references missing building {place.BuildingName}.");
                Assert.LessOrEqual(Vector2.Distance(place.Work, building.DoorApproach), 2.1f,
                    $"NPC {place.NpcId} work anchor is detached from {place.BuildingName}.");

                var stall = TownCityLayout.ResolveNpcStallPosition(place.NpcId, Vector3.zero);
                Assert.GreaterOrEqual(Vector2.Distance(stall, building.DoorApproach), 1.9f,
                    $"NPC {place.NpcId} stall blocks the door approach of {place.BuildingName}.");
            }
        }

        [Test]
        public void V8_ZonesUseLargeSemanticBuildings()
        {
            AssertLot("House_Temple", TownBuildingArchetype.Temple, 16f, 13f);
            AssertLot("House_Manor", TownBuildingArchetype.Noble, 16f, 13f);
            AssertLot("House_Chamber", TownBuildingArchetype.Civic, 16f, 11f);
            AssertLot("House_Archive", TownBuildingArchetype.Noble, 17f, 13f);
            AssertLot("House_MarketHall", TownBuildingArchetype.Market, 18f, 12f);
            AssertLot("House_Inn", TownBuildingArchetype.Inn, 16f, 11f);
            AssertLot("House_Bakery", TownBuildingArchetype.Bakery, 12f, 10f);
            AssertLot("House_Blacksmith", TownBuildingArchetype.Forge, 13f, 10f);
            AssertLot("House_AlchemyLab", TownBuildingArchetype.Alchemy, 13f, 10f);
            AssertLot("House_Tannery", TownBuildingArchetype.Tannery, 14f, 10f);
            AssertLot("House_Fishery", TownBuildingArchetype.Watermill, 12f, 9f);
            AssertLot("House_AnimalYard", TownBuildingArchetype.Warehouse, 17f, 12f);
            Assert.AreEqual(new Vector2(0f, 4f), TownCityLayout.CentralPlazaCenter);
            Assert.AreEqual(new Vector2(26f, 26f), TownCityLayout.CentralPlazaSize,
                "The civic square must occupy the large reserved central block from the visual reference.");
        }

        [Test]
        public void V8_SouthResidentialRowLeavesCentralGateAvenueOpen()
        {
            foreach (var name in new[] { "House_CarvalhoTorto", "House_Dagna", "House_GateKeeper", "House_Pip", "House_Residential_4", "House_Tovin" })
            {
                Assert.IsTrue(TownCityLayout.TryGetBuilding(name, out var lot));
                Assert.AreEqual(-36f, lot.Center.y, Eps, $"{name} left the south residential row.");
                Assert.AreEqual(TownCityLayout.StandardHouseExteriorSize, lot.Size, $"{name} must be 8x7 externally.");
                Assert.IsFalse(lot.MinX < 4f && lot.MaxX > -4f, $"{name} blocks the south gate avenue.");
            }
            Assert.AreEqual(new Vector2(6f, 5f), TownCityLayout.StandardHouseInteriorSize);
            Assert.AreEqual(
                TownCityLayout.StandardHouseExteriorSize - new Vector2(2f, 2f),
                TownCityLayout.StandardHouseInteriorSize,
                "A one-tile wall/inset per side must produce a 6x5 walkable interior.");
        }

        [Test]
        public void V8_CirculationUsesWideAvenuesAndClearStallAprons()
        {
            foreach (var road in TownCityLayout.AllRoads)
            {
                if (road.Id == "road_civic_frontage")
                {
                    Assert.GreaterOrEqual(road.Size.y, 3f, "Civic frontage is the only constrained secondary road.");
                    continue;
                }

                float crossSection = Mathf.Min(road.Size.x, road.Size.y);
                Assert.GreaterOrEqual(crossSection, 4f, $"{road.Id} is narrower than the v8 circulation contract.");
            }

            foreach (var place in TownCityLayout.AllNpcPlaces)
            {
                if (string.IsNullOrWhiteSpace(place.BuildingName)) continue;
                Assert.IsTrue(TownCityLayout.TryGetBuilding(place.BuildingName, out var lot));
                var stall = TownCityLayout.ResolveNpcStallPosition(place.NpcId, Vector3.zero);
                Assert.GreaterOrEqual(Vector2.Distance(stall, lot.DoorApproach), 3.9f,
                    $"Stall for {place.NpcId} intrudes into the 3x3 door apron.");
            }
        }

        private static void AssertLot(string name, TownBuildingArchetype archetype, float minWidth, float minHeight)
        {
            Assert.IsTrue(TownCityLayout.TryGetBuilding(name, out var lot), $"Missing v8 lot {name}.");
            Assert.AreEqual(archetype, lot.Archetype, $"Wrong semantic archetype for {name}.");
            Assert.GreaterOrEqual(lot.Size.x, minWidth, $"{name} is too narrow for the v8 reference.");
            Assert.GreaterOrEqual(lot.Size.y, minHeight, $"{name} is too shallow for the v8 reference.");
        }

        private static void AssertInside(TownDistrictLayout.District d, Vector3 point, string label)
        {
            var p2 = new Vector2(point.x, point.y);
            Assert.IsTrue(d.Contains(p2),
                $"{label} at {p2} is outside district '{d.Id}' (x[{d.MinX},{d.MaxX}] y[{d.MinY},{d.MaxY}]).");
            Assert.IsTrue(Mathf.Abs(point.x) <= TownDistrictLayout.HalfWidth &&
                          Mathf.Abs(point.y) <= TownDistrictLayout.HalfHeight,
                $"{label} at {p2} is outside the 120x90 bounds.");
        }
    }
}
