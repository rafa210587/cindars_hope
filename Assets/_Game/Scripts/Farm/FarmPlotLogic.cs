using System;
using CindarsHope.Farm.Crops;

namespace CindarsHope.Farm
{
    public struct DayResult
    {
        public bool CropAdvanced;
        public bool BecameReady;
        public bool CropDied;
        public bool TilledWetDried;
    }

    public struct HarvestOutcome
    {
        public bool Success;
        public string FailureReason;
        public string SeedId;
        public (string itemId, int amount, float yieldModifier, int qualityBonusUnits)[] Items;
        public CropQualityTier Quality;
        public bool FertilizerConsumed;
        public bool FertilizerWasActive;
    }

    public struct SeedParams
    {
        public string SeedId;
        public int GrowthDays;
        public int RegrowDays;
        public (string itemId, int baseAmount)[] HarvestPairs;
        public bool IsValid;
    }

    /// <summary>
    /// Lógica pura do FarmPlot — sem UnityEngine. Extraída de FarmPlot.cs (refactor fable_decomp).
    /// </summary>
    public class FarmPlotLogic
    {
        public const int DefaultDeathThresholdDays = 3;

        private FarmPlotState _state;
        private string _plantedSeedId = string.Empty;
        private int _daysGrown;
        private int _regrowRemainingDays;
        private int _daysWithoutWater;
        private int _lastProcessedDay;
        private string _fertilizerId = string.Empty;
        private int _wateredDaysCount;
        private int _currentDay;

        public FarmPlotState State => _state;
        public string PlantedSeedId => _plantedSeedId;
        public int DaysGrown => _daysGrown;
        public int RegrowRemainingDays => _regrowRemainingDays;
        public int DaysWithoutWater => _daysWithoutWater;
        public int LastProcessedDay => _lastProcessedDay;
        public string FertilizerId => _fertilizerId;
        public int WateredDaysCount => _wateredDaysCount;
        public int CurrentDay => _currentDay;

        public void SetCurrentDay(int day)
        {
            _currentDay = day;
        }

        public static FarmPlotState NormalizeState(FarmPlotState state)
        {
            switch (state)
            {
                case FarmPlotState.Blocked:
                case FarmPlotState.Raw:
                case FarmPlotState.TilledDry:
                case FarmPlotState.TilledWet:
                case FarmPlotState.PlantedDry:
                case FarmPlotState.PlantedWet:
                case FarmPlotState.ReadyToHarvest:
                case FarmPlotState.Dead:
                    return state;
                default:
                    return FarmPlotState.Raw;
            }
        }

        public static bool IsPlantedState(FarmPlotState state)
        {
            return state == FarmPlotState.PlantedDry || state == FarmPlotState.PlantedWet;
        }

        public void ResetPlot()
        {
            _plantedSeedId = string.Empty;
            _daysGrown = 0;
            _regrowRemainingDays = 0;
            _daysWithoutWater = 0;
            SetStateInternal(FarmPlotState.TilledDry);
        }

        public FarmPlotSaveData CaptureSaveData(int plotIndex)
        {
            var isWatered = _state == FarmPlotState.TilledWet || _state == FarmPlotState.PlantedWet;
            return new FarmPlotSaveData
            {
                PlotIndex = plotIndex,
                State = _state.ToString(),
                PlantedSeedId = _plantedSeedId,
                DaysGrown = _daysGrown,
                GrowthProgressDays = _daysGrown,
                IsWatered = isWatered,
                RegrowRemainingDays = _regrowRemainingDays,
                LastUpdatedDay = _currentDay,
                DaysWithoutWater = _daysWithoutWater,
                LastProcessedDay = _lastProcessedDay,
                FertilizerId = _fertilizerId,
                WateredDaysCount = _wateredDaysCount
            };
        }

        /// <summary>
        /// Restaura estado a partir de saveData. Não chama FarmFertilityRuntime nem resolve seedData.
        /// seedIsResolvable deve ser resolvido pelo adapter (FarmPlot) antes de chamar este método.
        /// Retorna false se o estado salvo for inválido e o plot foi resetado para TilledDry.
        /// </summary>
        public bool RestoreFromSaveData(FarmPlotSaveData saveData, bool seedIsResolvable)
        {
            if (saveData == null)
            {
                return false;
            }

            if (!Enum.TryParse(saveData.State, out FarmPlotState restoredState))
            {
                SetStateInternal(FarmPlotState.Raw);
                return false;
            }

            restoredState = NormalizeState(restoredState);
            _plantedSeedId = string.IsNullOrWhiteSpace(saveData.PlantedSeedId) ? string.Empty : saveData.PlantedSeedId;
            _daysGrown = Math.Max(0, saveData.GrowthProgressDays > 0 ? saveData.GrowthProgressDays : saveData.DaysGrown);
            _regrowRemainingDays = Math.Max(0, saveData.RegrowRemainingDays);
            _currentDay = Math.Max(1, saveData.LastUpdatedDay);
            _daysWithoutWater = Math.Max(0, saveData.DaysWithoutWater);
            _lastProcessedDay = Math.Max(0, saveData.LastProcessedDay);
            _wateredDaysCount = Math.Max(0, saveData.WateredDaysCount);
            _fertilizerId = string.IsNullOrWhiteSpace(saveData.FertilizerId) ? string.Empty : saveData.FertilizerId;

            if (!IsPlantedState(restoredState) && restoredState != FarmPlotState.ReadyToHarvest)
            {
                _plantedSeedId = string.Empty;
                _daysGrown = 0;
                _regrowRemainingDays = 0;
            }
            else if (!seedIsResolvable)
            {
                _plantedSeedId = string.Empty;
                _daysGrown = 0;
                _regrowRemainingDays = 0;
                restoredState = FarmPlotState.TilledDry;
            }

            SetStateInternal(restoredState);
            return true;
        }

