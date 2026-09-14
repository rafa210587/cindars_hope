using System.Collections;
using System.Reflection;
using CindarsHope.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Interaction
{
    public sealed class InteractionCandidateTestInteractable : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => "Abrir porta";
        public bool CanInteract(GameObject interactor) => true;
        public void Interact(GameObject interactor) { }
    }

    public sealed class InteractionSystemCandidateTests
    {
        private GameObject _player;
        private InteractionSystem _system;
        private GameObject _door;
        private BoxCollider2D _sensor;
        private BoxCollider2D _blocker;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("InteractionSystemCandidatePlayer");
            _player.AddComponent<CircleCollider2D>();
            _system = _player.AddComponent<InteractionSystem>();

            _door = new GameObject("InteractionSystemCandidateDoor");
            _door.AddComponent<InteractionCandidateTestInteractable>();
            _sensor = CreateDoorCollider("Sensor");
            _blocker = CreateDoorCollider("Blocker");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_door);
        }

        [Test]
        public void UnregisteringOneOfTwoDoorColliders_KeepsTheOtherCandidateAndPrompt()
        {
            _system.RegisterCandidate(_sensor);
            _system.RegisterCandidate(_blocker);

            _blocker.enabled = false;
            _system.UnregisterCandidate(_blocker);

            Assert.That(CandidateCount, Is.EqualTo(1));
            Assert.That(_system.HasCandidate, Is.True);
            Assert.That(_system.CurrentPrompt, Is.EqualTo("Abrir porta"));
        }

        [Test]
        public void RegisteringSameColliderTwice_DoesNotDuplicateCandidate()
        {
            _system.RegisterCandidate(_sensor);
            _system.RegisterCandidate(_sensor);

            Assert.That(CandidateCount, Is.EqualTo(1));
        }

        [Test]
        public void UnregisteringLastCollider_ClearsCandidateAndPrompt()
        {
            _system.RegisterCandidate(_sensor);
            _system.UnregisterCandidate(_sensor);

            Assert.That(_system.HasCandidate, Is.False);
            Assert.That(_system.CurrentPrompt, Is.Empty);
        }

        [Test]
        public void DisabledCollider_IsRemovedFromCandidates()
        {
            _system.RegisterCandidate(_sensor);
            _sensor.enabled = false;

            Assert.That(_system.HasCandidate, Is.False);
            Assert.That(CandidateCount, Is.Zero);
        }

        [Test]
        public void DestroyedCollider_IsRemovedFromCandidates()
        {
            _system.RegisterCandidate(_sensor);
            Object.DestroyImmediate(_sensor.gameObject);

            Assert.That(_system.HasCandidate, Is.False);
            Assert.That(CandidateCount, Is.Zero);
        }

        private int CandidateCount => ((IList)typeof(InteractionSystem)
            .GetField("_candidates", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(_system)).Count;

        private BoxCollider2D CreateDoorCollider(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(_door.transform, false);
            return child.AddComponent<BoxCollider2D>();
        }
    }
}
