using System.Collections.Generic;
using CindarsHope.MainProgression;

namespace CindarsHope.Fonte
{
    public enum FonteState
    {
        Dormant = 0, AwakenedReturnOnly = 1, WaterFlowing = 2, MemoryEchoing = 3,
        LifeBlooming = 4, HopeReady = 5,
        FinalizedProtected = 6, FinalizedSealed = 7, FinalizedUsed = 8,
        Corrupted = 9, Damaged = 10
    }

    public enum FonteFunction
    {
        ReturnPoint = 0, LimitedLivingWater = 1, Respec = 2,
        MinorPurification = 3, AdvancedPurification = 4, CorruptionResistance = 5,
        MemoryReveal = 6, AquaticClueActivation = 7, FinalChoicePreparation = 8, PostGameState = 9
    }

    // Metadata about what each visual stage implies — no scene/prefab editing
    public class FonteVisualStageMetadata
    {
        public FonteState State { get; set; }
        public string DisplayDescriptionKey { get; set; }
        public bool WaterVisible { get; set; }
        public bool HasMemoryReflections { get; set; }
        public bool HasLifeParticles { get; set; }

        public static Dictionary<FonteState, FonteVisualStageMetadata> All { get; } =
            new Dictionary<FonteState, FonteVisualStageMetadata>
            {
                [FonteState.Dormant]              = new FonteVisualStageMetadata { State = FonteState.Dormant, DisplayDescriptionKey = "fonte_dormant", WaterVisible = false },
                [FonteState.AwakenedReturnOnly]   = new FonteVisualStageMetadata { State = FonteState.AwakenedReturnOnly, DisplayDescriptionKey = "fonte_awakened_return", WaterVisible = false },
                [FonteState.WaterFlowing]         = new FonteVisualStageMetadata { State = FonteState.WaterFlowing, DisplayDescriptionKey = "fonte_water_flowing", WaterVisible = true },
                [FonteState.MemoryEchoing]        = new FonteVisualStageMetadata { State = FonteState.MemoryEchoing, DisplayDescriptionKey = "fonte_memory_echoing", WaterVisible = true, HasMemoryReflections = true },
                [FonteState.LifeBlooming]         = new FonteVisualStageMetadata { State = FonteState.LifeBlooming, DisplayDescriptionKey = "fonte_life_blooming", WaterVisible = true, HasLifeParticles = true },
                [FonteState.HopeReady]            = new FonteVisualStageMetadata { State = FonteState.HopeReady, DisplayDescriptionKey = "fonte_hope_ready", WaterVisible = true, HasLifeParticles = true },
            };
    }

    // All simple types — save-safe
    public class FonteAnyaSection : IFinalChoiceFonteStateSink
    {
        public int Version { get; set; } = 1;
        public FonteState FonteState { get; set; } = FonteState.Dormant;
        public List<FonteFunction> UnlockedFunctions { get; set; } = new List<FonteFunction>();
        public LivingWaterState LivingWater { get; set; } = new LivingWaterState();
        public RespecState Respec { get; set; } = new RespecState();
        public PurificationState Purification { get; set; } = new PurificationState();
        public List<string> FragmentIntegrationRefs { get; set; } = new List<string>();
        public List<FonteUseRecord> LastUseRecords { get; set; } = new List<FonteUseRecord>();
        public string CorruptionState { get; set; }
        public string FinalFonteState { get; set; }
        public int LastValidatedVersion { get; set; } = 0;

        public bool HasFunction(FonteFunction fn) => UnlockedFunctions.Contains(fn);

        public void ApplyFinalFonteState(string fonteFinalState)
        {
            FonteState = fonteFinalState switch
            {
                "FinalizedProtected" => FonteState.FinalizedProtected,
                "FinalizedSealed"    => FonteState.FinalizedSealed,
                "FinalizedUsed"      => FonteState.FinalizedUsed,
                _ => FonteState.FinalizedProtected
            };
            FinalFonteState = fonteFinalState;
        }
    }

    public class LivingWaterState
    {
        public bool Unlocked { get; set; } = false;
        public int CurrentCharges { get; set; } = 0;
        public int MaxCharges { get; set; } = 3;
        public string RechargePolicy { get; set; }
        public int? LastRechargeDay { get; set; }
        public bool CanBeStoredAsItem { get; set; } = false;
        public string InventoryItemId { get; set; }
        // Anti-exploit: mass sale blocked
        public bool BlockMassSale { get; set; } = true;
    }

    public class RespecState
    {
        public bool Unlocked { get; set; } = false;
        public string CostPolicy { get; set; }
        public int? CooldownDays { get; set; }
        public int? LastRespecDay { get; set; }
        public bool RequiresConfirmation { get; set; } = true;
        public List<string> AllowedSkillSystems { get; set; } = new List<string>();
    }

    public class PurificationState
    {
        public bool UnlockedMinor { get; set; } = false;
        public bool UnlockedAdvanced { get; set; } = false;
        public List<string> AllowedTargets { get; set; } = new List<string>();
        public string CostPolicy { get; set; }
        public int? CooldownDays { get; set; }
        public bool RequiresLivingWater { get; set; } = false;
        public bool RequiresFragmentLife { get; set; } = true;
        public bool CanPurifyCorruptedLivingWater { get; set; } = false;
        public bool CanReduceCavePenalty { get; set; } = false;
        public bool CanAffectNpcAnimalSoil { get; set; } = false;
    }

    public class FonteUseRecord
    {
        public FonteFunction Function { get; set; }
        public int UsedAtDay { get; set; }
        public string PlayerId { get; set; }
    }
}