        public bool TryTill(bool hasTool, bool temporarySliceMode, bool staminaOk)
        {
            if (_state != FarmPlotState.Raw || (!hasTool && !temporarySliceMode))
            {
                return false;
            }

            if (!staminaOk)
            {
                return false;
            }

            SetStateInternal(FarmPlotState.TilledDry);
            return true;
        }

        public bool TryWaterPlot(bool hasTool, bool temporarySliceMode, bool staminaOk)
        {
            if (!hasTool && !temporarySliceMode)
            {
                return false;
            }

            if (!staminaOk)
            {
                return false;
            }

            if (_state == FarmPlotState.TilledDry)
            {
                SetStateInternal(FarmPlotState.TilledWet);
                return true;
            }

            if (_state == FarmPlotState.PlantedDry)
            {
                SetStateInternal(FarmPlotState.PlantedWet);
                return true;
            }

            return false;
        }

        public bool TryPlantSeed(string seedId, bool seasonAllowed, bool hasInInventory, bool temporaryBypass, bool staminaOk)
        {
            if (_state != FarmPlotState.TilledDry && _state != FarmPlotState.TilledWet)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(seedId))
            {
                return false;
            }

            if (!seasonAllowed)
            {
                return false;
            }

            if (!temporaryBypass && !hasInInventory)
            {
                return false;
            }

            if (!staminaOk)
            {
                return false;
            }

            _plantedSeedId = seedId;
            _daysGrown = 0;
            _regrowRemainingDays = 0;
            SetStateInternal(_state == FarmPlotState.TilledWet ? FarmPlotState.PlantedWet : FarmPlotState.PlantedDry);
            return true;
        }

        /// <summary>
        /// Calcula o resultado da colheita sem mudar o estado. Chame OnHarvestCompleted depois de
        /// adicionar os itens ao inventário.
        /// </summary>
        public HarvestOutcome TryHarvestPure(SeedParams seed, float yieldModifier, bool fertilizerActive)
        {
            if (_state != FarmPlotState.ReadyToHarvest)
            {
                return new HarvestOutcome { Success = false, FailureReason = "CropNotReady" };
            }

            if (!seed.IsValid || seed.HarvestPairs == null || seed.HarvestPairs.Length == 0)
            {
                return new HarvestOutcome { Success = false, FailureReason = "NoHarvestData" };
            }

            var quality = CropQualityResolver.Resolve(BuildQualityInput(_wateredDaysCount, _daysGrown, fertilizerActive));
            var qualityBonusUnits = GetQualityBonusUnits(quality);
            var qualityBonusPending = qualityBonusUnits > 0;

            var items = new (string itemId, int amount, float yieldModifier, int qualityBonusUnits)[seed.HarvestPairs.Length];
            for (var i = 0; i < seed.HarvestPairs.Length; i++)
            {
                var (itemId, baseAmount) = seed.HarvestPairs[i];
                var amount = baseAmount;

                if (yieldModifier > 0f)
                {
                    amount += (int)Math.Floor(amount * yieldModifier);
                }

                var bonusForThisSlot = 0;
                if (qualityBonusPending)
                {
                    bonusForThisSlot = qualityBonusUnits;
                    amount += qualityBonusUnits;
                    qualityBonusPending = false;
                }

                items[i] = (itemId, amount, yieldModifier, bonusForThisSlot);
            }

            return new HarvestOutcome
            {
                Success = true,
                SeedId = _plantedSeedId,
                Items = items,
                Quality = quality,
                FertilizerWasActive = fertilizerActive,
                FertilizerConsumed = fertilizerActive
            };
        }

        /// <summary>
        /// Atualiza o estado pós-colheita (resetar WateredDays, regrow ou reset total).
        /// Deve ser chamado APÓS confirmar que ao menos um item foi adicionado ao inventário.
        /// </summary>
        public void OnHarvestCompleted(SeedParams seed)
        {
            _wateredDaysCount = 0;

            if (seed.RegrowDays > 0)
            {
                _daysGrown = Math.Max(0, seed.GrowthDays - seed.RegrowDays);
                _regrowRemainingDays = seed.RegrowDays;
                SetStateInternal(FarmPlotState.PlantedDry);
            }
            else
            {
                ResetPlot();
            }
        }

