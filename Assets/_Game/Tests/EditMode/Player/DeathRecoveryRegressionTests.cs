using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CindarsHope.Cave.Death;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.UI.Death;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    public sealed class DeathRecoveryRegressionTests
    {
        private GameObject _host, _avatar, _anchor, _ui, _eventSystem;
        private ItemDatabaseSO _database;
        private ItemDataSO _tear;
        private PlayerManager _player;
        private InventoryManager _inventory;
        private CorpseRecoveryManager _recovery;
        private DeathRevivalDecision _decision;

        [SetUp]
        public void SetUp()
        {
            _tear = ScriptableObject.CreateInstance<ItemDataSO>();
            _tear.Id = DeathRevivalDecision.GoddessTearItemId;
            _tear.MaxStack = 99;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registry = typeof(DataRegistrySO<ItemDataSO>);
            registry.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_database, new[] { _tear });
            registry.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_database, null);
            _host = new GameObject("death-resource-owners");
            _player = _host.AddComponent<PlayerManager>();
            _player.RestoreState(100, 0, 0);
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
            var equipment = _host.AddComponent<EquipmentManager>();
            _recovery = new CorpseRecoveryManager(_player, _inventory, equipment);
            _decision = new DeathRevivalDecision(_player, _recovery,
                () => _inventory.GetAmount(DeathRevivalDecision.GoddessTearItemId),
                () => _inventory.RemoveItem(DeathRevivalDecision.GoddessTearItemId, 1));
        }

        [TearDown]
        public void TearDown()
        {
            if (_ui != null) InvokeLifecycle(_ui.GetComponent<DeathScreenCanvasController>(), "OnDisable");
            if (_avatar != null) InvokeLifecycle(_avatar.GetComponent<PlayerController>(), "OnDisable");
            if (_host != null) InvokeLifecycle(_host.GetComponent<DeathSystemBootstrap>(), "OnDisable");
            if (_eventSystem != null) InvokeLifecycle(_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>(), "OnDisable");
            if (_ui != null) Object.DestroyImmediate(_ui);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
            if (_avatar != null) Object.DestroyImmediate(_avatar);
            if (_anchor != null) Object.DestroyImmediate(_anchor);
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_database);
            Object.DestroyImmediate(_tear);
        }

        [TestCase(false)] [TestCase(true)]
        public void DeathEvent_ProjectsPostTransferTearsRegardlessOfUiSubscriptionOrder(bool uiFirst)
        {
            _eventSystem = new GameObject("test-death-event-system", typeof(UnityEngine.EventSystems.EventSystem));
            InvokeLifecycle(_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>(), "OnEnable");
            _ui = new GameObject("test-death-canvas");
            if (uiFirst) AddUi();
            var owner = _host.AddComponent<DeathSystemBootstrap>();
            InvokeLifecycle(owner, "OnEnable");
            var cave = _host.AddComponent<CaveRunManager>();
            InvokeLifecycle(cave, "Awake");
            cave.RestoreRuntimeState(new CaveRuntimeState
            {
                HasActiveRun = true, CaveWorldSeed = "world", CaveRunSeed = "run",
                CaveRunId = "run-id", CurrentCaveLevel = 1, DeepestLayerReached = 1
            });
            SetOwner("_resolver", new CaveDeathResolver(new CaveDeathPolicy(), cave, _player,
                _inventory, _host.GetComponent<EquipmentManager>(), null, null, _recovery));
            SetOwner("_recoveryManager", _recovery);
            SetOwner("_playerManager", _player);
            SetOwner("_revival", _decision);
            SetOwner("_initialized", true);
            if (!uiFirst) AddUi();
            Assert.That(_inventory.AddItem(DeathRevivalDecision.GoddessTearItemId, 2), Is.True);
            GameEventBus.Publish(new PlayerDiedEvent { SceneName = "CaveScene" });
            Assert.That(_inventory.GetAmount(DeathRevivalDecision.GoddessTearItemId), Is.Zero);
            var view = (DeathScreenViewModel)typeof(DeathScreenCanvasController)
                .GetField("_viewModel", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_ui.GetComponent<DeathScreenCanvasController>());
            Assert.That(view.GoddessTearCount, Is.EqualTo(2));
            Assert.That(view.CanReviveWithTear, Is.True);
            GameEventBus.Publish(new DeathReviveRequestedEvent());
            Assert.That(_player.CurrentHP, Is.EqualTo(100));
            Assert.That(_recovery.GetRevivalTearCount(_recovery.ActiveCorpse.CorpseId), Is.EqualTo(1));
            Assert.That((bool)typeof(DeathScreenCanvasController)
                .GetField("_isShowing", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_ui.GetComponent<DeathScreenCanvasController>()), Is.False);

            void SetOwner(string field, object value) => typeof(DeathSystemBootstrap)
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
            void AddUi() => InvokeLifecycle(_ui.AddComponent<DeathScreenCanvasController>(), "OnEnable");
        }

        [Test]
        public void FountainTransition_UsesExactlyOneSerializedFarmSpawn()
        {
            var spawnId = (string)typeof(AnyaFountainRespawnFlow)
                .GetField("FountainSpawnAnchorId", BindingFlags.Static | BindingFlags.NonPublic).GetRawConstantValue();
            var scene = File.ReadAllText("Assets/_Game/Scenes/FarmScene.unity");
            Assert.That(Regex.Matches(scene, @"(?m)^\s*_spawnId: " + Regex.Escape(spawnId) + @"\s*$").Count,
                Is.EqualTo(1), "Respawn transition must use an actual, unique serialized Farm anchor.");
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void CaveRevive_ConsumesExactlyOneAndRejectsRepeatedChoice(int tears)
        {
            var corpse = Body("death-1", tears);
            _recovery.SetActiveCorpse(corpse);
            _decision.Begin(corpse.CorpseId);
            Assert.That(_decision.EligibleTearCount, Is.EqualTo(tears));
            Assert.That(_decision.TryRevive(), Is.EqualTo(tears > 0));
            Assert.That(_decision.TryRevive(), Is.False);
            Assert.That(_recovery.GetRevivalTearCount(corpse.CorpseId), Is.EqualTo(System.Math.Max(0, tears - 1)));
            Assert.That(_player.CurrentHP, Is.EqualTo(tears > 0 ? 100 : 0));
            Assert.That(corpse.GoldAmount, Is.EqualTo(17));
        }

        [Test]
        public void RevivedBody_RoundTripsRemainingProvenanceAndRecoversOnlyOnce()
        {
            _recovery.SetActiveCorpse(Body("death-1", 2));
            _decision.Begin("death-1");
            Assert.That(_decision.TryRevive(), Is.True);
            var provider = new DeathSectionProvider(_recovery);
            var saved = JsonUtility.FromJson<DeathSaveData>(JsonUtility.ToJson(provider.Capture(new GameSaveData())));
            provider.Restore(saved);
            var item = _recovery.ActiveCorpse.InventoryItems[0];
            Assert.That(item.Amount, Is.EqualTo(1));
            Assert.That(item.OriginLots[0].Quantity, Is.EqualTo(1));
            Assert.That(item.OriginLots[0].UnitPaidBasisGold, Is.EqualTo(25));
            _recovery.RecoverCorpse();
            Assert.That(_inventory.GetAmount(DeathRevivalDecision.GoddessTearItemId), Is.EqualTo(1));
            Assert.That(_player.CurrentGold, Is.EqualTo(17));
            Assert.That(_recovery.HasActiveCorpse, Is.False);
            // A repeated decision cannot recover or debit the consumed unit.
            Assert.That(_decision.TryRevive(), Is.False);
            Assert.That(_inventory.GetAmount(DeathRevivalDecision.GoddessTearItemId), Is.EqualTo(1));
        }

        [Test]
        public void OverworldDeath_DoesNotConsumePreviousCaveBody()
        {
            _recovery.SetActiveCorpse(Body("previous-death", 2));
            _decision.Begin(null);
            Assert.That(_decision.EligibleTearCount, Is.Zero);
            Assert.That(_decision.TryRevive(), Is.False);
            Assert.That(_recovery.GetRevivalTearCount("previous-death"), Is.EqualTo(2));
            Assert.That(_inventory.AddItem(DeathRevivalDecision.GoddessTearItemId, 1), Is.True);
            Assert.That(_decision.TryRevive(), Is.True);
            Assert.That(_inventory.GetAmount(DeathRevivalDecision.GoddessTearItemId), Is.Zero);
        }

        [Test]
        public void SecondDeath_UsesNewBodyAndRejectsReplacedBody()
        {
            _recovery.SetActiveCorpse(Body("first", 2));
            _decision.Begin("first");
            Assert.That(_decision.TryRevive(), Is.True);
            _player.SetHP(0);
            _recovery.SetActiveCorpse(Body("second", 1));
            _decision.Begin("first");
            Assert.That(_decision.TryRevive(), Is.False);
            _decision.Begin("second");
            Assert.That(_decision.TryRevive(), Is.True);
            Assert.That(_recovery.GetRevivalTearCount("second"), Is.Zero);
        }

        [Test]
        public void InvalidProvenance_DoesNotGrantHealthOrConsume()
        {
            var corpse = Body("invalid", 2);
            corpse.InventoryItems[0].OriginLots[0].Quantity = 1;
            _recovery.SetActiveCorpse(corpse);
            _decision.Begin("invalid");
            Assert.That(_decision.TryRevive(), Is.False);
            Assert.That(_player.CurrentHP, Is.Zero);
            Assert.That(corpse.InventoryItems[0].Amount, Is.EqualTo(2));
        }

        [Test]
        public void FountainRespawn_MovesAvatarNotPersistentResourceOwner()
        {
            _host.transform.position = new Vector3(100, 100);
            _avatar = new GameObject("scene-avatar");
            var body = _avatar.AddComponent<Rigidbody2D>();
            body.gravityScale = 0;
            InvokeLifecycle(_avatar.AddComponent<PlayerController>(), "OnEnable");
            _anchor = new GameObject("fountain-anchor");
            _anchor.transform.position = new Vector3(-28, 1.7f);
            new AnyaRespawnService(_player, null, null, _anchor.transform).RespawnAtAnyaFountain();
            Assert.That((Vector2)_avatar.transform.position, Is.EqualTo((Vector2)_anchor.transform.position));
            Assert.That(body.position, Is.EqualTo((Vector2)_anchor.transform.position));
            Assert.That(_host.transform.position, Is.EqualTo(new Vector3(100, 100)));
            Assert.That(_player.CurrentHP, Is.EqualTo(100));
        }

        private static Corpse Body(string id, int tears)
        {
            var body = new Corpse(id) { Status = CorpseStatus.Active, RunId = "run", CaveSeed = "run",
                CaveLevel = 1, SceneName = "CaveScene", GoldAmount = 17 };
            if (tears > 0)
                body.InventoryItems.Add(new CorpseItem(DeathRevivalDecision.GoddessTearItemId, tears)
                {
                    OriginSchemaVersion = 1,
                    OriginLots = new List<ItemOriginLotSaveData> { new ItemOriginLotSaveData {
                        Kind = (int)ItemOriginKind.Purchased, Quantity = tears,
                        UnitPaidBasisGold = 25, UnitResaleCapGold = 24 } }
                });
            return body;
        }

        private static void InvokeLifecycle(object component, string method)
        {
            if (component == null) return;
            component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(component, null);
        }
    }
}
