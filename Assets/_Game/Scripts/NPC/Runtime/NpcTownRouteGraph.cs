using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.NPC.Runtime
{
    [Serializable]
    public sealed class NpcTownRouteNode
    {
        public string Id;
        public Vector3 Position;
        public bool Safe = true;
    }

    [Serializable]
    public sealed class NpcTownRouteEdge
    {
        public string From;
        public string To;
        public float Cost = 1f;
    }

    /// <summary>
    /// Stable, scene-serializable Town route contract. The editor generator owns the node/edge data;
    /// runtime only resolves deterministic shortest routes and never creates scene objects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcTownRouteGraph : MonoBehaviour
    {
        private static NpcTownRouteGraph s_active;
        [SerializeField] private List<NpcTownRouteNode> _nodes = new List<NpcTownRouteNode>();
        [SerializeField] private List<NpcTownRouteEdge> _edges = new List<NpcTownRouteEdge>();

        private readonly Dictionary<string, NpcTownRouteNode> _byId = new Dictionary<string, NpcTownRouteNode>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<NpcTownRouteEdge>> _adjacency = new Dictionary<string, List<NpcTownRouteEdge>>(StringComparer.Ordinal);

        public IReadOnlyList<NpcTownRouteNode> Nodes => _nodes;
        public IReadOnlyList<NpcTownRouteEdge> Edges => _edges;
        public static NpcTownRouteGraph Active => s_active;

        public bool ContainsNode(string id) => !string.IsNullOrWhiteSpace(id) && _nodes.Exists(n => n != null && n.Id == id);

        public bool ValidateConnectivity(out int reachableCount)
        {
            RebuildIndex();
            reachableCount = 0;
            if (_nodes == null || _nodes.Count == 0) return false;
            var first = _nodes.Find(n => n != null && !string.IsNullOrWhiteSpace(n.Id));
            if (first == null || !_adjacency.ContainsKey(first.Id)) return false;
            var visited = new HashSet<string>(StringComparer.Ordinal) { first.Id };
            var queue = new Queue<string>();
            queue.Enqueue(first.Id);
            while (queue.Count > 0)
            {
                foreach (var edge in _adjacency[queue.Dequeue()])
                    if (visited.Add(edge.To)) queue.Enqueue(edge.To);
            }
            reachableCount = visited.Count;
            return reachableCount == _byId.Count;
        }

        public void CollectUnreachableNodeIds(List<string> result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            result.Clear();
            RebuildIndex();
            if (_nodes == null || _nodes.Count == 0) return;
            var first = _nodes.Find(n => n != null && !string.IsNullOrWhiteSpace(n.Id));
            if (first == null || !_adjacency.ContainsKey(first.Id)) return;
            var visited = new HashSet<string>(StringComparer.Ordinal) { first.Id };
            var queue = new Queue<string>();
            queue.Enqueue(first.Id);
            while (queue.Count > 0)
                foreach (var edge in _adjacency[queue.Dequeue()])
                    if (visited.Add(edge.To)) queue.Enqueue(edge.To);
            foreach (var id in _byId.Keys) if (!visited.Contains(id)) result.Add(id);
            result.Sort(StringComparer.Ordinal);
        }

        public void CollectPhysicallyBlockedEdgeIds(List<string> result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            result.Clear();
            RebuildIndex();
            foreach (var edge in _edges)
            {
                if (edge == null || !_byId.TryGetValue(edge.From, out var from) ||
                    !_byId.TryGetValue(edge.To, out var to)) continue;
                if (!SegmentIsPhysicallyReachable(from.Position, to.Position, null, out var blocker))
                    result.Add($"{edge.From}{from.Position}->{edge.To}{to.Position}:{blocker}");
            }
        }

        public int PrunePhysicallyBlockedEdges()
        {
            RebuildIndex();
            var removed = _edges.RemoveAll(edge => edge == null ||
                !_byId.TryGetValue(edge.From, out var from) || !_byId.TryGetValue(edge.To, out var to) ||
                !SegmentIsPhysicallyReachable(from.Position, to.Position, null));
            if (removed > 0)
            {
                var referenced = new HashSet<string>(StringComparer.Ordinal);
                foreach (var edge in _edges)
                {
                    if (edge == null) continue;
                    referenced.Add(edge.From);
                    referenced.Add(edge.To);
                }
                // A blocked authored road edge can leave its terminal sample isolated. It is not
                // a schedule anchor or recovery contract, so remove that unusable sample too.
                _nodes.RemoveAll(node => node != null && node.Id != null &&
                    node.Id.StartsWith("town_road_", StringComparison.Ordinal) && !referenced.Contains(node.Id));
                RebuildIndex();
            }
            return removed;
        }

        private void OnEnable() => s_active = this;
        private void OnDisable() { if (s_active == this) s_active = null; }
        private void Awake() => RebuildIndex();

        public void RebuildIndex()
        {
            _byId.Clear();
            _adjacency.Clear();
            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.Id)) continue;
                    _byId[node.Id] = node;
                    _adjacency[node.Id] = new List<NpcTownRouteEdge>();
                }
            }

            if (_edges == null) return;
            foreach (var edge in _edges)
            {
                if (edge == null || !_adjacency.ContainsKey(edge.From) || !_adjacency.ContainsKey(edge.To)) continue;
                _adjacency[edge.From].Add(edge);
                _adjacency[edge.To].Add(new NpcTownRouteEdge { From = edge.To, To = edge.From, Cost = edge.Cost });
            }
            foreach (var list in _adjacency.Values)
                list.Sort((a, b) => string.CompareOrdinal(a.To, b.To));
        }

        public void Configure(IList<NpcTownRouteNode> nodes, IList<NpcTownRouteEdge> edges)
        {
            _nodes = nodes != null ? new List<NpcTownRouteNode>(nodes) : new List<NpcTownRouteNode>();
            _edges = edges != null ? new List<NpcTownRouteEdge>(edges) : new List<NpcTownRouteEdge>();
            RebuildIndex();
        }

        public bool TryBuildRoute(Vector3 from, Vector3 to, List<Vector3> result)
            => TryBuildRoute(from, to, result, null);

        public string LastFailureDiagnostic { get; private set; }

        public bool TryBuildRoute(Vector3 from, Vector3 to, List<Vector3> result, Collider2D self)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            result.Clear();
            LastFailureDiagnostic = null;
            if (_nodes == null || _nodes.Count == 0)
            {
                LastFailureDiagnostic = "empty_graph";
                return false;
            }
            RebuildIndex();
            // Safe restricts recovery/snap destinations, not route origins. A resident inside an
            // authored non-safe home node must depart through its home->approach edge.
            var start = NearestReachableNode(from, safeOnly: false, self);
            var goal = NearestReachableNode(to, safeOnly: true, self);
            if (start == null || goal == null)
            {
                var nearestStart = NearestNode(from, true);
                var nearestGoal = NearestNode(to, true);
                var startBlock = nearestStart != null &&
                    !SegmentIsPhysicallyReachable(from, nearestStart.Position, self, out var startBlocker)
                    ? startBlocker : "none";
                var goalBlock = nearestGoal != null &&
                    !SegmentIsPhysicallyReachable(to, nearestGoal.Position, self, out var goalBlocker)
                    ? goalBlocker : "none";
                LastFailureDiagnostic = $"reachable_node_missing start={start?.Id ?? "none"}" +
                    $" nearestStart={nearestStart?.Id ?? "none"}/{(nearestStart != null ? Vector2.Distance(from, nearestStart.Position) : -1f):F3}" +
                    $" startBlock={startBlock} goal={goal?.Id ?? "none"}" +
                    $" nearestGoal={nearestGoal?.Id ?? "none"}/{(nearestGoal != null ? Vector2.Distance(to, nearestGoal.Position) : -1f):F3}" +
                    $" goalBlock={goalBlock}";
                return false;
            }
            result.Add(from);
            if (start.Id != goal.Id)
            {
                var previous = new Dictionary<string, string>(StringComparer.Ordinal) { [start.Id] = null };
                var distances = new Dictionary<string, float>(StringComparer.Ordinal);
                var open = new HashSet<string>(StringComparer.Ordinal);
                foreach (var id in _byId.Keys) distances[id] = float.PositiveInfinity;
                distances[start.Id] = 0f;
                open.Add(start.Id);
                while (open.Count > 0)
                {
                    string current = null;
                    var currentDistance = float.PositiveInfinity;
                    foreach (var candidate in open)
                    {
                        var candidateDistance = distances[candidate];
                        if (candidateDistance < currentDistance ||
                            (Mathf.Approximately(candidateDistance, currentDistance) &&
                             string.CompareOrdinal(candidate, current) < 0))
                        {
                            current = candidate;
                            currentDistance = candidateDistance;
                        }
                    }
                    open.Remove(current);
                    if (current == goal.Id) break;
                    foreach (var edge in _adjacency[current])
                    {
                        var candidateDistance = currentDistance + Mathf.Max(0.001f, edge.Cost);
                        if (candidateDistance < distances[edge.To] - 0.0001f ||
                            (Mathf.Approximately(candidateDistance, distances[edge.To]) &&
                             string.CompareOrdinal(current, previous.TryGetValue(edge.To, out var prior) ? prior : null) < 0))
                        {
                            previous[edge.To] = current;
                            distances[edge.To] = candidateDistance;
                            open.Add(edge.To);
                        }
                    }
                }
                if (!previous.ContainsKey(goal.Id))
                {
                    LastFailureDiagnostic = $"disconnected start={start.Id} goal={goal.Id}";
                    result.Clear();
                    return false;
                }
                var ids = new List<string>();
                for (var id = goal.Id; id != null; id = previous[id]) ids.Add(id);
                ids.Reverse();
                AppendCompactedPath(ids, result, self);
            }
            else
            {
                // Even when both positions resolve to the same graph node, visit that node before
                // the exact destination. Indoor recovery relies on this to re-enter through the
                // explicit door approach instead of drawing a direct segment across the facade.
                result.Add(start.Position);
            }
            result.Add(to);
            return true;
        }

        private void AppendCompactedPath(IReadOnlyList<string> ids, List<Vector3> result, Collider2D self)
        {
            if (ids == null || ids.Count == 0) return;
            var compacted = new List<Vector3>(ids.Count);
            for (var i = 0; i < ids.Count; i++)
            {
                var point = _byId[ids[i]].Position;
                // Serialized roads are sampled densely for editor validation. Remove an intermediate
                // sample only when the actor-sized physical probe can travel directly between its
                // neighbours; authored corners and solid-obstacle detours therefore remain intact.
                while (compacted.Count >= 2 &&
                       SegmentIsPhysicallyReachable(compacted[compacted.Count - 2], point, self))
                    compacted.RemoveAt(compacted.Count - 1);
                if (compacted.Count == 0 || Vector2.Distance(compacted[compacted.Count - 1], point) > 0.01f)
                    compacted.Add(point);
            }
            // Keep explicit graph traversal observable for a non-trivial authored path. When an
            // entirely open stretch collapses to its two endpoints, retain one real graph sample
            // only if both actor-sized segments remain clear.
            if (compacted.Count == 2 && ids.Count > 2)
            {
                for (var i = 1; i < ids.Count - 1; i++)
                {
                    var sample = _byId[ids[i]].Position;
                    if (!SegmentIsPhysicallyReachable(compacted[0], sample, self) ||
                        !SegmentIsPhysicallyReachable(sample, compacted[1], self)) continue;
                    compacted.Insert(1, sample);
                    break;
                }
                if (compacted.Count == 2)
                {
                    compacted.Clear();
                    for (var i = 0; i < ids.Count; i++) compacted.Add(_byId[ids[i]].Position);
                }
            }
            result.AddRange(compacted);
        }

        public bool TryFindNearestSafeUnoccupied(Vector3 requested, float radius, Collider2D self,
            out Vector3 safePosition)
        {
            RebuildIndex();
            NpcTownRouteNode best = null;
            var bestDistance = float.PositiveInfinity;
            foreach (var node in _nodes)
            {
                if (node == null || !node.Safe || string.IsNullOrWhiteSpace(node.Id) ||
                    !NpcRouteRecoveryPolicy.IsSafePoint(node.Position, radius, self)) continue;
                var distance = (node.Position - requested).sqrMagnitude;
                if (distance < bestDistance ||
                    (Mathf.Approximately(distance, bestDistance) && string.CompareOrdinal(node.Id, best?.Id) < 0))
                {
                    best = node;
                    bestDistance = distance;
                }
            }
            safePosition = best != null ? best.Position : default;
            return best != null;
        }

        private NpcTownRouteNode NearestNode(Vector3 position, bool safeOnly)
        {
            NpcTownRouteNode best = null;
            var bestDistance = float.PositiveInfinity;
            foreach (var node in _nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.Id) || (safeOnly && !node.Safe)) continue;
                var distance = (node.Position - position).sqrMagnitude;
                if (distance < bestDistance || (Mathf.Approximately(distance, bestDistance) && string.CompareOrdinal(node.Id, best?.Id) < 0))
                {
                    best = node;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private NpcTownRouteNode NearestReachableNode(Vector3 position, bool safeOnly, Collider2D self)
        {
            NpcTownRouteNode best = null;
            var bestDistance = float.PositiveInfinity;
            foreach (var node in _nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.Id) || (safeOnly && !node.Safe)) continue;
                var distance = (node.Position - position).sqrMagnitude;
                if (distance > bestDistance + 0.0001f || !SegmentIsPhysicallyReachable(position, node.Position, self)) continue;
                if (distance < bestDistance ||
                    (Mathf.Approximately(distance, bestDistance) && string.CompareOrdinal(node.Id, best?.Id) < 0))
                {
                    best = node;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private static bool SegmentIsPhysicallyReachable(Vector3 from, Vector3 to, Collider2D self)
            => SegmentIsPhysicallyReachable(from, to, self, out _);

        private static bool SegmentIsPhysicallyReachable(Vector3 from, Vector3 to, Collider2D self,
            out string blockerName)
        {
            blockerName = null;
            var delta = (Vector2)(to - from);
            var distance = delta.magnitude;
            var castSize = (self != null ? (Vector2)self.bounds.size : new Vector2(0.55f, 0.4f)) +
                Vector2.one * 0.16f;
            var actor = self != null ? self.GetComponentInParent<NpcWanderer>() : null;
            var owner = actor != null ? actor.transform : self != null ? self.transform : null;
            var centerOffset = self != null ? (Vector2)(self.bounds.center - owner.position) : new Vector2(0f, 0.273f);
            foreach (var endpoint in new[] { from, to })
            foreach (var collider in Physics2D.OverlapBoxAll((Vector2)endpoint + centerOffset, castSize, 0f))
            {
                if (collider == null || collider.isTrigger || collider == self ||
                    (owner != null && collider.transform.IsChildOf(owner)) ||
                    collider.attachedRigidbody != null || collider.GetComponentInParent<NpcWanderer>() != null ||
                    collider.GetComponentInParent<CindarsHope.World.HouseDoorInteractable>() != null) continue;
                blockerName = collider.transform.parent != null
                    ? $"{collider.transform.parent.name}/{collider.name}" : collider.name;
                return false;
            }
            if (distance <= 0.05f) return true;
            var hits = Physics2D.BoxCastAll((Vector2)from + centerOffset, castSize, 0f, delta / distance, distance);
            foreach (var hit in hits)
            {
                var collider = hit.collider;
                if (collider == null || collider.isTrigger || collider == self ||
                    (owner != null && collider.transform.IsChildOf(owner)) ||
                    collider.attachedRigidbody != null || collider.GetComponentInParent<NpcWanderer>() != null ||
                    collider.GetComponentInParent<CindarsHope.World.HouseDoorInteractable>() != null) continue;
                blockerName = collider.transform.parent != null
                    ? $"{collider.transform.parent.name}/{collider.name}" : collider.name;
                return false;
            }
            return true;
        }
    }
}
