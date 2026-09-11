using System.Collections.Generic;
using CindarsHope.NPC.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.NPC
{
    public sealed class NpcTownRouteGraphTests
    {
        [Test]
        public void BuildRoute_UsesStableConnectedWaypoints()
        {
            var go = new GameObject("route-test");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "a", Position = Vector3.zero },
                        new NpcTownRouteNode { Id = "b", Position = new Vector3(1f, 0f) },
                        new NpcTownRouteNode { Id = "c", Position = new Vector3(2f, 0f) }
                    },
                    new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "a", To = "b" },
                        new NpcTownRouteEdge { From = "b", To = "c" }
                    });

                var points = new List<Vector3>();
                Assert.IsTrue(graph.TryBuildRoute(new Vector3(0.05f, 0f), new Vector3(1.95f, 0f), points));
                Assert.That(points.Count, Is.EqualTo(5));
                Assert.That(points[1], Is.EqualTo(Vector3.zero));
                Assert.That(points[2], Is.EqualTo(new Vector3(1f, 0f)));
                Assert.That(points[3], Is.EqualTo(new Vector3(2f, 0f)));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BuildRoute_RejectsDisconnectedNodesWithoutTeleportFallback()
        {
            var go = new GameObject("route-test-disconnected");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "a", Position = Vector3.zero },
                        new NpcTownRouteNode { Id = "b", Position = new Vector3(10f, 0f) }
                    }, new List<NpcTownRouteEdge>());
                var points = new List<Vector3>();
                Assert.IsFalse(graph.TryBuildRoute(Vector3.zero, new Vector3(10f, 0f), points));
                Assert.IsEmpty(points);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SerializedGraph_ReportsEveryNodeReachable()
        {
            var go = new GameObject("route-test-connected");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "a", Position = Vector3.zero },
                        new NpcTownRouteNode { Id = "b", Position = Vector3.right },
                        new NpcTownRouteNode { Id = "c", Position = Vector3.right * 2f }
                    },
                    new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "a", To = "b" },
                        new NpcTownRouteEdge { From = "b", To = "c" }
                    });
                Assert.IsTrue(graph.ValidateConnectivity(out var reachable));
                Assert.AreEqual(3, reachable);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void BuildRoute_IsDeterministicForSameInput()
        {
            var go = new GameObject("route-test-determinism");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "a", Position = Vector3.zero },
                        new NpcTownRouteNode { Id = "b", Position = Vector3.up },
                        new NpcTownRouteNode { Id = "c", Position = Vector3.right },
                        new NpcTownRouteNode { Id = "d", Position = Vector3.one }
                    },
                    new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "a", To = "b" },
                        new NpcTownRouteEdge { From = "a", To = "c" },
                        new NpcTownRouteEdge { From = "b", To = "d" },
                        new NpcTownRouteEdge { From = "c", To = "d" }
                    });
                var first = new List<Vector3>();
                var second = new List<Vector3>();
                Assert.IsTrue(graph.TryBuildRoute(Vector3.zero, Vector3.one, first));
                Assert.IsTrue(graph.TryBuildRoute(Vector3.zero, Vector3.one, second));
                CollectionAssert.AreEqual(first, second);
                Assert.AreEqual(Vector3.up, first[2]);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void BuildRoute_PrefersLowerTotalCostOverFewerHops()
        {
            var go = new GameObject("route-test-weighted");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "a", Position = Vector3.zero },
                        new NpcTownRouteNode { Id = "expensive", Position = Vector3.up },
                        new NpcTownRouteNode { Id = "cheap_1", Position = Vector3.right },
                        new NpcTownRouteNode { Id = "cheap_2", Position = Vector3.right * 2f },
                        new NpcTownRouteNode { Id = "goal", Position = Vector3.right * 3f }
                    },
                    new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "a", To = "expensive", Cost = 10f },
                        new NpcTownRouteEdge { From = "expensive", To = "goal", Cost = 10f },
                        new NpcTownRouteEdge { From = "a", To = "cheap_1", Cost = 1f },
                        new NpcTownRouteEdge { From = "cheap_1", To = "cheap_2", Cost = 1f },
                        new NpcTownRouteEdge { From = "cheap_2", To = "goal", Cost = 1f }
                    });
                var points = new List<Vector3>();
                Assert.IsTrue(graph.TryBuildRoute(Vector3.zero, Vector3.right * 3f, points));
                Assert.AreEqual(Vector3.right, points[2]);
                Assert.AreEqual(Vector3.right * 2f, points[3]);
                CollectionAssert.DoesNotContain(points, Vector3.up);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void BuildRoute_RejectsEuclideanNearestStartBehindSolidWall()
        {
            var go = new GameObject("route-test-visible-start");
            var wall = new GameObject("route-test-wall");
            var actor = new GameObject("route-test-actor");
            try
            {
                wall.transform.position = new Vector3(0.5f, 0f);
                var wallCollider = wall.AddComponent<BoxCollider2D>();
                wallCollider.size = new Vector2(0.3f, 0.7f);
                var self = actor.AddComponent<CircleCollider2D>();
                self.radius = 0.2f;
                Physics2D.SyncTransforms();
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "behind", Position = Vector3.right },
                        new NpcTownRouteNode { Id = "visible", Position = Vector3.up },
                        new NpcTownRouteNode { Id = "goal", Position = Vector3.right * 5f }
                    },
                    new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "behind", To = "goal", Cost = 4f },
                        new NpcTownRouteEdge { From = "visible", To = "goal", Cost = 6f }
                    });
                var points = new List<Vector3>();
                Assert.IsTrue(graph.TryBuildRoute(Vector3.zero, Vector3.right * 5f, points, self));
                Assert.AreEqual(Vector3.up, points[1]);
            }
            finally
            {
                Object.DestroyImmediate(actor);
                Object.DestroyImmediate(wall);
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BuildRoute_AllowsNonSafeIndoorNodeAsOriginThroughApproach()
        {
            var go = new GameObject("route-test-indoor-origin");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "home", Position = Vector3.zero, Safe = false },
                        new NpcTownRouteNode { Id = "approach", Position = Vector3.right, Safe = true },
                        new NpcTownRouteNode { Id = "work", Position = Vector3.right * 2f, Safe = true }
                    },
                    new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "home", To = "approach", Cost = 1f },
                        new NpcTownRouteEdge { From = "approach", To = "work", Cost = 1f }
                    });
                var points = new List<Vector3>();
                Assert.IsTrue(graph.TryBuildRoute(Vector3.zero, Vector3.right * 2f, points));
                Assert.AreEqual(Vector3.zero, points[1]);
                Assert.AreEqual(Vector3.right, points[2]);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void RecoverySnap_NeverSelectsNonSafeIndoorNode()
        {
            var go = new GameObject("route-test-safe-snap");
            try
            {
                var graph = go.AddComponent<NpcTownRouteGraph>();
                graph.Configure(
                    new List<NpcTownRouteNode>
                    {
                        new NpcTownRouteNode { Id = "home", Position = Vector3.zero, Safe = false },
                        new NpcTownRouteNode { Id = "approach", Position = Vector3.right * 5f, Safe = true }
                    }, new List<NpcTownRouteEdge>
                    {
                        new NpcTownRouteEdge { From = "home", To = "approach", Cost = 5f }
                    });
                Assert.IsTrue(graph.TryFindNearestSafeUnoccupied(Vector3.zero, 0.2f, null, out var selected));
                Assert.AreEqual(Vector3.right * 5f, selected);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void RecoveryPolicy_UsesRealProgressWindowAndCapsReplans()
        {
            Assert.IsTrue(NpcRouteRecoveryPolicy.HasInsufficientProgress(1f, 0.95f, 2f));
            Assert.IsFalse(NpcRouteRecoveryPolicy.HasInsufficientProgress(1f, 0.95f, 1.99f));
            Assert.IsTrue(NpcRouteRecoveryPolicy.CanReplan(0));
            Assert.IsTrue(NpcRouteRecoveryPolicy.CanReplan(1));
            Assert.IsFalse(NpcRouteRecoveryPolicy.CanReplan(2));
        }

        [Test]
        public void RecoveryPolicy_ProgressCountsOnlyDistanceClosedTowardTarget()
        {
            var start = Vector2.zero;
            var target = new Vector2(10f, 0f);
            Assert.That(NpcRouteRecoveryPolicy.ProgressTowardTarget(start,
                new Vector2(0f, 3f), target), Is.LessThanOrEqualTo(0f),
                "lateral/away movement must not reset a blocked route window");
            Assert.That(NpcRouteRecoveryPolicy.ProgressTowardTarget(start,
                new Vector2(-1f, 0f), target), Is.LessThan(0f));
            Assert.That(NpcRouteRecoveryPolicy.ProgressTowardTarget(start,
                new Vector2(2f, 0f), target), Is.EqualTo(2f).Within(0.001f),
                "movement toward the requested target must count as progress");
        }

        [Test]
        public void RecoveryPolicy_SeparatesOffscreenSnapAndRejectsOccupiedSafePoint()
        {
            Assert.IsFalse(NpcRouteRecoveryPolicy.CanSnap(false));
            Assert.IsTrue(NpcRouteRecoveryPolicy.CanSnap(true));
            var blocker = new GameObject("safe-point-blocker");
            try
            {
                blocker.AddComponent<CircleCollider2D>().radius = 0.3f;
                Assert.IsFalse(NpcRouteRecoveryPolicy.IsSafePoint(Vector2.zero, 0.3f));
                Assert.IsTrue(NpcRouteRecoveryPolicy.IsSafePoint(Vector2.one * 5f, 0.3f));
            }
            finally { Object.DestroyImmediate(blocker); }
        }
    }
}
