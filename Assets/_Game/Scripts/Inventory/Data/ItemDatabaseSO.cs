// arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — ItemDatabaseSO
// movido de CindarsHope.Core.Data para CindarsHope.Inventory.Data (mesma namespace de ItemDataSO), pois
// nao ha motivo para o registry de itens residir no dominio Core; ItemDataSO permanece em Equipment/Magic.
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "CindarsHope/Database/Items")]
    public class ItemDatabaseSO : DataRegistrySO<ItemDataSO>
    {
    }
}
