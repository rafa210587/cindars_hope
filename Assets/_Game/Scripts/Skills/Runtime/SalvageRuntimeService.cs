using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Foundation;
using CindarsHope.Inventory;

namespace CindarsHope.Skills.Runtime
{
    public sealed class SalvageRuntimeService
    {
        public const string OperationKind = "salvage";
        public const float BaseReturnFraction = 0.4f;
        public const float BonusChanceCap = 0.15f;
        private readonly InventoryManager _inventory;
        private readonly ISalvageRecipeProvider _recipes;
        private readonly CraftingPassiveRngState _rng;

        public SalvageRuntimeService(InventoryManager inventory, ISalvageRecipeProvider recipes,
            CraftingPassiveRngState rng)
        { _inventory = inventory; _recipes = recipes; _rng = rng; }

        public bool TrySalvage(int slotIndex, float bonusChance, out string reason)
        {
            reason = string.Empty;
            if (bonusChance <= 0f)
            { reason = "Metodo de Salvage ainda nao foi aprendido."; return false; }
            if (_inventory == null || _recipes == null || _rng == null ||
                !_inventory.TryGetSlot(slotIndex, out var slot) || slot.IsEmpty || slot.IsEquipped || slot.Amount != 1 ||
                string.IsNullOrWhiteSpace(slot.ItemInstanceId) ||
                !_inventory.TryGetItemData(slot.ItemId, out var sourceData) || sourceData.MaxStack != 1 || sourceData.IsUnidentified)
            { reason = "Somente item individual, identificado e nao equipado pode ser reciclado."; return false; }
            if (!_recipes.TryGetCanonicalSalvageRecipe(slot.ItemId, out var recipe) || recipe.OutputAmount != 1)
            { reason = "O item nao possui receita canonica valida."; return false; }

            var rewards = new List<SalvageReward>();
            var eligibleTotal = 0;
            string firstEligible = null;
            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient.Amount <= 0 || string.Equals(ingredient.ItemId, slot.ItemId, StringComparison.Ordinal) ||
                    !_inventory.TryGetItemData(ingredient.ItemId, out var data) || !data.IsCommonMaterialBonusEligible) continue;
                eligibleTotal += ingredient.Amount;
                if (firstEligible == null) firstEligible = ingredient.ItemId;
                var amount = (int)Math.Floor(ingredient.Amount * BaseReturnFraction);
                if (amount > 0) AddReward(rewards, ingredient.ItemId, amount);
            }
            if (eligibleTotal < 2 || firstEligible == null)
            { reason = "A receita precisa de ao menos dois materiais comuns elegiveis."; return false; }
            if (rewards.Count == 0) rewards.Add(new SalvageReward(firstEligible, 1));

            var chance = Math.Min(BonusChanceCap, bonusChance);
            if (!_rng.TryPrepareRoll(OperationKind, slot.ItemInstanceId, chance, out var roll))
            { reason = "Nao foi possivel preparar o RNG do salvage."; return false; }
            if (roll.Succeeded)
            {
                var first = rewards[0];
                rewards[0] = new SalvageReward(first.ItemId, first.Amount + 1);
            }
            var salvagedItemId = slot.ItemId;
            var salvagedInstanceId = slot.ItemInstanceId;
            if (!_inventory.TryCommitSalvage(slotIndex, salvagedItemId, salvagedInstanceId, rewards,
                    () => _rng.Commit(roll), out reason)) return false;
            GameEventBus.Publish(new ItemSalvagedEvent(salvagedItemId, salvagedInstanceId, recipe.RecipeId, rewards));
            return true;
        }

        private static void AddReward(List<SalvageReward> rewards, string itemId, int amount)
        {
            for (var index = 0; index < rewards.Count; index++)
            {
                if (!string.Equals(rewards[index].ItemId, itemId, StringComparison.Ordinal)) continue;
                rewards[index] = new SalvageReward(itemId, rewards[index].Amount + amount);
                return;
            }
            rewards.Add(new SalvageReward(itemId, amount));
        }
    }
}
