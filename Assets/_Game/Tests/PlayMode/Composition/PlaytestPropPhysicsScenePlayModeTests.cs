using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CindarsHope.Farm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class PlaytestPropPhysicsScenePlayModeTests
    {
        [UnityTest]
        public IEnumerator FarmDock_RealPlayerBodyFitsDeckAndAdjacentWaterBlocks()
        {
            ExpectBackupSceneBootstrapTimeout();
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;
            var scene = SceneManager.GetActiveScene();
            var player = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<CindarsHope.Player.PlayerController>(true))
                .Single();
            var body = player.GetComponentsInChildren<Collider2D>(true).Single(c => c.enabled && !c.isTrigger);
            var rigidbody = player.GetComponent<Rigidbody2D>();
            var original = player.transform.position;
            var originalVelocity = rigidbody != null ? rigidbody.linearVelocity : Vector2.zero;
            try
            {
                var deck = new Vector2(FarmLevel1LayoutContract.FishingSpotX, FarmLevel1LayoutContract.FishingSpotY);
                MovePlayer(player, rigidbody, deck);
                Assert.That(HasStaticOverlap(body, player.transform), Is.False, "Fishing deck must remain usable");
                var adjacentWater = new[]
                {
                    deck + new Vector2(-1.8f, 0f),
                    deck + new Vector2(1.8f, -.7f),
                    deck + new Vector2(0f, -2.4f)
                };
                foreach (var sample in adjacentWater)
                {
                    MovePlayer(player, rigidbody, sample);
                    Assert.That(HasStaticOverlap(body, player.transform), Is.True,
                        "Player body must be blocked by water beside dock at " + sample);
                }
            }
            finally
            {
                MovePlayer(player, rigidbody, original);
                if (rigidbody != null) rigidbody.linearVelocity = originalVelocity;
            }
        }

        [UnityTest]
        public IEnumerator FarmSavedScene_HasEffectivePropBasesWithoutLowVegetationColliders()
        {
            ExpectBackupSceneBootstrapTimeout();
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;
            var scene = SceneManager.GetActiveScene();
            Assert.That(scene.name, Is.EqualTo("FarmScene"));
            var decoration = Root(scene, "FarmDecoration");
            var details = decoration.Find("FarmSettlementDetails");
            Assert.That(details, Is.Not.Null);
            AssertEffectiveSupport(details.Find("Visual_SouthYardWestPost"));
            AssertEffectiveSupport(details.Find("Visual_SouthYardEastPost"));
            AssertEffectiveSupport(Root(scene, "FarmHouse").Find("Visual_HomesteadHay"));
            var richness = Root(scene, "FarmRichness");
            AssertEffectiveSupport(richness.Find("Visual_HouseCrates"));
            AssertEffectiveSupport(richness.Find("Visual_ShedCrate"));
            // The planner omitted the proposed SouthCourtBush instances in the saved scene.
            // These are the actual authored low shrubs present in FarmScene.unity.
            var farmBushes = decoration.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name.StartsWith("Visual_MeadowLowBush_") ||
                    t.name.StartsWith("Visual_PathGardenBush_")).ToArray();
            Assert.That(farmBushes.Length, Is.EqualTo(32));
            Assert.That(farmBushes.All(t => t.GetComponentsInChildren<Collider2D>(true).Length == 0), Is.True,
                "South courtyard bushes must stay physically walkable");
            Assert.That(Root(scene, "Trees").Find("Visual_WestForestFoliage")
                .GetComponentsInChildren<Collider2D>(true).Length, Is.Zero,
                "Undergrowth is not a trunk collider");
        }

        [UnityTest]
        public IEnumerator TownSavedScene_HasWellFenceSupportsAndClearBridgeCorridors()
        {
            ExpectBackupSceneBootstrapTimeout();
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var scene = SceneManager.GetActiveScene();
            Assert.That(scene.name, Is.EqualTo("TownScene"));
            AssertEffectiveSupport(Root(scene, "TownDecorations").Find("TownWell"));
            var props = Root(scene, "TownProps");
            var fences = props.GetComponentsInChildren<SpriteRenderer>(true)
                .Where(r => r.enabled && r.gameObject.activeInHierarchy && r.name.StartsWith("GardenFence_"))
                .ToArray();
            Assert.That(fences.Length, Is.GreaterThan(0));
            foreach (var fence in fences) AssertEffectiveSupport(fence.transform);
            var townPlants = props.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name.Contains("Bush") || t.name.Contains("Flower")).ToArray();
            Assert.That(townPlants.Length, Is.GreaterThan(0));
            Assert.That(townPlants.All(t => t.GetComponentsInChildren<Collider2D>(true).Length == 0), Is.True,
                "Town bushes and flowers must stay physically walkable");

            var bridgeArts = Root(scene, "TownDecorations").GetComponentsInChildren<SpriteRenderer>(true)
                .Where(r => r.name.StartsWith("RiverBridgeArt_"))
                .ToArray();
            Assert.That(bridgeArts.Length, Is.EqualTo(2));
            var player = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<CindarsHope.Player.PlayerController>(true))
                .Single();
            var body = player.GetComponentsInChildren<Collider2D>(true).Single(c => c.enabled && !c.isTrigger);
            var original = player.transform.position;
            var rigidbody = player.GetComponent<Rigidbody2D>();
            var originalVelocity = rigidbody != null ? rigidbody.linearVelocity : Vector2.zero;
            try
            {
                foreach (var bridge in bridgeArts)
                {
                    var name = bridge.name.Substring("RiverBridgeArt_".Length);
                    if (!float.TryParse(name, NumberStyles.Float, CultureInfo.CurrentCulture, out var centerY) &&
                        !float.TryParse(name.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out centerY))
                        Assert.Fail("Bridge center is not encoded in the authored object name: " + bridge.name);
                    var centerX = bridge.bounds.center.x;
                    var from = new Vector2(centerX - bridge.bounds.extents.x - .35f, centerY);
                    var to = new Vector2(centerX + bridge.bounds.extents.x + .35f, centerY);
                    MovePlayer(player, rigidbody, from);
                    Assert.That(HasStaticOverlap(body, player.transform), Is.False, bridge.name + " west bank overlaps solid");
                    var filter = new ContactFilter2D { useTriggers = false };
                    filter.SetLayerMask(Physics2D.GetLayerCollisionMask(body.gameObject.layer));
                    var hits = new RaycastHit2D[128];
                    var count = body.Cast(Vector2.right, filter, hits, to.x - from.x);
                    Assert.That(count, Is.LessThan(hits.Length), bridge.name + " cast buffer saturated");
                    for (var i = 0; i < count; i++)
                    {
                        var hit = hits[i].collider;
                        if (hit == null || hit.transform.IsChildOf(player.transform)) continue;
                        if (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType != RigidbodyType2D.Static) continue;
                        Assert.Fail(bridge.name + " corridor blocked by " + hit.name);
                    }
                    MovePlayer(player, rigidbody, to);
                    Assert.That(HasStaticOverlap(body, player.transform), Is.False, bridge.name + " east bank overlaps solid");
                    count = body.Cast(Vector2.left, filter, hits, to.x - from.x);
                    Assert.That(count, Is.LessThan(hits.Length), bridge.name + " reverse cast buffer saturated");
                    for (var i = 0; i < count; i++)
                    {
                        var hit = hits[i].collider;
                        if (hit == null || hit.transform.IsChildOf(player.transform)) continue;
                        if (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType != RigidbodyType2D.Static) continue;
                        Assert.Fail(bridge.name + " reverse corridor blocked by " + hit.name);
                    }
                }
            }
            finally
            {
                MovePlayer(player, rigidbody, original);
                if (rigidbody != null) rigidbody.linearVelocity = originalVelocity;
            }
        }

        private static Transform Root(Scene scene, string name)
        {
            var matches = scene.GetRootGameObjects().Where(r => r.name == name).ToArray();
            Assert.That(matches.Length, Is.EqualTo(1), name + " root count");
            return matches[0].transform;
        }

        private static void ExpectBackupSceneBootstrapTimeout()
        {
            // The Test Runner's temporary scene can leave its death bootstrap waiting for
            // a GameBootstrap that only exists after loading Farm/Town. Match only that log.
            var scene = SceneManager.GetActiveScene();
            if (!scene.name.StartsWith("InitTestScene", System.StringComparison.Ordinal) &&
                !scene.path.Contains("__Backupscenes")) return;
            LogAssert.Expect(LogType.Error, new Regex(
                @"^\[DeathSystemBootstrap\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos apos 120 frames\. Sistema de morte NAO inicializado \(morte->corpo->respawn indisponivel\)\.$"));
        }

        private static void AssertEffectiveSupport(Transform visual)
        {
            Assert.That(visual, Is.Not.Null);
            var renderer = visual.GetComponent<SpriteRenderer>();
            Assert.That(renderer, Is.Not.Null);
            var bounds = renderer.bounds;
            var supportPoints = new[]
            {
                new Vector2(Mathf.Lerp(bounds.min.x, bounds.max.x, .2f), bounds.min.y + bounds.size.y * .1f),
                new Vector2(bounds.center.x, bounds.min.y + bounds.size.y * .1f),
                new Vector2(Mathf.Lerp(bounds.min.x, bounds.max.x, .8f), bounds.min.y + bounds.size.y * .1f)
            };
            var local = visual.Find("SolidFloorContact");
            if (local != null)
            {
                var collider = local.GetComponent<Collider2D>();
                Assert.That(collider != null && collider.enabled && !collider.isTrigger, Is.True, visual.name);
                foreach (var point in supportPoints)
                    Assert.That(collider.OverlapPoint(point), Is.True, visual.name + " base misses " + point);
                return;
            }
            Collider2D shared = null;
            foreach (var point in supportPoints)
            {
                var matches = Physics2D.OverlapPointAll(point).Where(c => c.enabled && !c.isTrigger &&
                    (c.attachedRigidbody == null || c.attachedRigidbody.bodyType == RigidbodyType2D.Static)).ToArray();
                if (shared == null) shared = matches.FirstOrDefault();
                Assert.That(shared != null && matches.Contains(shared), Is.True,
                    visual.name + " has no common static support across base at " + point);
            }
        }

        private static void MovePlayer(CindarsHope.Player.PlayerController player, Rigidbody2D body, Vector2 point)
        {
            player.transform.position = new Vector3(point.x, point.y, player.transform.position.z);
            if (body != null) { body.position = point; body.linearVelocity = Vector2.zero; }
            Physics2D.SyncTransforms();
        }

        private static bool HasStaticOverlap(Collider2D body, Transform player)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(body.gameObject.layer));
            var overlaps = new Collider2D[128];
            var count = body.Overlap(filter, overlaps);
            if (count >= overlaps.Length) Assert.Fail("Player overlap buffer saturated");
            for (var i = 0; i < count; i++)
            {
                var hit = overlaps[i];
                if (hit == null || hit.transform.IsChildOf(player)) continue;
                if (hit.attachedRigidbody == null || hit.attachedRigidbody.bodyType == RigidbodyType2D.Static) return true;
            }
            return false;
        }
    }
}
