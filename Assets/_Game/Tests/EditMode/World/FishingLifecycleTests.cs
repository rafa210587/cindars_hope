using System.Collections;
using System.Reflection;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public class FishingLifecycleTests
    {
        private GameObject root;
        private FishingSpot spot;
        [SetUp] public void SetUp() { root = new GameObject("FishingLifecycle"); spot = root.AddComponent<FishingSpot>(); }
        [TearDown] public void TearDown() => Object.DestroyImmediate(root);
        private void Set(string key, object value) => typeof(FishingSpot).GetField(key, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(spot, value);
        private bool Read(string key) => (bool)typeof(FishingSpot).GetField(key, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spot);
        private void Invoke(string key) => typeof(FishingSpot).GetMethod(key, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(spot, null);
        [Test] public void EarlyConfirm_DoesNotAwardOrEndCast()
        {
            Set("_isFishing", true); Set("_windowReady", false);
            Set("_windowOpenTime", Time.time);
            Assert.DoesNotThrow(() => Invoke("TryConfirmFishing")); // No inventory: an erroneous reward path would throw.
            Assert.That(Read("_isFishing"), Is.True);
            Assert.That(spot.InteractionPrompt, Is.EqualTo("Aguarde a mordida"));
        }
        [Test] public void Disable_ClearsCastState()
        {
            Set("_isFishing", true); Set("_windowReady", true);
            // EditMode does not dispatch lifecycle callbacks; exercise the actual handler directly.
            Invoke("OnDisable");
            Assert.That(Read("_isFishing"), Is.False);
            Assert.That(Read("_windowReady"), Is.False);

            Assert.That(spot.InteractionPrompt, Is.EqualTo("Pescar"));
        }
        [Test] public void LeavingStance_CancelsCast()
        {
            var actor = new GameObject("Actor");
            try
            {
                var zone = root.AddComponent<BoxCollider2D>(); zone.isTrigger = true;
                spot.ConfigureStanceZone(zone);
                actor.transform.position = new Vector3(100, 100);
                Set("_isFishing", true); Set("_windowReady", true); Set("_castingActor", actor);
                Invoke("Update");
                Assert.That(Read("_isFishing"), Is.False);
                Assert.That(Read("_windowReady"), Is.False);
            }
            finally { Object.DestroyImmediate(actor); }
        }
        [Test] public void Timeout_ClearsBiteWindow()
        {
            var routine = (IEnumerator)typeof(FishingSpot).GetMethod("FishingRoutine", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(spot, new object[] { 0 });
            Assert.That(routine.MoveNext(), Is.True); // Cast delay.
            Assert.That(routine.MoveNext(), Is.True); // Bite window opens.
            Assert.That(Read("_windowReady"), Is.True);
            Set("_windowOpenTime", Time.time - 100f);
            Assert.That(routine.MoveNext(), Is.False);
            Assert.That(Read("_isFishing"), Is.False);
            Assert.That(Read("_windowReady"), Is.False);
        }
    }
}
