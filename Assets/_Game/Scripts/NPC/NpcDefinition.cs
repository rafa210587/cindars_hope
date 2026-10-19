using System.Collections.Generic;

namespace CindarsHope.NPC
{
    // Pure C# NPC data contract. Coexists with NpcDataSO (Unity ScriptableObject).
    // Static definition only — mutable state (relationship points, questline progress,
    // service unlock flags, farm visit state) is persisted separately.
    public class NpcGiftPreferences
    {
        public List<string> LikedItemTags { get; set; } = new List<string>();
        public List<string> LovedItemIds { get; set; } = new List<string>();
        public List<string> DislikedItemTags { get; set; } = new List<string>();
        // fable_26 (emenda 2026-06-13-V3 §3): categorias que faltavam para o gosto por NPC.
        // NeutralItemTags é opcional (o default já é neutral quando não classificado).
        // HatedItemTags habilita a reação negativa alta (-6) específica por NPC.
        // Reutiliza esta struct (NÃO criar uma segunda) + a tag ItemTag.Giftable como porteiro.
        public List<string> NeutralItemTags { get; set; } = new List<string>();
        public List<string> HatedItemTags { get; set; } = new List<string>();
        public int DailyGiftLimit { get; set; } = 1;
    }

    public class NpcDefinition
    {
        public string NpcId { get; set; }
        public string DisplayName { get; set; }
        public NpcGender Gender { get; set; } = NpcGender.Unknown;
        public NpcAgeBand AgeBand { get; set; } = NpcAgeBand.Adult;
        public string RaceId { get; set; }
        public string SubraceId { get; set; }
        // Functional gameplay classes (NOT D&D classes)
        public FunctionalGameplayClass GameplayClassPrimary { get; set; } = FunctionalGameplayClass.None;
        public FunctionalGameplayClass GameplayClassSecondary { get; set; } = FunctionalGameplayClass.None;
        public List<string> RoleTags { get; set; } = new List<string>();
        public List<string> ServiceTags { get; set; } = new List<string>();
        public NpcReligionProfile ReligionProfile { get; set; } = new NpcReligionProfile();
        public RelationshipStatus RelationshipStatus { get; set; } = RelationshipStatus.Single;
        public string SpouseNpcId { get; set; }
        public RomanceEligibility RomanceEligibility { get; set; } = RomanceEligibility.NotEligible;
        public MarriageEligibility MarriageEligibility { get; set; } = MarriageEligibility.NotEligible;
        public bool CanVisitFarm { get; set; } = false;
        public bool CanMoveToFarmAfterMarriage { get; set; } = false;
        public List<string> FarmVisitRuleIds { get; set; } = new List<string>();
        public NpcStats Stats { get; set; } = new NpcStats();
        public List<string> StatusResistances { get; set; } = new List<string>();
        public List<string> StatusWeaknesses { get; set; } = new List<string>();
        public string CombatProfileId { get; set; }
        public string ScheduleId { get; set; }
        public string HomeLocationId { get; set; }
        public string WorkLocationId { get; set; }
        public string QuestlineId { get; set; }
        public NpcGiftPreferences GiftPreferences { get; set; } = new NpcGiftPreferences();
        public string DialogueSetId { get; set; }
        public string PortraitSpriteId { get; set; }
        // Status flags
        public List<string> RequiredQuestFlagsToMeet { get; set; } = new List<string>();
        public bool IsHiddenUntilUnlocked { get; set; } = false;
    }
}
