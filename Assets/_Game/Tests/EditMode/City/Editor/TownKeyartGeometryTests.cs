using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Editor.SceneCreation;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>Pure geometry tests. Scene materialization and PlayMode remain independent gates.</summary>
    [TestFixture]
    public class TownKeyartGeometryTests
    {
        private const float Epsilon=.001f;
        private const float ProbeWidth=1f;

        [TestCase(-60f,-43.2889f,true)]
        [TestCase(-70.6667f,0f,true)]
        [TestCase(-77.3333f,0f,false)]
        [TestCase(0f,0f,false)]
        public void ContainsWater_DistinguishesLakeRiverAndDryTown(float x,float y,bool expected) =>
            Assert.AreEqual(expected,TownKeyartGeometry.ContainsWater(new Vector2(x,y)));

        [Test]
        public void ContainsWater_PaddingRejectsActorWhoseRadiusCrossesRiverBank()
        {
            // Probe normal to a nearly horizontal local bank tangent, away from either bridge.
            var p=new Vector2(TownKeyartGeometry.RiverEastEdge(33.8667f)+.5f,33.8667f);
            Assert.IsFalse(TownKeyartGeometry.ContainsWater(p,.4f));
            Assert.IsTrue(TownKeyartGeometry.ContainsWater(p,.6f));
        }

        [Test]
        public void River_BothBanksBendAcrossTheWesternCorridorAndBridgeDecksRemainDry()
        {
            Assert.Greater(TownKeyartGeometry.RiverCenter(33.8667f)-TownKeyartGeometry.RiverCenter(21.4222f),6.5f);
            Assert.Greater(TownKeyartGeometry.RiverWestEdge(33.8667f)-TownKeyartGeometry.RiverWestEdge(21.4222f),5.3f);
            foreach(float y in TownKeyartGeometry.BridgeCentersY)
                for(float x=TownKeyartGeometry.RiverWestEdge(y);x<=TownKeyartGeometry.RiverEastEdge(y);x+=.1f)
                    Assert.IsFalse(TownKeyartGeometry.ContainsWater(new Vector2(x,y),.5f),"A one-unit actor must fit across the full bridge gap.");
            Assert.IsTrue(TownKeyartGeometry.ContainsWater(new Vector2(TownKeyartGeometry.RiverCenter(24.2f),24.2f)),
                "Water must resume beyond the upper bridge, not be removed with an oversized gap.");
        }

        [Test]
        public void Dock_LocalNotchIsDryButDoesNotEraseAdjacentWater()
        {
            Assert.IsFalse(TownKeyartGeometry.ContainsWater(TownKeyartGeometry.DockCenter,.5f));
            Assert.IsTrue(TownKeyartGeometry.ContainsWater(new Vector2(-61.3333f,-37.0667f)));
            Assert.IsTrue(TownKeyartGeometry.ContainsWater(new Vector2(-53.3333f,-37.0667f)));
        }

        [Test]
        public void ClearPoint_CorridorRejectsProbeWiderThanGap()
        {
            var lots=new[]{Lot("west",9,0,4,4),Lot("east",15,0,4,4)};
            Assert.IsTrue(TownKeyartGeometry.ClearPoint(new Vector2(12,0),lots,.9f));
            Assert.IsFalse(TownKeyartGeometry.ClearPoint(new Vector2(12,0),lots,1.1f));
        }

        [Test]
        public void ClearRoute_DiagonalCannotCutAnObstacleCornerEvenWithDryEndpoints()
        {
            var lots=new[]{Lot("corner",10,-20,2,2)};
            Vector2 a=new Vector2(7,-20),b=new Vector2(10,-17);
            Assert.IsTrue(TownKeyartGeometry.ClearPoint(a,lots,1));
            Assert.IsTrue(TownKeyartGeometry.ClearPoint(b,lots,1));
            Assert.IsFalse(TownKeyartGeometry.ClearRoute(a,b,1,lots));
            Assert.IsTrue(TownKeyartGeometry.ClearRoute(new Vector2(7,-17),b,1,lots));
        }

        [Test]
        public void SweptRoad_DoesNotTreatItsBoundingRectangleAsWalkable()
        {
            var road=new TownRoadSegment("diagonal",new Vector2(0,0),new Vector2(10,10),2);
            Assert.IsTrue(road.Contains(new Vector2(5,5)));
            Assert.IsFalse(road.Contains(new Vector2(0,10)),"The old AABB would falsely claim this corner as street.");
            Assert.AreEqual(1,TownKeyartGeometry.SegmentRectDistance(new Vector2(-3,0),new Vector2(-2,0),Vector2.zero,new Vector2(2,2)),Epsilon);
        }

        [Test]
        public void ClearPoint_TownHallBlocksItsInteriorAndProbeClearance()
        {
            var lots=new TownBuildingLot[0];var c=TownKeyartGeometry.HallFootprintCenter;
            var outside=c+Vector2.right*(TownKeyartGeometry.HallFootprintSize.x*.5f+1);
            Assert.IsFalse(TownKeyartGeometry.ClearPoint(c,lots,0));
            Assert.IsTrue(TownKeyartGeometry.ClearPoint(outside,lots,0));
            Assert.IsFalse(TownKeyartGeometry.ClearPoint(outside,lots,2));
        }

        [Test]
        public void DoorConnection_ObstacleDetourKeepsTheEntireWidthOutsideFootprints()
        {
            var lots=new[]{Lot("destination",12,-20,4,4),Lot("obstacle",6,-24,3,4)};
            var roads=BuildTestNetwork(lots);
            Assert.Greater(roads.Count,2);
            AssertRoadsOutsideLots(roads,lots);
            AssertApproachesReachMainNetwork(roads,lots);
        }

        [Test]
        public void DoorConnection_SameInputsProduceSameIdsAndGeometry()
        {
            var lots=new[]{Lot("destination",12,-20,4,4),Lot("obstacle",6,-24,3,4)};
            var first=BuildTestNetwork(lots);var second=BuildTestNetwork(lots);
            Assert.AreEqual(first.Count,second.Count);
            for(int i=0;i<first.Count;i++)
            {
                Assert.AreEqual(first[i].Id,second[i].Id);Assert.AreEqual(first[i].Start,second[i].Start);
                Assert.AreEqual(first[i].End,second[i].End);Assert.AreEqual(first[i].Width,second[i].Width);
            }
        }

        [Test]
        public void SouthGateRoute_AuthoredBendIsPresentInTheActualFourUnitStreet()
        {
            float minX=float.MaxValue,maxX=float.MinValue;int samples=0;
            foreach(var road in TownCityLayout.AllRoads)
            {
                if(!road.Id.StartsWith("main_south_"))continue;
                minX=Mathf.Min(minX,road.Center.x);maxX=Mathf.Max(maxX,road.Center.x);samples++;
                Assert.GreaterOrEqual(road.Width,4);
                Assert.LessOrEqual(Vector2.Distance(road.Start,road.End),.4f);
            }
            Assert.Greater(samples,20);
            Assert.Less(minX,-1.9f);Assert.Greater(maxX,.95f);
            Assert.IsFalse(TownCityLayout.IsPointOnRoad(new Vector3(4.5f,-37f),0),
                "The old six-unit straight avenue must not remain as an invisible logical road beside the authored bend.");
        }

        [Test]
        public void TownRoutes_FullSweptWidthsAvoidWaterTownHallFountainAndGardenSolids()
        {
            foreach(var road in TownCityLayout.AllRoads)
            {
                Assert.IsTrue(TownKeyartGeometry.RoadClearsWater(road),road.Id);
                Assert.IsTrue(TownKeyartGeometry.ClearRoute(road.Start,road.End,road.Width*.5f,TownCityLayout.AllBuildings),road.Id);
                // Independently sample the tube perimeter, not just center or an AABB.
                for(int step=0;step<=4;step++)
                for(int angle=0;angle<16;angle++)
                {
                    float a=angle*Mathf.PI*2/16;
                    var p=Vector2.Lerp(road.Start,road.End,step/4f)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(road.Width*.5f-Epsilon);
                    Assert.IsFalse(TownKeyartGeometry.ContainsWater(p),road.Id+" wet perimeter "+p);
                }
            }
        }

        [Test]
        public void CompositionMaterialPocketsHaveMeaningfulCoverageAndStayInsideTown()
        {
            Assert.GreaterOrEqual(TownKeyartGeometry.MaterialPocketPolygons.Length, 8,
                "Material composition must define spatial clusters, not rely on prop count.");
            var nominal = TownKeyartGeometry.MaterialPocketArea;
            var effective = TownKeyartGeometry.EffectiveMaterialPocketArea();
            Assert.Greater(effective, 180f,
                "Effective painted ground must cover a visible fraction after road/water/lot filters.");
            Assert.LessOrEqual(effective, nominal + Epsilon);
            foreach (var polygon in TownKeyartGeometry.MaterialPocketPolygons)
            {
                Assert.Greater(TownKeyartGeometry.PolygonArea(polygon), 20f);
                foreach (var point in polygon)
                {
                    Assert.Less(Mathf.Abs(point.x), TownDistrictLayout.HalfWidth);
                    Assert.Less(Mathf.Abs(point.y), TownDistrictLayout.HalfHeight);
                }
                Assert.Greater(TownKeyartGeometry.EffectiveMaterialPocketArea(polygon), 0f);
            }
        }

        [Test]
        public void MaterialPocketPaintingUsesEffectiveCellsAndGateKeeperPocketIsFiltered()
        {
            Assert.IsTrue(TownCityLayout.TryGetBuilding("House_GateKeeper", out var gateKeeper));
            var polygon = TownKeyartGeometry.MaterialPocketPolygons[6];
            int accepted = 0, rejected = 0;
            for (float x = 31f; x <= 46f; x += .5f)
            for (float y = -47f; y <= -38f; y += .5f)
            {
                var point = new Vector2(x + .25f, y + .25f);
                if (!TownKeyartGeometry.ContainsPolygon(point, polygon)) continue;
                if (TownKeyartGeometry.MaterialPocketPointAllowed(point, polygon)) accepted++; else rejected++;
            }
            Assert.Greater(accepted, 0, "GateKeeper pocket must retain some safe material cells.");
            Assert.Greater(rejected, 0, "GateKeeper pocket must prove lot/route filtering, not nominal painting.");
            Assert.Less(TownKeyartGeometry.EffectiveMaterialPocketArea(polygon),
                TownKeyartGeometry.PolygonArea(polygon) * .9f);
            Assert.IsFalse(TownKeyartGeometry.ClearPoint(gateKeeper.Center, TownCityLayout.AllBuildings, .35f));
        }

        [Test]
        public void CompositionRoadNetworkHasCurvedVisualTurns()
        {
            Assert.Greater(TownKeyartGeometry.RoadCurvatureRatio(TownCityLayout.AllRoads), .20f,
                "A material Town pass must retain curved/radial path geometry; straight segment count alone is insufficient.");
        }

        [Test]
        public void CompositionLakeBoundaryIsIrregularRatherThanRectangular()
        {
            Assert.Greater(TownKeyartGeometry.LakeBoundaryRadialVariation(), .06f,
                "Lake edge must carry authored radial variation instead of a rectangle or uniform ellipse.");
        }

        [Test]
        public void TownRoutes_OneUnitProbeReachesEveryApproachWithoutPointOnlyLinks()
        {
            AssertRoadsOutsideLots(TownCityLayout.AllRoads,TownCityLayout.AllBuildings);
            AssertApproachesReachMainNetwork(TownCityLayout.AllRoads,TownCityLayout.AllBuildings);
        }

        [Test]
        public void GateKeeper_OneTileWallsLeaveRoomForOneUnitProbe()
        {
            Assert.IsTrue(TownCityLayout.TryGetBuilding("House_GateKeeper",out var lot));
            var interior=lot.Size-new Vector2(2,2);
            Assert.GreaterOrEqual(interior.x,ProbeWidth);Assert.GreaterOrEqual(interior.y,ProbeWidth);
        }

        [Test]
        public void Mill_AsymmetricDoorLeavesWallAndFrameRoomOnBothSides()
        {
            Assert.IsTrue(TownCityLayout.TryGetBuilding("House_Fishery",out var lot));
            Assert.AreEqual(-2,lot.DoorTangentOffset);
            Assert.AreEqual(lot.Center.x-2,lot.DoorPosition.x,Epsilon);
            Assert.GreaterOrEqual(lot.DoorPosition.x-lot.MinX,1.5f);
            Assert.GreaterOrEqual(lot.MaxX-lot.DoorPosition.x,1.5f);
            Assert.GreaterOrEqual(lot.Size.x-2,6);Assert.GreaterOrEqual(lot.Size.y-2,5);
        }

        [Test]
        public void Orlan_WorkAnchorProbeStaysOutsideSolidBuildingFootprints()
        {
            bool found=false;
            foreach(var place in TownCityLayout.AllNpcPlaces)
            {
                if(place.NpcId!="npc_orlan")continue;found=true;
                foreach(var lot in TownCityLayout.AllBuildings)
                    Assert.GreaterOrEqual(TownKeyartGeometry.SegmentRectDistance(place.Work,place.Work,lot.Center,lot.Size),.5f,lot.Name);
            }
            Assert.IsTrue(found);
        }

        private static List<TownRoadSegment> BuildTestNetwork(IReadOnlyList<TownBuildingLot> lots)
        {
            var roads=new List<TownRoadSegment>{new TownRoadSegment("main_south_0",new Vector2(0,-40),new Vector2(0,-10),3)};
            foreach(var lot in lots)roads.AddRange(TownKeyartGeometry.BuildDoorConnection(lot.DoorApproach,roads,lots,"door_"+lot.Name));
            return roads;
        }
        private static void AssertRoadsOutsideLots(IReadOnlyList<TownRoadSegment> roads,IReadOnlyList<TownBuildingLot> lots)
        {
            foreach(var road in roads)foreach(var lot in lots)
                Assert.GreaterOrEqual(TownKeyartGeometry.SegmentRectDistance(road.Start,road.End,lot.Center,lot.Size),
                    road.Width*.5f-Epsilon,road.Id+" penetrates "+lot.Name);
        }
        private static void AssertApproachesReachMainNetwork(IReadOnlyList<TownRoadSegment> roads,IReadOnlyList<TownBuildingLot> lots)
        {
            var reached=new HashSet<int>();var queue=new Queue<int>();
            for(int i=0;i<roads.Count;i++)
            {
                Assert.GreaterOrEqual(roads[i].Width,ProbeWidth);
                if(!roads[i].Id.StartsWith("main_south_"))continue;reached.Add(i);queue.Enqueue(i);
            }
            Assert.IsNotEmpty(reached);
            while(queue.Count>0)
            {
                int current=queue.Dequeue();
                for(int i=0;i<roads.Count;i++)
                {
                    if(reached.Contains(i))continue;
                    float actorCenterJoin=(roads[current].Width+roads[i].Width)*.5f-ProbeWidth;
                    if(TownKeyartGeometry.SegmentDistance(roads[current].Start,roads[current].End,roads[i].Start,roads[i].End)>actorCenterJoin+Epsilon)continue;
                    reached.Add(i);queue.Enqueue(i);
                }
            }
            foreach(var lot in lots)
            {
                bool found=false;foreach(int i in reached)if(roads[i].Contains(lot.DoorApproach,-ProbeWidth*.5f)){found=true;break;}
                Assert.IsTrue(found,lot.Name+" lacks a continuous one-unit actor-center corridor.");
            }
        }
        private static TownBuildingLot Lot(string name,float x,float y,float w,float h)=>
            new TownBuildingLot(name,"test",new Vector2(x,y),new Vector2(w,h),TownDoorSide.South,TownBuildingArchetype.Residential,Color.white);
    }
}
