using System;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Tests.EditMode.Save
{
    public sealed class TownInteriorLocationSaveTests
    {
        private GameObject _saveManagerObject;
        private SaveManager _saveManager;

        [SetUp]
        public void SetUp()
        {
            GameEventBus.ClearAll();
            _saveManagerObject = new GameObject("TownInteriorLocationSaveTests_SaveManager");
            _saveManager = _saveManagerObject.AddComponent<SaveManager>();
        }

        [TearDown]
        public void TearDown()
        {
            GameEventBus.ClearAll();
            if (_saveManagerObject != null) UnityEngine.Object.DestroyImmediate(_saveManagerObject);
        }

        [Test]
        public void CaptureAndRestore_RoundTripsInteriorPoseAndIds()
        {
            var source = Snapshot("blacksmith", "entry", "return", 1.5f, -2.25f, 0f, 1f, 11f, 12f);
            TownInteriorLocationSnapshot restored = default;
            var provider = new TownInteriorLocationSectionProvider(() => source, value => restored = value);

            var data = provider.Capture(null) as TownInteriorLocationSaveData;
            var persisted = JsonUtility.FromJson<TownInteriorLocationSaveData>(JsonUtility.ToJson(data));
            provider.Restore(persisted);

            Assert.That(persisted, Is.Not.Null);
            Assert.That(persisted.HouseId, Is.EqualTo("blacksmith"));
            Assert.That(persisted.AnchorId, Is.EqualTo("entry"));
            Assert.That(persisted.ReturnAnchorId, Is.EqualTo("return"));
            Assert.That(persisted.FacingX, Is.EqualTo(0f));
            Assert.That(restored.LocalX, Is.EqualTo(1.5f));
            Assert.That(restored.LocalY, Is.EqualTo(-2.25f));
            Assert.That(restored.FacingX, Is.EqualTo(0f));
            Assert.That(restored.FacingY, Is.EqualTo(1f));
            Assert.That(restored.SafeTownX, Is.EqualTo(11f));
            Assert.That(restored.SafeTownY, Is.EqualTo(12f));
        }

        [Test]
        public void Restore_MissingSection_DoesNotInvokeCallback()
        {
            var restored = false;
            var provider = new TownInteriorLocationSectionProvider(() => null, _ => restored = true);

            provider.Restore(null);

            Assert.That(restored, Is.False);
        }

        [TestCase(" ", "entry", "return")]
        [TestCase("inn", "", "return")]
        public void TryDecode_InvalidIds_ReturnsReason(string houseId, string anchorId, string returnAnchorId)
        {
            var data = new TownInteriorLocationSaveData
            {
                HouseId = houseId, AnchorId = anchorId, ReturnAnchorId = returnAnchorId,
                LocalX = 1f, LocalY = 2f, FacingX = 1f, FacingY = 0f, SafeTownX = 3f, SafeTownY = 4f
            };

            var decoded = TownInteriorLocationSaveData.TryDecode(data, out _, out var reason);

            Assert.That(decoded, Is.False);
            Assert.That(reason, Is.Not.Empty);
        }

        [Test]
        public void Restore_InvalidData_ThrowsWithoutInvokingCallback()
        {
            var restored = false;
            var provider = new TownInteriorLocationSectionProvider(() => null, _ => restored = true);
            var valid = new TownInteriorLocationSaveData
            {
                HouseId = "inn", AnchorId = "entry", ReturnAnchorId = "return",
                LocalX = 1f, LocalY = 2f, FacingX = 1f, FacingY = 0f, SafeTownX = 3f, SafeTownY = 4f
            };

            valid.Version = 2;
            Assert.That(() => provider.Restore(valid), Throws.ArgumentException);
            valid.Version = 1;
            valid.LocalX = float.NaN;
            Assert.That(() => provider.Restore(valid), Throws.ArgumentException);
            valid.LocalX = 1f;
            valid.SafeTownY = float.PositiveInfinity;
            Assert.That(() => provider.Restore(valid), Throws.ArgumentException);
            provider.Restore("wrong type");

            Assert.That(restored, Is.False);
        }

        [Test]
        public void Capture_InvalidInteriorSnapshot_ThrowsInsteadOfOmittingLocation()
        {
            var invalid = Snapshot("inn", "entry", "return", float.NaN, 2f, 1f, 0f, 3f, 4f);
            var provider = new TownInteriorLocationSectionProvider(() => invalid, _ => { });

            Assert.That(() => provider.Capture(null), Throws.InvalidOperationException);
        }

        [Test]
        public void Capture_UsesOneFrozenSnapshotAndOmitsExterior()
        {
            var calls = 0;
            var first = Snapshot("inn", "entry_a", "return_a", 2f, 3f, 1f, 0f, 4f, 5f);
            var second = Snapshot("other", "entry_b", "return_b", 8f, 9f, 0f, 1f, 10f, 11f);
            var provider = new TownInteriorLocationSectionProvider(
                () => ++calls == 1 ? first : second,
                _ => { });

            var data = provider.Capture(null) as TownInteriorLocationSaveData;
            var exterior = new TownInteriorLocationSectionProvider(
                () => new TownInteriorLocationSnapshot(string.Empty, string.Empty, string.Empty, 0f, 0f, 1f, 0f, 4f, 5f),
                _ => { });

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(data.HouseId, Is.EqualTo("inn"));
            Assert.That(data.AnchorId, Is.EqualTo("entry_a"));
            Assert.That(exterior.Capture(null), Is.Null);
        }

        [Test]
        public void GameSaveData_JsonRoundTripsAdditiveInteriorSection()
        {
            var save = new GameSaveData
            {
                TownInteriorLocation = new TownInteriorLocationSaveData
                {
                    HouseId = "inn", AnchorId = "entry", ReturnAnchorId = "return",
                    LocalX = 1f, LocalY = 2f, FacingX = 0f, FacingY = 1f, SafeTownX = 3f, SafeTownY = 4f
                }
            };

            var restored = JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(save));

            Assert.That(restored.TownInteriorLocation, Is.Not.Null);
            Assert.That(restored.TownInteriorLocation.HouseId, Is.EqualTo("inn"));
            Assert.That(restored.TownInteriorLocation.FacingY, Is.EqualTo(1f));
        }

        [Test]
        public void LegacyLoad_InsideReadyInterior_IsBlockedWithExitGuidance()
        {
            BindReadyRuntime(Snapshot("inn", "entry", "return", 1f, 2f, 1f, 0f, 3f, 4f));

            var blocked = InvokeLegacyLoadGuard(out var reason);

            Assert.That(blocked, Is.True);
            Assert.That(reason, Does.Contain("Saia da casa"));
        }

        [Test]
        public void LegacyLoad_ReadyExteriorRuntime_IsNotBlocked()
        {
            BindReadyRuntime(Snapshot(string.Empty, string.Empty, string.Empty, 0f, 0f, 1f, 0f, 3f, 4f));

            var blocked = InvokeLegacyLoadGuard(out var reason);

            Assert.That(blocked, Is.False);
            Assert.That(reason, Is.Empty);
        }

        [Test]
        public void LegacyLoad_ExpectedTownRuntimeUnbound_IsBlockedUntilReady()
        {
            SetSceneHandle("_lastTownInteriorSceneHandle");

            var blocked = InvokeLegacyLoadGuard(out var reason);

            Assert.That(blocked, Is.True);
            Assert.That(reason, Does.Contain("cidade estar pronta"));
        }

        [Test]
        public void CaptureTownInteriorSnapshot_RuntimeExpectedButNotReady_ThrowsInsteadOfOmittingLocation()
        {
            SetPrivateField("_townInteriorRuntime", new FakeInteriorRuntime(Snapshot("inn", "entry", "return", 1f, 2f, 1f, 0f, 3f, 4f)));
            SetSceneHandle("_townInteriorSceneHandle");
            SetSceneHandle("_lastTownInteriorSceneHandle");

            var exception = Assert.Throws<TargetInvocationException>(() => InvokePrivate("CaptureTownInteriorSnapshot"));

            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Does.Contain("not ready"));
        }

        [Test]
        public void SaveGame_TransitioningInterior_RefusesBeforeAnyWriteAndPublishesFailure()
        {
            SetPrivateField("_townInteriorRuntime", new FakeInteriorRuntime(default, isTransitioning: true));
            GameSavedEvent? result = null;
            GameEventBus.Subscribe<GameSavedEvent>(evt => result = evt);

            var saved = _saveManager.SaveGame();

            Assert.That(saved, Is.False);
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value.WasSuccessful, Is.False);
            Assert.That(result.Value.Message, Does.Contain("transição de interior"));
        }

        [Test]
        public void InteriorRestoreFinished_OnlyMatchingOperationCompletesPendingRestore()
        {
            SetPrivateField("_pendingInteriorRestoreToken", "expected-operation");

            InvokePrivate("OnTownInteriorVisitFinished",
                new TownInteriorVisitFinishedEvent("stale-operation", "inn", true, string.Empty));

            Assert.That(GetPrivateField<bool>("_pendingInteriorRestoreFinished"), Is.False);
            Assert.That(GetPrivateField<bool>("_pendingInteriorRestoreSucceeded"), Is.False);

            InvokePrivate("OnTownInteriorVisitFinished",
                new TownInteriorVisitFinishedEvent("expected-operation", "inn", true, string.Empty));

            Assert.That(GetPrivateField<bool>("_pendingInteriorRestoreFinished"), Is.True);
            Assert.That(GetPrivateField<bool>("_pendingInteriorRestoreSucceeded"), Is.True);
        }

        private void BindReadyRuntime(TownInteriorLocationSnapshot snapshot)
        {
            SetPrivateField("<IsInitialized>k__BackingField", true);
            SetPrivateField("_townInteriorRuntime", new FakeInteriorRuntime(snapshot));
            SetSceneHandle("_townInteriorSceneHandle");
            SetSceneHandle("_lastTownInteriorSceneHandle");
        }

        private bool InvokeLegacyLoadGuard(out string reason)
        {
            var arguments = new object[] { null };
            var blocked = (bool)InvokePrivate("IsLegacyLoadBlockedByCurrentInterior", arguments);
            reason = (string)arguments[0];
            return blocked;
        }

        private object InvokePrivate(string methodName, params object[] arguments)
        {
            var method = typeof(SaveManager).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing SaveManager seam: {methodName}");
            return method.Invoke(_saveManager, arguments);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = GetPrivateField(fieldName);
            field.SetValue(_saveManager, value);
        }

        private T GetPrivateField<T>(string fieldName) => (T)GetPrivateField(fieldName).GetValue(_saveManager);

        private static FieldInfo GetPrivateField(string fieldName)
        {
            var field = typeof(SaveManager).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing SaveManager seam: {fieldName}");
            return field;
        }

        private void SetSceneHandle(string fieldName)
        {
            var field = GetPrivateField(fieldName);
            field.SetValue(_saveManager, SceneManager.GetActiveScene().handle);
        }

        private sealed class FakeInteriorRuntime : ITownInteriorLocationRuntime
        {
            private readonly TownInteriorLocationSnapshot _snapshot;

            public FakeInteriorRuntime(TownInteriorLocationSnapshot snapshot, bool isTransitioning = false)
            {
                _snapshot = snapshot;
                IsTransitioning = isTransitioning;
            }

            public bool IsTransitioning { get; }
            public TownInteriorLocationSnapshot CaptureCommitted() => _snapshot;
            public bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, out string reason)
            {
                reason = string.Empty;
                return true;
            }

            public bool TryRequestRestore(TownInteriorLocationSnapshot snapshot, string operationId, out string reason)
            {
                reason = string.Empty;
                return true;
            }
        }

        private static TownInteriorLocationSnapshot Snapshot(string houseId, string anchorId, string returnAnchorId,
            float localX, float localY, float facingX, float facingY, float safeTownX, float safeTownY) =>
            new TownInteriorLocationSnapshot(houseId, anchorId, returnAnchorId, localX, localY,
                facingX, facingY, safeTownX, safeTownY);
    }
}
