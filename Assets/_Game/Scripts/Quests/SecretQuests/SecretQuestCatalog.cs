using System.Collections.Generic;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 — the 8 canonical cave-secret quests (scq_*) from QUEST_CATALOG_DIRECTION §12
    /// (decision Q6.1 — "a caverna tambem pede"). All have NO marker and are discovered only by
    /// conversing/offering/hesitating before attacking; they ALIMENTAM the main act 3 (engraving,
    /// thrall name) without ever being mandatory (§13).
    ///
    /// This is the AUTHORING + DETERMINISM layer only — it does NOT touch Unity, the event bus, or
    /// the quest registry directly. Each definition is turned into a fable_34
    /// <see cref="QuestInstance"/> that flows through the EXISTING <see cref="Runtime.QuestService"/>
    /// (single accept / progress / turn-in / save). XP/gold scaling is the single fable_34 point
    /// (<see cref="QuestRewardScaling"/>); items/flags ride the SAME reward applicator via
    /// <see cref="QuestInstance.AdditionalRewards"/>. No second registry, no second reward formula.
    ///
    /// Determinism (rng-and-determinism / ADR-0005 generalized): the merchant offers the 3 lists in a
    /// fixed sequence, each 1x per run, stable per seed — no Unity Random / GUID / timestamp.
    /// </summary>
    public static class SecretQuestCatalog
    {
        // ─── Canonical scq_* ids (NEVER rename — QUEST_CATALOG §12) ──────────────────────────────
        public const string ScroungerBargainId = "scq_scrounger_bargain";
        public const string MerchantList1Id = "scq_merchant_list_1";
        public const string MerchantList2Id = "scq_merchant_list_2";
        public const string MerchantList3Id = "scq_merchant_list_3";
        public const string WardenOfferingId = "scq_warden_offering";
        public const string GoblinTruceId = "scq_goblin_truce";
        public const string ThrallNameId = "scq_thrall_name";
        public const string DragonEggId = "scq_dragon_egg";

        /// <summary>The 8 canonical secret quest ids, in catalog order.</summary>
        public static readonly IReadOnlyList<string> AllQuestIds = new[]
        {
            ScroungerBargainId, MerchantList1Id, MerchantList2Id, MerchantList3Id,
            WardenOfferingId, GoblinTruceId, ThrallNameId, DragonEggId
        };

        /// <summary>The 3 merchant lists in the fixed sequence the wandering merchant offers them.</summary>
        public static readonly IReadOnlyList<string> MerchantListSequence = new[]
        {
            MerchantList1Id, MerchantList2Id, MerchantList3Id
        };

        // ─── World-effect flag ids (granted on turn-in; persisted by QuestFlagService) ────────────
        public const string MerchantDiscountFlag = "merchant_discount_10";
        public const string WardenPeacefulFlag = "warden_peaceful_forever";
        public const string GoblinBandNeutralFlag = "goblin_band_neutral_run";
        public const string WarchiefCrestFlag = "warchief_crest_owned";
        public const string NymirianEngravingFlag = "nymirian_engraving_owned";
        public const string ThrallNameKnownFlag = "thrall_name_known";
        public const string DragonEggIncubatingFlag = "dragon_egg_incubating";
        public const string DragonEggHatchedFlag = "dragon_egg_hatched";

        /// <summary>scrounger_vendor_unlocked_&lt;level&gt; flag — the Scrounger King becomes a fixed vendor.</summary>
        public static string ScroungerVendorFlag(int caveLevel) => "scrounger_vendor_unlocked_" + caveLevel;

        // ─── Reward item ids (item catalog fable_32, by id) ──────────────────────────────────────
        public const string ScroungerRingItem = "item_accessory_scrounger_ring";
        public const string WarchiefCrestItem = "item_accessory_warchief_crest";

        /// <summary>The catalog ingredient + amount each merchant list asks for (5 glowcap / 3 frost_core / 1 wyrmling_scale).</summary>
        public static (string ItemId, int Amount) MerchantListRequirement(string questId)
        {
            switch (questId)
            {
                case MerchantList1Id: return ("item_material_glowcap", 5);
                case MerchantList2Id: return ("item_material_frost_core", 3);
                case MerchantList3Id: return ("item_material_wyrmling_scale", 1);
                default: return (null, 0);
            }
        }

        /// <summary>Per-merchant-list rare reward item (1 per list, on top of the permanent discount).</summary>
        public static string MerchantListRewardItem(string questId)
        {
            switch (questId)
            {
                case MerchantList1Id: return "item_consumable_potion_hp_large";
                case MerchantList2Id: return "item_material_frost_core";
                case MerchantList3Id: return "item_accessory_wyrmling_charm";
                default: return null;
            }
        }

        // ─── Builders (one per quest; pure, deterministic) ────────────────────────────────────────

        /// <summary>
        /// scq_scrounger_bargain — deliver 3 Rare+ items to the Old Scrounger King ⇒ ring + he becomes
        /// a FIXED vendor of that cave level. Objective is a count of 3 (rarity gate enforced on
        /// delivery by <see cref="SecretQuestConditions"/>, not by the generic CollectItem objective).
        /// </summary>
        public static QuestInstance BuildScroungerBargain(int caveLevel, int questLevel)
        {
            int level = NormalizeLevel(questLevel);
            return new QuestInstance
            {
                QuestId = ScroungerBargainId,
                QuestTemplateId = ScroungerBargainId,
                Source = QuestSource.CaveSecret,
                TargetId = "rare_plus_item",
                Quantity = 3,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(120, level),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    ItemReward(ScroungerBargainId, "ring", ScroungerRingItem),
                    FlagReward(ScroungerBargainId, "vendor", ScroungerVendorFlag(caveLevel))
                }
            };
        }

        /// <summary>
        /// scq_merchant_list_{1,2,3} — bring the wandering merchant N of an ingredient ⇒ permanent 10%
        /// discount (granted once; the flag is shared across the 3 lists) + a rare item. Each list is
        /// offered 1x per run in sequence (CaveWanderingMerchant owns the sequencing).
        /// </summary>
        public static QuestInstance BuildMerchantList(string questId, int questLevel)
        {
            var req = MerchantListRequirement(questId);
            if (req.ItemId == null) return null;

            int level = NormalizeLevel(questLevel);
            var rewards = new List<QuestRewardDefinition>
            {
                // Permanent discount: shared flag id, idempotent by flag — completing a 2nd/3rd list
                // never re-applies or stacks the discount.
                new QuestRewardDefinition
                {
                    RewardId = "reward_" + questId + "_discount",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = MerchantDiscountFlag,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            var rareItem = MerchantListRewardItem(questId);
            if (!string.IsNullOrEmpty(rareItem)) rewards.Add(ItemReward(questId, "rare", rareItem));

            return new QuestInstance
            {
                QuestId = questId,
                QuestTemplateId = questId,
                Source = QuestSource.CaveSecret,
                TargetId = req.ItemId,
                Quantity = req.Amount,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(60, level),
                GeneratedForDay = 0,
                AdditionalRewards = rewards
            };
        }

        /// <summary>
        /// scq_warden_offering — offer Agua Viva to the Silence Warden ⇒ he is peaceful FOREVER +
        /// nymirian_engraving (feeds act 3, never required). Objective: deliver 1 item_agua_viva.
        /// </summary>
        public static QuestInstance BuildWardenOffering(int questLevel)
        {
            int level = NormalizeLevel(questLevel);
            return new QuestInstance
            {
                QuestId = WardenOfferingId,
                QuestTemplateId = WardenOfferingId,
                Source = QuestSource.CaveSecret,
                TargetId = "item_agua_viva",
                Quantity = 1,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(100, level),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    FlagReward(WardenOfferingId, "peaceful", WardenPeacefulFlag),
                    FlagReward(WardenOfferingId, "engraving", NymirianEngravingFlag)
                }
            };
        }

        /// <summary>
        /// scq_goblin_truce — win the Warchief duel WITHOUT the pack dying ⇒ neutral band for 1 run +
        /// warchief_crest (the farm goblin mentions your name). Completion is driven by
        /// <see cref="PackDuelTracker"/>, not a count objective; TargetId pins the duel encounter id.
        /// </summary>
        public static QuestInstance BuildGoblinTruce(int questLevel)
        {
            int level = NormalizeLevel(questLevel);
            return new QuestInstance
            {
                QuestId = GoblinTruceId,
                QuestTemplateId = GoblinTruceId,
                Source = QuestSource.CaveSecret,
                TargetId = "goblin_warchief_duel",
                Quantity = 1,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(110, level),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    // Neutral band is a RUN flag (cleared on a new run); crest is a permanent item.
                    FlagReward(GoblinTruceId, "neutral", GoblinBandNeutralFlag),
                    FlagReward(GoblinTruceId, "crestflag", WarchiefCrestFlag),
                    ItemReward(GoblinTruceId, "crest", WarchiefCrestItem)
                }
            };
        }

        /// <summary>
        /// scq_thrall_name — a purified Thrall whispers a name ⇒ take it to Mara (act-3 lore). Completion
        /// is driven by talking to Mara (TalkToNpc), granting the thrall_name_known flag.
        /// </summary>
        public static QuestInstance BuildThrallName(int questLevel)
        {
            int level = NormalizeLevel(questLevel);
            return new QuestInstance
            {
                QuestId = ThrallNameId,
                QuestTemplateId = ThrallNameId,
                Source = QuestSource.CaveSecret,
                TargetId = "npc_mara",
                Quantity = 1,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(90, level),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    FlagReward(ThrallNameId, "name", ThrallNameKnownFlag)
                }
            };
        }

        /// <summary>
        /// scq_dragon_egg — the cold egg in the Ashwing nest ⇒ incubate at Nimble's (cosmetic in v1;
        /// future-pets hook). Completion is driven by the incubation timer (<see cref="DragonEggIncubator"/>),
        /// granting the hatched flag. Granting incubating is handled at quest accept, not turn-in.
        /// </summary>
        public static QuestInstance BuildDragonEgg(int questLevel)
        {
            int level = NormalizeLevel(questLevel);
            return new QuestInstance
            {
                QuestId = DragonEggId,
                QuestTemplateId = DragonEggId,
                Source = QuestSource.CaveSecret,
                TargetId = "nimble_incubator",
                Quantity = 1,
                QuestLevel = level,
                RewardGold = 0,
                RewardXp = QuestRewardScaling.Scale(80, level),
                GeneratedForDay = 0,
                AdditionalRewards = new List<QuestRewardDefinition>
                {
                    FlagReward(DragonEggId, "hatched", DragonEggHatchedFlag)
                }
            };
        }

        /// <summary>Builds the instance for any canonical scq_* id (used by the offer points / tests).</summary>
        public static QuestInstance Build(string questId, int caveLevel, int questLevel)
        {
            switch (questId)
            {
                case ScroungerBargainId: return BuildScroungerBargain(caveLevel, questLevel);
                case MerchantList1Id:
                case MerchantList2Id:
                case MerchantList3Id: return BuildMerchantList(questId, questLevel);
                case WardenOfferingId: return BuildWardenOffering(questLevel);
                case GoblinTruceId: return BuildGoblinTruce(questLevel);
                case ThrallNameId: return BuildThrallName(questLevel);
                case DragonEggId: return BuildDragonEgg(questLevel);
                default: return null;
            }
        }

        public static bool IsCanonical(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return false;
            foreach (var id in AllQuestIds) if (id == questId) return true;
            return false;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────────────────────

        private static int NormalizeLevel(int questLevel) => questLevel < 1 ? 1 : questLevel;

        private static QuestRewardDefinition ItemReward(string questId, string suffix, string itemId) =>
            new QuestRewardDefinition
            {
                RewardId = "reward_" + questId + "_" + suffix,
                RewardType = QuestRewardType.Item,
                TargetId = itemId,
                Quantity = 1,
                IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
            };

        private static QuestRewardDefinition FlagReward(string questId, string suffix, string flagId) =>
            new QuestRewardDefinition
            {
                RewardId = "reward_" + questId + "_" + suffix,
                RewardType = QuestRewardType.QuestFlagGrant,
                GrantedFlagId = flagId,
                IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
            };
    }
}
