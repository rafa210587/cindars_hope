namespace CindarsHope.UI.Tooltips
{
    public enum TooltipContext
    {
        Inventory = 0,
        ShopBuy = 1,
        ShopSell = 2,
        Equipment = 3,
        SkillMagic = 4,
        QuestKey = 5,
        Debug = 6,
        Crafting = 7
    }

    public static class TooltipLayerPolicy
    {
        public static bool ShowContextualPrice(TooltipContext ctx) =>
            ctx == TooltipContext.ShopBuy || ctx == TooltipContext.ShopSell || ctx == TooltipContext.Crafting;

        public static bool ShowEquipmentBlock(TooltipContext ctx) =>
            ctx == TooltipContext.Equipment || ctx == TooltipContext.Inventory;

        public static bool ShowSkillMagicBlock(TooltipContext ctx) =>
            ctx == TooltipContext.SkillMagic;

        public static bool ShowAdvancedBlock(TooltipContext ctx) =>
            ctx == TooltipContext.Debug;

        public static bool ShowQuestKeyWarning(TooltipContext ctx, bool isQuestOrKeyItem) =>
            isQuestOrKeyItem;

        public static bool IsSpoilerSafe(TooltipContext ctx) =>
            ctx != TooltipContext.Debug;
    }
}
