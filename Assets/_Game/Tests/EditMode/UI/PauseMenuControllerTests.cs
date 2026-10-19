using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Foundation;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Routing;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.UI
{
    public class PauseMenuControllerTests
    {
        private GameObject _host;
        private ModalManager _modal;
        private PauseMenuController _pause;
        [SetUp] public void SetUp()
        {
            GameTimeScaleCoordinator.Reset();
            _host = new GameObject("PauseTest");
            _modal = _host.AddComponent<ModalManager>();
            _pause = _host.AddComponent<PauseMenuController>();
            InvokeLifecycle("OnEnable");
        }
        [TearDown] public void TearDown()
        {
            InvokeLifecycle("OnDisable");
            Object.DestroyImmediate(_host);
            GameTimeScaleCoordinator.Reset();
        }
        private void InvokeLifecycle(string method)
            => typeof(PauseMenuController).GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(_pause, null);
        [Test] public void OpenRepeated_CloseEvent_ReleasesExactlyOnePause()
        {
            Assert.That(_pause.Open(_modal), Is.True);
            Assert.That(_pause.Open(_modal), Is.True);
            Assert.That(GameTimeScaleCoordinator.ActiveTokenCount, Is.EqualTo(1));
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(_modal.CurrentModal, Is.EqualTo(ModalType.Pause));
            GameEventBus.Publish(new ModalCloseRequestedEvent());
            Assert.That(_pause.IsOpen, Is.False);
            Assert.That(_modal.HasActiveModal, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }
        [Test] public void Disable_ReleasesPauseWithoutReleasingOtherOwner()
        {
            using (GameTimeScaleCoordinator.AcquirePause())
            {
                _pause.Open(_modal);
                _pause.enabled = false;
                InvokeLifecycle("OnDisable");
                Assert.That(GameTimeScaleCoordinator.ActiveTokenCount, Is.EqualTo(1));
                Assert.That(_modal.HasActiveModal, Is.False);
                Assert.That(Time.timeScale, Is.Zero);
            }
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }
        [Test] public void CloseRequestForInventory_DoesNotCloseNewPauseOwner()
        {
            _pause.Open(_modal);
            GameEventBus.Publish(new ModalCloseRequestedEvent(ModalType.Inventory));
            Assert.That(_pause.IsOpen, Is.True);
            Assert.That(_modal.CurrentModal, Is.EqualTo(ModalType.Pause));
            GameEventBus.Publish(new ModalCloseRequestedEvent(ModalType.Pause));
            Assert.That(_pause.IsOpen, Is.False);
        }
        [Test] public void MissingModal_DoesNotFreezeSimulation()
        {
            Assert.That(_pause.Open(null), Is.False);
            Assert.That(GameTimeScaleCoordinator.ActiveTokenCount, Is.Zero);
        }
    }
}
