using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Quests.Rewards;
using CindarsHope.World.Calendar;
using CindarsHope.World.Events;

namespace CindarsHope.Quests.FestivalQuests
{
    /// <summary>
    /// fable_53 — kind of objective each of the 8 festival quests (fq_*) maps to. Every kind reuses an
    /// EXISTING gameplay system (QUEST_CATALOG Parte F / spec escopo) — there is no new minigame class.
    /// </summary>
    public enum FestivalQuestKind
    {
        /// <summary>fq_plantio — plant N Thandra seeds during the festival (PlantCrop, F15).</summary>
        Planting = 0,

        /// <summary>fq_caravana — light escort: walk the caravan through checkpoints with 1-2 spawned encounters.</summary>
        CaravanEscort = 1,

        /// <summary>fq_luas — collect 3 echoes, one under each moon, same night (NightEchoTracker + F37 moons).</summary>
        NightEchoes = 2,

        /// <summary>fq_torneio — win 3 arena duels (existing encounters; DefeatEnemy count).</summary>
        ArenaDuels = 3,

        /// <summary>fq_colheita — deliver a Gold-quality crop (CropQualityResolver F15/F32).</summary>
        HarvestGold = 4,

        /// <summary>fq_veus — buy/find the "item that does not exist" at the night market (BuyItem, F19).</summary>
        VeilMarket = 5,

        /// <summary>fq_vigilia — interact with the Fountain at night and keep the vigil (Fonte F17; no combat).</summary>
        FountainVigil = 6,

        /// <summary>fq_anonovo — deliver gifts to 5 distinct NPCs before midnight (GiftCountTracker + F26 gift event).</summary>
        NewYearGifts = 7
    }

    /// <summary>
    /// fable_53 — a canonical festival quest definition. Pure data (no Unity refs): which festival
    /// offers it, which existing system completes it, the objective target/quantity, the base XP/gold
    /// the reward scaling (single point fable_34) acts on, and the optional one-time trophy flag.
    /// </summary>
    public sealed class FestivalQuestDefinitionData
    {
        /// <summary>Canonical fq_* id (never rename — anti-regression).</summary>
        public string QuestId { get; set; }

        /// <summary>Canonical festival id from F37 (e.g. "festival_plantio"). Single gating source.</summary>
        public string FestivalId { get; set; }

        public FestivalQuestKind Kind { get; set; }

        /// <summary>Objective target id (seed/crop/enemy band/item/npc/fountain). "any" where the kind allows.</summary>
        public string TargetId { get; set; }

        /// <summary>Required objective amount (seeds planted, duels won, gifts given, …).</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>Base XP before fable_34 scaling. Scaled with QuestLevel = player level at accept.</summary>
        public int BaseXp { get; set; }

        /// <summary>Base gold before fable_34 scaling.</summary>
        public int BaseGold { get; set; }

        /// <summary>Optional one-time festival item from the catalog (fable_32), granted every completion.</summary>
        public string RewardItemId { get; set; }

        /// <summary>Optional trophy/title flag granted ONLY on the first ever completion (idempotent across years).</summary>
        public string TrophyFlagId { get; set; }
    }

    /// <summary>
    /// fable_53 — pure, deterministic AUTHORING layer for the 8 canonical festival quests (fq_*).
    ///
    /// REUSE, not a second system: this only builds fable_34 <see cref="QuestInstance"/>s that flow
    /// through the existing QuestService (accept/progress/turn-in/save) and the single reward scaling
    /// point (<see cref="QuestRewardScaling"/>). It owns no quest state, no second registry, no second
    /// festival calendar (festival ids come from F37 <see cref="WorldEventDefinitions.Festivals"/>),
    /// and no second reward formula. <see cref="FestivalQuestService"/> is the thin runtime adapter.
    ///
    /// Annual repetition: the concrete instance id is per year (<see cref="InstanceId"/> =
    /// <c>fq_*_y&lt;year&gt;</c>) so completing in year 1 never blocks year 2. The trophy flag is
    /// attached only when not yet granted, so the title/trophy is one-time across all years.
    /// </summary>
    public static class FestivalQuestCatalog
    {
        /// <summary>Annual repetition flag key: fq_&lt;id&gt;_done_year_&lt;n&gt; (QuestFlagService / save_rules).</summary>
        public static string DoneYearFlag(string questId, int year) => questId + "_done_year_" + year;

