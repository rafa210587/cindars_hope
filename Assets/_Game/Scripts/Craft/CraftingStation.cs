using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Craft.Data;
using CindarsHope.Craft.Events;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Economy;
using CindarsHope.Player;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;

namespace CindarsHope.Craft
{
    public sealed class CraftingStation
    {
        private CraftingJob _job;

        public CraftingStation(string stationInstanceId, WorkshopType stationType)
        {
            StationInstanceId = stationInstanceId;
            StationType = stationType;
            StationLevel = 1;
        }

        public string StationInstanceId { get; }
        public WorkshopType StationType { get; }
        public int StationLevel { get; set; }
        public CraftingJob Job => _job;
        public bool HasActiveJob => _job != null && _job.Status == CraftingJobStatus.InProgress;
        public bool HasCompletedOutput => _job != null && _job.Status == CraftingJobStatus.Completed;
        public bool IsBusy => HasActiveJob || HasCompletedOutput;

        public bool CanStartCraft(RecipeDataSO recipe, out string failureReason)
        {
            if (recipe == null)
            {
                failureReason = "Recipe is unavailable.";
                return false;
            }

            if (recipe.RequiredStationType != StationType)
            {
                failureReason = $"Requires {recipe.RequiredStationType}.";
                return false;
            }

            if (recipe.RequiredWorkshopLevel > StationLevel)
            {
                failureReason = $"Requires station level {recipe.RequiredWorkshopLevel}.";
                return false;
            }

            if (!recipe.IsUnlockedByDefault &&
                !CindarsHope.Crafting.CraftingRecipeGate.IsRecipeUnlocked(
                    recipe.RequiredRecipeUnlockId))
            {
                failureReason = "Recipe is locked.";
                return false;
            }

            if (IsBusy)
            {
                failureReason = "Station already has a pending job or output.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        public bool TryStartCraft(RecipeDataSO recipe, InventoryManager inventory, out string failureReason,
            StaminaManager staminaManager = null, float craftTimeMultiplier = 1f,
            int staminaCostOverride = -1, Action<int> onStaminaSpent = null,
            Func<string, bool> isCommonIngredient = null,
            Func<string, string> reserveOutputInstanceId = null,
            Func<string, int?> resolveBaseDurability = null,
            float craftedDurabilityBonus = 0f,
            Action<string, int> initializeDurability = null,
            LivingForgeCraftSelection livingForgeSelection = default,
            CraftingSkillState craftingSkillState = null,
            int livingForgeRank = 0,
            int dayIndex = 1)
        {
            if (!CanStartCraft(recipe, out failureReason))
            {
                PublishFailure(recipe, failureReason);
                return false;
            }

            string livingForgeReservationToken = string.Empty;
            LivingForgePreparation livingForgePreparation = default;
            if (livingForgeSelection.IsSelected)
            {
                if (inventory == null ||
                    !inventory.TryGetItemData(recipe.OutputItemId, out var livingForgeOutputItem))
                {
                    failureReason = "Craft output is unavailable for Living Forge.";
                    PublishFailure(recipe, failureReason);
                    return false;
                }

                int livingForgeBaseDurability =
                    resolveBaseDurability?.Invoke(recipe.OutputItemId) ?? 0;
                if (!LivingForgeCapstoneResolver.TryPrepare(
                        livingForgeRank, livingForgeSelection, livingForgeOutputItem,
                        recipe.OutputAmount, livingForgeBaseDurability,
                        out livingForgePreparation, out failureReason))
                {
                    PublishFailure(recipe, failureReason);
                    return false;
                }

                if (!inventory.TryGetItemData(livingForgePreparation.OutputItemId, out _))
                {
                    failureReason = "Selected Living Forge output variant is unavailable.";
                    PublishFailure(recipe, failureReason);
                    return false;
                }

                livingForgeReservationToken = Guid.NewGuid().ToString("N");
                if (craftingSkillState == null ||
                    !craftingSkillState.TryReserve(dayIndex, livingForgeReservationToken))
                {
                    failureReason = "Living Forge charge is unavailable for this day.";
                    PublishFailure(recipe, failureReason);
                    return false;
                }
            }

            float materialReduction = CraftingPassiveConsumers.ResolveStationMaterialReduction(
                SkillModifierHooks.CraftCostReduction, StationType, recipe.RequiredStationType);
            Func<string, bool> eligibility = isCommonIngredient ?? (itemId =>
                inventory != null && inventory.TryGetItemData(itemId, out var item) &&
                CraftingPassiveConsumers.IsCommonIngredient(item));
            var requirements = CraftingPassiveConsumers.BuildRequirements(
                recipe.Ingredients, materialReduction, eligibility);
            if (livingForgeSelection.IsSelected &&
                livingForgePreparation.Choice == LivingForgeBenefitChoice.SaveCommonMaterial &&
                !TrySaveSelectedCommonMaterial(
                    requirements, livingForgePreparation.SavedCommonIngredientItemId,
                    eligibility, out failureReason))
            {
                ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                    livingForgeReservationToken);
                PublishFailure(recipe, failureReason);
                return false;
            }

            if (!ValidateIngredients(requirements, inventory, out failureReason))
            {
                ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                    livingForgeReservationToken);
                PublishFailure(recipe, failureReason);
                return false;
            }

