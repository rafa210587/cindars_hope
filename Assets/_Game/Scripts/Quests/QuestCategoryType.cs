using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public enum QuestCategory
    {
        Main = 0,
        Side = 1,
        FarmOrder = 2,
        SocialFuture = 90,
        CompanionFuture = 91,
        PetFuture = 92,
        Festival = 10,
        CaveContract = 11,
        Tutorial = 12,
        Hidden = 13,
        System = 14
    }

    public enum QuestStateStatus
    {
        Unknown = 0,
        Discovered = 1,
        Available = 2,
        Active = 3,
        Waiting = 4,
        ReadyToComplete = 5,
        Completed = 6,
        Failed = 7,
        Expired = 8,
        HiddenCompleted = 9,
        Blocked = 10
    }

    public enum CompletionMode
    {
        AllObjectivesRequired = 0,
        AnyObjectiveRequired = 1,
        ChoiceBranch = 2,
        Timed = 3,
        ManualEvent = 4,
        ScriptedSequence = 5,
        HiddenCondition = 6
    }

    public enum QuestObjectiveType
    {
        TalkToNpc = 0, ReachLocation = 1, InteractWithObject = 2, CollectItem = 3, DeliverItem = 4,
        UseItem = 5, PlantCrop = 6, WaterCrop = 7, HarvestCrop = 8, CraftItem = 9, ProcessItem = 10,
        BuyItem = 11, SellItem = 12, ShipItem = 13, EarnGold = 14, BuildOrUpgrade = 15,
        FeedAnimalFuture = 80, PetInteractionFuture = 81, CompanionAssignedFuture = 82,
        DefeatEnemy = 20, DefeatEnemyFamily = 21, DefeatBoss = 22, SurviveCombat = 23,
        DiscoverBestiaryKnowledge = 24, DiscoverWeakness = 25,
        ReachCaveDepth = 30, CompleteCaveRun = 31, RecoverCorpse = 32,
        UnlockFonteFunction = 40, UpgradeFonte = 41, ProtectFragment = 42, SealFragment = 43, UseFragment = 44,
        WaitForTime = 50, WaitForDay = 51, WaitForSeason = 52, WaitForWeather = 53, WaitForLunarEvent = 54,
        AttendFestival = 55, WinFestivalActivityFuture = 56,
        ReadDocument = 60, LearnSpell = 61, EquipItem = 62, RepairItem = 63, UpgradeItem = 64,
        MakeDialogueChoice = 70, MakeFinalChoice = 71
    }

    public static class QuestEventName
    {
        public const string QuestAccepted = "OnQuestAccepted";
        public const string QuestStarted = "OnQuestStarted";
        public const string QuestStepStarted = "OnQuestStepStarted";
        public const string QuestStepCompleted = "OnQuestStepCompleted";
        public const string QuestCompleted = "OnQuestCompleted";
        public const string QuestFailed = "OnQuestFailed";
        public const string QuestExpired = "OnQuestExpired";
        public const string NpcDialogueStarted = "OnNpcDialogueStarted";
        public const string NpcDialogueEnded = "OnNpcDialogueEnded";
        public const string DialogueChoiceSelected = "OnDialogueChoiceSelected";
        public const string ItemCollected = "OnItemCollected";
        public const string ItemDelivered = "OnItemDelivered";
        public const string ItemUsed = "OnItemUsed";
        public const string ItemCrafted = "OnItemCrafted";
        public const string ItemProcessed = "OnItemProcessed";
        public const string ItemSold = "OnItemSold";
        public const string ItemShipped = "OnItemShipped";
        public const string CropPlanted = "OnCropPlanted";
        public const string CropWatered = "OnCropWatered";
        public const string CropHarvested = "OnCropHarvested";
        public const string BuildingUpgraded = "OnBuildingUpgraded";
        public const string LocationReached = "OnLocationReached";
        public const string ObjectInteracted = "OnObjectInteracted";
        public const string EnemyDefeated = "OnEnemyDefeated";
        public const string EnemyFamilyDefeated = "OnEnemyFamilyDefeated";
        public const string BossPhaseReached = "OnBossPhaseReached";
        public const string CaveDepthReached = "OnCaveDepthReached";
        public const string CaveRunCompleted = "OnCaveRunCompleted";
        public const string PlayerDeath = "OnPlayerDeath";
        public const string CorpseRecovered = "OnCorpseRecovered";
        public const string FonteFunctionUnlocked = "OnFonteFunctionUnlocked";
        public const string FonteUsed = "OnFonteUsed";
        public const string FragmentProtected = "OnFragmentProtected";
        public const string FragmentSealed = "OnFragmentSealed";
        public const string FragmentUsed = "OnFragmentUsed";
        public const string BestiaryKnowledgeUnlocked = "OnBestiaryKnowledgeUnlocked";
        public const string WeaknessDiscovered = "OnWeaknessDiscovered";
        public const string SpellLearned = "OnSpellLearned";
        public const string ItemEquipped = "OnItemEquipped";
        public const string DayStarted = "OnDayStarted";
        public const string TimeReached = "OnTimeReached";
        public const string SeasonStarted = "OnSeasonStarted";
        public const string WeatherChanged = "OnWeatherChanged";
        public const string LunarPhaseStarted = "OnLunarPhaseStarted";
        public const string FestivalStarted = "OnFestivalStarted";

        public static readonly HashSet<string> AllKnownNames = new HashSet<string>
        {
            QuestAccepted, QuestStarted, QuestStepStarted, QuestStepCompleted, QuestCompleted, QuestFailed, QuestExpired,
            NpcDialogueStarted, NpcDialogueEnded, DialogueChoiceSelected,
            ItemCollected, ItemDelivered, ItemUsed, ItemCrafted, ItemProcessed, ItemSold, ItemShipped,
            CropPlanted, CropWatered, CropHarvested, BuildingUpgraded, LocationReached, ObjectInteracted,
            EnemyDefeated, EnemyFamilyDefeated, BossPhaseReached, CaveDepthReached, CaveRunCompleted,
            PlayerDeath, CorpseRecovered, FonteFunctionUnlocked, FonteUsed,
            FragmentProtected, FragmentSealed, FragmentUsed, BestiaryKnowledgeUnlocked, WeaknessDiscovered,
            SpellLearned, ItemEquipped, DayStarted, TimeReached, SeasonStarted, WeatherChanged, LunarPhaseStarted, FestivalStarted
        };

        public static bool IsKnown(string name) => AllKnownNames.Contains(name);
    }
}
