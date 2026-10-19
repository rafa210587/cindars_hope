using System.Collections.Generic;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Farm;
using CindarsHope.Farm.Processing;
using CindarsHope.Farm.Shipping;
using CindarsHope.Inventory;
using CindarsHope.NPC.Services;
using CindarsHope.Save;
using CindarsHope.Save.Migrations;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Save
{
    public sealed class InventoryOriginV5ToV6MigrationTests
    {
        private static bool Migrate(string json, out GameSaveData migrated, out string reason)
        {
            var migration = new InventoryOriginV5ToV6Migration();
            var success = migration.TryMigrate(new SaveMigrationContext("save.json", "save.bak",
                5, 6, json), out var output, out reason);
            migrated = success ? JsonUtility.FromJson<GameSaveData>(output) : null;
            return success;
        }

        [Test]
        public void GenuineV5DropWithoutNewFields_BecomesDynamicAndSurvivesWorldRestore()
        {
            const string raw = "{\"SchemaVersion\":5,\"CurrentSceneName\":\"CaveScene\","
                + "\"Inventory\":{\"Capacity\":40,\"Slots\":[]},"
                + "\"World\":{\"Pickups\":[{\"PickupIndex\":10000,"
                + "\"ItemId\":\"wood\",\"Amount\":2,\"IsCollected\":false}]}}";
            Assert.IsTrue(Migrate(raw, out var migrated, out var reason), reason);
            var pickup = migrated.World.Pickups[0];
            Assert.AreEqual(6, migrated.SchemaVersion);
            Assert.IsTrue(pickup.IsDynamic);
            Assert.AreEqual("CaveScene", pickup.SceneName);
            Assert.AreEqual("legacy-drop-10000", pickup.RuntimeGeneratedPickupId);
            Assert.AreEqual(1, pickup.OriginSchemaVersion);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(pickup.OriginLots, 2)[0].Kind);
        }

        [Test]
        public void V5DomainsGainFiniteLegacyLotsWithoutInventingPaidGold()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 5,
                Inventory = new InventorySaveData
                {
                    Capacity = 40,
                    Slots = new List<InventorySlotSaveData>
                    { new InventorySlotSaveData { SlotIndex = 0, ItemId = "wood", Amount = 3 } }
                },
                Farm = new FarmSaveData(),
                Death = new DeathSaveData
                {
                    ActiveCorpse = new PlayerCorpseRecoverySaveData
                    {
                        LostInventoryItems = new List<InventorySlotSaveData>
                        { new InventorySlotSaveData { SlotIndex = 1, ItemId = "bread", Amount = 1 } }
                    }
                },
                Crafting = new CraftingRuntimeSaveData
                {
                    Stations = new List<CraftingStationSaveData>
                    { new CraftingStationSaveData { StationInstanceId = "station_craft_01",
                        Job = new CraftingJobSaveData
                        {
                            RecipeId = "recipe_bread", OutputItemId = "bread", OutputAmount = 1,
                            IngredientsConsumed = new List<CraftingIngredientSaveData>
                            { new CraftingIngredientSaveData { ItemId = "flour", Amount = 2 } }
                        } } }
                },
                Equipment = new CindarsHope.Foundation.EquipmentSaveData
                {
                    Infusions = new List<CindarsHope.Foundation.WeaponInfusionSaveData>
                    { new CindarsHope.Foundation.WeaponInfusionSaveData
                        { ItemInstanceId = "sword#1", InfusionTier = 1, InfusionElement = "fire" } }
                },
                NpcServices = new NpcServicesSaveData
                {
                    PendingBookOrders = new List<NpcBookOrderSaveData>
                    { new NpcBookOrderSaveData { DeliveredBookItemId = "scroll", DeliveryDay = 4 } }
                }
            };
            save.Farm.PendingShipping.Entries.Add(new PendingShippingEntrySaveData
                { EntryId = "shipping-legacy-01", ItemId = "wood", Quantity = 2 });
            save.Farm.Processing.Jobs.Add(new FarmProcessingJobSaveData
            {
                StationId = ProcessingRecipeCatalog.StationCheesePressId,
                RecipeId = ProcessingRecipeCatalog.RecipeGoatCheeseId,
                OutputItemId = ProcessingRecipeCatalog.ItemGoatCheeseId,
                OutputQuantity = 1
            });

            Assert.IsTrue(Migrate(JsonUtility.ToJson(save), out var migrated, out var reason), reason);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.Inventory.Slots[0].OriginLots, 3)[0].Kind);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.Farm.PendingShipping.Entries[0].OriginLots, 2)[0].Kind);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.Death.ActiveCorpse.LostInventoryItems[0].OriginLots, 1)[0].Kind);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.Crafting.Stations[0].Job.OutputOriginLots, 1)[0].Kind);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.Farm.Processing.Jobs[0].OutputOriginLots, 1)[0].Kind);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.Equipment.Infusions[0].ConsumedEssenceLots, 3)[0].Kind);
            Assert.AreEqual(ItemOriginKind.LegacyUntracked,
                ItemOriginLot.FromSaveData(migrated.NpcServices.PendingBookOrders[0].OriginLots, 1)[0].Kind);
            Assert.AreEqual(0, migrated.NpcServices.PendingBookOrders[0].PaidGold);
        }

        [Test]
        public void V6HeaderAndImpossibleV5SlotAreRejectedWithoutMigratedJson()
        {
            Assert.IsFalse(Migrate("{\"SchemaVersion\":6}", out _, out _));
            const string broken = "{\"SchemaVersion\":5,\"Inventory\":{\"Slots\":["
                + "{\"SlotIndex\":-1,\"ItemId\":\"wood\",\"Amount\":1}]}}";
            Assert.IsFalse(Migrate(broken, out _, out _));
        }

        [Test]
        public void DuplicateProcessingStationRejectsMigrationInsteadOfLosingConsumedItems()
        {
            var save = new GameSaveData { SchemaVersion = 5, Farm = new FarmSaveData() };
            for (var i = 0; i < 2; i++)
                save.Farm.Processing.Jobs.Add(new FarmProcessingJobSaveData
                {
                    StationId = ProcessingRecipeCatalog.StationCheesePressId,
                    RecipeId = ProcessingRecipeCatalog.RecipeGoatCheeseId,
                    OutputItemId = ProcessingRecipeCatalog.ItemGoatCheeseId,
                    OutputQuantity = 1
                });
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
        }

        [Test]
        public void LegacyPaidOrdersReceiveStableUniqueIdsAndPositiveDuplicatesFail()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 5,
                NpcServices = new NpcServicesSaveData
                {
                    PendingBookOrders = new List<NpcBookOrderSaveData>
                    {
                        new NpcBookOrderSaveData { OrderId = 0, DeliveredBookItemId = "scroll_a" },
                        new NpcBookOrderSaveData { OrderId = 7, DeliveredBookItemId = "scroll_b" },
                        new NpcBookOrderSaveData { OrderId = -1, DeliveredBookItemId = "scroll_c" }
                    }
                }
            };
            Assert.IsTrue(Migrate(JsonUtility.ToJson(save), out var migrated,
                out var reason), reason);
            Assert.AreEqual(1, migrated.NpcServices.PendingBookOrders[0].OrderId);
            Assert.AreEqual(7, migrated.NpcServices.PendingBookOrders[1].OrderId);
            Assert.AreEqual(2, migrated.NpcServices.PendingBookOrders[2].OrderId);

            save.NpcServices.PendingBookOrders[0].OrderId = 7;
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
        }

        [Test]
        public void MissingDuplicateAndSyntheticShippingIdsRejectMigration()
        {
            var save = new GameSaveData { SchemaVersion = 5, Farm = new FarmSaveData() };
            save.Farm.PendingShipping.Entries.Add(new PendingShippingEntrySaveData
                { ItemId = "wood", Quantity = 1 });
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));

            save.Farm.PendingShipping.Entries[0].EntryId = "ship-1";
            save.Farm.PendingShipping.Entries.Add(new PendingShippingEntrySaveData
                { EntryId = "ship-1", ItemId = "wood", Quantity = 1 });
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));

            save.Farm.PendingShipping.Entries[0].Quantity = 2;
            save.Farm.PendingShipping.Entries[0].OriginSchemaVersion = 1;
            save.Farm.PendingShipping.Entries[0].OriginLots = new List<CindarsHope.Foundation.ItemOriginLotSaveData>
            {
                new CindarsHope.Foundation.ItemOriginLotSaveData
                    { Kind = (int)ItemOriginKind.Earned, Quantity = 1 },
                new CindarsHope.Foundation.ItemOriginLotSaveData
                    { Kind = (int)ItemOriginKind.Invalid, Quantity = 1 }
            };
            save.Farm.PendingShipping.Entries[1].EntryId = "ship-1_invalid";
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
        }

        [Test]
        public void MissingOrDuplicateCraftingStationIdRejectsMigration()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 5,
                Crafting = new CraftingRuntimeSaveData()
            };
            save.Crafting.Stations.Add(new CraftingStationSaveData());
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
            save.Crafting.Stations[0].StationInstanceId = "forge-01";
            save.Crafting.Stations.Add(new CraftingStationSaveData
                { StationInstanceId = "forge-01" });
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
        }

        [Test]
        public void UnknownHistoricalInfusionElementRejectsMigration()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 5,
                Equipment = new CindarsHope.Foundation.EquipmentSaveData
                {
                    Infusions = new List<CindarsHope.Foundation.WeaponInfusionSaveData>
                    { new CindarsHope.Foundation.WeaponInfusionSaveData
                        { ItemInstanceId = "sword#1", InfusionTier = 1,
                            InfusionElement = "unknown" } }
                }
            };
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
            save.Equipment.Infusions[0].InfusionElement = "fire";
            save.Equipment.Infusions.Add(new CindarsHope.Foundation.WeaponInfusionSaveData
                { ItemInstanceId = "sword#1", InfusionTier = 2,
                    InfusionElement = "ice" });
            Assert.IsFalse(Migrate(JsonUtility.ToJson(save), out _, out _));
        }
    }
}
