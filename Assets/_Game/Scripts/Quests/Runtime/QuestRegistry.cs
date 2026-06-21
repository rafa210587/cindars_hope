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
    public partial class QuestRegistry
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

            RegisterMainQuestAct1();
            RegisterMainQuestActs2To4();

            // fable_63 — quest-ponte mq_act1_00 (auto-ofertada no 1o DayStarted). Registrada DEPOIS
            // do Ato 1 para poder injetar-se como PrerequisiteQuestId da mq_act1_01.
            RegisterMainQuestHook();
        }

        // ─── fable_10 — Main Quest Ato 1: "A Fonte do Esquecimento" (Fragmento da Agua) ──────
        // 5 quests encadeadas por PrerequisiteQuestIds, distribuidas entre Corvus (templo),
        // Thalindra (arquivo) e Maelor (pistas noturnas), culminando no Fragmento da Agua e no
        // 1o desbloqueio da Fonte (Agua Viva). Categoria Main => nunca expira (anti-softlock,
        // QuestDefinition.CanExpire() retorna false para Main). Reusa os 7 tipos de objetivo
        // existentes (WI-26) e a Fonte WAVE 10 via MainProgressionQuestBridge (idempotente).
        //
        // NOTA gate nivel 10 (TEMP): CreateCaveBossAssets cria gates apenas nos niveis 15/30/45/
        // 60/75/90 — NAO existe boss estavel no nivel 10. Conforme o risco previsto na spec, o
        // objetivo de "vencer o guardiao" usa DefeatEnemy "any" (3 inimigos) no contexto do nivel
        // 10, ate um gate canonico de nivel 10 ser definido. ReachCaveDepth 10 garante a descida.
        private void RegisterMainQuestAct1()
        {
            // ── mq_act1_01: A Fonte Adormecida ────────────────────────────────────────
            // Corvus apresenta a Fonte adormecida e manda procurar Thalindra no arquivo.
            // TalkToNpc Thalindra. Reward: flag de marco (flag_main_arrival, lida pelo
            // TownNpcDialogueLibrary fable_28 para a saudacao de chegada).
            var q1Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_mq1_talk_thalindra",
                    ObjectiveType = QuestObjectiveType.TalkToNpc,
                    TargetId = QuestRuntimeIds.ThalindraId,
                    RequiredAmount = 1
                }
            };
            var q1 = new QuestDefinition
            {
                QuestId = QuestMainAct1Ids.Quest01FonteAdormecida,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                DisplayName = "A Fonte Adormecida",
                Description = "Corvus fala de uma agua que dorme sob a fazenda e da Litania do Primeiro Retorno, que ja nao lembra inteira. Procure Thalindra no arquivo: se ha resposta, esta entre os registros antigos.",
                Trackable = true
            };
            var q1Rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq1_flag_arrival",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = QuestMainAct1Ids.FlagMainArrival,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            Register(q1, q1Objectives, q1Rewards, QuestMainAct1Ids.CorvusId);

            // ── mq_act1_02: Registros Perdidos ────────────────────────────────────────
            // Thalindra precisa de material para restaurar os registros e aponta Maelor como
            // quem viu algo na noite. CollectItem pedra x5 + TalkToNpc Maelor. Prereq: q1.
            var q2Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_mq2_collect_stone_x5",
                    ObjectiveType = QuestObjectiveType.CollectItem,
                    TargetId = "item_material_stone",
                    RequiredAmount = 5
                },
                new QuestObjective
                {
                    ObjectiveId = "obj_mq2_talk_maelor",
                    ObjectiveType = QuestObjectiveType.TalkToNpc,
                    TargetId = QuestRuntimeIds.MaelorId,
                    RequiredAmount = 1
                }
            };
            var q2 = new QuestDefinition
            {
                QuestId = QuestMainAct1Ids.Quest02RegistrosPerdidos,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                DisplayName = "Registros Perdidos",
                Description = "Thalindra precisa de pedra firme para escorar as estantes que guardam os diarios da fundacao. Traga cinco pedras e fale com Maelor, que anda pela cidade quando todos dormem: dizem que ele ouve a caverna melhor do que admite.",
                Trackable = true,
                PrerequisiteQuestIds = new List<string> { QuestMainAct1Ids.Quest01FonteAdormecida }
            };
            var q2Rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq2_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 40,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq2_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = "flag_mq_act1_records_restored",
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            Register(q2, q2Objectives, q2Rewards, QuestRuntimeIds.ThalindraId);

            // ── mq_act1_03: O Eco da Agua ─────────────────────────────────────────────
            // Maelor da a pista: descer ate sentir o eco da agua. ReachCaveDepth 5. Prereq: q2.
            var q3Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_mq3_reach_depth_5",
                    ObjectiveType = QuestObjectiveType.ReachCaveDepth,
                    TargetId = "5",
                    RequiredAmount = 1
                }
            };
            var q3 = new QuestDefinition
            {
                QuestId = QuestMainAct1Ids.Quest03EcoDaAgua,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                DisplayName = "O Eco da Agua",
                Description = "Maelor sussurra que, fundo o bastante, a pedra devolve o som de uma agua que ninguem ve. Desca a caverna ate o quinto nivel e escute o eco que Thalindra encontrou nos registros.",
                Trackable = true,
                PrerequisiteQuestIds = new List<string> { QuestMainAct1Ids.Quest02RegistrosPerdidos }
            };
            var q3Rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq3_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 60,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq3_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = "flag_mq_act1_echo_heard",
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            Register(q3, q3Objectives, q3Rewards, QuestRuntimeIds.MaelorId);

            // ── mq_act1_04: O Guardiao da Agua ────────────────────────────────────────
            // Descer ate o nivel 10 e vencer o guardiao. ReachCaveDepth 10 + DefeatEnemy.
            // TEMP (sem gate canonico no nivel 10): DefeatEnemy "any" x3. Prereq: q3.
            var q4Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_mq4_reach_depth_10",
                    ObjectiveType = QuestObjectiveType.ReachCaveDepth,
                    TargetId = "10",
                    RequiredAmount = 1
                },
                new QuestObjective
                {
                    ObjectiveId = "obj_mq4_defeat_guardian",
                    ObjectiveType = QuestObjectiveType.DefeatEnemy,
                    TargetId = QuestMainAct1Ids.GuardianEnemyTarget,
                    RequiredAmount = 3
                }
            };
            var q4 = new QuestDefinition
            {
                QuestId = QuestMainAct1Ids.Quest04GuardiaoDaAgua,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                DisplayName = "O Guardiao da Agua",
                Description = "O eco tem um guardiao. Alcance o decimo nivel da caverna e abra caminho pela forca: so quem prova merecer pode trazer o Fragmento da Agua de volta a Fonte.",
                Trackable = true,
                PrerequisiteQuestIds = new List<string> { QuestMainAct1Ids.Quest03EcoDaAgua }
            };
            var q4Rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq4_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 80,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq4_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = "flag_mq_act1_guardian_defeated",
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            Register(q4, q4Objectives, q4Rewards, QuestRuntimeIds.MaelorId);

            // ── mq_act1_05: O Fragmento da Agua ───────────────────────────────────────
            // Entrega a Corvus. TalkToNpc Corvus. Reward: 150 gold + flag mq_act1_complete +
            // flag_main_post_act1. O FRAGMENTO em si e concedido pelo MainProgressionQuestBridge
            // (consome QuestCompletedEvent => FonteRuntimeService.IntegrateFragment(Water)),
            // idempotente por GrantedRewardIds/estado da Fonte. Prereq: q4.
            var q5Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "obj_mq5_talk_corvus",
                    ObjectiveType = QuestObjectiveType.TalkToNpc,
                    TargetId = QuestMainAct1Ids.CorvusId,
                    RequiredAmount = 1
                }
            };
            var q5 = new QuestDefinition
            {
                QuestId = QuestMainAct1Ids.Quest05FragmentoDaAgua,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                DisplayName = "O Fragmento da Agua",
                Description = "Leve o Fragmento da Agua a Corvus, no templo. A Fonte voltara a fluir: nao para devolver Anya, mas para que a agua sirva quem ainda desce. Corvus recita, enfim inteira, a Litania do Primeiro Retorno.",
                Trackable = true,
                PrerequisiteQuestIds = new List<string> { QuestMainAct1Ids.Quest04GuardiaoDaAgua }
            };
            var q5Rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq5_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 150,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq5_flag_complete",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = QuestMainAct1Ids.FlagAct1Complete,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq5_flag_post_act1",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = QuestMainAct1Ids.FlagMainPostAct1,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            Register(q5, q5Objectives, q5Rewards, QuestMainAct1Ids.CorvusId);
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

    /// <summary>
    /// fable_10 — stable IDs for the Act 1 main questline ("A Fonte do Esquecimento").
    /// Quest ids, giver ids and the milestone flag ids the dialogue library reads. Never renamed.
    /// </summary>
    public static class QuestMainAct1Ids
    {
        public const string Quest01FonteAdormecida = "mq_act1_01_fonte_adormecida";
        public const string Quest02RegistrosPerdidos = "mq_act1_02_registros_perdidos";
        public const string Quest03EcoDaAgua = "mq_act1_03_eco_da_agua";
        public const string Quest04GuardiaoDaAgua = "mq_act1_04_guardiao_da_agua";
        public const string Quest05FragmentoDaAgua = "mq_act1_05_fragmento_da_agua";

        public const string CorvusId = "npc_corvus";

        // Milestone flags also authored by TownNpcDialogueLibrary (fable_28). Stable ids.
        public const string FlagMainArrival = "flag_main_arrival";
        public const string FlagMainPostAct1 = "flag_main_post_act1";
        public const string FlagAct1Complete = "flag_mq_act1_complete";

        // TEMP target for the level-10 guardian: no canonical boss gate exists at level 10
        // (CreateCaveBossAssets gates are 15/30/45/60/75/90). "any" matches any defeated enemy.
        public const string GuardianEnemyTarget = "any";
    }
}
