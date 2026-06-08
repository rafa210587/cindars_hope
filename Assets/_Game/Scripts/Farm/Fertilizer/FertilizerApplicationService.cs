using System.Collections.Generic;

namespace CindarsHope.Farm.Fertilizer
{
    public enum FertilizerApplicationFailure
    {
        None = 0,
        FertilizerNotFound,
        PlotInvalidState,
        EndgameReserved,
        StackingRejected,
        RequiredFarmLevelNotMet
    }

    public class FertilizerApplicationResult
    {
        public bool Success { get; set; }
        public FertilizerApplicationFailure FailureReason { get; set; }
        public SoilModifierState AppliedModifier { get; set; }

        public static FertilizerApplicationResult Ok(SoilModifierState modifier) =>
            new FertilizerApplicationResult { Success = true, AppliedModifier = modifier };

        public static FertilizerApplicationResult Fail(FertilizerApplicationFailure reason) =>
            new FertilizerApplicationResult { Success = false, FailureReason = reason };
    }

    public class FertilizerApplicationService
    {
        private readonly Dictionary<string, FertilizerDefinition> _definitions;
        private readonly Dictionary<string, SoilModifierState> _activePlotModifiers;

        public FertilizerApplicationService(
            Dictionary<string, FertilizerDefinition> definitions,
            Dictionary<string, SoilModifierState> activePlotModifiers)
        {
            _definitions = definitions ?? new Dictionary<string, FertilizerDefinition>();
            _activePlotModifiers = activePlotModifiers ?? new Dictionary<string, SoilModifierState>();
        }

        public FertilizerApplicationResult Apply(string plotId, string fertilizerId, int currentDay, int playerFarmLevel = 0)
        {
            if (!_definitions.TryGetValue(fertilizerId, out var def))
                return FertilizerApplicationResult.Fail(FertilizerApplicationFailure.FertilizerNotFound);

            if (def.IsEndgameReserved)
                return FertilizerApplicationResult.Fail(FertilizerApplicationFailure.EndgameReserved);

            if (def.RequiredFarmLevel > playerFarmLevel)
                return FertilizerApplicationResult.Fail(FertilizerApplicationFailure.RequiredFarmLevelNotMet);

            // Check stacking policy
            if (_activePlotModifiers.TryGetValue(plotId, out var existing) && existing.IsActive)
            {
                switch (def.StackingPolicy)
                {
                    case FertilizerStackingPolicy.RejectIfAnyFertilizer:
                        return FertilizerApplicationResult.Fail(FertilizerApplicationFailure.StackingRejected);

                    case FertilizerStackingPolicy.ReplaceSameTier:
                        if (_definitions.TryGetValue(existing.FertilizerId, out var existingDef) && existingDef.Tier != def.Tier)
                            return FertilizerApplicationResult.Fail(FertilizerApplicationFailure.StackingRejected);
                        break;

                    case FertilizerStackingPolicy.ReplaceLowerTier:
                        if (_definitions.TryGetValue(existing.FertilizerId, out var existDef2) && existDef2.Tier >= def.Tier)
                            return FertilizerApplicationResult.Fail(FertilizerApplicationFailure.StackingRejected);
                        break;
                }
            }

            var modifier = new SoilModifierState
            {
                PlotId = plotId,
                FertilizerId = fertilizerId,
                AppliedDay = currentDay,
                ExpiresDay = def.Duration == DurationPolicy.NDays ? currentDay + def.DurationDays : -1,
                ConsumedOnHarvest = def.Duration == DurationPolicy.OneCrop || def.Duration == DurationPolicy.UntilHarvest,
                RemainingUses = 1,
                QualityModifierSnapshot = def.QualityModifier,
                YieldModifierSnapshot = def.YieldModifier,
                IsActive = true
            };

            _activePlotModifiers[plotId] = modifier;
            return FertilizerApplicationResult.Ok(modifier);
        }

        public bool HasActiveFertilizer(string plotId)
        {
            return _activePlotModifiers.TryGetValue(plotId, out var modifier) && modifier.IsActive;
        }

        public SoilModifierState GetModifier(string plotId)
        {
            _activePlotModifiers.TryGetValue(plotId, out var modifier);
            return modifier;
        }
    }
}