            int staminaCost = staminaCostOverride >= 0 ? staminaCostOverride : recipe.StaminaCost;
            if (staminaManager != null && staminaCost > 0 && !staminaManager.TrySpendStamina(staminaCost))
            {
                ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                    livingForgeReservationToken);
                failureReason = "Not enough stamina.";
                PublishFailure(recipe, failureReason);
                return false;
            }
            if (!TryPrepareOriginCraft(requirements, inventory, recipe.OutputAmount,
                    out var consumedBatch, out var outputOriginLots))
            {
                if (staminaManager != null && staminaCost > 0)
                    staminaManager.AddStamina(staminaCost);
                ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                    livingForgeReservationToken);
                failureReason = "Ingredient origin could not be preserved.";
                PublishFailure(recipe, failureReason);
                return false;
            }
            var inventoryBeforeCraft = inventory.CaptureSaveData();
            inventoryBeforeCraft.OriginSchemaVersion = 1;
            if (!inventory.TryCommitBatchRemoval(consumedBatch, out _))
            {
                inventory.RestoreFromSaveData(inventoryBeforeCraft);
                if (staminaManager != null && staminaCost > 0)
                    staminaManager.AddStamina(staminaCost);
                ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                    livingForgeReservationToken);
                failureReason = "Ingredients could not be consumed safely.";
                PublishFailure(recipe, failureReason);
                return false;
            }

            string outputItemId = livingForgeSelection.IsSelected
                ? livingForgePreparation.OutputItemId
                : recipe.OutputItemId;
            bool uniqueOutput = inventory.TryGetItemData(outputItemId, out var outputItem)
                && outputItem.MaxStack == 1;
            if (uniqueOutput && recipe.OutputAmount != 1)
            {
                inventory.RestoreFromSaveData(inventoryBeforeCraft);
                if (staminaManager != null && staminaCost > 0)
                    staminaManager.AddStamina(staminaCost);
                ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                    livingForgeReservationToken);
                failureReason = "Non-stackable crafted output must have amount one.";
                PublishFailure(recipe, failureReason);
                return false;
            }

            string outputInstanceId = uniqueOutput && reserveOutputInstanceId != null
                ? reserveOutputInstanceId(outputItemId)
                : string.Empty;
            int baseDurability = uniqueOutput ? resolveBaseDurability?.Invoke(outputItemId) ?? 0 : 0;
            if (baseDurability <= 0 && uniqueOutput && livingForgeSelection.IsSelected)
                baseDurability = resolveBaseDurability?.Invoke(recipe.OutputItemId) ?? 0;
            int outputDurabilityMax = CraftedItemDurabilityProvider.ResolveMaxDurability(
                baseDurability, craftedDurabilityBonus);
            if (uniqueOutput &&
                LivingForgeOutputVariantCatalog.TryDescribe(
                    outputItemId, out var outputVariant) &&
                outputVariant.DurabilityMaxMultiplier > 1f)
            {
                outputDurabilityMax = LivingForgeOutputVariantCatalog.ScaleInteger(
                    outputDurabilityMax, outputVariant.DurabilityMaxMultiplier);
            }
            if (livingForgeSelection.IsSelected &&
                livingForgePreparation.DurabilityMultiplier > 1f)
            {
                outputDurabilityMax = LivingForgeOutputVariantCatalog.ScaleInteger(
                    outputDurabilityMax, livingForgePreparation.DurabilityMultiplier);
            }

            if (recipe.IsInstantaneous)
            {
                bool added = !string.IsNullOrWhiteSpace(outputInstanceId)
                    ? inventory.TryAddItemInstanceWithLots(outputItemId, outputInstanceId,
                        outputOriginLots, false).Success
                    : inventory.TryAddItemWithLots(outputItemId, recipe.OutputAmount,
                        outputOriginLots, false).Success;
                if (!added)
                {
                    inventory.RestoreFromSaveData(inventoryBeforeCraft);
                    if (staminaManager != null && staminaCost > 0)
                        staminaManager.AddStamina(staminaCost);
                    ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                        livingForgeReservationToken);
                    failureReason = "Inventory is full for crafted output.";
                    PublishFailure(recipe, failureReason);
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(livingForgeReservationToken) &&
                    !craftingSkillState.Commit(dayIndex, livingForgeReservationToken))
                {
                    inventory.RestoreFromSaveData(inventoryBeforeCraft);
                    if (staminaManager != null && staminaCost > 0)
                        staminaManager.AddStamina(staminaCost);
                    ReleaseLivingForgeReservation(craftingSkillState, dayIndex,
                        livingForgeReservationToken);
                    failureReason = "Living Forge charge could not be committed safely.";
                    PublishFailure(recipe, failureReason);
                    return false;
                }

                if (outputDurabilityMax > 0)
                    initializeDurability?.Invoke(outputInstanceId, outputDurabilityMax);

                PublishOriginCraftInventoryEvents(inventory, consumedBatch, outputItemId,
                    recipe.OutputAmount);

                GameEventBus.Publish(new ItemCraftedEvent(recipe.Id, outputItemId, recipe.OutputAmount));
                GameEventBus.Publish(new CraftingOutputCollectedEvent(StationInstanceId, recipe.Id, outputItemId, recipe.OutputAmount));
                if (staminaManager != null && staminaCost > 0)
                    onStaminaSpent?.Invoke(staminaCost);
                failureReason = string.Empty;
                return true;
            }

            // fable_47 (follow-up 2): aplica o multiplicador de tempo de craft no ponto único.
            _job = new CraftingJob(StationInstanceId, recipe, craftTimeMultiplier,
                outputInstanceId, outputDurabilityMax, outputItemId,
                livingForgeReservationToken, livingForgeSelection.IsSelected ? dayIndex : 0,
                (int)livingForgePreparation.Choice);
            _job.OutputOriginLots = ItemOriginLot.CopyLots(outputOriginLots);
            _job.IngredientsConsumed.Clear();
            foreach (var selection in consumedBatch.Selections)
            {
                _job.IngredientsConsumed.Add(new CraftingIngredientSaveData
                {
                    ItemId = selection.ItemId,
                    Amount = selection.Quantity,
                    OriginLots = SaveOriginLots(selection.CopyLots())
                });
            }
            foreach (var selection in consumedBatch.Selections)
                inventory.PublishCommittedInventoryChange(selection.ItemId, -selection.Quantity);
            if (staminaManager != null && staminaCost > 0)
                onStaminaSpent?.Invoke(staminaCost);
            GameEventBus.Publish(new CraftingJobStartedEvent(StationInstanceId, recipe.Id, _job.JobId));
            failureReason = string.Empty;
            return true;
        }

        public bool TryCollectOutput(InventoryManager inventory, out string failureReason,
            Action<string, int> initializeDurability = null,
            CraftingSkillState craftingSkillState = null)
        {
            if (!HasCompletedOutput)
            {
                failureReason = "No completed output to collect.";
                return false;
            }

            var inventoryBeforeCollection = !string.IsNullOrWhiteSpace(_job.LivingForgeReservationToken)
                ? inventory.CaptureSaveData()
                : null;
            if (inventoryBeforeCollection != null) inventoryBeforeCollection.OriginSchemaVersion = 1;
            var outputLots = _job.OutputOriginLots;
            if (!ItemOriginLot.IsValid(outputLots, _job.OutputAmount))
                outputLots = ItemOriginLot.Invalid(_job.OutputAmount);
            bool added = !string.IsNullOrWhiteSpace(_job.OutputInstanceId)
                ? inventory.TryAddItemInstanceWithLots(_job.OutputItemId, _job.OutputInstanceId,
                    outputLots, false).Success
                : inventory.TryAddItemWithLots(_job.OutputItemId, _job.OutputAmount,
                    outputLots, false).Success;
            if (!added)
            {
                failureReason = "Inventory is full. Output remains at the station.";
                GameEventBus.Publish(new CraftingFailedEvent(StationInstanceId, _job.RecipeId, failureReason));
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_job.LivingForgeReservationToken) &&
                (craftingSkillState == null ||
                 !craftingSkillState.Commit(_job.LivingForgeDayIndex,
                     _job.LivingForgeReservationToken)))
            {
                inventory.RestoreFromSaveData(inventoryBeforeCollection);
                failureReason = "Living Forge charge could not be committed safely. Output remains at the station.";
                GameEventBus.Publish(new CraftingFailedEvent(
                    StationInstanceId, _job.RecipeId, failureReason));
                return false;
            }

            if (_job.OutputDurabilityMax > 0)
                initializeDurability?.Invoke(_job.OutputInstanceId, _job.OutputDurabilityMax);

            inventory.PublishCommittedInventoryChange(_job.OutputItemId, _job.OutputAmount);

            GameEventBus.Publish(new ItemCraftedEvent(_job.RecipeId, _job.OutputItemId, _job.OutputAmount));
            GameEventBus.Publish(new CraftingOutputCollectedEvent(StationInstanceId, _job.RecipeId, _job.OutputItemId, _job.OutputAmount));
            _job = null;
            failureReason = string.Empty;
            return true;
        }

        public bool TryCancelJob(InventoryManager inventory, out string failureReason,
            CraftingSkillState craftingSkillState = null)
        {
            if (!HasActiveJob)
            {
                failureReason = "Only an in-progress job can be cancelled.";
                return false;
            }

            var inventoryBeforeCancel = inventory.CaptureSaveData();
            inventoryBeforeCancel.OriginSchemaVersion = 1;
            foreach (var ingredient in _job.IngredientsConsumed)
            {
                var lots = ItemOriginLot.FromSaveData(ingredient.OriginLots, ingredient.Amount);
                if (!inventory.TryAddItemWithLots(ingredient.ItemId, ingredient.Amount,
                        lots, false).Success)
                {
                    inventory.RestoreFromSaveData(inventoryBeforeCancel);
                    failureReason = "Inventory has no room to return all ingredients.";
                    GameEventBus.Publish(new CraftingFailedEvent(StationInstanceId, _job.RecipeId, failureReason));
                    return false;
                }
            }

            foreach (var ingredient in _job.IngredientsConsumed)
                inventory.PublishCommittedInventoryChange(ingredient.ItemId, ingredient.Amount);

            var cancelledJobId = _job.JobId;
            var recipeId = _job.RecipeId;
            ReleaseLivingForgeReservation(craftingSkillState, _job.LivingForgeDayIndex,
                _job.LivingForgeReservationToken);
            _job.Cancel();
            _job = null;
            GameEventBus.Publish(new CraftingJobCancelledEvent(StationInstanceId, recipeId, cancelledJobId));
            failureReason = string.Empty;
            return true;
        }

        public void Update(float deltaTime)
        {
            if (!HasActiveJob)
            {
                return;
            }

            _job.Update(deltaTime);
            if (_job.IsComplete)
            {
                _job.Complete();
                GameEventBus.Publish(new CraftingJobCompletedEvent(StationInstanceId, _job.RecipeId, _job.JobId));
            }
        }

        public void LoadFromSaveData(CraftingStationSaveData saveData, RecipeDatabaseSO recipeDatabase)
            => LoadFromSaveData(saveData, recipeDatabase, false);

        public void LoadFromSaveData(CraftingStationSaveData saveData,
            RecipeDatabaseSO recipeDatabase, bool requireOriginSchema)
        {
            if (saveData == null)
            {
                return;
            }

            StationLevel = saveData.StationLevel > 0 ? saveData.StationLevel : 1;
            _job = null;
            if (saveData.Job == null || string.IsNullOrWhiteSpace(saveData.Job.RecipeId))
            {
                return;
            }

            recipeDatabase.TryGetById(saveData.Job.RecipeId, out var recipe);
            _job = new CraftingJob(saveData.Job, recipe, requireOriginSchema);
            if (recipe == null)
            {
                UnityEngine.Debug.LogError($"Crafting station '{StationInstanceId}' loaded missing recipe '{saveData.Job.RecipeId}'. Saved output and ingredients are retained.");
            }
        }

        public CraftingStationSaveData CaptureSaveData()
        {
            return new CraftingStationSaveData
            {
                StationInstanceId = StationInstanceId,
                StationType = (int)StationType,
                StationLevel = StationLevel,
                Job = _job?.CaptureSaveData()
            };
        }

        private static bool ValidateIngredients(
            IReadOnlyList<CraftingIngredientRequirement> requirements,
            InventoryManager inventory,
            out string failureReason)
        {
            if (inventory == null)
            {
                failureReason = "Inventory system is unavailable.";
                return false;
            }

            if (requirements == null || requirements.Count == 0)
            {
                failureReason = "Recipe has no ingredients.";
                return false;
            }

            foreach (var ingredient in requirements)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId) || ingredient.Amount <= 0 || !inventory.HasItem(ingredient.ItemId, ingredient.Amount))
                {
                    failureReason = $"Missing ingredient {ingredient.ItemId}.";
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        private static bool TryPrepareOriginCraft(
            IReadOnlyList<CraftingIngredientRequirement> requirements,
            InventoryManager inventory, int outputQuantity,
            out InventoryLotBatchSelection batch, out List<ItemOriginLot> outputLots)
        {
            batch = null;
            outputLots = null;
            var quantities = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var ingredient in requirements)
            {
                if (quantities.TryGetValue(ingredient.ItemId, out var existing))
                { if (existing > int.MaxValue - ingredient.Amount) return false;
                    quantities[ingredient.ItemId] = existing + ingredient.Amount; }
                else quantities.Add(ingredient.ItemId, ingredient.Amount);
            }
            if (!inventory.TryPrepareBatchRemoval(quantities, out batch)) return false;
            var inputs = new List<ItemOriginDerivation.Input>();
            foreach (var selected in batch.Selections)
            {
                if (!inventory.TryGetItemData(selected.ItemId, out var item)) return false;
                // Inject a shared balance config here if Shipping ever gains a materialized asset.
                var liquidation = ItemPriceResolver.ResolveSellingPrice(
                    item.BaseValue, SellContext.Shipping);
                inputs.Add(new ItemOriginDerivation.Input(selected.CopyLots(), liquidation));
            }
            return ItemOriginDerivation.TryDerive(inputs, outputQuantity, out outputLots);
        }

        private static List<ItemOriginLotSaveData> SaveOriginLots(IReadOnlyList<ItemOriginLot> lots)
        {
            var saved = new List<ItemOriginLotSaveData>();
            foreach (var lot in lots) saved.Add(lot.ToSaveData());
            return saved;
        }

        private static void PublishOriginCraftInventoryEvents(InventoryManager inventory,
            InventoryLotBatchSelection batch, string outputItemId, int outputAmount)
        {
            foreach (var selected in batch.Selections)
                inventory.PublishCommittedInventoryChange(selected.ItemId, -selected.Quantity);
            inventory.PublishCommittedInventoryChange(outputItemId, outputAmount);
        }

        private static bool TrySaveSelectedCommonMaterial(
            IList<CraftingIngredientRequirement> requirements,
            string selectedItemId,
            Func<string, bool> isCommonIngredient,
            out string failureReason)
        {
            if (string.IsNullOrWhiteSpace(selectedItemId) ||
                isCommonIngredient?.Invoke(selectedItemId) != true)
            {
                failureReason = "Selected ingredient is not an eligible common material.";
                return false;
            }

            for (int i = 0; i < requirements.Count; i++)
            {
                var requirement = requirements[i];
                if (!string.Equals(requirement.ItemId, selectedItemId,
                        StringComparison.Ordinal))
                    continue;

                if (requirement.Amount <= 1)
                {
                    failureReason = "Living Forge must still consume at least one selected material.";
                    return false;
                }

                requirements[i] = new CraftingIngredientRequirement(
                    requirement.ItemId, requirement.Amount - 1);
                failureReason = string.Empty;
                return true;
            }

            failureReason = "Selected common material is not required by this recipe.";
            return false;
        }

        private static void ReleaseLivingForgeReservation(
            CraftingSkillState state,
            int dayIndex,
            string reservationToken)
        {
            if (state != null && !string.IsNullOrWhiteSpace(reservationToken))
                state.Release(dayIndex, reservationToken);
        }

        private void PublishFailure(RecipeDataSO recipe, string failureReason)
        {
            GameEventBus.Publish(new CraftingFailedEvent(StationInstanceId, recipe != null ? recipe.Id : string.Empty, failureReason));
        }
    }

    [Serializable]
    public class CraftingStationSaveData
    {
        public string StationInstanceId;
        public int StationType;
        public int StationLevel;
        public CraftingJobSaveData Job;
    }
}
