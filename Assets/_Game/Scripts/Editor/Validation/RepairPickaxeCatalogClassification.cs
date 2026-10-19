using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class RepairPickaxeCatalogClassification
    {
        public static int Repair()
        {
            int changed = 0;
            foreach (string id in new[] { "item_tool_pickaxe_iron", "item_tool_pickaxe_steel" })
            {
                string path = "Assets/_Game/Data/Items/" + id + ".asset";
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item == null) throw new System.InvalidOperationException("Missing item: " + path);
                if (item.Category == ItemCategory.Tool) continue;
                var serialized = new SerializedObject(item);
                serialized.FindProperty("Category").intValue = (int)ItemCategory.Tool;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(item);
                changed++;
            }
            Debug.Log("Pickaxe category repair: " + changed + " changed (stable IDs preserved).");
            return changed;
        }
    }
}
