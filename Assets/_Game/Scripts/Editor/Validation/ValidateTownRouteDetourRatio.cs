using System.Collections.Generic;
using CindarsHope.NPC.Runtime;
using CindarsHope.NPC.Schedule;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Saved-scene check that the Town route graph returns SHORT routes between the anchors the
    /// schedule actually travels between, not just reachable ones.
    /// </summary>
    /// <remarks>
    /// Connectivity is not proximity. `ValidateConnectivity` passes while the graph is a single
    /// component, so a pruned local edge can leave two points 5u apart joined only by a detour
    /// across the whole town — the graph answers "yes, reachable" and the NPC walks the long way.
    ///
    /// Observed 2026-09-11: npc_thalindra was ordered from (67.84, 14.15) to its home anchor at
    /// (65.50, 19.00), 5.4u away, and walked 110u west before the bounded replan budget stopped it.
    /// No existing gate caught it: the graph was 2581/2581 connected, every anchor was "reachable",
    /// and TryBuildRoute reported success. Ratio is the missing signal.
    /// </remarks>
    public static class ValidateTownRouteDetourRatio
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string Tag = "[town-detour]";

        /// <summary>Graph path length over straight-line distance above which a route is a detour.</summary>
        /// <remarks>A street network that bends around blocks sits near 1.2-1.6. The threshold leaves
        /// generous headroom for authored detours while still catching order-of-magnitude nonsense.</remarks>
        private const float MaxDetourRatio = 3f;

        /// <summary>Pairs closer than this are ignored: over a few units the ratio is dominated by
        /// endpoint snapping rather than by the path, so it reports noise instead of defects.</summary>
        private const float MinimumPairDistance = 2f;

        public static void Validate()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var openedHere = !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            var errors = 0;
            var warnings = 0;
            var checkedPairs = 0;
            var worstRatio = 0f;
            var worstPair = "none";

            try
            {
                NpcTownRouteGraph graph = null;
                var anchorsByNpc = new Dictionary<string, List<NpcScheduleAnchor>>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (graph == null) graph = root.GetComponentInChildren<NpcTownRouteGraph>(true);
                    foreach (var anchor in root.GetComponentsInChildren<NpcScheduleAnchor>(true))
                    {
                        if (anchor == null || string.IsNullOrWhiteSpace(anchor.NpcId)) continue;
                        if (!anchorsByNpc.TryGetValue(anchor.NpcId, out var list))
                        {
                            list = new List<NpcScheduleAnchor>();
                            anchorsByNpc.Add(anchor.NpcId, list);
                        }
                        list.Add(anchor);
                    }
                }

                if (graph == null)
                {
                    Debug.LogWarning($"{Tag} No NpcTownRouteGraph in {ScenePath}. " +
                                     "Run CindarsHope/Inicializar Projeto to materialize the Town scene.");
                    Debug.Log($"{Tag} Validation finished. Errors=0, Warnings=1");
                    return;
                }

                graph.RebuildIndex();
                Physics2D.SyncTransforms();
                var route = new List<Vector3>();

                // Every ordered anchor pair a schedule block transition can request for one NPC.
                foreach (var pair in anchorsByNpc)
                {
                    var anchors = pair.Value;
                    for (var i = 0; i < anchors.Count; i++)
                    for (var j = 0; j < anchors.Count; j++)
                    {
                        if (i == j) continue;
                        var from = anchors[i];
                        var to = anchors[j];
                        var straight = Vector2.Distance(from.GetPosition(), to.GetPosition());
                        if (straight < MinimumPairDistance) continue;

                        checkedPairs++;
                        if (!graph.TryBuildRoute(from.GetPosition(), to.GetPosition(), route))
                        {
                            Debug.LogError($"{Tag} No route {from.AnchorId}->{to.AnchorId} " +
                                           $"({from.GetPosition()} -> {to.GetPosition()}, {straight:F2}u): " +
                                           $"{graph.LastFailureDiagnostic}");
                            errors++;
                            continue;
                        }

                        var travelled = 0f;
                        for (var k = 1; k < route.Count; k++) travelled += Vector3.Distance(route[k - 1], route[k]);
                        var ratio = straight > 0.001f ? travelled / straight : 0f;
                        if (ratio > worstRatio)
                        {
                            worstRatio = ratio;
                            worstPair = $"{from.AnchorId}->{to.AnchorId}";
                        }
                        if (ratio > MaxDetourRatio)
                        {
                            Debug.LogError($"{Tag} Detour {from.AnchorId}->{to.AnchorId}: " +
                                           $"straight={straight:F2}u graph={travelled:F2}u ratio={ratio:F1}x " +
                                           $"(limit {MaxDetourRatio:F1}x) waypoints={route.Count} " +
                                           $"from={from.GetPosition()} to={to.GetPosition()}");
                            errors++;
                        }
                    }
                }

                if (checkedPairs == 0)
                {
                    Debug.LogWarning($"{Tag} No anchor pairs to check; schedule anchors missing from the scene.");
                    warnings++;
                }
            }
            finally
            {
                if (openedHere && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log($"{Tag} Checked {checkedPairs} anchor pair(s); worst ratio {worstRatio:F1}x on {worstPair}. " +
                      $"Validation finished. Errors={errors}, Warnings={warnings}");
            if (errors > 0) Debug.LogError($"{Tag} VALIDATION FAILED with {errors} error(s).");
        }
    }
}
