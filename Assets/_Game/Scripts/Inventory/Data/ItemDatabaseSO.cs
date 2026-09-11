// arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — ItemDatabaseSO
// movido de CindarsHope.Core.Data para CindarsHope.Inventory.Data (mesma namespace de ItemDataSO), pois
// nao ha motivo para o registry de itens residir no dominio Core; ItemDataSO permanece em Equipment/Magic.
using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "CindarsHope/Database/Items")]
    public class ItemDatabaseSO : DataRegistrySO<ItemDataSO>, IWeaponItemCatalog
    {
        public bool TryGetWeaponId(string itemId, out string weaponId)
        {
            weaponId = string.Empty;
            if (!TryGetById(itemId, out var item) || item == null || string.IsNullOrWhiteSpace(item.WeaponId))
                return false;
            weaponId = item.WeaponId;
            return true;
        }
    }
}
