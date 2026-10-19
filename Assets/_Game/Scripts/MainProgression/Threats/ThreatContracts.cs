using System.Collections.Generic;

namespace CindarsHope.MainProgression.Threats
{
    public enum MemoryArcState
    {
        Unknown = 0, NameHinted = 1, TermDiscovered = 2, InterpretedByVaelrion = 3,
        LinkedToCindar = 4, LinkedToElyndor = 5, PartiallyActivated = 6,
        CorruptedByBlackStone = 7, FinalActivated = 8, Protected = 9, Sealed = 10, Used = 11
    }

    public enum BlackStoneState
    {
        Unknown = 0, ObservedSmall = 1, UnstableShardKnown = 2, CultUseSuspected = 3,
        CultUseConfirmed = 4, DrainsMemory = 5, DrainsLivingWater = 6, DrainsSoul = 7,
        StabilizedFragmentKnown = 8, DeepGateMaterial = 9, FinalCorruptionSource = 10,
        Contained = 11, PurifiedLimited = 12, Sealed = 13
    }

    public enum CorruptionThreatLevel
    {
        None = 0, Hint = 1, MinorForgetfulness = 2, WaterTurbid = 3, MemoryDrain = 4,
        LivingWaterCorruption = 5, SoulDrain = 6, WorldThreat = 7, FinalConvergence = 8,
        Contained = 9
    }

    public enum VaelrionArcState
    {
        Unknown = 0, ArrivedAsScholar = 1, UsefulInterpreter = 2, ArroganceVisible = 3,
        BoundaryCrossed = 4, UsesCultKnowledge = 5, SeeksMemoryArc = 6,
        FinalConvergence = 7, MergedIntoArchivist = 8, Resolved = 9
    }

    public enum SethraCultState
    {
        Unknown = 0, NightShopRumors = 1, NyxMisinterpreted = 2, CultSuspected = 3,
        SethraRevealed = 4, RitualsActive = 5, SilencingAnya = 6,
        FinalConvergence = 7, Resolved = 8
    }

    // BlackStone exposure source types
    public enum BlackStoneSourceType
    {
        Cave = 0, CultRitual = 1, Item = 2, Boss = 3, Water = 4, MemoryEvent = 5, Final = 6
    }

    // All simple types — save-safe
    public class BlackStoneExposureRecord
    {
        public string ExposureId { get; set; }
        public BlackStoneSourceType SourceType { get; set; }
        public int? CaveLevel { get; set; }
        public int ActGate { get; set; }
        public CorruptionThreatLevel ThreatLevel { get; set; }
        public bool AffectsMemory { get; set; }
        public bool AffectsLivingWater { get; set; }
        public bool AffectsSoul { get; set; }
        public bool CanBePurified { get; set; }
        public bool RequiresLifeFragment { get; set; }
        public bool IsStable { get; set; }
        public bool IsCommodity { get; set; } = false; // NEVER true for cultist/corrupted form
        public int SpoilerTier { get; set; }
    }

    // Corrupted Living Water — separate from ordinary LivingWater charges
    public class CorruptedLivingWaterRecord
    {
        public string RecordId { get; set; }
        public string SourceExposureId { get; set; }
        public bool IsHealingItem { get; set; } = false; // NEVER true — requires purification/story
        public bool RequiresAdvancedPurification { get; set; } = true;
        public bool RequiresStoryRitual { get; set; }
        public bool ContainedSuccessfully { get; set; }
        public int DiscoveredAtDay { get; set; }
    }

    // Purification compatibility per threat level
    public class PurificationCompatibility
    {
        public CorruptionThreatLevel ThreatLevel { get; set; }
        public bool AllowsMinorPurification { get; set; }
        public bool AllowsAdvancedPurification { get; set; }
        public bool RequiresLifeFragment { get; set; }
        public bool RequiresStoryRitual { get; set; }
        public bool CanBeFullyCured { get; set; }
        public string BlockReason { get; set; }

        public static List<PurificationCompatibility> BuildCanonicalTable() => new List<PurificationCompatibility>
        {
            new PurificationCompatibility
            {
                ThreatLevel = CorruptionThreatLevel.MinorForgetfulness,
                AllowsMinorPurification = true, AllowsAdvancedPurification = true,
                CanBeFullyCured = true
            },
            new PurificationCompatibility
            {
                ThreatLevel = CorruptionThreatLevel.WaterTurbid,
                AllowsMinorPurification = true, AllowsAdvancedPurification = true,
                CanBeFullyCured = true
            },
            new PurificationCompatibility
            {
                ThreatLevel = CorruptionThreatLevel.LivingWaterCorruption,
                AllowsMinorPurification = false, AllowsAdvancedPurification = true,
                RequiresLifeFragment = true, CanBeFullyCured = true
            },
            new PurificationCompatibility
            {
                ThreatLevel = CorruptionThreatLevel.SoulDrain,
                AllowsMinorPurification = false, AllowsAdvancedPurification = false,
                RequiresStoryRitual = true, CanBeFullyCured = false,
                BlockReason = "SOUL_DRAIN_CANNOT_BE_CURED_BY_COMMON_ITEM"
            },
            new PurificationCompatibility
            {
                ThreatLevel = CorruptionThreatLevel.FinalConvergence,
                AllowsMinorPurification = false, AllowsAdvancedPurification = false,
                RequiresStoryRitual = true, CanBeFullyCured = false,
                BlockReason = "FINAL_CONVERGENCE_REQUIRES_FINAL_ROUTE_CHOICE"
            }
        };
    }
}