        /// <summary>Per-year concrete instance id (annual repetition key — distinct quest per year).</summary>
        public static string InstanceId(string questId, int year) => questId + "_y" + year;

        /// <summary>Canonical fq_* ids (never rename).</summary>
        public const string PlantingId = "fq_plantio";
        public const string CaravanId = "fq_caravana";
        public const string MoonsId = "fq_luas";
        public const string TournamentId = "fq_torneio";
        public const string HarvestId = "fq_colheita";
        public const string VeilsId = "fq_veus";
        public const string VigilId = "fq_vigilia";
        public const string NewYearId = "fq_anonovo";

        /// <summary>The 8 canonical festival quests, one per F37 festival (QUEST_CATALOG Parte F).</summary>
        public static readonly IReadOnlyList<FestivalQuestDefinitionData> Definitions = new List<FestivalQuestDefinitionData>
        {
            new FestivalQuestDefinitionData
            {
                QuestId = PlantingId, FestivalId = "festival_plantio", Kind = FestivalQuestKind.Planting,
                TargetId = "seed_thandra", Quantity = 5, BaseXp = 60, BaseGold = 40,
                RewardItemId = "item_festival_cake", TrophyFlagId = null
            },
            new FestivalQuestDefinitionData
            {
                QuestId = CaravanId, FestivalId = "festival_caravana", Kind = FestivalQuestKind.CaravanEscort,
                TargetId = "caravan_escort", Quantity = 2, BaseXp = 80, BaseGold = 60,
                RewardItemId = null, TrophyFlagId = null
            },
            new FestivalQuestDefinitionData
            {
                QuestId = MoonsId, FestivalId = "festival_luas", Kind = FestivalQuestKind.NightEchoes,
                TargetId = "moon_echo", Quantity = 3, BaseXp = 90, BaseGold = 50,
                RewardItemId = "item_gift_moon_charm", TrophyFlagId = null
            },
            new FestivalQuestDefinitionData
            {
                QuestId = TournamentId, FestivalId = "festival_torneio", Kind = FestivalQuestKind.ArenaDuels,
                TargetId = "arena_duelist", Quantity = 3, BaseXp = 110, BaseGold = 70,
                RewardItemId = null, TrophyFlagId = "fq_torneio_trophy"
            },
            new FestivalQuestDefinitionData
            {
                QuestId = HarvestId, FestivalId = "festival_colheita", Kind = FestivalQuestKind.HarvestGold,
                TargetId = "item_crop_gold_quality", Quantity = 1, BaseXp = 90, BaseGold = 80,
                RewardItemId = "item_festival_cake", TrophyFlagId = "fq_colheita_trophy"
            },
            new FestivalQuestDefinitionData
            {
                QuestId = VeilsId, FestivalId = "festival_veus", Kind = FestivalQuestKind.VeilMarket,
                TargetId = "item_veil_that_does_not_exist", Quantity = 1, BaseXp = 80, BaseGold = 60,
                RewardItemId = null, TrophyFlagId = null
            },
            new FestivalQuestDefinitionData
            {
                QuestId = VigilId, FestivalId = "festival_vigilia", Kind = FestivalQuestKind.FountainVigil,
                TargetId = "fonte_anya", Quantity = 1, BaseXp = 70, BaseGold = 30,
                RewardItemId = null, TrophyFlagId = null
            },
            new FestivalQuestDefinitionData
            {
                QuestId = NewYearId, FestivalId = "festival_ano_novo", Kind = FestivalQuestKind.NewYearGifts,
                TargetId = "npc_gift", Quantity = 5, BaseXp = 100, BaseGold = 50,
                RewardItemId = "item_festival_cake", TrophyFlagId = null
            },
        };

