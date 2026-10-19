using System;
using System.Collections;
using System.Reflection;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.InputRouting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21PlayerKeyReaderPlayModeTests
    {
        private sealed class ControlledKeys : IPlayerKeyReader
        {
            public bool HoldRight;
            public bool PressQ;
            public bool ReleaseQ;

            public bool GetKey(KeyCode key) => HoldRight && key == KeyCode.D;
            public bool GetKeyDown(KeyCode key) => PressQ && key == KeyCode.Q;
            public bool GetKeyUp(KeyCode key) => ReleaseQ && key == KeyCode.Q;
        }

        [UnityTest]
        public IEnumerator MovementReader_UsesRealUpdateAndRespectsMovementLock()
        {
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            var controller = PlayerController.ActiveInstance;
            Assert.That(controller, Is.Not.Null, "FarmScene não materializou o player real.");
            Assert.That(controller.KeyReader, Is.SameAs(UnityLegacyPlayerKeyReader.Instance));
            var rigidbody = controller.GetComponent<Rigidbody2D>();
            Assert.That(rigidbody, Is.Not.Null);
            var originalPosition = rigidbody.position;
            var originalTransformPosition = controller.transform.position;
            var originalMovementLocked = controller.MovementLocked;
            var keys = new ControlledKeys { HoldRight = true };
            controller.KeyReader = keys;

            try
            {
                Assert.That(controller.KeyReader, Is.SameAs(keys));
                yield return null;
                Assert.That(controller.MoveInput, Is.EqualTo(Vector2.right));

                controller.MovementLocked = true;
                yield return null;
                Assert.That(controller.MoveInput, Is.EqualTo(Vector2.zero));

                controller.KeyReader = null;
                Assert.That(controller.KeyReader, Is.SameAs(UnityLegacyPlayerKeyReader.Instance));
            }
            finally
            {
                controller.KeyReader = null;
                controller.MovementLocked = originalMovementLocked;
                rigidbody.position = originalPosition;
                controller.transform.position = originalTransformPosition;
            }
        }

        [UnityTest]
        public IEnumerator AttackReader_QEdgesAndModalGuard_UseRealUpdate()
        {
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            var player = PlayerController.ActiveInstance;
            Assert.That(player, Is.Not.Null, "FarmScene não materializou o player real.");
            var attack = player.GetComponent<PlayerAttackController>();
            Assert.That(attack, Is.Not.Null, "PlayerAttackController não está no player da FarmScene.");
            var chargeField = typeof(PlayerAttackController).GetField("_leftCharge", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(chargeField, Is.Not.Null);
            var charge = chargeField.GetValue(attack) as AttackChargeTracker;
            Assert.That(charge, Is.Not.Null);
            Assert.That(charge.IsCharging, Is.False);
            var modal = GameBootstrap.Instance?.ModalManager;
            Assert.That(modal, Is.Not.Null);
            Assert.That(modal.HasActiveModal, Is.False, "A FarmScene abriu um modal antes do teste de input.");
            var rigidbody = player.GetComponent<Rigidbody2D>();
            var originalPosition = rigidbody.position;
            var originalTransformPosition = player.transform.position;
            var stamina = GameBootstrap.Instance.StaminaManager as StaminaManager;
            Assert.That(stamina, Is.Not.Null);
            var originalMaxStamina = stamina.MaxStamina;
            var originalStamina = stamina.CurrentStamina;
            player.transform.position = new Vector3(10000f, 10000f, originalTransformPosition.z);
            rigidbody.position = player.transform.position;
            var keys = new ControlledKeys();
            attack.KeyReader = keys;
            var windups = 0;
            Action<PlayerAttackWindupEvent> onWindup = _ => windups++;

            GameEventBus.Subscribe(onWindup);
            var pushedModal = false;
            try
            {
                keys.PressQ = true;
                yield return null;
                Assert.That(windups, Is.EqualTo(1));
                Assert.That(charge.IsCharging, Is.True);

                keys.PressQ = false;
                keys.ReleaseQ = true;
                yield return null;
                Assert.That(charge.IsCharging, Is.False, "GetKeyUp deve liberar a carga.");

                keys.ReleaseQ = false;
                pushedModal = modal.PushModal(ModalType.SkillTree);
                Assert.That(pushedModal, Is.True);
                keys.PressQ = true;
                yield return null;
                Assert.That(windups, Is.EqualTo(1), "Modal deve bloquear novo windup.");
                Assert.That(charge.IsCharging, Is.False);
            }
            finally
            {
                if (pushedModal)
                    modal.TryPopModal(ModalType.SkillTree, out _);
                GameEventBus.Unsubscribe(onWindup);
                attack.KeyReader = null;
                stamina.Initialize(originalMaxStamina, originalStamina);
                rigidbody.position = originalPosition;
                player.transform.position = originalTransformPosition;
            }
        }
    }
}
