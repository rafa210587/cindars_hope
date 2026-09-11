namespace CindarsHope.Foundation
{
    /// <summary>Neutral item-to-weapon link used by composition without importing Inventory data types.</summary>
    public interface IWeaponItemCatalog
    {
        bool TryGetWeaponId(string itemId, out string weaponId);
    }
}
