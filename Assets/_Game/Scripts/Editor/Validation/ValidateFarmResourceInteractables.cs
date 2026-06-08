using CindarsHope.Farm.Integration;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateFarmResourceInteractables
    {
        [MenuItem("CindarsHope/Repair and Validate/Validate Farm Resource Interactables")]
        public static void Validate()
        {
            var interactables = Object.FindObjectsByType<FarmResourceInteractable>();
            if (interactables.Length == 0)
            {
                Debug.LogWarning("[ValidateFarmResource] No FarmResourceInteractable found in scene. Run CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene or add manually.");
                return;
            }

            int issues = 0;
            foreach (var r in interactables)
            {
                var so = new SerializedObject(r);
                var invProp = so.FindProperty("_inventoryManager");
                if (invProp != null && invProp.objectReferenceValue == null)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: _inventoryManager not wired.", r);
                    issues++;
                }

                var vcProp = so.FindProperty("_visualController");
                if (vcProp != null && vcProp.objectReferenceValue == null)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: _visualController not wired.", r);
                    issues++;
                }

                var rewardProp = so.FindProperty("_reward");
                if (rewardProp != null)
                {
                    var itemIdProp = rewardProp.FindPropertyRelative("_itemId");
                    if (itemIdProp != null && string.IsNullOrEmpty(itemIdProp.stringValue))
                    {
                        Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: reward _itemId is empty.", r);
                        issues++;
                    }
                }
            }

            if (issues == 0)
            {
                Debug.Log($"[ValidateFarmResource] All {interactables.Length} FarmResourceInteractable(s) validated. No issues.");
            }
            else
            {
                Debug.LogWarning($"[ValidateFarmResource] {issues} issue(s) found across {interactables.Length} FarmResourceInteractable(s). Open scene objects to fix.");
            }
        }
    }
}
