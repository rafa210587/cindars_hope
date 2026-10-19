using System.Collections.Generic;
using CindarsHope.Editor.SceneCreation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class TownSpatialExpansionTests
    {
        private const float Epsilon = .001f;

        [Test]
        public void TownBounds_Are160By112_AndBuildingScaleRemains120Percent()
        {
            Assert.AreEqual(new Vector2(160,112), new Vector2(TownDistrictLayout.WidthTiles,TownDistrictLayout.HeightTiles));
            Assert.AreEqual(1.2f,TownKeyartBuildingArt.VisualEnlargement,Epsilon);
            Assert.IsTrue(TownKeyartGeometry.ClearPoint(new Vector2(77,0),new TownBuildingLot[0],.5f));
            Assert.IsFalse(TownKeyartGeometry.ClearPoint(new Vector2(80,0),new TownBuildingLot[0],.5f));
            Assert.IsFalse(TownKeyartGeometry.ClearPoint(new Vector2(0,56),new TownBuildingLot[0],.5f));
        }

        [Test]
        public void Expansion_PreservesScheduledCatalogAndStableIds_MaterializedCensusRequiresValidator()
        {
            string[] names={"Temple","Chamber","Prison","Manor","Registry","Archive","MarketHall","Bakery","Inn","Fishery",
                "Blacksmith","AlchemyLab","Workshop","Tannery","Residential_4","CarvalhoTorto","Residential_1","Dagna",
                "Residential_2","Pip","Residential_3","Tovin","GateKeeper","AnimalYard"};
            var actual=new HashSet<string>();foreach(var lot in TownCityLayout.AllBuildings)Assert.IsTrue(actual.Add(lot.Name));
            foreach(string name in names)Assert.IsTrue(actual.Contains("House_"+name),name);
            Assert.AreEqual(names.Length,actual.Count);
            var anchors=new HashSet<string>();var npcIds=new HashSet<string>();
            foreach(var npc in TownCityLayout.AllNpcPlaces)
            {
                Assert.IsTrue(npcIds.Add(npc.NpcId));
                foreach(string suffix in TownDistrictLayout.StableScheduleAnchorSuffixes)Assert.IsTrue(anchors.Add(npc.NpcId+"_"+suffix));
                if(!string.IsNullOrEmpty(npc.BuildingName))Assert.IsTrue(actual.Contains(npc.BuildingName),npc.NpcId+" lost its house.");
            }
            Assert.AreEqual(28,npcIds.Count);Assert.AreEqual(84,anchors.Count);
            CollectionAssert.AreEquivalent(new[]{"town_default","town_from_farm"},TownDistrictLayout.StableSpawnIds);
            Assert.AreEqual(23,TownCityLayout.BaselineNpcStallCount);Assert.AreEqual(6,TownCityLayout.BaselineMarketStallCount);
            Assert.GreaterOrEqual(TownCityLayout.BaselineTreeCount,554,"Do not use the obsolete 497-tree baseline to remove materialized content.");
            // The twenty-ninth unscheduled actor and renderer/reference census require the saved-scene gate.
        }

        [Test]
        public void Buildings_KeepFourUnitFootprintGap_AndWallReserve_VisualEnvelopesRequireValidator()
        {
            var lots=TownCityLayout.AllBuildings;
            for(int i=0;i<lots.Count;i++)
            {
                for(int j=i+1;j<lots.Count;j++)Assert.GreaterOrEqual(TownAccessMetrics.Gap(lots[i],lots[j]),4-Epsilon,lots[i].Name+" / "+lots[j].Name);
                Assert.GreaterOrEqual(TownAccessMetrics.RectGap(lots[i].Center,lots[i].Size,TownKeyartGeometry.HallFootprintCenter,
                    TownKeyartGeometry.HallFootprintSize),4-Epsilon,lots[i].Name+" / Hall");
                Assert.GreaterOrEqual(TownDistrictLayout.HalfWidth-Mathf.Max(Mathf.Abs(lots[i].MinX),Mathf.Abs(lots[i].MaxX)),3-Epsilon,lots[i].Name+" wall x");
                Assert.GreaterOrEqual(TownDistrictLayout.HalfHeight-Mathf.Max(Mathf.Abs(lots[i].MinY),Mathf.Abs(lots[i].MaxY)),3-Epsilon,lots[i].Name+" wall y");
            }
            var facade=new Rect(0,0,10,10);var steps=new Rect(4,-2,2,3);
            Assert.IsTrue(TownAccessMetrics.ObscuresCriticalFacade(new Rect(4,-1,1,1),facade,steps),"A small overlap may still hide the door.");
            Assert.IsFalse(TownAccessMetrics.ObscuresCriticalFacade(new Rect(-2,8,2,2),facade,steps),"Disjoint roof margins are not facade occlusion.");
        }

        [Test]
        public void RoadClasses_RespectMinimumClearWidths()
        {
            var seen=new HashSet<TownRoadClass>();
            foreach(var road in TownCityLayout.AllRoads)
            {
                var roadClass=TownAccessMetrics.RoadClass(road.Id);seen.Add(roadClass);
                Assert.GreaterOrEqual(road.Width,TownAccessMetrics.Width(roadClass)-Epsilon,road.Id);
                Assert.IsTrue(TownKeyartGeometry.ClearRoute(road.Start,road.End,road.Width*.5f,TownCityLayout.AllBuildings),road.Id);
            }
            CollectionAssert.AreEquivalent(new[]{TownRoadClass.Main,TownRoadClass.District,TownRoadClass.Local},seen);
        }

        [Test]
        public void DoorsHaveClearAprons_AndWalkInInteriorsHaveLaneAndTurnPocket()
        {
            int walkIn=0,yards=0;
            foreach(var lot in TownCityLayout.AllBuildings)
            {
                var apron=TownAccessMetrics.DoorApron(lot);
                Assert.AreEqual(12,apron.width*apron.height,Epsilon,lot.Name);
                Assert.IsTrue(TownAccessMetrics.ClearsApron(apron,TownCityLayout.AllBuildings),lot.Name);
                for(float x=apron.xMin;x<=apron.xMax+Epsilon;x+=.5f)for(float y=apron.yMin;y<=apron.yMax+Epsilon;y+=.5f)
                    Assert.IsFalse(TownKeyartGeometry.ContainsWater(new Vector2(x,y)),lot.Name+" wet apron");
                if(lot.Name=="House_AnimalYard"){yards++;continue;}walkIn++;
                var usable=new Rect(lot.Center-lot.Size*.5f+Vector2.one,lot.Size-Vector2.one*2);
                var pocket=TownAccessMetrics.InteriorPocket(lot);
                Assert.IsTrue(usable.Contains(pocket.min)&&usable.Contains(pocket.max-new Vector2(Epsilon,Epsilon)),lot.Name+" turn pocket outside shell");
                Assert.GreaterOrEqual(TownAccessMetrics.InteriorLane(lot).width,1.4f-Epsilon);
            }
            Assert.AreEqual(23,walkIn);Assert.AreEqual(1,yards);
            // Materialized furniture/walls and door-open states are checked by TownAccess, not inferred here.
        }

        [Test]
        public void ComfortProbe_ReachesEveryDoorWorkAnchorAndSpawn()
        {
            var roads=TownCityLayout.AllRoads;var start=TownAccessMetrics.PlazaAccessPoint;
            foreach(var lot in TownCityLayout.AllBuildings)
                Assert.IsFalse(float.IsInfinity(TownAccessMetrics.ShortestPathLength(start,lot.DoorApproach,roads,.5f)),lot.Name);
            foreach(var npc in TownCityLayout.AllNpcPlaces)
            {
                Assert.IsTrue(TownKeyartGeometry.ClearPoint(npc.Work,TownCityLayout.AllBuildings,.5f),npc.NpcId);
                Assert.IsFalse(float.IsInfinity(TownAccessMetrics.ShortestPathLength(start,npc.Work,roads,.5f)),npc.NpcId);
            }
            Assert.IsFalse(float.IsInfinity(TownAccessMetrics.ShortestPathLength(start,TownDistrictLayout.DefaultSpawn,roads,.5f)));
            Assert.IsFalse(float.IsInfinity(TownAccessMetrics.ShortestPathLength(start,TownDistrictLayout.FromFarmSpawn,roads,.5f)));
        }

        [Test]
        public void ShortestPaths_StayWithinTownTravelBudgets()
        {
            var roads=TownCityLayout.AllRoads;var plaza=TownAccessMetrics.PlazaAccessPoint;
            Assert.LessOrEqual(TownAccessMetrics.ShortestPathLength(TownDistrictLayout.FromFarmSpawn,plaza,roads,.5f),75f);
            foreach(var lot in TownCityLayout.AllBuildings)
                Assert.LessOrEqual(TownAccessMetrics.ShortestPathLength(plaza,lot.DoorApproach,roads,.5f),90f,lot.Name);
        }

        [Test]
        public void ExpandedGroundWaterWallsAndPortalsStayCoherent()
        {
            foreach(var district in TownDistrictLayout.AllDistricts)Assert.IsTrue(district.WithinBounds(),district.Id);
            foreach(var p in TownKeyartGeometry.LakeOutline)
            {Assert.LessOrEqual(Mathf.Abs(p.x),80);Assert.LessOrEqual(Mathf.Abs(p.y),56);}
            foreach(var road in TownCityLayout.AllRoads)Assert.IsTrue(TownKeyartGeometry.RoadClearsWater(road),road.Id);
            foreach(var portal in new Vector2[]{TownDistrictLayout.SouthPortal,TownKeyartGeometry.WestArrival,TownKeyartGeometry.EastArrival})
            {
                Assert.IsTrue(TownKeyartGeometry.ClearPoint(portal,TownCityLayout.AllBuildings,.5f),portal.ToString());
                Assert.IsFalse(float.IsInfinity(TownAccessMetrics.ShortestPathLength(TownAccessMetrics.PlazaAccessPoint,portal,TownCityLayout.AllRoads,.5f)),portal.ToString());
            }
        }

        [Test]
        public void ShortestPath_UsesConnectedCorridorDistanceAndRejectsNarrowOrDisconnectedShortcuts()
        {
            var elbow=new[]{new TownRoadSegment("a",Vector2.zero,new Vector2(10,0),2.4f),new TownRoadSegment("b",new Vector2(10,0),new Vector2(10,10),2.4f)};
            Assert.AreEqual(20,TownAccessMetrics.ShortestPathLength(Vector2.zero,new Vector2(10,10),elbow,.5f),Epsilon);
            var crossing=new[]{new TownRoadSegment("long_horizontal",Vector2.zero,new Vector2(100,0),2.4f),new TownRoadSegment("long_vertical",new Vector2(10,-50),new Vector2(10,50),2.4f)};
            Assert.AreEqual(2,TownAccessMetrics.ShortestPathLength(new Vector2(9,0),new Vector2(10,1),crossing,.5f),Epsilon,"Crossing must split both roads at their junction, not detour through their centres.");
            var disconnected=new[]{elbow[0],new TownRoadSegment("b",new Vector2(10,3),new Vector2(10,10),2.4f)};
            Assert.IsTrue(float.IsInfinity(TownAccessMetrics.ShortestPathLength(Vector2.zero,new Vector2(10,10),disconnected,.5f)));
            var tooNarrow=new[]{new TownRoadSegment("narrow",Vector2.zero,new Vector2(10,0),.9f)};
            Assert.IsTrue(float.IsInfinity(TownAccessMetrics.ShortestPathLength(Vector2.zero,new Vector2(10,0),tooNarrow,.5f)));
        }

        [Test]
        public void ApronAndOpaqueChecks_RejectObstructionsWithoutTreatingTouchingEdgesAsOverlap()
        {
            var owner=new TownBuildingLot("owner","test",new Vector2(10,20),new Vector2(8,8),TownDoorSide.East,TownBuildingArchetype.Residential,Color.white);
            var apron=TownAccessMetrics.DoorApron(owner);
            Assert.AreEqual(new Rect(14,18,3,4),apron);
            Assert.IsTrue(TownAccessMetrics.ClearsApron(apron,new[]{owner}));
            var blocker=new TownBuildingLot("obstacle","test",new Vector2(16,20),new Vector2(1,1),TownDoorSide.South,TownBuildingArchetype.Residential,Color.white);
            Assert.IsFalse(TownAccessMetrics.ClearsApron(apron,new[]{owner,blocker}));
            var target=new Rect(0,0,10,10);var door=new Rect(4,-2,2,2);
            Assert.IsFalse(TownAccessMetrics.ObscuresCriticalFacade(new Rect(9.5f,0,2,10),target,door));
            Assert.IsTrue(TownAccessMetrics.ObscuresCriticalFacade(new Rect(9.4f,0,2,10),target,door));
        }
    }
}
