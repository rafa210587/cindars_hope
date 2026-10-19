using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public class RoofRevealControllerTests
    {
        [Test]
        public void PlayerColliders_RevealInteriorUntilLastExit_WithoutChangingInteriorColor()
        {
            var house = new GameObject("House");
            var roofObject = new GameObject("Roof");
            var interiorObject = new GameObject("Interior");
            var player = new GameObject("Player") { tag = "Player" };
            try
            {
                var roof = roofObject.AddComponent<SpriteRenderer>();
                var interior = interiorObject.AddComponent<SpriteRenderer>();
                var interiorColor = new Color(0.2f, 0.4f, 0.6f, 0.8f);
                interior.color = interiorColor;
                var first = player.AddComponent<BoxCollider2D>();
                var second = player.AddComponent<CircleCollider2D>();
                var reveal = house.AddComponent<RoofRevealController>();
                reveal.Configure(new[] { roof }, interiorRenderers: new[] { interior });
                Assert.That(interior.enabled, Is.False);
                Assert.That(roof.color.a, Is.EqualTo(1f));
                InvokeTrigger(reveal, "OnTriggerEnter2D", first);
                InvokeTrigger(reveal, "OnTriggerEnter2D", second);
                Assert.That(interior.enabled, Is.True);
                Assert.That(roof.color.a, Is.Zero);
                InvokeTrigger(reveal, "OnTriggerExit2D", first);
                Assert.That(interior.enabled, Is.True);
                InvokeTrigger(reveal, "OnTriggerExit2D", second);
                Assert.That(interior.enabled, Is.False);
                Assert.That(roof.color.a, Is.EqualTo(1f));
                Assert.That(interior.color, Is.EqualTo(interiorColor));
            }
            finally
            {
                Object.DestroyImmediate(house);
                Object.DestroyImmediate(roofObject);
                Object.DestroyImmediate(interiorObject);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void LegacyConfiguration_StillControlsOnlyTheRoof()
        {
            var house = new GameObject("House");
            try
            {
                var roof = house.AddComponent<SpriteRenderer>();
                var reveal = house.AddComponent<RoofRevealController>();
                reveal.Configure(new[] { roof }, 0.75f, 0.2f);
                Assert.That(roof.color.a, Is.EqualTo(0.75f));
            }
            finally { Object.DestroyImmediate(house); }
        }
        [Test]
        public void FeetMode_IgnoresSensorAndBodyOverlapUntilFeetCrossUsefulInterior()
        {
            var house = new GameObject("House");
            var player = new GameObject("Player") { tag = "Player" };
            var sensorObject = new GameObject("InteractionSensor");
            sensorObject.transform.SetParent(player.transform, false);
            try
            {
                var roof = house.AddComponent<SpriteRenderer>();
                var box = house.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.offset = new Vector2(0f, 2f);
                box.size = new Vector2(4f, 4f); // useful interior y[0,4]
                var body = player.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                var solid = player.AddComponent<BoxCollider2D>();
                solid.offset = Vector2.up * 0.6f;
                solid.size = new Vector2(0.5f, 1.2f);
                var sensor = sensorObject.AddComponent<CircleCollider2D>();
                sensor.isTrigger = true;
                sensor.radius = 1f;
                var reveal = house.AddComponent<RoofRevealController>();
                reveal.Configure(new[] { roof });
                reveal.ConfigureFeetOccupancy(box);
                body.position = new Vector2(0f, -0.4f);
                InvokeTrigger(reveal, "OnTriggerEnter2D", sensor);
                InvokeTrigger(reveal, "OnTriggerEnter2D", solid);
                Assert.That(roof.color.a, Is.EqualTo(1f), "Body top/sensor overlap must not reveal from outside.");
                body.position = new Vector2(0f, 0.1f);
                InvokeTrigger(reveal, "OnTriggerStay2D", solid);
                Assert.That(roof.color.a, Is.Zero, "Feet entry must reveal without another Enter callback.");
                body.position = new Vector2(0f, -0.1f);
                InvokeTrigger(reveal, "OnTriggerStay2D", solid);
                Assert.That(roof.color.a, Is.EqualTo(1f), "Feet exit must restore while body still overlaps.");
            }
            finally { Object.DestroyImmediate(house); Object.DestroyImmediate(player); }
        }

        [Test]
        public void FeetMode_DisabledPlayerAndComponentRestoreExteriorWithoutChangingLeafEnabled()
        {
            var house = new GameObject("House");
            var player = new GameObject("Player") { tag = "Player" };
            try
            {
                var roof = house.AddComponent<SpriteRenderer>();
                roof.enabled = false; // Door owns this flag; reveal owns alpha only.
                var box = house.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = Vector2.one * 4f;
                var solid = player.AddComponent<BoxCollider2D>();
                var reveal = house.AddComponent<RoofRevealController>();
                reveal.Configure(new[] { roof });
                reveal.ConfigureFeetOccupancy(box);
                InvokeTrigger(reveal, "OnTriggerEnter2D", solid);
                Assert.That(roof.color.a, Is.Zero);
                player.SetActive(false);
                typeof(RoofRevealController).GetMethod("LateUpdate",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(reveal, null);
                Assert.That(roof.color.a, Is.EqualTo(1f));
                player.SetActive(true);
                InvokeTrigger(reveal, "OnTriggerEnter2D", solid);
                reveal.enabled = false;
                // This plain MonoBehaviour does not receive lifecycle messages automatically in EditMode.
                typeof(RoofRevealController).GetMethod("OnDisable",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(reveal, null);
                Assert.That(roof.color.a, Is.EqualTo(1f));
                Assert.That(roof.enabled, Is.False);
            }
            finally { Object.DestroyImmediate(house); Object.DestroyImmediate(player); }
        }

        private static void InvokeTrigger(RoofRevealController reveal, string methodName, Collider2D collider)
        {
            // EditMode does not dispatch MonoBehaviour messages; invoke the same Unity handler directly.
            var handler = typeof(RoofRevealController).GetMethod(methodName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(handler, Is.Not.Null);
            handler.Invoke(reveal, new object[] { collider });
        }
    }
}