        public bool TryFertilizePure(string fertilizerId, bool hasInInventory)
        {
            if (string.IsNullOrWhiteSpace(fertilizerId))
            {
                return false;
            }

            if (!hasInInventory)
            {
                return false;
            }

            _fertilizerId = fertilizerId;
            return true;
        }

        public (bool success, string clearedSeedId) TryClearDead(bool hasTool, bool temporarySliceMode, bool staminaOk)
        {
            if (_state != FarmPlotState.Dead || (!hasTool && !temporarySliceMode))
            {
                return (false, string.Empty);
            }

            if (!staminaOk)
            {
                return (false, string.Empty);
            }

            var cleared = _plantedSeedId;
            SetStateInternal(FarmPlotState.TilledDry);
            return (true, cleared);
        }

        /// <summary>
        /// Define o estado interno, normalizando e resetando campos de planta em estados não-plantados.
        /// Não chama UpdateVisual (responsabilidade do adapter).
        /// </summary>
        public void SetStateInternal(FarmPlotState state)
        {
            _state = NormalizeState(state);

            if (_state == FarmPlotState.Raw || _state == FarmPlotState.TilledDry ||
                _state == FarmPlotState.TilledWet || _state == FarmPlotState.Blocked)
            {
                _plantedSeedId = string.Empty;
                _daysGrown = 0;
                _regrowRemainingDays = 0;
                _daysWithoutWater = 0;
                _wateredDaysCount = 0;
            }
        }

        public bool TryWaterSilent()
        {
            if (_state == FarmPlotState.TilledDry)
            {
                SetStateInternal(FarmPlotState.TilledWet);
                return true;
            }

            if (_state == FarmPlotState.PlantedDry)
            {
                SetStateInternal(FarmPlotState.PlantedWet);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Avança DaysGrown em 1. Retorna true se avançou; muda para ReadyToHarvest se completo.
        /// Requer que seed.IsValid == true.
        /// </summary>
        public bool TryAdvanceStagePure(SeedParams seed)
        {
            if (!seed.IsValid)
            {
                return false;
            }

            _daysGrown++;
            if (_daysGrown >= seed.GrowthDays)
            {
                SetStateInternal(FarmPlotState.ReadyToHarvest);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Processa a virada de dia com idempotência (LastProcessedDay). Retorna DayResult com flags
        /// para que o adapter publique eventos e chame UpdateVisual.
        /// </summary>
        public DayResult ProcessDay(int day, SeedParams seed)
        {
            var result = new DayResult();
            _currentDay = day;

            if (_lastProcessedDay == _currentDay)
            {
                return result;
            }

            _lastProcessedDay = _currentDay;

            if (_state == FarmPlotState.PlantedWet)
            {
                _daysWithoutWater = 0;
                _wateredDaysCount++;

                if (seed.IsValid)
                {
                    _daysGrown++;
                    if (_daysGrown >= seed.GrowthDays)
                    {
                        SetStateInternal(FarmPlotState.ReadyToHarvest);
                        result.BecameReady = true;
                    }
                    else
                    {
                        result.CropAdvanced = true;
                    }
                }

                if (_state == FarmPlotState.PlantedWet)
                {
                    SetStateInternal(FarmPlotState.PlantedDry);
                }
            }
            else if (_state == FarmPlotState.PlantedDry)
            {
                _daysWithoutWater++;
                if (_daysWithoutWater >= DefaultDeathThresholdDays)
                {
                    var deadSeedId = _plantedSeedId;
                    SetStateInternal(FarmPlotState.Dead);
                    // Preservamos plantedSeedId no campo Dead para o adapter publicar o evento
                    _plantedSeedId = deadSeedId;
                    result.CropDied = true;
                }
            }
            else if (_state == FarmPlotState.TilledWet)
            {
                SetStateInternal(FarmPlotState.TilledDry);
                result.TilledWetDried = true;
            }

            return result;
        }

        public static CropQualityInput BuildQualityInput(int wateredDaysCount, int daysGrown, bool fertilizerApplied)
        {
            var consistency = daysGrown <= 0
                ? 100
                : Math.Max(0, Math.Min(100, (int)Math.Round(wateredDaysCount * 100f / daysGrown, MidpointRounding.AwayFromZero)));

            return new CropQualityInput
            {
                WateringConsistencyScore = consistency,
                SeasonMatch = true,
                FertilizerApplied = fertilizerApplied,
                IsQualityEnabled = true
            };
        }

        public static int GetQualityBonusUnits(CropQualityTier tier)
        {
            switch (tier)
            {
                case CropQualityTier.Good:
                    return 1;
                case CropQualityTier.Excellent:
                case CropQualityTier.Rare:
                case CropQualityTier.Arcane:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>Expõe o campo _plantedSeedId mesmo quando estado é Dead (para eventos de morte).</summary>
        internal void SetFertilizerId(string fertilizerId)
        {
            _fertilizerId = fertilizerId ?? string.Empty;
        }
    }
}
