using System.Collections.Generic;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    public sealed class InventorySlotsV1ToV2Migration : ISaveMigration
    {
        private const int LegacyFallbackMaxStack = 99;

        public string MigrationId => "save_v1_to_v2_inventory_slots";
        public int SourceSchemaVersion => 1;
        public int TargetSchemaVersion => 2;

        public bool TryMigrate(SaveMigrationContext context, out string migratedJson, out string errorMessage)
        {
            migratedJson = string.Empty;
            errorMessage = string.Empty;

            if (context == null || string.IsNullOrWhiteSpace(context.RawJson))
            {
                errorMessage = "Migration context or raw JSON is empty.";
                return false;
            }

            GameSaveData saveData;
            try
            {
                saveData = JsonUtility.FromJson<GameSaveData>(context.RawJson);
            }
            catch (System.Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }

            if (saveData == null)
            {
                errorMessage = "Unity JsonUtility returned null save data.";
                return false;
            }

            saveData.SchemaVersion = TargetSchemaVersion;
            saveData.Inventory ??= new InventorySaveData();
            saveData.Inventory.Items ??= new List<InventoryItemSaveData>();
            saveData.Inventory.Slots ??= new List<InventorySlotSaveData>();

            if (saveData.Inventory.Capacity <= 0)
            {
                saveData.Inventory.Capacity = InventoryManager.DefaultCapacity;
            }

            if (saveData.Inventory.Slots.Count == 0 && saveData.Inventory.Items.Count > 0)
            {
                var slotIndex = 0;
                foreach (var item in saveData.Inventory.Items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Amount <= 0)
                    {
                        continue;
                    }

                    var remaining = item.Amount;
                    while (remaining > 0)
                    {
                        var amount = Mathf.Min(remaining, LegacyFallbackMaxStack);
                        saveData.Inventory.Slots.Add(new InventorySlotSaveData
                        {
                            SlotIndex = slotIndex,
                            ItemId = item.ItemId,
                            Amount = amount,
                            IsEquipped = false,
                            EquipmentBindingId = string.Empty
                        });

                        slotIndex++;
                        remaining -= amount;
                    }
                }

                saveData.Inventory.Capacity = Mathf.Clamp(
                    Mathf.Max(saveData.Inventory.Capacity, saveData.Inventory.Slots.Count),
                    InventoryManager.DefaultCapacity,
                    InventoryManager.MaxCapacity);
            }

            migratedJson = JsonUtility.ToJson(saveData, true);
            return true;
        }
    }
}
