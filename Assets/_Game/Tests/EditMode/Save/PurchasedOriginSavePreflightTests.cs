using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Farm;
using CindarsHope.Farm.Processing;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC.Services;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.World;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Save
{
    public sealed class PurchasedOriginSavePreflightTests
    {
        private GameObject _host;
        private SaveManager _saveManager;
        private InventoryManager _inventory;
        private ItemDatabaseSO _database;
        private ItemDataSO _wood;
        private ItemDataSO _cheese;

        [SetUp]
        public void SetUp()
        {
            _wood = ScriptableObject.CreateInstance<ItemDataSO>();
            _wood.Id = "wood";
            _wood.MaxStack = 10;
            _cheese = ScriptableObject.CreateInstance<ItemDataSO>();
            _cheese.Id = ProcessingRecipeCatalog.ItemGoatCheeseId;
            _cheese.MaxStack = 10;
            _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var registryType = typeof(DataRegistrySO<ItemDataSO>);
            registryType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_database, new[] { _wood, _cheese });
            registryType.GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_database, null);
            _host = new GameObject("origin-save-preflight");
            _inventory = _host.AddComponent<InventoryManager>();
            _inventory.Initialize(_database);
            _saveManager = _host.AddComponent<SaveManager>();
            typeof(SaveManager).GetField("_inventoryManager", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_saveManager, _inventory);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_host);
            UnityEngine.Object.DestroyImmediate(_database);
            UnityEngine.Object.DestroyImmediate(_wood);
            UnityEngine.Object.DestroyImmediate(_cheese);
        }

        [Test]
        public void V6MissingOriginMarker_KeepsKnownItemButMakesItUnsellable()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 6,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData
                {
                    Capacity = 20,
                    Slots = new List<InventorySlotSaveData>
                    { new InventorySlotSaveData { SlotIndex = 0, ItemId = "wood", Amount = 2 } }
                }
            };
            var method = typeof(SaveManager).GetMethod("ValidateAndNormalizeSave",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var args = new object[] { save, null };
            Assert.That(method.Invoke(_saveManager, args), Is.True, args[1]?.ToString());
            Assert.That(save.Inventory.OriginSchemaVersion, Is.EqualTo(1));
            _inventory.RestoreFromSaveData(save.Inventory);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(2));
            Assert.That(_inventory.GetEligibleAmount("wood"), Is.Zero);
        }

        [TestCase(5)]
        [TestCase(6)]
        public void UnknownPickup_RejectsLoadWithoutRewritingOriginalFile(int schemaVersion)
        {
            var save = new GameSaveData
            {
                SchemaVersion = schemaVersion,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20 },
                World = new WorldSaveData
                {
                    Pickups = new List<ItemPickupSaveData>
                    { new ItemPickupSaveData { PickupIndex = 10000,
                        ItemId = "missing_item", Amount = 1 } }
                }
            };
            var path = Path.Combine(Application.temporaryCachePath,
                "skills21f-preflight-" + Guid.NewGuid().ToString("N") + ".json");
            var original = JsonUtility.ToJson(save, true);
            File.WriteAllText(path, original);
            try
            {
                var method = typeof(SaveManager).GetMethod("TryReadSaveWithMigration",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var args = new object[] { path, true, null, null };
                Assert.That(method.Invoke(_saveManager, args), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                var backup = path + ".backup";
                if (File.Exists(backup)) File.Delete(backup);
            }
        }

        [TestCase(5)]
        [TestCase(6)]
        public void PickupWithWrongInstanceId_RejectsLoadWithoutRewritingOriginalFile(int schemaVersion)
        {
            var save = new GameSaveData
            {
                SchemaVersion = schemaVersion,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20 },
                World = new WorldSaveData
                {
                    Pickups = new List<ItemPickupSaveData>
                    { new ItemPickupSaveData { PickupIndex = 10000,
                        ItemId = "wood", ItemInstanceId = "other#1", Amount = 1 } }
                }
            };
            var path = Path.Combine(Application.temporaryCachePath,
                "skills21f-preflight-" + Guid.NewGuid().ToString("N") + ".json");
            var original = JsonUtility.ToJson(save, true);
            File.WriteAllText(path, original);
            try
            {
                var method = typeof(SaveManager).GetMethod("TryReadSaveWithMigration",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method.Invoke(_saveManager, new object[] { path, true, null, null }), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(path + ".backup")) File.Delete(path + ".backup");
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void V6DynamicPickupWithMissingSceneOrFlag_PreservesOriginalFile(bool hasDynamicFlag)
        {
            var save = new GameSaveData
            {
                SchemaVersion = 6,
                CurrentDay = 1,
                CurrentSceneName = "FarmScene",
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20, OriginSchemaVersion = 1 },
                World = new WorldSaveData
                {
                    Pickups = new List<ItemPickupSaveData>
                    { new ItemPickupSaveData { PickupIndex = 10000, ItemId = "wood", Amount = 1,
                        IsDynamic = hasDynamicFlag, RuntimeGeneratedPickupId = "drop-10000",
                        SceneName = hasDynamicFlag ? string.Empty : "FarmScene" } }
                }
            };
            var path = Path.Combine(Application.temporaryCachePath,
                "skills21f-preflight-" + Guid.NewGuid().ToString("N") + ".json");
            var original = JsonUtility.ToJson(save, true);
            File.WriteAllText(path, original);
            try
            {
                var method = typeof(SaveManager).GetMethod("TryReadSaveWithMigration",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method.Invoke(_saveManager, new object[] { path, true, null, null }), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ProcessingJobWithUnknownRecipeOrDuplicateStation_PreservesSaveBytes(bool duplicate)
        {
            var job = new FarmProcessingJobSaveData
            {
                StationId = ProcessingRecipeCatalog.StationCheesePressId,
                RecipeId = duplicate ? ProcessingRecipeCatalog.RecipeGoatCheeseId : "missing_recipe",
                OutputItemId = duplicate ? ProcessingRecipeCatalog.ItemGoatCheeseId : "wood",
                OutputQuantity = 1
            };
            var save = new GameSaveData
            {
                SchemaVersion = 6,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20, OriginSchemaVersion = 1 },
                Farm = new FarmSaveData()
            };
            save.Farm.Processing.Jobs.Add(job);
            if (duplicate)
                save.Farm.Processing.Jobs.Add(new FarmProcessingJobSaveData
                {
                    StationId = job.StationId,
                    RecipeId = job.RecipeId,
                    OutputItemId = job.OutputItemId,
                    OutputQuantity = job.OutputQuantity
                });
            var path = Path.Combine(Application.temporaryCachePath,
                "skills21f-preflight-" + Guid.NewGuid().ToString("N") + ".json");
            var original = JsonUtility.ToJson(save, true);
            File.WriteAllText(path, original);
            try
            {
                var method = typeof(SaveManager).GetMethod("TryReadSaveWithMigration",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method.Invoke(_saveManager, new object[] { path, true, null, null }), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void V6DuplicatePaidOrderId_RejectsLoadWithoutRewritingOriginalFile()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 6,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20, OriginSchemaVersion = 1 },
                NpcServices = new NpcServicesSaveData
                {
                    PendingBookOrders = new List<NpcBookOrderSaveData>
                    {
                        new NpcBookOrderSaveData { OrderId = 7,
                            DeliveredBookItemId = "wood", DeliveryDay = 2 },
                        new NpcBookOrderSaveData { OrderId = 7,
                            DeliveredBookItemId = "wood", DeliveryDay = 3 }
                    }
                }
            };
            var path = Path.Combine(Application.temporaryCachePath,
                "skills21f-preflight-" + Guid.NewGuid().ToString("N") + ".json");
            var original = JsonUtility.ToJson(save, true);
            File.WriteAllText(path, original);
            try
            {
                var method = typeof(SaveManager).GetMethod("TryReadSaveWithMigration",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method.Invoke(_saveManager, new object[] { path, true, null, null }), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void V6DuplicateCorpseSlot_IsRejectedBeforeAnyRestore()
        {
            var save = new GameSaveData
            {
                SchemaVersion = 6,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20, OriginSchemaVersion = 1 },
                Death = new DeathSaveData
                {
                    ActiveCorpse = new PlayerCorpseRecoverySaveData
                    {
                        OriginSchemaVersion = 1,
                        LostInventoryItems = new List<InventorySlotSaveData>
                        {
                            new InventorySlotSaveData { SlotIndex = 3, ItemId = "wood", Amount = 1 },
                            new InventorySlotSaveData { SlotIndex = 3, ItemId = "wood", Amount = 1 }
                        }
                    }
                }
            };
            var method = typeof(SaveManager).GetMethod("ValidateAndNormalizeSave",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method.Invoke(_saveManager, new object[] { save, null }), Is.False);
        }

        [Test]
        public void InvalidExistingSlot_BlocksSubsequentSaveGameAndPreservesOriginalBytes()
        {
            var root = Path.Combine(Application.temporaryCachePath, "town-interior-save-tests");
            var directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            _saveManager.ConfigureTemporarySaveDirectoryForValidation(directory);
            var save = new GameSaveData
            {
                SchemaVersion = 6,
                CurrentDay = 1,
                Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20, OriginSchemaVersion = 1 },
                World = new WorldSaveData
                {
                    Pickups = new List<ItemPickupSaveData>
                    { new ItemPickupSaveData { PickupIndex = 10000,
                        ItemId = "missing_item", Amount = 1 } }
                }
            };
            var original = JsonUtility.ToJson(save, true);
            var path = _saveManager.SaveFilePath;
            File.WriteAllText(path, original);
            try
            {
                Assert.That(_saveManager.SaveGame(), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                Directory.Delete(directory);
            }
        }

        [TestCase(5)]
        [TestCase(6)]
        public void IdleCraftingStation_JsonRoundTrip_IsAcceptedAsNoJob(int schemaVersion)
        {
            var save = CraftingSave(schemaVersion, null);
            var path = Path.Combine(Application.temporaryCachePath, "craft-idle-" + Guid.NewGuid().ToString("N") + ".json");
            var original = JsonUtility.ToJson(save);
            File.WriteAllText(path, original);
            try
            {
                var args = new object[] { path, false, null, null };
                var read = typeof(SaveManager).GetMethod("TryReadSaveWithMigration",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(read.Invoke(_saveManager, args), Is.True, args[3]?.ToString());
                Assert.That(((GameSaveData)args[2]).Crafting.Stations[0].Job, Is.Null);
                Assert.That(File.ReadAllText(path), Is.EqualTo(original));
            }
            finally { File.Delete(path); }
        }

        [TestCase("missing_item", 1, "recipe_wood")]
        [TestCase("wood", 0, "recipe_wood")]
        [TestCase("wood", 1, "")]
        public void InvalidCraftingJob_IsRejectedWithoutMutatingLiveInventory(
            string outputId, int amount, string recipeId)
        {
            _inventory.AddItem("wood", 3);
            var save = CraftingSave(6, new CraftingJobSaveData
            {
                JobId = "job-1", RecipeId = recipeId, OutputItemId = outputId,
                OutputAmount = amount, StationInstanceId = "workbench"
            });
            var parsed = JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(save));
            var method = typeof(SaveManager).GetMethod("ValidateAndNormalizeSave",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method.Invoke(_saveManager, new object[] { parsed, null }), Is.False);
            Assert.That(_inventory.GetAmount("wood"), Is.EqualTo(3));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void RealCraftingJob_JsonRoundTrip_PreservesWork(int status)
        {
            var save = CraftingSave(6, new CraftingJobSaveData
            {
                JobId = "job-1", RecipeId = "recipe_wood", OutputItemId = "wood",
                OutputAmount = 2, StationInstanceId = "workbench", Status = status,
                RemainingSeconds = status == 0 ? 2.5f : 0f,
                IngredientsConsumed = new List<CraftingIngredientSaveData>
                { new CraftingIngredientSaveData { ItemId = "wood", Amount = 1 } }
            });
            var parsed = JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(save));
            var args = new object[] { parsed, null };
            var method = typeof(SaveManager).GetMethod("ValidateAndNormalizeSave",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method.Invoke(_saveManager, args), Is.True, args[1]?.ToString());
            var job = parsed.Crafting.Stations[0].Job;
            Assert.That(job.OutputAmount, Is.EqualTo(2));
            Assert.That(job.Status, Is.EqualTo(status));
            Assert.That(job.RemainingSeconds, Is.EqualTo(status == 0 ? 2.5f : 0f));
            Assert.That(job.IngredientsConsumed[0].Amount, Is.EqualTo(1));
        }

        private static GameSaveData CraftingSave(int schemaVersion, CraftingJobSaveData job)
        {
            return new GameSaveData
            {
                SchemaVersion = schemaVersion, CurrentDay = 1, Player = new PlayerSaveData(),
                Inventory = new InventorySaveData { Capacity = 20, OriginSchemaVersion = 1 },
                Crafting = new CraftingRuntimeSaveData
                {
                    Stations = new List<CraftingStationSaveData>
                    { new CraftingStationSaveData { StationInstanceId = "workbench", StationLevel = 1, Job = job } }
                }
            };
        }
    }
}
