using System;
using CindarsHope.Foundation;
using CindarsHope.SceneManagement;
using NUnit.Framework;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.World.Scenes;
using UnityEngine.SceneManagement;

namespace CindarsHope.Tests.EditMode.SceneManagement
{
    [TestFixture]
    public sealed class TownInteriorTransitionTests
    {
        private sealed class Admission : ITownInteriorVisitAdmission
        {
            public bool Allowed;
            public bool CanBegin(TownInteriorVisitRequest request, out string reason)
            { reason = Allowed ? string.Empty : "not ready"; return Allowed; }
        }

        [Test]
        public void Router_InteriorAdmissionHasNoSingleSideEffects()
        {
            var admission = new Admission();
            var previousSpawn = SceneTransitionState.PendingSpawnId;
            int visits = 0, singles = 0;
            Action<TownInteriorVisitRequestedEvent> visit = _ => visits++;
            Action<SceneTransitionStartedEvent> single = _ => singles++;
            Assert.IsTrue(SceneTransitionRouter.RegisterTownInteriorVisitAdmission(admission));
            var previousScene = SceneManager.GetActiveScene();
            var previousSceneName = previousScene.name;
            GameEventBus.Subscribe(visit);
            GameEventBus.Subscribe(single);
            try
            {
                previousScene.name = "TownInteriorRouterTest";
                SceneTransitionState.SetPendingSpawn("unrelated-existing-spawn");
                var request = new SceneTransitionRequest(SceneManager.GetActiveScene().name, Request());
                Assert.IsFalse(SceneTransitionRouter.Execute(request).Succeeded);
                Assert.AreEqual(0, visits);
                admission.Allowed = true;
                Assert.IsTrue(SceneTransitionRouter.Execute(request).Succeeded);
                Assert.AreEqual(1, visits);
                Assert.AreEqual(0, singles);
                Assert.AreEqual("unrelated-existing-spawn", SceneTransitionState.PendingSpawnId);
                Assert.IsFalse(SceneTransitionRouter.RegisterTownInteriorVisitAdmission(new Admission()));
            }
            finally
            {
                SceneTransitionRouter.UnregisterTownInteriorVisitAdmission(admission);
                GameEventBus.Unsubscribe(visit);
                GameEventBus.Unsubscribe(single);
                SceneTransitionState.SetPendingSpawn(previousSpawn);
                if (previousScene.IsValid() && previousScene.isLoaded) previousScene.name = previousSceneName;
            }
        }

        [Test]
        public void Request_OptInDoesNotChangeLegacyValidation()
        {
            var legacy = new SceneTransitionRequest("FarmScene", "TownScene", "town_spawn");
            Assert.AreEqual(SceneTransitionKind.Single, legacy.Kind);
            Assert.IsTrue(legacy.IsValid());
            Assert.IsFalse(new SceneTransitionRequest("TownScene", default(TownInteriorVisitRequest)).IsValid());
            Assert.IsTrue(new SceneTransitionRequest("TownScene", Request()).IsValid());
            Assert.IsFalse(new SceneTransitionRequest("FarmScene", "TownScene", "").IsValid());
        }

        private static TownInteriorVisitRequest Request(string operationId = "visit-1") =>
            new TownInteriorVisitRequest(operationId, "House_GateKeeper", "town_entry_House_GateKeeper",
                "town_return_House_GateKeeper", false);

