using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Quests
{
    /// <summary>
    /// fable_34 — kind of board contract template (Hund's notice board, QUEST_CATALOG §templates).
    /// </summary>
    public enum BoardTemplateKind
    {
        /// <summary>bd_cull — defeat N creatures of an aggressive band.</summary>
        Cull = 0,

        /// <summary>bd_gather — collect N of a NON-quest material.</summary>
        Gather = 1,

        /// <summary>bd_delivery — deliver N of a NON-quest item to the board.</summary>
        Delivery = 2
    }

    /// <summary>
    /// fable_34 — a board contract template. Targets are constrained at authoring time by the
    /// catalog rules (QUEST_CATALOG §10/§13.5): a Cull template NEVER points at a non-aggressive
    /// creature, and Gather/Delivery NEVER request a quest item. <see cref="QuestBoardService"/>
    /// enforces this with <see cref="QuestBoardService.IsTargetAllowed"/>.
    /// </summary>
    public sealed class BoardContractTemplate
    {
        public string TemplateId { get; }
        public BoardTemplateKind Kind { get; }
        public string TargetId { get; }
        public int Quantity { get; }
        public int BaseGold { get; }
        public int BaseXp { get; }

        public BoardContractTemplate(string templateId, BoardTemplateKind kind, string targetId,
            int quantity, int baseGold, int baseXp)
        {
            TemplateId = templateId;
            Kind = kind;
            TargetId = targetId;
            Quantity = quantity;
            BaseGold = baseGold;
            BaseXp = baseXp;
        }

        public QuestObjectiveType ObjectiveType()
        {
            switch (Kind)
            {
                case BoardTemplateKind.Cull: return QuestObjectiveType.DefeatEnemy;
                case BoardTemplateKind.Delivery: return QuestObjectiveType.DeliverItem;
                case BoardTemplateKind.Gather:
                default: return QuestObjectiveType.CollectItem;
            }
        }
    }

    /// <summary>
    /// fable_34 — generates the daily notice-board contracts (CA-1/CA-2).
    ///
    /// Pure/deterministic core (EditMode-testable): given (seed, day, playerLevel) it always
    /// returns the SAME 3 contracts (StableHash rotation, no Unity Random / timestamp / GUID).
    /// Rewards are scaled through the single point <see cref="QuestRewardScaling"/> by the player's
    /// level at generation time.
    ///
    /// This service does NOT accept or persist quests by itself — the runtime registers the
    /// generated instances into the existing QuestRegistry/QuestService (single flow).
    /// </summary>
    public sealed class QuestBoardService
    {
        public const int ContractsPerDay = 3;
        public const string InstanceIdPrefix = "board_contract";

        // Non-aggressive / non-targetable creatures (QUEST_CATALOG §13.5) — a Cull contract must
        // never point at these (Old Scrounger King, Goblin Warchief, Silence Warden, merchants).
        private static readonly HashSet<string> NonAggressiveCreatures = new HashSet<string>
        {
            "enemy_old_scrounger_king", "enemy_goblin_warchief", "enemy_silence_warden",
            "npc_cave_wandering_merchant"
        };

        // Quest items that a Gather/Delivery contract must never request (QUEST_CATALOG §10).
        // Board materials are ordinary resources only.
        private static readonly HashSet<string> QuestItems = new HashSet<string>
        {
            "item_quest_anya_fragment_water", "item_quest_anya_fragment_memory",
            "item_quest_anya_fragment_life", "item_quest_anya_fragment_hope",
            "item_quest_black_stone"
        };

        private readonly List<BoardContractTemplate> _templates;

        public QuestBoardService(IEnumerable<BoardContractTemplate> templates = null)
        {
            _templates = new List<BoardContractTemplate>(templates ?? DefaultTemplates());
            // Defensive: drop any template that violates the canonical board rules so a bad author
            // can never leak a quest item or a non-aggressive target onto the board.
            _templates.RemoveAll(t => t == null || !IsTargetAllowed(t.Kind, t.TargetId));
        }

        public IReadOnlyList<BoardContractTemplate> Templates => _templates;

        /// <summary>Canonical board rule guard (QUEST_CATALOG §10/§13.5).</summary>
        public static bool IsTargetAllowed(BoardTemplateKind kind, string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return false;
            if (kind == BoardTemplateKind.Cull)
                return !NonAggressiveCreatures.Contains(targetId);
            // Gather / Delivery
            return !QuestItems.Contains(targetId);
        }

        /// <summary>
        /// Deterministically generates the day's contracts. Same (seed, day, playerLevel) =>
        /// identical instances; consecutive days rotate. Reward scaling uses playerLevel.
        /// </summary>
        public List<QuestInstance> GenerateDailyContracts(string seed, int day, int playerLevel)
        {
            var result = new List<QuestInstance>();
            if (_templates.Count == 0) return result;

            int level = playerLevel < 1 ? 1 : playerLevel;
            var indices = QuestStableHash.SelectDailyIndices(
                QuestStableHash.BoardSalt, seed, day, _templates.Count, ContractsPerDay);

            int slot = 0;
            foreach (var idx in indices)
            {
                var template = _templates[idx];
                result.Add(new QuestInstance
                {
                    QuestId = $"{InstanceIdPrefix}_d{day}_{slot}",
                    QuestTemplateId = template.TemplateId,
                    Source = QuestSource.Board,
                    TargetId = template.TargetId,
                    Quantity = template.Quantity,
                    QuestLevel = level,
                    RewardGold = QuestRewardScaling.Scale(template.BaseGold, level),
                    RewardXp = QuestRewardScaling.Scale(template.BaseXp, level),
                    GeneratedForDay = day
                });
                slot++;
            }

            return result;
        }

        /// <summary>
        /// Default board template pool (QUEST_CATALOG moldes bd_*). Targets are deliberately
        /// ordinary aggressive bands and non-quest materials only.
        /// </summary>
        public static List<BoardContractTemplate> DefaultTemplates()
        {
            return new List<BoardContractTemplate>
            {
                new BoardContractTemplate("bd_cull_slime", BoardTemplateKind.Cull, "enemy_slime_basic", 4, 40, 30),
                new BoardContractTemplate("bd_cull_bat", BoardTemplateKind.Cull, "enemy_cave_bat", 5, 45, 32),
                new BoardContractTemplate("bd_cull_goblin", BoardTemplateKind.Cull, "enemy_goblin_scout", 3, 55, 40),
                new BoardContractTemplate("bd_gather_wood", BoardTemplateKind.Gather, "item_material_wood", 6, 30, 20),
                new BoardContractTemplate("bd_gather_stone", BoardTemplateKind.Gather, "item_material_stone", 6, 30, 20),
                new BoardContractTemplate("bd_gather_ore", BoardTemplateKind.Gather, "item_material_copper_ore", 4, 50, 35),
                new BoardContractTemplate("bd_delivery_bread", BoardTemplateKind.Delivery, "item_consumable_food_bread", 3, 35, 25),
                new BoardContractTemplate("bd_delivery_potion", BoardTemplateKind.Delivery, "item_consumable_potion_hp_small", 2, 60, 45)
            };
        }
    }
}
