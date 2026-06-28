using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Farm.Data;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Farm
{
    public partial class FarmPlot
    {
        private bool ValidateStamina(int requiredStamina)
        {
            if (_staminaManager == null)
                return true;

            return _staminaManager.CurrentStamina >= requiredStamina;
        }

        private bool TrySpendStamina(int requiredStamina)
        {
            if (_staminaManager == null)
                return true;

            return _staminaManager.TrySpendStamina(requiredStamina);
        }

        // â”€â”€ Private action methods â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private bool TryTill()
        {
            const int tillStaminaCost = 16;

            if (!ValidateStamina(tillStaminaCost))
            {
                PublishFeedback("Not enough stamina to till.");
                return false;
            }

            if (!TrySpendStamina(tillStaminaCost))
            {
                PublishFeedback("Not enough stamina to till.");
                return false;
            }

            var success = _logic.TryTill(HasRequiredTool(ToolType.Hoe), _temporarySequentialSliceMode, staminaOk: true);
            if (!success)
            {
                PublishFeedback("Cannot till this plot.");
                return false;
            }

            UpdateVisual();
            PublishFeedback("Soil tilled.");
            return true;
        }

        private bool TryWater()
        {
            const int waterStaminaCost = 8;

            if (!HasRequiredTool(ToolType.WateringCan) && !_temporarySequentialSliceMode)
            {
                PublishFeedback("Watering Can required.");
                return false;
            }

            if (!ValidateStamina(waterStaminaCost))
            {
                PublishFeedback("Not enough stamina to water.");
                return false;
            }

            if (!TrySpendStamina(waterStaminaCost))
            {
                PublishFeedback("Not enough stamina to water.");
                return false;
            }

            var success = _logic.TryWaterPlot(HasRequiredTool(ToolType.WateringCan), _temporarySequentialSliceMode, staminaOk: true);
            if (!success)
            {
                PublishFeedback("Cannot water this plot.");
                return false;
            }

            UpdateVisual();
            PublishFeedback(State == FarmPlotState.TilledWet ? "Soil watered." : "Crop watered.");
            return true;
        }

        private bool TryPlantSeed(string seedId, string seedItemId)
        {
            const int plantStaminaCost = 4;

            if (string.IsNullOrWhiteSpace(seedId) || _inventoryManager == null || _seedDatabase == null)
            {
                PublishFeedback("Seed data unavailable.");
                return false;
            }

            if (!_seedDatabase.TryGetById(seedId, out var seedData) || seedData == null)
            {
                PublishFeedback("Seed not registered.");
                return false;
            }

            if (!IsSeasonAllowedForSeed(seedData))
            {
                PublishFeedback("Fora de estacao para esta semente.");
                return false;
            }

            var inventorySeedId = string.IsNullOrWhiteSpace(seedItemId) ? seedId : seedItemId;
            var temporarySeedBypass = _temporarySequentialSliceMode && !_inventoryManager.HasItem(inventorySeedId);

            if (!temporarySeedBypass && !_inventoryManager.HasItem(inventorySeedId))
            {
                PublishFeedback("Seed not in inventory.");
                return false;
            }

            if (!ValidateStamina(plantStaminaCost))
            {
                PublishFeedback("Not enough stamina to plant.");
                return false;
            }

            if (!temporarySeedBypass && !_inventoryManager.RemoveItem(inventorySeedId, 1))
            {
                PublishFeedback("Could not consume seed.");
                return false;
            }

            if (!TrySpendStamina(plantStaminaCost))
            {
                if (!temporarySeedBypass)
                {
                    _inventoryManager.AddItem(inventorySeedId, 1);
                }

                PublishFeedback("Not enough stamina to plant.");
                return false;
            }

            var success = _logic.TryPlantSeed(
                seedId,
                seasonAllowed: true,       // jÃ¡ validado acima
                hasInInventory: true,      // jÃ¡ validado/consumido acima
                temporaryBypass: true,     // bypass de inventÃ¡rio jÃ¡ tratado
                staminaOk: true);          // jÃ¡ gasto acima

            if (!success)
            {
                PublishFeedback("Plot is not plantable.");
                return false;
            }

            UpdateVisual();
            GameEventBus.Publish(new SeedPlantedEvent(seedId, GetTilePosition(), _logic.CurrentDay));
            Debug.Log($"FarmPlot {_plotIndex} planted seed '{seedId}'.", this);
            return true;
        }

        private bool TryAdvanceTemporaryGrowth()
        {
            if (!_temporarySequentialSliceMode || State != FarmPlotState.PlantedWet)
            {
                PublishFeedback("Growth simulation unavailable.");
                return false;
            }

            if (!TryGetPlantedSeedData(out var seedData))
            {
                PublishFeedback("Seed data unavailable.");
                return false;
            }

            var seed = BuildSeedParams(seedData);
            var maxSteps = System.Math.Max(1, seedData.GrowthDays + 1);
            for (var step = 0; step < maxSteps && State == FarmPlotState.PlantedWet; step++)
            {
                _logic.TryAdvanceStagePure(seed);
            }

            UpdateVisual();
            PublishFeedback(State == FarmPlotState.ReadyToHarvest ? "Crop ready for harvest." : "Crop growth simulated.");
            return State == FarmPlotState.ReadyToHarvest;
        }

        private bool TryClearDead()
        {
            const int clearStaminaCost = 8;

            if (!ValidateStamina(clearStaminaCost))
            {
                PublishFeedback("Not enough stamina to clear.");
                return false;
            }

            if (!TrySpendStamina(clearStaminaCost))
            {
                PublishFeedback("Not enough stamina to clear.");
                return false;
            }

            var (success, clearedSeedId) = _logic.TryClearDead(HasRequiredTool(ToolType.Hoe), _temporarySequentialSliceMode, staminaOk: true);
            if (!success)
            {
                PublishFeedback("Cannot clear this plot.");
                return false;
            }

            UpdateVisual();
            Debug.Log($"FarmPlot {_plotIndex} cleared dead crop '{clearedSeedId}'. Plot reset to TilledDry.", this);
            PublishFeedback("Solo limpo.");
            return true;
        }

        private void ExecuteAnalyze()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"Estado: {State}");

            if (!string.IsNullOrWhiteSpace(PlantedSeedId))
            {
                sb.Append($" | Semente: {PlantedSeedId}");
                sb.Append($" | Dias crescido: {DaysGrown}");
            }

            if (State == FarmPlotState.PlantedDry || State == FarmPlotState.Dead)
            {
                sb.Append($" | Dias sem agua: {DaysWithoutWater}");
            }

            if (IsWatered)
            {
                sb.Append(" | Irrigado");
            }

            var feedback = sb.ToString();
            _menuController.SetFeedback(feedback);
            GameEventBus.Publish(new PlayerActionFeedbackEvent(feedback));
            Debug.Log($"FarmPlot {_plotIndex} analise: {feedback}", this);
        }

        private bool TryHarvest()
        {
            const int harvestStaminaCost = 4;

            if (State != FarmPlotState.ReadyToHarvest)
            {
                PublishFeedback("Crop is not ready.");
                return false;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest because InventoryManager is missing.", this);
                return false;
            }

            if (!ValidateStamina(harvestStaminaCost))
            {
                PublishFeedback("Not enough stamina to harvest.");
                return false;
            }

            if (!TryGetPlantedSeedData(out var seedData))
            {
                return false;
            }

            if (seedData.HarvestItems == null || seedData.HarvestAmounts == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest seed '{PlantedSeedId}' because harvest data is missing.", this);
                return false;
            }

            var pairCount = System.Math.Min(seedData.HarvestItems.Length, seedData.HarvestAmounts.Length);
            if (pairCount == 0)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest seed '{PlantedSeedId}' because harvest data is empty.", this);
                return false;
            }

            if (!TrySpendStamina(harvestStaminaCost))
            {
                PublishFeedback("Not enough stamina to harvest.");
                return false;
            }

            var fertilizerModifier = FarmFertilityRuntime.GetModifier(PlotId);
            var fertilizerActive = fertilizerModifier != null && fertilizerModifier.IsActive;
            var yieldModifier = fertilizerActive ? fertilizerModifier.YieldModifierSnapshot : 0f;

            var pairs = new (string itemId, int baseAmount)[pairCount];
            for (var i = 0; i < pairCount; i++)
            {
                var item = seedData.HarvestItems[i];
                pairs[i] = (item != null ? item.Id : string.Empty, seedData.HarvestAmounts[i]);
            }

            var seed = new SeedParams
            {
                SeedId = seedData.Id,
                GrowthDays = seedData.GrowthDays,
                RegrowDays = seedData.RegrowDays,
                HarvestPairs = pairs,
                IsValid = true
            };

            var outcome = _logic.TryHarvestPure(seed, yieldModifier, fertilizerActive);
            if (!outcome.Success)
            {
                PublishFeedback($"Harvest failed: {outcome.FailureReason}");
                return false;
            }

            var harvestedSeedId = outcome.SeedId;
            var tilePosition = GetTilePosition();
            var harvestedAnyItem = false;

            foreach (var (itemId, amount, _, _) in outcome.Items)
            {
                if (string.IsNullOrEmpty(itemId) || amount <= 0)
                {
                    continue;
                }

                if (!_inventoryManager.AddItem(itemId, amount))
                {
                    Debug.LogWarning($"FarmPlot {_plotIndex} could not add harvest item '{itemId}' x{amount} to inventory.", this);
                    continue;
                }

                harvestedAnyItem = true;
                GameEventBus.Publish(new CropHarvestedEvent(harvestedSeedId, itemId, amount, tilePosition));
                Debug.Log($"FarmPlot {_plotIndex} harvested '{itemId}' x{amount} from seed '{harvestedSeedId}'.", this);
            }

            if (!harvestedAnyItem)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} harvest produced no items and plot will remain ready.", this);
                return false;
            }

            if (fertilizerActive)
            {
                FarmFertilityRuntime.ConsumeOnHarvest(PlotId);
                if (!FarmFertilityRuntime.HasActiveFertilizer(PlotId))
                {
                    _logic.SetFertilizerId(string.Empty);
                }
            }

            if (outcome.Quality > Crops.CropQualityTier.Normal)
            {
                PublishFeedback($"Colheita de qualidade: {outcome.Quality}.");
            }

            _logic.OnHarvestCompleted(seed);
            UpdateVisual();
            return true;
        }

        private bool TryFertilize(string fertilizerId)
        {
            if (string.IsNullOrWhiteSpace(fertilizerId) || _inventoryManager == null)
            {
                PublishFeedback("Fertilizante indisponivel.");
                return false;
            }

            if (!_inventoryManager.HasItem(fertilizerId))
            {
                PublishFeedback("Fertilizante nao esta no inventario.");
                return false;
            }

            var result = FarmFertilityRuntime.TryApply(PlotId, fertilizerId, _logic.CurrentDay);
            if (!result.Success)
            {
                PublishFeedback($"Nao foi possivel fertilizar ({result.FailureReason}).");
                return false;
            }

            if (!_inventoryManager.RemoveItem(fertilizerId, 1))
            {
                PublishFeedback("Nao foi possivel consumir o fertilizante.");
                return false;
            }

            _logic.TryFertilizePure(fertilizerId, hasInInventory: true);
            PublishFeedback("Solo fertilizado.");
            return true;
        }

    }
}