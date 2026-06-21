using System.Collections.Generic;
using CindarsHope.Quests;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_36 — main-quest Acts 2-4 registration. Materializes QUEST_CATALOG §main as flag-chained
    /// Main quests, reusing the fable_10 Act 1 pattern (QuestDefinition + QuestObjective + reward
    /// flags) and the supported runtime objective handlers (TalkToNpc / CollectItem / ReachCaveDepth /
    /// DefeatEnemy). The fragment/skill-point/ActCompletedEvent hooks live in MainProgressionQuestBridge.
    ///
    /// Chaining: Act 2's entry quest is gated on Act 1's completion (flag_mq_act1_complete via the
    /// PrerequisiteQuestId on the prior act-final quest); each act's offer of N+1 is gated on act_N_done
    /// (set as a reward on the act-final quest). Every quest is Category.Main => never expires (Rule 8).
    /// </summary>
    public partial class QuestRegistry
    {
        // Each act-final quest grants: act_N_done (side-chain gate), flag_mq_actN_complete (next-act
        // chain link) and flag_main_post_actN (F28 dialogue). The fragment + skill point are applied by
        // MainProgressionQuestBridge on QuestCompletedEvent (idempotent), NOT as generic rewards.
        private static List<QuestRewardDefinition> ActFinaleRewards(
            int actNumber, int gold, string actDoneFlag, string actCompleteFlag, string postActFlag)
        {
            var rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = $"reward_mq_act{actNumber}_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = gold,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = $"reward_mq_act{actNumber}_done_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = actDoneFlag,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                },
                new QuestRewardDefinition
                {
                    RewardId = $"reward_mq_act{actNumber}_complete_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = actCompleteFlag,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                },
                new QuestRewardDefinition
                {
                    RewardId = $"reward_mq_act{actNumber}_post_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = postActFlag,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
            return rewards;
        }

        private static List<QuestRewardDefinition> SimpleGoldFlagRewards(string idPrefix, int gold, string flagId)
        {
            return new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = $"{idPrefix}_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = gold,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = $"{idPrefix}_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = flagId,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };
        }

        private QuestDefinition MainQuest(string id, string display, string desc, string prereq)
        {
            var def = new QuestDefinition
            {
                QuestId = id,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                DisplayName = display,
                Description = desc,
                Trackable = true
            };
            if (!string.IsNullOrEmpty(prereq))
                def.PrerequisiteQuestIds = new List<string> { prereq };
            return def;
        }

        private static List<QuestObjective> Talk(string objId, string npcId)
            => new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = objId, ObjectiveType = QuestObjectiveType.TalkToNpc, TargetId = npcId, RequiredAmount = 1 }
            };

        private static QuestObjective Collect(string objId, string itemId, int n)
            => new QuestObjective { ObjectiveId = objId, ObjectiveType = QuestObjectiveType.CollectItem, TargetId = itemId, RequiredAmount = n };

        private static QuestObjective ReachDepth(string objId, int depth)
            => new QuestObjective { ObjectiveId = objId, ObjectiveType = QuestObjectiveType.ReachCaveDepth, TargetId = depth.ToString(), RequiredAmount = 1 };

        private static QuestObjective Defeat(string objId, int n)
            => new QuestObjective { ObjectiveId = objId, ObjectiveType = QuestObjectiveType.DefeatEnemy, TargetId = "any", RequiredAmount = n };

        private static QuestObjective TalkObj(string objId, string npcId)
            => new QuestObjective { ObjectiveId = objId, ObjectiveType = QuestObjectiveType.TalkToNpc, TargetId = npcId, RequiredAmount = 1 };

        private void RegisterMainQuestActs2To4()
        {
            RegisterAct2();
            RegisterAct3();
            RegisterAct4();
        }

        // ─── Act 2 — "O Arco da Memoria" (QuestLevel 35; gate 30; Fragmento da MEMORIA) ──────────
        private void RegisterAct2()
        {
            // act2_01: Registros de Prata. Thalindra abre o arco da memoria. Gated on Act 1 finale.
            // CollectItem (amostra do veio) + TalkToNpc Thalindra.
            Register(
                MainQuest(QuestMainActsIds.Act2Quest01RecordsOfSilver, "Registros de Prata",
                    "A Fonte voltou a fluir, mas a agua trouxe um eco de prata: nomes que ninguem lembra de ter escrito. Traga uma amostra do veio raso e leve a Thalindra; o arquivo guarda mais do que poeira.",
                    QuestMainAct1Ids.Quest05FragmentoDaAgua),
                new List<QuestObjective> { Collect("obj_mq2_01_collect_silver_ore", "item_material_stone", 4), TalkObj("obj_mq2_01_talk_thalindra", QuestMainActsIds.ThalindraId) },
                SimpleGoldFlagRewards("reward_mq2_01", 90, "flag_mq_act2_records_opened"),
                QuestMainActsIds.ThalindraId);

            // act2_02: O Visitante Velado. Vaelrion chega apos o 1o boss. ReachCaveDepth 15 + TalkToNpc Vaelrion.
            Register(
                MainQuest(QuestMainActsIds.Act2Quest02TheVeiledVisitor, "O Visitante Velado",
                    "Apos o guardiao tombar, um estranho de manto cinza aparece a beira da caverna: Vaelrion, que diz lembrar do que a cidade esqueceu. Desca ao decimo quinto nivel onde ele espera e ouca o que tem a dizer.",
                    QuestMainActsIds.Act2Quest01RecordsOfSilver),
                new List<QuestObjective> { ReachDepth("obj_mq2_02_reach_depth_15", 15), TalkObj("obj_mq2_02_talk_vaelrion", QuestMainActsIds.VaelrionId) },
                SimpleGoldFlagRewards("reward_mq2_02", 100, "flag_vaelrion_introduced"),
                QuestMainActsIds.VaelrionId);

            // act2_03: A Cancao Embaixo. Liora ouviu a melodia da memoria. TalkToNpc Liora + Collect fragmento.
            Register(
                MainQuest(QuestMainActsIds.Act2Quest03SongBelow, "A Cancao Embaixo",
                    "Liora nao dorme: uma cancao sem autor sobe das fendas e ela jura que e a voz de Anya. Recolha os fragmentos de cristal que ressoam com a melodia e leve-os a ela antes que a cancao se cale.",
                    QuestMainActsIds.Act2Quest02TheVeiledVisitor),
                new List<QuestObjective> { Collect("obj_mq2_03_collect_resonant_crystal", "item_material_stone", 6), TalkObj("obj_mq2_03_talk_liora", QuestMainActsIds.LioraId) },
                SimpleGoldFlagRewards("reward_mq2_03", 120, "flag_mq_act2_song_heard"),
                QuestMainActsIds.LioraId);

            // act2_04: O Portao de Gelo. Rimelock Colossus / gate 30 (real boss gate). ReachDepth 30 + Defeat.
            Register(
                MainQuest(QuestMainActsIds.Act2Quest04GateOfFrost, "O Portao de Gelo",
                    "Vaelrion aponta o caminho: o Arco da Memoria esta selado atras do Portao de Gelo, no trigesimo nivel. La dorme o Colosso do Gelo Eterno. Quebre o selo e o gelo cedera ao calor da lembranca.",
                    QuestMainActsIds.Act2Quest03SongBelow),
                new List<QuestObjective> { ReachDepth("obj_mq2_04_reach_depth_30", 30), Defeat("obj_mq2_04_defeat_frost_gate", 1) },
                SimpleGoldFlagRewards("reward_mq2_04", 160, "flag_mq_act2_frost_gate_open"),
                QuestMainActsIds.VaelrionId);

            // act2_05: O Fragmento da Memoria. Entrega a Corvus -> respec. ACT FINALE.
            Register(
                MainQuest(QuestMainActsIds.Act2Quest05FragmentOfMemory, "O Fragmento da Memoria",
                    "Leve o Fragmento da Memoria a Corvus. A Fonte aprendera a desfazer o que foi mal aprendido: o que voce escolheu pode ser reescolhido. Mas a memoria devolvida tem um peso, e a pedra negra aparece pela primeira vez nas margens do registro.",
                    QuestMainActsIds.Act2Quest04GateOfFrost),
                Talk("obj_mq2_05_talk_corvus", QuestMainActsIds.CorvusId),
                ActFinaleRewards(2, 220, QuestMainActsIds.FlagAct2Done, QuestMainActsIds.FlagAct2Complete, QuestMainActsIds.FlagMainPostAct2),
                QuestMainActsIds.CorvusId);
        }

        // ─── Act 3 — "A Pedra que Sussurra" (QuestLevel 65; gate 70->clamped; Fragmento da VIDA) ─
        private void RegisterAct3()
        {
            // act3_01: O Livro-Razao da Pedra Negra. Investigacao na cidade. Gated on act_2_done.
            Register(
                MainQuest(QuestMainActsIds.Act3Quest01BlackstoneLedger, "O Livro-Razao da Pedra Negra",
                    "A memoria devolvida aponta um padrao: compras estranhas, nomes riscados, uma pedra que ninguem admite vender. Reuna as paginas espalhadas pela cidade e leve a Thalindra; o livro-razao da pedra negra precisa ser lido inteiro.",
                    QuestMainActsIds.Act2Quest05FragmentOfMemory),
                new List<QuestObjective> { Collect("obj_mq3_01_collect_ledger_pages", "item_material_wood", 5), TalkObj("obj_mq3_01_talk_thalindra", QuestMainActsIds.ThalindraId) },
                SimpleGoldFlagRewards("reward_mq3_01", 180, "flag_mq_act3_ledger_read"),
                QuestMainActsIds.ThalindraId);

            // act3_02: A Misericordia do Servo. Purificar 3 Thralls -> voltam como aldeoes. Defeat x3.
            Register(
                MainQuest(QuestMainActsIds.Act3Quest02ThrallMercy, "A Misericordia do Servo",
                    "Os Thralls da pedra negra ja foram pessoas. Vaelrion ensina o gesto que os liberta sem mata-los: enfrente tres servos corrompidos e devolva-os a si mesmos. Eles voltarao para a cidade como aldeoes.",
                    QuestMainActsIds.Act3Quest01BlackstoneLedger),
                new List<QuestObjective> { ReachDepth("obj_mq3_02_reach_depth_45", 45), Defeat("obj_mq3_02_purify_thralls", 3) },
                SimpleGoldFlagRewards("reward_mq3_02", 210, "flag_mq_act3_thralls_freed"),
                QuestMainActsIds.VaelrionId);

            // act3_03: O Sacerdote Silencioso. Escolha (prender/exilar) -> flag que retorna no final.
            // Modelado como TalkToNpc Corvus (a escolha em si e dialogo; a flag de marco registra a decisao).
            Register(
                MainQuest(QuestMainActsIds.Act3Quest03TheQuietPriest, "O Sacerdote Silencioso",
                    "Um sacerdote da cidade sussurra para a pedra quando pensa que ninguem ve. Leve a prova a Corvus e decida com ele: prender ou exilar. A escolha sera lembrada quando a Fonte fizer sua ultima pergunta.",
                    QuestMainActsIds.Act3Quest02ThrallMercy),
                Talk("obj_mq3_03_talk_corvus", QuestMainActsIds.CorvusId),
                SimpleGoldFlagRewards("reward_mq3_03", 200, "flag_mq_act3_priest_judged"),
                QuestMainActsIds.CorvusId);

            // act3_04: O Guardiao do Silencio. Nymirian chega. ReachDepth 60 + TalkToNpc Nymirian.
            // O turn-in seta flag_nymirian_available (Nymirian conversavel a partir daqui).
            var nymirianRewards = SimpleGoldFlagRewards("reward_mq3_04", 230, QuestMainActsIds.FlagNymirianAvailable);
            Register(
                MainQuest(QuestMainActsIds.Act3Quest04WardenOfSilence, "O Guardiao do Silencio",
                    "Fundo na caverna vive o ultimo do povo de Cindar: Nymirian, o Guardiao do Silencio, que guarda a lore de quem ergueu a Fonte. Desca ao sexagesimo nivel e ganhe sua confianca; so ele sabe o que a Vida custou.",
                    QuestMainActsIds.Act3Quest03TheQuietPriest),
                new List<QuestObjective> { ReachDepth("obj_mq3_04_reach_depth_60", 60), TalkObj("obj_mq3_04_talk_nymirian", QuestMainActsIds.NymirianId) },
                nymirianRewards,
                QuestMainActsIds.NymirianId);

            // act3_05: O Fragmento da Vida. gate 70 (clamped 75 real gate) -> purificacao. ACT FINALE.
            Register(
                MainQuest(QuestMainActsIds.Act3Quest05FragmentOfLife, "O Fragmento da Vida",
                    "O Fragmento da Vida esta alem do portao profundo. Alcance-o, vença o que o guarda e leve-o a Corvus. A Fonte aprendera a curar de verdade, e a pedra negra mostrara, enfim, que nao quer matar: quer ser lembrada.",
                    QuestMainActsIds.Act3Quest04WardenOfSilence),
                new List<QuestObjective> { ReachDepth("obj_mq3_05_reach_depth_75", 75), Defeat("obj_mq3_05_defeat_life_gate", 1), TalkObj("obj_mq3_05_talk_corvus", QuestMainActsIds.CorvusId) },
                ActFinaleRewards(3, 320, QuestMainActsIds.FlagAct3Done, QuestMainActsIds.FlagAct3Complete, QuestMainActsIds.FlagMainPostAct3),
                QuestMainActsIds.CorvusId);
        }

        // ─── Act 4 — "A Esperanca Enterrada" (QuestLevel 90; gate 100->clamped; prep da ESPERANCA) ─
        private void RegisterAct4()
        {
            // act4_01: A Litania Completa. Gated on act_3_done. Reune os versos com Corvus.
            Register(
                MainQuest(QuestMainActsIds.Act4Quest01LitanyComplete, "A Litania Completa",
                    "Tres fragmentos cantam; falta o quarto. Corvus precisa da Litania do Primeiro Retorno inteira para nomear a Esperanca. Recolha os versos perdidos pela caverna e recite-os com ele.",
                    QuestMainActsIds.Act3Quest05FragmentOfLife),
                new List<QuestObjective> { Collect("obj_mq4_01_collect_litany_verses", "item_material_wood", 6), TalkObj("obj_mq4_01_talk_corvus", QuestMainActsIds.CorvusId) },
                SimpleGoldFlagRewards("reward_mq4_01", 280, "flag_mq_act4_litany_complete"),
                QuestMainActsIds.CorvusId);

            // act4_02: O Carcereiro. Gate 100 (clamped ao conteudo real: deepest gate = 90). ReachDepth 90 + Defeat.
            Register(
                MainQuest(QuestMainActsIds.Act4Quest02TheJailer, "O Carcereiro",
                    "A Esperanca esta presa atras das portas profundas, guardada pelo Carcereiro Antigo. Alcance o nivel mais fundo que a pedra ainda permite e quebre a fechadura: o que esta selado nem sempre quer ficar.",
                    QuestMainActsIds.Act4Quest01LitanyComplete),
                new List<QuestObjective> { ReachDepth("obj_mq4_02_reach_depth_90", 90), Defeat("obj_mq4_02_defeat_jailer", 1) },
                SimpleGoldFlagRewards("reward_mq4_02", 360, "flag_mq_act4_jailer_defeated"),
                QuestMainActsIds.NymirianId);

            // act4_03: Vel-Karaum. Nymirian revela o nome enterrado. TalkToNpc Nymirian + Collect.
            Register(
                MainQuest(QuestMainActsIds.Act4Quest03VelKaraum, "Vel-Karaum",
                    "Nymirian pronuncia o nome que a cidade enterrou: Vel-Karaum, a cidade sob a cidade. Reuna o que sobrou dela nas profundezas e traga a Nymirian; a Esperanca nao e um milagre, e uma memoria que se recusa a morrer.",
                    QuestMainActsIds.Act4Quest02TheJailer),
                new List<QuestObjective> { Collect("obj_mq4_03_collect_vel_karaum_relics", "item_material_stone", 8), TalkObj("obj_mq4_03_talk_nymirian", QuestMainActsIds.NymirianId) },
                SimpleGoldFlagRewards("reward_mq4_03", 400, "flag_mq_act4_vel_karaum_known"),
                QuestMainActsIds.NymirianId);

            // act4_04: Lembranca Quebrada. ACT FINALE (prep da Esperanca). Entrega a Corvus.
            // A Esperanca e HINTADA (lore), NAO integrada: a escolha final fica na fable_43.
            Register(
                MainQuest(QuestMainActsIds.Act4Quest04BrokenRemembrance, "Lembranca Quebrada",
                    "Leve o que resta da Esperanca a Corvus. A Fonte agora conhece os quatro nomes, mas o quarto nao se integra ainda: ele aguarda uma escolha que so voce podera fazer. A pedra negra, enfim revelada por inteiro, espera a ultima pergunta. (A decisao final vira depois.)",
                    QuestMainActsIds.Act4Quest03VelKaraum),
                Talk("obj_mq4_04_talk_corvus", QuestMainActsIds.CorvusId),
                ActFinaleRewards(4, 500, QuestMainActsIds.FlagAct4Done, QuestMainActsIds.FlagAct4Complete, QuestMainActsIds.FlagMainPostAct4),
                QuestMainActsIds.CorvusId);
        }
    }
}
