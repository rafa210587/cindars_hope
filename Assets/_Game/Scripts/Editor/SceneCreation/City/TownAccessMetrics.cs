using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    public enum TownRoadClass { Main, District, Local }

    /// <summary>Pure editorial access contracts; no scene mutation, runtime navigation or save state.</summary>
    public static class TownAccessMetrics
    {
        public const float ActorBodyRadius = .35f;
        public const float ComfortProbeRadius = .50f;
        public const float MainRoadMinWidth = 4f;
        public const float DistrictRoadMinWidth = 3.2f;
        public const float LocalRoadMinWidth = 2.4f;
        public static readonly Vector2 DoorApronSize = new Vector2(4f, 3f);
        public const float InteriorLaneWidth = 1.4f;
        public static readonly Vector2 InteriorTurnPocket = new Vector2(2f, 2f);
        public const float BuildingGap = 4f;
        public const float VisualWallMargin = 3f;
        public const float SouthSpawnToPlazaMaxPath = 75f;
        public const float PlazaToPublicDoorMaxPath = 90f;
        public const int MinimumMaterializedTrees = 554;
        // The public plaza destination is the south ring entry, never the occupied fountain basin.
        public static Vector2 PlazaAccessPoint => TownCityLayout.CentralPlazaCenter + Vector2.down * 5f;

        public static float Width(TownRoadClass roadClass) => roadClass == TownRoadClass.Main
            ? MainRoadMinWidth : roadClass == TownRoadClass.District ? DistrictRoadMinWidth : LocalRoadMinWidth;

        public static TownRoadClass RoadClass(string id)
        {
            if (id.StartsWith("main_", StringComparison.Ordinal) || id.StartsWith("plaza_inner", StringComparison.Ordinal))
                return TownRoadClass.Main;
            if (id.StartsWith("door_", StringComparison.Ordinal) || id.StartsWith("work_", StringComparison.Ordinal) ||
                id.StartsWith("public_", StringComparison.Ordinal)) return TownRoadClass.Local;
            return TownRoadClass.District;
        }

        public static Vector2 Outward(TownDoorSide side) => side == TownDoorSide.North ? Vector2.up :
            side == TownDoorSide.East ? Vector2.right : side == TownDoorSide.West ? Vector2.left : Vector2.down;

        public static Rect DoorApron(TownBuildingLot lot)
        {
            Vector2 size = lot.DoorSide == TownDoorSide.East || lot.DoorSide == TownDoorSide.West
                ? new Vector2(DoorApronSize.y, DoorApronSize.x) : DoorApronSize;
            Vector2 center = (Vector2)lot.DoorPosition + Outward(lot.DoorSide) * DoorApronSize.y * .5f;
            return new Rect(center - size * .5f, size);
        }

        public static bool ClearsApron(Rect apron, IReadOnlyList<TownBuildingLot> lots)
        {
            if (apron.xMin < -TownDistrictLayout.HalfWidth || apron.xMax > TownDistrictLayout.HalfWidth ||
                apron.yMin < -TownDistrictLayout.HalfHeight || apron.yMax > TownDistrictLayout.HalfHeight) return false;
            foreach (var lot in lots)
                if (OverlapArea(apron, new Rect(lot.Center - lot.Size * .5f, lot.Size)) > .0001f) return false;
            return OverlapArea(apron, new Rect(TownKeyartGeometry.HallFootprintCenter - TownKeyartGeometry.HallFootprintSize * .5f,
                TownKeyartGeometry.HallFootprintSize)) <= .0001f;
        }

        public static float Gap(TownBuildingLot a, TownBuildingLot b) => RectGap(a.Center, a.Size, b.Center, b.Size);

        public static float RectGap(Vector2 a, Vector2 aSize, Vector2 b, Vector2 bSize)
        {
            float x = Mathf.Max(0, Mathf.Abs(a.x - b.x) - (aSize.x + bSize.x) * .5f);
            float y = Mathf.Max(0, Mathf.Abs(a.y - b.y) - (aSize.y + bSize.y) * .5f);
            return Mathf.Sqrt(x * x + y * y);
        }

        public static float OverlapArea(Rect a, Rect b) => Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin)) *
            Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));

        public static bool ObscuresCriticalFacade(Rect sourceOpaque, Rect targetOpaque, Rect targetDoorAndSteps,
            float maxOpaqueFraction = .05f)
        {
            if (targetOpaque.width <= 0 || targetOpaque.height <= 0 || maxOpaqueFraction < 0) return true;
            return OverlapArea(sourceOpaque, targetDoorAndSteps) > .0001f ||
                OverlapArea(sourceOpaque, targetOpaque) > targetOpaque.width * targetOpaque.height * maxOpaqueFraction + .0001f;
        }

        public static Rect InteriorLane(TownBuildingLot lot)
        {
            var inward = -Outward(lot.DoorSide);
            var size = lot.DoorSide == TownDoorSide.East || lot.DoorSide == TownDoorSide.West
                ? new Vector2(3f, InteriorLaneWidth) : new Vector2(InteriorLaneWidth, 3f);
            return new Rect((Vector2)lot.DoorPosition + inward * 2f - size * .5f, size);
        }

        public static Rect InteriorPocket(TownBuildingLot lot)
        {
            Vector2 center = (Vector2)lot.DoorPosition - Outward(lot.DoorSide) * 3f;
            // Ease asymmetric entrances towards the room centre. This keeps the pocket connected to
            // the entry lane without pressing it against a side wall (notably the fishery mill door).
            if (lot.DoorSide == TownDoorSide.North || lot.DoorSide == TownDoorSide.South)
                center.x = (center.x + lot.Center.x) * .5f;
            else
                center.y = (center.y + lot.Center.y) * .5f;
            return new Rect(center - InteriorTurnPocket * .5f, InteriorTurnPocket);
        }

        private readonly struct Edge
        {
            public readonly int Target;
            public readonly float Length;
            public Edge(int target, float length) { Target = target; Length = length; }
        }

        private sealed class RoadGraph
        {
            public IReadOnlyList<TownRoadSegment> Roads;
            public float Radius;
            public readonly List<Vector2> Points = new List<Vector2>();
            public readonly List<List<Edge>> Edges = new List<List<Edge>>();
            public List<int>[] SegmentNodes;
            public int Add(Vector2 point, int segment)
            {
                foreach (int existing in SegmentNodes[segment])
                    if ((Points[existing] - point).sqrMagnitude < .00000001f) return existing;
                int id = Points.Count; Points.Add(point); Edges.Add(new List<Edge>());
                SegmentNodes[segment].Add(id); return id;
            }
            public void Link(int a, int b)
            {
                float distance = Vector2.Distance(Points[a], Points[b]);
                Edges[a].Add(new Edge(b, distance)); Edges[b].Add(new Edge(a, distance));
            }
        }

        private static RoadGraph s_graph;

        /// <summary>Shortest centreline route with real junctions and query projections. Corridors
        /// are eroded by the actor radius; disconnected or off-road targets return infinity.
        /// Endpoints/junctions split segments, so crossing a long road never visits its centre.</summary>
        public static float ShortestPathLength(Vector2 start, Vector2 target, IReadOnlyList<TownRoadSegment> roads, float actorRadius)
        {
            if (roads == null || roads.Count == 0 || actorRadius < 0) return float.PositiveInfinity;
            if (s_graph == null || !ReferenceEquals(s_graph.Roads, roads) || s_graph.Radius != actorRadius)
                s_graph = BuildGraph(roads, actorRadius);
            var graph = s_graph;
            var distances = new float[graph.Points.Count];
            var targetCost = new float[graph.Points.Count];
            for (int n = 0; n < distances.Length; n++) distances[n] = targetCost[n] = float.PositiveInfinity;
            var queue = new List<Edge>();
            float best = float.PositiveInfinity;
            for (int i = 0; i < roads.Count; i++)
            {
                float clearance = roads[i].Width * .5f - actorRadius;
                if (clearance < 0) continue;
                var startProjection = Project(start, roads[i].Start, roads[i].End);
                var targetProjection = Project(target, roads[i].Start, roads[i].End);
                bool startOn = Vector2.Distance(start, startProjection) <= clearance + .0001f;
                bool targetOn = Vector2.Distance(target, targetProjection) <= clearance + .0001f;
                if (startOn && targetOn) best = Mathf.Min(best, Vector2.Distance(start, target));
                foreach (int n in graph.SegmentNodes[i])
                {
                    // The query projection is an extra split point on this segment, not its midpoint.
                    if (startOn)
                    {
                        float distance = Vector2.Distance(start, startProjection) + Vector2.Distance(startProjection, graph.Points[n]);
                        if (distance < distances[n]) { distances[n] = distance; Push(queue, new Edge(n, distance)); }
                    }
                    if (targetOn) targetCost[n] = Mathf.Min(targetCost[n],
                        Vector2.Distance(target, targetProjection) + Vector2.Distance(targetProjection, graph.Points[n]));
                }
            }
            while (queue.Count > 0)
            {
                var current = Pop(queue);
                if (current.Length > distances[current.Target] || current.Length >= best) continue;
                best = Mathf.Min(best, current.Length + targetCost[current.Target]);
                foreach (var edge in graph.Edges[current.Target])
                {
                    float distance = current.Length + edge.Length;
                    if (distance >= distances[edge.Target]) continue;
                    distances[edge.Target] = distance; Push(queue, new Edge(edge.Target, distance));
                }
            }
            return best;
        }

        private static void Push(List<Edge> heap, Edge value)
        {
            int i = heap.Count; heap.Add(value);
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (heap[parent].Length <= value.Length) break;
                heap[i] = heap[parent]; i = parent;
            }
            heap[i] = value;
        }

        private static Edge Pop(List<Edge> heap)
        {
            var result = heap[0]; var last = heap[heap.Count - 1]; heap.RemoveAt(heap.Count - 1);
            if (heap.Count == 0) return result;
            int i = 0;
            while (i * 2 + 1 < heap.Count)
            {
                int child = i * 2 + 1;
                if (child + 1 < heap.Count && heap[child + 1].Length < heap[child].Length) child++;
                if (heap[child].Length >= last.Length) break;
                heap[i] = heap[child]; i = child;
            }
            heap[i] = last; return result;
        }

        private static RoadGraph BuildGraph(IReadOnlyList<TownRoadSegment> roads, float radius)
        {
            var graph = new RoadGraph { Roads = roads, Radius = radius, SegmentNodes = new List<int>[roads.Count] };
            for (int i = 0; i < roads.Count; i++)
            {
                graph.SegmentNodes[i] = new List<int>();
                graph.Add(roads[i].Start, i); graph.Add(roads[i].End, i);
            }
            for (int i = 0; i < roads.Count; i++)
            for (int j = i + 1; j < roads.Count; j++)
            {
                var a = roads[i]; var b = roads[j];
                float reach = (a.Width + b.Width) * .5f - radius * 2;
                if (a.Width < radius * 2 || b.Width < radius * 2 || a.MaxX < b.MinX || b.MaxX < a.MinX ||
                    a.MaxY < b.MinY || b.MaxY < a.MinY) continue;
                ClosestPoints(a.Start, a.End, b.Start, b.End, out var p, out var q);
                if (Vector2.Distance(p, q) > reach + .0001f) continue;
                graph.Link(graph.Add(p, i), graph.Add(q, j));
            }
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i].Width < radius * 2) continue;
                var origin = roads[i].Start; var direction = roads[i].End - origin;
                graph.SegmentNodes[i].Sort((a, b) => Vector2.Dot(graph.Points[a] - origin, direction)
                    .CompareTo(Vector2.Dot(graph.Points[b] - origin, direction)));
                for (int j = 1; j < graph.SegmentNodes[i].Count; j++)
                    graph.Link(graph.SegmentNodes[i][j - 1], graph.SegmentNodes[i][j]);
            }
            return graph;
        }

        private static Vector2 Project(Vector2 p, Vector2 a, Vector2 b)
        {
            var d = b - a;
            return d.sqrMagnitude < .000001f ? a : a + d * Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude);
        }

        private static void ClosestPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 p, out Vector2 q)
        {
            float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;
            float denominator = Cross(b - a, d - c);
            if (Mathf.Abs(denominator) > .000001f)
            {
                float t = Cross(c - a, d - c) / denominator, u = Cross(c - a, b - a) / denominator;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) { p = q = a + (b - a) * t; return; }
            }
            p = a; q = Project(a, c, d); float best = (p - q).sqrMagnitude;
            var x = b; var y = Project(b, c, d); float distance = (x - y).sqrMagnitude;
            if (distance < best) { p = x; q = y; best = distance; }
            x = Project(c, a, b); y = c; distance = (x - y).sqrMagnitude;
            if (distance < best) { p = x; q = y; best = distance; }
            x = Project(d, a, b); y = d; distance = (x - y).sqrMagnitude;
            if (distance < best) { p = x; q = y; }
        }
    }
}
