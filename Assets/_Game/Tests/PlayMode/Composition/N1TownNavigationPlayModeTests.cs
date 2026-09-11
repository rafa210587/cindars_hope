using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CindarsHope.NPC;
using CindarsHope.NPC.Runtime;
using CindarsHope.World;
using CindarsHope.Foundation;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Save.Providers;
using CindarsHope.NPC.Schedule;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.NPC
{
    /// <summary>Dedicated N1 harness: no Camera.Render and no visual/hash preflight.</summary>
    public sealed class N1TownNavigationPlayModeTests
    {
        [SetUp]
        public void AllowUnrelatedTownBootstrapDiagnostics() => LogAssert.ignoreFailingMessages = true;

        [UnityTest]
        public IEnumerator TownScene_LongRoute_UsesPhysicalBodyWithoutTeleport()
        {
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var graph = Object.FindFirstObjectByType<NpcTownRouteGraph>();
            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.ValidateConnectivity(out var reachable));
            Assert.GreaterOrEqual(reachable, 2400);
            var route = new List<Vector3>();
            Assert.IsTrue(graph.TryBuildRoute(new Vector3(-40f, -30f), new Vector3(40f, 30f), route));
            Assert.Greater(route.Count, 3, "long route must cross materialized Town waypoints");

            var actor = new GameObject("N1_PhysicalRouteProbe");
            var body = actor.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Dynamic;
            actor.AddComponent<CircleCollider2D>().radius = 0.15f;
            var wanderer = actor.AddComponent<NpcWanderer>();
            actor.transform.position = route[0];
            wanderer.SetRoute(route, 0.15f);
            var previous = actor.transform.position;
            var moved = 0f;
            var maxFrameDelta = 0f;
            for (var i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
                var delta = Vector3.Distance(previous, actor.transform.position);
                maxFrameDelta = Mathf.Max(maxFrameDelta, delta);
                moved += delta;
                previous = actor.transform.position;
            }
            Assert.Greater(moved, 0.02f);
            Assert.Less(maxFrameDelta, 0.2f, "route movement must not teleport onscreen");
            Object.Destroy(actor);
        }

        [UnityTest]
        public IEnumerator TownScene_RealNpc_AutoOpensRealDoor_EntersAndExitsWithoutCrossingFacade()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var service = Object.FindFirstObjectByType<NpcScheduleService>();
            if (service != null) service.enabled = false;
            var house = FindNamedObject("House_GateKeeper");
            Assert.IsNotNull(house, "canonical GateKeeper building must exist");
            var door = house.GetComponentInChildren<HouseDoorInteractable>(true);
            Assert.IsNotNull(door, "GateKeeper must use the real authored HouseDoorInteractable");
            var blocker = FindDoorBlocker(door);
            var sensor = FindDoorSensor(door);
            Assert.IsNotNull(blocker, "real door needs a solid facade blocker");
            Assert.IsNotNull(sensor, "real door needs an NPC proximity sensor");
            Assert.Greater(house.GetComponentsInChildren<SpriteRenderer>(true).Length, 0,
                "door must belong to a rendered Town building, not a test fixture");

            var actor = FindNamedObject("NPC_Alaric_GuardPost");
            Assert.IsNotNull(actor, "canonical resident NPC Alaric must exist");
            var dweller = actor.GetComponent<NpcDweller>();
            var wanderer = actor.GetComponent<NpcWanderer>();
            var actorCollider = actor.GetComponent<Collider2D>();
            Assert.IsNotNull(dweller);
            Assert.AreEqual("npc_alaric", dweller.NpcId);
            Assert.IsNotNull(wanderer);
            Assert.IsNotNull(actorCollider);
            var original = actor.transform.position;
            try
            {
                DisableOtherNpcColliders(actor);
                door.enabled = false;
                yield return null;
                door.enabled = true;
                yield return null;
                wanderer.ClearDestination();
                wanderer.ConfigureMovement(2.5f, 0f, 100f, 100f, new Vector2(-60f, -60f), new Vector2(60f, 60f));
                var center = blocker.bounds.center;
                var outside = new Vector3(center.x, sensor.bounds.min.y - 3.5f, 0f);
                var inside = new Vector3(center.x, sensor.bounds.max.y + 0.45f, 0f);
                actor.transform.position = outside;
                Physics2D.SyncTransforms();
                Assert.IsTrue(blocker.enabled, "facade blocker starts closed");
                wanderer.SetRoute(new[] { outside, (Vector3)center, inside }, 0.12f);
                var sawAnimation = false;
                var sawBlockerRelease = false;
                var crossedInside = false;
                for (var i = 0; i < 400; i++)
                {
                    yield return new WaitForFixedUpdate();
                    sawAnimation |= door.IsAnimating;
                    sawBlockerRelease |= !blocker.enabled;
                    if (blocker.enabled)
                        Assert.LessOrEqual(actor.transform.position.y, center.y + 0.05f,
                            "NPC center may not cross the closed facade blocker");
                    if (actor.transform.position.y > center.y + 0.25f) { crossedInside = true; break; }
                }
                Assert.IsTrue(sawAnimation, "real NPC proximity must start the authored animation automatically");
                Assert.IsTrue(sawBlockerRelease, "blocker must release only through the door transition");
                Assert.IsTrue(crossedInside, "real NPC body must enter the GateKeeper interior");

                wanderer.SetRoute(new[] { actor.transform.position, (Vector3)center, outside }, 0.12f);
                var exited = false;
                for (var i = 0; i < 400; i++)
                {
                    yield return new WaitForFixedUpdate();
                    if (actor.transform.position.y < center.y - 0.25f) { exited = true; break; }
                }
                Assert.IsTrue(exited, "real NPC body must exit through the same real doorway");
                for (var i = 0; i < 150 && (!blocker.enabled || door.IsAnimating); i++) yield return new WaitForFixedUpdate();
                Assert.IsTrue(blocker.enabled,
                    $"facade blocker must restore after resident leaves; actor={actor.transform.position} sensor={sensor.bounds} open={door.IsOpen} anim={door.IsAnimating}");
            }
            finally
            {
                actor.transform.position = original;
                wanderer.ClearDestination();
            }
        }

        [UnityTest]
        public IEnumerator Recovery_RealObstacle_UsesBoundedReplansAndNoTeleport()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            const string recoveryNpcId = "npc_alaric";
            var actor = FindNamedObject("NPC_Alaric_GuardPost");
            Assert.IsNotNull(actor, "recovery proof must use the canonical Alaric scene actor");
            var wall = new GameObject("N1_InducedObstacle");
            var service = NpcScheduleRuntimeBootstrap.Install(null);
            service.enabled = true;
            // Let the scene's real load/bootstrap lifecycle finish before arranging the induced
            // obstacle; a late NpcManager restore must not overwrite the diagnostic start point.
            for (var settle = 0; settle < 140; settle++) yield return null;
            Application.LogCallback capture = null;
            var original = actor.transform.position;
            try
            {
                var body = actor.GetComponent<Rigidbody2D>();
                var actorCollider = actor.GetComponent<Collider2D>();
                var wanderer = actor.GetComponent<NpcWanderer>();
                var graph = Object.FindFirstObjectByType<NpcTownRouteGraph>();
                Assert.IsNotNull(body); Assert.IsNotNull(actorCollider); Assert.IsNotNull(wanderer); Assert.IsNotNull(graph);
                service.SetClock(new MutableClock { Hour = 12, Day = 1 });
                GameEventBus.Publish(new GameTimeTickEvent(12f, 1));
                foreach (var other in Object.FindObjectsByType<NpcWanderer>(FindObjectsInactive.Include,
                             FindObjectsSortMode.None))
                    if (other != wanderer) other.ClearDestination();
                wanderer.ClearDestination();
                service.ClearTrackedMovesForDiagnostics();
                Physics2D.SyncTransforms();
                yield return new WaitForFixedUpdate();
                var route = new List<Vector3>();
                var scheduledTarget = FindGraphNode(graph, "npc_alaric_social").Position;
                Assert.IsTrue(graph.TryBuildRoute(actor.transform.position, scheduledTarget, route, actorCollider),
                    $"real actor start must enter its canonical route: {graph.LastFailureDiagnostic}");
                Assert.Greater(route.Count, 3, "recovery must start from a real multi-segment Town route");
                var target = route[route.Count - 1];
                var segmentIndex = 1;
                while (segmentIndex < route.Count && (route[segmentIndex] - route[0]).sqrMagnitude <= 0.01f) segmentIndex++;
                Assert.Less(segmentIndex, route.Count, "route must contain a non-zero physical segment");
                var firstSegment = route[segmentIndex] - route[0];
                Assert.Greater(firstSegment.sqrMagnitude, 0.01f);
                wall.transform.position = route[0] + firstSegment.normalized * 1.5f;
                var wallCollider = wall.AddComponent<BoxCollider2D>();
                wallCollider.size = new Vector2(0.5f, 100f);
                wall.transform.rotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(firstSegment.y, firstSegment.x) * Mathf.Rad2Deg);
                var messages = new List<string>();
                capture = (condition, stackTrace, type) =>
                {
                    if (type == LogType.Warning && condition.Contains("[NpcScheduleService]")) messages.Add(condition);
                };
                Application.logMessageReceived += capture;
                var saveBefore = SnapshotPersistentFiles();
                Assert.IsTrue(service.BeginTrackedMoveForDiagnostics(recoveryNpcId, wanderer, target));
                var before = actor.transform.position;
                var previous = before;
                var maxStep = 0f;
                var recoveryDeadline = Time.time + 12f;
                while (Time.time < recoveryDeadline)
                {
                    yield return new WaitForFixedUpdate();
                    maxStep = Mathf.Max(maxStep, Vector2.Distance(previous, actor.transform.position));
                    previous = actor.transform.position;
                    if (messages.Exists(m => m.Contains($"Waiting at safe position npc={recoveryNpcId}"))) break;
                }
                Assert.Less(maxStep, 0.2f, "onscreen recovery must never teleport the actor");
                Assert.IsTrue(service.TryGetMoveDiagnostic(recoveryNpcId, out var diagnostic));
                Assert.That(diagnostic.Replans, Is.InRange(1, NpcRouteRecoveryPolicy.MaxReplans),
                    "real blocked movement must replan but never exceed the bounded budget");
                Assert.IsTrue(messages.Exists(m => m.Contains("Replan 1/2") &&
                    m.Contains("reason=insufficient_progress") && m.Contains("from=") && m.Contains("to=")),
                    "replan log must expose reason/from/to");
                Assert.IsTrue(messages.Exists(m => m.Contains($"Waiting at safe position npc={recoveryNpcId}") &&
                    m.Contains("reason=route_blocked") && m.Contains("from=") && m.Contains("to=")),
                    "after the bounded budget the blocked actor must stop at its physical safe side");
                Assert.IsFalse(actorCollider.Distance(wallCollider).isOverlapped,
                    "recovery may wait in contact but must not place the NPC inside the obstacle");
                Assert.AreEqual(saveBefore, SnapshotPersistentFiles(), "onscreen replan/wait must not write save");
                Application.logMessageReceived -= capture;
            }
            finally
            {
                if (capture != null) Application.logMessageReceived -= capture;
                service.ResetDiagnosticMoveIsolation();
                var restoredBody = actor.GetComponent<Rigidbody2D>();
                if (restoredBody != null) restoredBody.position = original;
                actor.GetComponent<NpcWanderer>()?.ClearDestination();
                Object.Destroy(wall);
            }
        }

        [UnityTest]
        public IEnumerator NpcSaveProvider_LoadRestore_UsesScheduleSafeNodeAndNeverWritesSave()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var service = NpcScheduleRuntimeBootstrap.Install(null);
            Assert.IsNotNull(service);
            var manager = Object.FindFirstObjectByType<NpcManager>();
            Assert.IsNotNull(manager);
            var actor = FindNamedObject("NPC_Zrix_CaveRoad");
            Assert.IsNotNull(actor, "offscreen/load seam must operate on a canonical Town NPC");
            var original = actor.transform.position;
            var messages = new List<string>();
            Application.LogCallback capture = (condition, stackTrace, type) =>
            {
                if (condition.Contains("Offscreen safe snap")) messages.Add(condition);
            };
            try
            {
                var wanderer = actor.GetComponent<NpcWanderer>();
                Assert.IsNotNull(wanderer);
                Application.logMessageReceived += capture;
                var saveBefore = SnapshotPersistentFiles();
                var house = FindNamedObject("House_GateKeeper");
                var occupied = FindPermanentSolid(house).bounds.center;
                var data = new NpcManagerSaveData();
                data.Npcs.Add(new NpcSaveData { NpcId = "npc_zrix", SceneId = "TownScene", Position = occupied, HasMet = false });
                new NpcsSectionProvider(manager).Restore(data);
                var safe = actor.transform.position;
                Assert.AreNotEqual((Vector3)occupied, safe, "production restore must reject an occupied saved position");
                var graph = Object.FindFirstObjectByType<NpcTownRouteGraph>();
                Assert.IsTrue(GraphContainsPosition(graph, safe), "production restore must resolve to a serialized safe node");
                Assert.IsTrue(messages.Exists(m => m.Contains("npc=npc_zrix") &&
                    m.Contains("reason=load_restore_safe_node") && m.Contains("from=") && m.Contains("to=")));
                Assert.That(SnapshotPersistentFiles(), Is.EqualTo(saveBefore),
                    "offscreen snap must not write any persistent save file");
            }
            finally
            {
                Application.logMessageReceived -= capture;
                actor.transform.position = original;
            }
        }

        [UnityTest]
        public IEnumerator TownScene_CorridorBodies_DoNotInterpenetrate_AndGateKeeperResumes()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var service = Object.FindFirstObjectByType<NpcScheduleService>();
            if (service != null) service.enabled = false;
            var first = FindNamedObject("NPC_Alaric_GuardPost");
            var second = FindNamedObject("NPC_Hund_GuardRoute");
            Assert.IsNotNull(first); Assert.IsNotNull(second);
            var originalA = first.transform.position;
            var originalB = second.transform.position;
            try
            {
                var house = FindNamedObject("House_GateKeeper");
                var door = house != null ? house.GetComponentInChildren<HouseDoorInteractable>(true) : null;
                var blocker = FindDoorBlocker(door);
                Assert.IsNotNull(door); Assert.IsNotNull(blocker);
                var graph = Object.FindFirstObjectByType<NpcTownRouteGraph>();
                Assert.IsNotNull(graph);
                var a = first.GetComponent<NpcWanderer>();
                var b = second.GetComponent<NpcWanderer>();
                var colliderA = first.GetComponent<Collider2D>();
                var colliderB = second.GetComponent<Collider2D>();
                Assert.IsNotNull(a); Assert.IsNotNull(b); Assert.IsNotNull(colliderA); Assert.IsNotNull(colliderB);
                a.ConfigureMovement(6f, 0f, 100f, 100f, new Vector2(-60f, -60f), new Vector2(60f, 60f));
                b.ConfigureMovement(6f, 0f, 100f, 100f, new Vector2(-60f, -60f), new Vector2(60f, 60f));
                var finish = FindGraphNode(graph, "npc_alaric_home").Position;
                var approachNode = FindAdjacentNode(graph, "npc_alaric_home").Position;
                var lateral = Vector3.Cross((finish - approachNode).normalized, Vector3.forward) * 0.22f;
                var startA = approachNode - lateral;
                var startB = approachNode + lateral;
                first.transform.position = startA;
                second.transform.position = startB;
                Physics2D.SyncTransforms();
                var routeA = new List<Vector3>();
                var routeB = new List<Vector3>();
                Assert.IsTrue(graph.TryBuildRoute(startA, finish, routeA));
                Assert.IsTrue(graph.TryBuildRoute(startB, finish, routeB));
                Assert.Greater(routeA.Count, 2); Assert.Greater(routeB.Count, 2);
                a.SetRoute(routeA, 0.12f);
                b.SetRoute(routeB, 0.12f);
                var excessiveOverlapSeconds = 0f;
                var maxExcessiveOverlapSeconds = 0f;
                var sawOrdinalYield = false;
                var releaseTime = -1f;
                var resumeA = -1f;
                var resumeB = -1f;
                var atReleaseA = first.transform.position;
                var atReleaseB = second.transform.position;
                for (var i = 0; i < 500; i++)
                {
                    yield return new WaitForFixedUpdate();
                    var separation = colliderA.Distance(colliderB);
                    if (separation.isOverlapped && separation.distance < -0.02f)
                        excessiveOverlapSeconds += Time.fixedDeltaTime;
                    else excessiveOverlapSeconds = 0f;
                    maxExcessiveOverlapSeconds = Mathf.Max(maxExcessiveOverlapSeconds, excessiveOverlapSeconds);
                    sawOrdinalYield |= b.YieldingToNpcId == "npc_alaric";
                    if (releaseTime < 0f && !blocker.enabled)
                    {
                        releaseTime = Time.time;
                        atReleaseA = first.transform.position;
                        atReleaseB = second.transform.position;
                    }
                    if (releaseTime >= 0f)
                    {
                        if (resumeA < 0f && Vector2.Distance(atReleaseA, first.transform.position) >= 0.10f) resumeA = Time.time;
                        if (resumeB < 0f && Vector2.Distance(atReleaseB, second.transform.position) >= 0.10f) resumeB = Time.time;
                        if (resumeA >= 0f && resumeB >= 0f) break;
                    }
                }
                Assert.LessOrEqual(maxExcessiveOverlapSeconds, 0.5f,
                    "penetration deeper than 0.02u may not persist for over 0.5s");
                Assert.IsTrue(sawOrdinalYield, "lexically later npc_hund must yield to npc_alaric");
                Assert.GreaterOrEqual(releaseTime, 0f, "real GateKeeper blocker must open by NPC proximity");
                Assert.That(resumeA - releaseTime, Is.InRange(0f, 5f));
                Assert.That(resumeB - releaseTime, Is.InRange(0f, 5f));
            }
            finally
            {
                first.transform.position = originalA; second.transform.position = originalB;
                first.GetComponent<NpcWanderer>()?.ClearDestination();
                second.GetComponent<NpcWanderer>()?.ClearDestination();
            }
        }

        [UnityTest]
        public IEnumerator NpcWanderer_ReissuedOccupiedAnchor_RemainsPausedWithoutDriftOrSaveMutation()
        {
            LogAssert.ignoreFailingMessages = false;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var go = new GameObject("NpcWanderer_ReissuedAnchor_Probe");
            go.transform.position = new Vector3(4f, 0f, 0f);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            var wanderer = go.AddComponent<NpcWanderer>();
            wanderer.ConfigureMovement(10f, 0f, 100f, 100f,
                new Vector2(-20f, -20f), new Vector2(20f, 20f));
            yield return null;
            wanderer.SetRoute(new[] { go.transform.position, new Vector3(6f, 0f, 0f) }, 0.08f);
            for (var i = 0; i < 30 && wanderer.HasActiveRoute; i++) yield return new WaitForFixedUpdate();
            Assert.IsFalse(wanderer.HasActiveRoute);
            var occupiedAnchor = go.transform.position;
            var saveBefore = SnapshotPersistentFiles();
            wanderer.SetRoute(new[] { occupiedAnchor }, 0.08f);
            for (var i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.LessOrEqual(Vector2.Distance(occupiedAnchor, go.transform.position), 0.01f,
                "reissuing an already occupied anchor must not wake the stale idle target");
            Assert.AreEqual(saveBefore, SnapshotPersistentFiles());
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator TownScene_PipReception_DoesNotCompeteWithScheduledLocomotion()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var pip = FindWandererByNpcId("npc_pip");
            Assert.IsNotNull(pip);
            Assert.IsNotNull(pip.GetComponent<PipReceptionController>());
            pip.ConfigureMovement(24f, 0f, 100f, 100f,
                new Vector2(-60f, -60f), new Vector2(60f, 60f));
            pip.ClearDestination();
            var schedule = Object.FindFirstObjectByType<NpcScheduleService>();
            if (schedule != null) schedule.enabled = false;
            var before = pip.transform.position;
            for (var i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
            Assert.LessOrEqual(Vector2.Distance(before, pip.transform.position), 0.01f,
                "PipReceptionController must preserve reception proximity without competing with NpcWanderer locomotion");
        }

        [UnityTest]
        public IEnumerator TownScene_AlaricAndVelorin_CompleteCriticalInteriorTransitionsWithoutRecovery()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var service = NpcScheduleRuntimeBootstrap.Install(null);
            for (var i = 0; i < 120 && service.RegisteredProfileCount < 28; i++) yield return null;
            var clock = new MutableClock { Hour = 0, Day = 1 };
            service.SetClock(clock);
            var ids = new[] { "npc_alaric", "npc_velorin" };
            var bodies = new Dictionary<string, NpcWanderer>();
            var allWanderers = Object.FindObjectsByType<NpcWanderer>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (var wanderer in allWanderers)
                wanderer.ConfigureMovement(24f, 0f, 100f, 100f,
                    new Vector2(-60f, -60f), new Vector2(60f, 60f));
            foreach (var id in ids)
            {
                var body = FindWandererByNpcId(id);
                Assert.IsNotNull(body);
                bodies[id] = body;
            }
            IgnoreNpcPairCollisions(allWanderers);
            var recovery = new List<string>();
            Application.LogCallback capture = (condition, stack, type) =>
            {
                if ((condition.Contains("npc_alaric") || condition.Contains("npc_velorin")) &&
                    (condition.Contains("Replan") || condition.Contains("Waiting at safe position")))
                    recovery.Add(condition);
            };
            Application.logMessageReceived += capture;
            try
            {
                foreach (var hour in new[] { 0, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 })
                {
                    clock.Hour = hour;
                    GameEventBus.Publish(new GameTimeTickEvent(hour, clock.Day));
                    // Exercise repeated same-anchor hourly orders through real FixedUpdate; a
                    // zero-length route must remain anchored rather than wake a stale idle target.
                    yield return new WaitForFixedUpdate();
                    for (var frame = 0; frame < 1600; frame++)
                    {
                        var complete = true;
                        foreach (var id in ids)
                        {
                            var suffix = NpcScheduleBlockResolver.AnchorSuffixForBlock(service.GetCurrentBlock(id, hour));
                            var target = FindScheduleAnchor($"{id}_{suffix}").GetPosition();
                            complete &= Vector2.Distance(bodies[id].transform.position, target) <= 0.01f &&
                                        !bodies[id].HasActiveRoute;
                        }
                        if (complete) break;
                        yield return new WaitForFixedUpdate();
                    }
                    foreach (var id in ids)
                    {
                        var suffix = NpcScheduleBlockResolver.AnchorSuffixForBlock(service.GetCurrentBlock(id, hour));
                        var target = FindScheduleAnchor($"{id}_{suffix}").GetPosition();
                        Assert.LessOrEqual(Vector2.Distance(bodies[id].transform.position, target), 0.01f,
                            $"{id} must physically complete critical hour {hour}; " +
                            $"pos={bodies[id].transform.position} target={target} " +
                            $"route={bodies[id].RouteIndex}/{bodies[id].RouteCount} " +
                            $"next={bodies[id].CurrentRouteTarget} yielding={bodies[id].YieldingToNpcId ?? "none"} " +
                            $"doorWait={bodies[id].WaitingForDoorName ?? "none"} blockers={DescribeSolidOverlaps(bodies[id])} " +
                            $"graph={Object.FindFirstObjectByType<NpcTownRouteGraph>()?.LastFailureDiagnostic}");
                        Assert.IsFalse(bodies[id].HasActiveRoute);
                    }
                }
                Assert.IsEmpty(recovery, string.Join(" | ", recovery));
            }
            finally { Application.logMessageReceived -= capture; }
        }

        [UnityTest]
        public IEnumerator TownScene_AcceleratedAgenda_ResolvesAllProfilesWithoutSaveMutation()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var service = NpcScheduleRuntimeBootstrap.Install(null);
            Assert.IsNotNull(service);
            for (var i = 0; i < 120 && service.RegisteredProfileCount < 28; i++)
                yield return null;
            Assert.GreaterOrEqual(service.RegisteredProfileCount, 28,
                "agenda validation must observe the bootstrap-registered profiles, not an empty service");
            var graph = Object.FindFirstObjectByType<NpcTownRouteGraph>();
            Assert.IsNotNull(graph);
            var clock = new MutableClock { Hour = 0, Day = 1 };
            service.SetClock(clock);
            var ids = new List<string>(service.RegisteredNpcIds);
            ids.Sort(System.StringComparer.Ordinal);
            Assert.AreEqual(28, ids.Count, "only the 28 canonical scheduled profiles enter the agenda runtime");
            var bodies = new Dictionary<string, NpcWanderer>();
            const float acceleratedSpeed = 24f;
            foreach (var npcId in ids)
            {
                var wanderer = FindWandererByNpcId(npcId);
                Assert.IsNotNull(wanderer, $"{npcId} must retain a real scene body");
                wanderer.ConfigureMovement(acceleratedSpeed, 0f, 100f, 100f,
                    new Vector2(-60f, -60f), new Vector2(60f, 60f));
                bodies[npcId] = wanderer;
            }
            IgnoreNpcPairCollisions(bodies.Values);
            var saveBefore = SnapshotPersistentFiles();
            var recoveryMessages = new List<string>();
            Application.LogCallback capture = (condition, stackTrace, type) =>
            {
                if (condition.Contains("Replan") || condition.Contains("Waiting at safe position"))
                    recoveryMessages.Add(condition);
            };
            Application.logMessageReceived += capture;
            try
            {
                var previousAnchors = new Dictionary<string, string>(System.StringComparer.Ordinal);
                var physicalTransitions = 0;
                var maximumFrameDisplacement = 0f;
                for (var hour = 0; hour < 24; hour++)
                {
                    clock.Hour = hour;
                    var moving = new Dictionary<string, (NpcWanderer wanderer, Vector3 target, Vector3 start)>();
                    foreach (var npcId in ids)
                    {
                        var suffix = NpcScheduleBlockResolver.AnchorSuffixForBlock(service.GetCurrentBlock(npcId, hour));
                        var anchorId = $"{npcId}_{suffix}";
                        if (previousAnchors.TryGetValue(npcId, out var previous) && previous == anchorId) continue;
                        var node = FindGraphNode(graph, anchorId);
                        var wanderer = bodies[npcId];
                        wanderer.ClearDestination();
                        moving[npcId] = (wanderer, node.Position, wanderer.transform.position);
                        previousAnchors[npcId] = anchorId;
                    }
                    GameEventBus.Publish(new GameTimeTickEvent(hour, clock.Day));
                    foreach (var pair in moving)
                    {
                        var directDistance = Vector2.Distance(pair.Value.start, pair.Value.target);
                        if (directDistance <= 2f) continue;
                        service.TryGetMoveDiagnostic(pair.Key, out var orderDiagnostic);
                        Assert.Greater(pair.Value.wanderer.RouteCount, 2,
                            $"{pair.Key} hour {hour} must use graph nodes instead of one unchecked direct segment; " +
                            $"start={pair.Value.start} current={pair.Value.wanderer.transform.position} target={pair.Value.target} " +
                            $"orderTarget={orderDiagnostic.Target} graph={graph.LastFailureDiagnostic}");
                    }
                    var arrived = new HashSet<string>(System.StringComparer.Ordinal);
                    var previousPositions = new Dictionary<string, Vector3>(System.StringComparer.Ordinal);
                    foreach (var pair in moving) previousPositions[pair.Key] = pair.Value.wanderer.transform.position;
                    for (var frame = 0; frame < 1600 && arrived.Count < moving.Count; frame++)
                    {
                        yield return new WaitForFixedUpdate();
                        foreach (var pair in moving)
                        {
                            var current = pair.Value.wanderer.transform.position;
                            maximumFrameDisplacement = Mathf.Max(maximumFrameDisplacement,
                                Vector2.Distance(previousPositions[pair.Key], current));
                            previousPositions[pair.Key] = current;
                            if (Vector2.Distance(pair.Value.wanderer.transform.position, pair.Value.target) <= 0.01f &&
                                !pair.Value.wanderer.HasActiveRoute)
                                arrived.Add(pair.Key);
                        }
                    }
                    var missing = new List<string>();
                    foreach (var pair in moving) if (!arrived.Contains(pair.Key))
                        missing.Add($"{pair.Key}@{pair.Value.wanderer.transform.position}->{pair.Value.target}" +
                                    $" route={pair.Value.wanderer.RouteIndex}/{pair.Value.wanderer.RouteCount}" +
                                    $" next={pair.Value.wanderer.CurrentRouteTarget} yielding={pair.Value.wanderer.YieldingToNpcId ?? "none"}" +
                                    $" doorWait={pair.Value.wanderer.WaitingForDoorName ?? "none"}" +
                                    $" blockers={DescribeSolidOverlaps(pair.Value.wanderer)}");
                    Assert.AreEqual(moving.Count, arrived.Count,
                        $"accelerated hour {hour} must physically complete every changed schedule route; missing={string.Join(",", missing)}");
                    foreach (var pair in moving)
                    {
                        var requiredTravel = Vector2.Distance(pair.Value.start, pair.Value.target);
                        if (requiredTravel > 0.5f)
                            Assert.Greater(Vector2.Distance(pair.Value.start, pair.Value.wanderer.transform.position), 0.25f,
                                $"{pair.Key} must move its real body for the hour {hour} transition");
                    }
                    physicalTransitions += arrived.Count;
                    foreach (var npcId in ids)
                    {
                        Assert.IsTrue(service.TryGetRuntimeState(npcId, out var state), $"missing state {npcId} at hour {hour}");
                        Assert.AreEqual(previousAnchors[npcId], state.CurrentAnchorId, $"wrong anchor {npcId} at hour {hour}");
                    }
                }
                Assert.GreaterOrEqual(physicalTransitions, 56,
                    "full-day acceleration must include at least two physical block transitions per profile on average");
                Assert.LessOrEqual(maximumFrameDisplacement, acceleratedSpeed * Time.fixedDeltaTime + 0.1f,
                    $"normal onscreen agenda movement must not teleport; max fixed-frame delta={maximumFrameDisplacement:F3}u");
                Assert.AreEqual(0, recoveryMessages.Count,
                    "normal accelerated routes require zero recovery/replan: " + string.Join(" | ", recoveryMessages));
                Assert.AreEqual(saveBefore, SnapshotPersistentFiles(), "accelerated agenda must not write save");
            }
            finally { Application.logMessageReceived -= capture; }
        }

        private static void IgnoreNpcPairCollisions(IEnumerable<NpcWanderer> wanderers)
        {
            var colliders = new List<Collider2D>();
            foreach (var wanderer in wanderers)
                if (wanderer != null) colliders.AddRange(wanderer.GetComponentsInChildren<Collider2D>(true));
            for (var i = 0; i < colliders.Count; i++)
                for (var j = i + 1; j < colliders.Count; j++)
                    if (colliders[i] != null && colliders[j] != null)
                        Physics2D.IgnoreCollision(colliders[i], colliders[j], true);
        }

        private static string DescribeSolidOverlaps(NpcWanderer wanderer)
        {
            Collider2D self = null;
            foreach (var candidate in wanderer.GetComponentsInChildren<Collider2D>(true))
                if (candidate != null && !candidate.isTrigger) { self = candidate; break; }
            var hits = Physics2D.OverlapCircleAll(wanderer.transform.position, 1.2f);
            var names = new List<string>();
            foreach (var hit in hits)
                if (hit != null && hit != self && !hit.isTrigger && hit.GetComponentInParent<NpcWanderer>() == null)
                {
                    var distance = self != null ? self.Distance(hit).distance : 0f;
                    names.Add((hit.transform.parent != null ? $"{hit.transform.parent.name}/{hit.name}" : hit.name) +
                              $"({distance:F2}u)");
                }
            return names.Count == 0 ? "none" : string.Join("+", names);
        }

        private sealed class MutableClock : CindarsHope.Foundation.Time.IGameClock
        {
            public int Hour;
            public int Day;
            public int CurrentDay => Day;
            public int CurrentHourOfDay => Hour;
            public bool IsDaytime => Hour >= 6 && Hour < 20;
        }

        private static NpcWanderer FindWandererByNpcId(string npcId)
        {
            var dwellers = Object.FindObjectsByType<NpcDweller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < dwellers.Length; i++)
                if (dwellers[i] != null && dwellers[i].NpcId == npcId)
                    return dwellers[i].GetComponent<NpcWanderer>();
            return null;
        }

        private static NpcScheduleAnchor FindScheduleAnchor(string anchorId)
        {
            var anchors = Object.FindObjectsByType<NpcScheduleAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < anchors.Length; i++)
                if (anchors[i] != null && anchors[i].AnchorId == anchorId) return anchors[i];
            return null;
        }

        private static void DisableOtherNpcColliders(GameObject retained)
        {
            var dwellers = Object.FindObjectsByType<NpcDweller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < dwellers.Length; i++)
            {
                if (dwellers[i] == null || dwellers[i].gameObject == retained) continue;
                var colliders = dwellers[i].GetComponentsInChildren<Collider2D>(true);
                for (var j = 0; j < colliders.Length; j++) colliders[j].enabled = false;
            }
        }

        private static NpcTownRouteNode FindGraphNode(NpcTownRouteGraph graph, string id)
        {
            Assert.IsNotNull(graph);
            for (var i = 0; i < graph.Nodes.Count; i++)
                if (graph.Nodes[i] != null && graph.Nodes[i].Id == id) return graph.Nodes[i];
            Assert.Fail($"serialized route node missing: {id}");
            return null;
        }

        private static bool TryFindGraphNode(NpcTownRouteGraph graph, string id, out NpcTownRouteNode node)
        {
            node = null;
            if (graph == null) return false;
            for (var i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i] == null || graph.Nodes[i].Id != id) continue;
                node = graph.Nodes[i];
                return true;
            }
            return false;
        }

        private static NpcTownRouteNode FindAdjacentNode(NpcTownRouteGraph graph, string id)
        {
            for (var i = 0; i < graph.Edges.Count; i++)
            {
                var edge = graph.Edges[i];
                if (edge.From == id) return FindGraphNode(graph, edge.To);
                if (edge.To == id) return FindGraphNode(graph, edge.From);
            }
            Assert.Fail($"serialized route edge missing: {id}");
            return null;
        }

        private static Collider2D FindDoorBlocker(HouseDoorInteractable door)
        {
            if (door == null) return null;
            if (door.PhysicalBlocker != null) return door.PhysicalBlocker;
            var colliders = door.transform.parent != null
                ? door.transform.parent.GetComponentsInChildren<Collider2D>(true)
                : door.GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < colliders.Length; i++)
                if (colliders[i] != null && !colliders[i].isTrigger && colliders[i].transform.IsChildOf(door.transform))
                    return colliders[i];
            for (var i = 0; i < colliders.Length; i++)
                if (colliders[i] != null && !colliders[i].isTrigger && colliders[i].name.Contains("Block"))
                    return colliders[i];
            return null;
        }

        private static Collider2D FindDoorSensor(HouseDoorInteractable door)
        {
            if (door == null) return null;
            var colliders = door.GetComponents<Collider2D>();
            for (var i = 0; i < colliders.Length; i++) if (colliders[i].isTrigger) return colliders[i];
            return null;
        }

        private static Collider2D FindPermanentSolid(GameObject root)
        {
            Assert.IsNotNull(root);
            var colliders = root.GetComponentsInChildren<Collider2D>(true);
            Collider2D best = null;
            var bestArea = 0f;
            for (var i = 0; i < colliders.Length; i++)
            {
                var candidate = colliders[i];
                if (candidate == null || !candidate.enabled || candidate.isTrigger ||
                    candidate.attachedRigidbody != null) continue;
                var area = candidate.bounds.size.x * candidate.bounds.size.y;
                if (area > bestArea) { best = candidate; bestArea = area; }
            }
            Assert.IsNotNull(best, "canonical building must contain an enabled permanent solid");
            return best;
        }

        private static Vector3 FindUnoccupiedSafeNode(NpcTownRouteGraph graph, Collider2D self)
        {
            Assert.IsNotNull(graph);
            for (var i = graph.Nodes.Count - 1; i >= 0; i--)
            {
                var node = graph.Nodes[i];
                if (node != null && node.Safe && NpcRouteRecoveryPolicy.IsSafePoint(node.Position, 0.3f, self))
                    return node.Position;
            }
            Assert.Fail("Town graph has no physically unoccupied safe node for offscreen recovery");
            return default;
        }

        private static bool GraphContainsPosition(NpcTownRouteGraph graph, Vector3 position)
        {
            if (graph == null) return false;
            for (var i = 0; i < graph.Nodes.Count; i++)
                if (graph.Nodes[i] != null && Vector2.Distance(graph.Nodes[i].Position, position) <= 0.01f)
                    return true;
            return false;
        }

        private static GameObject FindNamedObject(string name)
        {
            var objects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < objects.Length; i++)
                if (objects[i] != null && objects[i].name == name) return objects[i].gameObject;
            return null;
        }

        private static string SnapshotPersistentFiles()
        {
            var root = Application.persistentDataPath;
            if (!Directory.Exists(root)) return "<missing>";
            using var sha = SHA256.Create();
            var builder = new StringBuilder();
            foreach (var path in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                var bytes = File.ReadAllBytes(path);
                builder.Append(path).Append(':').Append(System.Convert.ToBase64String(sha.ComputeHash(bytes))).Append(';');
            }
            return builder.ToString();
        }
    }
}
