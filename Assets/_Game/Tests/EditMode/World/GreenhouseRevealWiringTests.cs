using CindarsHope.Editor.SceneCreation;
using CindarsHope.Farm.Scene;
using CindarsHope.Farm.Watering;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public class GreenhouseRevealWiringTests
    {
        [Test] public void ExistingSceneHost_WiresRevealWithoutSavingOrRegeneratingScene()
        {
            var greenhouse = new GameObject("Greenhouse");
            var exterior = new GameObject("GreenhouseFloor");
            exterior.transform.SetParent(greenhouse.transform, false);
            var renderer = exterior.AddComponent<SpriteRenderer>();
            var hostObject = new GameObject("GreenhouseRuntimeHost");
            hostObject.transform.SetParent(greenhouse.transform, false);
            var host = hostObject.AddComponent<GreenhouseRuntimeHost>();
            var awake = typeof(GreenhouseRuntimeHost).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var destroy = typeof(GreenhouseRuntimeHost).GetMethod("OnDestroy",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            try
            {
                awake.Invoke(host, null);
                var reveal = greenhouse.GetComponentInChildren<RoofRevealController>();
                Assert.That(reveal, Is.Not.Null);
                Assert.That(reveal.GetComponent<BoxCollider2D>().isTrigger, Is.True);
                Assert.That(renderer.color.a, Is.EqualTo(1f));
                Assert.That(GreenhouseRevealRuntime.Configure(greenhouse.transform, renderer), Is.SameAs(reveal));
                Assert.That(greenhouse.GetComponentsInChildren<RoofRevealController>().Length, Is.EqualTo(1));
            }
            finally
            {
                destroy.Invoke(host, null);
                Object.DestroyImmediate(greenhouse);
            }
        }

        private static void InvokeTrigger(RoofRevealController reveal, string methodName, Collider2D collider)
        {
            // Match RoofRevealControllerTests: EditMode does not dispatch MonoBehaviour messages.
            var handler = typeof(RoofRevealController).GetMethod(methodName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(handler, Is.Not.Null);
            handler.Invoke(reveal, new object[] { collider });
        }
        [Test] public void Configure_Idempotent_UsesFeetAndRestoresExterior()
        {
            var root = new GameObject("Greenhouse");
            var actor = new GameObject("Player") { tag = "Player" };
            try
            {
                root.transform.position = new Vector3(23.2f, 12.8f);
                var exterior = root.AddComponent<SpriteRenderer>();
                var reveal = GreenhouseRevealWiring.Configure(root.transform, exterior);
                Assert.That(GreenhouseRevealWiring.Configure(root.transform, exterior), Is.SameAs(reveal));
                Assert.That(root.GetComponentsInChildren<RoofRevealController>().Length, Is.EqualTo(1));
                var body = actor.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
                var solid = actor.AddComponent<BoxCollider2D>();
                var bounds = FarmSettlementPhysicsContract.GreenhouseInteriorBounds;
                body.position = bounds.center;
                InvokeTrigger(reveal, "OnTriggerEnter2D", solid);
                Assert.That(exterior.color.a, Is.Zero);
                body.position = bounds.center + Vector2.down * bounds.height;
                InvokeTrigger(reveal, "OnTriggerExit2D", solid);
                Assert.That(exterior.color.a, Is.EqualTo(1f));
                solid.isTrigger = true;
                body.position = bounds.center;
                InvokeTrigger(reveal, "OnTriggerEnter2D", solid);
                Assert.That(exterior.color.a, Is.EqualTo(1f), "Interaction sensor alone must not reveal.");
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(actor); }
        }
    }
}
