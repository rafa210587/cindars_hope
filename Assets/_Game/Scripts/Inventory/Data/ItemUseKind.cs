namespace CindarsHope.Inventory.Data
{
    /// <summary>
    /// SPEC_08: Item use kind contract. Defines how an item is used (equipment, consumption, tool, quest).
    /// Used with ItemDataSO.UseKind; if None, fallback to legacy Category/WeaponId/SpellId inference.
    /// </summary>
    public enum ItemUseKind
    {
        None = 0,
        EquipWeapon = 1,
        EquipAmmo = 2,
        EquipSpell = 3,
        ConsumeFood = 4,
        ConsumePotion = 5,
        UseTool = 6,
        Quest = 7
    }
}
