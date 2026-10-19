using UnityEngine;

namespace CindarsHope.Inventory
{
    public abstract class ItemUseHandler : ScriptableObject
    {
        public abstract bool CanUseItem(string itemId, int amount);
        public abstract bool TryUseItem(string itemId, int amount, GameObject user);
    }
}
