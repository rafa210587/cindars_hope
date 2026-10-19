using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CindarsHope.City.Services;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.NPC;
using CindarsHope.NPC.Runtime;
using CindarsHope.NPC.Schedule;
using CindarsHope.Player;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    /// <summary>C10: real Town door policies, clock transitions and physical safe exit.</summary>
    public sealed class TownShopDoorHoursPlayModeTests
    {
        private static readonly FieldInfo DoorField = typeof(TownShopDoorHoursPolicy)
            .GetField("_door", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo InteriorField = typeof(TownShopDoorHoursPolicy)
            .GetField("_interiorOccupancy", BindingFlags.Instance | BindingFlags.NonPublic);

        [UnityTest]
        public IEnumerator TownScene_ShopHours_BlockEntryAllowServiceExitAndPreserveNpcSensorAutoDoor()
        {
            // TownScene has baseline bootstrap diagnostics unrelated to C10. This scenario asserts
            // its own feedback/physics contracts and restores the runner-wide setting in finally.
            var previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            PlayerController player = null;
            Rigidbody2D playerBody = null;
            NpcWanderer wanderer = null;
            GameObject npc = null;
            GameTimeManager clock = null;
            var playerStart = default(Vector3);
            var npcStart = default(Vector3);
            var playerControllerEnabled = false;
            var wandererEnabled = false;
            var originalPhase = 0;
            var originalPhaseTimer = 0f;
            System.IDisposable feedbackSubscription = null;
            try
            {
                yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
                var roots = SceneManager.GetActiveScene().GetRootGameObjects();
                for (var frame = 0; frame < 180 &&
                     (GameBootstrap.Instance == null || GameBootstrap.Instance.GameTimeManager == null ||
                      FindNamedObject(roots, "Player") == null || FindNamedObject(roots, "House_Bakery") == null);
                     frame++)
                    yield return null;

                var policies = FindComponentsInTown<TownShopDoorHoursPolicy>(roots);
                Assert.AreEqual(9, policies.Count, "every shop_hours_gated Town lot needs one runtime policy");
                foreach (var listedPolicy in policies)
                {
                    var policyDoor = DoorField?.GetValue(listedPolicy) as HouseDoorInteractable;
                    var policyInterior = InteriorField?.GetValue(listedPolicy) as BoxCollider2D;
                    Assert.IsNotNull(policyDoor, $"{listedPolicy.name} has no physical door reference");
                    Assert.IsNotNull(policyInterior, $"{listedPolicy.name} has no feet/interior trigger reference");
                    Assert.AreSame(listedPolicy, policyDoor.GetComponent<TownShopDoorHoursPolicy>());
                }

                var bootstrap = GameBootstrap.Instance;
                clock = bootstrap != null ? bootstrap.GameTimeManager : null;
                Assert.IsNotNull(clock, "Town shop policy requires the existing GameTimeManager");
                var bakery = FindNamedObject(roots, "House_Bakery");
                Assert.IsNotNull(bakery);
                var door = bakery.GetComponentInChildren<HouseDoorInteractable>(true);
                var policy = bakery.GetComponentInChildren<TownShopDoorHoursPolicy>(true);
                var blocker = door != null ? door.PhysicalBlocker as BoxCollider2D : null;
                var interior = policy != null ? InteriorField?.GetValue(policy) as BoxCollider2D : null;
                var sensor = door != null ? FindDoorSensor(door) : null;
                var playerObject = FindNamedObject(roots, "Player");
                player = playerObject != null ? playerObject.GetComponent<PlayerController>() : null;
                npc = FindNamedObject(roots, "NPC_Mella_Bakery");
                Assert.IsNotNull(door); Assert.IsNotNull(policy); Assert.IsNotNull(blocker);
                Assert.IsNotNull(interior); Assert.IsNotNull(sensor); Assert.IsNotNull(player); Assert.IsNotNull(npc);

                playerBody = player.GetComponent<Rigidbody2D>();
                wanderer = npc.GetComponent<NpcWanderer>();
                Assert.IsNotNull(playerBody); Assert.IsNotNull(wanderer);
                playerStart = player.transform.position;
                playerControllerEnabled = player.enabled;
                npcStart = npc.transform.position;
                wandererEnabled = wanderer.enabled;
                originalPhase = (int)clock.CurrentPhase;
                originalPhaseTimer = clock.PhaseTimer;
                var saveBefore = SnapshotPersistentFiles();
                var feedback = new List<string>();
                feedbackSubscription = GameEventBus.Subscribe<PlayerActionFeedbackEvent>(evt => feedback.Add(evt.Message));
                player.enabled = false;
                wanderer.enabled = false;
                var blockerCenter = (Vector2)blocker.transform.TransformPoint(blocker.offset);
                var interiorCenter = (Vector2)interior.transform.TransformPoint(interior.offset);
                var inward = (interiorCenter - blockerCenter).normalized;
                var outside = blockerCenter - inward * 3f;
                var doorway = blockerCenter;

                SetDayHour(clock, 8);
                playerBody.position = outside;
                Physics2D.SyncTransforms();
                door.Interact(player.gameObject);
                yield return null;
                Assert.IsFalse(door.IsOpen, "08:00 must reject external entry to a closed Bakery");
                Assert.IsTrue(blocker.enabled);
                Assert.IsTrue(feedback.Exists(message => message.Contains("Horário: 09:00–22:00")),
                    "closed entry must publish its hours through the existing feedback bus");

                // Controlled sensor placement tests the existing NPC proximity callback; it does not
                // claim movement-input or route-walking evidence (covered by N1 navigation tests).
                npc.transform.position = outside;
                Physics2D.SyncTransforms();
                yield return new WaitForFixedUpdate();
                npc.transform.position = sensor.transform.TransformPoint(sensor.offset);
                Physics2D.SyncTransforms();
                for (var i = 0; i < 60 && !door.IsOpen; i++) yield return new WaitForFixedUpdate();
                Assert.IsTrue(door.IsOpen, "NPC doorway traversal must retain automatic opening outside shop hours");
                npc.transform.position = outside;
                Physics2D.SyncTransforms();
                for (var i = 0; i < 120 && (door.IsOpen || door.IsAnimating); i++) yield return new WaitForFixedUpdate();
                Assert.IsFalse(door.IsOpen, "automatic NPC traversal must still restore the closed blocker");

                SetDayHour(clock, 12);
                playerBody.position = outside;
                Physics2D.SyncTransforms();
                door.Interact(player.gameObject);
                yield return WaitForDoor(door, true);
                Assert.IsFalse(blocker.enabled, "12:00 must permit physical shop entry");

                // Closing time with a real body indoors permits exit; doorway occupancy prevents a crush.
                playerBody.position = interior.bounds.center;
                Physics2D.SyncTransforms();
                SetDayHour(clock, 8);
                yield return WaitForDoor(door, false);
                playerBody.position = interior.bounds.center;
                Physics2D.SyncTransforms();
                door.Interact(player.gameObject);
                yield return WaitForDoor(door, true);
                playerBody.position = doorway;
                Physics2D.SyncTransforms();
                for (var i = 0; i < 90; i++) yield return new WaitForFixedUpdate();
                Assert.IsFalse(blocker.enabled, "a body in the doorway must prevent a closed-hours door from crushing exit");
                playerBody.position = outside;
                Physics2D.SyncTransforms();
                yield return WaitForDoor(door, false);
                Assert.IsTrue(blocker.enabled, "after exit clears the doorway, the closed-hours blocker must restore");
                Assert.AreEqual(saveBefore, SnapshotPersistentFiles(), "door/hour proof must not write save state");
            }
            finally
            {
                feedbackSubscription?.Dispose();
                if (clock != null)
                {
                    clock.RestorePhaseState(originalPhase, originalPhaseTimer);
                    GameEventBus.Publish(new GameTimeTickEvent(clock.PhaseTimer, clock.CurrentDay));
                }
                if (playerBody != null) playerBody.position = playerStart;
                if (player != null) player.enabled = playerControllerEnabled;
                if (npc != null) npc.transform.position = npcStart;
                if (wanderer != null) wanderer.enabled = wandererEnabled;
                LogAssert.ignoreFailingMessages = previousIgnoreFailingMessages;
            }
        }

        private static void SetDayHour(GameTimeManager clock, int hour)
        {
            Assert.That(hour, Is.InRange(6, 17));
            clock.RestorePhaseState((int)GamePhaseChangedEvent.GamePhase.Day,
                clock.CurrentPhaseDurationSeconds * ((hour - 6 + 0.1f) / 12f));
            Assert.AreEqual(hour, clock.CurrentHourOfDay);
            GameEventBus.Publish(new GameTimeTickEvent(clock.PhaseTimer, clock.CurrentDay));
        }

        private static IEnumerator WaitForDoor(HouseDoorInteractable door, bool open)
        {
            for (var i = 0; i < 180 && (door.IsOpen != open || door.IsAnimating); i++)
                yield return new WaitForFixedUpdate();
            Assert.AreEqual(open, door.IsOpen);
            Assert.IsFalse(door.IsAnimating);
        }

        private static BoxCollider2D FindDoorSensor(HouseDoorInteractable door)
        {
            foreach (var collider in door.GetComponents<Collider2D>())
                if (collider is BoxCollider2D box && box.isTrigger) return box;
            return null;
        }

        private static List<T> FindComponentsInTown<T>(IEnumerable<GameObject> roots) where T : Component
        {
            var results = new List<T>();
            foreach (var root in roots)
                if (root != null) results.AddRange(root.GetComponentsInChildren<T>(true));
            return results;
        }

        private static GameObject FindNamedObject(IEnumerable<GameObject> roots, string name)
        {
            foreach (var root in roots)
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (transform.name == name) return transform.gameObject;
            return null;
        }

        private static string SnapshotPersistentFiles()
        {
            var root = Application.persistentDataPath;
            if (!Directory.Exists(root)) return "<missing>";
            using var sha = SHA256.Create();
            var builder = new StringBuilder();
            foreach (var path in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                builder.Append(path).Append(':').Append(System.Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)))).Append(';');
            return builder.ToString();
        }
    }
}