        [Test]
        public void ForwardVisit_RequiresEveryPhaseBeforeCompletion()
        {
            var machine = new TownInteriorTransitionStateMachine();
            Assert.IsTrue(machine.TryBegin(Request(), out var reason), reason);
            Assert.IsFalse(machine.Advance("visit-1", TownVisitPhase.Opening, TownVisitPhase.Loading));
            machine.Complete("visit-1");
            Assert.AreEqual(TownVisitPhase.Opening, machine.Phase);
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Opening, TownVisitPhase.Armed));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Armed, TownVisitPhase.FadingOut));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.FadingOut, TownVisitPhase.Loading));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Loading, TownVisitPhase.Placing));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Placing, TownVisitPhase.FadingIn));
            machine.Complete("visit-1");
            Assert.AreEqual(TownVisitPhase.Idle, machine.Phase);
            Assert.IsFalse(machine.IsTransitioning);
        }

        [Test]
        public void ConcurrentAndDefaultRequests_DoNotReplaceActiveOperation()
        {
            var machine = new TownInteriorTransitionStateMachine();
            Assert.IsFalse(machine.TryBegin(default, out _));
            Assert.IsTrue(machine.TryBegin(Request(), out _));
            Assert.IsFalse(machine.TryBegin(Request("visit-2"), out _));
            Assert.AreEqual("visit-1", machine.OperationId);
            Assert.AreEqual(TownVisitPhase.Opening, machine.Phase);
        }

        [Test]
        public void Failure_RequiresRollbackCompletion_AndRejectsLateCallbacks()
        {
            var machine = new TownInteriorTransitionStateMachine();
            Assert.IsTrue(machine.TryBegin(Request(), out _));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Opening, TownVisitPhase.Armed));
            Assert.IsFalse(machine.TryFail("stale", "late callback"));
            Assert.IsTrue(machine.TryFail("visit-1", "load failed"));
            Assert.AreEqual(TownVisitPhase.RollingBack, machine.Phase);
            Assert.AreEqual("load failed", machine.FailureReason);
            Assert.IsFalse(machine.Advance("visit-1", TownVisitPhase.Armed, TownVisitPhase.FadingOut));
            machine.Complete("visit-1");
            Assert.AreEqual(TownVisitPhase.RollingBack, machine.Phase);
            Assert.IsFalse(machine.CompleteRollback("stale"));
            Assert.IsTrue(machine.CompleteRollback("visit-1"));
            Assert.IsTrue(machine.TryBegin(Request("visit-2"), out _));
            Assert.IsFalse(machine.Advance("visit-1", TownVisitPhase.Opening, TownVisitPhase.Armed));
            Assert.AreEqual("visit-2", machine.OperationId);
        }

        [Test]
        public void Timeout_UsesExplicitFiniteTime_ThroughFadeIn()
        {
            var machine = new TownInteriorTransitionStateMachine();
            Assert.IsTrue(machine.TryBegin(Request(), out _));
            Assert.IsFalse(machine.TryTimeout("visit-1", float.NaN, 10f));
            Assert.IsFalse(machine.TryTimeout("visit-1", 10f, float.PositiveInfinity));
            Assert.IsFalse(machine.TryTimeout("visit-1", 9.99f, 10f));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Opening, TownVisitPhase.Armed));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Armed, TownVisitPhase.FadingOut));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.FadingOut, TownVisitPhase.Loading));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Loading, TownVisitPhase.Placing));
            Assert.IsTrue(machine.Advance("visit-1", TownVisitPhase.Placing, TownVisitPhase.FadingIn));
            Assert.IsTrue(machine.TryTimeout("visit-1", 10f, 10f));
            Assert.AreEqual(TownVisitPhase.RollingBack, machine.Phase);
            Assert.IsFalse(machine.TryTimeout("visit-1", 11f, 10f));
        }

        [Test]
        public void Request_NormalizesWhitespaceWithoutChangingStableId()
        {
            var request = new TownInteriorVisitRequest(" op-9 ", " House_GateKeeper ", " entry ", " return ", true);
            Assert.AreEqual("House_GateKeeper", request.HouseId);
            Assert.AreEqual("op-9", request.OperationId);
            Assert.IsTrue(request.Exiting);
            Assert.Throws<ArgumentException>(() => new TownInteriorVisitRequest(" ", "House_GateKeeper", "entry", "return", false));
        }

        [Test]
        public void LocationValidation_RejectsCorruptCoordinatesAndMissingInteriorAnchors()
        {
            Assert.IsTrue(default(TownInteriorLocationSnapshot).TryValidate(out _), "Legacy exterior default remains valid.");
            var valid = new TownInteriorLocationSnapshot("House_GateKeeper", "entry", "return", 1, 2, 0, -1, 30, -45);
            Assert.IsTrue(valid.TryValidate(out _));
            Assert.IsFalse(new TownInteriorLocationSnapshot("House_GateKeeper", "", "return", 1, 2, 0, -1, 30, -45).TryValidate(out _));
            Assert.IsFalse(new TownInteriorLocationSnapshot("House_GateKeeper", "entry", "", 1, 2, 0, -1, 30, -45).TryValidate(out _));
            foreach (float corrupt in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                for (int field = 0; field < 6; field++)
                {
                    var pose = new[] { 1f, 2f, 0f, -1f, 30f, -45f };
                    pose[field] = corrupt;
                    var invalid = new TownInteriorLocationSnapshot("House_GateKeeper", "entry", "return",
                        pose[0], pose[1], pose[2], pose[3], pose[4], pose[5]);
                    Assert.IsFalse(invalid.TryValidate(out var reason), "Corrupt field " + field);
                    Assert.IsNotEmpty(reason);
                }
            }
        }
    }
}
