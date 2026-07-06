using CindarsHope.Farm;
using CindarsHope.Gameplay.Input;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    public sealed class FarmPlotInputBlockIntegrationTests
    {
        private GameObject _ownerObject;
        private FarmPlotMenuController _menu;

        [SetUp]
        public void SetUp()
        {
            GameplayInputBlocker.Reset();
            _ownerObject = new GameObject("FarmPlotInputBlockTest");
            var owner = _ownerObject.AddComponent<FarmPlot>();
            _menu = new FarmPlotMenuController(owner);
        }

        [TearDown]
        public void TearDown()
        {
            _menu?.CloseMenu();
            GameplayInputBlocker.Reset();
            if (_ownerObject != null) Object.DestroyImmediate(_ownerObject);
        }

        [Test]
        public void OpenAndCloseMenu_AcquiresAndReleasesGameplayInputGate()
        {
            _menu.OpenMenu(FarmPlotState.Raw);

            Assert.That(GameplayInputBlocker.IsBlockedBy(GameplayInputBlockReason.FarmActionMenu), Is.True);

            _menu.CloseMenu();
            Assert.That(GameplayInputBlocker.IsBlocked, Is.False);
        }
    }
}
