using System.Collections.Generic;
using CindarsHope.MainProgression;

namespace CindarsHope.Fonte
{
    public class FonteUnlockResult
    {
        public bool Success { get; set; }
        public bool AlreadyUnlocked { get; set; }
        public FonteFunction Function { get; set; }
        public string FailureReason { get; set; }

        public static FonteUnlockResult Fail(string reason) =>
            new FonteUnlockResult { Success = false, FailureReason = reason };
    }

    public class FonteUseRequest
    {
        public FonteFunction RequestedFunction { get; set; }
        public string PlayerId { get; set; }
        public int CurrentDay { get; set; }
        public int AvailableLivingWaterCharges { get; set; }
        public bool HasConfirmedRespec { get; set; }
    }

    public class FonteUseResult
    {
        public bool Success { get; set; }
        public FonteFunction Function { get; set; }
        public string FailureReason { get; set; }
        public int LivingWaterChargesConsumed { get; set; }
        public List<string> GrantedEffects { get; set; } = new List<string>();

        public static FonteUseResult Fail(string reason) =>
            new FonteUseResult { Success = false, FailureReason = reason };
    }

    // Pure C# service — no Unity, no MonoBehaviour, no live scene state
    public class FonteFunctionUnlockService
    {
        // Try to unlock a function. Requires corresponding fragment to be integrated.
        public FonteUnlockResult TryUnlock(
            FonteAnyaSection section,
            FonteFunction function,
            MainProgressionSection progression)
        {
            if (section == null) return FonteUnlockResult.Fail("SECTION_NULL");
            if (progression == null) return FonteUnlockResult.Fail("PROGRESSION_NULL");

            if (section.HasFunction(function))
                return new FonteUnlockResult { Success = true, AlreadyUnlocked = true, Function = function };

            var prereq = CheckUnlockPrerequisite(function, progression);
            if (!prereq.Met)
                return FonteUnlockResult.Fail(prereq.Reason);

            section.UnlockedFunctions.Add(function);
            UpdateFonteState(section, function);
            return new FonteUnlockResult { Success = true, Function = function };
        }

        private (bool Met, string Reason) CheckUnlockPrerequisite(
            FonteFunction function, MainProgressionSection progression)
        {
            switch (function)
            {
                case FonteFunction.ReturnPoint:
                    return (true, null); // First death/faint; no fragment required

                case FonteFunction.LimitedLivingWater:
                    if (!progression.IsFragmentIntegrated(MainFragmentType.Water))
                        return (false, "LIVING_WATER_REQUIRES_WATER_FRAGMENT");
                    return (true, null);

                case FonteFunction.Respec:
                    if (!progression.IsFragmentIntegrated(MainFragmentType.Memory))
                        return (false, "RESPEC_REQUIRES_MEMORY_FRAGMENT");
                    return (true, null);

                case FonteFunction.MinorPurification:
                    if (!progression.IsFragmentIntegrated(MainFragmentType.Water))
                        return (false, "MINOR_PURIFICATION_REQUIRES_WATER_FRAGMENT");
                    return (true, null);

                case FonteFunction.AdvancedPurification:
                    if (!progression.IsFragmentIntegrated(MainFragmentType.Life))
                        return (false, "ADVANCED_PURIFICATION_REQUIRES_LIFE_FRAGMENT");
                    return (true, null);

                case FonteFunction.FinalChoicePreparation:
                    if (!progression.IsFragmentIntegrated(MainFragmentType.Hope))
                        return (false, "FINAL_CHOICE_REQUIRES_HOPE_FRAGMENT");
                    if (progression.Level101AccessState != Level101AccessStatus.Unlocked)
                        return (false, "FINAL_CHOICE_REQUIRES_LEVEL101_ACCESS");
                    return (true, null);

                default:
                    return (true, null);
            }
        }

        private void UpdateFonteState(FonteAnyaSection section, FonteFunction unlockedFn)
        {
            var newState = unlockedFn switch
            {
                FonteFunction.LimitedLivingWater   => FonteState.WaterFlowing,
                FonteFunction.Respec               => FonteState.MemoryEchoing,
                FonteFunction.AdvancedPurification => FonteState.LifeBlooming,
                FonteFunction.FinalChoicePreparation => FonteState.HopeReady,
                FonteFunction.ReturnPoint          => FonteState.AwakenedReturnOnly,
                _ => section.FonteState
            };

            // State only advances, never regresses
            if ((int)newState > (int)section.FonteState)
                section.FonteState = newState;
        }

        // Evaluate a use request — does not execute, returns result
        public FonteUseResult EvaluateUseRequest(FonteAnyaSection section, FonteUseRequest request)
        {
            if (section == null || request == null) return FonteUseResult.Fail("NULL_ARGS");
            if (!section.HasFunction(request.RequestedFunction))
                return FonteUseResult.Fail($"FUNCTION_NOT_UNLOCKED:{request.RequestedFunction}");

            switch (request.RequestedFunction)
            {
                case FonteFunction.Respec:
                    if (!section.Respec.Unlocked)
                        return FonteUseResult.Fail("RESPEC_NOT_UNLOCKED");
                    if (!request.HasConfirmedRespec)
                        return FonteUseResult.Fail("RESPEC_REQUIRES_CONFIRMATION");
                    if (section.Respec.CooldownDays.HasValue && section.Respec.LastRespecDay.HasValue &&
                        request.CurrentDay - section.Respec.LastRespecDay.Value < section.Respec.CooldownDays.Value)
                        return FonteUseResult.Fail("RESPEC_ON_COOLDOWN");
                    return new FonteUseResult { Success = true, Function = request.RequestedFunction };

                case FonteFunction.LimitedLivingWater:
                    if (!section.LivingWater.Unlocked)
                        return FonteUseResult.Fail("LIVING_WATER_NOT_UNLOCKED");
                    if (section.LivingWater.CurrentCharges <= 0)
                        return FonteUseResult.Fail("LIVING_WATER_NO_CHARGES");
                    return new FonteUseResult
                    {
                        Success = true, Function = request.RequestedFunction,
                        LivingWaterChargesConsumed = 1
                    };

                case FonteFunction.AdvancedPurification:
                    if (!section.Purification.UnlockedAdvanced)
                        return FonteUseResult.Fail("ADVANCED_PURIFICATION_NOT_UNLOCKED");
                    return new FonteUseResult { Success = true, Function = request.RequestedFunction };

                default:
                    return new FonteUseResult { Success = true, Function = request.RequestedFunction };
            }
        }
    }
}
