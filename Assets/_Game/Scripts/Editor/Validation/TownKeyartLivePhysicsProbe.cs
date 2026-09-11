using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
using CindarsHope.Player;
using CindarsHope.World.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Play Mode queries with the actual actor collider; never drives input or interacts.</summary>
    internal static class TownKeyartLivePhysicsProbe
    {
        private const float Step = 0.5f;
        private const int HitLimit = 128;
        // 160x112 expands the traversable search envelope by ~66% over the former 120x90 Town.
        // Keep enough headroom for obstacle detours so coverage, rather than the probe budget, ends the run.
        private const int NodeLimit = 90000;
        private const double TimeLimitSeconds = 45;
        private static readonly Vector2Int[] Directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

        [Serializable] internal sealed class Route
        {
            public string id, originId, status = "NOT_RUN", reason;
            public Vector2 requested, reached;
            public float tolerance;
            public Vector2[] path = Array.Empty<Vector2>();
        }

        [Serializable] internal sealed class Evidence
        {
            public string status = "FAIL", reason, startedUtc, finishedUtc, colliderType;
            public string method = "Live player Collider2D.OverlapCollider and Cast using its real collision mask. Cardinal BFS0.5u; exact swept door/mural endpoints, NPC work approach within1.25u because the NPC may occupy its own anchor. One component flood from town_default plus an exact swept connector to town_from_farm; second-origin routes reuse/reverse only proven edges. Dynamic actors remain present in this synchronous physics snapshot. No input, walking, interaction, save/load or animation proof.";
            public int expectedDoors = 24, expectedWork = 28, expectedSpawns = 2, visitedNodes, nodeLimit = NodeLimit, layerMask;
            public float step = Step, querySeconds;
            public Vector2 colliderOffset;
            public Vector3 colliderWorldSize, colliderLossyScale;
            public List<Route> routes = new List<Route>();
            public bool completeCoverage;
            public int accessPasses, accessFailures, npcSolidCount;
            public List<AccessScenario> accessScenarios = new List<AccessScenario>();
        }

        [Serializable] internal sealed class AccessScenario
        {
            public string id,status,detail;
        }

        [Serializable] internal sealed class DoorSweep
        {
            public string status = "FAIL", reason, colliderType, blockerPath;
            public Vector2 from, to;
            public bool sourceClear, destinationClear, blockerHit, segmentClear;
            public int layerMask;
            public string[] hitObjects = Array.Empty<string>();
        }

        /// <summary>Measures the same portal segment before/after a public door interaction. No physics steps or callbacks are synthesized.</summary>
        internal static DoorSweep ProbeDoorSegment(PlayerController player, Collider2D blocker, Vector2 from, Vector2 to)
        {
            var result = new DoorSweep { from = from, to = to };
            var original = player.transform.position;
            var body = player.GetComponent<Rigidbody2D>();
            var velocity = body != null ? body.linearVelocity : Vector2.zero;
            try
            {
                if (!Application.isPlaying || blocker == null || blocker.isTrigger || !blocker.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("A live active door blocker reference is required (it may be disabled only while open).");
                var solid = PlayerSolid(player);
                var filter = PlayerFilter(solid);
                var overlaps = new Collider2D[HitLimit];
                var hits = new RaycastHit2D[HitLimit];
                result.colliderType = solid.GetType().Name;
                result.blockerPath = ObjectPath(blocker.transform);
                result.layerMask = Physics2D.GetLayerCollisionMask(solid.gameObject.layer);
                var delta = to - from;
                if (delta.sqrMagnitude < .0001f) throw new InvalidOperationException("Door sweep endpoints must be distinct.");
                SetPosition(player, from);
                result.sourceClear = Clear(solid, filter, overlaps, player.transform);
                int count = solid.Cast(delta.normalized, filter, hits, delta.magnitude);
                if (count >= hits.Length) throw new InvalidOperationException("Door sweep buffer saturated; query incomplete.");
                var names = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    var hit = hits[i].collider;
                    if (hit == null || hit.transform.IsChildOf(player.transform)) continue;
                    names.Add(ObjectPath(hit.transform));
                    if (hit == blocker) result.blockerHit = true;
                }
                result.hitObjects = names.ToArray();
                SetPosition(player, to);
                result.destinationClear = Clear(solid, filter, overlaps, player.transform);
                result.segmentClear = result.sourceClear && result.destinationClear && names.Count == 0;
                result.status = "MEASURED";
                result.reason = "Real player collider cast and endpoint overlaps; closed/open expectations are evaluated by the house phase, not inferred from this measurement.";
            }
            catch (Exception error) { result.reason = error.ToString(); }
            finally
            {
                SetPosition(player, original);
                if (body != null) body.linearVelocity = velocity;
            }
            return result;
        }

        private sealed class Target
        {
            internal string id;
            internal Vector2 point;
            internal float tolerance;
            internal bool exact;
        }

        internal static Evidence Evaluate(PlayerController player)
        {
            var result = new Evidence { startedUtc = DateTime.UtcNow.ToString("O") };
            var original = player.transform.position;
            var body = player.GetComponent<Rigidbody2D>();
            var velocity = body != null ? body.linearVelocity : Vector2.zero;
            var clock = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!Application.isPlaying) throw new InvalidOperationException("Live Town probe requires Play Mode.");
                var scene = player.gameObject.scene;
                if (Components<PlatformEffector2D>(scene).Any(e => e.enabled))
                    throw new InvalidOperationException("One-way effectors require directional per-spawn probes; route reversal is not claimed here.");
                var solid = PlayerSolid(player);
                var filter = PlayerFilter(solid);
                var overlaps = new Collider2D[HitLimit];
                var hits = new RaycastHit2D[HitLimit];
                result.colliderType = solid.GetType().Name;
                result.colliderOffset = solid.offset;
                result.colliderWorldSize = solid.bounds.size;
                result.colliderLossyScale = solid.transform.lossyScale;
                result.layerMask = Physics2D.GetLayerCollisionMask(solid.gameObject.layer);
                var spawns = Components<SceneSpawnPoint>(scene).ToArray();
                if (spawns.Length != 2) throw new InvalidOperationException("Expected exactly two materialized Town spawns.");
                var primary = spawns.Single(p => p.SpawnId == "town_default");
                var secondary = spawns.Single(p => p.SpawnId == "town_from_farm");
                var targets = Targets(scene);
                targets.Add(new Target { id = "spawn_link:town_from_farm", point = secondary.Position, exact = true, tolerance = 0.001f });
                var origin = primary.Position;
                SetPosition(player, origin);
                if (!Clear(solid, filter, overlaps, player.transform)) throw new InvalidOperationException("town_default overlaps a solid collider after Awake.");
                var parents = new Dictionary<Vector2Int, Vector2Int> { [Vector2Int.zero] = Vector2Int.zero };
                var occupied = new HashSet<Vector2Int>();
                var queue = new Queue<Vector2Int>();
                var reached = new Dictionary<string, Vector2Int>();
                queue.Enqueue(Vector2Int.zero);
                bool exhaustedBudget = false;
                while (queue.Count > 0 && reached.Count < targets.Count)
                {
                    if (clock.Elapsed.TotalSeconds > TimeLimitSeconds || parents.Count >= NodeLimit) { exhaustedBudget = true; break; }
                    var cell = queue.Dequeue();
                    var position = origin + (Vector2)cell * Step;
                    SetPosition(player, position);
                    foreach (var target in targets)
                    {
                        if (reached.ContainsKey(target.id)) continue;
                        float distance = Vector2.Distance(position, target.point);
                        if (target.exact)
                        {
                            if (distance <= Step && SweepTo(player, solid, filter, overlaps, hits, position, target.point)) reached.Add(target.id, cell);
                            SetPosition(player, position);
                        }
                        else if (distance <= target.tolerance) reached.Add(target.id, cell);
                    }
                    foreach (var direction in Directions)
                    {
                        var next = cell + direction;
                        if (parents.ContainsKey(next) || occupied.Contains(next)) continue;
                        var point = origin + (Vector2)next * Step;
                        if (Mathf.Abs(point.x) > TownDistrictLayout.HalfWidth - .5f || Mathf.Abs(point.y) > TownDistrictLayout.HalfHeight - .5f) continue;
                        SetPosition(player, position);
                        int count = solid.Cast((Vector2)direction, filter, hits, Step);
                        if (count >= hits.Length) throw new InvalidOperationException("Player sweep buffer saturated; no PASS can be inferred.");
                        bool blockedEdge = false;
                        for (int i = 0; i < count; i++)
                            if (hits[i].collider != null && !hits[i].collider.transform.IsChildOf(player.transform)) { blockedEdge = true; break; }
                        // A rejected edge says nothing about access through another neighbor.
                        if (blockedEdge) continue;
                        SetPosition(player, point);
                        if (!Clear(solid, filter, overlaps, player.transform)) { occupied.Add(next); continue; }
                        parents.Add(next, cell);
                        queue.Enqueue(next);
                    }
                }
                result.visitedNodes = parents.Count;
                var proven = new Dictionary<string, Vector2[]>();
                foreach (var target in targets)
                {
                    var route = new Route { id = target.id, originId = primary.SpawnId, requested = target.point, tolerance = target.tolerance };
                    if (reached.TryGetValue(target.id, out var end))
                    {
                        var path = PathTo(origin, parents, end);
                        if (target.exact) path.Add(target.point);
                        route.path = path.ToArray();
                        route.reached = path[path.Count - 1];
                        route.status = "PASS";
                        proven.Add(target.id, route.path);
                    }
                    else
                    {
                        route.status = exhaustedBudget ? "NOT_RUN" : "FAIL";
                        route.reason = exhaustedBudget ? "Query budget exhausted before this endpoint was reached." : "Not connected by the sampled live player-collider graph.";
                        if (!exhaustedBudget)
                        {
                            var nearest = parents.Keys.OrderBy(cell => Vector2.Distance(origin + (Vector2)cell * Step, target.point)).First();
                            float nearestDistance = Vector2.Distance(origin + (Vector2)nearest * Step, target.point);
                            route.reason += $" Nearest reached cell={nearestDistance:0.###}u; endpoint blockers={EndpointBlockers(player, solid, filter, overlaps, target.point)}.";
                        }
                    }
                    result.routes.Add(route);
                }
                bool linked = proven.TryGetValue("spawn_link:town_from_farm", out var secondaryPath);
                foreach (var target in targets.Where(t => !t.id.StartsWith("spawn_link:")))
                {
                    var route = new Route { id = target.id, originId = secondary.SpawnId, requested = target.point, tolerance = target.tolerance };
                    if (linked && proven.TryGetValue(target.id, out var destinationPath))
                    {
                        var path = new List<Vector2>(secondaryPath.Reverse());
                        path.AddRange(destinationPath.Skip(1));
                        route.path = path.ToArray(); route.reached = path[path.Count - 1]; route.status = "PASS";
                    }
                    else { route.status = exhaustedBudget ? "NOT_RUN" : "FAIL"; route.reason = "Second spawn connector or destination lacks a proven route."; }
                    result.routes.Add(route);
                }
                // 53 destinations per spawn, plus one explicitly tested inter-spawn connector.
                result.completeCoverage = result.routes.Count == 107 && result.routes.All(r => r.status != "NOT_RUN");
                EvaluateAccessScenarios(result,scene,player,solid,primary,secondary,proven,targets);
                result.status = result.completeCoverage && result.routes.All(r => r.status == "PASS")&&result.accessFailures==0&&result.accessPasses==8
                    ? "PASS" : exhaustedBudget ? "PARTIAL" : "FAIL";
                result.reason = result.status == "PASS" ? "All 106 requested routes and inter-spawn connector were proven for this live physics snapshot." : "Inspect per-route results; unvisited targets never count as PASS.";
            }
            catch (Exception error) { result.reason = error.ToString(); }
            finally
            {
                SetPosition(player, original);
                if (body != null) body.linearVelocity = velocity;
                result.querySeconds = (float)clock.Elapsed.TotalSeconds;
                result.finishedUtc = DateTime.UtcNow.ToString("O");
            }
            if(result.accessScenarios.Count==8)
            {
                string summary=$"[TownAccessPlayMode] PASS:{result.accessPasses} FAIL:{result.accessFailures}";
                if(result.accessFailures==0)Debug.Log(summary);else Debug.LogError(summary);
            }
            else Debug.LogError("[TownAccessPlayMode] NOT_RUN: eight scenarios were not completed; "+result.reason);
            return result;
        }

        private static void EvaluateAccessScenarios(Evidence result,Scene scene,PlayerController player,Collider2D playerSolid,
            SceneSpawnPoint primary,SceneSpawnPoint secondary,Dictionary<string,Vector2[]> proven,List<Target> targets)
        {
            var actorRoots=Components<NpcShopController>(scene).Select(c=>c.gameObject)
                .Concat(Components<NpcController>(scene).Select(c=>c.gameObject)).Distinct().ToArray();
            var npcSolids=actorRoots.Select(root=>root.GetComponentsInChildren<Collider2D>(true)
                .SingleOrDefault(c=>c.enabled&&!c.isTrigger)).Where(c=>c!=null).ToArray();
            result.npcSolidCount=npcSolids.Length;
            Scenario(result,"actors",playerSolid!=null&&actorRoots.Length==29&&npcSolids.Length==29,
                $"player={playerSolid?.GetType().Name??"missing"} npcs={actorRoots.Length} npcSolids={npcSolids.Length}");
            Scenario(result,"defaultSpawn",ClearAt(player,playerSolid,primary.Position),"real player collider at town_default");
            Scenario(result,"fromFarmSpawn",ClearAt(player,playerSolid,secondary.Position),"real player collider at town_from_farm");
            Scenario(result,"doors",targets.Count(t=>t.id.StartsWith("door:",StringComparison.Ordinal)&&proven.ContainsKey(t.id))==24,
                "24 exact swept door approaches from town_default");
            Scenario(result,"work",targets.Count(t=>t.id.StartsWith("work:",StringComparison.Ordinal)&&proven.ContainsKey(t.id))==28,
                "28 work approaches from town_default");
            Scenario(result,"mural",proven.ContainsKey("mural"),"exact swept Town Hall mural endpoint");
            Scenario(result,"spawnConnector",proven.ContainsKey("spawn_link:town_from_farm"),"both spawns in one proven component");
            string bidirectionalDetail = "npc solid roster incomplete";
            bool bidirectional = false;
            if (npcSolids.Length == 29)
                bidirectional = ProbeBidirectionalActors(player, playerSolid, npcSolids[0], out bidirectionalDetail);
            Scenario(result,"bidirectionalActors",bidirectional,
                "real Player and real NPC solid bodies cast both directions on main/district/local corridors; "+bidirectionalDetail);
        }

        private static void Scenario(Evidence result,string id,bool ok,string detail)
        {
            result.accessScenarios.Add(new AccessScenario{id=id,status=ok?"PASS":"FAIL",detail=detail});
            if(ok)result.accessPasses++;else result.accessFailures++;
        }

        private static bool ClearAt(PlayerController player,Collider2D solid,Vector2 point)
        {
            var original=player.transform.position;var body=player.GetComponent<Rigidbody2D>();var velocity=body!=null?body.linearVelocity:Vector2.zero;
            try
            {
                SetPosition(player,point);return Clear(solid,PlayerFilter(solid),new Collider2D[HitLimit],player.transform);
            }
            finally{SetPosition(player,original);if(body!=null)body.linearVelocity=velocity;}
        }

        private static bool ProbeBidirectionalActors(PlayerController player,Collider2D playerSolid,Collider2D npcSolid,out string detail)
        {
            detail="not evaluated";
            var roadGroups=TownCityLayout.AllRoads.GroupBy(r=>TownAccessMetrics.RoadClass(r.Id)).ToArray();
            if(roadGroups.Length!=3){detail=$"roadClasses={roadGroups.Length}/3";return false;}
            var playerOriginal=player.transform.position;var playerBody=player.GetComponent<Rigidbody2D>();var playerVelocity=playerBody!=null?playerBody.linearVelocity:Vector2.zero;
            Transform npcRoot=npcSolid.attachedRigidbody!=null?npcSolid.attachedRigidbody.transform:npcSolid.transform.root;
            var npcBody=npcSolid.attachedRigidbody;Vector2 npcOriginal=npcRoot.position;var npcVelocity=npcBody!=null?npcBody.linearVelocity:Vector2.zero;
            try
            {
                foreach(var group in roadGroups)
                {
                    bool classPassed=false;
                    foreach(var road in group.OrderByDescending(r=>Vector2.Distance(r.Start,r.End)))
                    {
                        SetActorPosition(npcRoot,npcBody,npcOriginal);
                        if(!TryProbeRoad(player.transform,playerBody,playerSolid,road))continue;
                        // Keep the player away while the NPC body proves this same physical segment.
                        SetPosition(player,TownDistrictLayout.DefaultSpawn);
                        if(!TryProbeRoad(npcRoot,npcBody,npcSolid,road))continue;
                        classPassed=true;break;
                    }
                    if(!classPassed){detail="no shared clear segment for "+group.Key;return false;}
                }
                detail="player+npc clear on all three road classes";
                return true;
            }
            finally
            {
                SetPosition(player,playerOriginal);if(playerBody!=null)playerBody.linearVelocity=playerVelocity;
                SetActorPosition(npcRoot,npcBody,npcOriginal);if(npcBody!=null)npcBody.linearVelocity=npcVelocity;
            }
        }

        private static string EndpointBlockers(PlayerController player,Collider2D solid,ContactFilter2D filter,Collider2D[] overlaps,Vector2 point)
        {
            Vector2 original=player.transform.position;
            try
            {
                SetPosition(player,point);
                int count=solid.OverlapCollider(filter,overlaps);
                if(count>=overlaps.Length)return "BUFFER_SATURATED";
                var names=new List<string>();
                for(int i=0;i<count;i++)if(overlaps[i]!=null&&!overlaps[i].transform.IsChildOf(player.transform))names.Add(ObjectPath(overlaps[i].transform));
                return names.Count==0?"none":string.Join("|",names.Distinct());
            }
            finally{SetPosition(player,original);}
        }

        private static bool TryProbeRoad(Transform root,Rigidbody2D body,Collider2D solid,TownRoadSegment road)
        {
            Vector2 direction=(road.End-road.Start).normalized;if(direction.sqrMagnitude<.9f)return false;
            for(int sample=2;sample<=8;sample++)
            {
                Vector2 middle=Vector2.Lerp(road.Start,road.End,sample/10f),a=middle-direction,b=middle+direction;
                if(ProbeActor(root,body,solid,a,b)&&ProbeActor(root,body,solid,b,a))return true;
            }
            return false;
        }

        private static bool ProbeActor(Transform root,Rigidbody2D body,Collider2D solid,Vector2 from,Vector2 to)
        {
            SetActorPosition(root,body,from);var filter=PlayerFilter(solid);var overlaps=new Collider2D[HitLimit];
            if(!Clear(solid,filter,overlaps,root))return false;
            var hits=new RaycastHit2D[HitLimit];Vector2 delta=to-from;int count=solid.Cast(delta.normalized,filter,hits,delta.magnitude);
            if(count>=hits.Length)return false;
            for(int i=0;i<count;i++)if(hits[i].collider!=null&&!hits[i].collider.transform.IsChildOf(root))return false;
            SetActorPosition(root,body,to);return Clear(solid,filter,overlaps,root);
        }

        private static void SetActorPosition(Transform root,Rigidbody2D body,Vector2 position)
        {
            root.position=new Vector3(position.x,position.y,root.position.z);
            if(body!=null){body.position=position;body.linearVelocity=Vector2.zero;}Physics2D.SyncTransforms();
        }

        private static List<Target> Targets(Scene scene)
        {
            var objects = Components<Transform>(scene).ToArray();
            var targets = new List<Target>();
            if (TownCityLayout.AllBuildings.Count != 24) throw new InvalidOperationException("Town layout no longer contains24 houses.");
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                var house = objects.Single(t => t.name == lot.Name);
                if (Vector2.Distance(house.position, lot.Center) > .01f) throw new InvalidOperationException("Saved scene and current lot disagree: " + lot.Name);
                targets.Add(new Target { id = "door:" + lot.Name, point = lot.DoorApproach, exact = true, tolerance = .001f });
            }
            var work = Components<NpcScheduleAnchor>(scene).Where(a => a.AnchorId.EndsWith("_work")).ToArray();
            if (work.Length != 28 || work.Select(a => a.AnchorId).Distinct().Count() != 28) throw new InvalidOperationException("Expected28 unique materialized NPC work anchors.");
            foreach (var anchor in work) targets.Add(new Target { id = "work:" + anchor.AnchorId, point = anchor.transform.position, exact = false, tolerance = 1.25f });
            var mural = objects.Single(t => t.name == "TownHallMural_Interactable");
            if (Vector2.Distance(mural.position, TownDistrictLayout.TownHallMural) > .01f) throw new InvalidOperationException("Saved mural and current Town layout disagree.");
            targets.Add(new Target { id = "mural", point = mural.position, exact = true, tolerance = .001f });
            return targets;
        }

        private static List<Vector2> PathTo(Vector2 origin, Dictionary<Vector2Int, Vector2Int> parents, Vector2Int end)
        {
            var path = new List<Vector2>();
            for (var cell = end; ; cell = parents[cell]) { path.Add(origin + (Vector2)cell * Step); if (cell == Vector2Int.zero) break; }
            path.Reverse(); return path;
        }

        internal static bool TryFrame(PlayerController player, Vector2 desired, bool exact, out Vector2 actual)
        {
            var original = player.transform.position;
            var solid = PlayerSolid(player); var filter = PlayerFilter(solid); var overlaps = new Collider2D[HitLimit];
            var offsets = exact ? new[] { Vector2.zero } : new[] { Vector2.zero, Vector2.left * .5f, Vector2.right * .5f, Vector2.up * .5f, Vector2.down * .5f, Vector2.left, Vector2.right, Vector2.up, Vector2.down };
            foreach (var offset in offsets)
            {
                SetPosition(player, desired + offset);
                if (Clear(solid, filter, overlaps, player.transform)) { actual = desired + offset; return true; }
            }
            SetPosition(player, original); actual = original; return false;
        }

        private static bool SweepTo(PlayerController player, Collider2D solid, ContactFilter2D filter, Collider2D[] overlaps, RaycastHit2D[] hits, Vector2 from, Vector2 to)
        {
            SetPosition(player, from); var delta = to - from;
            if (delta.sqrMagnitude > .0000001f)
            {
                int count = solid.Cast(delta.normalized, filter, hits, delta.magnitude);
                if (count >= hits.Length) throw new InvalidOperationException("Exact target sweep buffer saturated.");
                for (int i = 0; i < count; i++) if (hits[i].collider != null && !hits[i].collider.transform.IsChildOf(player.transform)) return false;
            }
            SetPosition(player, to); return Clear(solid, filter, overlaps, player.transform);
        }

        private static bool Clear(Collider2D solid, ContactFilter2D filter, Collider2D[] overlaps, Transform player)
        {
            int count = solid.OverlapCollider(filter, overlaps);
            if (count >= overlaps.Length) throw new InvalidOperationException("Player overlap buffer saturated.");
            for (int i = 0; i < count; i++) if (!overlaps[i].transform.IsChildOf(player)) return false;
            return true;
        }

        internal static Collider2D PlayerSolid(PlayerController player) => player.GetComponentsInChildren<Collider2D>().Single(c => c.enabled && !c.isTrigger);
        private static ContactFilter2D PlayerFilter(Collider2D solid)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(solid.gameObject.layer)); return filter;
        }

        internal static void SetPosition(PlayerController player, Vector2 position)
        {
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            var body = player.GetComponent<Rigidbody2D>(); if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
            Physics2D.SyncTransforms();
        }

        private static IEnumerable<T> Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
        private static string ObjectPath(Transform value) => value.parent == null ? value.name : ObjectPath(value.parent) + "/" + value.name;
    }
}
