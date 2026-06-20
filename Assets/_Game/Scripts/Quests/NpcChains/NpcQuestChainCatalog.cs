using System.Collections.Generic;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.NpcChains
{
    /// <summary>
    /// fable_35 — the kind of objective each side-quest step maps to. Every kind reuses an EXISTING
    /// <see cref="QuestObjectiveType"/> (QUEST_CATALOG Parte C §9 / spec viability matrix) — this spec
    /// creates NO new objective type. Where the catalog's narrative had no viable existing type the
    /// objective is ADAPTED to the nearest existing kind (e.g. "escort/accompany" → TalkToNpc at the
    /// location, "investigate" → ReachLocation) keeping the catalog wording in the localized text.
    /// </summary>
    public enum NpcChainObjectiveKind
    {
        /// <summary>Collect/gather N items (CollectItem) — auto-progressed by inventory count.</summary>
        Collect = 0,

        /// <summary>Deliver a specific item / set of forms (DeliverItem) — MarkObjectiveComplete on turn-in.</summary>
        Deliver = 1,

        /// <summary>Talk to / accompany an NPC (TalkToNpc) — adapts "accompany the round", "long dialogue".</summary>
        Talk = 2,

        /// <summary>Reach / investigate a location or cave level (ReachLocation) — adapts "mark levels", "investigate".</summary>
        Reach = 3,

        /// <summary>Defeat an enemy band/boss N times (DefeatEnemy) — count-based.</summary>
        Defeat = 4,

        /// <summary>Harvest crops (HarvestCrop) — Sylveth chain.</summary>
        Harvest = 5,

        /// <summary>Plant a crop (PlantCrop) — Sylveth q3 (plant shadowroot in the cave).</summary>
        Plant = 6,

        /// <summary>Craft an item (CraftItem) — Corvus q1 (craft candles).</summary>
        Craft = 7
    }

    /// <summary>
    /// fable_35 — one step of an NPC side-quest chain (a single <c>sq_&lt;npc&gt;_&lt;n&gt;</c> quest).
    /// Pure data (no Unity refs): which NPC offers it, the canonical id, the objective kind/target/amount,
    /// the reference level the reward scaling acts on, the base gold/item reward, the cross-act gate (when
    /// the catalog cites an act), and the optional service-unlock flag the final step grants.
    /// </summary>
    public sealed class NpcChainStepData
    {
        /// <summary>Owning NPC id (canonical roster id, e.g. "npc_brumdar").</summary>
        public string NpcId { get; set; }

        /// <summary>Canonical sq_&lt;npc&gt;_&lt;n&gt; id (never rename — anti-regression).</summary>
        public string QuestId { get; set; }

        /// <summary>1-based step index within the chain (1 = domestic intro, 3 = milestone).</summary>
        public int Step { get; set; }

        /// <summary>Catalog reference level (QUEST_CATALOG §9 number) — drives reward scaling (QuestLevel).</summary>
        public int ReferenceLevel { get; set; }

        public NpcChainObjectiveKind ObjectiveKind { get; set; }

        /// <summary>Objective target id (item id, npc id, enemy band id, location/level id, crop id).</summary>
        public string TargetId { get; set; }

        /// <summary>Required objective amount (items collected, enemies defeated, crops harvested, …).</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>Base gold before fable_34 scaling. Side rewards are modest gold + the next step + amizade.</summary>
        public int BaseGold { get; set; }

        /// <summary>Base XP before fable_34 scaling.</summary>
        public int BaseXp { get; set; }

        /// <summary>Optional item reward id (item catalog fable_32), granted on turn-in.</summary>
        public string RewardItemId { get; set; }

        /// <summary>
        /// Optional cross-act gate flag the step REQUIRES before it can be offered (QUEST_CATALOG §253:
        /// "Têmpera ← Brumdar-3 + Ato 1"). Read-only consumption of an act flag set elsewhere (F36).
        /// Null = no act gate.
        /// </summary>
        public string RequiredActFlagId { get; set; }

        /// <summary>
        /// Optional service-unlock flag this step GRANTS on turn-in (final steps only). Forward-declared
        /// stable flag consumed by the owning system later (e.g. F22 tempering, F07 scrolls, F41 lot
        /// charters, F25 services). Null for non-final steps.
        /// </summary>
        public string ServiceUnlockFlagId { get; set; }

        /// <summary>True when this step is dormant: the catalog narrative has no viable runtime target yet.
        /// Dormant steps are authored (so nothing is silently cut) but NOT offered until the system exists.
        /// fable_35 v1 has none — every objective mapped to an existing type.</summary>
        public bool Dormant { get; set; }
    }

    /// <summary>
    /// fable_35 — pure, deterministic AUTHORING layer for the 12 canonical NPC side-quest chains
    /// (<c>sq_&lt;npc&gt;_&lt;n&gt;</c>, QUEST_CATALOG Parte C §9). 12 chains × 3 steps = 36 quests.
    ///
    /// REUSE, not a second system (spec §138 "Regras de não duplicação"):
    /// - the quests are fable_34 <see cref="QuestInstance"/>s that flow through the EXISTING QuestService
    ///   (single accept / progress / turn-in / save) and the single reward scaling point
    ///   (<see cref="QuestRewardScaling"/>) — no second registry, no second reward formula;
    /// - the +8 amizade per completed chain quest is granted by the EXISTING FriendshipService hook
    ///   (OnQuestGiverInteracted → RegisterQuestCompleted, idempotent by questId) — this catalog owns
    ///   no second friendship path;
    /// - chaining + composite gates (previous-step done flag + min friendship + act flag) are evaluated
    ///   by <see cref="NpcQuestChainService"/> through QuestService/FriendshipService — no new gate API;
    /// - objective types are mapped to EXISTING <see cref="QuestObjectiveType"/> values — no new type.
    ///
    /// The text (title/description) is addressed by stable LocalizationService ids in the form
    /// <c>quest.&lt;sq_id&gt;.title</c> / <c>quest.&lt;sq_id&gt;.desc</c> (ADR-0012 id→string); the
    /// player-facing strings live in the string table (fable_73), authored in the NPC's voice.
    /// </summary>
    public static class NpcQuestChainCatalog
    {
        public const int StepsPerChain = 3;
        public const int ChainCount = 12;

        /// <summary>Stable "step done" flag set on a quest turn-in; gates the next step. sq_&lt;npc&gt;_&lt;n&gt;_done.</summary>
        public static string DoneFlag(string questId) => questId + "_done";

        /// <summary>Min friendship level required to be offered step 2/3 of a chain (composite gate, F26).</summary>
        public const int ChainProgressFriendshipMin = 2;

        // ── Cross-act gate flags (read-only; set by the main quest system F36) ───────────────────────
        public const string ActOneDoneFlag = "act_1_done";
        public const string ActThreeDoneFlag = "act_3_done";

        // ── LocalizationService id helpers (ADR-0012 / fable_73; {domain}.{id}.{slot}) ───────────────
        public static string TitleKey(string questId) => "quest." + questId + ".title";
        public static string DescKey(string questId) => "quest." + questId + ".desc";
        public static string OfferKey(string questId) => "quest." + questId + ".offer";
        public static string TurnInKey(string questId) => "quest." + questId + ".turnin";

        // ── The 12 chains (QUEST_CATALOG §9). IDs are canonical — never rename. ───────────────────────
        private static readonly List<NpcChainStepData> s_steps = new List<NpcChainStepData>
        {
            // Brumdar (forja) — "o ferro lembra". q3 → Têmpera de Essência (F22), gated by Ato 1 (§253).
            Step("npc_brumdar", "sq_brumdar_1", 1, 8,  NpcChainObjectiveKind.Collect, "iron_ore", 10, 40, 40),
            Step("npc_brumdar", "sq_brumdar_2", 2, 20, NpcChainObjectiveKind.Reach,   "ruin_shallow_anvil", 1, 70, 90),
            Final("npc_brumdar","sq_brumdar_3", 3, 45, NpcChainObjectiveKind.Defeat,  "boss_forge_tyrant", 1, 140, 220,
                  serviceFlag: "service_unlock_brumdar_tempering", requiredAct: ActOneDoneFlag),

            // Ozzra (alquimia) — "tudo borbulha por um motivo". q3 → LearnableScrolls (F07).
            Step("npc_ozzra", "sq_ozzra_1", 1, 6,  NpcChainObjectiveKind.Collect, "glowcap", 5, 35, 35),
            Step("npc_ozzra", "sq_ozzra_2", 2, 28, NpcChainObjectiveKind.Collect, "essence_ice", 2, 90, 120),
            Final("npc_ozzra","sq_ozzra_3", 3, 50, NpcChainObjectiveKind.Defeat,  "band_veilkin_witch", 1, 150, 240,
                  serviceFlag: "service_unlock_ozzra_scrolls"),

            // Thalindra (arquivo) — "a poeira guarda". q3 → análise de amostra 2x/dia (F25/F21).
            Step("npc_thalindra", "sq_thalindra_1", 1, 5,  NpcChainObjectiveKind.Deliver, "library_book_set", 3, 30, 30),
            Step("npc_thalindra", "sq_thalindra_2", 2, 15, NpcChainObjectiveKind.Collect, "creature_sample", 3, 60, 70),
            Final("npc_thalindra","sq_thalindra_3", 3, 40, NpcChainObjectiveKind.Deliver, "nymirian_engraving", 1, 120, 180,
                  serviceFlag: "service_unlock_thalindra_double_analysis"),

            // Sylveth (sementes) — "a terra responde a quem pergunta". q3 → troca 2:1 + sementes de lua.
            Step("npc_sylveth", "sq_sylveth_1", 1, 4,  NpcChainObjectiveKind.Harvest, "any", 10, 25, 25),
            Step("npc_sylveth", "sq_sylveth_2", 2, 18, NpcChainObjectiveKind.Harvest, "alihana_tear", 1, 60, 75),
            Final("npc_sylveth","sq_sylveth_3", 3, 30, NpcChainObjectiveKind.Plant,   "shadowroot", 1, 100, 150,
                  serviceFlag: "service_unlock_sylveth_offseason_trade"),

            // Eiran (animais) — "bicho sente antes da gente". q3 → pensão de animais + filhote raro.
            Step("npc_eiran", "sq_eiran_1", 1, 5,  NpcChainObjectiveKind.Reach,   "cave_entrance_hen", 1, 30, 30),
            Step("npc_eiran", "sq_eiran_2", 2, 16, NpcChainObjectiveKind.Defeat,  "band_grimfang", 5, 60, 75),
            Final("npc_eiran","sq_eiran_3", 3, 28, NpcChainObjectiveKind.Deliver, "premium_feed", 5, 90, 140,
                  serviceFlag: "service_unlock_eiran_animal_boarding"),

            // Gruta (taverna) — "barriga cheia, língua solta". q3 → 2 pratos de buff/dia.
            Step("npc_gruta", "sq_gruta_1", 1, 7,  NpcChainObjectiveKind.Collect, "grape", 5, 35, 35),
            Step("npc_gruta", "sq_gruta_2", 2, 20, NpcChainObjectiveKind.Collect, "mirrorfin", 1, 70, 90),
            Final("npc_gruta","sq_gruta_3", 3, 35, NpcChainObjectiveKind.Deliver, "festival_feast", 1, 110, 170,
                  serviceFlag: "service_unlock_gruta_daily_dishes"),

            // Dagna (pedreira) — "toda pedra tem veio; é só ouvir". q3 → mapa do veio 2x/run.
            Step("npc_dagna", "sq_dagna_1", 1, 6,  NpcChainObjectiveKind.Collect, "stone", 8, 30, 30),
            Step("npc_dagna", "sq_dagna_2", 2, 22, NpcChainObjectiveKind.Reach,   "cave_singing_vein_15", 1, 80, 100),
            Final("npc_dagna","sq_dagna_3", 3, 38, NpcChainObjectiveKind.Collect, "glacier_hide", 1, 120, 180,
                  serviceFlag: "service_unlock_dagna_vein_map"),

            // Zrix (estrada da caverna) — "desça devagar, suba inteiro". q3 → resgate com desconto.
            Step("npc_zrix", "sq_zrix_1", 1, 8,  NpcChainObjectiveKind.Reach,   "cave_mark_levels", 3, 40, 40),
            Step("npc_zrix", "sq_zrix_2", 2, 25, NpcChainObjectiveKind.Reach,   "lost_patrol_kit", 1, 90, 110),
            Final("npc_zrix","sq_zrix_3", 3, 45, NpcChainObjectiveKind.Reach,   "cave_rescue_level_40", 1, 140, 220,
                  serviceFlag: "service_unlock_zrix_rescue_discount"),

            // Hund (guarda) — "ordem é rotina bem feita". q3 → contratos do quadro pagam +20%.
            Step("npc_hund", "sq_hund_1", 1, 6,  NpcChainObjectiveKind.Talk,   "npc_hund_night_round", 1, 30, 35),
            Step("npc_hund", "sq_hund_2", 2, 18, NpcChainObjectiveKind.Reach,  "footprints_investigation", 1, 70, 90),
            Final("npc_hund","sq_hund_3", 3, 50, NpcChainObjectiveKind.Defeat, "boss_packlord_ruvash", 1, 150, 240,
                  serviceFlag: "service_unlock_hund_board_bonus"),

            // Mirela (atelier) — "a costura conta a história do rasgo". q3 → 2º upgrade de mochila.
            Step("npc_mirela", "sq_mirela_1", 1, 5,  NpcChainObjectiveKind.Collect, "fiber", 5, 25, 25),
            Step("npc_mirela", "sq_mirela_2", 2, 24, NpcChainObjectiveKind.Collect, "veil_cloth", 3, 90, 110),
            Final("npc_mirela","sq_mirela_3", 3, 40, NpcChainObjectiveKind.Deliver, "midnight_dress_order", 1, 130, 190,
                  serviceFlag: "service_unlock_mirela_backpack_upgrade"),

            // Tovin (permissões) — "o carimbo protege quem carimba". q3 → alvarás de lote da fazenda (F41).
            Step("npc_tovin", "sq_tovin_1", 1, 6,  NpcChainObjectiveKind.Deliver, "stamped_forms", 1, 30, 35),
            Step("npc_tovin", "sq_tovin_2", 2, 20, NpcChainObjectiveKind.Talk,    "npc_yael", 1, 70, 90),
            Final("npc_tovin","sq_tovin_3", 3, 35, NpcChainObjectiveKind.Talk,    "npc_charter_signatures", 5, 110, 170,
                  serviceFlag: "service_unlock_tovin_farm_charters"),

            // Corvus (templo) — "a Fonte não esqueceu; nós esquecemos dela". q3 → bênção 2 opções/dia; liga Ato 4.
            Step("npc_corvus", "sq_corvus_1", 1, 5,  NpcChainObjectiveKind.Craft, "candle", 5, 30, 30),
            Step("npc_corvus", "sq_corvus_2", 2, 25, NpcChainObjectiveKind.Talk,  "npc_corvus_faith_dialogue", 1, 90, 110),
            Final("npc_corvus","sq_corvus_3", 3, 45, NpcChainObjectiveKind.Reach, "temple_litany_fragment", 1, 140, 220,
                  serviceFlag: "service_unlock_corvus_blessing", requiredAct: ActThreeDoneFlag),
        };

        public static IReadOnlyList<NpcChainStepData> AllSteps => s_steps;

        /// <summary>The 12 owning NPC ids in catalog order (one chain each).</summary>
        public static readonly IReadOnlyList<string> ChainNpcIds = new[]
        {
            "npc_brumdar", "npc_ozzra", "npc_thalindra", "npc_sylveth", "npc_eiran", "npc_gruta",
            "npc_dagna", "npc_zrix", "npc_hund", "npc_mirela", "npc_tovin", "npc_corvus"
        };

        /// <summary>All steps of an NPC's chain, ordered by step index (1..3). Empty when the NPC has none.</summary>
        public static IReadOnlyList<NpcChainStepData> ForNpc(string npcId)
        {
            var list = new List<NpcChainStepData>();
            if (string.IsNullOrEmpty(npcId)) return list;
            foreach (var s in s_steps)
            {
                if (s.NpcId == npcId) list.Add(s);
            }
            list.Sort((a, b) => a.Step.CompareTo(b.Step));
            return list;
        }

        /// <summary>The step with this canonical sq_ id, or null.</summary>
        public static NpcChainStepData FindByQuestId(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return null;
            foreach (var s in s_steps)
            {
                if (s.QuestId == questId) return s;
            }
            return null;
        }

        /// <summary>The step that immediately precedes <paramref name="questId"/> in its chain, or null for step 1.</summary>
        public static NpcChainStepData PreviousStep(string questId)
        {
            var step = FindByQuestId(questId);
            if (step == null || step.Step <= 1) return null;
            foreach (var s in s_steps)
            {
                if (s.NpcId == step.NpcId && s.Step == step.Step - 1) return s;
            }
            return null;
        }

        /// <summary>
        /// fable_35 — the existing <see cref="QuestObjectiveType"/> a chain objective kind maps to. The
        /// mapping is the spec's viability matrix made executable: NO new objective type is introduced.
        /// </summary>
        public static QuestObjectiveType ObjectiveTypeFor(NpcChainObjectiveKind kind)
        {
            switch (kind)
            {
                case NpcChainObjectiveKind.Collect: return QuestObjectiveType.CollectItem;
                case NpcChainObjectiveKind.Deliver: return QuestObjectiveType.DeliverItem;
                case NpcChainObjectiveKind.Talk:    return QuestObjectiveType.TalkToNpc;
                case NpcChainObjectiveKind.Reach:   return QuestObjectiveType.ReachLocation;
                case NpcChainObjectiveKind.Defeat:  return QuestObjectiveType.DefeatEnemy;
                case NpcChainObjectiveKind.Harvest: return QuestObjectiveType.HarvestCrop;
                case NpcChainObjectiveKind.Plant:   return QuestObjectiveType.PlantCrop;
                case NpcChainObjectiveKind.Craft:   return QuestObjectiveType.CraftItem;
                default: return QuestObjectiveType.TalkToNpc;
            }
        }

        /// <summary>
        /// Builds the fable_34 <see cref="QuestInstance"/> for a chain step. Source = Npc. Rewards =
        /// scaled gold + XP (single point fable_34, QuestLevel = the catalog reference level) + the step
        /// "done" flag (gates the next step) + an optional item + an optional service-unlock flag on the
        /// final step. The +8 amizade is NOT a reward here — it is granted by the FriendshipService
        /// turn-in hook (reuse), so this stays the single reward path.
        /// </summary>
        public static QuestInstance BuildInstance(NpcChainStepData step)
        {
            if (step == null) return null;

            int level = step.ReferenceLevel < 1 ? 1 : step.ReferenceLevel;
            int qty = step.Quantity < 1 ? 1 : step.Quantity;

            var extras = new List<QuestRewardDefinition>
            {
                // The step "done" flag — gates the next step and records completion idempotently.
                new QuestRewardDefinition
                {
                    RewardId = "reward_" + step.QuestId + "_done",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = DoneFlag(step.QuestId),
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };

            if (!string.IsNullOrEmpty(step.RewardItemId))
            {
                extras.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + step.QuestId + "_item",
                    RewardType = QuestRewardType.Item,
                    TargetId = step.RewardItemId,
                    Quantity = 1,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                });
            }

            if (!string.IsNullOrEmpty(step.ServiceUnlockFlagId))
            {
                extras.Add(new QuestRewardDefinition
                {
                    RewardId = "reward_" + step.QuestId + "_service",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = step.ServiceUnlockFlagId,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                });
            }

            return new QuestInstance
            {
                QuestId = step.QuestId,
                QuestTemplateId = step.QuestId,
                Source = QuestSource.Npc,
                TargetId = step.TargetId,
                Quantity = qty,
                QuestLevel = level,
                RewardGold = QuestRewardScaling.Scale(step.BaseGold, level),
                RewardXp = QuestRewardScaling.Scale(step.BaseXp, level),
                GeneratedForDay = 0,
                AdditionalRewards = extras
            };
        }

        // ── Builders ─────────────────────────────────────────────────────────────────────────────────
        private static NpcChainStepData Step(string npcId, string questId, int step, int level,
            NpcChainObjectiveKind kind, string target, int qty, int baseGold, int baseXp)
        {
            return new NpcChainStepData
            {
                NpcId = npcId, QuestId = questId, Step = step, ReferenceLevel = level,
                ObjectiveKind = kind, TargetId = target, Quantity = qty, BaseGold = baseGold, BaseXp = baseXp
            };
        }

        private static NpcChainStepData Final(string npcId, string questId, int step, int level,
            NpcChainObjectiveKind kind, string target, int qty, int baseGold, int baseXp,
            string serviceFlag, string requiredAct = null)
        {
            return new NpcChainStepData
            {
                NpcId = npcId, QuestId = questId, Step = step, ReferenceLevel = level,
                ObjectiveKind = kind, TargetId = target, Quantity = qty, BaseGold = baseGold, BaseXp = baseXp,
                ServiceUnlockFlagId = serviceFlag, RequiredActFlagId = requiredAct
            };
        }
    }
}
