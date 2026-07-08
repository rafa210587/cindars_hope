using CindarsHope.Foundation;

namespace CindarsHope.Quests
{
    /// <summary>
    /// fable_34 — a dynamically generated quest instance (board contract or any procedural quest).
    ///
    /// Holds ONLY simple types (ids/ints/enum) so it survives save/load with no Unity references
    /// (save-dto-simple-types-only rule). A QuestInstance is registered into the existing
    /// QuestRegistry/QuestService as a normal quest — it does NOT create a parallel flow.
    ///
    /// The <see cref="QuestTemplateId"/> identifies which template produced it; the
    /// <see cref="QuestId"/> is the concrete instance id used everywhere in the quest flow.
    /// </summary>
    public sealed class QuestInstance
    {
        /// <summary>Concrete instance id (e.g. <c>board_contract_d12_0</c>). Used by the quest flow.</summary>
        public string QuestId { get; set; }

        /// <summary>Template this instance was generated from (e.g. <c>bd_cull</c>).</summary>
        public string QuestTemplateId { get; set; }

        /// <summary>Channel this instance is delivered through.</summary>
        public QuestSource Source { get; set; } = QuestSource.Board;

        /// <summary>Objective target id (enemy band id, item id, npc id, …).</summary>
        public string TargetId { get; set; }

        /// <summary>Objective required amount (kills / items / deliveries).</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>The level used to scale rewards (player level at accept for dailies).</summary>
        public int QuestLevel { get; set; } = 1;

        /// <summary>Pre-computed scaled gold reward (single point of calculation — see <see cref="QuestRewardScaling"/>).</summary>
        public int RewardGold { get; set; }

        /// <summary>Pre-computed scaled XP reward.</summary>
        public int RewardXp { get; set; }

        /// <summary>Day (StableHash bucket) this instance was generated for. 0 = not day-bound.</summary>
        public int GeneratedForDay { get; set; }

        /// <summary>
        /// fable_51 — optional non-gold rewards (item / quest flag) granted on turn-in through the
        /// SAME single reward flow (<see cref="CindarsHope.Quests.Rewards.QuestRewardApplicator"/>).
        /// Empty for plain board contracts (which only grant scaled gold + XP). Cave contracts use
        /// this for "map segment" flags/items, guaranteed boss essences, and no-hit title + charm.
        /// Idempotency is carried by each reward's RewardId (board contracts) / GrantedFlagId.
        /// </summary>
        public System.Collections.Generic.List<CindarsHope.Quests.Rewards.QuestRewardDefinition> AdditionalRewards { get; set; }
    }

    /// <summary>
    /// fable_34 — THE single point of quest reward scaling (no duplicated formula anywhere else).
    ///
    /// Decision Q6.1/Q6.2 (FABLE_DECISOES_RESPOSTAS_v1.0 §5): XP and gold scale with quest level,
    /// nothing static. Catalog curve (QUEST_CATALOG §2 / BALANCE_CURVES §7):
    /// <c>reward = base * (1 + 0.08 * questLevel)</c>, floored to int, never below base.
    ///
    /// Pure/deterministic — EditMode-testable.
    /// </summary>
    public static class QuestRewardScaling
    {
        /// <summary>Catalog scaling coefficient (per quest level).</summary>
        public const double PerLevelCoefficient = 0.08d;

        public static int Scale(int baseAmount, int questLevel)
        {
            if (baseAmount <= 0) return 0;
            int level = questLevel < 0 ? 0 : questLevel;
            double scaled = baseAmount * (1d + PerLevelCoefficient * level);
            int result = (int)System.Math.Floor(scaled);
            return result < baseAmount ? baseAmount : result;
        }
    }
}
