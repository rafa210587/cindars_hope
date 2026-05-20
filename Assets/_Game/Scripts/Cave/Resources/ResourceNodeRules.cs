using CindarsHope.Cave.Data;
using CindarsHope.Tools;

namespace CindarsHope.Cave.Resources
{
    public static class ResourceNodeRules
    {
        public static ResourceNodeToolCheckResult CheckTool(
            ResourceNodeDataSO nodeData,
            ToolType equippedToolType,
            ToolTier equippedToolTier)
        {
            if (nodeData == null)
            {
                return new ResourceNodeToolCheckResult(false, false, "Resource node data missing.");
            }

            if (nodeData.RequiredToolType == ToolType.None)
            {
                return new ResourceNodeToolCheckResult(true, false, string.Empty);
            }

            var hasCorrectTool = equippedToolType == nodeData.RequiredToolType;
            var hasRequiredTier = equippedToolTier >= nodeData.RequiredToolTier;
            if (hasCorrectTool && hasRequiredTier)
            {
                return new ResourceNodeToolCheckResult(true, false, string.Empty);
            }

            var hasFallback = !string.IsNullOrWhiteSpace(nodeData.FallbackItemId) && nodeData.FallbackAmount > 0;
            var message = hasCorrectTool
                ? $"Requires {nodeData.RequiredToolTier} {nodeData.RequiredToolType} or better."
                : $"Requires {nodeData.RequiredToolType}.";

            return new ResourceNodeToolCheckResult(false, false, message);
        }

        public static ResourceNodeInteractionResult BuildPrimaryDrop(ResourceNodeDataSO nodeData)
        {
            if (nodeData == null || string.IsNullOrWhiteSpace(nodeData.PrimaryDropItemId) || nodeData.PrimaryDropAmount <= 0)
            {
                return new ResourceNodeInteractionResult(false, false, string.Empty, 0, "Primary drop missing.");
            }

            return new ResourceNodeInteractionResult(true, true, nodeData.PrimaryDropItemId, nodeData.PrimaryDropAmount, string.Empty);
        }

        public static ResourceNodeInteractionResult BuildFallback(ResourceNodeDataSO nodeData, string message)
        {
            if (nodeData == null || string.IsNullOrWhiteSpace(nodeData.FallbackItemId) || nodeData.FallbackAmount <= 0)
            {
                return new ResourceNodeInteractionResult(false, false, string.Empty, 0, message);
            }

            return new ResourceNodeInteractionResult(true, nodeData.FallbackDepletesNode, nodeData.FallbackItemId, nodeData.FallbackAmount, message);
        }
    }
}
