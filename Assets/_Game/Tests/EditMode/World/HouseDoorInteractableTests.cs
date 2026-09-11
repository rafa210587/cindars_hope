using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    public sealed class DoorTravelerTestStub : MonoBehaviour, INpcDoorTraveler { }

    public class HouseDoorInteractableTests
    {
        private GameObject _root, _actor;
        private HouseDoorInteractable _door;
        private BoxCollider2D _blocker, _actorCollider;
        private SpriteRenderer _leaf;
        private Texture2D _texture;
        private Sprite[] _frames;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("DoorTest");
            _root.transform.position = new Vector3(7000f, 7000f);
            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(_root.transform, false);
            _leaf = leaf.AddComponent<SpriteRenderer>();
            _blocker = _root.AddComponent<BoxCollider2D>();
            _blocker.size = new Vector2(1f, 0.22f);
            _door = _root.AddComponent<HouseDoorInteractable>();
            _door.Configure(leaf.transform, _blocker, Vector3.zero, Vector3.right);
            _texture = new Texture2D(4, 1);
            _frames = new Sprite[4];
            for (var i = 0; i < _frames.Length; i++)
                _frames[i] = Sprite.Create(_texture, new Rect(i, 0, 1, 1), Vector2.zero, 1f);
            _actor = new GameObject("Actor") { tag = "Player" };
            _actor.transform.position = _root.transform.position + Vector3.down * 4f;
            _actorCollider = _actor.AddComponent<BoxCollider2D>();
            _actorCollider.size = new Vector2(0.4f, 0.6f);
            Physics2D.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_actor);
            foreach (var frame in _frames) Object.DestroyImmediate(frame);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void LegacyDoor_StillOpensImmediatelyAndHidesLeaf()
        {
            _door.Interact(_actor);
            Assert.That(_door.IsOpen, Is.True);
            Assert.That(_blocker.enabled, Is.False);
            Assert.That(_leaf.enabled, Is.False);
            Assert.That(_leaf.transform.localPosition, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void ConfigureAfterPresentation_RestoresLegacyImmediateBehavior()
        {
            ConfigureFrames();
            _door.Interact(_actor);
            Advance(0.11f);
            _door.Configure(_leaf.transform, _blocker, Vector3.zero, Vector3.right);
            _door.Interact(_actor);
            Assert.That(_door.IsOpen, Is.True);
            Assert.That(_blocker.enabled, Is.False);
            Assert.That(_leaf.enabled, Is.False);
            Assert.That(_leaf.transform.localPosition, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void AnimatedOpening_ReleasesBlockerOnlyAtFinalClearPose()
        {
            ConfigureFrames();
            _door.Interact(_actor);
            Advance(0.11f);
            Assert.That(_leaf.sprite, Is.SameAs(_frames[1]));
            Assert.That(_blocker.enabled, Is.True);
            Assert.That(_door.IsOpen, Is.False);
            Assert.That(_leaf.transform.localPosition, Is.EqualTo(Vector3.zero));
            Advance(0.21f);
            Assert.That(_leaf.sprite, Is.SameAs(_frames[3]));
            Assert.That(_leaf.enabled, Is.True);
            Assert.That(_blocker.enabled, Is.False);
            Assert.That(_door.IsOpen, Is.True);
            Assert.That(_door.IsAnimating, Is.False);
        }

        [Test]
        public void AnimatedTransition_PublishesOnePhysicalEventOnlyAtCompletion()
        {
            ConfigureFrames();
            var completed = 0;
            var opened = false;
            var subscription = GameEventBus.Subscribe<HouseDoorTransitionCompletedEvent>(evt => { completed++; opened = evt.IsOpen; });
            try
            {
                _door.Interact(_actor);
                Assert.That(completed, Is.EqualTo(0));
                Advance(0.31f);
                Assert.That(completed, Is.EqualTo(1));
                Assert.That(opened, Is.True);
                _door.Interact(_actor);
                Advance(0.31f);
                Assert.That(completed, Is.EqualTo(2));
                Assert.That(opened, Is.False);
            }
            finally { subscription.Dispose(); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DisabledBlocker_StillDetectsActorAndRefusesClosing(bool npc)
        {
            if (npc)
            {
                _actor.tag = "Untagged";
                _actor.AddComponent<DoorTravelerTestStub>();
            }
            ConfigureFrames();
            OpenFully();
            _actor.transform.position = _root.transform.position;
            Physics2D.SyncTransforms();
            _door.Interact(_actor);
            Advance(0.5f);
            Assert.That(_door.IsOpen, Is.True);
            Assert.That(_blocker.enabled, Is.False);
            Assert.That(_leaf.sprite, Is.SameAs(_frames[3]));
        }

        [Test]
        public void ActorEnteringDuringClosing_ReversesBeforeBlockerReturns()
        {
            ConfigureFrames();
            OpenFully();
            _door.Interact(_actor);
            Advance(0.11f);
            Assert.That(_leaf.sprite, Is.SameAs(_frames[2]));
            Assert.That(_blocker.enabled, Is.False);
            _actor.transform.position = _root.transform.position;
            Physics2D.SyncTransforms();
            Advance(0.11f);
            Assert.That(_door.IsOpen, Is.True);
            Assert.That(_blocker.enabled, Is.False);
            Assert.That(_leaf.sprite, Is.SameAs(_frames[3]));
        }

        [Test]
        public void TriggerSensorAlone_DoesNotPreventClosing()
        {
            ConfigureFrames();
            OpenFully();
            _actorCollider.isTrigger = true;
            _actor.transform.position = _root.transform.position;
            Physics2D.SyncTransforms();
            _door.Interact(_actor);
            Advance(0.31f);
            Assert.That(_door.IsOpen, Is.False);
            Assert.That(_blocker.enabled, Is.True);
            Assert.That(_leaf.sprite, Is.SameAs(_frames[0]));
        }

        [Test]
        public void Occupancy_UsesScaledOffsetDoorwayRatherThanRootPosition()
        {
            _root.transform.localScale = new Vector3(2f, 1.5f, 1f);
            _root.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            _blocker.offset = new Vector2(1f, 0.2f);
            ConfigureFrames();
            OpenFully();
            _actor.transform.position = _blocker.transform.TransformPoint(_blocker.offset);
            Physics2D.SyncTransforms();
            _door.Interact(_actor);
            Advance(0.5f);
            Assert.That(_blocker.enabled, Is.False);
            Assert.That(_door.IsOpen, Is.True);
        }

        private void ConfigureFrames() => _door.ConfigurePresentation(_leaf, _frames, 0.1f);
        private void OpenFully() { _door.Interact(_actor); Advance(0.31f); }
        private void Advance(float seconds)
        {
            typeof(HouseDoorInteractable).GetMethod("Advance", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_door, new object[] { seconds });
        }
    }
}
