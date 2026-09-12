using System.Collections.Generic;
using CindarsHope.NPC.Runtime;
using CindarsHope.NPC.Schedule;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>
    /// Diagnostic-only: prints the node-id sequence the Town route graph actually returns for the
    /// worst anchor detours, so the bad edge can be named instead of guessed.
    /// </summary>
    /// <remarks>
    /// Read-only companion to ValidateTownRouteDetourRatio, which reports THAT a pair detours.
    /// This reports THROUGH WHAT: node ids expose whether the path leaves via the authored
    /// `&lt;anchor&gt;__approach` lane, through generated `town_detour_*` BFS cells, or across a long
    /// approach-to-road edge. Not registered in Validar Projeto — it produces no pass/fail.
    /// </remarks>
    public static class DiagnoseTownRouteDetour
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string Tag = "[town-detour-diag]";

        /// <summary>Anchor pairs reported, ordered worst ratio first.</summary>
        private const int ReportedPairs = 6;

        public static void Run()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var openedHere = !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                NpcTownRouteGraph graph = null;
                var anchors = new List<NpcScheduleAnchor>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (graph == null) graph = root.GetComponentInChildren<NpcTownRouteGraph>(true);
                    anchors.AddRange(root.GetComponentsInChildren<NpcScheduleAnchor>(true));
                }
                if (graph == null)
                {
                    Debug.LogWarning($"{Tag} No NpcTownRouteGraph in {ScenePath}.");
                    return;
                }

                graph.RebuildIndex();
                Physics2D.SyncTransforms();

                // Reverse map so a returned waypoint can be named. Positions are the generator's own
                // node positions, so exact-ish matching within a tight epsilon is enough.
                var byPosition = new List<NpcTownRouteNode>(graph.Nodes);

                string NameAt(Vector3 p)
                {
                    NpcTownRouteNode best = null;
                    var bestDistance = float.PositiveInfinity;
                    foreach (var node in byPosition)
                    {
                        if (node == null) continue;
                        var d = (node.Position - p).sqrMagnitude;
                        if (d < bestDistance) { bestDistance = d; best = node; }
                    }
                    return bestDistance <= 0.0025f ? best.Id : $"<unmapped {p}>";
                }

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

                var worst = new List<(float ratio, string label, string path, float straight, float travelled)>();
                var route = new List<Vector3>();
                foreach (var pair in byNpc)
                {
                    var list = pair.Value;
                    for (var i = 0; i < list.Count; i++)
                    for (var j = 0; j < list.Count; j++)
                    {
                        if (i == j) continue;
                        var from = list[i];
                        var to = list[j];
                        var straight = Vector2.Distance(from.GetPosition(), to.GetPosition());
                        if (straight < 2f) continue;
                        if (!graph.TryBuildRoute(from.GetPosition(), to.GetPosition(), route)) continue;

                        var travelled = 0f;
                        for (var k = 1; k < route.Count; k++) travelled += Vector3.Distance(route[k - 1], route[k]);
                        var ratio = straight > 0.001f ? travelled / straight : 0f;
                        if (ratio <= 3f) continue;

                        var names = new List<string>();
                        foreach (var point in route) names.Add(NameAt(point));
                        worst.Add((ratio, $"{from.AnchorId}->{to.AnchorId}",
                            string.Join(" | ", names), straight, travelled));
                    }
                }

                // Adjacency of the endpoints themselves. If an anchor/approach node's only neighbours
                // are far away, the local road mesh never formed and Dijkstra has no short path to
                // find -- which is a generation defect, not a pathfinding one.
                var neighbours = new Dictionary<string, List<string>>();
                foreach (var edge in graph.Edges)
                {
                    if (edge == null) continue;
                    if (!neighbours.TryGetValue(edge.From, out var a)) neighbours[edge.From] = a = new List<string>();
                    if (!neighbours.TryGetValue(edge.To, out var b)) neighbours[edge.To] = b = new List<string>();
                    a.Add(edge.To);
                    b.Add(edge.From);
                }
                var positionById = new Dictionary<string, Vector3>();
                foreach (var node in graph.Nodes) if (node != null) positionById[node.Id] = node.Position;

                void DumpNeighbours(string id)
                {
                    if (!positionById.TryGetValue(id, out var origin))
                    {
                        Debug.Log($"{Tag}   adj {id}: NODE MISSING");
                        return;
                    }
                    if (!neighbours.TryGetValue(id, out var list) || list.Count == 0)
                    {
                        Debug.Log($"{Tag}   adj {id} @{origin}: NO EDGES");
                        return;
                    }
                    var parts = new List<string>();
                    foreach (var other in list)
                        parts.Add(positionById.TryGetValue(other, out var p)
                            ? $"{other}/{Vector2.Distance(origin, p):F2}u"
                            : $"{other}/?");
                    Debug.Log($"{Tag}   adj {id} @{origin}: {list.Count} edge(s) -> {string.Join(", ", parts)}");
                }

                Debug.Log($"{Tag} graph totals: nodes={graph.Nodes.Count} edges={graph.Edges.Count}");
                foreach (var id in new[]
                         {
                             "npc_thalindra_home", "npc_thalindra_home__approach", "npc_thalindra_work",
                             "npc_maelor_home", "npc_maelor_home__approach", "npc_maelor_work"
                         })
                    DumpNeighbours(id);

                worst.Sort((a, b) => b.ratio.CompareTo(a.ratio));
                Debug.Log($"{Tag} {worst.Count} detouring pair(s); printing the {ReportedPairs} worst.");
                for (var i = 0; i < worst.Count && i < ReportedPairs; i++)
                {
                    var entry = worst[i];
                    Debug.Log($"{Tag} {entry.label} straight={entry.straight:F2}u graph={entry.travelled:F2}u " +
                              $"ratio={entry.ratio:F1}x\n{Tag}   path: {entry.path}");
                }
            }
            finally
            {
                if (openedHere && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
