using System;
using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.EditMode.UI
{
    public sealed class InventoryItemActionTests
    {
        private GameObject _root;
        private InventoryManager _inventory;
        private InventoryPanelController _panel;
        private ItemUseManager _use;
        private ItemDatabaseSO _database;
        private ItemDataSO _item;
        private UseHandlerFixture _handler;
        private object _previousUseInstance;
        private object _previousPanelInstance;

        [SetUp]
        public void SetUp()
        {
            _previousUseInstance = StaticField(typeof(ItemUseManager), "_instance").GetValue(null);
            _previousPanelInstance = StaticField(typeof(InventoryPanelController), "_instance").GetValue(null);
            StaticField(typeof(ItemUseManager), "_instance").SetValue(null, null);
            StaticField(typeof(InventoryPanelController), "_instance").SetValue(null, null);
            _root = new GameObject("inventory-action-fixture");
            var child = new GameObject("inventory-action-components");
            child.transform.SetParent(_root.transform);
            _inventory = child.AddComponent<InventoryManager>();
            _use = child.AddComponent<ItemUseManager>();
            _panel = child.AddComponent<InventoryPanelController>();
            StaticField(typeof(ItemUseManager), "_instance").SetValue(null, _use);

            _item = ScriptableObject.CreateInstance<ItemDataSO>();
            _item.Id = "fixture_consumable";
            _item.Category = ItemCategory.Consumable;
            _item.MaxStack = 10;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            typeof(DataRegistrySO<ItemDataSO>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _item });
            typeof(DataRegistrySO<ItemDataSO>).GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _inventory.Initialize(_database);
            _inventory.AddItem(_item.Id, 1);
            _use.Initialize(_inventory);
            _handler = ScriptableObject.CreateInstance<UseHandlerFixture>();
            _use.RegisterHandler(_item.Id, _handler);
            PanelField("_inventoryManager").SetValue(_panel, _inventory);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_root);
            UnityEngine.Object.DestroyImmediate(_handler);
            UnityEngine.Object.DestroyImmediate(_database);
            UnityEngine.Object.DestroyImmediate(_item);
            StaticField(typeof(ItemUseManager), "_instance").SetValue(null, _previousUseInstance);
            StaticField(typeof(InventoryPanelController), "_instance").SetValue(null, _previousPanelInstance);
        }

        [Test]
        public void UseLastUnit_PreservesIdInFeedback_AndPublishesInventoryBeforeUsed()
        {
            var events = new List<string>();
            Action<InventoryChangedEvent> changed = change => events.Add("inventory:" + change.NewAmount + ":" + _inventory.GetAmount(_item.Id));
            Action<ItemUsedEvent> used = change => events.Add("used:" + change.ItemId + ":" + change.Amount);
            GameEventBus.Subscribe(changed);
            GameEventBus.Subscribe(used);
            try
            {
                Assert.That(InvokePanel("CanUseSelectedItem"), Is.True);
                InvokePanel("ExecuteUse");
                Assert.That(_handler.LastItemId, Is.EqualTo(_item.Id));
                Assert.That(_handler.Attempts, Is.EqualTo(1));
                Assert.That(_inventory.GetAmount(_item.Id), Is.Zero);
                Assert.That(events, Is.EqualTo(new[] { "inventory:0:0", "used:fixture_consumable:1" }));
                Assert.That(PanelField("_message").GetValue(_panel), Is.EqualTo("Used fixture_consumable."));
            }
            finally
            {
                GameEventBus.Unsubscribe(changed);
                GameEventBus.Unsubscribe(used);
            }
        }

        [Test]
        public void HandlerRefusal_PreservesQuantityAndDoesNotPublishUsed()
        {
            _handler.AcceptUse = false;
            int usedCount = 0;
            Action<ItemUsedEvent> onUsed = _ => usedCount++;
            GameEventBus.Subscribe(onUsed);
            try
            {
                LogAssert.Expect(LogType.Warning, "ItemUseManager: Handler failed for item 'fixture_consumable'.");
                InvokePanel("ExecuteUse");
                Assert.That(_handler.Attempts, Is.EqualTo(1));
                Assert.That(_inventory.GetAmount(_item.Id), Is.EqualTo(1));
                Assert.That(usedCount, Is.Zero);
                Assert.That(PanelField("_message").GetValue(_panel), Is.EqualTo("Failed to use fixture_consumable."));
            }
            finally { GameEventBus.Unsubscribe(onUsed); }
        }

        [TestCase(false, true, "Slot is empty.")]
        [TestCase(true, false, "Item use system not available.")]
        [TestCase(false, false, "Slot is empty.")]
        public void MissingSelectionOrService_DisablesUseAndDoesNotInvokeHandler(bool selected, bool service, string message)
        {
            if (!selected) PanelField("_selectedSlotIndex").SetValue(_panel, 1);
            if (!service) StaticField(typeof(ItemUseManager), "_instance").SetValue(null, null);
            Assert.That(InvokePanel("CanUseSelectedItem"), Is.False);
            InvokePanel("ExecuteUse");
            Assert.That(_handler.Attempts, Is.Zero);
            Assert.That(_inventory.GetAmount(_item.Id), Is.EqualTo(1));
            Assert.That(PanelField("_message").GetValue(_panel), Is.EqualTo(message));
        }

        [Test]
        public void UnusableItem_DisablesUseAndPreservesContents()
        {
            _handler.CanUse = false;
            Assert.That(InvokePanel("CanUseSelectedItem"), Is.False);
            InvokePanel("ExecuteUse");
            Assert.That(_handler.Attempts, Is.Zero);
            Assert.That(_inventory.GetAmount(_item.Id), Is.EqualTo(1));
            Assert.That(PanelField("_message").GetValue(_panel), Is.EqualTo("This item cannot be used."));
        }

        [Test]
        public void DropWithoutSpawner_PreservesStackAndReportsFailure()
        {
            var field = StaticField(typeof(CindarsHope.World.ItemDropSpawner), "_instance");
            var previous = field.GetValue(null);
            field.SetValue(null, null);
            try
            {
                LogAssert.Expect(LogType.Warning, "InventoryManager: ItemDropSpawner not available.");
                InvokePanel("ExecuteDrop");
                Assert.That(_inventory.GetAmount(_item.Id), Is.EqualTo(1));
                Assert.That(PanelField("_message").GetValue(_panel), Is.EqualTo("Failed to drop item."));
            }
            finally { field.SetValue(null, previous); }
        }

        private object InvokePanel(string method) => typeof(InventoryPanelController)
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_panel, null);
        private static FieldInfo PanelField(string name) => typeof(InventoryPanelController)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo StaticField(Type type, string name) => type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);

        public sealed class UseHandlerFixture : ItemUseHandler
        {
            public bool AcceptUse = true;
            public bool CanUse = true;
            public int Attempts;
            public string LastItemId;
            public override bool CanUseItem(string itemId, int amount) => CanUse && amount > 0;
            public override bool TryUseItem(string itemId, int amount, GameObject user)
            {
                Attempts++;
                LastItemId = itemId;
                return AcceptUse;
            }
        }
    }
}
