using System.Collections.Generic;
using CindarsHope.Quests;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// In-memory catalog of QuestDefinitions and their associated rewards.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// TEMPORARY_QUEST_SMOKE_TEST: quests are hard-coded here for smoke testing.
    /// Future: replace with ScriptableObject-backed data pipeline.
    ///
    /// NOTE: QuestDefinition uses ObjectiveDefinition (legacy field) for Objectives.
    /// QuestObjective is the richer model — used only in QuestRegistry's extended catalog.
    /// For runtime use QuestObjective is stored in QuestCatalogEntry (separate from QuestDefinition).
    ///
    /// Anti-pattern: no ScriptableObject/MonoBehaviour references in this class.
    /// </summary>
    public class QuestRegistry
    {
        private readonly Dictionary<string, QuestDefinition> _quests = new Dictionary<string, QuestDefinition>();
        private readonly Dictionary<string, List<QuestRewardDefinition>> _rewardsByQuestId = new Dictionary<string, List<QuestRewardDefinition>>();
        private readonly Dictionary<string, List<QuestObjective>> _objectivesByQuestId = new Dictionary<string, List<QuestObjective>>();
        // Metadata not in QuestDefinition: GiverId
        private readonly Dictionary<string, string> _giverIdByQuestId = new Dictionary<string, string>();

        public QuestRegistry()
        {
            RegisterSmokeTestQuests();
        }

        public bool TryGetQuest(string questId, out QuestDefinition definition)
        {
            return _quests.TryGetValue(questId, out definition);
        }

        public IReadOnlyCollection<QuestDefinition> GetAllQuests()
        {
            return _quests.Values;
        }

        public List<QuestRewardDefinition> GetRewards(string questId)
        {
            if (_rewardsByQuestId.TryGetValue(questId, out var rewards))
                return rewards;
            return new List<QuestRewardDefinition>();
        }

        /// <summary>
        /// Returns richer QuestObjective list (with ObjectiveType/TargetId).
        /// This is the model used by QuestService for runtime objective tracking.
        /// </summary>
        public List<QuestObjective> GetObjectives(string questId)
        {
            if (_objectivesByQuestId.TryGetValue(questId, out var objs))
                return objs;
            return new List<QuestObjective>();
        }

        public string GetGiverId(string questId)
        {
            _giverIdByQuestId.TryGetValue(questId, out var giver);
            return giver ?? "";
        }

        public void Register(
            QuestDefinition definition,
            List<QuestObjective> objectives,
            List<QuestRewardDefinition> rewards = null,
            string giverId = null)
        {
            if (definition == null || string.IsNullOrEmpty(definition.QuestId)) return;
            _quests[definition.QuestId] = definition;
            _objectivesByQuestId[definition.QuestId] = objectives ?? new List<QuestObjective>();
            _rewardsByQuestId[definition.QuestId] = rewards ?? new List<QuestRewardDefinition>();
            if (giverId != null) _giverIdByQuestId[definition.QuestId] = giverId;
        }

        // ─── TEMPORARY_QUEST_SMOKE_TEST ────────────────────────────────────────────
        // Quest: quest_first_supplies_for_cindar
        // Fallback quest (always safe — no recipe dependency required)
        // Giver: npc_thalindra
        // Objectives: CollectItem wood x2, CollectItem stone x2
        // Rewards: Gold 50, QuestFlag "flag_first_town_supplies_delivered"
        // TurnIn: AtGiver (npc_thalindra)
        // ────────────────────────────────────────────────────────────────────────────
        //
        // WAVE_INTEGRATION_26 — Questline Expansion: chain of 3 quests with objective variety
        // Quest 2: quest_tools_for_the_town — SellItem (economy), Giver npc_pip, prereq Q1
        // Quest 3: quest_echo_from_the_cave — CaveLevelEntered, Giver npc_maelor, prereq Q2
        // ────────────────────────────────────────────────────────────────────────────
        private void RegisterSmokeTestQuests()
        {
            // ── Quest 1: Suprimentos para Cindar ──────────────────────────────────
            var supplyQuestObjectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_collect_wood_x2",
                    ObjectiveType = QuestObjectiveType.CollectItem,
                    TargetId = "item_material_wood",
                    RequiredAmount = 2
                },
                new QuestObjective
                {
                    ObjectiveId = "obj_collect_stone_x2",
                    ObjectiveType = QuestObjectiveType.CollectItem,
                    TargetId = "item_material_stone",
                    RequiredAmount = 2
                }
            };

            var supplyQuest = new QuestDefinition
            {
                QuestId = QuestRuntimeIds.SupplyQuestId,
                Category = QuestCategory.Tutorial,
                DisplayName = "Suprimentos para Cindar",
                Description = "A cidade precisa de recursos básicos para os primeiros reparos. Colete madeira e pedras perto da fazenda e entregue à Thalindra na cidade.",
                Trackable = true
            };

            var supplyRewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_supply_quest_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 50,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_supply_quest_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = "flag_first_town_supplies_delivered",
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };

            Register(supplyQuest, supplyQuestObjectives, supplyRewards, QuestRuntimeIds.ThalindraId);

            // ── Quest 2: Ferramentas para a Cidade ────────────────────────────────
            // WAVE_INTEGRATION_26: SellItem economy objective; Giver: npc_pip
            // Prerequisite: quest_first_supplies_for_cindar completed
            var toolsQuestObjectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_sell_crop_x1",
                    ObjectiveType = QuestObjectiveType.SellItem,
                    TargetId = QuestRuntimeIds.AnyCropItemTarget,
                    RequiredAmount = 1
                }
            };

            var toolsQuest = new QuestDefinition
            {
                QuestId = QuestRuntimeIds.ToolsQuestId,
                Category = QuestCategory.Side,
                DisplayName = "Ferramentas para a Cidade",
                Description = "Pip precisa de ouro para repor as ferramentas da cidade. Venda qualquer colheita ou recurso na banca de venda da fazenda.",
                Trackable = true,
                PrerequisiteQuestIds = new List<string> { QuestRuntimeIds.SupplyQuestId }
            };

            var toolsRewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_tools_quest_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 30,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_tools_quest_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = "flag_town_tools_funded",
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };

            Register(toolsQuest, toolsQuestObjectives, toolsRewards, QuestRuntimeIds.PipId);

            // ── Quest 3: Eco das Cavernas ─────────────────────────────────────────
            // WAVE_INTEGRATION_26: Cave entry / exploration objective; Giver: npc_maelor
            // Prerequisite: quest_tools_for_the_town completed
            var caveQuestObjectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_enter_cave_level1",
                    ObjectiveType = QuestObjectiveType.ReachCaveDepth,
                    TargetId = QuestRuntimeIds.CaveLevel1Target,
                    RequiredAmount = 1
                }
            };

            var caveQuest = new QuestDefinition
            {
                QuestId = QuestRuntimeIds.CaveQuestId,
                Category = QuestCategory.Side,
                DisplayName = "Eco das Cavernas",
                Description = "Maelor ouviu rumores de algo estranho nas cavernas próximas. Entre nas cavernas e investigue o primeiro andar.",
                Trackable = true,
                PrerequisiteQuestIds = new List<string> { QuestRuntimeIds.ToolsQuestId }
            };

            var caveRewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_cave_quest_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 60,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_cave_quest_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = "flag_cave_first_explored",
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };

            Register(caveQuest, caveQuestObjectives, caveRewards, QuestRuntimeIds.MaelorId);
        }
    }

    /// <summary>
    /// Stable quest and NPC ID constants for WAVE_INTEGRATION_15 smoke test quests
    /// and WAVE_INTEGRATION_26 questline expansion.
    /// </summary>
    public static class QuestRuntimeIds
    {
        // Quest 1 (WAVE15)
        public const string SupplyQuestId = "quest_first_supplies_for_cindar";
        public const string ThalindraId = "npc_thalindra";
        public const string SmokeTestBoardId = "board_first_quest_01";

        // Quest 2 (WAVE26)
        public const string ToolsQuestId = "quest_tools_for_the_town";
        public const string PipId = "npc_pip";
        // Sentinel value: SellItem with this TargetId means "any item sold" (bridge checks GoldDelta > 0)
        public const string AnyCropItemTarget = "any";

        // Quest 3 (WAVE26)
        public const string CaveQuestId = "quest_echo_from_the_cave";
        public const string MaelorId = "npc_maelor";
        // Cave level 1 entry target (matches CaveLevelEnteredEvent.CaveLevel == 1)
        public const string CaveLevel1Target = "cave_level_1";
    }
}
