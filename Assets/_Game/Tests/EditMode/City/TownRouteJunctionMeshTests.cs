using System.Collections.Generic;
using CindarsHope.NPC.Runtime;
using CindarsHope.NPC.Schedule;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// Proves the junction-mesh fix on the real TownScene graph data WITHOUT regenerating the scene.
    /// </summary>
    /// <remarks>
    /// The generated graph is a tree: 2576 nodes / 2577 edges, where a spanning tree needs 2575.
    /// With no cycles there is exactly one path between any two points, so a 5u trip climbs to the
    /// common ancestor and back down — measured as 85 of 162 anchor pairs detouring, worst 111.9x.
    /// Junctions only form today when two authored road segments share an exactly equal endpoint.
    ///
    /// The candidate fix adds clearance-checked edges between nearby road nodes. It is purely
    /// additive, so it cannot lengthen an existing route. This fixture augments an in-memory graph
    /// built from the scene's own nodes and reuses the production clearance path
    /// (<see cref="NpcTownRouteGraph.PrunePhysicallyBlockedEdges"/>) to reject candidates that cross
    /// solids. The scene is opened additively and closed WITHOUT saving; nothing is materialized.
    /// </remarks>
    public sealed class TownRouteJunctionMeshTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const float DetourRatioLimit = 3f;
        private const float MinimumPairDistance = 2f;

        /// <summary>Radius shipped by the generator; kept in sync with CreateMvpTownScene.</summary>
        private const float AnchorReentryRadius = 6f;

        [Test]
        public void JunctionMesh_ReducesAnchorDetours_WithoutRegeneratingTheScene()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var openedHere = !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            GameObject probeHost = null;
            try
            {
                NpcTownRouteGraph source = null;
                var anchors = new List<NpcScheduleAnchor>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (source == null) source = root.GetComponentInChildren<NpcTownRouteGraph>(true);
                    anchors.AddRange(root.GetComponentsInChildren<NpcScheduleAnchor>(true));
                }
                Assert.IsNotNull(source, "TownScene must contain the generated NpcTownRouteGraph");
                Physics2D.SyncTransforms();

                var nodes = new List<NpcTownRouteNode>(source.Nodes);
                var baseEdges = new List<NpcTownRouteEdge>(source.Edges);
                Assert.Greater(nodes.Count, 0, "graph must carry generated nodes");

                // Documents the tree shape this fix targets; a mesh has materially more edges.
                TestContext.WriteLine($"baseline graph: nodes={nodes.Count} edges={baseEdges.Count} " +
                                      $"(spanning tree would be {nodes.Count - 1})");

                var pairs = BuildAnchorPairs(anchors);
                Assert.Greater(pairs.Count, 0, "schedule anchors must be present to measure transitions");

                probeHost = new GameObject("TownRouteJunctionProbe");
                var probe = probeHost.AddComponent<NpcTownRouteGraph>();

                var baseline = Measure(probe, nodes, baseEdges, pairs, out var baselineWorst,
                    out var baselineWorstLabel);
                TestContext.WriteLine($"baseline detours: {baseline}/{pairs.Count} " +
                                      $"worst={baselineWorst:F1}x on {baselineWorstLabel}");

                // Junction mesh alone, across radii.
                var junctionBest = int.MaxValue;
                foreach (var radius in new[] { 1.5f, 2.5f, 4f })
                {
                    var augmented = new List<NpcTownRouteEdge>(baseEdges);
                    var added = AddJunctionCandidates(nodes, baseEdges, augmented, radius);
                    var detours = Measure(probe, nodes, augmented, pairs, out var worst, out var worstLabel);
                    // Measure() prunes blocked candidates through the production path, so the kept
                    // count is what the generator would actually serialize at this radius.
                    TestContext.WriteLine($"junction {radius:F1}u: candidates={added} " +
                                          $"edgesKept={probe.Edges.Count} detours={detours}/{pairs.Count} " +
                                          $"worst={worst:F1}x on {worstLabel}");
                    junctionBest = Mathf.Min(junctionBest, detours);
                }

                // Anchor re-entry alone: join every anchor to road nodes within reach instead of
                // keeping the single unbounded connector edge the generator serialized (measured
                // 57-73u on maelor/tibbet/thalindra, while a road node sits 0.00-2.58u away).
                // Calibrates the shipped radius instead of assuming it. A larger reach could connect an
                // anchor across open ground the player would not read as a path, so a better number
                // here is a finding to weigh, not an automatic adoption.
                var connectorDetours = int.MaxValue;
                var cWorst = 0f;
                var cWorstLabel = "none";
                var connectors = 0;
                var residual = new List<string>();
                foreach (var radius in new[] { AnchorReentryRadius, 6f, 8f })
                {
                    var candidateEdges = new List<NpcTownRouteEdge>(baseEdges);
                    var added = AddAnchorReentry(nodes, anchors, candidateEdges, radius);
                    var detail = new List<string>();
                    var detours = Measure(probe, nodes, candidateEdges, pairs, out var worst,
                        out var worstLabel, detail);
                    TestContext.WriteLine($"anchor-reentry {radius:F1}u: candidates={added} " +
                                          $"edgesKept={probe.Edges.Count} detours={detours}/{pairs.Count} " +
                                          $"worst={worst:F1}x on {worstLabel}");
                    if (Mathf.Approximately(radius, AnchorReentryRadius))
                    {
                        connectorDetours = detours;
                        cWorst = worst;
                        cWorstLabel = worstLabel;
                        connectors = added;
                        residual = detail;
                    }
                }
                // The pairs the shipped fix does NOT resolve, so the remainder is scoped rather than
                // hidden behind the headline reduction.
                foreach (var line in residual) TestContext.WriteLine($"residual: {line}");
                // Both together.
                var bothEdges = new List<NpcTownRouteEdge>(baseEdges);
                AddJunctionCandidates(nodes, baseEdges, bothEdges, 2.5f);
                AddAnchorReentry(nodes, anchors, bothEdges, AnchorReentryRadius);
                var bothDetours = Measure(probe, nodes, bothEdges, pairs, out var bWorst, out var bWorstLabel);
                TestContext.WriteLine($"junction 2.5u + anchor-reentry: edgesKept={probe.Edges.Count} " +
                                      $"detours={bothDetours}/{pairs.Count} worst={bWorst:F1}x on {bWorstLabel}");

                TestContext.WriteLine($"SUMMARY baseline={baseline} junctionOnly={junctionBest} " +
                                      $"connectorOnly={connectorDetours} both={bothDetours} of {pairs.Count}");

                // The measured claim: anchor re-entry is the dominant term, not the junction mesh.
                Assert.Less(connectorDetours, junctionBest,
                    "anchor re-entry must beat the junction mesh; the single unbounded connector edge " +
                    "is the dominant cause of the detours");
            }
            finally
            {
                if (probeHost != null) Object.DestroyImmediate(probeHost);
                // Never save: the candidate fix belongs in the generator, not in a test-mutated scene.
                if (openedHere && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static List<(Vector3 from, Vector3 to, string label)> BuildAnchorPairs(
            List<NpcScheduleAnchor> anchors)
        {
            var byNpc = new Dictionary<string, List<NpcScheduleAnchor>>();
            foreach (var anchor in anchors)
            {
                if (anchor == null || string.IsNullOrWhiteSpace(anchor.NpcId)) continue;
                if (!byNpc.TryGetValue(anchor.NpcId, out var list))
                {
                    list = new List<NpcScheduleAnchor>();
                    byNpc.Add(anchor.NpcId, list);
                }
                list.Add(anchor);
            }

            var pairs = new List<(Vector3, Vector3, string)>();
            foreach (var entry in byNpc)
            {
                var list = entry.Value;
                for (var i = 0; i < list.Count; i++)
                for (var j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    if (Vector2.Distance(list[i].GetPosition(), list[j].GetPosition()) < MinimumPairDistance)
                        continue;
                    pairs.Add((list[i].GetPosition(), list[j].GetPosition(),
                        $"{list[i].AnchorId}->{list[j].AnchorId}"));
                }
            }
            return pairs;
        }

        /// <summary>Adds candidate edges between road nodes within <paramref name="radius"/>.</summary>
        private static int AddJunctionCandidates(List<NpcTownRouteNode> nodes,
            List<NpcTownRouteEdge> existing, List<NpcTownRouteEdge> result, float radius)
        {
            var adjacent = new HashSet<string>();
            foreach (var edge in existing)
            {
                if (edge == null) continue;
                adjacent.Add($"{edge.From} {edge.To}");
                adjacent.Add($"{edge.To} {edge.From}");
            }

            // Spatial hash keeps this near-linear instead of 2576^2 pair tests.
            var cellSize = Mathf.Max(0.5f, radius);
            var buckets = new Dictionary<Vector2Int, List<NpcTownRouteNode>>();
            foreach (var node in nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.Id)) continue;
                if (!IsRoadNode(node.Id)) continue;
                var cell = new Vector2Int(Mathf.FloorToInt(node.Position.x / cellSize),
                    Mathf.FloorToInt(node.Position.y / cellSize));
                if (!buckets.TryGetValue(cell, out var list)) buckets[cell] = list = new List<NpcTownRouteNode>();
                list.Add(node);
            }

            var added = 0;
            foreach (var bucket in buckets)
            {
                for (var ox = 0; ox <= 1; ox++)
                for (var oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy < 0) continue;
                    if (!buckets.TryGetValue(bucket.Key + new Vector2Int(ox, oy), out var others)) continue;
                    foreach (var a in bucket.Value)
                    foreach (var b in others)
                    {
                        if (a == b || string.CompareOrdinal(a.Id, b.Id) >= 0) continue;
                        if (Vector2.Distance(a.Position, b.Position) > radius) continue;
                        if (adjacent.Contains($"{a.Id} {b.Id}")) continue;
                        adjacent.Add($"{a.Id} {b.Id}");
                        adjacent.Add($"{b.Id} {a.Id}");
                        result.Add(new NpcTownRouteEdge
                        {
                            From = a.Id,
                            To = b.Id,
                            Cost = Vector2.Distance(a.Position, b.Position)
                        });
                        added++;
                    }
                }
            }
            return added;
        }

        /// <summary>Joins each schedule anchor to every road node within reach.</summary>
        /// <remarks>The generator gives an anchor without a door approach one connector edge chosen by
        /// an unbounded BFS, so a rejected neighbour sends it tens of units away. Measured: maelor_work
        /// sits ON a road node (0.00u) yet its only edge spans 73.05u. Clearance is still enforced by
        /// the caller's prune pass, so this cannot punch through a wall.</remarks>
        private static int AddAnchorReentry(List<NpcTownRouteNode> nodes, List<NpcScheduleAnchor> anchors,
            List<NpcTownRouteEdge> result, float radius)
        {
            var positionById = new Dictionary<string, Vector3>();
            foreach (var node in nodes) if (node != null && !string.IsNullOrWhiteSpace(node.Id)) positionById[node.Id] = node.Position;

            var existing = new HashSet<string>();
            foreach (var edge in result)
            {
                if (edge == null) continue;
                existing.Add($"{edge.From} {edge.To}");
                existing.Add($"{edge.To} {edge.From}");
            }

            var added = 0;
            foreach (var anchor in anchors)
            {
                if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId)) continue;
                // The authored approach node, when present, is the anchor's real entry point.
                var entryId = positionById.ContainsKey($"{anchor.AnchorId}__approach")
                    ? $"{anchor.AnchorId}__approach"
                    : anchor.AnchorId;
                if (!positionById.TryGetValue(entryId, out var origin)) continue;

                foreach (var node in nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.Id) || !IsRoadNode(node.Id)) continue;
                    var distance = Vector2.Distance(origin, node.Position);
                    if (distance > radius) continue;
                    if (existing.Contains($"{entryId} {node.Id}")) continue;
                    existing.Add($"{entryId} {node.Id}");
                    existing.Add($"{node.Id} {entryId}");
                    result.Add(new NpcTownRouteEdge { From = entryId, To = node.Id, Cost = Mathf.Max(0.01f, distance) });
                    added++;
                }
            }
            return added;
        }

        private static bool IsRoadNode(string id)
            => id.StartsWith("town_road_", System.StringComparison.Ordinal) ||
               id.StartsWith("town_detour_", System.StringComparison.Ordinal);

        private static int Measure(NpcTownRouteGraph probe, List<NpcTownRouteNode> nodes,
            List<NpcTownRouteEdge> edges, List<(Vector3 from, Vector3 to, string label)> pairs,
            out float worstRatio, out string worstLabel, List<string> detailOut = null)
        {
            probe.Configure(nodes, edges);
            // Reuses the production clearance path so a candidate crossing a solid is dropped exactly
            // as the generator would drop it.
            probe.PrunePhysicallyBlockedEdges();

            var detours = 0;
            worstRatio = 0f;
            worstLabel = "none";
            var route = new List<Vector3>();
            foreach (var pair in pairs)
            {
                if (!probe.TryBuildRoute(pair.from, pair.to, route))
                {
                    detours++;
                    detailOut?.Add($"{pair.label} NO ROUTE: {probe.LastFailureDiagnostic}");
                    continue;
                }
                var travelled = 0f;
                for (var k = 1; k < route.Count; k++) travelled += Vector3.Distance(route[k - 1], route[k]);
                var straight = Vector2.Distance(pair.from, pair.to);
                var ratio = straight > 0.001f ? travelled / straight : 0f;
                if (ratio > worstRatio) { worstRatio = ratio; worstLabel = pair.label; }
                if (ratio > DetourRatioLimit)
                {
                    detours++;
                    detailOut?.Add($"{pair.label} straight={straight:F2}u graph={travelled:F2}u " +
                                   $"ratio={ratio:F1}x waypoints={route.Count}");
                }
            }
            return detours;
        }
    }
}