        /// <summary>Finds the fq_* definition offered by a festival id, or null when that festival has none.</summary>
        public static FestivalQuestDefinitionData FindByFestival(string festivalId)
        {
            if (string.IsNullOrEmpty(festivalId)) return null;
            foreach (var def in Definitions)
            {
                if (def.FestivalId == festivalId) return def;
            }
            return null;
        }

        /// <summary>Finds a fq_* definition by its canonical quest id.</summary>
        public static FestivalQuestDefinitionData FindByQuestId(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return null;
            foreach (var def in Definitions)
            {
                if (def.QuestId == questId) return def;
            }
            return null;
        }

        /// <summary>Calendar year for an absolute day (matches GameDate.Year — 112-day year).</summary>
        public static int YearForDay(int absoluteDay)
        {
            int d = absoluteDay < 1 ? 1 : absoluteDay;
            return (d - 1) / GameDate.DaysPerYear + 1;
        }

        /// <summary>
        /// fable_34 objective type the kind maps to. NightEchoes/ArenaDuels/FountainVigil/NewYearGifts
        /// are driven by trackers/event hooks via MarkObjectiveComplete or count progress, so they use
        /// a neutral count objective that the service progresses; the others ride existing event hooks.
        /// </summary>
        public static QuestObjectiveType ObjectiveTypeFor(FestivalQuestKind kind)
        {
            switch (kind)
            {
                case FestivalQuestKind.Planting: return QuestObjectiveType.PlantCrop;
                case FestivalQuestKind.CaravanEscort: return QuestObjectiveType.DefeatEnemy;
                case FestivalQuestKind.NightEchoes: return QuestObjectiveType.InteractWithObject;
                case FestivalQuestKind.ArenaDuels: return QuestObjectiveType.DefeatEnemy;
                case FestivalQuestKind.HarvestGold: return QuestObjectiveType.DeliverItem;
                case FestivalQuestKind.VeilMarket: return QuestObjectiveType.BuyItem;
                case FestivalQuestKind.FountainVigil: return QuestObjectiveType.InteractWithObject;
                case FestivalQuestKind.NewYearGifts: return QuestObjectiveType.InteractWithObject;
                default: return QuestObjectiveType.InteractWithObject;
            }
        }

        /// <summary>
        /// Builds the per-year fable_34 <see cref="QuestInstance"/> for a festival quest. Rewards =
        /// scaled XP + gold (single point fable_34, QuestLevel = player level at accept) + optional
        /// festival item every year + optional trophy flag only on the first ever completion
        /// (controlled by <paramref name="trophyAlreadyGranted"/>). Source = Npc (festival organizer).
        /// </summary>
        public static QuestInstance BuildInstance(FestivalQuestDefinitionData def, int year, int playerLevel,
            bool trophyAlreadyGranted)
        {
            if (def == null) return null;

            int level = playerLevel < 1 ? 1 : playerLevel;
            int qty = def.Quantity < 1 ? 1 : def.Quantity;
            string questId = InstanceId(def.QuestId, year);

            var extras = new List<QuestRewardDefinition>();
            if (!string.IsNullOrEmpty(def.RewardItemId))
            {
                extras.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + questId + "_item",
                    RewardType = QuestRewardType.Item,
                    TargetId = def.RewardItemId,
                    Quantity = 1,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                });
            }

            // The trophy flag is one-time across ALL years: attach it only while it has not been
            // granted yet. TrackByFlagId keeps it idempotent even within a single turn-in.
            if (!string.IsNullOrEmpty(def.TrophyFlagId) && !trophyAlreadyGranted)
            {
                extras.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + questId + "_trophy",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = def.TrophyFlagId,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                });
            }

            return new QuestInstance
            {
                QuestId = questId,
                QuestTemplateId = def.QuestId,
                Source = QuestSource.Npc,
                TargetId = def.TargetId,
                Quantity = qty,
                QuestLevel = level,
                RewardGold = QuestRewardScaling.Scale(def.BaseGold, level),
                RewardXp = QuestRewardScaling.Scale(def.BaseXp, level),
                GeneratedForDay = 0,
                AdditionalRewards = extras.Count > 0 ? extras : null
            };
        }
    }
}
