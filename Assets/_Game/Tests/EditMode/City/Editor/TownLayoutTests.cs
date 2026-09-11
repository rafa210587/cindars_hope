using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC.Schedule;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// Pure validation of the preservation-first 160x112 town layout (no scene side effects).
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
        public void Footprint_Is160x112()
        {
            Assert.AreEqual(160f, TownDistrictLayout.WidthTiles, Eps);
            Assert.AreEqual(112f, TownDistrictLayout.HeightTiles, Eps);
            Assert.AreEqual(80f, TownDistrictLayout.HalfWidth, Eps);
            Assert.AreEqual(56f, TownDistrictLayout.HalfHeight, Eps);
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
                    $"District '{d.Id}' rectangle escapes the 160x112 bounds " +
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
            var landmarkOverlays = new HashSet<string>
            {
                TownDistrictLayout.DistrictTownHallNortheast,
                TownDistrictLayout.DistrictLakeParkSouthwest
            };
            for (int i = 0; i < all.Count; i++)
            {
                if (landmarkOverlays.Contains(all[i].Id)) continue;
                for (int j = 0; j < all.Count; j++)
                {
                    if (i == j || landmarkOverlays.Contains(all[j].Id)) continue;
                    Assert.IsFalse(all[j].Contains(all[i].Center),
                        $"District '{all[i].Id}' center lies inside district '{all[j].Id}' — cores overlap.");
                }
            }
        }

        // ── CA-3: new districts exist with landmarks inside their rectangle + bounds ──
        [Test]
        public void LakeParkDistrict_LandmarksStayInsideTownBounds()
        {
            Assert.IsTrue(TownDistrictLayout.TryGetDistrict(TownDistrictLayout.DistrictLakeParkSouthwest, out var d));
            AssertInside(d, TownDistrictLayout.LakeCenter, "LakeWater");
            AssertInside(d, TownDistrictLayout.LakeBenchWest, "ParkBench_W");
            AssertInsideTownBounds(TownDistrictLayout.LakeBenchEast, "ParkBench_E promenade");
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
            Assert.AreEqual(554, TownCityLayout.BaselineTreeCount);
            Assert.AreEqual(3, TownCityLayout.ExteriorForestBandCount, "Count must describe actual materialized forest depth, not skipped logical bands.");
            Assert.LessOrEqual(TownCityLayout.ExteriorForestStep, 2.3f);
            Assert.GreaterOrEqual(TownCityLayout.ExteriorForestGateHalfClearance, 6f);
            Assert.GreaterOrEqual(TownCityLayout.ExteriorForestGroundPadding, 28f);
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
        public void DoorFrontages_AllFaceSouthInV9()
        {
            var sides = new HashSet<TownDoorSide>();
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                sides.Add(lot.DoorSide);
            }

            CollectionAssert.AreEquivalent(new[] { TownDoorSide.South }, sides);
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
                Assert.LessOrEqual(Vector2.Distance(place.Work, building.DoorApproach), 16f,
                    $"NPC {place.NpcId} work anchor escaped the district of {place.BuildingName}.");

                var stall = TownCityLayout.ResolveNpcStallPosition(place.NpcId, Vector3.zero);
                Assert.GreaterOrEqual(Vector2.Distance(stall, building.DoorApproach), 1.9f,
                    $"NPC {place.NpcId} stall blocks the door approach of {place.BuildingName}.");
            }
        }

        [Test]
        public void BuildingIdentity_AllTwentyFourCanonicalIdsKeepTheirArchetypes()
        {
            var expected=new Dictionary<string,TownBuildingArchetype>
            {
                ["House_Temple"]=TownBuildingArchetype.Temple,["House_Chamber"]=TownBuildingArchetype.Civic,
                ["House_Prison"]=TownBuildingArchetype.Guard,["House_Manor"]=TownBuildingArchetype.Noble,
                ["House_Registry"]=TownBuildingArchetype.Civic,["House_Archive"]=TownBuildingArchetype.Noble,
                ["House_MarketHall"]=TownBuildingArchetype.Market,["House_Bakery"]=TownBuildingArchetype.Bakery,
                ["House_Inn"]=TownBuildingArchetype.Inn,["House_Fishery"]=TownBuildingArchetype.Watermill,
                ["House_Blacksmith"]=TownBuildingArchetype.Forge,["House_AlchemyLab"]=TownBuildingArchetype.Alchemy,
                ["House_Workshop"]=TownBuildingArchetype.Workshop,["House_Tannery"]=TownBuildingArchetype.Tannery,
                ["House_Residential_4"]=TownBuildingArchetype.Residential,["House_CarvalhoTorto"]=TownBuildingArchetype.Residential,
                ["House_Residential_1"]=TownBuildingArchetype.Rural,["House_Dagna"]=TownBuildingArchetype.Residential,
                ["House_Residential_2"]=TownBuildingArchetype.Market,["House_Pip"]=TownBuildingArchetype.Residential,
                ["House_Residential_3"]=TownBuildingArchetype.Workshop,["House_Tovin"]=TownBuildingArchetype.Residential,
                ["House_GateKeeper"]=TownBuildingArchetype.Guard,["House_AnimalYard"]=TownBuildingArchetype.Warehouse
            };
            Assert.AreEqual(24,expected.Count);
            Assert.AreEqual(expected.Count,TownCityLayout.AllBuildings.Count);
            foreach(var lot in TownCityLayout.AllBuildings)
            {
                Assert.IsTrue(expected.TryGetValue(lot.Name,out var archetype),lot.Name+" changed a stable identity.");
                Assert.AreEqual(archetype,lot.Archetype,lot.Name+" changed gameplay archetype.");
            }
        }

        [Test]
        public void BuildingShells_ContainUsableInteriorAndAnActualDoorFrame()
        {
            // Replaces legacy v8 footprint sizes explicitly authorized for keyart hierarchy.
            // A smaller civic annex must still hold the same minimum useful walk-in space.
            foreach(var lot in TownCityLayout.AllBuildings)
            {
                var interior=lot.Size-new Vector2(2,2);
                Assert.GreaterOrEqual(interior.x,6,lot.Name+" loses useful interior width.");
                Assert.GreaterOrEqual(interior.y,lot.Name=="House_GateKeeper"?4:5,lot.Name+" loses useful interior depth.");
                float doorX=lot.DoorPosition.x;
                Assert.GreaterOrEqual(doorX-lot.MinX,1.5f,lot.Name+" lacks west door frame/wall.");
                Assert.GreaterOrEqual(lot.MaxX-doorX,1.5f,lot.Name+" lacks east door frame/wall.");
                Assert.AreEqual(lot.MinY,lot.DoorPosition.y,Eps);
                Assert.AreEqual(lot.MinY-1.5f,lot.DoorApproach.y,Eps);
            }
        }

        [Test]
        public void CivicHierarchy_AnnexesRemainSubordinateWhileLandmarkAndPlazaReserveStayIntact()
        {
            Assert.IsTrue(TownCityLayout.TryGetBuilding("House_Temple",out var temple));
            Assert.AreEqual(new Vector2(16,11),temple.Size);
            foreach(var name in new[]{"House_Chamber","House_Prison","House_Registry","House_Manor","House_Archive","House_MarketHall"})
            {
                Assert.IsTrue(TownCityLayout.TryGetBuilding(name,out var annex));
                Assert.Less(annex.Size.x,temple.Size.x*.65f,name+" competes with the landmark footprint.");
                Assert.Less(annex.Size.x*annex.Size.y,temple.Size.x*temple.Size.y*.5f,name+" is no longer an annex.");
                Assert.AreEqual(annex.Size.x * 1.20f,TownKeyartBuildingArt.VisibleWidth(name,annex.Size),Eps,
                    name+" must retain the authorized twenty-percent visual enlargement over the preserved shell.");
            }
            Assert.AreEqual(28.8f,TownKeyartBuildingArt.VisibleWidth("House_Temple",temple.Size),Eps);
            Assert.AreEqual(28.8f,TownKeyartBuildingArt.VisibleWidth("TownHallBuilding",TownKeyartGeometry.HallFootprintSize),Eps);
            Assert.AreEqual(new Vector2(0,4),TownCityLayout.CentralPlazaCenter);
            Assert.AreEqual(new Vector2(26,26),TownCityLayout.CentralPlazaSize);
        }

        [Test]
        public void SouthResidentialCourtsLeaveCentralGateAvenueOpen()
        {
            foreach (var name in new[] { "House_CarvalhoTorto", "House_Dagna", "House_Pip", "House_Tovin" })
            {
                Assert.IsTrue(TownCityLayout.TryGetBuilding(name, out var lot));
                Assert.Less(lot.MaxY, -20f, $"{name} must remain in the southern residential district.");
                Assert.AreEqual(TownCityLayout.StandardHouseExteriorSize, lot.Size, $"{name} must be 8x7 externally.");
                Assert.IsFalse(lot.MinX < 4f && lot.MaxX > -4f, $"{name} blocks the south gate avenue.");
            }
            foreach (var name in new[] { "House_Residential_4", "House_Residential_1", "House_Residential_2", "House_Residential_3" })
            {
                Assert.IsTrue(TownCityLayout.TryGetBuilding(name, out var lot));
                Assert.Less(lot.MaxY, -9f, $"{name} must remain south of the plaza.");
                Assert.AreEqual(TownCityLayout.StandardHouseExteriorSize, lot.Size);
                Assert.IsFalse(lot.MinX < 4f && lot.MaxX > -4f, $"{name} blocks the south gate avenue.");
            }
            Assert.AreEqual(new Vector2(6f, 5f), TownCityLayout.StandardHouseInteriorSize);
            Assert.AreEqual(
                TownCityLayout.StandardHouseExteriorSize - new Vector2(2f, 2f),
                TownCityLayout.StandardHouseInteriorSize,
                "A one-tile wall/inset per side must produce a 6x5 walkable interior.");
        }

        [Test]
        public void V9_CirculationPreservesPassableRoadsAndClearStallAprons()
        {
            foreach (var road in TownCityLayout.AllRoads)
            {
                float crossSection = road.Width;
                if (road.Id.StartsWith("main_south_") || road.Id.StartsWith("main_north_") || road.Id.StartsWith("market_") || road.Id.StartsWith("craft_"))
                {
                    Assert.GreaterOrEqual(crossSection, 3f, $"{road.Id} must retain the approved three-unit primary corridor.");
                    continue;
                }

                Assert.GreaterOrEqual(crossSection, 2f, $"{road.Id} is narrower than the v9 frontage contract.");
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

        private static void AssertInside(TownDistrictLayout.District d, Vector3 point, string label)
        {
            var p2 = new Vector2(point.x, point.y);
            Assert.IsTrue(d.Contains(p2),
                $"{label} at {p2} is outside district '{d.Id}' (x[{d.MinX},{d.MaxX}] y[{d.MinY},{d.MaxY}]).");
            Assert.IsTrue(Mathf.Abs(point.x) <= TownDistrictLayout.HalfWidth &&
                          Mathf.Abs(point.y) <= TownDistrictLayout.HalfHeight,
                $"{label} at {p2} is outside the 160x112 bounds.");
        }

        private static void AssertInsideTownBounds(Vector3 point, string label)
        {
            Assert.IsTrue(Mathf.Abs(point.x) <= TownDistrictLayout.HalfWidth &&
                          Mathf.Abs(point.y) <= TownDistrictLayout.HalfHeight,
                $"{label} at ({point.x}, {point.y}) is outside the 160x112 bounds.");
        }
    }
}
