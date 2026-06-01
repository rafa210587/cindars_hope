using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// LEGACY: This class is part of the deprecated fireball flow via ItemUseManager.
    ///
    /// Status: QUARANTINED - Registered by FireballItemBridge to handle fireball item use via
    /// the old ItemUseManager pattern. The new flow uses PlayerAttackController.TryExecuteSpellAttack().
    ///
    /// Reference: SPEC_04 (Wave 1 - Legacy Combat Quarantine)
    /// </summary>
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
