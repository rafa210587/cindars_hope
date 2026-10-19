using CindarsHope.Farm;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateFarmSceneComposition
    {
        private static readonly FarmCompositionGroup[] Groups =
        {
            FarmCompositionGroup.NorthCliff,
            FarmCompositionGroup.WestForest,
            FarmCompositionGroup.CentralAgriculture,
            FarmCompositionGroup.NorthEastHomestead,
            FarmCompositionGroup.SouthAnimalRow,
            FarmCompositionGroup.SouthEastLake
        };

        [MenuItem("CindarsHope/Validar Composicao FarmScene")]
        public static void ValidateFromMenu()
        {
            var validGroups = 0;
            var errors = 0;
            for (var i = 0; i < Groups.Length; i++)
            {
                if (TryValidateGroup(Groups[i])) validGroups++;
                else errors++;
            }

            var outliers = CountCompositionOutliers();
            errors += outliers;
            Debug.Log("ValidateFarmSceneComposition: " + validGroups + "/" + Groups.Length + " groups valid");
            Debug.Log("ValidateFarmSceneComposition: Composition outliers: " + outliers);
            if (errors > 0) Debug.LogError("ValidateFarmSceneComposition: " + errors + " error(s)");
        }

        private static bool TryValidateGroup(FarmCompositionGroup group)
        {
            var anchor = FindAnchor(group);
            if (anchor != null && FarmSceneCompositionContract.GetTargetBounds(group).Contains(anchor.position)) return true;

            Debug.LogError("ValidateFarmSceneComposition: missing or out-of-bounds " + group + " anchor", anchor);
            return false;
        }

        private static Transform FindAnchor(FarmCompositionGroup group)
        {
            return group switch
            {
                FarmCompositionGroup.NorthCliff => FindByName("MountainBarrier"),
                FarmCompositionGroup.WestForest => FindChildAnchor("Trees", FarmSceneCompositionContract.GetTargetBounds(group)),
                FarmCompositionGroup.CentralAgriculture => FindByName("FarmPlots"),
                FarmCompositionGroup.NorthEastHomestead => FindByName("FarmHouse"),
                FarmCompositionGroup.SouthAnimalRow => FindChildAnchor("FarmAnimalHousings", FarmSceneCompositionContract.GetTargetBounds(group)),
                FarmCompositionGroup.SouthEastLake => FindByName("LakeCompositionAnchor"),
                _ => null
            };
        }

        private static int CountCompositionOutliers()
        {
            var outliers = 0;
            var bridge = FindByName(FarmSceneCompositionContract.BridgeScaleKey);
            if (bridge == null || !FarmSceneCompositionContract.IsScaleWithinApprovedRange(FarmSceneCompositionContract.BridgeScaleKey, bridge.localScale)) outliers++;

            var roof = FindChildByName("FarmHouse", "Roof");
            if (roof == null || !FarmSceneCompositionContract.IsScaleWithinApprovedRange(FarmSceneCompositionContract.HomesteadRoofScaleKey, roof.localScale)) outliers++;

            var playerRenderer = FindByName("Player")?.GetComponent<SpriteRenderer>();
            if (!TryGetOpaqueWorldExtent(playerRenderer, false, out var playerHeight)) return outliers + 6;

            var coopVisual = FindChildByName("Coop_01", "Visual");
            outliers += CountBuildingExtentOutlier(coopVisual, FarmSceneCompositionContract.CoopScaleKey, playerHeight, false);

            var barnVisual = FindChildByName("Barn_01", "Visual");
            outliers += CountBuildingExtentOutlier(barnVisual, FarmSceneCompositionContract.BarnScaleKey, playerHeight, false);

            outliers += CountBuildingExtentOutlier(FindChildByName("Station_CheesePress", "Visual"),
                FarmSceneCompositionContract.SouthProcessingScaleKey, playerHeight, false);
            outliers += CountBuildingExtentOutlier(FindChildByName("Station_WineBarrel", "Visual"),
                FarmSceneCompositionContract.SouthProcessingScaleKey, playerHeight, false);
            outliers += CountBuildingExtentOutlier(FindChildByName("Greenhouse", "GreenhouseFloor"),
                FarmSceneCompositionContract.GreenhouseScaleKey, playerHeight, false);
            outliers += CountBuildingExtentOutlier(roof, FarmSceneCompositionContract.HomesteadRoofScaleKey, playerHeight, true);

            var coopRoot = FindByName("Coop_01");
            var barnRoot = FindByName("Barn_01");
            var cheeseRoot = FindByName("Station_CheesePress");
            var wineRoot = FindByName("Station_WineBarrel");
            if (coopRoot == null || barnRoot == null || cheeseRoot == null || wineRoot == null ||
                !FarmSceneCompositionContract.IsSouthBuildingClusterCompact(
                    coopRoot.position.x, barnRoot.position.x, cheeseRoot.position.x, wineRoot.position.x))
                outliers++;

            var transforms = Object.FindObjectsByType<Transform>();
            for (var i = 0; i < transforms.Length; i++)
            {
                var transform = transforms[i];
                if (!transform.name.StartsWith(FarmSceneCompositionContract.TreeScaleKey, System.StringComparison.Ordinal)) continue;
                if (!FarmSceneCompositionContract.IsScaleWithinApprovedRange(transform.name, transform.localScale)) outliers++;
            }

            return outliers;
        }

        private static int CountBuildingExtentOutlier(Transform visual, string scaleKey, float playerHeight, bool useWidth)
        {
            var renderer = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
            if (!TryGetOpaqueWorldExtent(renderer, useWidth, out var extent)) return 1;
            return FarmSceneCompositionContract.IsBuildingVisualExtentApproved(scaleKey, extent) &&
                   FarmSceneCompositionContract.IsBuildingToPlayerRatioApproved(scaleKey, extent, playerHeight) ? 0 : 1;
        }

        private static bool TryGetOpaqueWorldExtent(SpriteRenderer renderer, bool useWidth, out float extent)
        {
            extent = 0f;
            if (renderer == null || renderer.sprite == null ||
                !SpriteOpaqueBoundsUtility.TryGetOpaqueLocalBounds(renderer.sprite, 0.05f, out var opaqueBounds)) return false;
            var localExtent = useWidth ? opaqueBounds.size.x : opaqueBounds.size.y;
            var scale = useWidth ? Mathf.Abs(renderer.transform.lossyScale.x) : Mathf.Abs(renderer.transform.lossyScale.y);
            extent = localExtent * scale;
            return extent > 0f;
        }

        private static Transform FindByName(string objectName)
        {
            var transforms = Object.FindObjectsByType<Transform>();
            for (var i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName) return transforms[i];
            }

            return null;
        }

        private static Transform FindChildAnchor(string rootName, Bounds bounds)
        {
            var root = FindByName(rootName);
            if (root == null) return null;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (bounds.Contains(child.position)) return child;
            }

            return null;
        }

        private static Transform FindChildByName(string rootName, string childName)
        {
            var root = FindByName(rootName);
            if (root == null) return null;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName) return child;
            }

            return null;
        }
    }
}
