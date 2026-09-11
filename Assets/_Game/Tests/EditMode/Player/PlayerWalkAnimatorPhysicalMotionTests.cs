using System.Reflection;
using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    public class PlayerWalkAnimatorPhysicalMotionTests
    {
        private GameObject _player;
        private PlayerController _controller;
        private PlayerWalkAnimator _animator;
        private SpriteRenderer _renderer;
        private Rigidbody2D _body;

        [TearDown]
        public void TearDown()
        {
            if (_animator != null) InvokeLifecycle("OnDisable");
            if (_player != null) Object.DestroyImmediate(_player);
        }

        [Test]
        public void DisabledController_PhysicalMoveUpThenStop_ShowsWalkUpThenIdleUp()
        {
            CreatePlayer(withBody: true, controllerEnabled: false);
            _body.position = new Vector2(15f, 10f);
            SamplePhysics();
            Tick();
            AssertUsesFrames("idle/right"); // Framing is not motion.

            _body.position += Vector2.up * 0.1f;
            SamplePhysics();
            Assert.That(_body.linearVelocity, Is.EqualTo(Vector2.zero));
            Tick();
            AssertUsesFrames("walk/up");

            SamplePhysics(); // Unchanged body position must stop walking, retaining physical facing.
            Tick();
            AssertUsesFrames("idle/up");
        }

        [Test]
        public void EnabledController_ExternalDisplacement_DoesNotReplaceInputOrFacing()
        {
            CreatePlayer(withBody: true, controllerEnabled: true);
            SamplePhysics();
            _body.position += Vector2.up * 0.1f;
            SamplePhysics();
            Tick();
            AssertUsesFrames("idle/right");

            // Fixture supplies controller state; production receives no synthetic input API.
            typeof(PlayerController).GetProperty(nameof(PlayerController.MoveInput))
                .GetSetMethod(true).Invoke(_controller, new object[] { Vector2.left });
            Tick();
            AssertUsesFrames("walk/left");
        }

        [Test]
        public void DisabledController_WithoutRigidbody_KeepsSafeIdleFacing()
        {
            CreatePlayer(withBody: false, controllerEnabled: false);
            Assert.DoesNotThrow(SamplePhysics);
            Assert.DoesNotThrow(Tick);
            AssertUsesFrames("idle/right");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PhysicalSampling_Reset_DoesNotAnimateReframing(bool resetThroughController)
        {
            CreatePlayer(withBody: true, controllerEnabled: false);
            SamplePhysics();
            _body.position += Vector2.up * 0.1f;
            SamplePhysics();
            Tick();
            AssertUsesFrames("walk/up");

            if (resetThroughController)
            {
                _controller.enabled = true;
                SamplePhysics();
                _controller.enabled = false;
            }
            else
            {
                InvokeLifecycle("OnDisable");
                InvokeLifecycle("OnEnable");
            }
            _body.position = new Vector2(-12f, 30f);
            SamplePhysics();
            Tick();
            AssertUsesFrames("idle/right");
        }

        private void CreatePlayer(bool withBody, bool controllerEnabled)
        {
            _player = new GameObject("PlayerWalkAnimatorPhysicalMotionTest");
            _player.SetActive(false);
            _renderer = _player.AddComponent<SpriteRenderer>();
            if (withBody) _body = _player.AddComponent<Rigidbody2D>();
            _controller = _player.AddComponent<PlayerController>();
            _controller.enabled = controllerEnabled;
            _animator = _player.AddComponent<PlayerWalkAnimator>();
            // Explicit EditMode lifecycle: no scene, input polling or physics simulation.
            typeof(PlayerWalkAnimator).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_animator, null);
        }

        private void Tick() => typeof(PlayerWalkAnimator)
            .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_animator, null);

        private void SamplePhysics() => InvokeLifecycle("FixedUpdate");

        private void InvokeLifecycle(string method) => typeof(PlayerWalkAnimator)
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_animator, null);

        private void AssertUsesFrames(string folder)
        {
            var frames = Resources.LoadAll<Sprite>("PlayerSprites/" + folder);
            Assert.That(frames, Is.Not.Empty, "The real directional art must be imported for this regression.");
            Assert.That(frames, Does.Contain(_renderer.sprite), "Rendered sprite must belong to " + folder);
        }
    }
}
