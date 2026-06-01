using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Player
{
    public sealed class FireballUseHandler : ItemUseHandler
    {
        private FireballItemBridge _bridge;

        public void Configure(FireballItemBridge bridge) => _bridge = bridge;

        public override bool CanUseItem(string itemId, int amount) => _bridge != null && amount > 0;

        public override bool TryUseItem(string itemId, int amount, GameObject user)
        {
            if (_bridge == null)
                return false;
            _bridge.CastFireball();
            return true;
        }
    }
}
