namespace CindarsHope.Inventory.Data
{
    /// <summary>
    /// SPEC_08: Helper for resolving ItemUseKind with fallback inference.
    /// Pure static utility; no dependencies on runtime systems.
    /// </summary>
    public static class ItemUseContractResolver
    {
        /// <summary>
        /// Resolve item use kind. Returns explicit UseKind if set; otherwise infers from legacy Category/WeaponId/SpellId/HungerRestore.
        /// All existing assets with UseKind=None continue to work via inference (backward compat).
        /// </summary>
        public static ItemUseKind Resolve(ItemDataSO item)
        {
            if (item == null) return ItemUseKind.None;

            // Explicit use kind takes precedence
            if (item.UseKind != ItemUseKind.None) return item.UseKind;

            // Inference from legacy Category fields (backward compat)
            if (item.Category == ItemCategory.Weapon && !string.IsNullOrEmpty(item.WeaponId))
                return ItemUseKind.EquipWeapon;

            if (item.Category == ItemCategory.Ammo)
                return ItemUseKind.EquipAmmo;

            if (item.Category == ItemCategory.Magic && !string.IsNullOrEmpty(item.SpellId))
                return ItemUseKind.EquipSpell;

            if (item.HungerRestore > 0 && (item.Category == ItemCategory.Food || item.Category == ItemCategory.Consumable))
                return ItemUseKind.ConsumeFood;

            if (item.Category == ItemCategory.Consumable && item.ConsumableSubtype == ConsumableSubtype.Potion)
                return ItemUseKind.ConsumePotion;

            if (item.Category == ItemCategory.Tool)
                return ItemUseKind.UseTool;

            if (item.Category == ItemCategory.Quest)
                return ItemUseKind.Quest;

            return ItemUseKind.None;
        }
    }
}
