using System.Collections.Generic;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.CaveContracts
{
    /// <summary>
    /// fable_51 — kind of Zrix cave contract (QUEST_CATALOG §11).
    /// </summary>
    public enum CaveContractKind
    {
        /// <summary>cc_depth_N — "First to depth N" milestone (1×, XP + map segment).</summary>
        DepthMilestone = 0,

        /// <summary>cc_boss_rematch — weekly: re-defeat a gate boss already defeated (guaranteed essence).</summary>
        BossRematch = 1,

        /// <summary>cc_no_hit_floor — weekly: clear one eligible level without taking damage (title + charm).</summary>
        NoHitFloor = 2
    }

    /// <summary>
    /// fable_51 — pure, deterministic catalog of the 8 canonical cave contracts (cc_*).
    ///
    /// This is the AUTHORING + DETERMINISM layer only — it does NOT touch Unity, the event bus,
    /// or the quest registry. <see cref="CaveContractService"/> turns these definitions into
    /// fable_34 <c>QuestInstance</c>s that flow through the single existing quest flow.
    ///
    /// Determinism (rng-and-determinism / ADR-0005 generalized): weekly target selection is a pure
    /// function of (worldSeed, week) via <see cref="QuestStableHash"/> — no Unity Random / GUID /
    /// timestamp. The same (worldSeed, week) always yields the same boss / no-hit target.
    /// </summary>
    public static class CaveContractCatalog
    {
        public const string MilestonePrefix = "cc_depth_";
        public const string BossRematchId = "cc_boss_rematch";
        public const string NoHitFloorId = "cc_no_hit_floor";
        public const string NoHitCharmItemId = "item_accessory_charm_no_hit";

        // StableHash salts — distinct per weekly channel so the two weeklies never correlate.
        public const string BossRematchSalt = "fable_51_cc_boss_rematch_v1";
        public const string NoHitFloorSalt = "fable_51_cc_no_hit_floor_v1";

        /// <summary>The six canonical depth milestones (1×). IDs are canonical — never rename.</summary>
        public static readonly IReadOnlyList<int> MilestoneDepths = new[] { 5, 15, 30, 50, 70, 90 };

        /// <summary>cc_depth_N id for a milestone depth.</summary>
        public static string MilestoneId(int depth) => MilestonePrefix + depth;

        /// <summary>map_segment_&lt;band&gt; flag granted by a depth milestone (consumed by F38 minimap later).</summary>
        public static string MapSegmentFlag(int depth) => "map_segment_" + DepthBand(depth);

        /// <summary>title_no_hit_&lt;n&gt; stable flag granted by the no-hit contract.</summary>
        public static string NoHitTitleFlag(int level) => "title_no_hit_" + level;

        /// <summary>Coarse band label for the "map segment" flag (shallow/mid/deep/abyss).</summary>
        public static string DepthBand(int depth)
        {
            if (depth <= 15) return "shallow";
            if (depth <= 50) return "mid";
            if (depth <= 90) return "deep";
            return "abyss";
        }

        // ─── Depth milestone definitions (1× fixed quests, modeled as F34 instances) ───────────

        /// <summary>
        /// Base XP for a depth milestone before fable_34 scaling. Deeper milestones are worth more
        /// (QUEST_CATALOG §11: "XP alto"); the final scaled amount uses QuestLevel = N.
        /// </summary>
        public static int MilestoneBaseXp(int depth) => 40 + depth * 4;

        /// <summary>
        /// Builds the deterministic <c>QuestInstance</c> for a depth milestone. Objective = reach
        /// CaveLevel N (auto-completed by QuestService.OnCaveLevelEntered). Rewards = scaled XP
        /// (single point fable_34) + map-segment flag. Idempotent: a completed quest never repeats.
        /// </summary>
        public static QuestInstance BuildMilestoneInstance(int depth)
        {
            int baseXp = MilestoneBaseXp(depth);
            return new QuestInstance
            {
                QuestId = MilestoneId(depth),
                QuestTemplateId = MilestoneId(depth),
                Source = QuestSource.CaveContract,
                TargetId = depth.ToString(),            // OnCaveLevelEntered matches int target <= caveLevel
                Quantity = 1,
                QuestLevel = depth,
                RewardGold = 0,                          // milestones reward XP + map segment, not gold
                RewardXp = QuestRewardScaling.Scale(baseXp, depth),
                GeneratedForDay = 0,                     // not day-bound (1× milestone)
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    new QuestRewardDefinition
                    {
                        RewardId = "reward_" + MilestoneId(depth) + "_map",
                        RewardType = QuestRewardType.QuestFlagGrant,
                        GrantedFlagId = MapSegmentFlag(depth),
                        IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                    }
                }
            };
        }

        // ─── Weekly: boss rematch ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Deterministically picks the rematch boss for a week, among the gate bosses ALREADY
        /// defeated. Returns null when no gate has been defeated yet (contract unavailable).
        /// Same (worldSeed, week) + same eligible set ⇒ same boss (StableHash, no Random).
        /// </summary>
        public static string SelectRematchBoss(string worldSeed, int week, IReadOnlyList<string> defeatedGateBossIds)
        {
            if (defeatedGateBossIds == null || defeatedGateBossIds.Count == 0) return null;

            // Stable, order-independent: sort the eligible ids, then pick by StableHash bucket.
            var sorted = new List<string>(defeatedGateBossIds);
            sorted.Sort(System.StringComparer.Ordinal);

            int idx = StableIndex(BossRematchSalt, worldSeed, week, sorted.Count);
            return sorted[idx];
        }

        /// <summary>
        /// Builds the weekly boss-rematch instance. Objective = defeat the chosen boss band again.
        /// Reward = guaranteed band essence (item, fable_22/F32). Returns null when no eligible boss.
        /// </summary>
        public static QuestInstance BuildBossRematchInstance(string worldSeed, int week,
            IReadOnlyList<string> defeatedGateBossIds, int playerLevel)
        {
            var bossId = SelectRematchBoss(worldSeed, week, defeatedGateBossIds);
            if (string.IsNullOrEmpty(bossId)) return null;

            int level = playerLevel < 1 ? 1 : playerLevel;
            return new QuestInstance
            {
                QuestId = WeeklyInstanceId(BossRematchId, week),
                QuestTemplateId = BossRematchId,
                Source = QuestSource.CaveContract,
                TargetId = bossId,                       // OnEnemyKilled matches this band
                Quantity = 1,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(90, level),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    new QuestRewardDefinition
                    {
                        RewardId = BossEssenceRewardId(WeeklyInstanceId(BossRematchId, week)),
                        RewardType = QuestRewardType.Item,
                        TargetId = EssenceItemForBoss(bossId),
                        Quantity = 1,
                        IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                    }
                }
            };
        }

        /// <summary>Guaranteed essence item id for a gate boss band (item catalog fable_32).</summary>
        public static string EssenceItemForBoss(string bossId) => "item_essence_" + (bossId ?? "gate");
        public static string BossEssenceRewardId(string questId) => "reward_" + questId + "_essence";
        public static string NoHitTitleRewardId(string questId) => "reward_" + questId + "_title";
        public static string NoHitCharmRewardId(string questId) => "reward_" + questId + "_charm";

        // ─── Weekly: no-hit floor ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Deterministically picks the eligible no-hit target level for a week, within the player's
        /// current band [bandMin, bandMax]. Same (worldSeed, week, band) ⇒ same level.
        /// </summary>
        public static int SelectNoHitLevel(string worldSeed, int week, int bandMin, int bandMax)
        {
            if (bandMax < bandMin) bandMax = bandMin;
            int span = bandMax - bandMin + 1;
            int offset = StableIndex(NoHitFloorSalt, worldSeed, week, span);
            return bandMin + offset;
        }

        /// <summary>
        /// Builds the weekly no-hit instance. The objective is NOT count-based — completion is
        /// driven by <see cref="NoHitFloorTracker"/> on a clean clear, via
        /// <c>QuestService.MarkObjectiveComplete</c>. Reward = title flag + charm accessory.
        /// </summary>
        public static QuestInstance BuildNoHitInstance(string worldSeed, int week, int bandMin, int bandMax,
            int playerLevel)
        {
            int level = SelectNoHitLevel(worldSeed, week, bandMin, bandMax);
            int qLevel = playerLevel < 1 ? 1 : playerLevel;
            return new QuestInstance
            {
                QuestId = WeeklyInstanceId(NoHitFloorId, week),
                QuestTemplateId = NoHitFloorId,
                Source = QuestSource.CaveContract,
                TargetId = level.ToString(),             // the eligible level for this week
                Quantity = 1,
                QuestLevel = qLevel,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(70, qLevel),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    new QuestRewardDefinition
                    {
                        RewardId = NoHitTitleRewardId(WeeklyInstanceId(NoHitFloorId, week)),
                        RewardType = QuestRewardType.QuestFlagGrant,
                        GrantedFlagId = NoHitTitleFlag(level),
                        IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                    },
                    new QuestRewardDefinition
                    {
                        RewardId = NoHitCharmRewardId(WeeklyInstanceId(NoHitFloorId, week)),
                        RewardType = QuestRewardType.Item,
                        TargetId = NoHitCharmItemId,
                        Quantity = 1,
                        IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                    }
                }
            };
        }

        // ─── Shared helpers ─────────────────────────────────────────────────────────────────────

        /// <summary>Concrete instance id for a weekly contract (stable per week — no day churn).</summary>
        public static string WeeklyInstanceId(string templateId, int week) => templateId + "_w" + week;

        /// <summary>
        /// Stable, unbiased index in [0, count) from (salt, seed, week) via the shared FNV hash.
        /// </summary>
        public static int StableIndex(string salt, string seed, int week, int count)
        {
            if (count <= 0) return 0;
            long h = (uint)QuestStableHash.Compute($"{salt}|{seed}|{week}");
            return (int)(h % count);
        }
    }
}
