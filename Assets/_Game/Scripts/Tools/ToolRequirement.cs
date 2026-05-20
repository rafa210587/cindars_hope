namespace CindarsHope.Tools
{
    public readonly struct ToolRequirement
    {
        public ToolType RequiredToolType { get; }
        public ToolTier RequiredTier { get; }

        public ToolRequirement(ToolType requiredToolType, ToolTier requiredTier)
        {
            RequiredToolType = requiredToolType;
            RequiredTier = requiredTier;
        }

        public bool IsSatisfiedBy(ToolType toolType, ToolTier tier)
        {
            if (RequiredToolType == ToolType.None)
            {
                return true;
            }

            return toolType == RequiredToolType && tier >= RequiredTier;
        }
    }
}
