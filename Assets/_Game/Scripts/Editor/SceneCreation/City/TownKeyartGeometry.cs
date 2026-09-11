using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>One Editor geometry contract for swept streets, visible banks and physical water.</summary>
    public static class TownKeyartGeometry
    {
        public static readonly Vector2 HallFootprintCenter = new Vector2(43.5f,22f);
        public static readonly Vector2 HallFootprintSize = new Vector2(14f,7f);
        public static readonly Vector2[] CaveRockCenters = {new Vector2(73.3333f,-5.7067f),new Vector2(78.6667f,-5.7067f)};
        public static readonly Vector2 CaveRockSize = new Vector2(1.6f,2.4f);
        public static readonly Vector2 DockCenter = new Vector2(-57.6f,-35.8222f);
        public static readonly Vector2 WestArrival = new Vector2(-77.3333f,-3.2178f);
        public static readonly Vector2 EastArrival = new Vector2(76f,-5.9556f);
        public static readonly Vector2[] LakeOutline = BuildLakeOutline();
        // Material pockets are broad visual ground breaks, not physical zones. Their area and
        // shape are part of the composition contract so a later scene pass cannot regress to a
        // nearly uniform grass field while merely increasing prop counts.
        public static readonly Vector2[][] MaterialPocketPolygons = BuildMaterialPockets();
        public static readonly Vector2[] RiverSolidIntervals = { new Vector2(-34.5778f,-5.3333f),new Vector2(-1.1022f,19.5556f),new Vector2(23.7867f,46.3111f) };
        public static readonly float[] BridgeCentersY = { -3.2178f,21.6711f };
        private static readonly Vector2[] RiverControls = { new Vector2(-72f,-34.5778f),new Vector2(-72f,-22.1333f),new Vector2(-74.6667f,-10.9333f),
            new Vector2(-70.6667f,-0.9778f),new Vector2(-68f,8.9778f),new Vector2(-73.3333f,21.4222f),new Vector2(-66.6667f,33.8667f),new Vector2(-70.6667f,46.3111f) };
        private static Vector2[][] s_water, s_gardens;
        private static Vector2[][] WaterPolygons
        {
            get
            {
                if(s_water!=null)return s_water;
                s_water=new Vector2[4][];s_water[0]=LakeOutline;
                for(int i=0;i<3;i++)s_water[i+1]=RiverPolygon(RiverSolidIntervals[i].x,RiverSolidIntervals[i].y);
                return s_water;
            }
        }
        public static Vector2[][] PlazaGardenPolygons
        {
            get
            {
                if(s_gardens!=null)return s_gardens;
                s_gardens=new Vector2[4][];
                for(int q=0;q<4;q++)
                {
                    var p=new List<Vector2>();
                    for(int i=0;i<=24;i++)p.Add(PlazaArc(q*90+15+i*2.5f,13,9.25f));
                    for(int i=24;i>=0;i--)p.Add(PlazaArc(q*90+15+i*2.5f,10.5f,7.2f));
                    s_gardens[q]=p.ToArray();
                }
                return s_gardens;
            }
        }
        public static Vector2 PlazaArc(float degrees,float rx,float ry)
        {
            float a=degrees*Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a)*rx,4+Mathf.Sin(a)*ry);
        }
        public static float RiverCenter(float y)
        {
            for(int i=0;i<RiverControls.Length-1;i++)
                if(y<=RiverControls[i+1].y)
                    return Catmull(RiverControls[Mathf.Max(0,i-1)],RiverControls[i],RiverControls[i+1],
                        RiverControls[Mathf.Min(RiverControls.Length-1,i+2)],
                        Mathf.Clamp01((y-RiverControls[i].y)/(RiverControls[i+1].y-RiverControls[i].y))).x;
            return RiverControls[RiverControls.Length-1].x;
        }
        public static float RiverHalfWidth(float y) => (2.1f+.65f*(1+Mathf.Sin(((y-4f)*45f/56f+4f)*.22f+1))*.5f)*4f/3f;
        public static float RiverEastEdge(float y) => RiverCenter(y)+RiverHalfWidth(y);
        public static float RiverWestEdge(float y) => RiverCenter(y)-RiverHalfWidth(y);
        public static Vector2[] RiverPolygon(float bottom,float top)
        {
            var p=new List<Vector2>();
            for(float y=bottom;y<top;y+=.5f)p.Add(new Vector2(RiverWestEdge(y),y));
            p.Add(new Vector2(RiverWestEdge(top),top));
            for(float y=top;y>bottom;y-=.5f)p.Add(new Vector2(RiverEastEdge(y),y));
            p.Add(new Vector2(RiverEastEdge(bottom),bottom));return p.ToArray();
        }
        private static Vector2[] BuildLakeOutline()
        {
            var p=SampleCurve(new[]{new Vector2(-79.3333f,-18.4f),new Vector2(-72f,-20.8889f),new Vector2(-69.6f,-28.9778f),
                new Vector2(-69.0667f,-33.0844f),new Vector2(-63.3333f,-33.7067f),new Vector2(-60.5333f,-33.8311f)},.35f);
            // The local dock opening stays rectangular; the surrounding natural bank is curved.
            p.Add(new Vector2(-60.5333f,-38.3111f));p.Add(new Vector2(-54.6667f,-38.3111f));p.Add(new Vector2(-54.6667f,-34.08f));
            var east=SampleCurve(new[]{new Vector2(-54.6667f,-34.08f),new Vector2(-49.3333f,-34.3289f),new Vector2(-44.6667f,-35.5733f),
                new Vector2(-40.6667f,-40.8f),new Vector2(-42f,-47.0222f),new Vector2(-53.3333f,-52f),new Vector2(-69.3333f,-52f),new Vector2(-79.3333f,-45.7778f)},.35f);
            east.RemoveAt(0);p.AddRange(east);
            return p.ToArray();
        }

        private static Vector2[][] BuildMaterialPockets() => new[]
        {
            new[] { new Vector2(-67,18), new Vector2(-57,20), new Vector2(-55,26), new Vector2(-62,29), new Vector2(-70,25) },
            new[] { new Vector2(-24,14), new Vector2(-16,15), new Vector2(-14,20), new Vector2(-20,23), new Vector2(-27,19) },
            new[] { new Vector2(22,30), new Vector2(34,29), new Vector2(37,35), new Vector2(31,40), new Vector2(22,38) },
            new[] { new Vector2(67,8), new Vector2(75,10), new Vector2(76,17), new Vector2(70,20), new Vector2(64,15) },
            new[] { new Vector2(-43,-9), new Vector2(-34,-8), new Vector2(-31,-3), new Vector2(-36,1), new Vector2(-45,-2) },
            new[] { new Vector2(12,-34), new Vector2(21,-35), new Vector2(25,-29), new Vector2(19,-25), new Vector2(11,-28) },
            new[] { new Vector2(31,-47), new Vector2(42,-48), new Vector2(46,-43), new Vector2(40,-38), new Vector2(31,-40) },
            new[] { new Vector2(65,-33), new Vector2(74,-31), new Vector2(76,-24), new Vector2(69,-21), new Vector2(63,-26) },
        };

        public static float PolygonArea(IReadOnlyList<Vector2> polygon)
        {
            float area = 0f;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                area += polygon[j].x * polygon[i].y - polygon[i].x * polygon[j].y;
            return Mathf.Abs(area) * .5f;
        }

        public static float MaterialPocketArea
        {
            get { float area = 0f; foreach (var polygon in MaterialPocketPolygons) area += PolygonArea(polygon); return area; }
        }

        /// <summary>Counts only cells that the scene painter is allowed to materialize.</summary>
        public static float EffectiveMaterialPocketArea(float cellSize = .5f, float lotPadding = .35f, float roadPadding = .45f)
        {
            float area = 0f;
            foreach (var polygon in MaterialPocketPolygons)
                area += EffectiveMaterialPocketArea(polygon, cellSize, lotPadding, roadPadding);
            return area;
        }

        public static float EffectiveMaterialPocketArea(IReadOnlyList<Vector2> polygon, float cellSize = .5f,
            float lotPadding = .35f, float roadPadding = .45f)
        {
            if (polygon == null || polygon.Count == 0) return 0f;
            GetBounds(polygon, out float minX, out float maxX, out float minY, out float maxY);
            float area = 0f;
            for (float x = minX; x <= maxX; x += cellSize)
            for (float y = minY; y <= maxY; y += cellSize)
            {
                var point = new Vector2(x + cellSize * .5f, y + cellSize * .5f);
                if (MaterialPocketPointAllowed(point, polygon, lotPadding, roadPadding)) area += cellSize * cellSize;
            }
            return area;
        }

        public static bool MaterialPocketPointAllowed(Vector2 point, IReadOnlyList<Vector2> polygon,
            float lotPadding = .35f, float roadPadding = .45f)
        {
            return ContainsPolygon(point, polygon) && ClearPoint(point, TownCityLayout.AllBuildings, lotPadding) &&
                !TownCityLayout.IsPointOnRoad(point, roadPadding);
        }

        public static bool TryFindMaterialPocketSupport(int index, out Vector2 support,
            float cellSize = .5f, float lotPadding = .35f, float roadPadding = .45f)
        {
            support = default;
            if (index < 0 || index >= MaterialPocketPolygons.Length) return false;
            var polygon = MaterialPocketPolygons[index];
            GetBounds(polygon, out float minX, out float maxX, out float minY, out float maxY);
            for (float x = minX; x <= maxX; x += cellSize)
            for (float y = minY; y <= maxY; y += cellSize)
            {
                var point = new Vector2(x + cellSize * .5f, y + cellSize * .5f);
                if (!MaterialPocketPointAllowed(point, polygon, lotPadding, roadPadding)) continue;
                support = point; return true;
            }
            return false;
        }

        private static void GetBounds(IReadOnlyList<Vector2> polygon, out float minX, out float maxX, out float minY, out float maxY)
        {
            minX = maxX = polygon[0].x; minY = maxY = polygon[0].y;
            for (int i = 1; i < polygon.Count; i++)
            {
                minX = Mathf.Min(minX, polygon[i].x); maxX = Mathf.Max(maxX, polygon[i].x);
                minY = Mathf.Min(minY, polygon[i].y); maxY = Mathf.Max(maxY, polygon[i].y);
            }
        }

        public static float LakeBoundaryRadialVariation()
        {
            Vector2 centroid = Vector2.zero;
            foreach (var p in LakeOutline) centroid += p;
            centroid /= LakeOutline.Length;
            float mean = 0f;
            foreach (var p in LakeOutline) mean += Vector2.Distance(centroid, p);
            mean /= LakeOutline.Length;
            if (mean < .001f) return 0f;
            float variance = 0f;
            foreach (var p in LakeOutline) { float d = Vector2.Distance(centroid, p) - mean; variance += d * d; }
            return Mathf.Sqrt(variance / LakeOutline.Length) / mean;
        }

        public static float RoadCurvatureRatio(IReadOnlyList<TownRoadSegment> roads)
        {
            var groups = new Dictionary<string, List<TownRoadSegment>>(StringComparer.Ordinal);
            foreach (var road in roads)
            {
                int separator = road.Id.LastIndexOf('_');
                string prefix = separator > 0 ? road.Id.Substring(0, separator) : road.Id;
                if (!groups.TryGetValue(prefix, out var group)) groups.Add(prefix, group = new List<TownRoadSegment>());
                group.Add(road);
            }
            int joins = 0, turns = 0;
            foreach (var group in groups.Values)
            {
                group.Sort((a, b) => SegmentIndex(a.Id).CompareTo(SegmentIndex(b.Id)));
                for (int i = 1; i < group.Count; i++)
                {
                    var previous = group[i - 1]; var current = group[i];
                    var a = previous.End - previous.Start; var b = current.End - current.Start;
                    if (a.sqrMagnitude < .01f || b.sqrMagnitude < .01f) continue;
                    // Curved authored roads are sampled densely; a small but repeated heading
                    // change is meaningful even when no join is a sharp corner.
                    joins++; if (Vector2.Angle(a, b) > 1.5f) turns++;
                }
            }
            return joins == 0 ? 0f : turns / (float)joins;
        }

        private static int SegmentIndex(string id)
        {
            int separator = id.LastIndexOf('_');
            return separator >= 0 && int.TryParse(id.Substring(separator + 1), out var index) ? index : int.MaxValue;
        }
        public static bool ContainsWater(Vector2 p,float padding=0)
        {
            if(p.x>-36-padding)return false;
            foreach(var polygon in WaterPolygons)
                if(ContainsPolygon(p,polygon)||PolygonDistance(p,p,polygon)<padding-.0001f)return true;
            return false;
        }
        public static bool ContainsPolygon(Vector2 p,IReadOnlyList<Vector2> polygon)
        {
            bool inside=false;
            for(int i=0,j=polygon.Count-1;i<polygon.Count;j=i++)
            {var a=polygon[i];var b=polygon[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;}
            return inside;
        }
        public static float PointSegmentDistance(Vector2 p,Vector2 a,Vector2 b)
        {
            var d=b-a;if(d.sqrMagnitude<.000001f)return Vector2.Distance(p,a);
            return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));
        }
        public static float SegmentDistance(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            float Cross(Vector2 u,Vector2 v)=>u.x*v.y-u.y*v.x;
            float denominator=Cross(b-a,d-c);
            if(Mathf.Abs(denominator)>.000001f)
            {
                float t=Cross(c-a,d-c)/denominator,u=Cross(c-a,b-a)/denominator;
                if(t>=0&&t<=1&&u>=0&&u<=1)return 0;
            }
            return Mathf.Min(Mathf.Min(PointSegmentDistance(a,c,d),PointSegmentDistance(b,c,d)),
                Mathf.Min(PointSegmentDistance(c,a,b),PointSegmentDistance(d,a,b)));
        }
        public static float SegmentRectDistance(Vector2 a,Vector2 b,Vector2 center,Vector2 size)
        {
            Vector2 lo=center-size*.5f,hi=center+size*.5f;
            bool Inside(Vector2 p)=>p.x>=lo.x&&p.x<=hi.x&&p.y>=lo.y&&p.y<=hi.y;
            if(Inside(a)||Inside(b))return 0;
            var nw=new Vector2(lo.x,hi.y);var se=new Vector2(hi.x,lo.y);
            return Mathf.Min(Mathf.Min(SegmentDistance(a,b,lo,nw),SegmentDistance(a,b,nw,hi)),
                Mathf.Min(SegmentDistance(a,b,hi,se),SegmentDistance(a,b,se,lo)));
        }
        private static float PolygonDistance(Vector2 a,Vector2 b,IReadOnlyList<Vector2> polygon)
        {
            if(ContainsPolygon(a,polygon)||ContainsPolygon(b,polygon))return 0;
            float result=float.MaxValue;
            for(int i=0,j=polygon.Count-1;i<polygon.Count;j=i++)result=Mathf.Min(result,SegmentDistance(a,b,polygon[i],polygon[j]));
            return result;
        }
        public static bool ClearPoint(Vector2 p,IReadOnlyList<TownBuildingLot> lots,float padding)
        {
            if(Mathf.Abs(p.x)>TownDistrictLayout.HalfWidth-.5f-padding||Mathf.Abs(p.y)>TownDistrictLayout.HalfHeight-1.3f-padding||ContainsWater(p,padding))return false;
            foreach(var center in CaveRockCenters)if(SegmentRectDistance(p,p,center,CaveRockSize)<padding-.0001f)return false;
            foreach(var lot in lots)if(SegmentRectDistance(p,p,lot.Center,lot.Size)<padding-.0001f ||
                p.x>lot.MinX&&p.x<lot.MaxX&&p.y>lot.MinY&&p.y<lot.MaxY)return false;
            return !(SegmentRectDistance(p,p,HallFootprintCenter,HallFootprintSize)<padding-.0001f ||
                Mathf.Abs(p.x-HallFootprintCenter.x)<HallFootprintSize.x*.5f&&Mathf.Abs(p.y-HallFootprintCenter.y)<HallFootprintSize.y*.5f);
        }
        public static bool ClearRoute(Vector2 a,Vector2 b,float radius,IReadOnlyList<TownBuildingLot> lots)
        {
            if(!ClearPoint(a,lots,radius)||!ClearPoint(b,lots,radius))return false;
            foreach(var center in CaveRockCenters)if(SegmentRectDistance(a,b,center,CaveRockSize)<radius-.0001f)return false;
            foreach(var lot in lots)if(SegmentRectDistance(a,b,lot.Center,lot.Size)<radius-.0001f)return false;
            if(SegmentRectDistance(a,b,HallFootprintCenter,HallFootprintSize)<radius-.0001f)return false;
            if(Mathf.Min(a.x,b.x)<-36-radius)
                foreach(var p in WaterPolygons)if(PolygonDistance(a,b,p)<radius-.0001f)return false;
            if(Mathf.Min(a.x,b.x)<15+radius&&Mathf.Max(a.x,b.x)>-15-radius&&Mathf.Min(a.y,b.y)<16+radius&&Mathf.Max(a.y,b.y)>-8-radius)
            {
                if(SegmentRectDistance(a,b,new Vector2(0,4),new Vector2(8,3.2f))<radius-.0001f)return false;
                foreach(var p in PlazaGardenPolygons)if(PolygonDistance(a,b,p)<radius-.0001f)return false;
            }
            return true;
        }
        public static bool RoadClearsWater(TownRoadSegment road)
        {
            if(Mathf.Min(road.Start.x,road.End.x)>-36-road.Width*.5f)return true;
            foreach(var p in WaterPolygons)if(PolygonDistance(road.Start,road.End,p)<road.Width*.5f-.0001f)return false;
            return true;
        }
        public static TownRoadSegment[] BuildRoads(IReadOnlyList<TownBuildingLot> lots)
        {
            var roads=new List<TownRoadSegment>();
            AddCurve(roads,lots,"main_south",TownRoadClass.Main,P(0f,-52.5f,1.3333f,-45.7778f,-2.6667f,-37.0667f,-1.3333f,-27.1111f,1.3333f,-18.4f,0f,-10.9333f,0f,-2.2222f));
            // A real ring joins the four entries without crossing the fountain or raised gardens.
            var ring=new List<Vector2>();for(int i=0;i<=96;i++)ring.Add(PlazaArc(i*360f/96,8,5));
            AddSegments(roads,lots,"plaza_inner",TownAccessMetrics.MainRoadMinWidth,ring);
            AddCurve(roads,lots,"main_north",TownRoadClass.Main,P(0f,10.2222f,0f,20.1778f,-4f,27.6444f,0f,36.3556f,0f,50.0444f));
            AddCurve(roads,lots,"market",TownRoadClass.District,P(-8f,4f,-18.6667f,4f,-24f,8.9778f,-33.3333f,11.0933f,-41.3333f,11.0933f,-46.6667f,7.7333f,-42f,4f,-41.0667f,-4.7111f,-41.0667f,-9.6889f,-41.3333f,-12.1778f,-42.6667f,-14.6667f,-45.3333f,-17.1556f,-45.3333f,-27.1111f,-38.6667f,-30.8444f,-30.6667f,-30.8444f,-24f,-33.3333f,-13.3333f,-34.5778f,-2.6667f,-37.0667f));
            AddCurve(roads,lots,"craft",TownRoadClass.District,P(8,4,16,4,18.5f,1,18.5f,-3.5f,25,-3.5f,33,-3.5f,40,-5,44,-9,44,-17,44.3f,-22,44.3f,-30,44.3f,-37,38.5f,-37,30.5f,-37,24,-32,18.5f,-29.5f,9.5f,-29.5f,-1.3333f,-27.1111f));
            AddCurve(roads,lots,"east_gate",TownRoadClass.District,P(44f,-7.2f,52f,-6.2667f,56f,-6.2667f,61.3333f,-6.2667f,62.6667f,-6.2667f,64f,-6.3289f,65.3333f,-7.2f,68f,-9.6889f,74f,-10.9333f,76f,-9.6889f,76f,-8.4444f,76f,-5.9556f));
            AddCurve(roads,lots,"west_bridge_low",TownRoadClass.District,P(-42f,4f,-50.6667f,6.4889f,-60f,5.2444f,-63.6f,2.7556f,-64.2133f,0.7644f,-64.6667f,-0.9778f,-67.7333f,-3.2178f,-77.3333f,-3.2178f));
            AddCurve(roads,lots,"west_bridge_high",TownRoadClass.District,P(-46.6667f,7.7333f,-48f,13.9556f,-49.3333f,23.9111f,-58.6667f,26.4f,-65.3333f,25.1556f,-67.3333f,21.6711f,-77.3333f,21.6711f));
            // Cross-district walking link prevents the southeast yard from requiring the full craft loop.
            AddCurve(roads,lots,"residential_cross",TownRoadClass.District,P(0,-18.4f,5,-15,9,-13,19,-13,22,-17.5f,28,-18,38,-18,44,-17));
            foreach(var lot in lots)roads.AddRange(BuildDoorConnection(lot.DoorApproach,roads,lots,"door_"+lot.Name));
            foreach(var npc in TownCityLayout.AllNpcPlaces)roads.AddRange(BuildDoorConnection(npc.Work,roads,lots,"work_"+npc.NpcId));
            roads.AddRange(BuildDoorConnection(TownDistrictLayout.TownHallMural,roads,lots,"public_hall"));
            roads.AddRange(BuildDoorConnection(DockCenter,roads,lots,"public_dock"));
            return roads.ToArray();
        }
        public static TownRoadSegment[] BuildDoorConnection(Vector2 approach,IReadOnlyList<TownRoadSegment> network,
            IReadOnlyList<TownBuildingLot> lots,string id)
        {
            var roads=new List<TownRoadSegment>();
            float radius=TownAccessMetrics.LocalRoadMinWidth*.5f;
            AddSegments(roads,lots,id,TownAccessMetrics.LocalRoadMinWidth,SmoothConnection(FindConnection(approach,network,lots,radius),lots,radius));
            return roads.ToArray();
        }
        private static Vector2[] P(params float[] xy)
        {var p=new Vector2[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new Vector2(xy[i*2],xy[i*2+1]);return p;}
        private static Vector2 Catmull(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t)=>
            .5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);
        private static List<Vector2> SampleCurve(IReadOnlyList<Vector2> p,float step=.25f)
        {
            var result=new List<Vector2>();
            for(int i=0;i<p.Count-1;i++)
            {
                int count=Mathf.Max(2,Mathf.CeilToInt(Vector2.Distance(p[i],p[i+1])/step));
                for(int j=0;j<count;j++)result.Add(Catmull(p[Mathf.Max(0,i-1)],p[i],p[i+1],p[Mathf.Min(p.Count-1,i+2)],j/(float)count));
            }
            result.Add(p[p.Count-1]);return result;
        }
        private static void AddCurve(List<TownRoadSegment> roads,IReadOnlyList<TownBuildingLot> lots,string id,TownRoadClass roadClass,Vector2[] controls)
        {
            AddSegments(roads,lots,id,TownAccessMetrics.Width(roadClass),SampleCurve(controls));
        }
        private static void AddSegments(List<TownRoadSegment> roads,IReadOnlyList<TownBuildingLot> lots,string id,float width,IReadOnlyList<Vector2> points)
        {
            if(points.Count==1)
            {
                if(!ClearRoute(points[0],points[0],width*.5f,lots))throw new InvalidOperationException("Town approach blocked: "+id);
                roads.Add(new TownRoadSegment(id+"_0",points[0],points[0],width));
            }
            for(int i=1;i<points.Count;i++)
            {
                if(!ClearRoute(points[i-1],points[i],width*.5f,lots))throw new InvalidOperationException("Town swept route blocked: "+id+" at "+points[i]);
                roads.Add(new TownRoadSegment(id+"_"+(i-1),points[i-1],points[i],width));
            }
        }
        private static List<Vector2> FindConnection(Vector2 start,IReadOnlyList<TownRoadSegment> roads,IReadOnlyList<TownBuildingLot> lots,float radius)
        {
            bool Reached(Vector2 p)
            {foreach(var r in roads)if(r.Contains(p,-radius+.02f))return true;return false;}
            if(Reached(start))return new List<Vector2>{start};
            var first=new Vector2Int(Mathf.RoundToInt(start.x*2),Mathf.FloorToInt(start.y*2));
            if(!ClearRoute(start,(Vector2)first*.5f,radius,lots))throw new InvalidOperationException("Door approach cannot enter half-unit navigation grid: "+start);
            var queue=new Queue<Vector2Int>();var previous=new Dictionary<Vector2Int,Vector2Int>();
            queue.Enqueue(first);previous.Add(first,first);Vector2Int goal=first;bool found=false;
            var directions=new[]{new Vector2Int(-1,0),new Vector2Int(1,0),new Vector2Int(0,-1),new Vector2Int(0,1),
                new Vector2Int(-1,-1),new Vector2Int(1,-1),new Vector2Int(-1,1),new Vector2Int(1,1)};
            while(queue.Count>0)
            {
                var cell=queue.Dequeue();Vector2 a=(Vector2)cell*.5f;
                if(Reached(a)){goal=cell;found=true;break;}
                foreach(var d in directions)
                {
                    var next=cell+d;if(previous.ContainsKey(next))continue;Vector2 b=(Vector2)next*.5f;
                    // Exact swept disc vs every boundary prevents diagonal corner cutting.
                    if(!ClearRoute(a,b,radius,lots))continue;
                    previous.Add(next,cell);queue.Enqueue(next);
                }
            }
            if(!found)throw new InvalidOperationException("No two-unit Town route from "+start);
            var path=new List<Vector2>();var cursor=goal;
            while(cursor!=first){path.Add((Vector2)cursor*.5f);cursor=previous[cursor];}
            path.Add((Vector2)first*.5f);path.Add(start);path.Reverse();return path;
        }
        private static List<Vector2> SmoothConnection(IReadOnlyList<Vector2> path,IReadOnlyList<TownBuildingLot> lots,float radius)
        {
            var simplified=new List<Vector2>{path[0]};
            for(int i=0;i<path.Count-1;)
            {int next=path.Count-1;while(next>i+1&&!ClearRoute(path[i],path[next],radius,lots))next--;simplified.Add(path[next]);i=next;}
            if(simplified.Count<3)return simplified;
            var smooth=new List<Vector2>{simplified[0]};
            for(int i=1;i<simplified.Count-1;i++)
            {
                Vector2 b=simplified[i],a=Vector2.MoveTowards(b,simplified[i-1],Mathf.Min(.8f,Vector2.Distance(b,simplified[i-1])*.3f)),
                    c=Vector2.MoveTowards(b,simplified[i+1],Mathf.Min(.8f,Vector2.Distance(b,simplified[i+1])*.3f));
                var arc=new List<Vector2>{a};bool clear=ClearRoute(smooth[smooth.Count-1],a,radius,lots);
                for(int j=1;j<=8;j++){float t=j/8f;var p=(1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c;clear&=ClearRoute(arc[arc.Count-1],p,radius,lots);arc.Add(p);}
                if(clear)smooth.AddRange(arc);else smooth.Add(b);
            }
            smooth.Add(simplified[simplified.Count-1]);return smooth;
        }
    }
}
