using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public class FishingStanceZoneTests
    {
        private GameObject root, actor;
        private FishingSpot spot;
        private BoxCollider2D zone;
        [SetUp] public void SetUp()
        {
            root = new GameObject("FishingTest");
            root.transform.position = new Vector3(8000, 8000);
            root.transform.localScale = Vector3.one * 6f;
            spot = root.AddComponent<FishingSpot>();
            spot.RebindInventoryManager(root.AddComponent<InventoryManager>());
            actor = new GameObject("FootActor");
            var child = new GameObject("Stance");
            child.transform.SetParent(root.transform, false);
            child.transform.localPosition = Vector3.down;
            zone = child.AddComponent<BoxCollider2D>();
            zone.isTrigger = true;
            zone.size = new Vector2(0.2f, 0.1f);
            Physics2D.SyncTransforms();
        }
        [TearDown] public void TearDown()
        {
            Object.DestroyImmediate(actor);
            Object.DestroyImmediate(root);
        }
        private void Foot(Vector3 position) { actor.transform.position = position; Physics2D.SyncTransforms(); }
        private static void Set(FishingSpot value, string name, object data) =>
            typeof(FishingSpot).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(value, data);
        [Test] public void LegacyUnset_KeepsRingAndRequireEdgeBehavior()
        {
            Foot(root.transform.TransformPoint(new Vector3(0.4f, 0)));
            Assert.That(spot.CanInteract(actor), Is.True);
            Foot(root.transform.position);
            Assert.That(spot.CanInteract(actor), Is.False);
            Set(spot, "_requireEdgeInteraction", false);
            Assert.That(spot.CanInteract(actor), Is.True);
        }
        [Test] public void AuthoredZone_UsesWorldFeetUnderScaledParent()
        {
            spot.ConfigureStanceZone(zone);
            Foot(zone.transform.position);
            Assert.That(spot.CanInteract(actor), Is.True);
            Foot(root.transform.position);
            Assert.That(spot.CanInteract(actor), Is.False);
            Foot(zone.transform.position + Vector3.right * 0.7f);
            Assert.That(spot.CanInteract(actor), Is.False);
        }
        [Test] public void BodyOverlapWithoutFeetInside_DoesNotEnableFishing()
        {
            spot.ConfigureStanceZone(zone);
            var body = actor.AddComponent<BoxCollider2D>();
            body.size = Vector2.one * 2f;
            Foot(zone.transform.position + Vector3.right * 0.7f);
            Assert.That(body.bounds.Intersects(zone.bounds), Is.True);
            Assert.That(spot.CanInteract(actor), Is.False);
        }
        [Test] public void DisabledOrDestroyedAuthoredZone_FailsClosed()
        {
            spot.ConfigureStanceZone(zone); Foot(zone.transform.position);
            zone.enabled = false;
            Assert.That(spot.CanInteract(actor), Is.False);
            zone.enabled = true; zone.gameObject.SetActive(false);
            Assert.That(spot.CanInteract(actor), Is.False);
            Object.DestroyImmediate(zone.gameObject);
            Assert.That(spot.CanInteract(actor), Is.False);
        }
        [Test] public void InvalidZone_IsRejectedAndNullRestoresLegacy()
        {
            zone.isTrigger = false;
            Assert.Throws<System.ArgumentException>(() => spot.ConfigureStanceZone(zone));
            zone.isTrigger = true;
            spot.ConfigureStanceZone(zone); spot.ConfigureStanceZone(null);
            Foot(root.transform.TransformPoint(new Vector3(0.4f, 0)));
            Assert.That(spot.CanInteract(actor), Is.True);
        }
        [Test] public void DirectCastOutsideZone_ReturnsBeforeToolFeedbackOrFishingState()
        {
            spot.ConfigureStanceZone(zone); Foot(root.transform.position);
            int feedback = 0;
            System.Action<PlayerActionFeedbackEvent> onFeedback = _ => feedback++;
            GameEventBus.Subscribe(onFeedback);
            try
            {
                spot.Interact(actor); // A missing-rod path publishes feedback if the guard is absent.
                Assert.That(feedback, Is.Zero, "Outside stance must return before tool feedback.");
                Assert.That((bool)typeof(FishingSpot).GetField("_isFishing",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(spot), Is.False);
            }
            finally { GameEventBus.Unsubscribe(onFeedback); }
        }
    }
}
