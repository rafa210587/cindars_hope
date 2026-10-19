using System.Collections.Generic;
using CindarsHope.Farm.Animals;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class AnimalMotionTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = _created.Count - 1; i >= 0; i--)
                if (_created[i] != null) Object.DestroyImmediate(_created[i]);
            _created.Clear();
        }

        [Test]
        public void SameStableIds_ProduceSameLocalMotionSequence()
        {
            var settings = new AnimalMotionSettings(0.7f, 1f, 0f, 0f, 0.2f, 0.3f, 0.7f, 0.2f, 6, 0.02f);
            var first = new AnimalMotionState(settings, "animal_coop_01_0", FarmAnimalCatalog.AnimalChicken);
            var second = new AnimalMotionState(settings, "animal_coop_01_0", FarmAnimalCatalog.AnimalChicken);
            var bounds = Rect.MinMaxRect(-2f, -1f, 2f, 1f);
            Vector2 firstPosition = Vector2.zero;
            Vector2 secondPosition = Vector2.zero;

            for (int i = 0; i < 240; i++)
            {
                Vector2 firstStep = first.Advance(0.02f, firstPosition, bounds, AlwaysClear);
                Vector2 secondStep = second.Advance(0.02f, secondPosition, bounds, AlwaysClear);
                Assert.AreEqual(first.Mode, second.Mode, $"Mode diverged at step {i}.");
                Assert.AreEqual(first.FacingDirection, second.FacingDirection, $"Facing diverged at step {i}.");
                Assert.That(Vector2.Distance(firstStep, secondStep), Is.LessThan(0.000001f));
                firstPosition += firstStep;
                secondPosition += secondStep;
            }
        }

        [Test]
        public void StableSeed_UsesAnimalDataId_NotSharedSpecies()
        {
            int goat = AnimalMotionState.CreateStableSeed("animal_barn_01_0", FarmAnimalCatalog.AnimalGoat);
            int sheep = AnimalMotionState.CreateStableSeed("animal_barn_01_0", FarmAnimalCatalog.AnimalSheep);
            Assert.AreNotEqual(goat, sheep, "Goat and sheep share an enum species but need distinct visual streams.");
        }

        [Test]
        public void BodyAwareBounds_KeepWholeFootprintInsideSharedPen()
        {
            Rect pen = Rect.MinMaxRect(-2.2f, -1.8f, 2.2f, 0.1f);
            Vector2 body = new Vector2(0.55f, 0.42f);
            const float skin = 0.03f;
            Rect centers = AnimalMotionBounds.Contract(pen, body, skin);
            var state = new AnimalMotionState(
                new AnimalMotionSettings(0.9f, 1.2f, 0f, 0f, 0.2f, 0.2f, 1f, 0f, 6, 0.02f),
                "animal_coop_01_1", FarmAnimalCatalog.AnimalChicken);
            Vector2 position = centers.center;

            for (int i = 0; i < 1200; i++)
            {
                position += state.Advance(0.02f, position, centers, AlwaysClear);
                Assert.That(position.x - body.x * 0.5f, Is.GreaterThanOrEqualTo(pen.xMin + skin - 0.0001f));
                Assert.That(position.x + body.x * 0.5f, Is.LessThanOrEqualTo(pen.xMax - skin + 0.0001f));
                Assert.That(position.y - body.y * 0.5f, Is.GreaterThanOrEqualTo(pen.yMin + skin - 0.0001f));
                Assert.That(position.y + body.y * 0.5f, Is.LessThanOrEqualTo(pen.yMax - skin + 0.0001f));
            }
        }

        [Test]
        public void BlockedMotion_UsesBoundedAttempts_ThenReturnsToIdle()
        {
            const int maxAttempts = 4;
            var state = new AnimalMotionState(
                new AnimalMotionSettings(1f, 1f, 0f, 0f, 0.2f, 0.2f, 1f, 0f, maxAttempts, 0.02f),
                "animal_coop_01_2", FarmAnimalCatalog.AnimalChicken);

            Vector2 displacement = state.Advance(0.02f, Vector2.zero,
                Rect.MinMaxRect(-1f, -1f, 1f, 1f), NeverClear);

            Assert.AreEqual(Vector2.zero, displacement);
            Assert.AreEqual(AnimalMotionMode.Idle, state.Mode);
            Assert.AreEqual(maxAttempts, state.LastTargetAttempts);
            Assert.IsTrue(state.IsPaused);
        }

        [Test]
        public void InteractionFreeze_AndUnavailableHealth_StopDisplacement()
        {
            var state = new AnimalMotionState(
                new AnimalMotionSettings(1f, 1f, 0f, 0f, 0.2f, 0.2f, 1f, 0f, 4, 0.02f),
                "animal_coop_01_3", FarmAnimalCatalog.AnimalChicken);
            Rect bounds = Rect.MinMaxRect(-1f, -1f, 1f, 1f);
            Assert.AreNotEqual(Vector2.zero, state.Advance(0.02f, Vector2.zero, bounds, AlwaysClear));

            state.Freeze(0.4f);
            Assert.AreEqual(Vector2.zero, state.Advance(0.2f, Vector2.zero, bounds, AlwaysClear));
            Assert.IsTrue(state.IsFrozen);

            state.SetHealthStopped(true);
            Assert.AreEqual(Vector2.zero, state.Advance(1f, Vector2.zero, bounds, AlwaysClear));
            Assert.AreEqual(AnimalMotionMode.Stopped, state.Mode);
        }

        [Test]
        public void PenSmallerThanBody_IsRejectedFailClosed()
        {
            Assert.IsFalse(AnimalMotionBounds.CanContain(
                Rect.MinMaxRect(0f, 0f, 0.4f, 0.3f), new Vector2(0.55f, 0.42f), 0.03f));
        }

        [Test]
        public void Restore_IsHousingFilteredAndIdempotentByInstanceId()
        {
            var registryObject = CreateGameObject("AnimalRegistry_MotionTest");
            var registry = registryObject.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(registry);
            registry.RegisterHousing("coop_motion", AnimalHousingBuildingType.Coop, 4);
            registry.RegisterHousing("barn_motion", AnimalHousingBuildingType.Barn, 4);

            var handlerObject = CreateGameObject("Coop_MotionTest");
            var handler = handlerObject.AddComponent<AnimalReleaseHandler>();
            handler.Configure("coop_motion", AnimalHousingBuildingType.Coop, 4, registry, null);
            handler.SetPenBounds(new Vector2(-2f, -1f), new Vector2(2f, 1f));

            var chicken = new AnimalInstanceState("animal_coop_motion_0", FarmAnimalCatalog.AnimalChicken, "Galinha", "coop_motion");
            var goat = new AnimalInstanceState("animal_barn_motion_0", FarmAnimalCatalog.AnimalGoat, "Cabra", "barn_motion");
            var states = new[] { chicken, goat };
            handler.RespawnExistingAnimals(states, null, registry);
            Assert.IsTrue(handler.TryGetRuntime(chicken.AnimalInstanceId, out var first));
            _created.Add(first.gameObject);
            Assert.IsFalse(handler.TryGetRuntime(goat.AnimalInstanceId, out _), "A coop handler must not restore barn animals.");

            handler.RespawnExistingAnimals(states, null, registry);
            Assert.AreEqual(1, handler.RuntimeCount);
            Assert.IsTrue(handler.TryGetRuntime(chicken.AnimalInstanceId, out var second));
            Assert.AreSame(first, second, "Repeated restore must keep one runtime for the stable instance ID.");
        }

        [Test]
        public void Restore_BlockedTemporarily_QueuesThenMaterializesExactlyOnce()
        {
            var registryObject = CreateGameObject("AnimalRegistry_PendingRestoreTest");
            var registry = registryObject.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(registry);
            registry.RegisterHousing("coop_pending", AnimalHousingBuildingType.Coop, 4);
            Assert.AreEqual(FarmAnimalRegistry.ReleaseResult.Success,
                registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_pending", out string instanceId));

            var handlerObject = CreateGameObject("Coop_PendingRestoreTest");
            var handler = handlerObject.AddComponent<AnimalReleaseHandler>();
            handler.Configure("coop_pending", AnimalHousingBuildingType.Coop, 4, registry, null);
            handler.SetPenBounds(new Vector2(-1f, -0.6f), new Vector2(1f, 0.6f));

            var obstacle = CreateGameObject("TemporaryPenObstacle");
            obstacle.transform.position = handlerObject.transform.position;
            obstacle.AddComponent<BoxCollider2D>().size = new Vector2(2.2f, 1.4f);
            Physics2D.SyncTransforms();

            handler.RespawnExistingAnimals(new[] { registry.GetAnimal(instanceId) }, null, registry);
            Assert.AreEqual(0, handler.RuntimeCount);
            Assert.AreEqual(1, handler.PendingRestoreCount, "Saved animal identity must survive temporary blockage.");

            Object.DestroyImmediate(obstacle);
            Physics2D.SyncTransforms();
            handler.TickPendingRestores(1f);
            Assert.AreEqual(0, handler.PendingRestoreCount);
            Assert.AreEqual(1, handler.RuntimeCount);
            Assert.IsTrue(handler.TryGetRuntime(instanceId, out var runtime));
            _created.Add(runtime.gameObject);

            handler.TickPendingRestores(2f);
            Assert.AreEqual(1, handler.RuntimeCount, "Retry completion must not duplicate the restored runtime.");
        }

        [Test]
        public void Restore_DeadAnimal_IsNeitherQueuedNorMaterialized()
        {
            var registryObject = CreateGameObject("AnimalRegistry_DeadRestoreTest");
            var registry = registryObject.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(registry);
            registry.RegisterHousing("coop_dead", AnimalHousingBuildingType.Coop, 4);
            registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_dead", out string instanceId);
            registry.GetAnimal(instanceId).HealthState = AnimalHealthState.Dead;

            var handlerObject = CreateGameObject("Coop_DeadRestoreTest");
            var handler = handlerObject.AddComponent<AnimalReleaseHandler>();
            handler.Configure("coop_dead", AnimalHousingBuildingType.Coop, 4, registry, null);
            handler.RespawnExistingAnimals(new[] { registry.GetAnimal(instanceId) }, null, registry);

            Assert.AreEqual(0, handler.RuntimeCount);
            Assert.AreEqual(0, handler.PendingRestoreCount);
        }

        [Test]
        public void Presenter_StoppedMode_FreezesCurrentPose()
        {
            var texture = new Texture2D(8, 4);
            _created.Add(texture);
            var idle0 = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.25f), 4f);
            var idle1 = Sprite.Create(texture, new Rect(4, 0, 4, 4), new Vector2(0.5f, 0.25f), 4f);
            _created.Add(idle0);
            _created.Add(idle1);
            var profile = ScriptableObject.CreateInstance<AnimalMotionProfileSO>();
            _created.Add(profile);
            profile.Configure(
                FarmAnimalCatalog.AnimalChicken, AnimalMotionSettings.Default,
                new Vector2(0.55f, 0.42f), 0.03f, ~0, 0.45f, 0.1f,
                0.4f, 0.18f, 0.6f,
                new[] { idle0, idle1 }, new[] { idle0 }, new[] { idle0 }, new[] { idle0 },
                new[] { idle0 }, new[] { idle0 });

            var presenterObject = CreateGameObject("AnimalPresenter_StoppedTest");
            var renderer = presenterObject.AddComponent<SpriteRenderer>();
            var presenter = presenterObject.AddComponent<AnimalSpriteAnimator>();
            presenter.Configure(renderer, profile);
            presenter.UpdatePresentation(0.41f, AnimalMotionMode.Idle, AnimalFacingDirection.Down, 0f, 0f);
            Sprite frozen = renderer.sprite;
            int frozenIndex = presenter.CurrentFrameIndex;

            presenter.UpdatePresentation(10f, AnimalMotionMode.Stopped, AnimalFacingDirection.Down, 50f, 1f);

            Assert.AreSame(frozen, renderer.sprite);
            Assert.AreEqual(frozenIndex, presenter.CurrentFrameIndex,
                "Dead/Unavailable presentation must not advance living idle frames.");
        }

        private GameObject CreateGameObject(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        private static bool AlwaysClear(Vector2 origin, Vector2 displacement) => true;
        private static bool NeverClear(Vector2 origin, Vector2 displacement) => false;
    }
}
