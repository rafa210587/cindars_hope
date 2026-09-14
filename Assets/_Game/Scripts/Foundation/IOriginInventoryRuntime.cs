using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    // Corpse recovery lives outside Inventory. Keep provenance transfer on a primitive-only port.
    public interface IOriginInventoryRuntime
    {
        bool TryAddItemWithOriginLots(string itemId, string itemInstanceId, int amount,
            IReadOnlyList<ItemOriginLotSaveData> originLots);
        bool TryStageOriginAddition(string itemId, string itemInstanceId, int amount,
            IReadOnlyList<ItemOriginLotSaveData> originLots, out object token);
        bool TryRollbackOriginAddition(object token);
        bool TryCommitOriginAddition(object token);
        bool TryCommitEquippedOriginAddition(object token, string itemInstanceId,
            EquipmentSlot equipmentSlot);
    }
}
