namespace CindarsHope.Foundation
{
    public interface IEquipmentRuntime
    {
        string GetEquippedItem(EquipmentSlot slot);
        void EquipItem(EquipmentSlot slot, string itemInstanceId);
        void ConfigureWeaponDurabilityResolver(System.Func<string, int?> resolver);
        int? ResolveBaseDurability(string itemId);
        void InitializeCraftedItemDurability(string itemInstanceId, int maxDurability);
    }
}
