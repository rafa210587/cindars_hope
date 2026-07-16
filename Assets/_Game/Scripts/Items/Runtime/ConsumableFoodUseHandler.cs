using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// Handler de uso para itens consumíveis de comida/poção. Registrado no <see cref="ItemUseManager"/>
    /// por <see cref="ConsumableItemRuntimeBootstrap"/> para todos os itens de Category=Food/Consumable.
    ///
    /// Aplica HungerRestore, StaminaRestore e StatusEffectIds do <see cref="ItemDataSO"/> via singletons
    /// resolvidos do <see cref="GameBootstrap"/>. Usa <c>AccessoryEffectRouter</c> para escalar o efeito
    /// de fome/stamina pelo Charm de Thandra (+15%), espelhando <see cref="CindarsHope.Player.FoodConsumer"/>.
    ///
    /// Retorna <c>false</c> quando nenhum efeito é aplicável (fome/stamina cheias e sem status effect),
    /// preservando o item no inventário.
    /// </summary>
    public sealed class ConsumableFoodUseHandler : ItemUseHandler
    {
        private ItemDatabaseSO _itemDatabase;

        public void Configure(ItemDatabaseSO itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        public override bool CanUseItem(string itemId, int amount)
        {
            return amount > 0 && IsConsumable(itemId);
        }

        public override bool TryUseItem(string itemId, int amount, GameObject user)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
                return false;

            if (_itemDatabase == null || !_itemDatabase.TryGetById(itemId, out var itemData) || itemData == null)
            {
                Debug.LogWarning($"[ConsumableFoodUseHandler] ItemDatabase não resolveu '{itemId}'.", this);
                return false;
            }

            var bootstrap = GameBootstrap.Instance;
            var hungerManager = bootstrap != null ? bootstrap.HungerManager as CindarsHope.Player.HungerManager : null;
            var staminaManager = bootstrap != null ? bootstrap.StaminaManager as CindarsHope.Player.StaminaManager : null;
            var statusManager = bootstrap != null ? bootstrap.StatusEffectManager as CindarsHope.Player.StatusEffectManager : null;

            var hungerRestoreBase = itemData.HungerRestore;
            var staminaRestoreBase = itemData.StaminaRestore;

            var restoresHunger  = hungerRestoreBase > 0 && hungerManager != null && hungerManager.CurrentHunger < hungerManager.MaxHunger;
            var restoresStamina = staminaRestoreBase > 0 && staminaManager != null && staminaManager.CurrentStamina < staminaManager.MaxStamina;
            var appliesStatus   = statusManager != null && itemData.StatusEffectIds != null && itemData.StatusEffectIds.Length > 0;

            if (!restoresHunger && !restoresStamina && !appliesStatus)
            {
                Debug.Log($"[ConsumableFoodUseHandler] '{itemId}' ignorado — nenhum efeito aplicável no momento.", this);
                return false;
            }

            if (restoresHunger)
            {
                var scaled = CindarsHope.Equipment.AccessoryEffectRouter.ApplyFoodEffect(hungerRestoreBase);
                hungerManager.RestoreHunger(scaled);
            }

            if (restoresStamina)
            {
                var scaled = CindarsHope.Equipment.AccessoryEffectRouter.ApplyFoodEffect(staminaRestoreBase);
                staminaManager.AddStamina(scaled);
            }

            if (appliesStatus)
            {
                foreach (var effectId in itemData.StatusEffectIds)
                {
                    if (!string.IsNullOrWhiteSpace(effectId))
                        statusManager.TryAddEffect(effectId, itemData.BuffDurationSeconds);
                }
            }

            return true;
        }

        private bool IsConsumable(string itemId)
        {
            if (_itemDatabase == null || !_itemDatabase.TryGetById(itemId, out var itemData) || itemData == null)
                return false;

            return itemData.Category == ItemCategory.Food
                || itemData.Category == ItemCategory.Consumable
                || itemData.ConsumableSubtype == ConsumableSubtype.Food
                || itemData.ConsumableSubtype == ConsumableSubtype.BuffFood
                || itemData.ConsumableSubtype == ConsumableSubtype.Potion;
        }
    }
}
