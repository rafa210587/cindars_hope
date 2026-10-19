using System.Collections.Generic;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Save.Providers;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Economy
{
    public sealed class PurchasedOriginEquipmentSaveTests
    {
        private GameObject _host;
        private EquipmentManager _equipment;
        private EquipmentSectionProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("origin-equipment-save");
            _equipment = _host.AddComponent<EquipmentManager>();
            _equipment.RestoreFromSaveData(new EquipmentSaveData());
            _provider = new EquipmentSectionProvider(_equipment);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_host);

        [Test]
        public void CanonicalEquipmentSave_RoundTripsConsumedPurchasedEssence()
        {
            _equipment.InfusionRegistry.SetWithEssenceLots("sword#1",
                new WeaponInfusion(TemperingElement.Fire, 1),
                new[] { new ItemOriginLot(ItemOriginKind.Purchased, 3, 7, 6) });
            var saved = (EquipmentSaveData)_provider.Capture(null);
            Assert.That(saved.Infusions.Count, Is.EqualTo(1));
            Assert.That(saved.Infusions[0].ConsumedEssenceOriginSchemaVersion, Is.EqualTo(1));

            var json = JsonUtility.ToJson(saved);
            _provider.Restore(JsonUtility.FromJson<EquipmentSaveData>(json), 6);

            var lots = _equipment.InfusionRegistry.GetConsumedEssenceLots("sword#1");
            Assert.That(lots.Count, Is.EqualTo(1));
            Assert.That(lots[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
            Assert.That(lots[0].Quantity, Is.EqualTo(3));
            Assert.That(lots[0].UnitPaidBasisGold, Is.EqualTo(7));
        }

        [Test]
        public void V5AndV6MissingOrigin_DivergeWithoutErasingInfusion()
        {
            var saved = new EquipmentSaveData
            {
                Infusions = new List<WeaponInfusionSaveData>
                {
                    new WeaponInfusionSaveData
                    { ItemInstanceId = "sword#1", InfusionElement = "fire", InfusionTier = 1 }
                }
            };

            _provider.Restore(saved, 5);
            Assert.That(_equipment.InfusionRegistry.Get("sword#1").IsActive, Is.True);
            Assert.That(_equipment.InfusionRegistry.GetConsumedEssenceLots("sword#1")[0].Kind,
                Is.EqualTo(ItemOriginKind.LegacyUntracked));

            _provider.Restore(saved, 6);
            Assert.That(_equipment.InfusionRegistry.Get("sword#1").IsActive, Is.True);
            Assert.That(_equipment.InfusionRegistry.GetConsumedEssenceLots("sword#1")[0].Kind,
                Is.EqualTo(ItemOriginKind.Invalid));
        }
    }
}
