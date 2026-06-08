using CindarsHope.Farm.Integration;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateFarmResourceInteractables
    {
        // Zone centers and max-distance thresholds matching CreateMvpFarmScene zone positions.
        // Residual: item IDs (item_wood, item_stone, item_herbs) are not validated against
        // ItemDatabase here because that requires loading a ScriptableObject asset at edit time.
        // Verify item IDs manually or in Play Mode if harvest returns false.
        private const float TreeZoneX = 9.5f, TreeZoneY = 1.0f, TreeZoneDist = 8f;
        private const float RockZoneX = -9.0f, RockZoneY = 5.0f, RockZoneDist = 5f;
        private const float ForageZoneX = -8.0f, ForageZoneY = -2.0f, ForageZoneDist = 5f;

        [MenuItem("CindarsHope/Validate/Validate Farm Resources", priority = 42)]
        public static void Validate()
        {
            int issues = 0;

            // 1. Confirm the three expected smoke nodes are present and correctly typed
            issues += CheckSmokeNode("TreeResource_01", FarmResourceInteractableType.Tree);
            issues += CheckSmokeNode("RockResource_01", FarmResourceInteractableType.Rock);
            issues += CheckSmokeNode("ForageResource_01", FarmResourceInteractableType.Forage);

            // 2. Deep-inspect all FarmResourceInteractable components in the open scene
            var interactables = Object.FindObjectsByType<FarmResourceInteractable>();
            if (interactables.Length == 0)
            {
                Debug.LogWarning("[ValidateFarmResource] No FarmResourceInteractable found in scene. Run CindarsHope/Archive/Scenes/Create MVP FarmScene first.");
                return;
            }

            foreach (var r in interactables)
            {
                var so = new SerializedObject(r);

                // resourceType must not be Unknown
                var typeProp = so.FindProperty("_resourceType");
                if (typeProp != null && typeProp.enumValueIndex == (int)FarmResourceInteractableType.Unknown)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: _resourceType is Unknown.", r);
                    issues++;
                }

                // _inventoryManager must be wired
                var invProp = so.FindProperty("_inventoryManager");
                if (invProp == null || invProp.objectReferenceValue == null)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: _inventoryManager not wired.", r);
                    issues++;
                }

                // _visualController must be wired
                var vcProp = so.FindProperty("_visualController");
                if (vcProp == null || vcProp.objectReferenceValue == null)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: _visualController not wired.", r);
                    issues++;
                }

                // reward: itemId non-empty, amount >= 1
                var rewardProp = so.FindProperty("_reward");
                if (rewardProp != null)
                {
                    var itemIdProp = rewardProp.FindPropertyRelative("_itemId");
                    if (itemIdProp != null && string.IsNullOrEmpty(itemIdProp.stringValue))
                    {
                        Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: reward._itemId is empty.", r);
                        issues++;
                    }

                    var amountProp = rewardProp.FindPropertyRelative("_amount");
                    if (amountProp != null && amountProp.intValue < 1)
                    {
                        Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: reward._amount is {amountProp.intValue} (must be >= 1; ClampedAmount will be used at runtime).", r);
                        issues++;
                    }
                }

                // SpriteRenderer: check on same object (visual controller target)
                var sr = r.GetComponent<SpriteRenderer>();
                var vcComp = vcProp?.objectReferenceValue as FarmResourceVisualController;
                if (sr == null && vcComp != null)
                    sr = vcComp.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: no SpriteRenderer found on object or its visual controller.", r);
                    issues++;
                }

                // Collider2D must exist for interaction trigger
                var col = r.GetComponent<Collider2D>();
                if (col == null)
                {
                    Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: no Collider2D found.", r);
                    issues++;
                }

                // Zone proximity check
                if (typeProp != null)
                {
                    var rType = (FarmResourceInteractableType)typeProp.enumValueIndex;
                    var pos = r.transform.position;
                    switch (rType)
                    {
                        case FarmResourceInteractableType.Tree:
                            if (!InZone(pos, TreeZoneX, TreeZoneY, TreeZoneDist))
                                Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: Tree at ({pos.x:F1},{pos.y:F1}) is outside Zone_ResourceTrees (center {TreeZoneX},{TreeZoneY} ±{TreeZoneDist}u).", r);
                            break;
                        case FarmResourceInteractableType.Rock:
                            if (!InZone(pos, RockZoneX, RockZoneY, RockZoneDist))
                                Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: Rock at ({pos.x:F1},{pos.y:F1}) is outside Zone_ResourceRocks (center {RockZoneX},{RockZoneY} ±{RockZoneDist}u).", r);
                            break;
                        case FarmResourceInteractableType.Forage:
                            if (!InZone(pos, ForageZoneX, ForageZoneY, ForageZoneDist))
                                Debug.LogWarning($"[ValidateFarmResource] {r.gameObject.name}: Forage at ({pos.x:F1},{pos.y:F1}) is outside Zone_Forage (center {ForageZoneX},{ForageZoneY} ±{ForageZoneDist}u).", r);
                            break;
                    }
                }
            }

            if (issues == 0)
                Debug.Log($"[ValidateFarmResource] All {interactables.Length} FarmResourceInteractable(s) validated. No issues found.");
            else
                Debug.LogWarning($"[ValidateFarmResource] {issues} issue(s) found across {interactables.Length} FarmResourceInteractable(s). Residual: item IDs not validated against ItemDatabase — verify manually in Play Mode.");
        }

        private static int CheckSmokeNode(string nodeName, FarmResourceInteractableType expectedType)
        {
            var go = GameObject.Find(nodeName);
            if (go == null)
            {
                Debug.LogWarning($"[ValidateFarmResource] Smoke node '{nodeName}' not found. Run Create MVP FarmScene first.");
                return 1;
            }

            var interactable = go.GetComponent<FarmResourceInteractable>();
            if (interactable == null)
            {
                Debug.LogWarning($"[ValidateFarmResource] '{nodeName}' found but missing FarmResourceInteractable.", go);
                return 1;
            }

            var so = new SerializedObject(interactable);
            var typeProp = so.FindProperty("_resourceType");
            if (typeProp != null && (FarmResourceInteractableType)typeProp.enumValueIndex != expectedType)
            {
                Debug.LogWarning($"[ValidateFarmResource] '{nodeName}' has wrong type: {(FarmResourceInteractableType)typeProp.enumValueIndex}, expected {expectedType}.", go);
                return 1;
            }

            return 0;
        }

        private static bool InZone(Vector3 pos, float cx, float cy, float maxDist)
        {
            return Mathf.Abs(pos.x - cx) <= maxDist && Mathf.Abs(pos.y - cy) <= maxDist;
        }
    }
}
