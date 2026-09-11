using UnityEngine;

namespace CindarsHope.World.Scale
{
    public enum FarmCompositionGroup
    {
        NorthCliff,
        WestForest,
        CentralAgriculture,
        NorthEastHomestead,
        SouthAnimalRow,
        SouthEastLake
    }

    /// <summary>
    /// Visual macro-composition limits for the generated FarmScene.
    /// These bounds describe the approved keyart reading; collision and interaction
    /// ownership remains in the Farm spatial contract.
    /// </summary>
    public static class FarmSceneCompositionContract
    {
        public const string BridgeScaleKey = "Bridge_01";
        public const string TreeScaleKey = "TreeNode";
        public const string HomesteadRoofScaleKey = "FarmHouse/Roof";
        public const string CoopScaleKey = "Coop_01/Visual";
        public const string BarnScaleKey = "Barn_01/Visual";
        public const string SouthProcessingScaleKey = "SouthProcessing/Visual";
        public const string GreenhouseScaleKey = "Greenhouse/GreenhouseFloor";

        // Visual-only. The walkable bridge corridor is still owned by FarmSceneSpatialContract.
        // This preserves the former 2.3:1 silhouette while reducing the oversized keyart mass.
        public static readonly Vector3 BridgeVisualLocalScale = new Vector3(1.8f, 1.4f, 1f);
        public const float HomesteadRoofTargetWidth = 9.65f;
        public const float CraftingVisualTargetHeight = 3.4f;
        public const float CraftingSolidColliderWidth = 0.75f;
        public const float CraftingSolidColliderHeight = 0.6f;
        public const float CraftingSolidColliderLocalYOffset = -0.15f;
        public const float HomesteadDecorationClearance = 2f;
        public static readonly Vector2 BridgePassageProbe = new Vector2(27f, 3f);
        public const float WestForestTreeScaleMultiplier = 1.35f;
        // Keyart v2 calibration against the farmer at the gameplay scale. The south row was
        // deliberately raised as a group after the first integrated scene read as miniature.
        public const float CoopVisualTargetHeight = 5.1f;
        public const float BarnVisualTargetHeight = 7f;
        public const float SouthProcessingVisualTargetHeight = 6.85f;
        public const float GreenhouseVisualTargetHeight = 7.3f;
        public const float BuildingVisualExtentTolerance = 0.08f;
        public const float PrimaryPathWidth = 1.3f;
        public const float SecondaryPathWidth = 1.1f;
        public const float PathSpurWidth = 0.9f;
        public const float TownPathWidth = 1.45f;
        public const float SouthFrontPathWidth = 1.15f;
        public const float SouthBuildingMaximumCenterGap = 5.5f;
        public const float SouthBuildingMaximumClusterSpan = 15.5f;

        private const float BridgeScaleTolerance = 0.01f;
        // Escala por ALTURA-ALVO (CreateTree usa TreeVisualTargetHeight / nativeHeight): com a arte
        // nova (~2.7-4.3u nativa a 128 PPU) o localScale resultante cai em ~1.0-1.8. O range antigo
        // (3-5) valia para a arte pequena antiga; ajustado para o novo regime.
        private const float TreeScaleMinimum = 0.5f;
        private const float TreeScaleMaximum = 2.5f;
        private const float PositiveVisualScaleMinimum = 0.01f;
        private const float PositiveVisualScaleMaximum = 100f;

        public static Bounds GetTargetBounds(FarmCompositionGroup group)
        {
            return group switch
            {
                FarmCompositionGroup.NorthCliff => new Bounds(new Vector3(0f, 22f, 0f), new Vector3(72f, 11f, 1f)),
                FarmCompositionGroup.WestForest => new Bounds(new Vector3(-23f, 3f, 0f), new Vector3(18f, 28f, 1f)),
                FarmCompositionGroup.CentralAgriculture => new Bounds(new Vector3(-1.5f, 2.625f, 0f), new Vector3(13f, 13.13f, 1f)),
                FarmCompositionGroup.NorthEastHomestead => new Bounds(new Vector3(18f, 8f, 0f), new Vector3(18f, 16f, 1f)),
                FarmCompositionGroup.SouthAnimalRow => new Bounds(new Vector3(-3.5f, -11f, 0f), new Vector3(25f, 7f, 1f)),
                FarmCompositionGroup.SouthEastLake => new Bounds(new Vector3(22.78f, -10.07f, 0f), new Vector3(22.44f, 12.26f, 1f)),
                _ => throw new System.ArgumentOutOfRangeException(nameof(group), group, null)
            };
        }

