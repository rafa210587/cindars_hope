#if UNITY_EDITOR
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory.Data;
using CindarsHope.Loot;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public class ValidateSpec10Equipment
    {
        [MenuItem("CindarsHope/Advanced/Legacy/Validation/SPEC 10 - Equipment Durability Loot")]
        public static void ValidateSpec10()
        {
            Debug.Log("=== SPEC 10 Validation: Equipment Durability Loot ===");

            var passed = true;
            passed &= ValidateEquipmentManagerExists();
            passed &= ValidateDurabilityTrackerExists();
            passed &= ValidateEquipmentDataSOExists();
            passed &= ValidateRepairKitItemsExist();
            passed &= ValidateEventsExist();
            passed &= ValidateLootTableIntegration();
            passed &= ValidateSaveDataStructures();

            if (passed)
            {
                Debug.Log("✓ SPEC 10 Validation: ALL CHECKS PASSED");
            }
            else
            {
                Debug.LogError("✗ SPEC 10 Validation: SOME CHECKS FAILED");
            }
        }

        private static bool ValidateEquipmentManagerExists()
        {
            var script = Resources.Load<MonoScript>("Scripts/Equipment/EquipmentManager") ?? AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Equipment/EquipmentManager.cs");
            if (script != null)
            {
                Debug.Log("✓ EquipmentManager.cs exists");
                return true;
            }

            Debug.LogError("✗ EquipmentManager.cs not found");
            return false;
        }

        private static bool ValidateDurabilityTrackerExists()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Equipment/EquipmentDurabilityTracker.cs");
            if (script != null && script.text.Contains("DurabilityChangedEvent") && script.text.Contains("ItemBrokenEvent"))
            {
                Debug.Log("✓ EquipmentDurabilityTracker.cs exists and publishes events");
                return true;
            }

            Debug.LogError("✗ EquipmentDurabilityTracker.cs missing or incomplete");
            return false;
        }

        private static bool ValidateEquipmentDataSOExists()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs");
            if (script != null)
            {
                Debug.Log("✓ EquipmentDataSO.cs exists");
                return true;
            }

            Debug.LogError("✗ EquipmentDataSO.cs not found");
            return false;
        }

        private static bool ValidateRepairKitItemsExist()
        {
            var repairKits = new[] { "item_consumable_repair_kit_basic", "item_consumable_repair_kit_standard", "item_consumable_repair_kit_superior" };
            var found = 0;

            foreach (var kitId in repairKits)
            {
                var asset = AssetDatabase.LoadAssetAtPath<ItemDataSO>($"Assets/_Game/Data/Items/{kitId}.asset");
                if (asset != null && asset.ConsumableSubtype == ConsumableSubtype.RepairKit)
                {
                    found++;
                }
            }

            if (found == 3)
            {
                Debug.Log($"✓ All 3 RepairKit items exist with correct subtype");
                return true;
            }

            Debug.LogWarning($"⚠ Found {found}/3 RepairKit items");
            return found > 0;
        }

        private static bool ValidateEventsExist()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Core/Events/EquipmentSlotChangedEvent.cs");
            if (script != null && script.text.Contains("DurabilityChangedEvent") && script.text.Contains("ItemBrokenEvent") && script.text.Contains("ItemRepairedEvent"))
            {
                Debug.Log("✓ Equipment events (DurabilityChanged, ItemBroken, ItemRepaired) exist");
                return true;
            }

            Debug.LogError("✗ Equipment events missing or incomplete");
            return false;
        }

        private static bool ValidateLootTableIntegration()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Loot/LootTableSO.cs");
            if (script != null && script.text.Contains("TryRollEquipment") && script.text.Contains("EquipmentLootData"))
            {
                Debug.Log("✓ LootTableSO has equipment drop support with TryRollEquipment()");
                return true;
            }

            Debug.LogError("✗ LootTableSO missing equipment drop support");
            return false;
        }

        private static bool ValidateSaveDataStructures()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Save/SaveData.cs");
            if (script != null && script.text.Contains("EquipmentSaveData") && script.text.Contains("EquipmentSlotSaveData") && script.text.Contains("DurabilityEntryData"))
            {
                Debug.Log("✓ SaveData structures exist (EquipmentSaveData, EquipmentSlotSaveData, DurabilityEntryData)");
                return true;
            }

            Debug.LogError("✗ SaveData structures missing or incomplete");
            return false;
        }
    }
}
#endif
