namespace CindarsHope.Crafting
{
    public enum RecipeType
    {
        Unknown = 0,
        CraftRecipe,
        CookingRecipe,
        PotionRecipe,
        FertilizerRecipe,
        UpgradeRecipe,
        RepairRecipe,
        RefinementRecipe,
        BuildingRecipe,
        DecorationRecipe,
        ArrowRecipe,
        WandRecipe,
        ScrollRecipe,
        FocusRecipe,
        BromecianRecipe,
        ArcaneRecipe
    }

    public enum StationType
    {
        Workbench = 0,
        Forge,
        Anvil,
        Kitchen,
        AlchemyTable,
        FertilizerBin,
        LoomFuture,
        TanneryFuture,
        ArcaneBench,
        BromecianWorkbench,
        RepairStation,
        CookingPot,
        MakerStation
    }

    public enum ProcessingJobState
    {
        Queued = 0,
        Processing,
        ReadyToCollect,
        Collected,
        Cancelled,
        Failed
    }
}