        public static bool IsScaleWithinApprovedRange(string sceneObjectName, Vector3 localScale)
        {
            if (string.IsNullOrEmpty(sceneObjectName)) return false;

            if (sceneObjectName == BridgeScaleKey)
            {
                return Mathf.Abs(localScale.x - BridgeVisualLocalScale.x) <= BridgeScaleTolerance &&
                       Mathf.Abs(localScale.y - BridgeVisualLocalScale.y) <= BridgeScaleTolerance &&
                       Mathf.Abs(localScale.z - BridgeVisualLocalScale.z) <= BridgeScaleTolerance;
            }

            if (sceneObjectName.StartsWith(TreeScaleKey, System.StringComparison.Ordinal))
            {
                return localScale.x >= TreeScaleMinimum && localScale.x <= TreeScaleMaximum &&
                       Mathf.Approximately(localScale.x, localScale.y) && Mathf.Approximately(localScale.z, 1f);
            }

            return localScale.x >= PositiveVisualScaleMinimum && localScale.x <= PositiveVisualScaleMaximum &&
                   localScale.y >= PositiveVisualScaleMinimum && localScale.y <= PositiveVisualScaleMaximum &&
                   Mathf.Approximately(localScale.z, 1f);
        }

        public static float GetApprovedBuildingVisualExtent(string sceneObjectName)
        {
            return sceneObjectName switch
            {
                CoopScaleKey => CoopVisualTargetHeight,
                BarnScaleKey => BarnVisualTargetHeight,
                SouthProcessingScaleKey => SouthProcessingVisualTargetHeight,
                GreenhouseScaleKey => GreenhouseVisualTargetHeight,
                HomesteadRoofScaleKey => HomesteadRoofTargetWidth,
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneObjectName), sceneObjectName, null)
            };
        }

        public static Vector2 GetApprovedBuildingToPlayerRatioRange(string sceneObjectName)
        {
            // ScaleProfileApplicator changes the farmer from authored1.3 to runtime2.0. The range
            // deliberately covers both states while rejecting the miniature reading found in v2.
            return sceneObjectName switch
            {
                CoopScaleKey => new Vector2(4.1f, 6.9f),
                BarnScaleKey => new Vector2(5.6f, 9.4f),
                SouthProcessingScaleKey => new Vector2(4.4f, 7.5f),
                GreenhouseScaleKey => new Vector2(5.8f, 10f),
                HomesteadRoofScaleKey => new Vector2(6.8f, 11.4f),
                _ => throw new System.ArgumentOutOfRangeException(nameof(sceneObjectName), sceneObjectName, null)
            };
        }

        public static bool IsBuildingVisualExtentApproved(string sceneObjectName, float visualExtent)
        {
            var target = GetApprovedBuildingVisualExtent(sceneObjectName);
            return visualExtent > 0f && Mathf.Abs(visualExtent - target) <= target * BuildingVisualExtentTolerance;
        }

        public static bool IsBuildingToPlayerRatioApproved(string sceneObjectName, float visualExtent, float playerHeight)
        {
            if (visualExtent <= 0f || playerHeight <= 0f) return false;
            var range = GetApprovedBuildingToPlayerRatioRange(sceneObjectName);
            var ratio = visualExtent / playerHeight;
            return ratio >= range.x && ratio <= range.y;
        }

        public static bool IsSouthBuildingClusterCompact(float coopX, float barnX, float processingAX, float processingBX)
        {
            if (!(coopX < barnX && barnX < processingAX && processingAX < processingBX)) return false;
            return barnX - coopX <= SouthBuildingMaximumCenterGap &&
                   processingAX - barnX <= SouthBuildingMaximumCenterGap &&
                   processingBX - processingAX <= SouthBuildingMaximumCenterGap &&
                   processingBX - coopX <= SouthBuildingMaximumClusterSpan;
        }
    }
}
