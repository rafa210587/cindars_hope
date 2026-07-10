using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;

namespace CindarsHope.UI
{
    /// <summary>
    /// Regras puras de resolucao/compatibilidade de slot de equipamento, extraidas do
    /// InventoryPanelController para isolar logica de dominio sem dependencia de UnityEngine UI
    /// ou estado de instancia (arch: legibilidade — extracao faithful, sem mudanca de comportamento).
    /// </summary>
    public static class EquipmentSlotRules
    {
        public static EquipmentSlot ResolveEquipmentSlot(ItemDataSO item)
        {
            // Slots explicitos vencem a inferencia por categoria (gerador preenche para municao:
            // a flecha permite [LeftHand,RightHand]). Sem isto, Ammo caia em None e a flecha
            // aparecia como "nao equipavel". Prefere a mao esquerda no Equip generico para deixar
            // a mao direita livre para o arco (Weapon -> RightHand).
            if (item.AllowedEquipmentSlots != null && item.AllowedEquipmentSlots.Length > 0)
            {
                foreach (var allowed in item.AllowedEquipmentSlots)
                {
                    if (allowed == EquipmentSlot.LeftHand)
                    {
                        return EquipmentSlot.LeftHand;
                    }
                }
                return item.AllowedEquipmentSlots[0];
            }

            if (item.Category == ItemCategory.Weapon)
            {
                return EquipmentSlot.RightHand;
            }

            if (item.Category == ItemCategory.Tool)
            {
                return EquipmentSlot.LeftHand;
            }

            // Municao (flecha): mao esquerda por padrao (deixa a direita para o arco). Fallback por
            // categoria caso o asset ainda nao tenha AllowedEquipmentSlots preenchido pelo gerador.
            if (item.Category == ItemCategory.Ammo)
            {
                return EquipmentSlot.LeftHand;
            }

            var id = item.Id?.ToLowerInvariant() ?? string.Empty;
            if (id.Contains("armor"))
            {
                return EquipmentSlot.Chest;
            }

            if (id.Contains("accessory") || id.Contains("ring") || id.Contains("amulet"))
            {
                return EquipmentSlot.Accessory;
            }

            return EquipmentSlot.None;
        }

        public static bool IsCompatibleWithEquipmentSlot(ItemDataSO item, EquipmentSlot equipmentSlot)
        {
            if (item == null || !item.IsEquippable)
            {
                return false;
            }

            // Slots explicitos vencem a inferencia por categoria: a flecha (Ammo) permite as duas
            // maos via AllowedEquipmentSlots; sem este caso, IsCompatible so aceitava Weapon na
            // direita e a flecha nunca era aceita por nenhum slot.
            if (item.AllowedEquipmentSlots != null && item.AllowedEquipmentSlots.Length > 0)
            {
                foreach (var allowed in item.AllowedEquipmentSlots)
                {
                    if (allowed == equipmentSlot)
                    {
                        return true;
                    }
                }
                return false;
            }

            // Municao (flecha) aceita em qualquer mao (fallback por categoria, idem acima).
            if (equipmentSlot == EquipmentSlot.RightHand)
            {
                return item.Category == ItemCategory.Weapon || item.Category == ItemCategory.Ammo;
            }

            if (equipmentSlot == EquipmentSlot.LeftHand)
            {
                return item.Category == ItemCategory.Tool || item.Category == ItemCategory.Ammo;
            }

            var id = item.Id?.ToLowerInvariant() ?? string.Empty;
            if (equipmentSlot == EquipmentSlot.Chest)
            {
                return id.Contains("armor");
            }

            if (equipmentSlot == EquipmentSlot.Accessory)
            {
                return id.Contains("accessory") || id.Contains("ring") || id.Contains("amulet");
            }

            return false;
        }
    }
}
